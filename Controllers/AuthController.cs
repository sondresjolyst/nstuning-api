using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using nstuning_api.Constants;
using nstuning_api.Dtos.Auth;
using nstuning_api.Models;
using nstuning_api.Models.Auth;
using Swashbuckle.AspNetCore.Annotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace nstuning_api.Controllers
{
    /// <summary>
    /// Handles authentication-related actions.
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    [EnableCors("AllowAllOrigins")]
    [Authorize]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IConfiguration configuration,
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Registers a new user.")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto registerUserDto)
        {
            _logger.LogInformation("Register called");

            const string genericResponse = "If the email is available, an account has been created.";

            var existingUser = await _userManager.FindByEmailAsync(registerUserDto.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("Register: email already registered");
                return Ok(new { message = genericResponse });
            }

            var user = _mapper.Map<User>(registerUserDto);
            var result = await _userManager.CreateAsync(user, registerUserDto.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, RoleNames.Default);
                _logger.LogInformation("User registered successfully {UserId}", user.Id);
                return Ok(new { message = genericResponse });
            }

            _logger.LogError("Register failed: {@Errors}", result.Errors);
            var fieldErrors = result.Errors
                .GroupBy(e => RegisterErrorField(e.Code))
                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());
            return BadRequest(new { errors = fieldErrors });
        }

        private static string RegisterErrorField(string code) =>
            code.Contains("UserName", StringComparison.OrdinalIgnoreCase) ? "UserName"
            : code.Contains("Email", StringComparison.OrdinalIgnoreCase) ? "Email"
            : code.Contains("Password", StringComparison.OrdinalIgnoreCase) ? "Password"
            : "UserName";

        /// <summary>
        /// Logs in a user.
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Logs in a user.")]
        public async Task<IActionResult> Login([FromBody] LoginModel login)
        {
            _logger.LogInformation("Login called");

            var user = await _userManager.FindByEmailAsync(login.Email);
            if (user == null || user.IsDeleted)
            {
                _logger.LogWarning("Login failed: Invalid credentials");
                return Unauthorized(new { message = "Invalid email or password." });
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName ?? string.Empty, login.Password, false, lockoutOnFailure: true);
            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("Login blocked: account locked {UserId}", user.Id);
                    return Unauthorized(new { message = "Too many failed attempts — this login is temporarily locked. Try again later." });
                }
                _logger.LogWarning("Login failed: Invalid credentials");
                return Unauthorized(new { message = "Invalid email or password." });
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var tokenString = BuildJwt(user, userRoles);
            var rawToken = await IssueRefreshTokenAsync(user.Id);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User logged in successfully {UserId}", user.Id);
            return Ok(new { token = tokenString, refreshToken = rawToken });
        }

        /// <summary>
        /// Refresh JWT using a valid refresh token.
        /// </summary>
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Refresh JWT using a valid refresh token.")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            _logger.LogInformation("RefreshToken called");

            var principal = GetPrincipalFromExpiredToken(request.Token);
            if (principal == null)
                return BadRequest(new { message = "Invalid token" });

            var userId = principal.Claims.FirstOrDefault(x =>
                x.Type == JwtRegisteredClaimNames.Sub || x.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return BadRequest(new { message = "Invalid token" });

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsDeleted)
                return Unauthorized(new { message = "User not found" });

            var hashedInput = HashText(request.RefreshToken);

            await using var tx = await _context.Database.BeginTransactionAsync();

            var anyMatch = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == hashedInput && t.UserId == user.Id);

            if (anyMatch != null && anyMatch.Revoked != null)
            {
                var now = DateTime.UtcNow;
                var familyLive = await _context.RefreshTokens
                    .Where(t => t.UserId == user.Id && t.Revoked == null)
                    .ToListAsync();
                foreach (var t in familyLive) t.Revoked = now;
                await _context.SaveChangesAsync();
                await tx.CommitAsync();
                _logger.LogWarning("RefreshToken reuse detected; family revoked {UserId}", user.Id);
                return Unauthorized(new { message = "Invalid or expired refresh token" });
            }

            var storedToken = anyMatch != null && anyMatch.Revoked == null && anyMatch.Expires > DateTime.UtcNow
                ? anyMatch
                : null;

            if (storedToken == null)
            {
                await tx.RollbackAsync();
                return Unauthorized(new { message = "Invalid or expired refresh token" });
            }

            storedToken.Revoked = DateTime.UtcNow;

            var userRoles = await _userManager.GetRolesAsync(user);
            var newTokenString = BuildJwt(user, userRoles);
            var newRawToken = await IssueRefreshTokenAsync(user.Id);

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            _logger.LogInformation("Token refreshed successfully {UserId}", user.Id);
            return Ok(new { token = newTokenString, refreshToken = newRawToken });
        }

        private string BuildJwt(User user, IList<string> roles)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? string.Empty);
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
                new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty)
            };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                NotBefore = DateTime.UtcNow.AddSeconds(-5),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Issuer"]
            };
            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        }

        private async Task<string> IssueRefreshTokenAsync(string userId)
        {
            var rawToken = GenerateRefreshToken();
            var hashedToken = HashText(rawToken);

            var userTokens = await _context.RefreshTokens
                .Where(t => t.UserId == userId && t.Revoked == null && t.Expires > DateTime.UtcNow)
                .OrderBy(t => t.Created)
                .ToListAsync();
            if (userTokens.Count >= 5)
                _context.RefreshTokens.Remove(userTokens.First());

            _context.RefreshTokens.Add(new RefreshToken
            {
                Token = hashedToken,
                UserId = userId,
                Expires = DateTime.UtcNow.AddMonths(6),
                Created = DateTime.UtcNow
            });
            return rawToken;
        }

        private static string HashText(string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            var hash = SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? string.Empty)),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
                if (securityToken is JwtSecurityToken jwtSecurityToken &&
                    jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return principal;
                }
            }
            catch
            {
                return null;
            }
            return null;
        }
    }
}

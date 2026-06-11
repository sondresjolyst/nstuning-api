using System.ComponentModel.DataAnnotations;

namespace nstuning_api.Dtos.Auth
{
    public class RegisterUserDto
    {
        [Required]
        [MaxLength(50)]
        public required string UserName { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [MaxLength(50)]
        public required string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        public required string LastName { get; set; }

        [Required]
        public required string Password { get; set; }
    }

    public class LoginModel
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string Password { get; set; }
    }

    public class RefreshTokenRequestDto
    {
        [Required]
        public required string Token { get; set; }

        [Required]
        public required string RefreshToken { get; set; }
    }
}

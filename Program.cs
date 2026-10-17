using FluentValidation;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using nstuning_api.Constants;
using nstuning_api.Features.Admin;
using nstuning_api.Infrastructure;
using nstuning_api.Models;
using nstuning_api.Models.Admin;
using nstuning_api.Profiles;
using nstuning_api.Services;
using Serilog;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using SjolystInnovation.AppKit.Auth;
using SjolystInnovation.AppKit.SiteSettings;
using SjolystInnovation.AppKit.Hosting;
using SjolystInnovation.AppKit.Email;
using SjolystInnovation.AppKit.AdminStats;
using SjolystInnovation.AppKit.Storage.Images;

namespace nstuning_api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .Enrich.FromLogContext()
                .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger();

            builder.Host.UseSerilog();

            builder.Configuration.AddEnvironmentVariables();

            var mapsterConfig = TypeAdapterConfig.GlobalSettings;
            mapsterConfig.Scan(typeof(MappingProfile).Assembly);
            builder.Services.AddSingleton(mapsterConfig);
            builder.Services.AddScoped<IMapper, ServiceMapper>();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddAppKitIdentity<User, ApplicationDbContext>();

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                });

            builder.Services.AddAppKitBrevo(builder.Configuration.GetSection("BrevoSettings"));
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IEmailBrand, AuthEmailBrand>();
            builder.Services.AddAppKitAuthEmails<User>(o =>
            {
                o.HeaderColor = "#ffd400";
                o.AccentColor = "#ffd400";
                o.SetPasswordUrl = $"{builder.Configuration["Site:BaseUrl"]?.TrimEnd('/')}/reset-password";
            });
            builder.Services.AddAppKitAccounts<User, ApplicationDbContext>(o => o.AdminRole = RoleNames.Admin);
            builder.Services.AddAppKitAuth<ApplicationDbContext>(builder.Configuration.GetSection("Jwt"));
            builder.Services.AddReportStore(builder.Configuration);
            builder.Services.AddAppKitContentImages<ApplicationDbContext>(builder.Configuration.GetSection("Storage"));
            builder.Services.AddAppKitDailyStats<ApplicationDbContext, DailyStatSnapshot>(o => o.LiveSampler = DailyStats.SampleAsync);

            builder.Services.AddProblemDetails();
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddValidatorsFromAssemblyContaining<Program>();
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddAppKitJwtBearer<User>(u => !u.IsDeleted);

            builder.Services.AddAppKitHealthChecks<ApplicationDbContext>();

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", policy => policy.RequireRole(RoleNames.Admin));
            });

            var allowedOrigins = builder.Configuration
                .GetSection("Cors:AllowedOrigins")
                .Get<string[]>() ?? [];

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins", policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

            builder.Services.AddAppKitRateLimiting(builder.Configuration.GetSection("RateLimiting"));

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "NS Tuning API", Version = "v1" });
                if (File.Exists(xmlPath))
                    c.IncludeXmlComments(xmlPath);

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter a valid token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecuritySchemeReference("Bearer"),
                        new List<string>()
                    }
                });
            });

            var app = builder.Build();

            app.Logger.LogInformation("Starting API.");
            await app.Services.MigrateAppKitDatabaseAsync<ApplicationDbContext>(app.Configuration);
            await app.Services.SeedAppKitRolesAsync(RoleNames.AllRoles);
            await app.Services.SeedAppKitFirstAdminAsync<User>(app.Configuration, u => new User { FirstName = u.FirstName, LastName = u.LastName });

            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                if (await context.AppSettings.FindAsync(1) == null)
                {
                    context.AppSettings.Add(new AppSettings());
                    await context.SaveChangesAsync();
                }
            }

            app.UseAppKitForwardedHeaders();

            app.UseExceptionHandler();

            app.UseAppKitSecurityHeaders();

            app.UseAppKitHttps();

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "NS Tuning API V1"));

            app.UseRouting();
            app.UseCors("AllowAllOrigins");
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseAppKitRateLimiting();
            app.MapControllers();
            app.MapEndpoints();
            app.MapAppKitAuth<User, ApplicationDbContext>(configure: o => o.IsActive = u => !u.IsDeleted);
            app.MapAppKitAccount<User>();
            app.MapAppKitSiteSettings<ApplicationDbContext, AppSettings>(configure: o => o.Policy = Policies.Admin);
            app.MapAppKitUserAdmin<User, ApplicationDbContext>(configure: o =>
            {
                o.Roles = RoleNames.AllRoles;
                o.Policy = Policies.Admin;
                o.NewUser = u => new User { FirstName = u.FirstName, LastName = u.LastName };
            });
            app.MapAppKitContentImages<ApplicationDbContext>("/api/content-images", o => o.Policy = Policies.Admin);
            app.MapAppKitDailyStats<ApplicationDbContext, DailyStatSnapshot>("/api/admin/stats", o => o.Policy = Policies.Admin);
            app.MapAppKitHealthChecks();
            app.Run();
        }
    }
}

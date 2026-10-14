using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace nstuning_api.Features.Auth
{
    public class RegisterUserDto
    {
        [Required][MaxLength(50)] public required string UserName { get; set; }
        [Required][EmailAddress] public required string Email { get; set; }
        [Required][MaxLength(50)] public required string FirstName { get; set; }
        [Required][MaxLength(50)] public required string LastName { get; set; }
        [Required] public required string Password { get; set; }
    }

    public class RegisterValidator : AbstractValidator<RegisterUserDto>
    {
        public RegisterValidator()
        {
            RuleFor(x => x.UserName).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Password).NotEmpty();
        }
    }
}

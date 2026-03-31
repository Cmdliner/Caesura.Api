using FluentValidation;

namespace Caesura.Api.Validators.Auth;

public class RegisterRequestValidator: AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters.");
    
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters long")
            .MaximumLength(30).WithMessage("Username must not exceed 30 characters.")
            .Matches(@"^[a-zA-Z0-9_]+$")
            .WithMessage("Username may only contain letters, numbers, and underscores.")
            // Prevent reserved or confusing usernames
            .Must(u => !ReservedUsernames.Contains(u.ToLower()))
            .WithMessage("That username is not available.");
 
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .MaximumLength(72).WithMessage("Password must not exceed 72 characters.")
            // bcrypt/PBKDF2 silent truncation guard
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one number.");
 
        RuleFor(x => x.DisplayName)
            .MaximumLength(100).WithMessage("Display name must not exceed 100 characters.")
            .When(x => x.DisplayName is not null);
    }
 
    private static readonly HashSet<string> ReservedUsernames =
    [
        "admin", "administrator", "root", "system", "api",
        "support", "help", "me", "library", "explore", "search"
    ];
        
        
    
}
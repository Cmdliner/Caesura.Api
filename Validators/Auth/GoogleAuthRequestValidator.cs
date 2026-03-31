using FluentValidation;

namespace Caesura.Api.Validators.Auth;

public class GoogleAuthRequestValidator: AbstractValidator<GoogleAuthRequest>
{
    public GoogleAuthRequestValidator()
    {
        RuleFor(x => x.IdToken)
            .NotEmpty().WithMessage("Google Id Token is required")
            .MinimumLength(100).WithMessage("Invalid Google ID token.");
    }
}
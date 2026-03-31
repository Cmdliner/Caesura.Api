using FluentValidation;

namespace Caesura.Api.Validators.Books;

public class UpdateProgressRequestValidator : AbstractValidator<UpdateProgressRequest>
{
    public UpdateProgressRequestValidator()
    {
        RuleFor(x => x.ChapterId)
            .NotEmpty().WithMessage("Chapter ID is required.");
 
        RuleFor(x => x.ScrollPosition)
            .InclusiveBetween(0, 10_000)
            .WithMessage("Scroll position must be between 0 and 10000.");
    }
}
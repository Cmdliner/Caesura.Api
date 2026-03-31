using FluentValidation;

namespace Caesura.Api.Validators.Books;

public class RateBookRequestValidator : AbstractValidator<RateBookRequest>
{
    public RateBookRequestValidator()
    {
        RuleFor(x => x.Rating)
            .InclusiveBetween((short)1, (short)5)
            .WithMessage("Rating must be between 1 and 5.");
 
        RuleFor(x => x.Review)
            .MaximumLength(2000).WithMessage("Review must not exceed 2000 characters.")
            .Must(r => !string.IsNullOrWhiteSpace(r)).WithMessage("Review cannot be whitespace only.")
            .When(x => x.Review is not null);
    }
}

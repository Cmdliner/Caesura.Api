using FluentValidation;

namespace Caesura.Api.Validators.Books;

public class InlineCommentRequestValidator : AbstractValidator<InlineCommentRequest>
{
    public InlineCommentRequestValidator()
    {
        RuleFor(x => x.FromPos)
            .GreaterThanOrEqualTo(0).WithMessage("fromPos must be 0 or greater.");
 
        RuleFor(x => x.ToPos)
            .GreaterThanOrEqualTo(0).WithMessage("toPos must be 0 or greater.")
            .GreaterThan(x => x.FromPos).WithMessage("toPos must be greater than fromPos.");
 
        // Prevent ridiculously large selections
        RuleFor(x => x)
            .Must(x => x.ToPos - x.FromPos <= 1000)
            .WithMessage("Highlighted range cannot exceed 1000 characters.")
            .OverridePropertyName("range");
 
        RuleFor(x => x.QuoteText)
            .NotEmpty().WithMessage("Quote text is required.")
            .MaximumLength(500).WithMessage("Quote text must not exceed 500 characters.");
 
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Comment content is required.")
            .MinimumLength(1).WithMessage("Comment cannot be empty.")
            .MaximumLength(2000).WithMessage("Comment must not exceed 2000 characters.")
            .Must(c => c != null && !string.IsNullOrWhiteSpace(c)).WithMessage("Comment cannot be whitespace only.");
    }
}
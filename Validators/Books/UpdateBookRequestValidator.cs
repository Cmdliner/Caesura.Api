using FluentValidation;

namespace Caesura.Api.Validators.Books;

public class UpdateBookRequestValidator : AbstractValidator<UpdateBookRequest>
{
    private static readonly HashSet<string> ValidStatuses = ["draft", "published", "completed"];
 
    public UpdateBookRequestValidator()
    {
        RuleFor(x => x.Title)
            .MinimumLength(1).WithMessage("Title cannot be empty.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.")
            .Must(t => !string.IsNullOrWhiteSpace(t)).WithMessage("Title cannot be whitespace only.")
            .When(x => x.Title is not null);
 
        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.")
            .When(x => x.Description is not null);
 
        RuleFor(x => x.CoverUrl)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var uri)
                         && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            .WithMessage("Cover URL must be a valid HTTP/HTTPS URL.")
            .When(x => x.CoverUrl is not null);
 
        RuleFor(x => x.Status)
            .Must(s => ValidStatuses.Contains(s!.ToLower()))
            .WithMessage($"Status must be one of: {string.Join(", ", ValidStatuses)}.")
            .When(x => x.Status is not null);
    }
}
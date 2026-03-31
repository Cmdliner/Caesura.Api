using FluentValidation;

namespace Caesura.Api.Validators.Books;

public class CreateBookRequestValidator : AbstractValidator<CreateBookRequest>
{
    private static readonly HashSet<string> SupportedLanguages =
        ["en", "fr", "de", "es", "pt", "it", "nl", "ru", "zh", "ja", "ar"];
 
    public CreateBookRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MinimumLength(1).WithMessage("Title cannot be empty.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.")
            .Must(t => !string.IsNullOrWhiteSpace(t))
            .WithMessage("Title cannot be whitespace only.");
 
        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.")
            .When(x => x.Description is not null);
 
        RuleFor(x => x.CoverUrl)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var uri)
                         && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            .WithMessage("Cover URL must be a valid HTTP/HTTPS URL.")
            .When(x => x.CoverUrl is not null);
 
        RuleFor(x => x.Language)
            .Must(lang => SupportedLanguages.Contains(lang.ToLower()))
            .WithMessage($"Language must be one of: {string.Join(", ", SupportedLanguages)}.")
            .When(x => x.Language is not null);
    }
}
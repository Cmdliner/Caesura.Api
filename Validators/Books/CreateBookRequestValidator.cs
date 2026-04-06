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
            .Must(t => t != null && !string.IsNullOrWhiteSpace(t))
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
            .NotEmpty().WithMessage("Language is required.")
            .Must(lang => SupportedLanguages.Contains(lang.ToLower()))
            .WithMessage($"Language must be one of: {string.Join(", ", SupportedLanguages)}.");

        RuleFor(x => x.Authors)
            .Must(authors => authors is null || authors.Count <= 10)
            .WithMessage("Authors must not exceed 10 entries.")
            .Must(authors => authors is null || authors
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count() == authors.Count)
            .WithMessage("Authors must not contain duplicates.");

        RuleForEach(x => x.Authors)
            .NotEmpty().WithMessage("Author name is required.")
            .Must(author => !string.IsNullOrWhiteSpace(author))
            .WithMessage("Author name cannot be whitespace only.")
            .MaximumLength(200).WithMessage("Author name must not exceed 200 characters.");

        RuleFor(x => x.GenreIds)
            .Must(genreIds => genreIds is null || genreIds.Count <= 20)
            .WithMessage("Genre IDs must not exceed 20 entries.")
            .Must(genreIds => genreIds is null || genreIds.Distinct().Count() == genreIds.Count)
            .WithMessage("Genre IDs must not contain duplicates.");

        RuleForEach(x => x.GenreIds)
            .GreaterThan(0).WithMessage("Genre ID must be greater than 0.");
    }
}
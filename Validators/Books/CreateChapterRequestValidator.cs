using FluentValidation;

namespace Caesura.Api.Validators.Books;

public class CreateChapterRequestValidator : AbstractValidator<CreateChapterRequest>
{
    public CreateChapterRequestValidator()
    {
        RuleFor(x => x.ChapterNumber)
            .GreaterThan(0).WithMessage("Chapter number must be greater than 0.")
            .LessThanOrEqualTo(10_000).WithMessage("Chapter number cannot exceed 10,000.");
 
        RuleFor(x => x.Title)
            .MaximumLength(200).WithMessage("Chapter title must not exceed 200 characters.")
            .When(x => x.Title is not null);
 
        RuleFor(x => x.Content)
            .Must(BeValidProseMirrorDoc)
            .WithMessage("Content must be a valid ProseMirror document with a 'doc' type.");
 
        RuleFor(x => x.ContentHtml)
            .MaximumLength(500_000).WithMessage("Rendered HTML exceeds maximum allowed size.")
            .When(x => x.ContentHtml is not null);
 
        RuleFor(x => x.WordCount)
            .GreaterThanOrEqualTo(0).WithMessage("Word count cannot be negative.")
            .LessThanOrEqualTo(500_000).WithMessage("Word count seems unreasonably large.")
            .When(x => x.WordCount.HasValue);
 
        RuleFor(x => x.Status)
            .Must(s => s == "draft" || s == "published")
            .WithMessage("Status must be 'draft' or 'published'.");
    }
 
    private static bool BeValidProseMirrorDoc(System.Text.Json.JsonElement content)
    {
        try
        {
            // A valid ProseMirror doc must have { "type": "doc", "content": [...] }
            if (content.ValueKind != System.Text.Json.JsonValueKind.Object) return false;
            if (!content.TryGetProperty("type", out var type)) return false;
            if (type.GetString() != "doc") return false;
            if (!content.TryGetProperty("content", out var contentArr)) return false;
            return contentArr.ValueKind == System.Text.Json.JsonValueKind.Array;
        }
        catch
        {
            return false;
        }
    }
}
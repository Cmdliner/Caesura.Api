using FluentValidation;

namespace Caesura.Api.Validators.Books;

public class UpdateChapterRequestValidator : AbstractValidator<UpdateChapterRequest>
{
    public UpdateChapterRequestValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(200).WithMessage("Chapter title must not exceed 200 characters.")
            .When(x => x.Title is not null);
 
        RuleFor(x => x.Content)
            .Must(c => BeValidProseMirrorDoc(c!.Value))
            .WithMessage("Content must be a valid ProseMirror document.")
            .When(x => x.Content.HasValue);
 
        RuleFor(x => x.ContentHtml)
            .MaximumLength(500_000).WithMessage("Rendered HTML exceeds maximum allowed size.")
            .When(x => x.ContentHtml is not null);
 
        RuleFor(x => x.WordCount)
            .GreaterThanOrEqualTo(0).WithMessage("Word count cannot be negative.")
            .When(x => x.WordCount.HasValue);
 
        RuleFor(x => x.Status)
            .Must(s => s == "draft" || s == "published")
            .WithMessage("Status must be 'draft' or 'published'.")
            .When(x => x.Status is not null);
    }
 
    private static bool BeValidProseMirrorDoc(System.Text.Json.JsonElement content)
    {
        try
        {
            if (content.ValueKind != System.Text.Json.JsonValueKind.Object) return false;
            if (!content.TryGetProperty("type", out var type)) return false;
            if (type.GetString() != "doc") return false;
            if (!content.TryGetProperty("content", out var arr)) return false;
            return arr.ValueKind == System.Text.Json.JsonValueKind.Array;
        }
        catch { return false; }
    }
}
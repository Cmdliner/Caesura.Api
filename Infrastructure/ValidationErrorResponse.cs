using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Caesura.Api.Infrastructure;

/// <summary>
/// Shapes FluentValidation errors into a consistent snake_case JSON response:
/// {
///   "error": "Validation failed",
///   "errors": {
///     "email": ["Email must be a valid email address."],
///     "password": ["Password must be at least 8 characters.", "Password must contain at least one number."]
///    }
/// }
/// </summary>
public static class ValidationErrorResponse
{
    public static IActionResult Build(FluentValidation.Results.ValidationResult result)
    {
        var errors = result.Errors
            .GroupBy(e => ToSnakeCase(e.PropertyName))
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            );

        return new UnprocessableEntityObjectResult(new
        {
            error = "Validation failed.",
            errors
        });
    }

    // Reuse your SnakeCaseNamingPolicy logic for field names in errors
    private static string ToSnakeCase(string name)
    {
        return string.IsNullOrEmpty(name) ? name : SnakeCaseNamingPolicy.Instance.ConvertName(name);
    }
}

/// <summary>
/// Extension to validate and return immediately from a controller action.
/// Usage:
///   var check = await this.ValidateAsync(validator, request);
///   if (check is not null) return check;
/// </summary>
public static class ControllerValidationExtensions
{
    public static async Task<IActionResult?> ValidateAsync<T>(
        this ControllerBase controller,
        IValidator<T> validator,
        T instance)
    {
        var result = await validator.ValidateAsync(instance);
        return result.IsValid ? null : ValidationErrorResponse.Build(result);
    }
}
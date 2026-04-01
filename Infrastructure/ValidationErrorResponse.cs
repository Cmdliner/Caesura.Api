using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Caesura.Api.Infrastructure;

/// <summary>
/// Shapes FluentValidation errors into a consistent snake_case JSON response:
/// {
///   "error": "Validation failed.",
///   "errors": {
///     "email": ["Email must be a valid email address."],
///     "password": ["Password must be at least 8 characters."]
///   }
/// }
/// </summary>
public static class ValidationErrorResponse
{
    public static IActionResult Build(FluentValidation.Results.ValidationResult result)
    {
        var errors = result.Errors
            .GroupBy(e => SnakeCaseNamingPolicy.Instance.ConvertName(e.PropertyName))
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

    /// <summary>
    /// Returns a 400 response when the request body is null/missing entirely.
    /// This happens when the client sends no body or an empty body.
    /// </summary>
    public static IActionResult MissingBody() =>
        new BadRequestObjectResult(new
        {
            error = "Request body is required."
        });
}

/// <summary>
/// Extension to validate a DTO and return immediately if invalid.
/// Handles null instances gracefully — returns 400 instead of throwing.
///
/// Usage in a controller:
///   var check = await this.ValidateAsync(validator, request);
///   if (check is not null) return check;
/// </summary>
public static class ControllerValidationExtensions
{
    public static async Task<IActionResult?> ValidateAsync<T>(
        this ControllerBase _,
        IValidator<T> validator,
        T? instance)
        where T : class
    {
        // Guard: if the model binder gave us null (missing/empty body),
        // return a clean 400 instead of letting FluentValidation throw
        if (instance is null)
            return ValidationErrorResponse.MissingBody();

        var result = await validator.ValidateAsync(instance);
        return result.IsValid ? null : ValidationErrorResponse.Build(result);
    }
}
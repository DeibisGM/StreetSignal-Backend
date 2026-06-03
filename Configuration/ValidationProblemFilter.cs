using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using StreetSignalApi.Common.Errors;
using StreetSignalApi.DTOs.Responses;

namespace StreetSignalApi.Configuration;

public static class ValidationProblemFilter
{
    public static IActionResult Build(ActionContext context)
    {
        var errors = context.ModelState
            .Where(kv => kv.Value is not null && kv.Value!.Errors.Count > 0)
            .ToDictionary(
                kv => ToCamelCase(kv.Key),
                kv => kv.Value!.Errors
                    .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage)
                        ? "Invalid value."
                        : e.ErrorMessage)
                    .ToArray()
            );

        var payload = new ValidationErrorResponse
        {
            Code = ErrorCodes.ValidationError,
            Message = "One or more validation errors occurred.",
            Errors = errors
        };

        return new BadRequestObjectResult(payload)
        {
            ContentTypes = { "application/json" }
        };
    }

    private static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name) || char.IsLower(name[0])) return name;
        // Handle indexed names like "Items[0].Title" → "items[0].title" on the first segment only
        return char.ToLowerInvariant(name[0]) + name.Substring(1);
    }
}

using System.Text.Json;
using StreetSignalApi.Common.Errors;
using StreetSignalApi.DTOs.Responses;

namespace StreetSignalApi.Configuration;

public static class AuthResponseFactories
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public static Task WriteUnauthorizedAsync(HttpContext ctx)
    {
        if (ctx.Response.HasStarted) return Task.CompletedTask;
        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
        ctx.Response.ContentType = "application/json";
        var body = new ErrorResponse { Code = ErrorCodes.Unauthorized, Message = "Authentication is required." };
        return ctx.Response.WriteAsync(JsonSerializer.Serialize(body, JsonOpts));
    }

    public static Task WriteForbiddenAsync(HttpContext ctx)
    {
        if (ctx.Response.HasStarted) return Task.CompletedTask;
        ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
        ctx.Response.ContentType = "application/json";
        var body = new ErrorResponse { Code = ErrorCodes.Forbidden, Message = "You do not have permission to access this resource." };
        return ctx.Response.WriteAsync(JsonSerializer.Serialize(body, JsonOpts));
    }
}

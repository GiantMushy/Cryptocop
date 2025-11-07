using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cryptocop.Software.API.Middleware;

public class ProblemDetailsExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ProblemDetailsExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await WriteProblemDetailsAsync(context, ex);
        }
    }

    private static async Task WriteProblemDetailsAsync(HttpContext context, Exception ex)
    {
        var (status, title) = MapExceptionToStatus(ex);

        var problem = new ProblemDetails
        {
            Status = (int)status,
            Title = title,
            Detail = ex.Message,
            Instance = context.Request.Path
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = problem.Status ?? (int)HttpStatusCode.InternalServerError;

        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });
        await context.Response.WriteAsync(json);
    }

    private static (HttpStatusCode status, string title) MapExceptionToStatus(Exception ex)
    {
        switch (ex)
        {
            case ArgumentException:
                return (HttpStatusCode.UnprocessableEntity, "Validation failed"); // 422
            case UnauthorizedAccessException:
                return (HttpStatusCode.Unauthorized, "Unauthorized");
            case DbUpdateException:
                return (HttpStatusCode.Conflict, "Conflict");
            case InvalidOperationException ioe:
                var msg = ioe.Message.ToLowerInvariant();
                if (msg.Contains("not found")) return (HttpStatusCode.NotFound, "Not Found");
                if (msg.Contains("empty")) return (HttpStatusCode.UnprocessableEntity, "Unprocessable Entity");
                return (HttpStatusCode.BadRequest, "Bad Request");
            default:
                return (HttpStatusCode.InternalServerError, "Internal Server Error");
        }
    }
}

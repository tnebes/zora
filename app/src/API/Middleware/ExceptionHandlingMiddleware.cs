#region

using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

#endregion

namespace zora.API.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly IHostEnvironment _environment;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        this._next = next;
        this._logger = logger;
        this._environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await this._next(context);
        }
        catch (Exception exception)
        {
            this._logger.LogError(exception, "An unexpected error occurred.");
            await this.HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        ProblemDetails problem = this.CreateProblemDetails(context, exception);
        context.Response.StatusCode = problem.Status ?? (int)HttpStatusCode.InternalServerError;

        string json = JsonSerializer.Serialize(problem);
        await context.Response.WriteAsync(json);
    }

    private ProblemDetails CreateProblemDetails(HttpContext context, Exception exception)
    {
        int statusCode = GetStatusCodeForException(exception);
        Dictionary<string, object?> extensions = this.CreateExtensions(context, exception);

        return new ProblemDetails
        {
            Status = statusCode,
            Title = GetTitleForException(exception),
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Instance = context.TraceIdentifier,
            Detail = this._environment.IsDevelopment() ? exception.ToString() : null,
            Extensions = extensions
        };
    }

    private Dictionary<string, object?> CreateExtensions(HttpContext context, Exception exception)
    {
        Dictionary<string, object?> extensions = new()
        {
            ["correlationId"] = context.TraceIdentifier
        };

        if (this._environment.IsDevelopment())
        {
            extensions["stackTrace"] = exception.StackTrace;
        }

        return extensions;
    }

    private static int GetStatusCodeForException(Exception exception)
    {
        return exception switch
        {
            ArgumentException => (int)HttpStatusCode.BadRequest,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            ValidationException => (int)HttpStatusCode.BadRequest,
            InvalidOperationException => (int)HttpStatusCode.BadRequest,
            FileNotFoundException => (int)HttpStatusCode.NotFound,
            DirectoryNotFoundException => (int)HttpStatusCode.NotFound,
            NotSupportedException => (int)HttpStatusCode.NotImplemented,
            _ => (int)HttpStatusCode.InternalServerError
        };
    }

    private static string GetTitleForException(Exception exception)
    {
        return exception switch
        {
            ArgumentException => "Invalid request parameters",
            UnauthorizedAccessException => "Access denied",
            ValidationException => "Validation failed",
            InvalidOperationException => "Invalid operation",
            FileNotFoundException => "File not found",
            DirectoryNotFoundException => "Directory not found",
            NotSupportedException => "Operation not supported",
            _ => "An error occurred while processing your request."
        };
    }
}

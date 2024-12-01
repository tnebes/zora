#region

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

        ProblemDetails problem = new()
        {
            Status = (int)HttpStatusCode.InternalServerError,
            Title = "An error occurred while processing your request.",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
        };

        if (this._environment.IsDevelopment())
        {
            problem.Detail = exception.ToString();
            problem.Instance = context.TraceIdentifier;
        }

        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}

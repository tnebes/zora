#region

using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

#endregion

namespace zora.API.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly IHostEnvironment environment;
    private readonly ILogger<ExceptionHandlingMiddleware> logger;
    private readonly RequestDelegate next;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        this.next = next;
        this.logger = logger;
        this.environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await this.next(context);
        }
        catch (Exception exception)
        {
            this.logger.LogError(exception, "An unexpected error occurred.");
            await this.HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        ProblemDetails problem = new()
        {
            Status = context.Response.StatusCode,
            Title = "An error occurred while processing your request.",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Instance = context.TraceIdentifier
        };

        if (this.environment.IsDevelopment())
        {
            problem.Detail = exception.ToString();
        }

        string json = JsonSerializer.Serialize(problem);
        await context.Response.WriteAsync(json);
    }
}

namespace zora.API.Middleware;

public sealed class JwtLoggingMiddleware
{
    private readonly ILogger<JwtLoggingMiddleware> _logger;
    private readonly RequestDelegate _next;

    public JwtLoggingMiddleware(RequestDelegate next, ILogger<JwtLoggingMiddleware> logger)
    {
        this._next = next;
        this._logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string? authHeader = context.Request.Headers.Authorization.ToString();

        if (!string.IsNullOrEmpty(authHeader))
        {
            this._logger.LogInformation("Request path: {Path}", context.Request.Path);
            this._logger.LogInformation("Auth header present: {Header}", authHeader);
        }

        await this._next(context);
    }
}

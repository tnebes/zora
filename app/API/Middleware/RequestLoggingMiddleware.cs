namespace zora.API.Middleware;

public sealed class RequestLoggingMiddleware
{
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    private readonly RequestDelegate _next;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        this._next = next;
        this._logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await this._next(context);
        }
        finally
        {
            this._logger.LogInformation(
                "Request {Method} {Url} => {StatusCode}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode);

            if (context.Request.Headers.Authorization.Count > 0)
            {
                this._logger.LogDebug(
                    "Authorization header present: {Type}",
                    context.Request.Headers.Authorization.ToString().Split(' ')[0]);
            }
        }
    }
}

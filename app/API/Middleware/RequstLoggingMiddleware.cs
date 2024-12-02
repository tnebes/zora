namespace zora.API.Middleware;

public class RequestLoggingMiddleware
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
        this._logger.LogInformation(
            "Request {method} {url} => {statusCode}",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode);

        this._logger.LogInformation(
            "Authorization header: {auth}",
            context.Request.Headers.Authorization.ToString());

        await this._next(context);
    }
}

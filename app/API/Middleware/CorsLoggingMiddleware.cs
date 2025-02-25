namespace zora.API.Middleware;

public sealed class CorsLoggingMiddleware
{
    private readonly ILogger<CorsLoggingMiddleware> _logger;
    private readonly RequestDelegate _next;

    public CorsLoggingMiddleware(RequestDelegate next, ILogger<CorsLoggingMiddleware> logger)
    {
        this._next = next;
        this._logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        this._logger.LogDebug(
            "CORS Request Details - Origin: {Origin}, Method: {Method}, Headers: {Headers}",
            context.Request.Headers.Origin,
            context.Request.Headers.AccessControlRequestMethod,
            context.Request.Headers.AccessControlRequestHeaders);

        if (context.Request.Method == "OPTIONS")
        {
            this._logger.LogDebug("CORS Preflight request received from Origin: {Origin}",
                context.Request.Headers.Origin);
        }

        context.Response.OnStarting(() =>
        {
            this._logger.LogDebug(
                "CORS Response Headers - Origin: {Origin}, Methods: {Methods}, Headers: {Headers}, Credentials: {Credentials}",
                context.Response.Headers.AccessControlAllowOrigin,
                context.Response.Headers.AccessControlAllowMethods,
                context.Response.Headers.AccessControlAllowHeaders,
                context.Response.Headers.AccessControlAllowCredentials);

            if (!context.Response.Headers.AccessControlAllowOrigin.Any())
            {
                this._logger.LogWarning("No Access-Control-Allow-Origin header in response. Request Origin: {Origin}",
                    context.Request.Headers.Origin);
            }

            return Task.CompletedTask;
        });

        await this._next(context);
    }
}

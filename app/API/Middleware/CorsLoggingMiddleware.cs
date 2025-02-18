using Microsoft.AspNetCore.Cors.Infrastructure;

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
        this._logger.LogDebug("CORS Request Details:");
        this._logger.LogDebug("Origin: {Origin}", context.Request.Headers.Origin);
        this._logger.LogDebug("Access-Control-Request-Method: {Method}", 
            context.Request.Headers.AccessControlRequestMethod);
        this._logger.LogDebug("Access-Control-Request-Headers: {Headers}", 
            context.Request.Headers.AccessControlRequestHeaders);

        if (context.Request.Method == "OPTIONS")
        {
            this._logger.LogInformation("CORS Preflight request received from Origin: {Origin}", 
                context.Request.Headers.Origin);
        }

        context.Response.OnStarting(() =>
        {
            this._logger.LogDebug("CORS Response Headers:");
            this._logger.LogDebug("Access-Control-Allow-Origin: {Origin}", 
                context.Response.Headers.AccessControlAllowOrigin);
            this._logger.LogDebug("Access-Control-Allow-Methods: {Methods}", 
                context.Response.Headers.AccessControlAllowMethods);
            this._logger.LogDebug("Access-Control-Allow-Headers: {Headers}", 
                context.Response.Headers.AccessControlAllowHeaders);
            this._logger.LogDebug("Access-Control-Allow-Credentials: {Credentials}", 
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
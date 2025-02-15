#region

using System.IdentityModel.Tokens.Jwt;

#endregion

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
        DateTime startTime = DateTime.UtcNow;
        string? authHeader = context.Request.Headers.Authorization.ToString();

        if (!string.IsNullOrEmpty(authHeader))
        {
            string authType = authHeader.Split(' ')[0];
            this._logger.LogInformation(
                "Request received - Path: {Path}, Method: {Method}, AuthType: {AuthType}, CorrelationId: {CorrelationId}",
                context.Request.Path,
                context.Request.Method,
                authType,
                context.TraceIdentifier
            );

            if (authType.Equals("Bearer", StringComparison.OrdinalIgnoreCase))
            {
                this.LogJwtDetails(authHeader);
            }
        }

        await this._next(context);

        TimeSpan elapsed = DateTime.UtcNow - startTime;
        this._logger.LogDebug(
            "Request completed - Path: {Path}, Status: {StatusCode}, Duration: {Elapsed}ms",
            context.Request.Path,
            context.Response.StatusCode,
            elapsed.TotalMilliseconds
        );
    }

    private void LogJwtDetails(string authHeader)
    {
        try
        {
            string token = authHeader.Split(' ')[1];
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

            if (handler.CanReadToken(token))
            {
                JwtSecurityToken? jwtToken = handler.ReadJwtToken(token);
                this._logger.LogDebug("JWT Details - Issuer: {Issuer}, Subject: {Subject}, ValidTo: {ValidTo}",
                    jwtToken.Issuer,
                    jwtToken.Subject,
                    jwtToken.ValidTo);
            }
        }
        catch (Exception ex)
        {
            this._logger.LogWarning(ex, "Failed to parse JWT token");
        }
    }
}

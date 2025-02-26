#region

using System.Diagnostics;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerUI;
using zora.API.Middleware;
using zora.Core;
using zora.Core.Enums;
using zora.Core.Interfaces.Services;

#endregion

namespace zora.Extensions;

public static class AppExtensions
{
    public static WebApplication ConfigureApplication(this WebApplication app,
        IEnvironmentManagerService environmentManager)
    {
        Log.Information("Starting application configuration");
        app.ConfigureAdvancedLogging();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseMiddleware<JwtLoggingMiddleware>();

        if (environmentManager.CurrentEnvironment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            Log.Information("Developer exception page is enabled.");
            app.UseHttpsRedirection();
            Log.Information("HTTPS redirection is enabled.");
        }
        else
        {
            Log.Information("Configuring for production environment");
            Log.Information("HTTPS redirection is disabled.");
        }

        Log.Information("Configuring static files, routing, and authentication pipeline");
        app.UseStaticFiles();
        app.UseRouting();
        app.UseMiddleware<CorsLoggingMiddleware>();
        app.UseCors(Constants.ZORA_CORS_POLICY_NAME);
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        app.ConfigureSwagger();
        Log.Information("Application configuration completed");

        return app;
    }

    private static WebApplication ConfigureSwagger(this WebApplication app)
    {
        Log.Information("Configuring Swagger documentation");
        app.ConfigureSwaggerOptions();
        Log.Information("Swagger UI is available at /swagger");
        return app;
    }

    private static WebApplication ConfigureSwaggerOptions(this WebApplication app)
    {
        Log.Information("Setting up Swagger UI options");
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Zora API v1");
            options.DocExpansion(DocExpansion.List);
            options.DefaultModelsExpandDepth(-1);
            options.DisplayRequestDuration();
            options.EnableFilter();
            options.EnableDeepLinking();
            options.EnableValidator();
            options.ShowExtensions();
            options.EnableTryItOutByDefault();
        });
        Log.Debug("Swagger UI configured with expanded lists and try-it-out enabled");
        return app;
    }

    private static WebApplication ConfigureAdvancedLogging(this WebApplication app)
    {
        Log.Information("Setting up request performance tracking middleware");
        app.Use(async (context, next) =>
        {
            Log.Information("Request {Method} {Path} started", context.Request.Method, context.Request.Path);

            Stopwatch timer = Stopwatch.StartNew();
            try
            {
                await next();
                timer.Stop();

                Log.Information(
                    "Request {Method} {Path} completed in {ElapsedMilliseconds}ms with status code {StatusCode}",
                    context.Request.Method,
                    context.Request.Path,
                    timer.ElapsedMilliseconds,
                    context.Response.StatusCode);
            }
            catch (Exception ex)
            {
                timer.Stop();
                Log.Error(
                    ex,
                    "Request {Method} {Path} failed after {ElapsedMilliseconds}ms",
                    context.Request.Method,
                    context.Request.Path,
                    timer.ElapsedMilliseconds);
                throw;
            }
        });
        return app;
    }
}

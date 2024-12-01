#region

using Swashbuckle.AspNetCore.SwaggerUI;
using zora.API.Middleware;
using zora.Core.Interfaces;

#endregion

namespace zora.Extensions;

public static class AppExtensions
{
    public static WebApplication ConfigureApplication(this WebApplication app,
        IEnvironmentManagerService environmentManager)
    {
        app.UseCors("CorsPolicy");

        if (environmentManager.IsDevelopment())
        {
            app.ConfigureSwagger();
        }

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseStaticFiles();
        app.UseDefaultFiles();
        app.MapControllers();
        app.MapFallbackToFile("index.html");
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        return app;
    }

    private static WebApplication ConfigureSwagger(this WebApplication app)
    {
        app.ConfigureSwaggerOptions();
        Console.WriteLine("Swagger UI is available at /swagger");
        return app;
    }

    private static WebApplication ConfigureSwaggerOptions(this WebApplication app)
    {
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
        return app;
    }
}

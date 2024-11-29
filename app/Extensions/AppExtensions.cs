using Swashbuckle.AspNetCore.SwaggerUI;
using zora.Common.Enums;
using zora.Services.Configuration;

namespace zora.Extensions
{
    public static class AppExtensions
    {

        public static WebApplication ConfigureApplication(this WebApplication app, ISecretsManagerService secretsManager)
        {
            if (secretsManager.CurrentEnvironment.IsDevelopment())
            {
                app.ConfigureSwagger();
            }
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCors("CorsPolicy");
            app.UseStaticFiles();
            app.UseDefaultFiles();
            app.MapControllers();
            app.MapFallbackToFile("index.html");
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
}

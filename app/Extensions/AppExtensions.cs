using Swashbuckle.AspNetCore.SwaggerUI;

namespace zora.Extensions
{
    public static class AppExtensions
    {
        public static WebApplication ConfigureApplication(this WebApplication app)
        {
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.UseCors("CorsPolicy");
            app.UseStaticFiles();
            app.UseDefaultFiles();
            app.MapFallbackToFile("index.html");
            return app;
        }

        public static WebApplication ConfigureForDevEnvironment(this WebApplication app)
        {

            ConfigObject swaggerConfigObject = new()
            {
                //DeepLinking = true,
                //DisplayOperationId = true,
                //DefaultModelsExpandDepth = 3,
                //DefaultModelExpandDepth = 3,
                //DefaultModelRendering = ModelRendering.Model,
                //DisplayRequestDuration = true,
                DocExpansion = DocExpansion.None,
                //ShowExtensions = true,
                //ShowCommonExtensions = true,
                TryItOutEnabled = true
            };

            SwaggerUIOptions swaggerUIOptions = new()
            {
                RoutePrefix = string.Empty,
                DocumentTitle = "Zora API",
                HeadContent = "<style>body { background-color: #f0f0f0 !important; }</style>",
                ConfigObject = swaggerConfigObject
            };

            swaggerUIOptions.SwaggerEndpoint("/swagger/v1/swagger.json", "Zora API V1");

            
            app.UseSwagger();
            app.UseSwaggerUI(swaggerUIOptions);
            Console.WriteLine("Swagger UI is available at /swagger");
            return app;
        }
    }
}

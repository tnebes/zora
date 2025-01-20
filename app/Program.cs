#region

using Serilog;
using zora.Core.Interfaces.Services;
using zora.Extensions;

#endregion

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
try
{
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services));

    builder.Services.AddCustomServices(builder.Configuration);
    WebApplication app = builder.Build();

    IEnvironmentManagerService environmentManager =
        app.Services.GetRequiredService<IEnvironmentManagerService>();

    await app.ConfigureApplication(environmentManager).RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex,
        "Application terminated unexpectedly due to an unhandled exception.\nPlease read the logs for more information.");
    string path = Path.Combine(Directory.GetCurrentDirectory(), "logs", "crash.log");
    await File.WriteAllTextAsync(path, ex.ToString());
}
finally
{
    Log.Information("Committing seppuku.");
    await Log.CloseAndFlushAsync();
}

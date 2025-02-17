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
    writeToCrashLog(ex);

    async void writeToCrashLog(Exception exception)
    {
        string logName = "crash.log";
        string logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
        string fullPath = Path.Combine(logDirectory, logName);
        if (!File.Exists(fullPath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(logDirectory));
        }

        await File.WriteAllTextAsync(fullPath, ex.ToString());
    }
}
finally
{
    Log.Information("Committing seppuku.");
    await Log.CloseAndFlushAsync();
}

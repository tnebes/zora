#region

using Serilog;
using zora.Core.Interfaces.Services;
using zora.Extensions;

#endregion

ConfigureLogging();

try
{
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    builder.Services.AddCustomServices(builder.Configuration, builder.Environment.IsDevelopment());
    WebApplication app = builder.Build();

    IEnvironmentManagerService environmentManager =
        app.Services.GetRequiredService<IEnvironmentManagerService>();

    Log.Information("Running in {0} mode.", environmentManager.CurrentEnvironment);

    await app.ConfigureApplication(environmentManager).RunAsync();
}
catch (Exception ex)
{
    await WriteToCrashLogAsync(ex);
    Log.Fatal(ex,
        "Application terminated unexpectedly due to an unhandled exception.\nPlease read the logs for more information.");

    throw;
}
finally
{
    Log.Information("Shutting down.");
    await Log.CloseAndFlushAsync();
}

static void ConfigureLogging()
{
    string? environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    IConfigurationRoot configuration = new ConfigurationBuilder()
        .SetBasePath(environment == "Development"
            ? Directory.GetCurrentDirectory()
            : Path.Combine(Directory.GetCurrentDirectory(), "app"))
        .AddJsonFile("appsettings.json", false, true)
        .AddJsonFile($"appsettings.{environment}.json", true, true)
        .Build();

    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(configuration)
        .CreateLogger();

    Log.Information("Logging configured successfully.");
}

static async Task WriteToCrashLogAsync(Exception exception)
{
    const string logName = "crash.log";
    string logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
    string fullPath = Path.Combine(logDirectory, logName);

    try
    {
        Directory.CreateDirectory(logDirectory);
        await File.WriteAllTextAsync(fullPath, exception.ToString());
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to write crash log");
    }
}

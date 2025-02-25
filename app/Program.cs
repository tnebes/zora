#region

using Serilog;
using Serilog.Exceptions;
using zora.Core.Interfaces.Services;
using zora.Extensions;

#endregion

try
{
    Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .CreateBootstrapLogger();

    Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "logs"));

    Log.Information("Starting up application");
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithExceptionDetails());

    builder.Services.AddCustomServices(builder.Configuration, builder.Environment.IsDevelopment());

    builder.WebHost.ConfigureKestrel(options => { options.AddServerHeader = false; });

    Log.Information("All services registered, building application");
    WebApplication app = builder.Build();

    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(app.Services)
        .Enrich.FromLogContext()
        .Enrich.WithExceptionDetails()
        .CreateLogger();

    Log.Information("Application built successfully");
    IEnvironmentManagerService environmentManager =
        app.Services.GetRequiredService<IEnvironmentManagerService>();

    Log.Information("Starting application in {Environment} mode", environmentManager.CurrentEnvironment);

    await app.ConfigureApplication(environmentManager).RunAsync();
}
catch (Exception ex)
{
    await WriteToCrashLogAsync(ex);
    Log.Fatal(ex,
        "Application terminated unexpectedly due to an unhandled exception. See crash log for details.");

    throw;
}
finally
{
    Log.Information("Shutting down application");
    await Log.CloseAndFlushAsync();
}

static async Task WriteToCrashLogAsync(Exception exception)
{
    const string logName = "crash.log";
    string logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
    string fullPath = Path.Combine(logDirectory, logName);

    try
    {
        Directory.CreateDirectory(logDirectory);
        string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        string logContent = $"[{timestamp} UTC]\n{exception}\n\n";

        await File.AppendAllTextAsync(fullPath, logContent);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to write crash log to {LogPath}", fullPath);
    }
}

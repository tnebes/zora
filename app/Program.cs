#region

using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using zora.Core.Interfaces.Services;
using zora.Extensions;
using Microsoft.Extensions.Configuration;

#endregion

try
{
    Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .CreateBootstrapLogger();

    Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "logs"));

    Log.Information("Starting up application");
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("System", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .Enrich.WithExceptionDetails()
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
        .WriteTo.File(
            Path.Combine("logs", "log-.txt"),
            rollingInterval: RollingInterval.Day,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
        .CreateLogger();

    builder.Host.UseSerilog();

    builder.Services.AddCustomServices(builder.Configuration, builder.Environment.IsDevelopment());

    builder.WebHost.ConfigureKestrel(options => { options.AddServerHeader = false; });

    Log.Information("All services registered, building application");
    WebApplication app = builder.Build();

    try
    {
        Log.Information("Performing Serilog diagnostic check");
        Log.Debug("Debug level message - if you see this, debug logging is enabled");
        Log.Information("Information level message - if you see this, information logging is enabled");
        Log.Warning("Warning level message - if you see this, warning logging is enabled");
        Log.Error("Error level message - if you see this, error logging is enabled");
        Log.Information("Serilog diagnostic check completed");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error during Serilog diagnostic check: {ex.Message}");
    }

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

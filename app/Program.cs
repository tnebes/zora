#region

using Serilog;
using zora.Core.Interfaces.Services;
using zora.Extensions;

#endregion

try
{
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration));

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

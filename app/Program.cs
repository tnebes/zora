#region

using Serilog;
using zora.Core.Interfaces.Services;
using zora.Extensions;

#endregion

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
try
{
    if (builder.Environment.IsDevelopment())
    {
        builder.Host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services));
        Console.WriteLine("Writing logs in development mode.");
    }
    else
    {
        builder.Host.UseSystemd();
        Console.WriteLine("Writing logs in production mode.");
    }

    builder.Services.AddCustomServices(builder.Configuration, builder.Environment.IsDevelopment());
    WebApplication app = builder.Build();

    IEnvironmentManagerService environmentManager =
        app.Services.GetRequiredService<IEnvironmentManagerService>();

    Log.Information("Running in {0} mode.", environmentManager.CurrentEnvironment);

    await app.ConfigureApplication(environmentManager).RunAsync();
}
catch (Exception ex)
{
    WriteToCrashLog(ex);
    Log.Fatal(ex,
        "Application terminated unexpectedly due to an unhandled exception.\nPlease read the logs for more information.");

    async void WriteToCrashLog(Exception exception)
    {
        const string logName = "crash.log";
        string logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
        string fullPath = Path.Combine(logDirectory, logName);
        Directory.CreateDirectory(logDirectory);
        await File.WriteAllTextAsync(fullPath, exception.ToString());
    }

    throw;
}
finally
{
    Log.Information("Committing seppuku.");
    await Log.CloseAndFlushAsync();
}

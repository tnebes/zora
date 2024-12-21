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
    Console.WriteLine(ex);
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.Information("Commiting seppuku");
    await Log.CloseAndFlushAsync();
}

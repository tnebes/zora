#region

using Serilog;
using zora.Core.Interfaces;
using zora.Extensions;

#endregion

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));

builder.Services.AddCustomServices(builder.Configuration);
WebApplication app = builder.Build();

IEnvironmentManagerService environmentManager = app.Services.GetRequiredService<IEnvironmentManagerService>();

await app.ConfigureApplication(environmentManager).RunAsync();

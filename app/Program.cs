using Serilog;
using zora.Extensions;
using zora.Services.Configuration;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));

builder.Services.AddCustomServices(builder.Configuration);
WebApplication app = builder.Build();

ISecretsManagerService secretsManager = app.Services.GetRequiredService<ISecretsManagerService>();

app.ConfigureApplication(secretsManager).Run();

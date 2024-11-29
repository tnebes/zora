using Serilog;
using zora.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));

builder.Services.AddCustomServices(builder.Configuration);
WebApplication app = builder.Build();

app.ConfigureApplication().Run();


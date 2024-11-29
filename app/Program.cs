using Serilog;
using zora.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(Serilog.Events.LogEventLevel.Debug)
    .WriteTo.File("logs/zora-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7, rollOnFileSizeLimit: true, fileSizeLimitBytes: 1000000, shared: true)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddCustomServices();
WebApplication app = builder.Build();

app.ConfigureApplication().Run();


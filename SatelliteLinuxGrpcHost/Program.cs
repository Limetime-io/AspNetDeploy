using SatelliteLinuxGrpcHost.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddSingleton<IDeploymentService, DeploymentService>();
builder.Services.AddSingleton<IInformationService, InformationService>();
builder.Services.AddSingleton<IMonitoringService, MonitoringService>();

// Configure Kestrel for Linux
builder.WebHost.UseKestrel(options =>
{
    int port = 5000; // Default port, can be configured

    if (builder.Configuration["Service:Port"] != null)
    {
        port = int.Parse(builder.Configuration["Service:Port"]);
    }

    options.ListenAnyIP(port);
});

var app = builder.Build();

app.UseRouting();
app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });

app.UseEndpoints(endpoints =>
{
    endpoints.MapGrpcService<DeploymentController>().EnableGrpcWeb();
    endpoints.MapGrpcService<InformationController>().EnableGrpcWeb();
    endpoints.MapGrpcService<MonitoringController>().EnableGrpcWeb();
});

app.MapGet("/", () => "SatelliteLinuxGrpcHost is running. Communication with gRPC endpoints must be made through a gRPC client.");

app.Run();

using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Utils.Messaging;

const string serviceName = "Microservice 2";

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddOpenTelemetry(options =>
{
    options
        .SetResourceBuilder(
            ResourceBuilder.CreateDefault()
                .AddService(serviceName))
        .AddConsoleExporter();
});

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName))
    .WithTracing(tracing => tracing
        .AddSource(nameof(MessageReceiver))
        .AddAspNetCoreInstrumentation()
        .AddOtlpExporter(opt =>
        {
            opt.Endpoint = new Uri("http://localhost:4317"); // TODO: Change this endpoint to the Docker container's IP address or DNS name.
            opt.Protocol = OtlpExportProtocol.Grpc;
        })
        .AddConsoleExporter());

builder.Services.AddRazorPages();

builder.Services.AddSingleton<MessageReceiver>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseRouting();

app.MapRazorPages();

app.Run();

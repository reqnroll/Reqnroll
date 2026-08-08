#nullable enable

using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using System;

namespace Reqnroll.Diagnostics.Analytics;

public sealed class TelemetryHost : ITelemetryHost
{
    private const string ApplicationInsightsDataCollectionRuleKey = "";

    private readonly ILoggerFactory _loggerFactory;

    public TelemetryHost()
    {
        var resource = ResourceBuilder
            .CreateEmpty()
            .AddDetector(new ReqnrollServiceDetector())
            .AddDetector(new OperatingSystemDetector())
            .AddDetector(new ProcessRuntimeDetector())
            .AddDetector(new CicdPlatformDetector())
            .AddDetector(new ContainerDetector())
            .AddDetector(new UserDetector());

        _loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.ClearProviders();

            builder.AddOpenTelemetry(options =>
            {
                options.SetResourceBuilder(resource);

                options.AddOtlpExporter(options =>
                {
                    options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
                    options.Endpoint = new Uri("https://otlp.monitor.azure.com/v1/logs");
                    options.Headers = $"Authorization=Bearer {ApplicationInsightsDataCollectionRuleKey}";
                });
            });
        });
    }

    public ILogger<T> CreateLogger<T>() => _loggerFactory.CreateLogger<T>();

    public void Dispose() => _loggerFactory.Dispose();
}

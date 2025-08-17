using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Reqnroll.Analytics.AppInsights
{
    /// <summary>
    /// For property names, check: https://github.com/microsoft/ApplicationInsights-Home/tree/master/EndpointSpecs/Schemas/Bond
    /// For tags, check: https://github.com/microsoft/ApplicationInsights-Home/blob/master/EndpointSpecs/Schemas/Bond/ContextTagKeys.bond
    /// </summary>
    public class AppInsightsEventTelemetry
    {
        [JsonPropertyName("name")]
        public string DataTypeName { get; set; }

        [JsonPropertyName("time")]
        public string EventDateTime { get; set; }

        [JsonPropertyName("iKey")]
        public string InstrumentationKey { get; set; }

        [JsonPropertyName("data")]
        public TelemetryData TelemetryData { get; set; }

        [JsonPropertyName("tags")]
        public Dictionary<string, string> TelemetryTags { get; set; }

        private const string DefaultValue = "undefined";

        public AppInsightsEventTelemetry(IAnalyticsEvent analyticsEvent, string instrumentationKey)
        {
            InstrumentationKey = instrumentationKey;
            DataTypeName = $"Microsoft.ApplicationInsights.{InstrumentationKey}.Event";

            EventDateTime = analyticsEvent.UtcDate.ToString("O");

            TelemetryTags = new Dictionary<string, string>
            {
                { "ai.user.id", analyticsEvent.UserId },
                { "ai.user.accountId", analyticsEvent.UserId },
                { "ai.session.id", analyticsEvent.SessionId }
            };

            TelemetryData = new TelemetryData
            {
                ItemTypeName = "EventData",
                TelemetryDataItem = new TelemetryDataItem
                {
                    EventName = analyticsEvent.EventName,
                    Properties = new Dictionary<string, string>
                    {
                        { "UtcDate", analyticsEvent.UtcDate.ToString("O") },
                        { "UserId", analyticsEvent.UserId },
                        { "Platform", analyticsEvent.Platform },
                        { "PlatformDescription", analyticsEvent.PlatformDescription },
                        { "ReqnrollVersion", analyticsEvent.ReqnrollVersion },
                        { "UnitTestProvider", analyticsEvent.UnitTestProvider ?? DefaultValue },
                        { "IsBuildServer", analyticsEvent.IsBuildServer.ToString() },
                        { "BuildServerName", analyticsEvent.BuildServerName ?? DefaultValue },
                        { "IsDockerContainer", analyticsEvent.IsDockerContainer.ToString() },
                        { "HashedAssemblyName", analyticsEvent.HashedAssemblyName ?? DefaultValue },
                        { "TargetFrameworks", analyticsEvent.TargetFrameworks },
                        { "TargetFramework", analyticsEvent.TargetFramework },
                    }
                }
            };

            if (analyticsEvent is ReqnrollProjectCompilingEvent reqnrollProjectCompiledEvent)
            {
                TelemetryData.TelemetryDataItem.Properties.Add("MSBuildVersion", reqnrollProjectCompiledEvent.MSBuildVersion);
                TelemetryData.TelemetryDataItem.Properties.Add("ProjectGuid", reqnrollProjectCompiledEvent.ProjectGuid ?? DefaultValue);
            }

            if (analyticsEvent is ReqnrollFeatureUseEvent featureUseEvent)
            {
                TelemetryData.TelemetryDataItem.Properties.Add(ReqnrollFeatureUseEvent.FeatureNameProperty, featureUseEvent.FeatureName);
                if (featureUseEvent.Properties != null)
                {
                    foreach (var property in featureUseEvent.Properties)
                        TelemetryData.TelemetryDataItem.Properties.Add(property.Key, property.Value ?? DefaultValue);
                }
                if (featureUseEvent.Metrics != null)
                {
                    TelemetryData.TelemetryDataItem.Metrics ??= new Dictionary<string, double>();
                    foreach (var metric in featureUseEvent.Metrics)
                        TelemetryData.TelemetryDataItem.Metrics.Add(metric.Key, metric.Value);
                }
            }
        }
    }
    public class TelemetryData
    {
        [JsonPropertyName("baseType")]
        public string ItemTypeName { get; set; }

        [JsonPropertyName("baseData")]
        public TelemetryDataItem TelemetryDataItem { get; set; }
    }

    public class TelemetryDataItem
    {
        [JsonPropertyName("ver")]
        public string EndPointSchemaVersion => "2";
        [JsonPropertyName("name")]
        public string EventName { get; set; }
        [JsonPropertyName("properties")]
        public Dictionary<string, string> Properties { get; set; }
        [JsonPropertyName("measurements")]
        public Dictionary<string, double> Metrics { get; set; }
    }
}

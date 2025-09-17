using System;
using System.Collections.Generic;

namespace Reqnroll.Analytics;

public class ReqnrollFeatureUseEvent(
    DateTime utcDate,
    string userId,
    string sessionId,
    string platform,
    string platformDescription,
    string reqnrollVersion,
    string unitTestProvider,
    string buildServerName,
    string hashedAssemblyName,
    string targetFrameworks,
    string targetFramework,
    bool isDockerContainer,
    string featureName,
    IDictionary<string, string> properties = null,
    IDictionary<string, double> metrics = null
    )
    : ReqnrollAnalyticsEventBase(
        utcDate,
        userId,
        sessionId,
        platform,
        platformDescription,
        reqnrollVersion,
        unitTestProvider,
        buildServerName,
        hashedAssemblyName,
        targetFrameworks,
        targetFramework,
        isDockerContainer)
{
    public static class FeatureNames
    {
        public const string Formatter = "formatter"; // report the individual formatter used with name
    }

    public const string FeatureNameProperty = "feature_name";
    public const string FormatterNameProperty = "formatter_name";

    public string FeatureName { get; } = featureName;
    public IDictionary<string, string> Properties { get; } = properties;
    public IDictionary<string, double> Metrics { get; } = metrics;

    public override string EventName => "runtime_feature_use";
}
#nullable enable

namespace Reqnroll.Diagnostics.Analytics;

internal class AnalyticsContext : IAnalyticsContext
{
    public AttributeBag ExecutionAttributes { get; } = new();
}

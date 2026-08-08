#nullable enable

namespace Reqnroll.Diagnostics.Analytics;

/// <summary>
/// Represents the analytics context for an execution of the Reqnroll runtime.
/// </summary>
public interface IAnalyticsContext
{
    /// <summary>
    /// Gets the attributes to be included in analytics events for the execution context.
    /// </summary>
    AttributeBag ExecutionAttributes { get; }
}

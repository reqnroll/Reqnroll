namespace Reqnroll.Configuration
{
    /// <summary>
    /// Defines the granularity of parallel test execution.
    /// </summary>
    public enum ParallelizationScope
    {
        /// <summary>
        /// Default. Features run in parallel; scenarios within a feature run sequentially.
        /// </summary>
        Feature,

        /// <summary>
        /// Scenarios run in parallel independently, regardless of feature grouping.
        /// BeforeFeature/AfterFeature hooks execute once per feature with reference counting.
        /// </summary>
        Scenario
    }
}

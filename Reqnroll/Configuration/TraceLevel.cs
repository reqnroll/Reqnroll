namespace Reqnroll.Configuration
{
    /// <summary>
    /// Controls the verbosity of trace output from the test tracer.
    /// The default is <see cref="Normal"/>, which preserves the existing behavior.
    /// </summary>
    public enum TraceLevel
    {
        /// <summary>No trace output at all.</summary>
        None = 0,
        /// <summary>Only errors, warnings, pending steps, and undefined steps.</summary>
        Minimal = 1,
        /// <summary>Standard step execution output (default, matches current behavior).</summary>
        Normal = 2,
        /// <summary>Includes step completion confirmations with method names and duration.</summary>
        Detailed = 3,
        /// <summary>All information including feature/scenario duration timings.</summary>
        Diagnostic = 4
    }
}

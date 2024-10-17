using Reqnroll.FeatureSourceGenerator.MSTest;
using Reqnroll.FeatureSourceGenerator.NUnit;
using Reqnroll.FeatureSourceGenerator.XUnit;

namespace Reqnroll.FeatureSourceGenerator;

/// <summary>
/// Provides instances of the set of built-in handlers.
/// </summary>
internal static class BuiltInTestFrameworkHandlers
{
    /// <summary>
    /// Gets the handler instance for NUnit.
    /// </summary>
    public static NUnitHandler NUnit { get; } = new();

    /// <summary>
    /// Gets the handler instance for MSTest.
    /// </summary>
    public static MSTestHandler MSTest { get; } = new();

    /// <summary>
    /// Gets the handler instance for xUnit.
    /// </summary>
    public static XUnitHandler XUnit { get; } = new();
}

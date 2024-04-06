namespace Reqnroll.FeatureSourceGenerator;

internal static class BuiltInTestFrameworkHandlers
{
    public static NUnitHandler NUnit { get; } = new();

    public static MSTestHandler MSTest { get; } = new();

    public static XUnitHandler XUnit { get; } = new();
}
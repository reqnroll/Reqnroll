namespace Reqnroll.Analyzers;

internal static partial class DiagnosticResources
{
    internal static LocalizableResourceString CreateResourceString(string name) =>
        new(name, ResourceManager, typeof(DiagnosticResources));
}

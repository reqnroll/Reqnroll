using Microsoft.CodeAnalysis;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Reqnroll.CodeAnalysis.Gherkin;

internal static partial class DiagnosticDescriptors
{
    private static ResourceManager ResourceManager { get; } = new ResourceManager(typeof(DiagnosticDescriptors));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string? GetResourceString(string resourceKey) => ResourceManager.GetString(resourceKey);

    public static readonly DiagnosticDescriptor ErrorExpectedFeatureOrTag = new(
        id: DiagnosticIds.ErrorExpectedFeatureOrTag,
        title: GetResourceString("ErrorExpectedFeatureOrTagTitle")!,
        messageFormat: GetResourceString("ErrorExpectedFeatureOrTagMessage")!,
        "Reqnroll.Gherkin",
        DiagnosticSeverity.Error,
        true);
}

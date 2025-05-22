using Microsoft.CodeAnalysis;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Reqnroll.CodeAnalysis.Gherkin;

internal static class DiagnosticDescriptors
{
    private static readonly ResourceManager ResourceManager = new(typeof(DiagnosticDescriptors));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static LocalizableResourceString GetLocalizableResourceString(string resourceKey) =>
        new(resourceKey, ResourceManager, typeof(DiagnosticDescriptor));

    public static readonly DiagnosticDescriptor ErrorExpectedFeatureOrTag = new(
        id: DiagnosticIds.ErrorExpectedFeatureOrTag,
        title: GetLocalizableResourceString("ErrorExpectedFeatureOrTagTitle"),
        messageFormat: GetLocalizableResourceString("ErrorExpectedFeatureOrTagMessage"),
        "Reqnroll.Gherkin",
        DiagnosticSeverity.Error,
        true);
}

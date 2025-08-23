using Microsoft.CodeAnalysis;
using System.Resources;
using System.Runtime.CompilerServices;

#pragma warning disable RS2008 // Enable analyzer release tracking

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

    public static readonly DiagnosticDescriptor ErrorFeatureIsMissingName = new(
        id: DiagnosticIds.ErrorFeatureIsMissingName,
        title: GetLocalizableResourceString("ErrorFeatureIsMissingNameTitle"),
        messageFormat: GetLocalizableResourceString("ErrorFeatureIsMissingNameMessage"),
        "Reqnroll.Gherkin",
        DiagnosticSeverity.Error,
        true);
}

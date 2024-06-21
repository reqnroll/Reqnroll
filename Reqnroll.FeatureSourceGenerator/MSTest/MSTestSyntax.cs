using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator.MSTest;
internal static class MSTestSyntax
{
    public static readonly NamespaceString MSTestNamespace = new("Microsoft.VisualStudio.TestTools.UnitTesting");

    public static AttributeDescriptor TestMethodAttribute() => 
        new(new NamedTypeIdentifier(MSTestNamespace, new IdentifierString("TestMethod")));

    public static AttributeDescriptor DescriptionAttribute(string description) =>
        new(
            new NamedTypeIdentifier(MSTestNamespace, new IdentifierString("Description")),
            ImmutableArray.Create<object?>(description));

    public static AttributeDescriptor TestPropertyAttribute(string propertyName, object? propertyValue) =>
        new(
            new NamedTypeIdentifier(MSTestNamespace, new IdentifierString("TestProperty")),
            namedArguments: new Dictionary<string, object?> { { propertyName, propertyValue } }.ToImmutableDictionary());

    public static AttributeDescriptor DataRowAttribute(ImmutableArray<object?> values) =>
        new(new NamedTypeIdentifier(MSTestNamespace, new IdentifierString("DataRow")), values);
}

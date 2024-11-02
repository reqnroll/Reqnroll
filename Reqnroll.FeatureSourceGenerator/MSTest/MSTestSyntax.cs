using System.Collections.Immutable;
using Reqnroll.FeatureSourceGenerator.SourceModel;

namespace Reqnroll.FeatureSourceGenerator.MSTest;
internal static class MSTestSyntax
{
    public static readonly NamespaceString MSTestNamespace = new("Microsoft.VisualStudio.TestTools.UnitTesting");
    public static AttributeDescriptor TestClassAttribute() =>
        new(MSTestNamespace + new SimpleTypeIdentifier(new IdentifierString("TestClass")));

    public static AttributeDescriptor TestMethodAttribute() => 
        new(MSTestNamespace + new SimpleTypeIdentifier(new IdentifierString("TestMethod")));

    public static AttributeDescriptor DescriptionAttribute(string description) =>
        new(
            MSTestNamespace + new SimpleTypeIdentifier(new IdentifierString("Description")),
            ImmutableArray.Create<object?>(description));

    public static AttributeDescriptor TestPropertyAttribute(string propertyName, object? propertyValue) =>
        new(
            MSTestNamespace + new SimpleTypeIdentifier(new IdentifierString("TestProperty")),
            positionalArguments: ImmutableArray.Create(propertyName, propertyValue));

    public static AttributeDescriptor DataRowAttribute(ImmutableArray<object?> values) =>
        new(MSTestNamespace + new SimpleTypeIdentifier(new IdentifierString("DataRow")), values);
}

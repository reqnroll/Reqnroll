namespace Reqnroll.FeatureSourceGenerator.SourceModel;
internal static class CommonTypes
{
    public static readonly QualifiedTypeIdentifier String =
        new(new NamespaceString("System"), new SimpleTypeIdentifier(new IdentifierString("String")));
}

namespace Reqnroll.FeatureSourceGenerator.SourceModel;

public abstract class LocalTypeIdentifier(bool isNullable = false) : TypeIdentifier
{
    public override bool IsNullable { get; } = isNullable;
}

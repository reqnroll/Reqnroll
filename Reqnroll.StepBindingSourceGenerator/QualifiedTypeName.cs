namespace Reqnroll.StepBindingSourceGenerator;

internal record struct QualifiedTypeName(Namespace Namespace, string Name)
{
    public override readonly string ToString() => $"{Namespace}.{Name}";
}

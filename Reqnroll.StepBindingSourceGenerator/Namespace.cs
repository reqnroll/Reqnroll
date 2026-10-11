namespace Reqnroll.StepBindingSourceGenerator;

internal record class Namespace(string Name, Namespace? Parent)
{
    public override string ToString() => Parent == null ? Name : $"{Parent}.{Name}";
}

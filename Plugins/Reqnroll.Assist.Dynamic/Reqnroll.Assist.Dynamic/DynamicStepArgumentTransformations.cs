namespace Reqnroll.Assist.Dynamic;

[Binding]
public class DynamicStepArgumentTransformations
{
        
    [StepArgumentTransformation]
    public IEnumerable<object> TransformToEnumerable(Table table)
    {
        return table.CreateDynamicSet();
    }

    [StepArgumentTransformation]
    public IList<object> TransformToList(Table table)
    {
        return table.CreateDynamicSet().ToList<object>();
    }

    [StepArgumentTransformation]
    public dynamic TransformToDynamicInstance(Table table)
    {
        return table.CreateDynamicInstance();
    }
}
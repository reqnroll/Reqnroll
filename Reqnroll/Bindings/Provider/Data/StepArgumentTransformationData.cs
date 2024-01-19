#nullable disable
namespace Reqnroll.Bindings.Provider.Data;
public class StepArgumentTransformationData
{
    public BindingSourceData Source { get; set; }

    public string Name { get; set; }

    public string Regex { get; set; }

    public string[] ParamTypes { get; set; }
}

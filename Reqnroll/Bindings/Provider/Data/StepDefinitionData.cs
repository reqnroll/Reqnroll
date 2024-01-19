#nullable disable
namespace Reqnroll.Bindings.Provider.Data;

public class StepDefinitionData
{
    public BindingSourceData Source { get; set; }
    public string Type { get; set; }
    public string Expression { get; set; }
    public string Regex { get; set; }
    public string[] ParamTypes { get; set; }
    public BindingScopeData Scope { get; set; }

    public string Error { get; set; }
}

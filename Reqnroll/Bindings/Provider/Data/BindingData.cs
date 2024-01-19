#nullable disable
namespace Reqnroll.Bindings.Provider.Data;

public class BindingData
{
    public string[] Errors { get; set; }
    public string[] Warnings { get; set; }
    public StepDefinitionData[] StepDefinitions { get; set; }
    public HookData[] Hooks { get; set; }
    public StepArgumentTransformationData[] StepArgumentTransformations { get; set; }
}

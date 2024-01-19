#nullable disable
namespace Reqnroll.Bindings.Provider.Data;
public class HookData
{
    public BindingSourceData Source { get; set; }
    public BindingScopeData Scope { get; set; }
    public string Type { get; set; }
    public int HookOrder { get; set; }
}

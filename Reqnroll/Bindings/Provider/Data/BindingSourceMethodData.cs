#nullable disable
namespace Reqnroll.Bindings.Provider.Data;

public class BindingSourceMethodData
{
    public string Type { get; set; } // full type name (with namespace)
    public string Assembly { get; set; } // assembly name, null if the assembly is the main test assembly
    public string FullName { get; set; } // method name with signature: <return-type> <name>(<param-type1>,<param-type2>)
    public int? MetadataToken { get; set; } // MethodInfo.MetadataToken
}

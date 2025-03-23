using Reqnroll.Bindings;
using System.Reflection;

namespace Reqnroll.StepBindingSourceGenerator;

public static class StepAssemblyExtensions
{
    public static IStepDefinitionDescriptorsProvider GetStepRegistry(this Assembly assembly)
    {
        var registryType = assembly.ExportedTypes.Single(type => type.Name == "ReqnrollStepRegistry");

        return (IStepDefinitionDescriptorsProvider?)registryType.GetProperty("Instance")?.GetGetMethod()?.Invoke(null, null) ?? 
            throw new InvalidOperationException($"No registry exposed by assembly \"{assembly.FullName}\"");
    }
}

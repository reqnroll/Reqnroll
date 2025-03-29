using System;

namespace Reqnroll.Bindings;

[AttributeUsage(AttributeTargets.Assembly)]
public class StepDefinitionRegistryAttribute(Type registryType) : Attribute
{
    public Type RegistryType { get; } = registryType;
}

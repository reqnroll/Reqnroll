using System;

namespace Reqnroll.Bindings;

[AttributeUsage(AttributeTargets.Assembly)]
public class StepRegistryAttribute(Type registryType) : Attribute
{
    public Type RegistryType { get; } = registryType;
}

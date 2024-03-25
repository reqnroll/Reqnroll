using System;
using System.Collections.Generic;
using Reqnroll.BoDi;

namespace Reqnroll.Configuration;
public class DependencyConfigurationCollection : List<DependencyConfiguration>
{
    public void Add(string implementationType, string interfaceType, string name = null)
    {
        Add(new DependencyConfiguration
        {
            ImplementationType = implementationType,
            InterfaceType = interfaceType,
            Name = name
        });
    }

    public void RegisterTo(ObjectContainer container)
    {
        foreach (var registration in this)
        {
            registration.RegisterTo(container);
        }
    }
}

public class DependencyConfiguration
{
    public string ImplementationType { get; set; }
    public string InterfaceType { get; set; }
    public string Name { get; set; }

    public void RegisterTo(ObjectContainer container)
    {
        var interfaceType = Type.GetType(InterfaceType, true);
        var implementationType = Type.GetType(ImplementationType, true);
        container.RegisterTypeAs(implementationType, interfaceType, Name);
    }
}

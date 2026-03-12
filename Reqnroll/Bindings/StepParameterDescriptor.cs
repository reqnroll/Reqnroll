using System;

namespace Reqnroll.Bindings;

/// <summary>
/// Describes a parameter to a step.
/// </summary>
public class StepParameterDescriptor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StepParameterDescriptor"/> class.
    /// </summary>
    /// <param name="name">The name of the paramater.</param>
    /// <param name="parameterType">The type of the parameter.</param>
    public StepParameterDescriptor(string name, Type parameterType)
    {
        Name = name;
        ParameterType = parameterType;
    }

    /// <summary>
    /// Gets the name of the parameter.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the type of the parameter.
    /// </summary>
    public Type ParameterType { get; }
}

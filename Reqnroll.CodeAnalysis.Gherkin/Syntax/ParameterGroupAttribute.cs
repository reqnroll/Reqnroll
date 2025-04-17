namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Indicates that the property is part of a group of parameters that can be used together.
/// </summary>
/// <param name="groupName">The name of the property group. Other properties with this group name will be 
/// considered together.</param>
[AttributeUsage(AttributeTargets.Property)]
internal class ParameterGroupAttribute(string groupName) : Attribute
{
    public string GroupName { get; } = groupName;
}

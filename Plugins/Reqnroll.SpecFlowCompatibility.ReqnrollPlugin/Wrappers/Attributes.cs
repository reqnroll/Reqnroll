// ReSharper disable once CheckNamespace
namespace TechTalk.SpecFlow;

/// <summary>
/// Marker attribute that specifies that this class may contain bindings (step definitions, hooks, etc.)
/// </summary>
public class BindingAttribute : Reqnroll.BindingAttribute;

/// <summary>
/// Specifies a 'Given' step definition that matches for the provided regular expression.
/// </summary>
public class GivenAttribute : Reqnroll.GivenAttribute
{
    public GivenAttribute()
    {
    }

    public GivenAttribute(string expression) : base(expression)
    {
    }

    public GivenAttribute(string expression, string culture) : base(expression)
    {
    }
}

/// <summary>
/// Specifies a 'When' step definition that matches for the provided regular expression.
/// </summary>
public class WhenAttribute : Reqnroll.WhenAttribute
{
    public WhenAttribute()
    {
    }

    public WhenAttribute(string expression) : base(expression)
    {
    }

    public WhenAttribute(string expression, string culture) : base(expression)
    {
    }
}

/// <summary>
/// Specifies a 'Then' step definition that matches for the provided regular expression.
/// </summary>
public class ThenAttribute : Reqnroll.ThenAttribute
{
    public ThenAttribute()
    {
    }

    public ThenAttribute(string expression) : base(expression)
    {
    }

    public ThenAttribute(string expression, string culture) : base(expression)
    {
    }
}

/// <summary>
/// Specifies a step definition that matches for the provided regular expression and any step kinds (given, when, then).
/// </summary>
public class StepDefinitionAttribute : Reqnroll.StepDefinitionAttribute
{
    public StepDefinitionAttribute()
    {
    }

    public StepDefinitionAttribute(string expression) : base(expression)
    {
    }

    public StepDefinitionAttribute(string expression, string culture) : base(expression)
    {
    }
}

public class StepArgumentTransformation : Reqnroll.StepArgumentTransformationAttribute
{
    public StepArgumentTransformation(string regex) : base(regex)
    {
    }
    public StepArgumentTransformation()
    {
    }
}

public class ScopeAttribute : Reqnroll.ScopeAttribute
{
}
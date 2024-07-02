namespace Reqnroll.FeatureSourceGenerator.SourceModel;

/// <summary>
/// Specifies the type of a scenario step.
/// </summary>
public enum StepType
{
    /// <summary>
    /// The step sets up the context of the scenario. Associated with the "Given" keyword.
    /// </summary>
    Context,

    /// <summary>
    /// The step performs an action in the scenario. Associated with the "When" keyword.
    /// </summary>
    Action,

    /// <summary>
    /// The step performs an assertion on the result of the scenario. Associated with the "Then" keyword.
    /// </summary>
    Outcome,

    /// <summary>
    /// The step is a continuation of the previous step type. Associted with the "And" keyword.
    /// </summary>
    Conjunction
}

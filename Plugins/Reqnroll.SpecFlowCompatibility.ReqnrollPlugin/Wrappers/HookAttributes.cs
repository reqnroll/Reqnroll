// ReSharper disable once CheckNamespace
namespace TechTalk.SpecFlow;

public class BeforeTestRunAttribute : Reqnroll.BeforeTestRunAttribute;

public class AfterTestRunAttribute : Reqnroll.AfterTestRunAttribute;

public class BeforeFeatureAttribute(params string[] tags) : Reqnroll.BeforeFeatureAttribute(tags);

public class AfterFeatureAttribute(params string[] tags) : Reqnroll.AfterFeatureAttribute(tags);

/// <summary>
/// Specifies a hook to be executed before each scenario.
/// </summary>
public class BeforeScenarioAttribute(params string[] tags) : Reqnroll.BeforeScenarioAttribute(tags);

public class BeforeAttribute(params string[] tags) : Reqnroll.BeforeAttribute(tags);

/// <summary>
/// Specifies a hook to be executed after each scenario.
/// </summary>
public class AfterScenarioAttribute(params string[] tags) : Reqnroll.AfterScenarioAttribute(tags);

/// <summary>
/// Specifies a hook to be executed after each scenario. This attribute is a synonym to <see cref="AfterScenarioAttribute"/>.
/// </summary>
public class AfterAttribute(params string[] tags) : Reqnroll.AfterAttribute(tags);

public class BeforeScenarioBlockAttribute(params string[] tags) : Reqnroll.BeforeScenarioBlockAttribute(tags);

public class AfterScenarioBlockAttribute(params string[] tags) : Reqnroll.AfterScenarioBlockAttribute(tags);

public class BeforeStepAttribute(params string[] tags) : Reqnroll.BeforeStepAttribute(tags);

public class AfterStepAttribute(params string[] tags) : Reqnroll.AfterStepAttribute(tags);
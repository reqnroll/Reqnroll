using Gherkin.Ast;
using Microsoft.CodeAnalysis.CSharp;

namespace Reqnroll.FeatureSourceGenerator.CSharp;

/// <summary>
/// Represents a generation of a C# test fixture to allow the execution of a Gherkin feature.
/// </summary>
/// <remarks>
/// <para>This class provides the base for all Reqnroll's built-in test-framework implementations. It lays out the fundamental
/// common structure:</para>
/// <list type="bullet">
/// <item>A test class named after the feature title.</item>
/// <item>Every scenario/example and outline mapped to a method which passes the steps to the Reqnroll runtime.</item>
/// </list>
/// </remarks>
public abstract class CSharpTestFixtureGeneration(FeatureInformation featureInfo)
{
    public FeatureInformation FeatureInformation { get; } = featureInfo;

    protected bool SupportsNullable { get; }

    private bool IsLineMappingEnabled { get; } = featureInfo.FeatureSyntax.FilePath != null;

    protected GherkinDocument Document { get; } = featureInfo.FeatureSyntax.GetRoot();

    protected CSharpSourceTextBuilder SourceBuilder { get; } = new();

    internal SourceText GetSourceText()
    {
        SourceBuilder.Reset();

        SourceBuilder.Append("namespace ").Append(FeatureInformation.FeatureNamespace).AppendLine();
        SourceBuilder.BeginBlock("{");

        AppendTestFixtureClass();

        SourceBuilder.EndBlock("}");

        return SourceBuilder.ToSourceText();
    }

    protected virtual string GetClassName() => CSharpSyntax.CreateIdentifier(Document.Feature.Name + Document.Feature.Keyword);

    protected virtual string? GetBaseType() => null;

    protected virtual IEnumerable<string> GetInterfaces() => [];

    protected virtual void AppendTestFixtureClass()
    {
        var attributes = GetTestFixtureAttributes();

        foreach (var attribute in attributes)
        {
            AppendAttribute(attribute);
        }

        var feature = Document.Feature;
        var className = GetClassName();
        var baseType = GetBaseType();
        var interfaces = GetInterfaces().ToList();

        SourceBuilder.Append("public class ").Append(className);
        
        if (baseType != null || interfaces.Count > 0)
        {
            var baseTypes = new List<string>();

            if (baseType != null)
            {
                baseTypes.Add(baseType);
            }

            baseTypes.AddRange(interfaces);

            SourceBuilder.Append(" : ");

            var first = true;

            foreach (var value in baseTypes)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    SourceBuilder.Append(", ");
                }

                SourceBuilder.Append(value);
            }
        }

        SourceBuilder.BeginBlock("{");

        AppendTestFixturePreamble();

        foreach (var child in feature.Children)
        {
            switch (child)
            {
                case Scenario scenario:
                    SourceBuilder.AppendLine();
                    AppendTestMethodForScenario(scenario);
                    break;
            }
        }

        SourceBuilder.EndBlock("}");
    }

    private void AppendAttribute(AttributeDescriptor attribute)
    {
        SourceBuilder.Append("[global::").Append(attribute.Namespace).Append(".").Append(attribute.TypeName);

        if (attribute.Arguments.Any() || attribute.PropertyValues.Any())
        {
            var first = true;

            SourceBuilder.Append("(");

            foreach (var argument in attribute.Arguments)
            {
                if (!first)
                {
                    SourceBuilder.Append(", ");
                }

                first = false;

                SourceBuilder.AppendConstant(argument);
            }

            foreach (var (propertyName, propertyValue) in attribute.PropertyValues)
            {
                if (!first)
                {
                    SourceBuilder.Append(", ");
                }

                first = false;

                SourceBuilder.Append(propertyName).Append(" = ").AppendConstant(propertyValue);
            }

            SourceBuilder.Append(")");
        }

        SourceBuilder.AppendLine("]");
    }

    protected virtual IEnumerable<AttributeDescriptor> GetTestFixtureAttributes() => [];

    protected virtual void AppendTestFixturePreamble()
    {
        SourceBuilder.AppendLine("// start: shared service method & consts, NO STATE!");
        SourceBuilder.AppendLine();

        if (IsLineMappingEnabled && FeatureInformation.FeatureSyntax.FilePath != null)
        {
            SourceBuilder.AppendDirective($"#line 1 \"{FeatureInformation.FeatureSyntax.FilePath}\"");
            SourceBuilder.AppendDirective("#line hidden");
            SourceBuilder.AppendLine();
        }

        var feature = FeatureInformation.FeatureSyntax.GetRoot().Feature;

        SourceBuilder
            .Append("private static readonly string[] featureTags = new string[] { ")
            .AppendConstantList(feature.Tags.Select(tag => tag.Name.TrimStart('@')))
            .AppendLine(" };")
            .AppendLine();

        SourceBuilder
            .Append("private static readonly global::Reqnroll.FeatureInfo FeatureInfo = new global::Reqnroll.FeatureInfo(")
            .Append("new global::System.Globalization.CultureInfo(\"").Append(feature.Language).Append("\"), ")
            .AppendConstant(Path.GetDirectoryName(FeatureInformation.FeatureSyntax.FilePath).Replace("\\", "\\\\")).Append(", ")
            .AppendConstant(feature.Name).Append(", ")
            .AppendLine("null, global::Reqnroll.ProgrammingLanguage.CSharp, featureTags);")
            .AppendLine();

        AppendScenarioInitializeMethod();

        SourceBuilder.AppendLine();
        SourceBuilder.AppendLine("// end: shared service method & consts, NO STATE!");
    }

    private void AppendScenarioInitializeMethod()
    {
        SourceBuilder.AppendLine(
            "public async global::System.Threading.Tasks.Task ScenarioInitialize(" +
            "global::Reqnroll.ITestRunner testRunner, " +
            "global::Reqnroll.ScenarioInfo scenarioInfo)");

        SourceBuilder.BeginBlock("{");

        AppendScenarioInitializeMethodBody();

        SourceBuilder.EndBlock("}");
    }

    protected virtual void AppendScenarioInitializeMethodBody()
    {
        SourceBuilder.AppendLine("testRunner.OnScenarioInitialize(scenarioInfo);");
    }

    protected virtual void AppendTestMethodForScenario(Scenario scenario)
    {
        var attributes = GetTestMethodAttributes(scenario);

        foreach (var attribute in attributes)
        {
            AppendAttribute(attribute);
        }

        SourceBuilder.Append("public async Task ").Append(CSharpSyntax.CreateIdentifier(scenario.Name)).AppendLine("()");
        SourceBuilder.BeginBlock("{");

        AppendTestMethodBodyForScenario(scenario);

        SourceBuilder.EndBlock("}");
    }

    protected virtual void AppendTestMethodBodyForScenario(Scenario scenario)
    {
        AppendTestRunnerLookupForScenario(scenario);
        SourceBuilder.AppendLine();

        AppendScenarioInfo(scenario);
        SourceBuilder.AppendLine();

        SourceBuilder.AppendLine("try");
        SourceBuilder.BeginBlock("{");

        if (IsLineMappingEnabled)
        {
            SourceBuilder.AppendDirective($"#line {scenario.Location.Line}");
        }

        SourceBuilder.AppendLine("await ScenarioInitialize(testRunner, scenarioInfo);");

        if (IsLineMappingEnabled)
        {
            SourceBuilder.AppendDirective("#line hidden");
        }

        SourceBuilder.AppendLine();

        SourceBuilder.AppendLine("if (global::Reqnroll.TagHelper.ContainsIgnoreTag(scenarioInfo.CombinedTags))");
        SourceBuilder.BeginBlock("{");
        SourceBuilder.AppendLine("testRunner.SkipScenario();");
        SourceBuilder.EndBlock("}");
        SourceBuilder.AppendLine("else");
        SourceBuilder.BeginBlock("{");
        SourceBuilder.AppendLine("await testRunner.OnScenarioStartAsync();");
        SourceBuilder.AppendLine();
        SourceBuilder.AppendLine("// start: invocation of scenario steps");

        foreach (var step in scenario.Steps)
        {
            AppendScenarioStepInvocation(step, scenario);
        }

        SourceBuilder.AppendLine("// end: invocation of scenario steps");
        SourceBuilder.EndBlock("}");
        SourceBuilder.AppendLine();
        SourceBuilder.AppendLine("// finishing the scenario");
        SourceBuilder.AppendLine("await testRunner.CollectScenarioErrorsAsync();");

        SourceBuilder.EndBlock("}");
        SourceBuilder.AppendLine("finally");
        SourceBuilder.BeginBlock("{");
        SourceBuilder.AppendLine("await testRunner.OnScenarioEndAsync();");
        SourceBuilder.EndBlock("}");
    }

    protected virtual void AppendScenarioStepInvocation(Step step, Scenario scenario)
    {
        if (IsLineMappingEnabled)
        {
            SourceBuilder.AppendDirective($"#line {step.Location.Line}");
        }

        SourceBuilder
            .Append("await testRunner.")
            .Append(
                step.KeywordType switch
                {
                    global::Gherkin.StepKeywordType.Context => "Given",
                    global::Gherkin.StepKeywordType.Action => "When",
                    global::Gherkin.StepKeywordType.Outcome => "Then",
                    global::Gherkin.StepKeywordType.Conjunction => "And",
                    _ => throw new NotSupportedException($"Steps of type \"{step.Keyword}\" are not supported.") // TODO: Add message from resx
                })
            .Append("Async(")
            .AppendConstant(step.Text)
            .Append(", null, null, ")
            .AppendConstant(step.Keyword)
            .AppendLine(");");

        if (IsLineMappingEnabled)
        {
            SourceBuilder.AppendDirective("#line hidden");
        }
    }

    protected virtual void AppendScenarioInfo(Scenario scenario)
    {
        SourceBuilder.AppendLine("// start: calculate ScenarioInfo");
        SourceBuilder
            .Append("string[] tagsOfScenario = new string[] { ")
            .AppendConstantList(scenario.Tags.Select(tag => tag.Name.TrimStart('@')))
            .AppendLine(" };");
        SourceBuilder.AppendLine(
            "var argumentsOfScenario = new global::System.Collections.Specialized.OrderedDictionary(); // needed for scenario outlines");

        // TODO: Add support for rules.
        SourceBuilder.AppendLine("var inheritedTags = featureTags; // will be more complex if there are rules");

        SourceBuilder
            .Append("var scenarioInfo = new global::Reqnroll.ScenarioInfo(")
            .AppendConstant(scenario.Name)
            .AppendLine(", null, tagsOfScenario, argumentsOfScenario, inheritedTags);");
        SourceBuilder.AppendLine("// end: calculate ScenarioInfo");
    }

    /// <summary>
    /// Appends the code to provide the test runner instance for the scenario execution.
    /// </summary>
    /// <param name="scenario">The scenario to append code for.</param>
    /// <remarks>
    /// <para>Implementations of this method <b>must</b> append code to achieve the following:</para>
    /// <list type="bullet">
    ///     <item>Declare a local variable named <c>testRunner</c> of a type which can be assigned to type <c>Reqnroll.TestRunner</c>.</item>
    ///     <item>Assign to the <c>testRunner</c> variable the instance to use as the test runner for the scenario.</item>
    /// </list>
    /// </remarks>
    protected virtual void AppendTestRunnerLookupForScenario(Scenario scenario)
    {
        SourceBuilder.AppendLine("// getting test runner");
        SourceBuilder.AppendLine("var testWorkerId = global::System.Threading.Thread.CurrentThread.ManagedThreadId.ToString();");
        SourceBuilder.AppendLine("var testRunner = global::Reqnroll.TestRunnerManager.GetTestRunnerForAssembly(null, testWorkerId);");
    }

    protected virtual IEnumerable<AttributeDescriptor> GetTestMethodAttributes(Scenario scenario) => [];
}

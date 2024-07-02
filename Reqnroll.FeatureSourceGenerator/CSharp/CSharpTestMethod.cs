using Reqnroll.FeatureSourceGenerator.SourceModel;
using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator.CSharp;

public class CSharpTestMethod : TestMethod, IEquatable<CSharpTestMethod?>
{
    public CSharpTestMethod(
    IdentifierString identifier,
    ScenarioInformation scenario,
    ImmutableArray<StepInvocation> stepInvocations,
    ImmutableArray<AttributeDescriptor> attributes = default,
    ImmutableArray<ParameterDescriptor> parameters = default,
    ImmutableArray<KeyValuePair<string, IdentifierString>> scenarioParameters = default) 
        : base(identifier, scenario, stepInvocations, attributes, parameters, scenarioParameters)
    {
    }

    public CSharpTestMethod(TestMethodDescriptor descriptor) : base(descriptor)
    {
    }

    public override bool Equals(object obj) => Equals(obj as CSharpTestMethod);

    public bool Equals(CSharpTestMethod? other) => base.Equals(other);

    public override int GetHashCode() => base.GetHashCode();

    public void RenderTo(
        CSharpSourceTextBuilder sourceBuilder,
        CSharpRenderingOptions renderingOptions,
        CancellationToken cancellationToken = default)
    {
        if (!Attributes.IsEmpty)
        {
            foreach (var attribute in Attributes)
            {
                cancellationToken.ThrowIfCancellationRequested();
                sourceBuilder.AppendAttributeBlock(attribute);
                sourceBuilder.AppendLine();
            }
        }

        // Our test methods are always asynchronous and never return a value.
        sourceBuilder.Append("public async Task ").Append(Identifier);

        if (!Parameters.IsEmpty)
        {
            sourceBuilder.BeginBlock("(");

            var first = true;
            foreach (var parameter in Parameters)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!first)
                {
                    sourceBuilder.AppendLine(",");
                }

                sourceBuilder
                    .AppendTypeReference(parameter.Type)
                    .Append(' ')
                    .Append(parameter.Name);

                first = false;
            }

            sourceBuilder.EndBlock(")");
        }
        else
        {
            sourceBuilder.AppendLine("()");
        }

        sourceBuilder.BeginBlock("{");

        RenderMethodBodyTo(sourceBuilder, renderingOptions, cancellationToken);

        sourceBuilder.EndBlock("}");
    }

    protected virtual void RenderMethodBodyTo(
        CSharpSourceTextBuilder sourceBuilder,
        CSharpRenderingOptions renderingOptions,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        RenderTestRunnerLookupTo(sourceBuilder, renderingOptions, cancellationToken);
        sourceBuilder.AppendLine();

        RenderScenarioInfoTo(sourceBuilder, renderingOptions, cancellationToken);
        sourceBuilder.AppendLine();

        sourceBuilder.AppendLine("try");
        sourceBuilder.BeginBlock("{");

        if (renderingOptions.EnableLineMapping)
        {
            sourceBuilder.AppendDirective($"#line {Scenario.LineNumber}");
        }

        sourceBuilder.AppendLine("await ScenarioInitialize(testRunner, scenarioInfo);");

        if (renderingOptions.EnableLineMapping)
        {
            sourceBuilder.AppendDirective("#line hidden");
        }

        sourceBuilder.AppendLine();

        sourceBuilder.AppendLine("if (global::Reqnroll.TagHelper.ContainsIgnoreTag(scenarioInfo.CombinedTags))");
        sourceBuilder.BeginBlock("{");
        sourceBuilder.AppendLine("testRunner.SkipScenario();");
        sourceBuilder.EndBlock("}");
        sourceBuilder.AppendLine("else");
        sourceBuilder.BeginBlock("{");
        sourceBuilder.AppendLine("await testRunner.OnScenarioStartAsync();");
        sourceBuilder.AppendLine();
        sourceBuilder.AppendLine("// start: invocation of scenario steps");

        foreach (var invocation in StepInvocations)
        {
            cancellationToken.ThrowIfCancellationRequested();
            RenderScenarioStepInvocationTo(invocation, sourceBuilder, renderingOptions, cancellationToken);
        }

        sourceBuilder.AppendLine("// end: invocation of scenario steps");
        sourceBuilder.EndBlock("}");
        sourceBuilder.AppendLine();
        sourceBuilder.AppendLine("// finishing the scenario");
        sourceBuilder.AppendLine("await testRunner.CollectScenarioErrorsAsync();");

        sourceBuilder.EndBlock("}");
        sourceBuilder.AppendLine("finally");
        sourceBuilder.BeginBlock("{");
        sourceBuilder.AppendLine("await testRunner.OnScenarioEndAsync();");
        sourceBuilder.EndBlock("}");
    }

    protected virtual void RenderScenarioStepInvocationTo(
        StepInvocation invocation,
        CSharpSourceTextBuilder sourceBuilder,
        CSharpRenderingOptions renderingOptions,
        CancellationToken cancellationToken)
    {
        if (renderingOptions.EnableLineMapping)
        {
            sourceBuilder.AppendDirective($"#line {invocation.SourceLineNumber}");
        }

        sourceBuilder
            .Append("await testRunner.")
            .Append(
                invocation.Type switch
                {
                    StepType.Context => "Given",
                    StepType.Action => "When",
                    StepType.Outcome => "Then",
                    StepType.Conjunction => "And",
                    _ => throw new NotSupportedException()
                })
            .Append("Async(");

        if (invocation.Arguments.IsEmpty)
        {
            sourceBuilder.AppendLiteral(invocation.Text);
        }
        else
        {
            sourceBuilder.Append("string.Format(").AppendLiteral(invocation.Text);

            foreach (var argument in invocation.Arguments)
            {
                sourceBuilder.Append(", ").Append(argument);
            }

            sourceBuilder.Append(")");
        }

        sourceBuilder
            .Append(", null, null, ")
            .AppendLiteral(invocation.Keyword)
            .AppendLine(");");

        if (renderingOptions.EnableLineMapping)
        {
            sourceBuilder.AppendDirective("#line hidden");
        }
    }

    protected virtual void RenderScenarioInfoTo(
        CSharpSourceTextBuilder sourceBuilder,
        CSharpRenderingOptions renderingOptions,
        CancellationToken cancellationToken)
    {
        sourceBuilder.AppendLine("// start: calculate ScenarioInfo");
        sourceBuilder
            .Append("var tagsOfScenario = new string[] { ")
            .AppendLiteralList(Scenario.Tags)
            .Append(" }");

        // If a parameter has been defined for passing tags from the example, include it in the scenario's tags.
        var exampleTagsParameter = Parameters.FirstOrDefault(parameter => parameter.Name == CSharpSyntax.ExampleTagsParameterName);
        if (exampleTagsParameter != null)
        {
            sourceBuilder.Append(".Concat(").Append(exampleTagsParameter.Name).Append(").ToArray()");
        }
        sourceBuilder.AppendLine(";");

        sourceBuilder.AppendLine(
            "var argumentsOfScenario = new global::System.Collections.Specialized.OrderedDictionary(); // needed for scenario outlines");

        foreach (var (name, value) in ParametersOfScenario)
        {
            sourceBuilder
                .Append("argumentsOfScenario.Add(").AppendLiteral(name).Append(", ").Append(value).Append(");");
        }

        if (Scenario.Rule == null)
        {
            sourceBuilder.AppendLine("var inheritedTags = FeatureTags;");
        }
        else
        {
            sourceBuilder
                .Append("var ruleTags = new string[] { ")
                .AppendLiteralList(Scenario.Rule.Tags)
                .AppendLine(" };");

            sourceBuilder.AppendLine("var inheritedTags = FeatureTags.Concat(ruleTags).ToArray();");
        }

        sourceBuilder
            .Append("var scenarioInfo = new global::Reqnroll.ScenarioInfo(")
            .AppendLiteral(Scenario.Name)
            .AppendLine(", null, tagsOfScenario, argumentsOfScenario, inheritedTags);");
        sourceBuilder.AppendLine("// end: calculate ScenarioInfo");
    }

    /// <summary>
    /// Renders the code to provide the test runner instance for the test method.
    /// </summary>
    /// <param name="sourceBuilder">The source builder to append the code to.</param>
    /// <param name="renderingOptions">Options which control the rendering of the C# code.</param>
    /// <param name="cancellationToken">A token used to signal when rendering should be canceled.</param>
    /// <remarks>
    /// <para>Implementations of this method <b>must</b> append code to achieve the following:</para>
    /// <list type="bullet">
    ///     <item>Declare a local variable named <c>testRunner</c> of a type which can be assigned to type <c>Reqnroll.TestRunner</c>.</item>
    ///     <item>Assign to the <c>testRunner</c> variable the instance to use as the test runner for the scenario.</item>
    /// </list>
    /// </remarks>
    protected virtual void RenderTestRunnerLookupTo(
        CSharpSourceTextBuilder sourceBuilder,
        CSharpRenderingOptions renderingOptions,
        CancellationToken cancellationToken)
    {
        sourceBuilder.AppendLine("var testRunner = global::Reqnroll.TestRunnerManager.GetTestRunnerForAssembly();");
    }
}

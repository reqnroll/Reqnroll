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
        CSharpSourceTextWriter writer,
        CSharpRenderingOptions renderingOptions,
        CancellationToken cancellationToken = default)
    {
        if (!Attributes.IsEmpty)
        {
            foreach (var attribute in Attributes)
            {
                cancellationToken.ThrowIfCancellationRequested();
                writer.WriteAttributeBlock(attribute);
                writer.WriteLine();
            }
        }

        // Our test methods are always asynchronous and never return a value.
        writer.Write("public async Task ").Write(Identifier);

        if (!Parameters.IsEmpty)
        {
            writer.BeginBlock("(");

            var first = true;
            foreach (var parameter in Parameters)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!first)
                {
                    writer.WriteLine(",");
                }

                writer
                    .WriteTypeReference(parameter.Type)
                    .Write(' ')
                    .Write(parameter.Name);

                first = false;
            }

            writer.EndBlock(")");
        }
        else
        {
            writer.WriteLine("()");
        }

        writer.BeginBlock("{");

        RenderMethodBodyTo(writer, renderingOptions, cancellationToken);

        writer.EndBlock("}");
    }

    protected virtual void RenderMethodBodyTo(
        CSharpSourceTextWriter writer,
        CSharpRenderingOptions renderingOptions,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        RenderTestRunnerLookupTo(writer, renderingOptions, cancellationToken);
        writer.WriteLine();

        RenderScenarioInfoTo(writer, renderingOptions, cancellationToken);
        writer.WriteLine();

        writer.WriteLine("try");
        writer.BeginBlock("{");

        if (renderingOptions.EnableLineMapping)
        {
            writer.WriteLineDirective(
                Scenario.KeywordAndNamePosition.StartLinePosition,
                Scenario.KeywordAndNamePosition.EndLinePosition,
                writer.NewLineOffset,
                Scenario.KeywordAndNamePosition.Path);
        }

        writer.WriteLine("await ScenarioInitialize(testRunner, scenarioInfo);");

        if (renderingOptions.EnableLineMapping)
        {
            writer.WriteDirective("#line hidden");
        }

        writer.WriteLine();

        writer.WriteLine("if (global::Reqnroll.TagHelper.ContainsIgnoreTag(scenarioInfo.CombinedTags))");
        writer.BeginBlock("{");
        writer.WriteLine("testRunner.SkipScenario();");
        writer.EndBlock("}");
        writer.WriteLine("else");
        writer.BeginBlock("{");
        writer.WriteLine("await testRunner.OnScenarioStartAsync();");
        writer.WriteLine();
        writer.WriteLine("// start: invocation of scenario steps");

        for (var i = 0; i < StepInvocations.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var invocation = StepInvocations[i];
            RenderScenarioStepInvocationTo(invocation, i, writer, renderingOptions, cancellationToken);
        }

        writer.WriteLine("// end: invocation of scenario steps");
        writer.EndBlock("}");
        writer.WriteLine();
        writer.WriteLine("// finishing the scenario");
        writer.WriteLine("await testRunner.CollectScenarioErrorsAsync();");

        writer.EndBlock("}");
        writer.WriteLine("finally");
        writer.BeginBlock("{");
        writer.WriteLine("await testRunner.OnScenarioEndAsync();");
        writer.EndBlock("}");
    }

    protected virtual void RenderScenarioStepInvocationTo(
        StepInvocation invocation,
        int index,
        CSharpSourceTextWriter writer,
        CSharpRenderingOptions renderingOptions,
        CancellationToken cancellationToken)
    {
        var tableArgName = invocation.DataTableArgument is null ? "null" : $"step{index}DataTableArg";
        var docStringArgName = invocation.DocStringArgument is null ? "null" : $"step{index}DocStringArg";

        if (invocation.DataTableArgument is not null)
        {
            writer.WriteLine($"var {tableArgName} = new global::Reqnroll.DataTable(");
            writer.BeginBlock();
            var firstHeading = true;
            foreach (var heading in invocation.DataTableArgument.Headings)
            {
                if (firstHeading)
                {
                    firstHeading = false;
                }
                else
                {
                    writer.WriteLine(",");
                }

                writer.WriteLiteral(heading);
            }
            writer.WriteLine(");");
            writer.EndBlock();

            foreach (var row in invocation.DataTableArgument.Rows)
            {
                writer.Write(tableArgName).WriteLine(".AddRow(");
                writer.BeginBlock();
                var firstColumn = true;
                foreach (var cell in row)
                {
                    if (firstColumn)
                    {
                        firstColumn = false;
                    }
                    else
                    {
                        writer.WriteLine(",");
                    }

                    writer.WriteLiteral(cell);
                }
                writer.WriteLine(");");
                writer.EndBlock();
            }
        }

        if (invocation.DocStringArgument is not null)
        {
            writer.WriteLine($"var {tableArgName} = ");
            writer.BeginBlock();

            var firstLine = true;

            foreach (var line in invocation.DocStringArgument.Split())
            {
                if (firstLine)
                {
                    firstLine = false;
                }
                else
                {
                    writer.WriteLine(" +");
                }

                writer.Write('"');
                writer.WriteLiteral(line);
                writer.Write('"');
            }

            writer.WriteLine(";");

            writer.EndBlock();
        }

        var position = invocation.Position;

        if (position.Path != null && renderingOptions.EnableLineMapping)
        {
            writer.WriteLineDirective(
                position.StartLinePosition,
                position.EndLinePosition,
                writer.NewLineOffset,
                position.Path);
        }

        writer
            .Write("await testRunner.")
            .Write(
                invocation.Type switch
                {
                    StepType.Context => "Given",
                    StepType.Action => "When",
                    StepType.Outcome => "Then",
                    StepType.Conjunction => "And",
                    _ => throw new NotSupportedException()
                })
            .Write("Async(");

        if (invocation.Arguments.IsEmpty)
        {
            writer.WriteLiteral(invocation.Text);
        }
        else
        {
            writer.Write("string.Format(").WriteLiteral(invocation.Text);

            foreach (var argument in invocation.Arguments)
            {
                writer.Write(", ").Write(argument);
            }

            writer.Write(")");
        }

        writer
            .Write(", null, ")
            .Write(tableArgName)
            .Write(", ")
            .WriteLiteral(invocation.Keyword)
            .WriteLine(");");

        if (renderingOptions.EnableLineMapping)
        {
            writer.WriteDirective("#line hidden");
        }
    }

    protected virtual void RenderScenarioInfoTo(
        CSharpSourceTextWriter writer,
        CSharpRenderingOptions renderingOptions,
        CancellationToken cancellationToken)
    {
        writer.WriteLine("// start: calculate ScenarioInfo");
        writer
            .Write("var tagsOfScenario = new string[] { ")
            .WriteLiteralList(Scenario.Tags)
            .Write(" }");

        // If a parameter has been defined for passing tags from the example, include it in the scenario's tags.
        var exampleTagsParameter = Parameters.FirstOrDefault(parameter => parameter.Name == CSharpSyntax.ExampleTagsParameterName);
        if (exampleTagsParameter != null)
        {
            writer.Write(".Concat(").Write(exampleTagsParameter.Name).Write(").ToArray()");
        }
        writer.WriteLine(";");

        writer.WriteLine(
            "var argumentsOfScenario = new global::System.Collections.Specialized.OrderedDictionary(); // needed for scenario outlines");

        foreach (var (name, value) in ParametersOfScenario)
        {
            writer
                .Write("argumentsOfScenario.Add(").WriteLiteral(name).Write(", ").Write(value).Write(");");
        }

        if (Scenario.Rule == null)
        {
            writer.WriteLine("var inheritedTags = FeatureTags;");
        }
        else
        {
            writer
                .Write("var ruleTags = new string[] { ")
                .WriteLiteralList(Scenario.Rule.Tags)
                .WriteLine(" };");

            writer.WriteLine("var inheritedTags = FeatureTags.Concat(ruleTags).ToArray();");
        }

        writer
            .Write("var scenarioInfo = new global::Reqnroll.ScenarioInfo(")
            .WriteLiteral(Scenario.Name)
            .WriteLine(", null, tagsOfScenario, argumentsOfScenario, inheritedTags);");
        writer.WriteLine("// end: calculate ScenarioInfo");
    }

    /// <summary>
    /// Renders the code to provide the test runner instance for the test method.
    /// </summary>
    /// <param name="writer">The source builder to append the code to.</param>
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
        CSharpSourceTextWriter writer,
        CSharpRenderingOptions renderingOptions,
        CancellationToken cancellationToken)
    {
        writer.WriteLine("var testRunner = global::Reqnroll.TestRunnerManager.GetTestRunnerForAssembly();");
    }
}

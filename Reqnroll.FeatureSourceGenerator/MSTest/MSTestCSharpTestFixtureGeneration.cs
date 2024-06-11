using Gherkin.Ast;
using Reqnroll.FeatureSourceGenerator.CSharp;
using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator.MSTest;

internal class MSTestCSharpTestFixtureGeneration(FeatureInformation featureInfo) : CSharpTestFixtureGeneration(featureInfo)
{
    private static readonly NamespaceString MSTestNamespace = new("Microsoft.VisualStudio.TestTools.UnitTesting");

    protected override IEnumerable<AttributeDescriptor> GetTestFixtureAttributes()
    {
        return base.GetTestFixtureAttributes().Concat(
        [
            new AttributeDescriptor(new TypeIdentifier(MSTestNamespace, new IdentifierString("TestClass")))
        ]);
    }

    protected override void AppendTestFixturePreamble()
    {
        SourceBuilder.AppendLine("// start: MSTest Specific part");
        SourceBuilder.AppendLine();

        SourceBuilder.Append("public global::Microsoft.VisualStudio.TestTools.UnitTesting.TestContext");
        if (AreNullableReferenceTypesEnabled)
        {
            SourceBuilder.Append("?");
        }
        SourceBuilder.AppendLine(" TestContext { get; set; }");
        SourceBuilder.AppendLine();

        AppendClassInitializeMethod();
        SourceBuilder.AppendLine();

        AppendClassCleanupMethod();
        SourceBuilder.AppendLine();

        SourceBuilder.AppendLine("// end: MSTest Specific part");

        base.AppendTestFixturePreamble();
    }

    protected virtual void AppendClassInitializeMethod()
    {
        SourceBuilder.AppendLine("[global::Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitialize]");
        SourceBuilder.AppendLine("public static Task IntializeFeatureAsync(global::Microsoft.VisualStudio.TestTools.UnitTesting.TestContext testContext)");
        SourceBuilder.BeginBlock("{");
        SourceBuilder.AppendLine("var testWorkerId = global::System.Threading.Thread.CurrentThread.ManagedThreadId.ToString();");
        SourceBuilder.AppendLine("var testRunner = global::Reqnroll.TestRunnerManager.GetTestRunnerForAssembly(null, testWorkerId);");
        SourceBuilder.AppendLine("return testRunner.OnFeatureStartAsync(FeatureInfo);");
        SourceBuilder.EndBlock("}");
    }

    protected virtual void AppendClassCleanupMethod()
    {
        SourceBuilder.AppendLine("[global::Microsoft.VisualStudio.TestTools.UnitTesting.ClassCleanup]");
        SourceBuilder.AppendLine("public static Task TeardownFeatureAsync()");
        SourceBuilder.BeginBlock("{");
        SourceBuilder.AppendLine("var testWorkerId = global::System.Threading.Thread.CurrentThread.ManagedThreadId.ToString();");
        SourceBuilder.AppendLine("var testRunner = global::Reqnroll.TestRunnerManager.GetTestRunnerForAssembly(null, testWorkerId);");
        SourceBuilder.AppendLine("return testRunner.OnFeatureEndAsync();");
        SourceBuilder.EndBlock("}");
    }

    protected override IEnumerable<AttributeDescriptor> GetTestMethodAttributes(Scenario scenario)
    {
        var attributes = new List<AttributeDescriptor>
        {
            new(
                new TypeIdentifier(MSTestNamespace, new IdentifierString("TestMethod"))),
            new(
                new TypeIdentifier(MSTestNamespace, new IdentifierString("Description")),
                ImmutableArray.Create<object?>(scenario.Name)),
            new(
                new TypeIdentifier(MSTestNamespace, new IdentifierString("TestProperty")),
                ImmutableArray.Create<object?>("FeatureTitle", Document.Feature.Name))
        };

        foreach (var tag in Document.Feature.Tags.Concat(scenario.Tags))
        {
            attributes.Add(
                new AttributeDescriptor(
                    new TypeIdentifier(MSTestNamespace, new IdentifierString("TestCategory")),
                    ImmutableArray.Create<object?>(tag.Name.TrimStart('@'))));
        }

        foreach (var example in scenario.Examples)
        {
            var values = new object?[example.TableHeader.Cells.Count() + 1];

            // Add tags as the last argument in the values passed to the data-row.
            values[values.Length - 1] = example.Tags
                .Select(tag => tag.Name.TrimStart('@'))
                .ToImmutableArray();

            foreach (var row in example.TableBody)
            {
                var i = 0;
                foreach (var cell in row.Cells)
                {
                    values[i++] = cell.Value;
                }

                // DataRow's constructor is DataRow(object? data, params object?[] moreData)
                // Because we often pass an array of strings as a second argument, we always wrap moreData
                // in an explicit array to avoid the compiler mistaking our string array as the moreData value.
                var first = values.First();
                var others = values.Skip(1).ToImmutableArray();

                var positionalArguments = others.Length > 0 ?
                    ImmutableArray.Create(first, others) :
                    ImmutableArray.Create(first);

                attributes.Add(
                    new AttributeDescriptor(
                        new TypeIdentifier(MSTestNamespace, new IdentifierString("DataRow")),
                        positionalArguments));
            }
        }

        return base.GetTestMethodAttributes(scenario).Concat(attributes);
    }

    protected override void AppendScenarioInitializeMethodBody()
    {
        base.AppendScenarioInitializeMethodBody();

        SourceBuilder.AppendLine();
        SourceBuilder.AppendLine("// MsTest specific customization:");
        SourceBuilder.AppendLine("testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs(TestContext);");
    }
}

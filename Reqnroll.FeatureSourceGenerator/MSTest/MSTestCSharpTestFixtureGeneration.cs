using Gherkin.Ast;
using Reqnroll.FeatureSourceGenerator.CSharp;
using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator.MSTest;

internal class MSTestCSharpTestFixtureGeneration(FeatureInformation featureInfo) : CSharpTestFixtureGeneration(featureInfo)
{
    const string MSTestNamespace = "Microsoft.VisualStudio.TestTools.UnitTesting";

    protected override IEnumerable<AttributeDescriptor> GetTestFixtureAttributes()
    {
        return base.GetTestFixtureAttributes().Concat(
        [
            new AttributeDescriptor(
                "TestClass",
                MSTestNamespace,
                ImmutableArray<object?>.Empty,
                ImmutableArray<KeyValuePair<string, object?>>.Empty)
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
            new("TestMethod", MSTestNamespace),
            new("Description", MSTestNamespace, ImmutableArray.Create<object?>(scenario.Name)),
            new("TestProperty", MSTestNamespace, ImmutableArray.Create<object?>("FeatureTitle", Document.Feature.Name))
        };

        foreach (var tag in Document.Feature.Tags.Concat(scenario.Tags))
        {
            attributes.Add(
                new AttributeDescriptor(
                    "TestCategory",
                    MSTestNamespace,
                    ImmutableArray.Create<object?>(tag.Name.TrimStart('@'))));
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

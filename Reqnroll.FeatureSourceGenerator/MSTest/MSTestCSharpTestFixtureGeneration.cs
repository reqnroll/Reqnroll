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

        SourceBuilder.AppendLine("private global::Microsoft.VisualStudio.TestTools.UnitTesting.TestContext? _testContext;");
        SourceBuilder.AppendLine();

        SourceBuilder.AppendLine("public virtual global::Microsoft.VisualStudio.TestTools.UnitTesting.TestContext? TestContext");
        SourceBuilder.BeginBlock("{");

        SourceBuilder
            .AppendLine("get")
            .BeginBlock("{")
            .AppendLine("return this._testContext;")
            .EndBlock("}");

        SourceBuilder
            .AppendLine("set")
            .BeginBlock("{")
            .AppendLine("this._testContext = value;")
            .EndBlock("}");

        SourceBuilder.EndBlock("}");

        SourceBuilder.AppendLine();
        SourceBuilder.AppendLine("// end: MSTest Specific part");
        SourceBuilder.AppendLine();

        base.AppendTestFixturePreamble();
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
        SourceBuilder.AppendLine("testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs(_testContext);");
    }
}

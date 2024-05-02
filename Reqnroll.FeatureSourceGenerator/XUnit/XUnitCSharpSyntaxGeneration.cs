using Gherkin.Ast;
using Reqnroll.FeatureSourceGenerator.CSharp;

namespace Reqnroll.FeatureSourceGenerator.XUnit;
public class XUnitCSharpSyntaxGeneration(FeatureInformation featureInfo) : CSharpTestFixtureGeneration(featureInfo)
{
    const string XUnitNamespace = "Xunit";

    protected override IEnumerable<AttributeDescriptor> GetTestMethodAttributes(Scenario scenario)
    {
        var attributes = new List<AttributeDescriptor>
        {
            new("Fact", XUnitNamespace)
        };

        return base.GetTestMethodAttributes(scenario).Concat(attributes);
    }

    protected override IEnumerable<string> GetInterfaces() => 
        base.GetInterfaces().Concat([ $"global::Xunit.IClassFixture<{GetClassName()}.Lifetime>" ]);

    protected override void AppendTestFixturePreamble()
    {
        AppendLifetimeClass();

        AppendConstructor();

        base.AppendTestFixturePreamble();
    }

    protected virtual void AppendConstructor()
    {
        // Lifetime class is initialzed once per feature, then passed to the constructor of each test class instance.
        SourceBuilder.AppendLine($"public {GetClassName()}(Lifetime lifetime)");
        SourceBuilder.BeginBlock("{");
        SourceBuilder.AppendLine("Lifetime = lifetime;");
        SourceBuilder.EndBlock("}");
    }

    protected virtual void AppendLifetimeClass()
    {
        // This class represents the feature lifetime in the xUnit framework.
        SourceBuilder.AppendLine("public class Lifetime : global::Xunit.IAsyncLifetime");
        SourceBuilder.BeginBlock("{");

        SourceBuilder.AppendLine("public global::Reqnroll.TestRunner TestRunner { get; private set; }");

        SourceBuilder.AppendLine("public global::System.Threading.Tasks.Task InitializeAsync()");
        SourceBuilder.BeginBlock("{");
        // Our XUnit infrastructure uses a custom mechanism for identifying worker IDs.
        SourceBuilder.AppendLine("var testWorkerId = global::Reqnroll.xUnit.ReqnrollPlugin.XUnitParallelWorkerTracker.Instance.GetWorkerId();");
        SourceBuilder.AppendLine("TestRunner = global::Reqnroll.TestRunnerManager.GetTestRunnerForAssembly(null, testWorkerId);");
        SourceBuilder.AppendLine("return TestRunner.OnFeatureStartAsync(featureInfo);");
        SourceBuilder.EndBlock("}");

        SourceBuilder.AppendLine("public async global::System.Threading.Tasks.Task DisposeAsync()");
        SourceBuilder.BeginBlock("{");
        SourceBuilder.BeginBlock("var testWorkerId = testRunner.TestWorkerId;");
        SourceBuilder.BeginBlock("await testRunner.OnFeatureEndAsync();");
        SourceBuilder.BeginBlock("TestRunner = null;");
        SourceBuilder.BeginBlock("global::Reqnroll.xUnit.ReqnrollPlugin.XUnitParallelWorkerTracker.Instance.ReleaseWorker(testWorkerId);");
        SourceBuilder.EndBlock("}");

        SourceBuilder.EndBlock("}");
        SourceBuilder.AppendLine();
    }

    protected override void AppendTestRunnerLookupForScenario(Scenario scenario)
    {
        // For xUnit test runners are scoped to the whole feature execution lifetime
        SourceBuilder.AppendLine("var testRunner = Lifecycle.TestRunner;");
    }
}

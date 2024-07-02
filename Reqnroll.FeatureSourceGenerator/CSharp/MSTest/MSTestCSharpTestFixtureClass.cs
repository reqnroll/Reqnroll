using System.Collections.Immutable;
using Reqnroll.FeatureSourceGenerator.SourceModel;

namespace Reqnroll.FeatureSourceGenerator.CSharp.MSTest;
public class MSTestCSharpTestFixtureClass(
    NamedTypeIdentifier identifier,
    string hintName,
    FeatureInformation feature,
    ImmutableArray<AttributeDescriptor> attributes = default,
    ImmutableArray<CSharpTestMethod> methods = default,
    CSharpRenderingOptions? renderingOptions = null) : CSharpTestFixtureClass(identifier, hintName, feature, attributes, methods, renderingOptions)
{
    protected override void RenderTestFixtureContentTo(CSharpSourceTextBuilder sourceBuilder, CancellationToken cancellationToken)
    {
        RenderTestRunnerFieldTo(sourceBuilder);

        sourceBuilder.AppendLine();

        RenderTestContextPropertyTo(sourceBuilder);

        sourceBuilder.AppendLine();

        RenderClassInitializeMethodTo(sourceBuilder, cancellationToken);

        sourceBuilder.AppendLine();

        RenderClassCleanupMethodTo(sourceBuilder, cancellationToken);

        sourceBuilder.AppendLine();

        base.RenderTestFixtureContentTo(sourceBuilder, cancellationToken);
    }

    private void RenderTestRunnerFieldTo(CSharpSourceTextBuilder sourceBuilder)
    {
        sourceBuilder.Append("private static global::Reqnroll.ITestRunner");
        if (RenderingOptions.UseNullableReferenceTypes)
        {
            sourceBuilder.Append("?");
        }
        sourceBuilder.AppendLine(" TestRunner;");
    }

    private void RenderTestContextPropertyTo(CSharpSourceTextBuilder sourceBuilder)
    {
        sourceBuilder.Append("public global::Microsoft.VisualStudio.TestTools.UnitTesting.TestContext");
        if (RenderingOptions.UseNullableReferenceTypes)
        {
            sourceBuilder.Append("?");
        }
        sourceBuilder.AppendLine(" TestContext { get; set; }");
    }

    protected virtual void RenderClassInitializeMethodTo(CSharpSourceTextBuilder sourceBuilder, CancellationToken cancellationToken)
    {
        sourceBuilder
            .AppendLine("[global::Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitialize]")
            .AppendLine("public static Task IntializeFeatureAsync(global::Microsoft.VisualStudio.TestTools.UnitTesting.TestContext testContext)")
            .BeginBlock("{")
            .AppendLine("TestRunner = global::Reqnroll.TestRunnerManager.GetTestRunnerForAssembly();")
            .AppendLine("return TestRunner.OnFeatureStartAsync(FeatureInfo);")
            .EndBlock("}");
    }

    protected virtual void RenderClassCleanupMethodTo(
        CSharpSourceTextBuilder sourceBuilder,
        CancellationToken cancellationToken)
    {
        sourceBuilder
            .AppendLine("[global::Microsoft.VisualStudio.TestTools.UnitTesting.ClassCleanup(" +
                "Microsoft.VisualStudio.TestTools.UnitTesting.ClassCleanupBehavior.EndOfClass)]")
            .AppendLine("public static async Task TeardownFeatureAsync()")
            .BeginBlock("{")
            .Append("if (TestRunner == null)")
            .BeginBlock("{")
            .AppendLine("return;")
            .EndBlock("}")
            .AppendLine("await TestRunner.OnFeatureEndAsync();")
            .AppendLine("global::Reqnroll.TestRunnerManager.ReleaseTestRunner(TestRunner);")
            .AppendLine("TestRunner = null;")
            .EndBlock("}");
    }

    protected override void RenderScenarioInitializeMethodBodyTo(
        CSharpSourceTextBuilder sourceBuilder,
        CancellationToken cancellationToken)
    {
        base.RenderScenarioInitializeMethodBodyTo(sourceBuilder, cancellationToken);

        sourceBuilder.AppendLine("testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs(TestContext);");
    }
}

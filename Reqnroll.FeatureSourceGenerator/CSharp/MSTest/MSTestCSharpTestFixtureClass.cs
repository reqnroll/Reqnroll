using System.Collections.Immutable;
using Reqnroll.FeatureSourceGenerator.SourceModel;

namespace Reqnroll.FeatureSourceGenerator.CSharp.MSTest;
public class MSTestCSharpTestFixtureClass : CSharpTestFixtureClass
{
    public MSTestCSharpTestFixtureClass(
        QualifiedTypeIdentifier identifier,
        string hintName,
        FeatureInformation feature,
        ImmutableArray<AttributeDescriptor> attributes = default,
        ImmutableArray<MSTestCSharpTestMethod> methods = default,
        CSharpRenderingOptions? renderingOptions = null)
        : base(identifier, hintName, feature, attributes, methods.CastArray<CSharpTestMethod>(), renderingOptions)
    {
    }

    public MSTestCSharpTestFixtureClass(
        TestFixtureDescriptor descriptor,
        ImmutableArray<MSTestCSharpTestMethod> methods = default,
        CSharpRenderingOptions? renderingOptions = null) 
        : base(descriptor, methods.CastArray<CSharpTestMethod>(), renderingOptions)
    {
    }

    protected override void RenderTestFixtureContentTo(CSharpSourceTextWriter writer, CancellationToken cancellationToken)
    {
        RenderTestRunnerFieldTo(writer);

        writer.WriteLine();

        RenderTestContextPropertyTo(writer);

        writer.WriteLine();

        RenderClassInitializeMethodTo(writer, cancellationToken);

        writer.WriteLine();

        RenderClassCleanupMethodTo(writer, cancellationToken);

        writer.WriteLine();

        base.RenderTestFixtureContentTo(writer, cancellationToken);
    }

    private void RenderTestRunnerFieldTo(CSharpSourceTextWriter writer)
    {
        writer.Write("private static global::Reqnroll.ITestRunner");
        if (RenderingOptions.UseNullableReferenceTypes)
        {
            writer.Write("?");
        }
        writer.WriteLine(" TestRunner;");
    }

    private void RenderTestContextPropertyTo(CSharpSourceTextWriter writer)
    {
        writer.Write("public global::Microsoft.VisualStudio.TestTools.UnitTesting.TestContext");
        if (RenderingOptions.UseNullableReferenceTypes)
        {
            writer.Write("?");
        }
        writer.WriteLine(" TestContext { get; set; }");
    }

    protected virtual void RenderClassInitializeMethodTo(CSharpSourceTextWriter writer, CancellationToken cancellationToken)
    {
        writer
            .WriteLine("[global::Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitialize]")
            .WriteLine("public static Task InitializeFeatureAsync(global::Microsoft.VisualStudio.TestTools.UnitTesting.TestContext testContext)")
            .BeginBlock("{")
            .WriteLine("TestRunner = global::Reqnroll.TestRunnerManager.GetTestRunnerForAssembly();")
            .WriteLine("return TestRunner.OnFeatureStartAsync(FeatureInfo);")
            .EndBlock("}");
    }

    protected virtual void RenderClassCleanupMethodTo(
        CSharpSourceTextWriter writer,
        CancellationToken cancellationToken)
    {
        writer
            .WriteLine("[global::Microsoft.VisualStudio.TestTools.UnitTesting.ClassCleanup(" +
                "Microsoft.VisualStudio.TestTools.UnitTesting.ClassCleanupBehavior.EndOfClass)]")
            .WriteLine("public static async Task TeardownFeatureAsync()")
            .BeginBlock("{")
            .Write("if (TestRunner == null)")
            .BeginBlock("{")
            .WriteLine("return;")
            .EndBlock("}")
            .WriteLine("await TestRunner.OnFeatureEndAsync();")
            .WriteLine("global::Reqnroll.TestRunnerManager.ReleaseTestRunner(TestRunner);")
            .WriteLine("TestRunner = null;")
            .EndBlock("}");
    }

    protected override void RenderScenarioInitializeMethodBodyTo(
        CSharpSourceTextWriter writer,
        CancellationToken cancellationToken)
    {
        base.RenderScenarioInitializeMethodBodyTo(writer, cancellationToken);

        writer.WriteLine("testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs(TestContext);");
    }
}

using Reqnroll.FeatureSourceGenerator.SourceModel;
using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator.CSharp.NUnit;
public class NUnitCSharpTestFixtureClass : CSharpTestFixtureClass
{
    public NUnitCSharpTestFixtureClass(
        QualifiedTypeIdentifier identifier,
        string hintName,
        FeatureInformation feature,
        ImmutableArray<AttributeDescriptor> attributes = default,
        ImmutableArray<NUnitCSharpTestMethod> methods = default,
        CSharpRenderingOptions? renderingOptions = null)
        : base(identifier, hintName, feature, attributes, methods.CastArray<CSharpTestMethod>(), renderingOptions)
    {
    }

    public NUnitCSharpTestFixtureClass(
        TestFixtureDescriptor descriptor,
        ImmutableArray<NUnitCSharpTestMethod> methods = default,
        CSharpRenderingOptions? renderingOptions = null)
        : base(descriptor, methods.CastArray<CSharpTestMethod>(), renderingOptions)
    {
    }

    protected override void RenderTestFixtureContentTo(CSharpSourceTextWriter writer, CancellationToken cancellationToken)
    {
        writer.Write("private global::Reqnroll.ITestRunner");

        if (RenderingOptions.UseNullableReferenceTypes)
        {
            writer.Write('?');
        }
        
        writer.WriteLine(" _testRunner;");

        writer.WriteLine();

        RenderFeatureSetupMethodTo(writer);

        writer.WriteLine();

        RenderFeatureTearDownMethodTo(writer);

        writer.WriteLine();

        base.RenderTestFixtureContentTo(writer, cancellationToken);
    }

    private void RenderFeatureSetupMethodTo(CSharpSourceTextWriter writer)
    {
        writer
            .WriteLine("[global::NUnit.Framework.OneTimeSetUp]")
            .WriteLine("public virtual global::System.Threading.Tasks.Task FeatureSetupAsync()")
            .BeginBlock("{")
            .WriteLine("_testRunner = global::Reqnroll.TestRunnerManager.GetTestRunnerForAssembly();")
            .WriteLine("return _testRunner.OnFeatureStartAsync(FeatureInfo);")
            .EndBlock("}");
    }

    private void RenderFeatureTearDownMethodTo(CSharpSourceTextWriter writer)
    {
        writer
            .WriteLine("[global::NUnit.Framework.OneTimeTearDown]")
            .WriteLine("public async virtual global::System.Threading.Tasks.Task FeatureTearDownAsync()")
            .BeginBlock("{");

        writer.Write("await _testRunner");

        if (RenderingOptions.UseNullableReferenceTypes)
        {
            writer.Write('!');
        }

        writer.WriteLine(".OnFeatureEndAsync();");

        writer
            .WriteLine("global::Reqnroll.TestRunnerManager.ReleaseTestRunner(_testRunner);")
            .WriteLine("_testRunner = null;")
            .EndBlock("}");
    }

    protected override void RenderScenarioInitializeMethodBodyTo(CSharpSourceTextWriter writer, CancellationToken cancellationToken)
    {
        base.RenderScenarioInitializeMethodBodyTo(writer, cancellationToken);

        writer.WriteLine(
            "testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<global::NUnit.Framework.TestContext>(" +
            "global::NUnit.Framework.TestContext.CurrentContext);");
    }
}

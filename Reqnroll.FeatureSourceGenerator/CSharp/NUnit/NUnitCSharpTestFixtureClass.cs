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

    protected override void RenderTestFixtureContentTo(CSharpSourceTextBuilder sourceBuilder, CancellationToken cancellationToken)
    {
        sourceBuilder.Append("private global::Reqnroll.ITestRunner");

        if (RenderingOptions.UseNullableReferenceTypes)
        {
            sourceBuilder.Append('?');
        }
        
        sourceBuilder.AppendLine(" _testRunner;");

        sourceBuilder.AppendLine();

        RenderFeatureSetupMethodTo(sourceBuilder);

        sourceBuilder.AppendLine();

        RenderFeatureTearDownMethodTo(sourceBuilder);

        sourceBuilder.AppendLine();

        base.RenderTestFixtureContentTo(sourceBuilder, cancellationToken);
    }

    private void RenderFeatureSetupMethodTo(CSharpSourceTextBuilder sourceBuilder)
    {
        sourceBuilder
            .AppendLine("[global::NUnit.Framework.OneTimeSetUp]")
            .AppendLine("public virtual global::System.Threading.Tasks.Task FeatureSetupAsync()")
            .BeginBlock("{")
            .AppendLine("_testRunner = global::Reqnroll.TestRunnerManager.GetTestRunnerForAssembly();")
            .AppendLine("return _testRunner.OnFeatureStartAsync(FeatureInfo);")
            .EndBlock("}");
    }

    private void RenderFeatureTearDownMethodTo(CSharpSourceTextBuilder sourceBuilder)
    {
        sourceBuilder
            .AppendLine("[global::NUnit.Framework.OneTimeTearDown]")
            .AppendLine("public async virtual global::System.Threading.Tasks.Task FeatureTearDownAsync()")
            .BeginBlock("{");

        sourceBuilder.Append("await _testRunner");

        if (RenderingOptions.UseNullableReferenceTypes)
        {
            sourceBuilder.Append('!');
        }

        sourceBuilder.AppendLine(".OnFeatureEndAsync();");

        sourceBuilder
            .AppendLine("global::Reqnroll.TestRunnerManager.ReleaseTestRunner(_testRunner);")
            .AppendLine("_testRunner = null;")
            .EndBlock("}");
    }

    protected override void RenderScenarioInitializeMethodBodyTo(CSharpSourceTextBuilder sourceBuilder, CancellationToken cancellationToken)
    {
        base.RenderScenarioInitializeMethodBodyTo(sourceBuilder, cancellationToken);

        sourceBuilder.AppendLine(
            "testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<global::NUnit.Framework.TestContext>(" +
            "global::NUnit.Framework.TestContext.CurrentContext);");
    }
}

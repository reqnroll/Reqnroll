using Reqnroll.FeatureSourceGenerator.SourceModel;
using Reqnroll.FeatureSourceGenerator.XUnit;
using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator.CSharp.XUnit;
public class XUnitCSharpTestFixtureClass : CSharpTestFixtureClass
{
    public XUnitCSharpTestFixtureClass(
        QualifiedTypeIdentifier identifier,
        string hintName,
        FeatureInformation feature,
        ImmutableArray<AttributeDescriptor> attributes = default,
        ImmutableArray<CSharpTestMethod> methods = default,
        CSharpRenderingOptions? renderingOptions = null)
        : base(identifier, hintName, feature, attributes, methods, renderingOptions)
    {
        Interfaces = ImmutableArray.Create<TypeIdentifier>(
            XUnitSyntax.LifetimeInterfaceType(Identifier));
    }

    public XUnitCSharpTestFixtureClass(
        TestFixtureDescriptor descriptor,
        ImmutableArray<CSharpTestMethod> methods = default,
        CSharpRenderingOptions? renderingOptions = null)
        : base(descriptor, methods, renderingOptions)
    {
        Interfaces = ImmutableArray.Create<TypeIdentifier>(
            XUnitSyntax.LifetimeInterfaceType(Identifier));
    }

    public override ImmutableArray<TypeIdentifier> Interfaces { get; }

    protected override void RenderTestFixtureContentTo(
        CSharpSourceTextBuilder sourceBuilder,
        CancellationToken cancellationToken)
    {
        RenderLifetimeClassTo(sourceBuilder, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        sourceBuilder.AppendLine("private readonly FeatureLifetime _lifetime;");
        sourceBuilder.AppendLine();

        sourceBuilder.AppendLine("private readonly global::Xunit.Abstractions.ITestOutputHelper _testOutputHelper;");
        sourceBuilder.AppendLine();

        RenderConstructorTo(sourceBuilder, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        sourceBuilder.AppendLine();

        base.RenderTestFixtureContentTo(sourceBuilder, cancellationToken);
    }

    protected virtual void RenderConstructorTo(
        CSharpSourceTextBuilder SourceBuilder,
        CancellationToken cancellationToken)
    {
        var className = Identifier.LocalType switch
        {
            SimpleTypeIdentifier simple => simple.Name,
            GenericTypeIdentifier generic => generic.Name,
            _ => throw new NotImplementedException(
                $"Writing constructor for {Identifier.GetType().Name} values is not implemented.")
        };

        // Lifetime class is initialzed once per feature, then passed to the constructor of each test class instance.
        // Output helper is included to by registered in the container.
        SourceBuilder.Append("public ").Append(className).AppendLine("(FeatureLifetime lifetime, " +
            "global::Xunit.Abstractions.ITestOutputHelper testOutputHelper)");
        SourceBuilder.BeginBlock("{");
        SourceBuilder.AppendLine("_lifetime = lifetime;");
        SourceBuilder.AppendLine("_testOutputHelper = testOutputHelper;");
        SourceBuilder.EndBlock("}");
    }

    protected virtual void RenderLifetimeClassTo(CSharpSourceTextBuilder sourceBuilder, CancellationToken cancellationToken)
    {
        // This class represents the feature lifetime in the xUnit framework.
        sourceBuilder.AppendLine("public class FeatureLifetime : global::Xunit.IAsyncLifetime");
        sourceBuilder.BeginBlock("{");
        RenderLifetimeClassContentTo(sourceBuilder, cancellationToken);
        sourceBuilder.EndBlock("}");
        sourceBuilder.AppendLine();
    }

    private void RenderLifetimeClassContentTo(CSharpSourceTextBuilder sourceBuilder, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        sourceBuilder.Append("public global::Reqnroll.ITestRunner");

        if (RenderingOptions.UseNullableReferenceTypes)
        {
            sourceBuilder.Append('?');
        }

        sourceBuilder.AppendLine(" TestRunner { get; private set; }");

        sourceBuilder.AppendLine();

        sourceBuilder.AppendLine("public global::System.Threading.Tasks.Task InitializeAsync()");
        sourceBuilder.BeginBlock("{");
        sourceBuilder.AppendLine("TestRunner = global::Reqnroll.TestRunnerManager.GetTestRunnerForAssembly();");
        sourceBuilder.Append("return TestRunner.OnFeatureStartAsync(").AppendTypeReference(Identifier).Append(".FeatureInfo").AppendLine(");");
        sourceBuilder.EndBlock("}");

        sourceBuilder.AppendLine();

        sourceBuilder.AppendLine("public async global::System.Threading.Tasks.Task DisposeAsync()");
        sourceBuilder.BeginBlock("{");
        sourceBuilder.Append("await TestRunner");

        if (RenderingOptions.UseNullableReferenceTypes)
        {
            sourceBuilder.Append('!');
        }

        sourceBuilder.AppendLine(".OnFeatureEndAsync();");
        sourceBuilder.AppendLine("global::Reqnroll.TestRunnerManager.ReleaseTestRunner(TestRunner);");
        sourceBuilder.AppendLine("TestRunner = null;");
        sourceBuilder.EndBlock("}");
    }

    protected override void RenderScenarioInitializeMethodBodyTo(CSharpSourceTextBuilder sourceBuilder, CancellationToken cancellationToken)
    {
        base.RenderScenarioInitializeMethodBodyTo(sourceBuilder, cancellationToken);

        sourceBuilder.AppendLine(
            "testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs" +
            "<global::Xunit.Abstractions.ITestOutputHelper>(_testOutputHelper);");
    }
}

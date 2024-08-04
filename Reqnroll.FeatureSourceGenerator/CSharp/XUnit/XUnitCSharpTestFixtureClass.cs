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
        ImmutableArray<XUnitCSharpTestMethod> methods = default,
        CSharpRenderingOptions? renderingOptions = null)
        : base(identifier, hintName, feature, attributes, methods.CastArray<CSharpTestMethod>(), renderingOptions)
    {
        Interfaces = ImmutableArray.Create<TypeIdentifier>(
            XUnitSyntax.LifetimeInterfaceType(Identifier));
    }

    public XUnitCSharpTestFixtureClass(
        TestFixtureDescriptor descriptor,
        ImmutableArray<XUnitCSharpTestMethod> methods = default,
        CSharpRenderingOptions? renderingOptions = null)
        : base(descriptor, methods.CastArray<CSharpTestMethod>(), renderingOptions)
    {
        Interfaces = ImmutableArray.Create<TypeIdentifier>(
            XUnitSyntax.LifetimeInterfaceType(Identifier));
    }

    public override ImmutableArray<TypeIdentifier> Interfaces { get; }

    protected override void RenderTestFixtureContentTo(
        CSharpSourceTextWriter writer,
        CancellationToken cancellationToken)
    {
        RenderLifetimeClassTo(writer, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        writer.WriteLine("private readonly FeatureLifetime _lifetime;");
        writer.WriteLine();

        writer.WriteLine("private readonly global::Xunit.Abstractions.ITestOutputHelper _testOutputHelper;");
        writer.WriteLine();

        RenderConstructorTo(writer, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        writer.WriteLine();

        base.RenderTestFixtureContentTo(writer, cancellationToken);
    }

    protected virtual void RenderConstructorTo(
        CSharpSourceTextWriter SourceBuilder,
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
        SourceBuilder.Write("public ").Write(className).WriteLine("(FeatureLifetime lifetime, " +
            "global::Xunit.Abstractions.ITestOutputHelper testOutputHelper)");
        SourceBuilder.BeginBlock("{");
        SourceBuilder.WriteLine("_lifetime = lifetime;");
        SourceBuilder.WriteLine("_testOutputHelper = testOutputHelper;");
        SourceBuilder.EndBlock("}");
    }

    protected virtual void RenderLifetimeClassTo(CSharpSourceTextWriter writer, CancellationToken cancellationToken)
    {
        // This class represents the feature lifetime in the xUnit framework.
        writer.WriteLine("public class FeatureLifetime : global::Xunit.IAsyncLifetime");
        writer.BeginBlock("{");
        RenderLifetimeClassContentTo(writer, cancellationToken);
        writer.EndBlock("}");
        writer.WriteLine();
    }

    private void RenderLifetimeClassContentTo(CSharpSourceTextWriter writer, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        writer.Write("public global::Reqnroll.ITestRunner");

        if (RenderingOptions.UseNullableReferenceTypes)
        {
            writer.Write('?');
        }

        writer.WriteLine(" TestRunner { get; private set; }");

        writer.WriteLine();

        writer.WriteLine("public global::System.Threading.Tasks.Task InitializeAsync()");
        writer.BeginBlock("{");
        writer.WriteLine("TestRunner = global::Reqnroll.TestRunnerManager.GetTestRunnerForAssembly();");
        writer.Write("return TestRunner.OnFeatureStartAsync(").WriteTypeReference(Identifier).Write(".FeatureInfo").WriteLine(");");
        writer.EndBlock("}");

        writer.WriteLine();

        writer.WriteLine("public async global::System.Threading.Tasks.Task DisposeAsync()");
        writer.BeginBlock("{");
        writer.Write("await TestRunner");

        if (RenderingOptions.UseNullableReferenceTypes)
        {
            writer.Write('!');
        }

        writer.WriteLine(".OnFeatureEndAsync();");
        writer.WriteLine("global::Reqnroll.TestRunnerManager.ReleaseTestRunner(TestRunner);");
        writer.WriteLine("TestRunner = null;");
        writer.EndBlock("}");
    }

    protected override void RenderScenarioInitializeMethodBodyTo(CSharpSourceTextWriter writer, CancellationToken cancellationToken)
    {
        base.RenderScenarioInitializeMethodBodyTo(writer, cancellationToken);

        writer.WriteLine(
            "testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs" +
            "<global::Xunit.Abstractions.ITestOutputHelper>(_testOutputHelper);");
    }
}

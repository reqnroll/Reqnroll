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
        Interfaces = ImmutableArray.Create<TypeIdentifier>(XUnitSyntax.LifetimeInterfaceType(Identifier));
    }

    public XUnitCSharpTestFixtureClass(
        TestFixtureDescriptor descriptor,
        ImmutableArray<CSharpTestMethod> methods = default,
        CSharpRenderingOptions? renderingOptions = null)
        : base(descriptor, methods, renderingOptions)
    {
        Interfaces = ImmutableArray.Create<TypeIdentifier>(XUnitSyntax.LifetimeInterfaceType(Identifier));
    }

    public override ImmutableArray<TypeIdentifier> Interfaces { get; }

    protected override void RenderTestFixtureContentTo(
        CSharpSourceTextBuilder sourceBuilder,
        CancellationToken cancellationToken)
    {
        RenderLifetimeClassTo(sourceBuilder, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        RenderConstructorTo(sourceBuilder, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

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
        SourceBuilder.Append("public ").Append(className).AppendLine("(Lifetime lifetime)");
        SourceBuilder.BeginBlock("{");
        SourceBuilder.AppendLine("Lifetime = lifetime;");
        SourceBuilder.EndBlock("}");
    }

    protected virtual void RenderLifetimeClassTo(CSharpSourceTextBuilder sourceBuilder, CancellationToken cancellationToken)
    {
        // This class represents the feature lifetime in the xUnit framework.
        sourceBuilder.AppendLine("public class Lifetime : global::Xunit.IAsyncLifetime");
        sourceBuilder.BeginBlock("{");

        sourceBuilder.AppendLine("public global::Reqnroll.TestRunner TestRunner { get; private set; }");

        sourceBuilder.AppendLine("public global::System.Threading.Tasks.Task InitializeAsync()");
        sourceBuilder.BeginBlock("{");
        // Our XUnit infrastructure uses a custom mechanism for identifying worker IDs.
        sourceBuilder.AppendLine("var testWorkerId = global::Reqnroll.xUnit.ReqnrollPlugin.XUnitParallelWorkerTracker.Instance.GetWorkerId();");
        sourceBuilder.AppendLine("TestRunner = global::Reqnroll.TestRunnerManager.GetTestRunnerForAssembly(null, testWorkerId);");
        sourceBuilder.AppendLine("return TestRunner.OnFeatureStartAsync(featureInfo);");
        sourceBuilder.EndBlock("}");

        sourceBuilder.AppendLine("public async global::System.Threading.Tasks.Task DisposeAsync()");
        sourceBuilder.BeginBlock("{");
        sourceBuilder.BeginBlock("var testWorkerId = testRunner.TestWorkerId;");
        sourceBuilder.BeginBlock("await testRunner.OnFeatureEndAsync();");
        sourceBuilder.BeginBlock("TestRunner = null;");
        sourceBuilder.BeginBlock("global::Reqnroll.xUnit.ReqnrollPlugin.XUnitParallelWorkerTracker.Instance.ReleaseWorker(testWorkerId);");
        sourceBuilder.EndBlock("}");

        sourceBuilder.EndBlock("}");
        sourceBuilder.AppendLine();
    }
}

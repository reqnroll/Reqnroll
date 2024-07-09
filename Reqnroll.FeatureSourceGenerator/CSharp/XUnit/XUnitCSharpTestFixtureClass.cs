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

    //protected override IEnumerable<string> GetInterfaces() =>
    //    base.GetInterfaces().Concat([$"global::Xunit.IClassFixture<{GetClassName()}.Lifetime>"]);

    //protected override void AppendTestFixturePreamble()
    //{
    //    AppendLifetimeClass();

    //    AppendConstructor();

    //    base.AppendTestFixturePreamble();
    //}

    //protected virtual void AppendConstructor()
    //{
    //    // Lifetime class is initialzed once per feature, then passed to the constructor of each test class instance.
    //    SourceBuilder.AppendLine($"public {GetClassName()}(Lifetime lifetime)");
    //    SourceBuilder.BeginBlock("{");
    //    SourceBuilder.AppendLine("Lifetime = lifetime;");
    //    SourceBuilder.EndBlock("}");
    //}

    //protected virtual void AppendLifetimeClass()
    //{
    //    // This class represents the feature lifetime in the xUnit framework.
    //    SourceBuilder.AppendLine("public class Lifetime : global::Xunit.IAsyncLifetime");
    //    SourceBuilder.BeginBlock("{");

    //    SourceBuilder.AppendLine("public global::Reqnroll.TestRunner TestRunner { get; private set; }");

    //    SourceBuilder.AppendLine("public global::System.Threading.Tasks.Task InitializeAsync()");
    //    SourceBuilder.BeginBlock("{");
    //    // Our XUnit infrastructure uses a custom mechanism for identifying worker IDs.
    //    SourceBuilder.AppendLine("var testWorkerId = global::Reqnroll.xUnit.ReqnrollPlugin.XUnitParallelWorkerTracker.Instance.GetWorkerId();");
    //    SourceBuilder.AppendLine("TestRunner = global::Reqnroll.TestRunnerManager.GetTestRunnerForAssembly(null, testWorkerId);");
    //    SourceBuilder.AppendLine("return TestRunner.OnFeatureStartAsync(featureInfo);");
    //    SourceBuilder.EndBlock("}");

    //    SourceBuilder.AppendLine("public async global::System.Threading.Tasks.Task DisposeAsync()");
    //    SourceBuilder.BeginBlock("{");
    //    SourceBuilder.BeginBlock("var testWorkerId = testRunner.TestWorkerId;");
    //    SourceBuilder.BeginBlock("await testRunner.OnFeatureEndAsync();");
    //    SourceBuilder.BeginBlock("TestRunner = null;");
    //    SourceBuilder.BeginBlock("global::Reqnroll.xUnit.ReqnrollPlugin.XUnitParallelWorkerTracker.Instance.ReleaseWorker(testWorkerId);");
    //    SourceBuilder.EndBlock("}");

    //    SourceBuilder.EndBlock("}");
    //    SourceBuilder.AppendLine();
    //}
}

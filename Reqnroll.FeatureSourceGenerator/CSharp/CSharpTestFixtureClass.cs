using System.Collections.Immutable;
using System.Text;
using Reqnroll.FeatureSourceGenerator.SourceModel;

namespace Reqnroll.FeatureSourceGenerator.CSharp;

/// <summary>
/// Represents a class which is a test fixture to execute the scenarios associated with a feature.
/// </summary>
public class CSharpTestFixtureClass : TestFixtureClass, IEquatable<CSharpTestFixtureClass?>
{
    public CSharpTestFixtureClass(
        QualifiedTypeIdentifier identifier,
        string hintName,
        FeatureInformation feature,
        ImmutableArray<AttributeDescriptor> attributes = default,
        ImmutableArray<CSharpTestMethod> methods = default,
        CSharpRenderingOptions? renderingOptions = null) 
        : base(
            identifier,
            hintName,
            feature,
            attributes)
    {
        Methods = methods.IsDefault ? ImmutableArray<CSharpTestMethod>.Empty : methods;
        RenderingOptions = renderingOptions ?? new CSharpRenderingOptions();
    }

    public CSharpTestFixtureClass(
        TestFixtureDescriptor descriptor,
        ImmutableArray<CSharpTestMethod> methods = default,
        CSharpRenderingOptions? renderingOptions = null) : base(descriptor)
    {
        Methods = methods.IsDefault ? ImmutableArray<CSharpTestMethod>.Empty : methods;
        RenderingOptions = renderingOptions ?? new CSharpRenderingOptions();
    }

    private static readonly Encoding Encoding = new UTF8Encoding(false);

    public ImmutableArray<CSharpTestMethod> Methods { get; }

    public CSharpRenderingOptions RenderingOptions { get; }

    public virtual ImmutableArray<NamespaceString> NamespaceUsings { get; } = 
        ImmutableArray.Create(new NamespaceString("System.Linq"));

    public override IEnumerable<TestMethod> GetMethods() => Methods;

    public override SourceText Render(CancellationToken cancellationToken = default)
    {
        var buffer = new CSharpSourceTextWriter();

        RenderTo(buffer, cancellationToken);

        return SourceText.From(buffer.ToString(), Encoding);
    }

    public void RenderTo(CSharpSourceTextWriter writer, CancellationToken cancellationToken = default)
    {
        
        writer.Write("namespace ").Write(Identifier.Namespace).WriteLine();
        writer.BeginBlock("{");

        if (!NamespaceUsings.IsEmpty)
        {
            foreach (var import in NamespaceUsings)
            {
                writer.Write("using ").Write(import).WriteLine(";");
            }

            writer.WriteLine();
        }

        if (!Attributes.IsEmpty)
        {
            foreach (var attribute in Attributes)
            {
                cancellationToken.ThrowIfCancellationRequested();
                writer.WriteAttributeBlock(attribute);
                writer.WriteLine();
            }
        }

        if (RenderingOptions.EnableLineMapping && FeatureInformation.FilePath != null)
        {
            writer.WriteDirective("#line hidden");
            writer.WriteLine();
        }

        writer.Write("public partial class ").WriteTypeReference(Identifier.LocalType);

        if (!Interfaces.IsEmpty)
        {
            writer.Write(" :").WriteTypeReference(Interfaces[0]);

            for (var i = 1; i < Interfaces.Length; i++)
            {
                writer.Write(" ,").WriteTypeReference(Interfaces[i]);
            }
        }

        writer.WriteLine();

        writer.BeginBlock("{");

        RenderTestFixtureContentTo(writer, cancellationToken);

        writer.EndBlock("}");
        writer.EndBlock("}");
    }

    protected virtual void RenderTestFixtureContentTo(CSharpSourceTextWriter writer, CancellationToken cancellationToken)
    {
        RenderFeatureInformationPropertiesTo(writer);

        RenderScenarioInitializeMethodTo(writer, cancellationToken);

        writer.WriteLine();

        RenderMethodsTo(writer, cancellationToken);
    }

    private void RenderFeatureInformationPropertiesTo(CSharpSourceTextWriter writer)
    {
        writer
            .Write("private static readonly string[] FeatureTags = new string[] { ")
            .WriteLiteralList(FeatureInformation.Tags)
            .WriteLine(" };");

        writer.WriteLine();

        writer
            .WriteLine("private static readonly global::Reqnroll.FeatureInfo FeatureInfo = new global::Reqnroll.FeatureInfo(")
            .BeginBlock()
            .Write("new global::System.Globalization.CultureInfo(").WriteLiteral(FeatureInformation.Language).WriteLine("), ")
            .WriteLiteral(Path.GetDirectoryName(FeatureInformation.FilePath)).WriteLine(", ")
            .WriteLiteral(FeatureInformation.Name).WriteLine(", ")
            .WriteLiteral(FeatureInformation.Description).WriteLine(", ")
            .WriteLine("global::Reqnroll.ProgrammingLanguage.CSharp, ")
            .WriteLine("FeatureTags);")
            .EndBlock();
    }

    private void RenderScenarioInitializeMethodTo(
        CSharpSourceTextWriter writer,
        CancellationToken cancellationToken)
    {
        writer.WriteLine(
            "private global::System.Threading.Tasks.Task ScenarioInitialize(" +
            "global::Reqnroll.ITestRunner testRunner, " +
            "global::Reqnroll.ScenarioInfo scenarioInfo)");

        writer.BeginBlock("{");

        RenderScenarioInitializeMethodBodyTo(writer, cancellationToken);

        writer.WriteLine("return global::System.Threading.Tasks.Task.CompletedTask;");

        writer.EndBlock("}");
    }

    protected virtual void RenderScenarioInitializeMethodBodyTo(
        CSharpSourceTextWriter writer,
        CancellationToken cancellationToken)
    {
        writer.WriteLine("testRunner.OnScenarioInitialize(scenarioInfo);");
    }

    protected virtual void RenderMethodsTo(CSharpSourceTextWriter writer, CancellationToken cancellationToken)
    {
        var first = true;
        foreach (var method in Methods)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!first)
            {
                writer.WriteLine();
            }

            method.RenderTo(writer, RenderingOptions);

            if (first)
            {
                first = false;
            }
        }
    }

    public override bool Equals(object obj) => Equals(obj as CSharpTestFixtureClass);

    public override string ToString() => $"Identifier={Identifier}";

    public bool Equals(CSharpTestFixtureClass? other)
    {
        if (!base.Equals(other))
        {
            return false;
        }

        return base.Equals(other) &&
            Methods.SequenceEqual(other!.Methods) &&
            RenderingOptions.Equals(other.RenderingOptions);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = base.GetHashCode();

            hash *= 83155477 + Methods.GetSequenceHashCode();
            hash *= 83155477 + RenderingOptions.GetHashCode();

            return hash;
        }
    }
}

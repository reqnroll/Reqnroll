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
        var buffer = new CSharpSourceTextBuilder();

        RenderTo(buffer, cancellationToken);

        return SourceText.From(buffer.ToString(), Encoding);
    }

    public void RenderTo(CSharpSourceTextBuilder sourceBuilder, CancellationToken cancellationToken = default)
    {
        
        sourceBuilder.Append("namespace ").Append(Identifier.Namespace).AppendLine();
        sourceBuilder.BeginBlock("{");

        if (!NamespaceUsings.IsEmpty)
        {
            foreach (var import in NamespaceUsings)
            {
                sourceBuilder.Append("using ").Append(import).AppendLine(";");
            }

            sourceBuilder.AppendLine();
        }

        if (!Attributes.IsEmpty)
        {
            foreach (var attribute in Attributes)
            {
                cancellationToken.ThrowIfCancellationRequested();
                sourceBuilder.AppendAttributeBlock(attribute);
                sourceBuilder.AppendLine();
            }
        }

        sourceBuilder.Append("public partial class ").AppendTypeReference(Identifier.LocalType);

        if (!Interfaces.IsEmpty)
        {
            sourceBuilder.Append(" :").AppendTypeReference(Interfaces[0]);

            for (var i = 1; i < Interfaces.Length; i++)
            {
                sourceBuilder.Append(" ,").AppendTypeReference(Interfaces[i]);
            }
        }

        sourceBuilder.AppendLine();

        sourceBuilder.BeginBlock("{");

        if (RenderingOptions.EnableLineMapping && FeatureInformation.FilePath != null)
        {
            sourceBuilder.AppendDirective($"#line 1 \"{FeatureInformation.FilePath}\"");
            sourceBuilder.AppendDirective("#line hidden");
            sourceBuilder.AppendLine();
        }

        RenderTestFixtureContentTo(sourceBuilder, cancellationToken);

        sourceBuilder.EndBlock("}");
        sourceBuilder.EndBlock("}");
    }

    protected virtual void RenderTestFixtureContentTo(CSharpSourceTextBuilder sourceBuilder, CancellationToken cancellationToken)
    {
        RenderFeatureInformationPropertiesTo(sourceBuilder);

        RenderScenarioInitializeMethodTo(sourceBuilder, cancellationToken);

        sourceBuilder.AppendLine();

        RenderMethodsTo(sourceBuilder, cancellationToken);
    }

    private void RenderFeatureInformationPropertiesTo(CSharpSourceTextBuilder sourceBuilder)
    {
        sourceBuilder
            .Append("private static readonly string[] FeatureTags = new string[] { ")
            .AppendLiteralList(FeatureInformation.Tags)
            .AppendLine(" };");

        sourceBuilder.AppendLine();

        sourceBuilder
            .AppendLine("private static readonly global::Reqnroll.FeatureInfo FeatureInfo = new global::Reqnroll.FeatureInfo(")
            .BeginBlock()
            .Append("new global::System.Globalization.CultureInfo(").AppendLiteral(FeatureInformation.Language).AppendLine("), ")
            .AppendLiteral(Path.GetDirectoryName(FeatureInformation.FilePath)).AppendLine(", ")
            .AppendLiteral(FeatureInformation.Name).AppendLine(", ")
            .AppendLiteral(FeatureInformation.Description).AppendLine(", ")
            .AppendLine("global::Reqnroll.ProgrammingLanguage.CSharp, ")
            .AppendLine("FeatureTags);")
            .EndBlock();
    }

    private void RenderScenarioInitializeMethodTo(
        CSharpSourceTextBuilder sourceBuilder,
        CancellationToken cancellationToken)
    {
        sourceBuilder.AppendLine(
            "private global::System.Threading.Tasks.Task ScenarioInitialize(" +
            "global::Reqnroll.ITestRunner testRunner, " +
            "global::Reqnroll.ScenarioInfo scenarioInfo)");

        sourceBuilder.BeginBlock("{");

        RenderScenarioInitializeMethodBodyTo(sourceBuilder, cancellationToken);

        sourceBuilder.AppendLine("return global::System.Threading.Tasks.Task.CompletedTask;");

        sourceBuilder.EndBlock("}");
    }

    protected virtual void RenderScenarioInitializeMethodBodyTo(
        CSharpSourceTextBuilder sourceBuilder,
        CancellationToken cancellationToken)
    {
        sourceBuilder.AppendLine("testRunner.OnScenarioInitialize(scenarioInfo);");
    }

    protected virtual void RenderMethodsTo(CSharpSourceTextBuilder sourceBuilder, CancellationToken cancellationToken)
    {
        var first = true;
        foreach (var method in Methods)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!first)
            {
                sourceBuilder.AppendLine();
            }

            method.RenderTo(sourceBuilder, RenderingOptions);

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
            Methods.SequenceEqual(other.Methods) &&
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

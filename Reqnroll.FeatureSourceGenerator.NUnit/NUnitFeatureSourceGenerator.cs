using Gherkin.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator.NUnit;

[Generator(LanguageNames.CSharp)]
internal class NUnitFeatureSourceGenerator : IIncrementalGenerator
{
    private readonly GherkinParser _gherkinParser = new();

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Generate sources for all feature files.
        var featureFiles = context.AdditionalTextsProvider
            .Where(text => text.Path.EndsWith(".feature"));

        // Determine namespace information for generated classes.
        var featureFilesWithNamespace = featureFiles
            .Combine(context.AnalyzerConfigOptionsProvider)
            .Select((pair, ct) =>
            {
                var (file, options) = pair;

                var fileOptions = options.GetOptions(file);

                if (!fileOptions.TryGetValue("build_property.RootNamespace", out var ns))
                {
                    ns = "Checklight.Specification";
                }

                return (file, ns);
            });

        // Parse the feature files into Gherkin documents.
        var parsedFeatures = featureFilesWithNamespace
            .Select((tuple, ct) =>
            {
                var (file, ns) = tuple;

                GherkinSyntaxTree? syntaxTree = default;

                var source = file.GetText(ct);

                if (source != null)
                {
                    syntaxTree = _gherkinParser.Parse(source, file.Path, ct);
                }

                return (syntaxTree, ns);
            })
            .Where(tuple => tuple.syntaxTree != null)
            .Select((tuple, ct) => (tuple.syntaxTree!, tuple.ns));

        // Emit source files for each feature.
        context.RegisterImplementationSourceOutput(parsedFeatures, (context, tuple) =>
        {
            var (syntaxTree, ns) = tuple;

            var diagnostics = syntaxTree.GetDiagnostics().ToImmutableList();

            var hasDiagnostic = false;
            foreach (var diagnostic in syntaxTree.GetDiagnostics())
            {
                hasDiagnostic = true;
                context.ReportDiagnostic(diagnostic);
            }

            if (hasDiagnostic)
            {
                return;
            }

            var source = GenerateFeatureClass(syntaxTree!.GetRoot(), ns);
            if (source != null)
            {
                context.AddSource($"{Path.GetFileName(syntaxTree.FilePath)}.g.cs", source);
            }
        });
    }

    private SourceText GenerateFeatureClass(GherkinDocument document, string ns)
    {
        var feature = document.Feature;
        var builder = new CSharpSourceBuilder();

        builder.Append("namespace ").Append(ns).AppendLine();
        builder.BeginBlock("{");

        AppendFeatureClass(builder, feature);

        builder.EndBlock("}");

        return builder.ToSourceText();
    }

    private static void AppendFeatureClass(CSharpSourceBuilder builder, Feature feature)
    {
        builder.Append("public class ").Append(CSharpSyntax.CreateIdentifier(feature.Name)).AppendLine();

        builder.BeginBlock("{");
        
        foreach (var child in feature.Children)
        {
            switch (child)
            {
                case Scenario scenario:
                    AppendTestMethodForScenario(builder, scenario);
                    break;
            }
        }

        builder.EndBlock("}");
    }

    private static void AppendTestMethodForScenario(CSharpSourceBuilder builder, Scenario scenario)
    {
        var name = $"{scenario.Keyword}: {scenario.Name}";

        builder.AppendLine("[global::NUnit.Framework.Test]");
        builder.Append("[global::NUnit.Framework.Description(\"").Append(name).AppendLine("\")]");
        builder.Append("public void ").Append(CSharpSyntax.CreateIdentifier(name)).AppendLine("()");
        builder.BeginBlock("{");
        builder.EndBlock("}");
    }
}

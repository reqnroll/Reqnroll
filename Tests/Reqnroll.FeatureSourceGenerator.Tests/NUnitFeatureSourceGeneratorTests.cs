using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator.NUnit;

public class NUnitFeatureSourceGeneratorTests
{
    [Fact]
    public void EmptyFeatureGeneratesEmptyTestFixture()
    {
        var features = ImmutableArray.Create<AdditionalText>(
            new FeatureFile("Test.feature", "Feature: Test"));
            
        var generator = new CSharpFeatureSourceGenerator();

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        var references = assemblies
            .Where(assembly => !assembly.IsDynamic)
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location));

        var compilation = CSharpCompilation.Create("test");

        CSharpGeneratorDriver
            .Create(generator)
            .AddAdditionalTexts(features)
            .RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

        var cs8785 = diagnostics.FirstOrDefault(diag => diag.Id == "CS8785");

        if (cs8785 != null)
        {
            Assert.Fail(cs8785.GetMessage());
        }

        diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).Should().BeEmpty();

        output.SyntaxTrees.Should().ContainSingle().Which.ToString().Should().BeEquivalentTo(
@"namespace ReqnrollGenerated
{
    public class Test
    {
    }
}
");
    }
}
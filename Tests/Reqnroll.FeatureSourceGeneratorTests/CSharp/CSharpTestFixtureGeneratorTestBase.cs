using Microsoft.CodeAnalysis;
using Reqnroll.FeatureSourceGenerator.MSTest;
using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator.CSharp;
public abstract class CSharpTestFixtureGeneratorTestBase<THandler> where THandler : ITestFrameworkHandler
{
    public CSharpTestFixtureGeneratorTestBase(THandler handler)
    {
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(asm => !asm.IsDynamic)
            .Select(AssemblyIdentity.FromAssemblyDefinition);

        Compilation = new CSharpCompilationInformation(
            "Test.dll",
            references.ToImmutableArray(),
            Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp11,
        true);

        Generator = handler.GetTestFixtureGenerator<CSharpCompilationInformation>() ?? 
            throw new InvalidOperationException($"Handler for {handler.TestFrameworkName} does not support C# generation.");
    }

    protected CSharpCompilationInformation Compilation { get; }

    protected ITestFixtureGenerator<CSharpCompilationInformation> Generator { get; }
}

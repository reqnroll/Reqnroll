using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace Reqnroll.Analyzers;

public class ReqnrollCSharpAnalyzerTest<TAnalyzer> : CSharpAnalyzerTest<TAnalyzer, DefaultVerifier> 
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    public ReqnrollCSharpAnalyzerTest()
    {
        var reqnrollAssemblyLocation = typeof(WhenAttribute).Assembly.Location;
        var reqnrollAssembly = Path.Combine(
            Path.GetDirectoryName(reqnrollAssemblyLocation)!,
            Path.GetFileNameWithoutExtension(reqnrollAssemblyLocation));

        ReferenceAssemblies = ReferenceAssemblies.Net.Net80.AddAssemblies([reqnrollAssembly]);
    }
}

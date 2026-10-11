using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace Reqnroll.Analyzers;

public class ReqnrollCSharpCodeFixTest<TAnalyzer, TCodeFix> : CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFix : CodeFixProvider, new()
{
    public ReqnrollCSharpCodeFixTest()
    {
        var reqnrollAssemblyLocation = typeof(WhenAttribute).Assembly.Location;
        var reqnrollAssembly = Path.Combine(
            Path.GetDirectoryName(reqnrollAssemblyLocation)!,
            Path.GetFileNameWithoutExtension(reqnrollAssemblyLocation));

        ReferenceAssemblies = ReferenceAssemblies.Net.Net80.AddAssemblies([reqnrollAssembly]);
    }
}

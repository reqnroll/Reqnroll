﻿using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator.CSharp;

/// <summary>
/// A generator of Reqnroll test fixtures for the C# language.
/// </summary>
[Generator(LanguageNames.CSharp)]
public class CSharpTestFixtureSourceGenerator : TestFixtureSourceGenerator<CSharpCompilationInformation>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CSharpTestFixtureSourceGenerator"/> class.
    /// </summary>
    public CSharpTestFixtureSourceGenerator()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CSharpTestFixtureSourceGenerator"/> class specifying
    /// the test framework handlers to be used.
    /// </summary>
    /// <param name="handlers">The handlers to use.</param>
    internal CSharpTestFixtureSourceGenerator(params ITestFrameworkHandler[] handlers) : base(handlers.ToImmutableArray())
    {
    }

    protected override CSharpCompilationInformation GetCompilationInformation(Compilation compilation)
    {
        var cSharpCompilation = (CSharpCompilation)compilation;

        return new CSharpCompilationInformation(
            compilation.AssemblyName,
            compilation.ReferencedAssemblyNames.ToImmutableArray(),
            cSharpCompilation.LanguageVersion,
            cSharpCompilation.Options.NullableContextOptions != NullableContextOptions.Disable);
    }
}

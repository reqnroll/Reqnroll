using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator;

internal record GeneratorInformation<TCompilationInformation>(
    ImmutableArray<ITestFixtureGenerator<TCompilationInformation>> CompatibleGenerators,
    ImmutableArray<ITestFixtureGenerator<TCompilationInformation>> UseableGenerators,
    ITestFixtureGenerator<TCompilationInformation>? DefaultGenerator)
    where TCompilationInformation : CompilationInformation
{
    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 47;

            hash *= 13 + CompatibleGenerators.GetSetHashCode();
            hash *= 13 + UseableGenerators.GetSetHashCode();
            hash *= 13 + DefaultGenerator?.GetHashCode() ?? 0;

            return hash;
        }
    }
}

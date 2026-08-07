using Microsoft.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Reqnroll.StepBindingSourceGenerator.Assertions;

internal static class AssertionOptionsInitializer
{
    [ModuleInitializer]
    public static void SetAssertionOptions()
    {
        AssertionOptions.AssertEquivalencyUsing(
            options => options
                .ComparingByMembers<Diagnostic>()
                .ComparingByMembers<Location>());
    }
}

using System.Reflection;
using System.Runtime.Loader;

namespace Reqnroll.StepBindingSourceGenerator;

public sealed class AssemblyTestContext(AssemblyLoadContext context, Assembly assembly) : IDisposable
{
    public Assembly Assembly { get; } = assembly;

    public void Dispose() => context.Unload();
}

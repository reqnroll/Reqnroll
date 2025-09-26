using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

#nullable enable

namespace Reqnroll.Tools.MsBuild.Generation;

internal static class AsyncRunner
{

#if NETFRAMEWORK
    private static Func<object, Func<Task<T>>, T>? TryCreateRunDelegate<T>(object joinableTaskFactory)
    {
        // Find the Run<T> method
        var runMethod = joinableTaskFactory.GetType().GetMethods()
            .FirstOrDefault(m => m.Name == "Run" &&
                m.IsGenericMethod &&
                m.GetParameters().Length == 1 &&
                typeof(Func<Task<T>>).IsAssignableFrom(m.GetParameters()[0].ParameterType.GetGenericArguments()[0]));

        if (runMethod == null)
        {
            return null;
        }

        var genericRun = runMethod.MakeGenericMethod(typeof(T));

        return (Func<object, Func<Task<T>>, T>)
            genericRun.CreateDelegate(typeof(Func<object, Func<Task<T>>, T>));
    }

#endif

    /// <summary>
    /// Runs an asynchronous function and blocks until it completes, returning the result.
    /// </summary>
    /// <typeparam name="T">The type returned by the function.</typeparam>
    /// <param name="func">The function to invoke.</param>
    /// <returns>The value returned by the function.</returns>
    public static T RunAndJoin<T>(Func<Task<T>> func)
    {
#if NETFRAMEWORK
        // If we're running in Visual Studio, we want use its JoinableTaskFactory to avoid deadlocks.
        // We can't guarantee the version of Visual Studio, so we use reflection to try to find
        // ThreadHelper.JoinableTaskFactory at runtime
        var threadHelperType = Type.GetType(
            "Microsoft.VisualStudio.Shell.ThreadHelper, Microsoft.VisualStudio.Shell.15.0",
            throwOnError: false);

        if (threadHelperType != null)
        {
            var joinableTaskFactoryProperty = threadHelperType.GetProperty(
                "JoinableTaskFactory",
                BindingFlags.Public | BindingFlags.Static);

            var joinableTaskFactory = joinableTaskFactoryProperty?.GetValue(null);
            if (joinableTaskFactory != null)
            {
                var runDelegate = TryCreateRunDelegate<T>(joinableTaskFactory);
                if (runDelegate != null)
                {
                    return runDelegate(joinableTaskFactory, func);
                }
            }
        }

#endif
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
        return func().GetAwaiter().GetResult();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
    }
}

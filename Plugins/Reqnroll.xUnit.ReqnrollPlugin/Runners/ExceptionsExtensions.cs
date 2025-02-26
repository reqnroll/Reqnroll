// Contains Code from https://github.com/xunit/xunit/blob/v2/src/common/ExceptionExtensions.cs
// originally published under Apache 2.0 license
// Xunit v2 will be not developed further
using System;
using System.Reflection;

namespace Reqnroll.xUnit.ReqnrollPlugin.Runners;

static class ExceptionExtensions
{
    /// <summary>
    /// Unwraps an exception to remove any wrappers, like <see cref="TargetInvocationException"/>.
    /// </summary>
    /// <param name="ex">The exception to unwrap.</param>
    /// <returns>The unwrapped exception.</returns>
    public static Exception Unwrap(this Exception ex)
    {
        while (true)
        {
            var tiex = ex as TargetInvocationException;
            if (tiex == null) return ex;

            ex = tiex.InnerException;
        }
    }
}

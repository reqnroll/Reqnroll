using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Reqnroll.SystemTests.Portability;

public class SkippableTestMethodAttribute : TestMethodAttribute
{
    private readonly Type[] _exceptionsTypesToSkip;

    public SkippableTestMethodAttribute(params Type[] exceptions)
    {
        _exceptionsTypesToSkip = exceptions ?? throw new ArgumentNullException(nameof(exceptions));
    }

    public override TestResult[] Execute(ITestMethod testMethod)
    {
        var results = base.Execute(testMethod);
        foreach (var result in results)
        {
            if (result.Outcome != UnitTestOutcome.Failed)
                continue;
            var ex = result.TestFailureException;
            while (ex is not null)
            {
                if (_exceptionsTypesToSkip.Contains(ex.GetType()))
                {
                    result.Outcome = UnitTestOutcome.Inconclusive;
                    break;
                }
                ex = ex.InnerException;
            }
        }
        return results;
    }
}

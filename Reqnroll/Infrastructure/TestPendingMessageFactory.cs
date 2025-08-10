using System;
using System.Linq;
using Reqnroll.ErrorHandling;

namespace Reqnroll.Infrastructure;

public class TestPendingMessageFactory : ITestPendingMessageFactory
{
    public string BuildFromScenarioContext(ScenarioContext scenarioContext)
    {
        var pendingSteps = scenarioContext.PendingSteps.Distinct().OrderBy(s => s);
        return $"{PendingScenarioException.GenericErrorMessage}{Environment.NewLine}  {string.Join(Environment.NewLine + "  ", pendingSteps)}";
    }
}
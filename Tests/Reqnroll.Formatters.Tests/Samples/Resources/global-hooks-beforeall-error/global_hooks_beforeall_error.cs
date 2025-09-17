using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reqnroll;

namespace Reqnroll.Formatters.Tests.Samples.Resources.global_hooks_beforeall_error;

[Binding]
public class global_hooks_beforeall_error
{
    [BeforeTestRun]
    public static void BeforeTestRun1()
    {
    }

    [BeforeTestRun]
    public static void BeforeTestRun2()
    {
        throw new ApplicationException("BeforeAll hook went wrong");
    }

    [BeforeTestRun]
    public static void BeforeTestRun3()
    {
    }

    [When("a step passes")]
    public void AStepPasses()
    {
    }

    [AfterTestRun]
    public static void AfterTestRun1()
    {
    }

    [AfterTestRun]
    public static void AfterTestRun2()
    {
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reqnroll;

namespace Reqnroll.Formatters.Tests.Samples.Resources.global_hooks_afterall_error;

[Binding]
public class global_hooks_afterall_error
{
    [BeforeTestRun]
    public static void BeforeTestRun1()
    {

    }

    [BeforeTestRun]
    public static void BeforeTestRun2()
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
        throw new ApplicationException("AfterAll hook went wrong");
    }

    [AfterTestRun]
    public static void AfterTestRun3()
    {

    }
}

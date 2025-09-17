using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reqnroll;

namespace Reqnroll.Formatters.Tests.Samples.Resources.global_hooks;

[Binding]
public class global_hooks
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

    [When("a step fails")]
    public void AStepFails()
    {
        throw new ApplicationException("Exception in step");
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

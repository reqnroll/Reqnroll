using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reqnroll;

namespace Reqnroll.Formatters.Tests.Samples.Resources.global_hooks_attachments;

[Binding]
internal class global_hooks_attachments
{

    [BeforeTestRun]
    public static void BeforeAllHook(IReqnrollOutputHelper reqnrollOutputHelper)
    {
        reqnrollOutputHelper.WriteLine("Attachment from BeforeAll hook");
    }

    [When("a step passes")]
    public void WhenAStepPasses() { }

    [AfterTestRun]
    public static void AfterAllHook(IReqnrollOutputHelper reqnrollOutputHelper)
    {
        reqnrollOutputHelper.WriteLine("Attachment from AfterAll hook");
    }
}

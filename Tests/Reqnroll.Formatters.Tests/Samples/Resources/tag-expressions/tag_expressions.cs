using System;
using System.Collections.Generic;
using System.Text;
using Reqnroll;

namespace Reqnroll.Formatters.Tests.Samples.Resources.tag_expressions;

[Binding]
internal class tag_expressions
{
    [Given("something")]
    public void GivenSomething()
    {
        // No-op
    }

    [Given("something else")]
    public void GivenSomethingElse()
    {
        // No-op
    }

    [BeforeScenario("@tag2 and @tag1")]
    public void BeforeScenario_Tag1AndTag2(IReqnrollOutputHelper reqnrollOutputHelper)
    {
        reqnrollOutputHelper.WriteLine("BeforeScenario with @tag2 and @tag1 executed");
    }

    [BeforeScenario("@tag1 and not @tag3")]
    public void BeforeScenario_Tag1AndNotTag3(IReqnrollOutputHelper reqnrollOutputHelper)
    {
        reqnrollOutputHelper.WriteLine("BeforeScenario with @tag1 and not @tag3 executed");
    }
}

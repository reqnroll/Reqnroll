using Reqnroll;

namespace Reqnroll.Verify.ReqnrollPlugin.IntegrationTest.Steps;

[Binding]
internal class Steps
{
    [When("I try Verify with Reqnroll")]
    public async Task ITryVerifyWithReqnroll()
    {
        await Verifier.Verify("value");
    }

    [When(@"I try Verify with Reqnroll for Parameter '([^']*)'")]
    public async Task WhenITryVerifyWithReqnrollForParameter(string p0)
    {
        await Verifier.Verify("value");
    }

    [Then("it works")]
    public void ItWorks()
    {
    }
}

namespace Reqnroll.Verify.ReqnrollPlugin.IntegrationTest.StepDefinitions;

[Binding]
internal class StepDefinitions
{
    private readonly VerifySettings _settings;

    public StepDefinitions(VerifySettings settings)
    {
        _settings = settings;
    }

    [When("I try Verify with Reqnroll")]
    public async Task WhenITryVerifyWithReqnroll()
    {
        await Verifier.Verify("value", _settings);
    }

    [When(@"I try Verify with Reqnroll for Parameter '([^']*)'")]
    public async Task WhenITryVerifyWithReqnrollForParameter(string p0)
    {
        await Verifier.Verify("value", _settings);
    }

    [When(@"I try Verify with Reqnroll for Parameter '(.*)' and some additional parameter '(.*)'")]
    public async Task WhenITryVerifyWithReqnrollForParameterAndSomeAdditionalParameter(string p0, string p1)
    {
        await Verifier.Verify($"{p0} .. {p1}", _settings);
    }

    [When(@"I try Verify with Reqnroll with global registered path info")]
    public async Task WhenITryVerifyWithReqnrollWithGlobalRegisteredPathInfo()
    {
        await Verifier.Verify("value");
    }

    [Then("it works")]
    public void ThenItWorks()
    {
        // no-op
    }
}

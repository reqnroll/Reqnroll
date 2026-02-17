namespace Reqnroll.Verify.ReqnrollPlugin.IntegrationTest.StepDefinitions;

[Binding]
internal class StepDefinitions
{
    private readonly VerifySettings _settings;
    private VerifyResult? _verifyResult;

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

    private static readonly object Lock = new();
    private static bool _isFirstCallComplete;

    [When(@"I try Verify with Reqnroll in parallel")]
    public static void WhenITryVerifyWithReqnrollInParallel()
    {
        // lock so the first execution will always finish after the second execution

        if (!_isFirstCallComplete)
        {
            lock (Lock)
            {
                if (!_isFirstCallComplete)
                {
                    Thread.Sleep(500);
                    _isFirstCallComplete = true;
                }
            }
        }
    }

    [Then(@"it works in parallel with contents `(.*)`")]
    public async Task ThenItWorksInParallelWithVerifyContents(string contents)
    {
        _verifyResult = await Verifier.Verify(contents, _settings);
    }

    private void AssertEqual(string expected, string actual)
    {
        if (expected != actual)
        {
            throw new Exception($"Expected: {expected}, Actual: {actual}");
        }
    }

    [Then(@"the verified file is `(.*)` with contents `(.*)`")]
    public void ThenTheVerifiedFileIsWithContents(string fileName, string contents)
    {
        var actualFilePath = _verifyResult!.Files.First();
        var actualFileName = Path.GetFileName(actualFilePath);
        AssertEqual(fileName, actualFileName);

        var actualContents = File.ReadAllText(actualFilePath);
        AssertEqual(contents, actualContents);
    }

    // Demonstrate the workaround to allow using global VerifySettings instead of the settings provided via DI
    // This is a workaround for a feature we supported before plugin version v3.1
    [BeforeTestRun]
    public static void EnableGlobalVerifySettingsForCompatibility()
    {
        Verifier.DerivePathInfo(
            (_, projectDirectory, _, _) =>
            {
                var scenarioContext = Reqnroll.ScenarioContext.Current;
                var featureContext = Reqnroll.FeatureContext.Current;
                string scenarioInfoTitle = scenarioContext.ScenarioInfo.Title;

                foreach (System.Collections.DictionaryEntry scenarioInfoArgument in scenarioContext.ScenarioInfo.Arguments)
                {
                    scenarioInfoTitle += "_" + scenarioInfoArgument.Value;
                }

                return new PathInfo(
                    Path.Combine(projectDirectory, featureContext.FeatureInfo.FolderPath),
                    featureContext.FeatureInfo.Title,
                    scenarioInfoTitle);
            });
    }
}

using Reqnroll;
using Reqnroll.ScenarioCall.ReqnrollPlugin;

namespace Reqnroll.ScenarioCall.IntegrationTests.StepDefinitions
{
    [Binding]
    public class ScenarioSetup
    {
        [BeforeTestRun]
        public static void RegisterScenarios()
        {
            // Register the scenarios that can be called
            ScenarioRegistry.Register(
                scenarioName: "User logs in with valid credentials",
                featureName: "Authentication",
                ScenarioRegistry.Given("the user enters username \"testuser\""),
                ScenarioRegistry.And("the user enters password \"testpass\""),
                ScenarioRegistry.When("the user clicks login"),
                ScenarioRegistry.Then("the user should be logged in successfully")
            );

            ScenarioRegistry.Register(
                scenarioName: "User logs in with invalid credentials",
                featureName: "Authentication",
                ScenarioRegistry.Given("the user enters username \"baduser\""),
                ScenarioRegistry.And("the user enters password \"badpass\""),
                ScenarioRegistry.When("the user clicks login"),
                ScenarioRegistry.Then("the user should see an error message")
            );
        }
    }
}
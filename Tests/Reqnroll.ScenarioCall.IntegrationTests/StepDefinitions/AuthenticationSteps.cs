using Reqnroll;

namespace Reqnroll.ScenarioCall.IntegrationTests.StepDefinitions
{
    [Binding]
    public class AuthenticationSteps
    {
        private readonly ScenarioContext _scenarioContext;
        private string _username = "";
        private string _password = "";
        private bool _loginSuccessful = false;
        private bool _errorDisplayed = false;

        public AuthenticationSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        [Given(@"the user enters username ""([^""]*)""")]
        public void GivenTheUserEntersUsername(string username)
        {
            _username = username;
            _scenarioContext["Username"] = username;
        }

        [Given(@"the user enters password ""([^""]*)""")]
        public void GivenTheUserEntersPassword(string password)
        {
            _password = password;
            _scenarioContext["Password"] = password;
        }

        [When(@"the user clicks login")]
        public void WhenTheUserClicksLogin()
        {
            // Simulate login logic
            if (_username == "testuser" && _password == "testpass")
            {
                _loginSuccessful = true;
                _scenarioContext["LoginSuccessful"] = true;
            }
            else
            {
                _errorDisplayed = true;
                _scenarioContext["ErrorDisplayed"] = true;
            }
        }

        [Then(@"the user should be logged in successfully")]
        public void ThenTheUserShouldBeLoggedInSuccessfully()
        {
            if (!_loginSuccessful && !(_scenarioContext.ContainsKey("LoginSuccessful") && (bool)_scenarioContext["LoginSuccessful"]))
            {
                throw new Exception("User was not logged in successfully");
            }
        }

        [Then(@"the user should see an error message")]
        public void ThenTheUserShouldSeeAnErrorMessage()
        {
            if (!_errorDisplayed && !(_scenarioContext.ContainsKey("ErrorDisplayed") && (bool)_scenarioContext["ErrorDisplayed"]))
            {
                throw new Exception("Error message was not displayed");
            }
        }

        [Then(@"I should have executed both scenarios")]
        public void ThenIShouldHaveExecutedBothScenarios()
        {
            // Validate that both scenarios were called by checking the context
            bool hasLogin = _scenarioContext.ContainsKey("LoginSuccessful");
            bool hasError = _scenarioContext.ContainsKey("ErrorDisplayed");
            
            if (!hasLogin || !hasError)
            {
                throw new Exception("Both scenarios were not executed properly");
            }
        }
    }
}
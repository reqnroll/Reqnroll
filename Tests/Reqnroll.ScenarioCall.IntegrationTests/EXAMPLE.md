# Reqnroll ScenarioCall Plugin Example

This example demonstrates how to use the Reqnroll ScenarioCall plugin to call scenarios from other features.

## Test Results

✅ **All core functionality tests pass:**

- ✅ ScenarioRegistry can register and find scenarios
- ✅ ScenarioCallService throws appropriate errors when scenarios are not found  
- ✅ ScenarioCallService executes registered scenarios correctly

## Example Usage

### 1. Register Scenarios

```csharp
[Binding]
public class ScenarioSetup
{
    [BeforeTestRun]
    public static void RegisterScenarios()
    {
        // Register the login scenario
        ScenarioRegistry.Register(
            scenarioName: "User logs in with valid credentials",
            featureName: "Authentication",
            ScenarioRegistry.Given("the user enters username \"testuser\""),
            ScenarioRegistry.And("the user enters password \"testpass\""),
            ScenarioRegistry.When("the user clicks login"),
            ScenarioRegistry.Then("the user should be logged in successfully")
        );
    }
}
```

### 2. Call Scenarios from Feature Files

```gherkin
Feature: Order Processing
    Scenario: Process order for logged in user
        Given I call scenario "User logs in with valid credentials" from feature "Authentication"
        When I add an item to cart
        And I proceed to checkout
        Then the order should be processed successfully
```

### 3. Implement Step Definitions

```csharp
[Binding]
public class AuthenticationSteps
{
    private readonly ScenarioContext _scenarioContext;

    public AuthenticationSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [Given(@"the user enters username ""([^""]*)""")]
    public void GivenTheUserEntersUsername(string username)
    {
        _scenarioContext["Username"] = username;
    }

    [Given(@"the user enters password ""([^""]*)""")]
    public void GivenTheUserEntersPassword(string password)
    {
        _scenarioContext["Password"] = password;
    }

    [When(@"the user clicks login")]
    public void WhenTheUserClicksLogin()
    {
        // Simulate login logic
        var username = _scenarioContext["Username"] as string;
        var password = _scenarioContext["Password"] as string;
        
        if (username == "testuser" && password == "testpass")
        {
            _scenarioContext["LoginSuccessful"] = true;
        }
    }

    [Then(@"the user should be logged in successfully")]
    public void ThenTheUserShouldBeLoggedInSuccessfully()
    {
        Assert.True(_scenarioContext.ContainsKey("LoginSuccessful"));
    }
}
```

## Key Features

1. **Simple Registration**: Use `ScenarioRegistry.Register()` to register scenarios that can be called
2. **Helper Methods**: Use `ScenarioRegistry.Given()`, `When()`, `Then()`, etc. to create steps
3. **Natural Language**: Call scenarios using natural Gherkin syntax
4. **Context Preservation**: Called scenarios share the same ScenarioContext and FeatureContext
5. **Error Handling**: Clear error messages when scenarios are not found

## Plugin Architecture

The plugin consists of:

- **RuntimePlugin**: Registers the plugin with Reqnroll
- **ScenarioCallService**: Handles the execution of called scenarios
- **ScenarioRegistry**: Provides a simple API for registering scenarios
- **GlobalScenarioRegistry**: Internal registry for storing scenarios
- **ScenarioCallSteps**: Provides the step definitions for calling scenarios

This approach allows for maximum flexibility while maintaining simplicity and type safety.
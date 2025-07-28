# Reqnroll ScenarioCall Generator Plugin

This generator plugin allows calling scenarios from other features during code generation by expanding the scenario calls inline. This approach is much simpler and more reliable than runtime scenario calling.

## How it Works

Instead of runtime scenario calling, this plugin preprocesses feature files during code generation. When it encounters a step like:

```gherkin
Given I call scenario "User logs in with valid credentials" from feature "Authentication"
```

It will:
1. Find the referenced scenario in the specified feature file
2. Extract the steps from that scenario  
3. Replace the "call scenario" step with the actual steps from the referenced scenario
4. Generate the final .feature.cs file with all steps expanded inline

## Benefits

- **No runtime complexity**: All expansion happens at generation time
- **No context issues**: Steps execute as if they were written directly in the calling feature
- **Better performance**: No runtime lookup or execution overhead
- **Simpler debugging**: Generated code shows actual steps being executed
- **Leverages existing Reqnroll infrastructure**: Uses standard step execution
- **Automatic discovery**: No manual registration required

## Example

### Before (Original feature file):
```gherkin
Feature: Order Processing
    Scenario: Process order for logged in user
        Given I call scenario "User logs in with valid credentials" from feature "Authentication"
        When I add an item to cart
        Then the order should be processed successfully
```

### After (Generated/Expanded):
```gherkin  
Feature: Order Processing
    Scenario: Process order for logged in user
        # Expanded from scenario call: "User logs in with valid credentials" from feature "Authentication"
        Given there is a user registered with username "testuser" and password "testpass"
        And the user is on the login page
        When the user enters username "testuser"
        And the user enters password "testpass"
        And the user clicks login
        Then the user should be logged in successfully
        And the user should see the dashboard
        When I add an item to cart
        Then the order should be processed successfully
```

## Usage

1. Add the plugin to your `reqnroll.json`:
```json
{
  "plugins": [
    {
      "name": "Reqnroll.ScenarioCall.Generator"
    }
  ]
}
```

2. Use scenario calls in your feature files:
```gherkin
Feature: Order Processing
    Scenario: Process order for logged in user
        Given I call scenario "User logs in with valid credentials" from feature "Authentication"  
        When I add an item to cart
        Then the order should be processed successfully
```

The plugin will automatically find and expand the referenced scenarios during code generation.

## Supported Step Keywords

You can use scenario calls with any step keyword:
- `Given I call scenario "..." from feature "..."`
- `When I call scenario "..." from feature "..."`  
- `Then I call scenario "..." from feature "..."`
- `And I call scenario "..." from feature "..."`
- `But I call scenario "..." from feature "..."`

## Installation

The plugin is automatically discovered during the build process when installed as a NuGet package.
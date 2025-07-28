Feature: TestScenarioCalling

    Scenario: Call another scenario
        Given I call scenario "User logs in with valid credentials" from feature "TestAuthentication"
        Then the called scenario should have executed
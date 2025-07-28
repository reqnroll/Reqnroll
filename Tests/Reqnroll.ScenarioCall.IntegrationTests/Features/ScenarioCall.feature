Feature: ScenarioCall
    In order to reuse existing scenarios
    As a test developer
    I want to call scenarios from other features

Scenario: Test calling a valid login scenario
    Given I call scenario "User logs in with valid credentials" from feature "Authentication"
    Then the user should be logged in successfully

Scenario: Test calling multiple scenarios
    Given I call scenario "User logs in with valid credentials" from feature "Authentication"
    When I call scenario "User logs in with invalid credentials" from feature "Authentication"
    Then I should have executed both scenarios
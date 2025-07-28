Feature: Authentication
    In order to secure my application
    As a developer
    I want to validate user credentials

Scenario: User logs in with valid credentials
    Given the user enters username "testuser"
    And the user enters password "testpass"
    When the user clicks login
    Then the user should be logged in successfully

Scenario: User logs in with invalid credentials
    Given the user enters username "baduser"
    And the user enters password "badpass"
    When the user clicks login
    Then the user should see an error message
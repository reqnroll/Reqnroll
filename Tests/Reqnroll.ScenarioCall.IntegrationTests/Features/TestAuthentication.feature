Feature: TestAuthentication

    Scenario: User logs in with valid credentials
        Given there is a user registered with user name "Trillian" and password "139139"
        When the user attempts to log in with user name "Trillian" and password "139139"
        Then the login attempt should be successful
        And the user should be authenticated
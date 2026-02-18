Feature: All statuses

  This sample exercises all the possible result statuses, which is useful
  for testing formatters and other tools.

  Scenario: Passing
    Given a step
    And a step
    And a step

  Scenario: Failing
    Given a step
    And a failing step
    And a step

  Scenario: Pending
    Given a step
    And a pending step
    And a step

  Scenario: Skipped
    Given a step
    And a skipped step
    And a step

  Scenario: Undefined
    Given a step
    And an undefined step
    And a step

  Scenario: Ambiguous
    Given a step
    And an ambiguous step
    And a step
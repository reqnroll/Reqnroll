Feature: Global hooks - AfterAll error
  Errors in AfterAll hooks cause the whole test run to fail, and no subsequent AfterAll hooks are executed.

  Scenario: A passing scenario
    When a step passes

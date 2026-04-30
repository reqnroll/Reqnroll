Feature: Test run exceptions

  Sometimes an exception might happen during the test run, but outside of user-supplied hook
  or step code. This might be bad configuration, a bug in Cucumber, or an environmental issue.
  In such cases, Cucumber will abort the test run and include the exception in the final message.

  Scenario: exception during test run
    Given a step
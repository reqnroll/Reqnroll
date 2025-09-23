@retry(2)
Feature: Retry - With Undefined Steps
  Scenario: Test cases won't retry when the status is UNDEFINED
    Given a non-existent step

@retry(2)
Feature: Retry - With Pending Steps
  Scenario: Test cases won't retry when the status is PENDING
    Given a pending step

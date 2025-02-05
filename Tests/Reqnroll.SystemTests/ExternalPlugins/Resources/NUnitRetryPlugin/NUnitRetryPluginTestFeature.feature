Feature: NRetryPluginFeature

Used by Reqnroll.SystemTests.Plugins.NUnitRetryPluginTest

Scenario: Scenario with Retry
	When fail for first 2 times A

Scenario Outline: Scenario outline with Retry
	When fail for first 2 times <label>
Examples:
    | label |
    | B     |
    | C     |

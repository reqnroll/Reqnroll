Feature: XRetryPluginFeature

Used by Reqnroll.SystemTests.Plugins.XRetryPluginTest

@retry
Scenario: Scenario with Retry
	When fail for first 2 times A

@retry
Scenario Outline: Scenario outline with Retry
	When fail for first 2 times <label>
Examples:
    | label |
    | B     |
    | C     |

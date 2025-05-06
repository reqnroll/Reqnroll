Feature: ambiguous

This feature demonstrates Cucumber Messages emitted when a step results in an ambigous match with Step Definitions

NOTE: This feature is not present in the CCK, but added to round out the Reqnroll messages validation suite

Scenario: Ambiguous
	Given a step that matches more than one step binding
	Then this step gets skipped because of the prior ambiguous step

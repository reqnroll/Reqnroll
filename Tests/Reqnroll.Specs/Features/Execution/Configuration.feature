Feature: Configuration
	
Background: 
	Given there is a feature file in the project as
		"""
			Feature: Simple Feature
			Scenario: Simple Scenario
				Given there is something
				When I do something
				Then something should happen
		"""
	And all steps are bound and pass


Scenario: Generation configuration in reqnroll.json
	Given Reqnroll is configured in the reqnroll.json
	When the solution is built with log level normal
	And I execute the tests
	Then the reqnroll.json is used for configuration

Scenario: Runtime configuration in reqnroll.json
	Given Reqnroll is configured in the reqnroll.json
	When the solution is built with log level normal
	And I execute the tests
	Then the reqnroll.json is used for configuration


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


Scenario: Generation configuration in app.config
	Given Reqnroll is configured in the app.config
	When I execute the tests
	Then the app.config is used for configuration

Scenario: Generation configuration in reqnroll.json
	Given Reqnroll is configured in the reqnroll.json
	When I execute the tests
	Then the reqnroll.json is used for configuration	

Scenario: Runtime configuration in app.config
	Given Reqnroll is configured in the app.config
	When I execute the tests
	Then the app.config is used for configuration

Scenario: Runtime configuration in reqnroll.json
	Given Reqnroll is configured in the reqnroll.json
	When I execute the tests
	Then the reqnroll.json is used for configuration	


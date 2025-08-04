Feature: docString

A short summary of the feature

Scenario: The contents of a doc string will not be emitted as an argument match
	Given A step with a doc string
		"""
		This text will be an argument to a step definition.
		"""


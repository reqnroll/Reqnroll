Feature: Generator "all in" Sample 2

Sample feature file that contains various different Gherkin elements to test 
generation.

In this file we demonstrate
* Backgrounds
* Rules
* Rule Backgrounds

See also GeneratorSample1.feature

Background: 
	Given there is something

Scenario: Scenario outside of rules
	When something happens

@rule_tag1
Rule: Rule with single scenario
This description belongs to the rule

Scenario: Single scenario in a rule
	When something happens

Rule: Rule with multiple scenarios

Scenario: First scenario in a rule
	When something happens

Scenario: Second scenario in a rule
	When something happens

Rule: Rule with background

Background: Rule background
	Given there is something else

Scenario: Scenario in a rule with background
	When something happens

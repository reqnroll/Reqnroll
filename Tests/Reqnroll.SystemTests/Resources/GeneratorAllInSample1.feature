Feature: Generator "all in" Sample 1

Sample feature file that contains various different Gherkin elements to test 
generation.

In this file we demonstrate
* Scenarios
* Scenario Steps
* Scenario Step arguments: DataTable, DocString
* Scenario tags
* Feature & scenario descriptions
* Comments

See also GeneratorSample2.feature

# this is a comment
@tag1 @tag2
Scenario: Basic scenario with steps
	This text describes details about the purpose 
	of the scenario.

	Given there is something
	And there is something else
	When something happens
	Then something should have happened

@single_tag
Scenario: Scenario with DataTable
	When something happens with
		| who          | when     |
		| me           | today    |
		| someone else | tomorrow |

Scenario: Scenario with DocString
	When something happens as
		"""
		Something happens to 
		  - me
		  - and someone else
		"""

Scenario Outline: Scenario outline with different placeholders
	Given there is <what>
	And there is something else
	When <what> happens with
		| who          | when     |
		| me           | <when>   |
		| someone else | tomorrow |
	When something should have happened as
		"""
		Something happens to 
		  - me <when>
		  - and someone else tomorrow
		"""
Examples:
	| what                      | when     | not used          |
	| something                 | today    | this is not used  |
	| something else            | tomorrow | this is not used  |
	| special characters \| " ' | today    | should work still |

@tag1
Scenario Outline: Scenario outline with multiple examples blocks
	Given there is <what>
	And there is something else
	When <what> happens <when>
	When something should have happened

Examples:
	| what           | when     | not used         |
	| something      | today    | this is not used |
	| something else | tomorrow | this is not used |
@tag2
Examples:
	| what | when | not used |
	| foo  | bar  | baz      |
	| qux  | quux | corge    |
@tag3 @tag4
Examples: Example block name
	| what   | when   | not used |
	| grault | garply | waldo    |
	| fred   | plugh  | xyzzy    |

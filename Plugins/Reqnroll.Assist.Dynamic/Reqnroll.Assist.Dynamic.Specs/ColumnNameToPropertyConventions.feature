Feature: Column name converts by convention to nicely formatted property name
	As a developer
	I want my dynamic code to follow common conventions
	So that I know how to access them


Scenario: Single word in columns are left untouched
	Given I create a dynamic instance from this table
         | Name   | age |
         | Marcus | 39  |
	Then the Name property should equal 'Marcus'
		And the age property should equal 39

Scenario: Two word in the column headers is converted to camel cased properties
	When I create a dynamic instance from this table
		| Birth date | Length in meters |
		| 1972-10-09 | 1.96             |
	Then the BirthDate property should equal 1972-10-09
		And the LengthInMeters property should equal '1.96'

Scenario: Even if you go crazy with naming you columns we try to shape it up
	When I create a dynamic instance from this table
		| Birth dAtE | Length IN mETERs |
		| 1972-10-09 | 1.96             |
	Then the BirthDate property should equal 1972-10-09
		And the LengthInMeters property should equal '1.96'

Scenario: But the first word is always left untouched since it might be abbreviations
	When I create a dynamic instance from this table
		| Name                               | SAT score |
		| Have no idea what an SAT should be | 132       |
	Then the SATScore should be 132
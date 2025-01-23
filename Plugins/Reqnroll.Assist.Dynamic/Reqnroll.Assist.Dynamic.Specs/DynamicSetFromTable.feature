Feature: Create dynamic sets objects from SpecFlow table
	In order to write only code that matters
	As a SpecFlow developer
	I want SpecFlow to create set of dynamic objects from a table row

Scenario: Create set of dynamic objects
	When I create a set of dynamic instances from this table
		| Name   | Age | Birth date | Length in meters |
		| Marcus | 39  | 1972-10-09 | 1.96             |
		| Albert | 3   | 2008-01-24 | 1.03             |
		| Gustav | 1   | 2010-03-19 | 0.84             |
		| Arvid  | 1   | 2010-03-19 | 0.85             |
	Then I should have a list of 4 dynamic objects
		And the 1 item should have Name equal to 'Marcus'
		And the 2 item should have Age equal to '3'
		And the 3 item should have BirthDate equal to '2010-03-19'
		And the 4 item should have LengthInMeters equal to '0.85'
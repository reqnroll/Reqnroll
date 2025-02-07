Feature: Conversions of values
	In order to easier compare values of the most common types
	As a user of SpecFlow Dynamic
	I want SpecFlow Dynamic to translate strings into the closest ressembling real type


	Scenario: Strings should be translated to string
		When I create a dynamic instance from this table
			| Name   |
			| Marcus |
		Then the Name property should equal 'Marcus'

	Scenario: Integers should be translated from strings
		When I create a dynamic instance from this table
			| Age |
			| 39  |
		Then the Age property should equal 39

	Scenario: Doubles should be translated from strings
		When I create a dynamic instance from this table
			| Length in meters |
			| 1.96             |
		Then the LengthInMeters property should equal '1.96'

	Scenario: Decimals should be translated from strings
		When I create a dynamic instance from this table
			| Molecular Weight      |
			| 1000000000.1111991111 |
		Then the MolecularWeight property should equal '1000000000.1111991111'

	Scenario: Dates should be translated from strings
		When I create a dynamic instance from this table
			| Birth date |
			| 1972-10-09 |
		Then the BirthDate property should equal 1972-10-09

	Scenario: Bools should be translated from strings
		When I create a dynamic instance from this table
			| Is developer |
			| false        |
		Then the IsDeveloper property should equal 'false'

	Scenario: A strange double should not be translated into a date
		When I create a dynamic instance from this table
			| Length in meters |
			| 4.567            |
		Then the LengthInMeters property should equal '4.567'

	Scenario: There's ways to disable type conversion for instance creation
		When I create a dynamic instance from this table using no type conversion
			| Name   | Age | Birth date | Length in meters |
			| 012345 | 044 | 1972-13-09 | 1,96             |
		Then the Name value should still be '012345'
		And the Age value should still be '044'
		And the birth date should still be '1972-13-09'
		And length in meter should still be '1,96'

	Scenario: There's ways to disable type conversion for instance creation with key/value tables
		When I create a dynamic instance from this table using no type conversion
			| Key              | Value      |
			| Name             | 012345     |
			| Age              | 044        |
			| Birth date       | 1972-13-09 |
			| Length in meters | 1,96       |
		Then the Name value should still be '012345'
		And the Age value should still be '044'
		And the birth date should still be '1972-13-09'
		And length in meter should still be '1,96'

	Scenario: There's ways to disable type conversion for set creation
		When I create a set of dynamic instances from this table using no type conversion
			| Name   | Age |
			| 012345 | 044 |
			| Arvid  | 1   |
		Then I should have a list of 2 dynamic objects
		And the 1 item should still Name equal '012345'
		And the 1 item should still Age equal '044'

	Scenario: There's ways to disable type conversion for matching a dynamic instance against a table
		Given I create a dynamic instance from this table using no type conversion
			| Name   | Age |
			| 012345 | 039 |
		When I compare it to this table using no type conversion
			| Name   | Age |
			| 012345 | 039 |
		Then no instance comparison exception should have been thrown

	Scenario: Comparing against an identical table should match
		Given I create a set of dynamic instances from this table using no type conversion
			| Name   | Age |
			| 012345 | 039 |
			| 065484 | 003 |
		When I compare the set to this table using no type conversion
			| Name   | Age |
			| 012345 | 039 |
			| 065484 | 003 |
		Then no set comparison exception should have been thrown
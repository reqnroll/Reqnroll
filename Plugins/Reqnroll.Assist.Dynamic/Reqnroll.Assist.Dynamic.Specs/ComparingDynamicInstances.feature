Feature: Comparing dynamic instances
	In order to be able to easy do assertions
	As a SpecFlow developer
	I want to be able to compare dynamic instances

Scenario: Matching a dynamic instance against a table
	Given I create a dynamic instance from this table
		| Name   | Age | Birth date | Length in meters |
		| Marcus | 39  | 1972-10-09 | 1.96             |
	When I compare it to this table
		| Name   | Age | Birth date | Length in meters |
		| Marcus | 39  | 1972-10-09 | 1.96             |
	Then no instance comparison exception should have been thrown

Scenario: Not matching when 1 header differ
	Given I create a dynamic instance from this table
		| Name   | 
		| Marcus | 
	When I compare it to this table	 
	   | N      |
	   | Marcus |
	Then an instance comparison exception should be thrown with 2 differences
		And one difference should be on the 'Name' field of the instance
		And one difference should be on the 'N' column of the table

Scenario: Not matching when 2 header differ
	Given I create a dynamic instance from this table
		| Name   | Birth date |
		| Marcus | 2000-01-01 |
	When I compare it to this table	 
	   | N      | Date of birth |
	   | Marcus | 2000-01-01    |
	Then an instance comparison exception should be thrown with 4 differences
		And one difference should be on the 'Name' field of the instance
		And one difference should be on the 'BirthDate' field of the instance
		And one difference should be on the 'N' column of the table
		And one difference should be on the 'DateOfBirth' column of the table

Scenario: Not matching when 1 value differ
	Given I create a dynamic instance from this table
		| Name   |
		| Marcus |
	When I compare it to this table	 
	   | Name   |
	   | Albert |
	Then an instance comparison exception should be thrown with 1 difference
		And one difference should be on the 'Name' property
		And one message should state that the instance had the value 'Marcus'
		And one message should state that the table had the value 'Albert'

Scenario: Not matching when several value differ
	Given I create a dynamic instance from this table
		| Name   | BirthDate  | LengthInMeters |
		| Marcus | 1972-10-09 | 1.96           |
	When I compare it to this table	 
	   | Name   | Birth date | Length in meters |
	   | Albert | 2008-01-24 | 1.04             |
	Then an instance comparison exception should be thrown with 3 difference
		And one difference should be on the 'Name' property
		And one difference should be on the 'BirthDate' property
		And one difference should be on the 'LengthInMeters' property
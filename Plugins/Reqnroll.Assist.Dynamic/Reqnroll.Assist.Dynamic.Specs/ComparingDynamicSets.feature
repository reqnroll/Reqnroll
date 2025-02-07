
Feature: Comparing dynamic sets against tables
	In order to easier and slicker do assertions
	As a SpecFlow developer
	I want to be able to compare a list of dynamic items against a table

	Scenario: Comparing against an identical table should match
		Given I create a set of dynamic instances from this table
			| Name   | Age | Birth date | Length in meters |
			| Marcus | 39  | 1972-10-09 | 1.96             |
			| Albert | 3   | 2008-01-24 | 1.03             |
			| Gustav | 1   | 2010-03-19 | 0.84             |
			| Arvid  | 1   | 2010-03-19 | 0.85             |
		When I compare the set to this table
			| Name   | Age | Birth date | Length in meters |
			| Marcus | 39  | 1972-10-09 | 1.96             |
			| Albert | 3   | 2008-01-24 | 1.03             |
			| Gustav | 1   | 2010-03-19 | 0.84             |
			| Arvid  | 1   | 2010-03-19 | 0.85             |
		Then no set comparison exception should have been thrown

	Scenario: Not matching when 1 column name differ
		Given I create a set of dynamic instances from this table
			| Name   |
			| Marcus |
			| Albert |
			| Gustav |
			| Arvid  |
		When I compare the set to this table
			| N      |
			| Marcus |
			| Albert |
			| Gustav |
			| Arvid  |
		Then an set comparision exception should be thrown with 2 differences
		And one set difference should be on the 'Name' field of the instance
		And one set difference should be on the 'N' column of the table

	Scenario: Not matching when 2 header differ
		Given I create a set of dynamic instances from this table
			| Name   | Age |
			| Marcus | 39  |
			| Albert | 3   |
			| Gustav | 1   |
			| Arvid  | 1   |
		When I compare the set to this table
			| Namn   | Ålder |
			| Marcus | 39    |
			| Albert | 3     |
			| Gustav | 1     |
			| Arvid  | 1     |
		Then an set comparision exception should be thrown with 4 differences
		And one set difference should be on the 'Name' field of the instance
		And one set difference should be on the 'Age' field of the instance
		And one set difference should be on the 'Namn' column of the table
		And one set difference should be on the 'Ålder' column of the table

	Scenario: Not matching when the number of rows are more in the table
		Given I create a set of dynamic instances from this table
			| Name   | Age |
			| Marcus | 39  |
			| Albert | 3   |
		When I compare the set to this table
			| Name   | Age |
			| Marcus | 39  |
			| Albert | 3   |
			| Arvid  | 1   |
		Then an set comparison exception should be thrown
		And the error message for different rows should expect 3 rows for table and 2 rows for instance

	Scenario: Differences on 1 value in 1 row should throw exceptions
		Given I create a set of dynamic instances from this table
			| Name   | Age | Birth date | Length in meters |
			| Marcus | 39  | 1972-10-09 | 1.96             |
			| Albert | 3   | 2008-01-24 | 1.03             |
		When I compare the set to this table
			| Name   | Age | Birth date | Length in meters |
			| Hugo   | 39  | 1972-10-09 | 1.96             |
			| Albert | 3   | 2008-01-24 | 1.03             |
		Then an set comparision exception should be thrown with 1 difference
		And 1 difference should be on row 1 on property 'Name' for the values 'Marcus' and 'Hugo'

	Scenario: Differences on 2 value in 2 different row should throw exceptions
		Given I create a set of dynamic instances from this table
			| Name   | Age | Birth date | Length in meters |
			| Marcus | 39  | 1972-10-09 | 1.96             |
			| Albert | 3   | 2008-01-24 | 0.03             |
		When I compare the set to this table
			| Name   | Age | Birth date | Length in meters |
			| Hugo   | 39  | 1972-10-09 | 1.96             |
			| Albert | 3   | 2008-01-24 | 1.03             |
		Then an set comparision exception should be thrown with 2 difference
		And 1 difference should be on row 1 on property 'Name' for the values 'Marcus' and 'Hugo'
		And 2 difference should be on row 2 on property 'LengthInMeters' for the values '0.03' and '1.03'

	Scenario: Differences on 4 value on 1 row should throw exceptions
		Given I create a set of dynamic instances from this table
			| Name   | Age | Birth date | Length in meters |
			| Marcus | 39  | 1972-10-09 | 1.96             |
			| Albert | 3   | 2008-01-24 | 1.03             |
		When I compare the set to this table
			| Name   | Age | Birth date | Length in meters |
			| Marcus | 39  | 1972-10-09 | 1.96             |
			| Hugo   | 2   | 2010-01-24 | 0.03             |
		Then an set comparision exception should be thrown with 4 difference
		And 1 difference should be on row 2 on property 'Name' for the values 'Marcus' and 'Hugo'
		And 2 difference should be on row 2 on property 'Age' for the values '3' and '2'
		And 3 difference should be on row 2 on property 'BirthDate' for the values '2008-01-24 12:00AM' and '2010-01-24 12:00AM'
		And 4 difference should be on row 2 on property 'LengthInMeters' for the values '1.03' and '0.03'

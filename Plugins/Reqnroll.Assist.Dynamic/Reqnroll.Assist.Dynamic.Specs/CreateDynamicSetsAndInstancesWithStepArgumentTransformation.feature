Feature: Create dynamic sets and instance with step argument transformations
  In order to write super slick and easy code
  As a SpecFlow step definition developer
  I want to be able to define the types as argument to the step

  Scenario: Creating dynamic set with the use of step argument transformation
    Given I create a set of dynamic instances from this table using step argument transformation
      | Name   | Age | Birth date | Length in meters |
      | Marcus | 39  | 1972-10-09 | 1.96             |
      | Albert | 3   | 2008-01-24 | 1.03             |
      | Gustav | 1   | 2010-03-19 | 0.84             |
      | Arvid  | 1   | 2010-03-19 | 0.85             |
    When I compare the set to this table using step argument transformation
      | Name   | Age | Birth date | Length in meters |
      | Marcus | 39  | 1972-10-09 | 1.96             |
      | Albert | 3   | 2008-01-24 | 1.03             |
      | Gustav | 1   | 2010-03-19 | 0.84             |
      | Arvid  | 1   | 2010-03-19 | 0.85             |
    Then no set comparison exception should have been thrown

  Scenario: Matching a dynamic instance against a table
    Given I create a dynamic instance from this table using step argument transformation
      | Name   | Age | Birth date | Length in meters | Is Developer |
      | Marcus | 39  | 1972-10-09 | 1.96             | true         |
    When I compare it to this table using step argument transformation
      | Name   | Age | Birth date | Length in meters | Is Developer |
      | Marcus | 39  | 1972-10-09 | 1.96             | true         |
    Then no instance comparison exception should have been thrown

  Scenario: Test property with step argument transformation
    Given I create a dynamic instance from this table using step argument transformation
      | Name   | Age | Birth date | Length in meters | Is Developer |
      | Marcus | 39  | 1972-10-09 | 1.96             | true         |
    Then the Name property should equal 'Marcus'
    And the IsDeveloper property should equal 'true'
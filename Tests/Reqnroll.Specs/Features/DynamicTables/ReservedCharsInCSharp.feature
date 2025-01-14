Feature: ReservedCharsInCSharp
	In order to be able to write more expressive meaningful scenarios
	As a scenario writer
	I want to be able to use any character, including reserved words

@wip
Scenario: Using reserved C# characters in column names
    When I create a dynamic instance from this table
        | C$harp n@me (with strange chars) |
        | A value | 
    Then the CharpNmeWithStrangeChars property should equal 'A value'


@wip
Scenario: Only alfa numeric characters, plus underscore is allowed in variable names
       When I create a dynamic instance from this table
        | My_Nice_Variable | My $$ Variable (needs clean up) |
        | A value          |  Another value |
    Then the My_Nice_Variable property should equal 'A value'
           And the MyVariableNeedsCleanUp property should equal 'Another value'

@wip
Scenario: Using only reserved C# characters in column names
    When I create a dynamic instance with only reserved chars
        | $@() |
        | A value | 
    Then an exception with a nice error message about the property only containing reserved chars should be thrown
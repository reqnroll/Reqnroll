Feature: Examples Tables - With Undefined Steps
  The replacement pattern used in scenario outlines does not influence how steps
  are matched. The replacement pattern is replaced, and step definitions are
  matched against that text. Because of that the following results in one
  undefined step for each example and a suggested snippet to implement it. 

  Scenario Outline: Eating cucumbers
    Given there are <start> cucumbers
    When I eat <eat> cucumbers
    Then I should have <left> cucumbers

    @undefined
    Examples: These are undefined because the value is not an {int}
      | start | eat    | left  |
      | pear  | 1      | 12    |
      | 12    | banana | 12    |
      | 0     | 1      | apple |

Feature: Parameter Types
  Cucumber will generate an error message if a step definition registers
  an unknown parameter type, but the suite will run. Additionally, because
  the step is effectively undefined, a suggestion will also be created.

  Scenario: undefined parameter type
    Given CDG is closed because of a strike

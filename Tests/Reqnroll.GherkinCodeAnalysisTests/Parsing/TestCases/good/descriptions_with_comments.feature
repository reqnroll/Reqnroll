Feature: Descriptions with comments everywhere
  This is a description
  # comment
  with a comment in the middle and at the end
  # comment 2

  Scenario: two lines
  This description
  # comment
  # comment 2
  has two lines and two comments in the middle and is indented with two spaces
    Given the minimalism

Scenario: without indentation
This is a description without indentation
# comment
and a comment in the middle and at the end
# comment 2

  Given the minimalism

  Scenario: empty lines in the middle
  This description
  # comment

  has an empty line and a comment in the middle
    Given the minimalism

  Scenario: empty lines around

  # comment
  This description
  has an empty lines around
  # comment

    Given the minimalism

  Scenario Outline: scenario outline with a description
# comment
This is a scenario outline description with comments
# comment 2
in the middle and before and at the end
# comment 3
    Given the minimalism

  Examples: examples with description
# comment
This is an examples description
# comment
with a comment in the middle
# comment

    | foo |
    | bar |

  Scenario: scenario with just a comment
    # comment
    Given the minimalism

  Scenario: scenario with a comment with new lines around

    # comment

    Given the minimalism  
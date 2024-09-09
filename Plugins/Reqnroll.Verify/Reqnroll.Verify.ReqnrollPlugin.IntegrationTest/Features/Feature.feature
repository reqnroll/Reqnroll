@DoNotParallelize
Feature: Verify Test

    Scenario: Check if Verify is working
        When I try Verify with Reqnroll
        Then it works

    Scenario Outline: Check if Verify is working with Example Tables
        When I try Verify with Reqnroll for Parameter '<Parameter>'
        Then it works

        Examples:
          | Parameter |
          | 1         |
          | 2         |

    Scenario: Check if Verify is working with multiple scenario parameters
        When I try Verify with Reqnroll for Parameter '<Parameter>' and some additional parameter '<Additional Parameter>'
        Then it works

    Examples:
      | Parameter | Additional Parameter |
      | 1         | a                    |
      | 2         | b                    |

    Scenario: Check if Verify is working with global registered path info
        When I try Verify with Reqnroll with global registered path info
        Then it works
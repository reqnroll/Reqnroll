Feature: Verify Parallel feature #2

    Scenario: Check if Verify uses the correct paths when ran in parallel 2
        When I try Verify with Reqnroll in parallel
        Then it works in parallel with contents `Verify Parallel feature #2`
        And the verified file is `Verify Parallel feature #2.Check if Verify uses the correct paths when ran in parallel 2.verified.txt` with contents `Verify Parallel feature #2`

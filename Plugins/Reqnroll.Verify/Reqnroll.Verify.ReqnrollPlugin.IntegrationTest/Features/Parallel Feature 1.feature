Feature: Verify Parallel feature #1

    Scenario: Check if Verify uses the correct paths when ran in parallel 1
        When I try Verify with Reqnroll in parallel
        Then it works in parallel with contents `Verify Parallel feature #1`
        And the verified file is `Verify Parallel feature #1.Check if Verify uses the correct paths when ran in parallel 1.verified.txt` with contents `Verify Parallel feature #1`
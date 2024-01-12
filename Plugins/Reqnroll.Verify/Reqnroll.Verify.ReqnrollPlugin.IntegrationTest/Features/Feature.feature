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

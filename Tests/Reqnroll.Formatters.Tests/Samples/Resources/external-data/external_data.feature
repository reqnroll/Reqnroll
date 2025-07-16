Feature: External Data from CSV file

@DataSource:products.csv
Scenario: Valid product prices are calculated
    The scenario will be treated as a scenario outline with the examples from the CSV file.
	Given the customer has put 1 pcs of <product> to the basket
	When the basket price is calculated
	Then the basket price should be greater than zero

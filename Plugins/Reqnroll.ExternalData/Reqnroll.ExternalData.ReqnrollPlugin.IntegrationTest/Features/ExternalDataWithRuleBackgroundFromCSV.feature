Feature: ExternalDataWithRuleBackgroundFromCSV

This feature demonstrates that Rule Background steps are handled appropriately when using the External Data Plugin.
This test validates a regression.

Rule: Steps in Background are properly executed
	Background:
	Given my favorite color is blue
@DataSource:products.csv
Scenario: The basket price is calculated correctly
	The scenario will be treated as a scenario outline with the examples from the CSV file.
	The CSV file contains multile fields, including product and price.
	Given the price of <product> is €<price>
	And the customer has put 1 pcs of <product> to the basket
	When the basket price is calculated
	Then the basket price should be €<price>
	And the color given as my favorite was blue

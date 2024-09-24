Feature: background

Steps provided in a background section should properly result in Cucumber Message TestSteps in the scenarios of the feature

Background: 
# set up bank account balance
Given I have $500 in my checking account
And I have $200 in my savings account

Scenario: Combined Balance
	When the accounts are combined
	Then I have $700

Scenario: Transfer Money
	When I transfer $150 from savings to checking
	Then My checking account has a balance of $650
	And My savings account has a balance of $50

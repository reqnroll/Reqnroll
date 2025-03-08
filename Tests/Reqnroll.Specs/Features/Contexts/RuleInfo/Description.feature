Feature: Rule Description Accessing

Scenario: Check Rule description is not empty
	Given the following binding class
        """
		using System;
		using Reqnroll;

		[Binding]
		public class DescriptionTestsBinding : Steps
		{
			[Then(@"Check ""(.*)"" match with rule description in context")]
			public void ThenCheckMatchWithRuleDescriptionInContext(string desc)
			{
				var testValue = ScenarioContext.RuleInfo.Description;
				if (testValue != desc) throw new Exception("Rule Description is incorrectly parsed"); 						 
			}
		}
        """	

	And there is a feature file in the project as
         """
		 Feature: DescriptionFeature
		 Test Feature Description

		 Rule: RuleDescriptionCheck
		 Test Rule Description

		 Scenario: ScenarioDescriptionCheck
		 Test Scenario Description
		 Then Check "Test Rule Description" match with rule description in context
         """
	When I execute the tests
	Then the execution summary should contain
         | Succeeded |
         | 1         |

Scenario: Check Rule description is null if empty
	Given the following binding class
        """
		using System;
		using Reqnroll;

		[Binding]
		public class DescriptionTestsBinding : Steps
		{	
			[Then(@"Check that rule description is null in context")]
			public void ThenCheckThatRuleDescriptionIsNullInContext()
			{
				var testValue = ScenarioContext.RuleInfo.Description;
				if (testValue != null) throw new Exception("Rule Description is incorrectly parsed"); 						 
			}
		}
        """	

	And there is a feature file in the project as
         """
		 Feature: DescriptionFeature
		 Test Feature Description

		 Rule: RuleDescriptionCheck

		 Scenario: ScenarioDescriptionCheck
		 
		 Then Check that rule description is null in context
         """
	When I execute the tests
	Then the execution summary should contain
         | Succeeded |
         | 1         |

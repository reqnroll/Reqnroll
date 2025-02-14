Feature: Rule Title Accessing

Scenario: Check Rule title is not empty
	Given the following binding class
        """
		using System;
		using Reqnroll;

		[Binding]
		public class TitleTestsBinding : Steps
		{
			[Then(@"Check ""(.*)"" match with rule title in context")]
			public void ThenCheckMatchWithRuleTitleInContext(string title)
			{
				var testValue = ScenarioContext.RuleInfo.Title;
				if (testValue != title) throw new Exception("Rule Title is incorrectly parsed"); 						 
			}
		}
        """	

	And there is a feature file in the project as
         """
		 Feature: TitleFeature
		 Test Feature Description

		 Rule: RuleTitleCheck

		 Scenario: ScenarioTitleCheck
		 Test Scenario Description
		 Then Check "RuleTitleCheck" match with rule title in context
         """
	When I execute the tests
	Then the execution summary should contain
         | Succeeded |
         | 1         |

Scenario: Check Rule is null if empty
	Given the following binding class
        """
		using System;
		using Reqnroll;

		[Binding]
		public class NullTestsBinding : Steps
		{	
			[Then(@"Check that rule is null in context")]
			public void ThenCheckThatRuleIsNullInContext()
			{
				var testValue = ScenarioContext.RuleInfo;
				if (testValue != null) throw new Exception("Rule should be null");
			}
		}
        """	

	And there is a feature file in the project as
         """
		 Feature: DescriptionFeature

		 Scenario: ScenarioDescriptionCheck
		 
		 Then Check that rule is null in context
         """
	When I execute the tests
	Then the execution summary should contain
         | Succeeded |
         | 1         |

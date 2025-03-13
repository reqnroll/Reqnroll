Feature: Rule Tags Accessing

Scenario: Accessing tags of a simple rule
	Given the following step definitions
		"""
		[When("the rule tags are accessed from a step definition")]
		public void WhenTheRuleTagsAreAccessedFromAStepDefinition()
		{
			if (!_scenarioContext.RuleInfo.Tags.Contains("rule_tag")) throw new Exception("Rule Tags does not contain rule tags");
		}
        """	

	And there is a feature file in the project as
         """
		 @feature_tag
		 Feature: Sample feature

		 @rule_tag
		 Rule: Sample rule

		 @scenario_tag
		 Scenario: Sample scenario
		 When the rule tags are accessed from a step definition
         """
	When I execute the tests
	Then the execution summary should contain
         | Succeeded |
         | 1         |

Scenario: Accessing tags of a simple rule without tags
	Given the following step definitions
		"""
		[When("the rule tags are accessed from a step definition")]
		public void WhenTheRuleTagsAreAccessedFromAStepDefinition()
		{
			if (_scenarioContext.RuleInfo.Tags.Length != 0) throw new Exception("Rule Tags should be empty");
		}
        """	

	And there is a feature file in the project as
         """
		 @feature_tag
		 Feature: Sample feature

		 Rule: Sample rule

		 @scenario_tag
		 Scenario: Sample scenario
		 When the rule tags are accessed from a step definition
         """
	When I execute the tests
	Then the execution summary should contain
         | Succeeded |
         | 1         |

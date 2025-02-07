Feature: ExternalBinding

This feature calls upon one step defined in an internal class
and one step defined in an external assembly

Scenario: External_Binding_Assemblies_Work_With_Cucumber_Messages
	Given I have 3 cukes in my belly
	When the sample external binding class is called

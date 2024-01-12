Feature: SingleFileGenerator Checks


Scenario: Build Error when using the SingleFileGenerator and Reqnroll.Tools.MSBuild.Generator

	Given there is a Reqnroll project
	And it is using Reqnroll.Tools.MSBuild.Generator
	And has a feature file with the SingleFileGenerator configured

	When I compile the solution

	Then is a compilation error
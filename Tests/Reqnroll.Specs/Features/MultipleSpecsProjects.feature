Feature: Multiple Specs Projects

@quarantaine
Scenario Outline: Two projects with the same unit test provider
	Given I have Specs.Project.A and Specs.Project.B using the same unit test provider
	And Specs.Project.B references Specs.Project.A
	When I build the solution using '<Build Tool>'
	Then the build should succeed

	Examples:
	| Build Tool     |
	| dotnet build   |
	| dotnet msbuild |

# duplicated scenario to be able to filter it out on CI build
@quarantaine
@requiresMsBuild
@globalusingdirective #MSBuild for VS2019 and Mono throws error CS8652: The feature 'global using directive' is currently in Preview and unsupported.
Scenario Outline: Two projects with the same unit test provider (MsBuild)
	Given I have Specs.Project.A and Specs.Project.B using the same unit test provider
	And Specs.Project.B references Specs.Project.A
	When I build the solution using '<Build Tool>'
	Then the build should succeed

	Examples: 
	| Build Tool    |
	| MSBuild       |

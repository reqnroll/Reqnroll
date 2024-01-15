Function Write-Header 
{
	Param([String]$headerText)
	
	Write-Host $headerText -ForegroundColor Blue -BackgroundColor Gray -NoNewLine
	Write-Host
}

$scriptDir = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent
$testFolder = "TestTemp"
$testDir="$scriptDir\$testFolder"
$testSlnFolder = "TestSln"

Write-Header "Cleanup test directory"
Write-Host $testDir
rd $testDir -r -force -ErrorAction Ignore
md $testDir

Push-Location -Path $testDir

Write-Header "Create tool manifest"
dotnet new tool-manifest

Write-Header "Install TPG tool"
#Note: to install pre-release version we need to specify a floating version number: https://github.com/NuGet/Home/wiki/Support-pre-release-packages-with-floating-versions
dotnet tool install --no-cache --add-source ..\..\SpecFlow.TestProjectGenerator.Cli\nupkg\ SpecFlow.TestProjectGenerator.Cli --version *-*

Write-Header "Run TPG"
dotnet tool run specflow-tpg --sln-name Test

Write-Header "Run tests in generated project"
dotnet test Test

Pop-Location
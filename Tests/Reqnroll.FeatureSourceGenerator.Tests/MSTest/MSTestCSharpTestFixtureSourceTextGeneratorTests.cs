using Gherkin;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.ComponentModel.DataAnnotations;
using System.Configuration;

namespace Reqnroll.FeatureSourceGenerator.MSTest;

public class MSTestCSharpTestFixtureSourceTextGeneratorTests
{
    [Fact]
    public void SampleFeatureGeneratesExpectedTestFixture()
    {
        var handler = BuiltInTestFrameworkHandlers.MSTest;

        const string featureText = @"#language: en
@featureTag1
Feature: Calculator

@mytag
Scenario: Add two numbers
	Given the first number is 50
	And the second number is 70
	When the two numbers are added
	Then the result should be 120";

        var featureInfo = new FeatureInformation(
            new GherkinSyntaxTree(
                new Parser().Parse(new SourceTokenScanner(SourceText.From(featureText))), 
                [], 
                "C:\\Users\\ReqnrollUser\\Source\\Repos\\TestProject\\Features\\Calculator.feature"),
            "Calculator",
            "MyProject.Features",
            new CompilationInformation(null, LanguageNames.CSharp, []),
            handler);

        var generator = new MSTestCSharpTestFixtureSourceTextGenerator(featureInfo);

        var sourceText = generator.GenerateTestFixtureSourceText();
        var result = sourceText.ToString();

        var expected = @"namespace MyProject.Features
{
    [global::Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public class CalculatorFeature
    {
        // start: MSTest Specific part
        
        private global::Microsoft.VisualStudio.TestTools.UnitTesting.TestContext? _testContext;
        
        public virtual global::Microsoft.VisualStudio.TestTools.UnitTesting.TestContext? TestContext
        {
            get
            {
                return this._testContext;
            }
            set
            {
                this._testContext = value;
            }
        }
        
        // end: MSTest Specific part
        
        // start: shared service method & consts, NO STATE!
        
#line 1 ""C:\Users\ReqnrollUser\Source\Repos\TestProject\Features\Calculator.feature""
#line hidden
        
        private static readonly string[] featureTags = new string[] { ""featureTag1"" };
        
        private static readonly global::Reqnroll.FeatureInfo featureInfo = new global::Reqnroll.FeatureInfo(new global::System.Globalization.CultureInfo(""en""), ""C:\Users\ReqnrollUser\Source\Repos\TestProject\Features"", ""Calculator"", null, global::Reqnroll.ProgrammingLanguage.CSharp, featureTags);
        
        public async global::System.Threading.Tasks.Task ScenarioInitialize(global::Reqnroll.ITestRunner testRunner, global::Reqnroll.ScenarioInfo scenarioInfo)
        {
            // handle feature initialization
            if (testRunner.FeatureContext == null || !object.ReferenceEquals(testRunner.FeatureContext.FeatureInfo, featureInfo))
            await testRunner.OnFeatureStartAsync(featureInfo);
            
            // handle scenario initialization
            testRunner.OnScenarioInitialize(scenarioInfo);
            
            // MsTest specific customization:
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs(_testContext);
        }
        
        // end: shared service method & consts, NO STATE!
        
        [global::Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        [global::Microsoft.VisualStudio.TestTools.UnitTesting.Description(""Add two numbers"")]
        [global::Microsoft.VisualStudio.TestTools.UnitTesting.TestProperty(""FeatureTitle"", ""Calculator"")]
        [global::Microsoft.VisualStudio.TestTools.UnitTesting.TestCategory(""featureTag1"")]
        [global::Microsoft.VisualStudio.TestTools.UnitTesting.TestCategory(""mytag"")]
        public async Task AddTwoNumbers()
        {
            // getting test runner
            string testWorkerId = global::System.Threading.Thread.CurrentThread.ManagedThreadId.ToString(); // this might be different with other test runners
            var testRunner = global::Reqnroll.TestRunnerManager.GetTestRunnerForAssembly(null, testWorkerId);
            
            // start: calculate ScenarioInfo
            string[] tagsOfScenario = new string[] { ""mytag"" };
            var argumentsOfScenario = new global::System.Collections.Specialized.OrderedDictionary(); // needed for scenario outlines
            var inheritedTags = featureTags; // will be more complex if there are rules
            var scenarioInfo = new global::Reqnroll.ScenarioInfo(""Add two numbers"", null, tagsOfScenario, argumentsOfScenario, inheritedTags);
            // end: calculate ScenarioInfo
            
            try
            {
#line 6
                await ScenarioInitialize(testRunner, scenarioInfo);
#line hidden
                
                if (global::Reqnroll.TagHelper.ContainsIgnoreTag(scenarioInfo.CombinedTags))
                {
                    testRunner.SkipScenario();
                }
                else
                {
                    await testRunner.OnScenarioStartAsync();
                    
                    // start: invocation of scenario steps
#line 7
                    await testRunner.GivenAsync(""the first number is 50"", null, null, ""Given "");
#line hidden
#line 8
                    await testRunner.AndAsync(""the second number is 70"", null, null, ""And "");
#line hidden
#line 9
                    await testRunner.WhenAsync(""the two numbers are added"", null, null, ""When "");
#line hidden
#line 10
                    await testRunner.ThenAsync(""the result should be 120"", null, null, ""Then "");
#line hidden
                    // end: invocation of scenario steps
                }
                
                // finishing the scenario
                await testRunner.CollectScenarioErrorsAsync();
            }
            finally
            {
                await testRunner.OnScenarioEndAsync();
            }
        }
    }
}
";

        result.Should().Be(expected);
    }
}

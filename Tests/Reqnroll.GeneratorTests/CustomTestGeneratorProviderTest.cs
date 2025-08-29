using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.IO;
using FluentAssertions;
using Xunit;
using Reqnroll.Configuration;
using Reqnroll.Generator;
using Reqnroll.Generator.CodeDom;
using Reqnroll.Generator.Generation;
using Reqnroll.Generator.UnitTestProvider;
using Reqnroll.Parser;
using Reqnroll.Tracing;

namespace Reqnroll.GeneratorTests
{
    /// <summary>
    /// A test for testing cusomterized test generator provider
    /// </summary>
    
    public class CustomTestGeneratorProviderTest
    {
        private const string SampleFeatureFile = @"
            Feature: Sample feature file for a custom generator provider
            
            Scenario: Simple scenario
                Given there is something
                When I do something
                Then something should happen

            @mytag
            Scenario Outline: Simple Scenario Outline
                Given there is something
                    """"""
                      long string
                    """"""
                When I do <what>
                    | foo | bar |
                    | 1   | 2   |
                Then something should happen
            Examples: 
                | what           |
                | something      |
                | something else |
";

        public static UnitTestFeatureGenerator CreateUnitTestConverter(IUnitTestGeneratorProvider testGeneratorProvider)
        {
            var codeDomHelper = new CodeDomHelper(CodeDomProviderLanguage.CSharp);
            var runtimeConfiguration = ConfigurationLoader.GetDefault();
            runtimeConfiguration.AllowRowTests = true;
            runtimeConfiguration.AllowDebugGeneratedFiles = true;

            return new UnitTestFeatureGenerator(testGeneratorProvider, codeDomHelper, runtimeConfiguration, new DecoratorRegistryStub());
        }

        /// <summary>
        /// Generates the scenario example tests.
        /// </summary>
        [Fact]
        public void GenerateScenarioExampleTests()
        {
            var parser = new ReqnrollGherkinParser(new CultureInfo("en-US"));
            using (var reader = new StringReader(SampleFeatureFile))
            {
                var feature = parser.Parse(reader, new ReqnrollDocumentLocation($"dummy_location_for_{nameof(GenerateScenarioExampleTests)}"));
                feature.Should().NotBeNull();
                
                var sampleTestGeneratorProvider = new SimpleTestGeneratorProvider(new CodeDomHelper(CodeDomProviderLanguage.CSharp));
                var converter = CreateUnitTestConverter(sampleTestGeneratorProvider);
                CodeNamespace code = converter.GenerateUnitTestFixture(feature, "TestClassName", "Target.Namespace").CodeNamespace;

                code.Should().NotBeNull();
                
                
                // make sure name space is changed
                code.Name.Should().Be(SimpleTestGeneratorProvider.DefaultNameSpace);
                
                // make sure all method titles are changed correctly
                List<string> methodTitles = new List<string>();
                for (int i = 0; i < code.Types[0].Members.Count; i++)
                {
                    methodTitles.Add(code.Types[0].Members[i].Name);
                }

                foreach (var title in sampleTestGeneratorProvider.newTitles)
                {
                    methodTitles.Contains(title).Should().BeTrue();
                }
            }
        }

        /// <summary>
        /// This class will change the default name space of a geneated test code, and the method name for tests corresponds to a scenario example
        /// </summary>
        class SimpleTestGeneratorProvider : MsTestGeneratorProvider
        {
            public SimpleTestGeneratorProvider(CodeDomHelper codeDomHelper) : base(codeDomHelper)
            {
            }

            public static string DefaultNameSpace
            {
                get
                {
                    return "SampleTestGeneratorProvider";
                }
            }

            public override void FinalizeTestClass(TestClassGenerationContext generationContext)
            {
                base.FinalizeTestClass(generationContext);
                // change namespace 
                generationContext.Namespace.Name = DefaultNameSpace;
            }

            public override void SetTestMethodAsRow(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, string scenarioTitle, string exampleSetName, string variantName, IEnumerable<KeyValuePair<string, string>> arguments)
            {
                base.SetTestMethodAsRow(generationContext, testMethod, scenarioTitle, exampleSetName, variantName, arguments);

                // change memberMethodName
                testMethod.Name = GetMethodName(scenarioTitle, exampleSetName, arguments);
                newTitles.Add(testMethod.Name);
            }

            public List<string> newTitles = new List<string>();

            /// <summary>
            /// Gets the name of the method by concat the name and value of arugment together
            /// </summary>
            /// <param name="title">The title.</param>
            /// <param name="exampleSetTitle">The example set title.</param>
            /// <param name="arguments">The arguments.</param>
            /// <returns></returns>
            private static string GetMethodName(string title, string exampleSetTitle, IEnumerable<KeyValuePair<string, string>> arguments)
            {
                string testMethodName = string.IsNullOrEmpty(exampleSetTitle)
               ? string.Format("{0}", title)
               : string.Format("{0}_{1}", title, exampleSetTitle);

                testMethodName += string.Concat(arguments.Select(kp => "_" + kp.Key + "_" + kp.Value).ToArray());
                IEnumerable<char> chars = from ch in testMethodName
                                          where (char.IsLetter(ch) || char.IsNumber(ch) || ch == '_') && ch != ' '
                                          select ch;
                testMethodName = new string(chars.ToArray());
                return testMethodName;
            }
        }
    }
}

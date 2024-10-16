using Io.Cucumber.Messages.Types;
using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions;
using Newtonsoft.Json.Bson;
using Reqnroll;
using System.Reflection;
using FluentAssertions;
using System.Text.Json;
using System.ComponentModel;
using Reqnroll.TestProjectGenerator;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Reqnroll.TestProjectGenerator.Driver;
using Moq;
using Reqnroll.Tracing;
using Reqnroll.EnvironmentAccess;
using SpecFlow.Internal.Json;
using Microsoft.VisualBasic.FileIO;
using Reqnroll.CucumberMessages.PayloadProcessing;
using Reqnroll.CommonModels;
using Reqnroll.CucumberMessages.Configuration;

namespace CucumberMessages.CompatibilityTests
{
    [TestClass]
    public class CucumberCompatibilityTests : CucumberCompatibilityTestBase
    {
        [TestMethod]
        public void Unit_PartialConfiguration()
        {
            var envWrapper = new Moq.Mock<IEnvironmentWrapper>();
            var envVariable = new Success<string>("partialConfiguration.json");

            envWrapper.Setup(x => x.GetEnvironmentVariable(It.IsAny<string>())).Returns(envVariable);

            var configReader = new RCM_ConfigFile_ConfigurationSource(envWrapper.Object);
            var config = configReader.GetConfiguration();

            config.FileOutputEnabled.Should().BeTrue();
        }

        [TestMethod]
        public void NullTest()
        {
            ResetCucumberMessages();
            // The purpose of this test is to confirm that when Cucumber Messages are turned off, the Cucumber Messages ecosystem does not cause any interference anywhere else
            AddFeatureFile("""
                Feature: Cucumber Messages Null Test
                  Scenario: Eating Cukes
                     When I eat 5 cukes
                """);

            AddPassingStepBinding("When");

            ExecuteTests();

            ShouldAllScenariosPass();
        }
        [TestMethod]
        public void SmokeTest()
        {
            ResetCucumberMessages("reqnoll_report.ndjson");
            EnableCucumberMessages();
            //SetCucumberMessagesOutputFileName();
            CucumberMessagesAddConfigurationFile("CucumberMessages.configuration.json");

            AddFeatureFile("""
                Feature: Cucumber Messages Smoke Test
                  Scenario: Eating Cukes
                     When I eat 5 cukes
                """);

            AddPassingStepBinding("When");
            ExecuteTests();

            ShouldAllScenariosPass();
        }

        [TestMethod]
        public void CanGenerateGUIDIds_SmokeTest()
        {
            ResetCucumberMessages("CanGenerateGUIDIds_SmokeTest.ndjson");
            EnableCucumberMessages();
            SetCucumberMessagesOutputFileName("CanGenerateGUIDIds_SmokeTest.ndjson");
            SetEnvironmentVariableForGUIDIdGeneration();
            CucumberMessagesAddConfigurationFile("CucumberMessages.configuration.json");

            AddFeatureFile("""
                Feature: Cucumber Messages Smoke Test
                  Scenario: Eating Cukes
                     When I eat 5 cukes
                """);

            AddPassingStepBinding("When");
            ExecuteTests();

            ShouldAllScenariosPass();
        }

        [TestMethod]
        public void SmokeTestMultipleFeatures()
        {
            ResetCucumberMessages("SmokeTestMultipleFeatures.ndjson");
            EnableCucumberMessages();
            SetCucumberMessagesOutputFileName("SmokeTestMultipleFeatures.ndjson");
            CucumberMessagesAddConfigurationFile("CucumberMessages.configuration.json");

            AddFeatureFile("""
                Feature: Cucumber Messages Smoke Test
                  Scenario: Eating Cukes
                     When I eat 5 cukes
                """);
            AddFeatureFile("""
                Feature: Cucumber Messages Smoke Test Second Smoke Test
                  Scenario: Eating Other Cukes
                     When I eat 6 cukes
                """);

            AddPassingStepBinding("When");
            ExecuteTests();

            ShouldAllScenariosPass();
        }


        [TestMethod]
        public void SmokeOutlineTest()
        {
            ResetCucumberMessages("Cucumber Messages Smoke Outline Test.ndjson");
            EnableCucumberMessages();
            SetCucumberMessagesOutputFileName("CucumberMessages Smoke Outline Test.ndjson");
            CucumberMessagesAddConfigurationFile("CucumberMessages.configuration.json");

            AddFeatureFile("""
                Feature: Cucumber Messages Smoke Outline Test
                  @some-tag
                  Scenario Outline: Log JSON
                     When the following string is attached as <color>:
                  Examples:
                    | color |
                    | "red" |
                    | "green" |
                    | "blue" |
                """);

            AddPassingStepBinding("When");
            ExecuteTests();

            ShouldAllScenariosPass();
        }
        [TestMethod]
        public void SmokeOutlineTestAsMethods()
        {
            ResetCucumberMessages("Cucumber Messages Smoke Outline Test As Methods.ndjson");
            var _configurationFileDriver = GetServiceSafe<ConfigurationFileDriver>();
            _configurationFileDriver.SetIsRowTestsAllowed(false);

            EnableCucumberMessages();
            SetCucumberMessagesOutputFileName("CucumberMessages Smoke Outline Test As Methods.ndjson");
            CucumberMessagesAddConfigurationFile("CucumberMessages.configuration.json");

            AddFeatureFile("""
                Feature: Cucumber Messages Smoke Outline Test As Methods
                  @some-tag
                  Scenario Outline: Log JSON
                     When the following string is attached as <color>:
                  Examples:
                    | color |
                    | "red" |
                    | "green" |
                    | "blue" |
                """);

            AddPassingStepBinding("When");
            ExecuteTests();

            ShouldAllScenariosPass();
        }


        [TestMethod]
        public void CucumberMessagesInteropWithExternalData()
        {
            ResetCucumberMessages("External Data from CSV file.ndjson");
            // The purpose of this test is to prove that the ScenarioOutline tables generated by the ExternalData plugin can be used in Cucumber messages
            EnableCucumberMessages();
            SetCucumberMessagesOutputFileName("External Data from CSV file.ndjson");
            _projectsDriver.AddNuGetPackage("Reqnroll.ExternalData", "2.1.1-local");
            CucumberMessagesAddConfigurationFile("CucumberMessages.configuration.json");

            // this test borrows a subset of a feature, the binding class, and the data file from ExternalData.ReqnrollPlugin.IntegrationTest
            var content = _testFileManager.GetTestFileContent("products.csv", "CucumberMessages.CompatibilityTests", Assembly.GetExecutingAssembly());
            _projectsDriver.AddFile("products.csv", content);
            AddFeatureFile("""
                Feature: External Data from CSV file

                @DataSource:products.csv
                Scenario: Valid product prices are calculated
                	The scenario will be treated as a scenario outline with the examples from the CSV file.
                	Given the customer has put 1 pcs of <product> to the basket
                	When the basket price is calculated
                	Then the basket price should be greater than zero
                
                """);

            AddBindingClass("""
                using System;
                using System.Collections.Generic;
                using System.Linq;

                namespace Reqnroll.ExternalData.ReqnrollPlugin.IntegrationTest.StepDefinitions
                {
                    [Binding]
                    public class PricingStepDefinitions
                    {
                        class PriceCalculator
                        {
                            private readonly Dictionary<string, int> _basket = new();
                            private readonly Dictionary<string, decimal> _itemPrices = new();

                            public void AddToBasket(string productName, int quantity)
                            {
                                if (!_basket.TryGetValue(productName, out var currentQuantity)) 
                                    currentQuantity = 0;
                                _basket[productName] = currentQuantity + quantity;
                            }

                            public decimal CalculatePrice()
                            {
                                return _basket.Sum(bi => GetPrice(bi.Key) * bi.Value);
                            }

                            private decimal GetPrice(string productName)
                            {
                                if (_itemPrices.TryGetValue(productName, out var itemPrice)) 
                                    return itemPrice;
                                return 1.5m;
                            }

                            public void SetPrice(string productName, in decimal itemPrice)
                            {
                                _itemPrices[productName] = itemPrice;
                            }
                        }

                        private readonly ScenarioContext _scenarioContext;
                        private readonly PriceCalculator _priceCalculator = new();
                        private decimal _calculatedPrice;

                        public PricingStepDefinitions(ScenarioContext scenarioContext)
                        {
                            _scenarioContext = scenarioContext;
                        }

                        [Given(@"the price of (.*) is �(.*)")]
                        public void GivenThePriceOfProductIs(string productName, decimal itemPrice)
                        {
                            _priceCalculator.SetPrice(productName, itemPrice);
                        }

                        [Given(@"the customer has put (.*) pcs of (.*) to the basket")]
                        public void GivenTheCustomerHasPutPcsOfProductToTheBasket(int quantity, string productName)
                        {
                            _priceCalculator.AddToBasket(productName, quantity);
                        }

                        [Given(@"the customer has put a product to the basket")]
                        public void GivenTheCustomerHasPutAProductToTheBasket()
                        {
                            var productName = _scenarioContext.ScenarioInfo.Arguments["product"]?.ToString();
                            _priceCalculator.AddToBasket(productName, 1);
                        }

                        [When(@"the basket price is calculated")]
                        public void WhenTheBasketPriceIsCalculated()
                        {
                            _calculatedPrice = _priceCalculator.CalculatePrice();
                        }

                        [Then(@"the basket price should be greater than zero")]
                        public void ThenTheBasketPriceShouldBeGreaterThanZero()
                        {
                            if (_calculatedPrice <= 0) throw new Exception("Basket price is less than zero: " + _calculatedPrice );
                        }

                        [Then(@"the basket price should be �(.*)")]
                        public void ThenTheBasketPriceShouldBe(decimal expectedPrice)
                        {
                            if(expectedPrice != _calculatedPrice) throw new Exception("Basket price is not as expected: " + _calculatedPrice + " vs " + expectedPrice);
                        }

                    }
                }
                
                """);
            ExecuteTests();

            ShouldAllScenariosPass(3);

        }

        [TestMethod]
        [DataRow("minimal", "minimal")]
        [DataRow("cdata", "cdata")]
        [DataRow("pending", "Pending steps")]
        [DataRow("empty", "Empty Scenarios")]
        [DataRow("examples-tables", "Examples Tables")]
        [DataRow("data-tables", "Data Tables")]
        [DataRow("hooks", "Hooks")]
        [DataRow("parameter-types", "Parameter Types")]
        [DataRow("undefined", "Undefined steps")]
        [DataRow("stack-traces", "Stack traces")]
        [DataRow("rules", "Usage of a `Rule`")]
        // These CCK scenario examples produce Cucumber Messages that are materially compliant with the CCK.
        // The messages produced match the CCK expected messages, with exceptions for things
        // that are not material to the CCK spec (such as IDs don't have to be generated in the same order, timestamps don't have to match, etc.)
        // The rules for what must match and what is allowed to not match are built in to a series of custom FluentAssertion validation rules
        // (located in the CucumberMessagesValidator class)
        public void CCKScenarios(string testName, string featureNameText)
        {
            ResetCucumberMessages(featureNameText + ".ndjson");
            EnableCucumberMessages();
            SetCucumberMessagesOutputFileName(featureNameText + ".ndjson");
            CucumberMessagesAddConfigurationFile("CucumberMessages.configuration.json");
            AddUtilClassWithFileSystemPath();

            var featureFileName = testName.Replace("-", "_");

            AddFeatureFileFromResource($"{featureFileName}/{featureFileName}.feature", "CucumberMessages.CompatibilityTests.CCK", Assembly.GetExecutingAssembly());
            AddBindingClassFromResource($"{featureFileName}/{featureFileName}.cs", "CucumberMessages.CompatibilityTests.CCK", Assembly.GetExecutingAssembly());
            //AddBinaryFilesFromResource($"{testName}", "CucumberMessages.CompatibilityTests.CCK", Assembly.GetExecutingAssembly());

            ExecuteTests();

            var validator = new CucumberMessagesValidator(GetActualResults(testName, featureNameText).ToList(), GetExpectedResults(testName, featureFileName).ToList());
            validator.ShouldPassBasicStructuralChecks();
            validator.ResultShouldPassAllComparisonTests();
            validator.ResultShouldPassSanityChecks();

            // This is necessary b/c the System Test framework doesn't understand Rules and can't determine the number of expected tests
            ConfirmAllTestsRan(testName == "rules" ? 3 : null);
        }

        [TestMethod]
        [DataRow("ambiguous", "ambiguous")]
        [DataRow("background", "background")]
        // These tests are not (yet) within the CCK but are included here to round out the testing. The expected results were generated by the CucumberMessages plugin.
        // Once the CCK includes these scenarios, the expected results should come from the CCK repo.
        public void NonCCKScenarios(string testName, string featureNameText)
        {
            CCKScenarios(testName, featureNameText);
        }

        [TestMethod]
        [DataRow("attachments", "Attachments")]
        [DataRow("skipped", "Skipping scenarios")]
        [DataRow("unknown-parameter-type", "Unknown Parameter Types")]
        // These scenarios are from the CCK, but Reqnroll cannot provide a compliant implementation. This is usually the result of differences in behavior or support of Gherkin features.
        // When these scenarios are run, expect them to fail.
        public void NonCompliantCCKScenarios(string testName, string featureNameText)
        {
            CCKScenarios(testName, featureNameText);
        }

        private void AddUtilClassWithFileSystemPath()
        {
            string location = AppContext.BaseDirectory;
            AddBindingClass(
                $"public class FileSystemPath {{  public static string GetFilePathForAttachments()  {{  return @\"{location}\\CCK\"; }}  }} ");
        }

        private IEnumerable<Envelope> GetExpectedResults(string testName, string featureFileName)
        {
            var workingDirectory = Path.Combine(AppContext.BaseDirectory, "..\\..\\..");
            var expectedJsonText = File.ReadAllLines(Path.Combine(workingDirectory!, "CCK", $"{testName}\\{featureFileName}.feature.ndjson"));

            foreach (var json in expectedJsonText)
            {
                var e = NdjsonSerializer.Deserialize(json);
                yield return e;
            };
        }

        private IEnumerable<Envelope> GetActualResults(string testName, string fileName)
        {
            string resultLocation = ActualsResultLocationDirectory();

            // Hack: the file name is hard-coded in the test row data to match the name of the feature within the Feature file for the example scenario

            var actualJsonText = File.ReadAllLines(Path.Combine(resultLocation, $"{fileName}.ndjson"));

            foreach (var json in actualJsonText) yield return NdjsonSerializer.Deserialize(json);
        }

    }

}
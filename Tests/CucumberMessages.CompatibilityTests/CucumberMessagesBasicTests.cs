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
using Microsoft.VisualBasic.FileIO;
using Reqnroll.CucumberMessages.PayloadProcessing;
using Reqnroll.CommonModels;
using Reqnroll.CucumberMessages.Configuration;
using Reqnroll.TestProjectGenerator.Data;
using Reqnroll.CucumberMessages.PayloadProcessing.Cucumber;

namespace CucumberMessages.Tests
{
    [TestClass]
    public class CucumberMessagesBasicTests : CucumberCompatibilityTestBase
    {
        [TestMethod]
        public void PartialConfigurationIsCorrectlyHandled()
        {
            ResetCucumberMessages("reqnoll_report.ndjson");
            CucumberMessagesAddConfigurationFile("partialConfiguration.json");

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
        public void NullTest()
        {
            ResetCucumberMessages();
            // The purpose of this test is to confirm that when Cucumber Messages are turned off, the Cucumber Messages ecosystem does not cause any interference anywhere else
            DisableCucumberMessages();
            SetCucumberMessagesOutputFileName("null_test.ndjson");
            CucumberMessagesAddConfigurationFile("reqnroll.json");

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
            CucumberMessagesAddConfigurationFile("reqnroll.json");

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
        public void SmokeHookTest()
        {
            ResetCucumberMessages("smoke_hooks.ndjson"); 
            EnableCucumberMessages();
            CucumberMessagesAddConfigurationFile("reqnroll.json");
            SetCucumberMessagesOutputFileName("smoke_hooks.ndjson");

            AddFeatureFile("""
                Feature: Cucumber Messages Smoke Test
                  Scenario: Eating Cukes
                     When I eat 5 cukes
                """);
            AddPassingStepBinding("When");

            AddBindingClass("""
                using Reqnroll;
                using System;
                using System.Collections.Generic;
                using System.Linq;
                using System.Text;
                using System.Threading.Tasks;
                using System.Diagnostics;

                namespace CucumberMessages.CompatibilityTests.Smoke
                {
                    [Binding]
                    internal class Hooks
                    {
                        public Hooks()
                        {
                        }

                        // Hook implementations
                        [BeforeScenario]
                        public void BeforeScenarioHook() { }
                    }
                }
                
                """);

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
            CucumberMessagesAddConfigurationFile("reqnroll.json");

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
        public void SmokeTestMultipleFeaturesInParallel()
        {
            _projectsDriver.EnableTestParallelExecution();
            ResetCucumberMessages("SmokeTestMultipleFeatures.ndjson");
            EnableCucumberMessages();
            SetCucumberMessagesOutputFileName("SmokeTestMultipleFeatures.ndjson");
            CucumberMessagesAddConfigurationFile("reqnroll.json");

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
            CucumberMessagesAddConfigurationFile("reqnroll.json");

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
            CucumberMessagesAddConfigurationFile("reqnroll.json");

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
        public void CanGenerateVB()
        {
            _testRunConfiguration.ProgrammingLanguage = Reqnroll.TestProjectGenerator.ProgrammingLanguage.VB;
            ResetCucumberMessages("VB Generation Test.ndjson");
            EnableCucumberMessages();
            SetCucumberMessagesOutputFileName("VB Generation Test.ndjson");
            CucumberMessagesAddConfigurationFile("reqnroll.json");

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
        public void CucumberMessagesInteropWithExternalData()
        {
            ResetCucumberMessages("External Data from CSV file.ndjson");
            // The purpose of this test is to prove that the ScenarioOutline tables generated by the ExternalData plugin can be used in Cucumber messages
            EnableCucumberMessages();
            SetCucumberMessagesOutputFileName("External Data from CSV file.ndjson");
            _projectsDriver.AddNuGetPackage("Reqnroll.ExternalData", "2.1.1-local");
            CucumberMessagesAddConfigurationFile("reqnroll.json");

            // this test borrows a subset of a feature, the binding class, and the data file from ExternalData.ReqnrollPlugin.IntegrationTest
            var content = _testFileManager.GetTestFileContent("products.csv", "CucumberMessages.Tests", Assembly.GetExecutingAssembly());
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

                        [Given(@"the price of (.*) is €(.*)")]
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

                        [Then(@"the basket price should be €(.*)")]
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
        public void ExternalAssembliesAreSupportedInStepDefinitionMessages()
        {
            var featureNameText = "ExternalBinding";
            ResetCucumberMessages(featureNameText + ".ndjson");
            EnableCucumberMessages();
            SetCucumberMessagesOutputFileName(featureNameText + ".ndjson");
            
            // BECAUSE the following won't work (still investigating), will use a specially crafted reqnroll.json file
            // that already has the ExternalBindingsProject assembly mentioned. 
            // This should be a short-term hack.

            // set the Reqnroll configuration to include an external Assemblies setting
            //var _configurationFileDriver = GetServiceSafe<ConfigurationFileDriver>();
            //_configurationFileDriver.AddStepAssembly(new BindingAssembly("ExternalBindingsProject"));

            CucumberMessagesAddConfigurationFile("reqnrollConfigurationWithExternalAssembly.json");

            //Set up the Default Project (main test assembly)
            AddUtilClassWithFileSystemPath();

            AddFeatureFileFromResource($"ExternalBindingAssemblies/{featureNameText}.feature", "CucumberMessages.Tests.Samples", Assembly.GetExecutingAssembly());
            AddBindingClassFromResource($"ExternalBindingAssemblies/SampleInternalBindingClass.cs", "CucumberMessages.Tests.Samples", Assembly.GetExecutingAssembly());

            // fix problem with TestFolders data structure getting munged by adding a second project to the solution
            var testPath = _testProjectFolders.ProjectFolder;
            var compAssPath = _testProjectFolders.CompiledAssemblyPath;
            var projectBinOutputPath = _testProjectFolders.ProjectBinOutputPath;

            //setup an external project; add a binding class to it; and add it to the solution
            var externalProject = _projectsDriver.CreateProject("ExternalBindingsProject", "C#", ProjectType.Library);
            externalProject.IsReqnrollFeatureProject = false;
            var bindingCLassFileContent = _testFileManager.GetTestFileContent("SampleExternalBindingClass.cs", "CucumberMessages.Tests.Samples.ExternalBindingAssemblies", Assembly.GetExecutingAssembly());
            externalProject.AddBindingClass(bindingCLassFileContent); //add the binding 

            _projectsDriver.AddProjectReference("ExternalBindingsProject");
            // restoring values to the TestProjectFolders data structure
            _testProjectFolders.ProjectFolder = testPath;
            _testProjectFolders.ProjectBinOutputPath = projectBinOutputPath;
            _testProjectFolders.CompiledAssemblyPath = compAssPath;

            ExecuteTests();
            ShouldAllScenariosPass();

            var actualResults = GetActualResults("", featureNameText).ToList();
            var stepDefs = actualResults.Select(e => e.Content()).OfType<StepDefinition>();
            stepDefs.Any(sd => sd.SourceReference.JavaMethod.ClassName.Contains("SampleExternalBindingClass")).Should().BeTrue("StepDefinition for SampleExternalBindingClass should be found in the StepDefinitions");

        }
    }

}
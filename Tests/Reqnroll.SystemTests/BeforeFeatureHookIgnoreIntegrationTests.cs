using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using Reqnroll.SystemTests.Generation;
using Reqnroll.TestProjectGenerator;

namespace Reqnroll.SystemTests
{
    /// <summary>
    /// Integration tests to reproduce issue #568: Assert.Ignore in BeforeFeature hook gives System.NullReferenceException
    /// https://github.com/reqnroll/Reqnroll/issues/568
    /// 
    /// This test creates a real Reqnroll project with NUnit test framework and reproduces the scenario
    /// where a BeforeFeature hook calls Assert.Ignore, which should not cause NullReferenceException.
    /// </summary>
    [TestClass]
    public class BeforeFeatureHookIgnoreIntegrationTests : GenerationTestBase
    {
        [TestMethod]
        public void Should_handle_assert_ignore_in_before_feature_hook_without_null_reference_nunit()
        {
            // This test simulates the exact scenario from issue #568:
            // 1. NUnit test framework
            // 2. BeforeFeature hook that calls Assert.Ignore
            // 3. Verify no NullReferenceException occurs during teardown

            // Configure the test to use NUnit test framework
            _testRunConfiguration.UnitTestProvider = UnitTestProvider.NUnit3;

            // Create a feature file
            AddFeatureFile(
                """
                Feature: Calculator
                
                Scenario: Add two numbers
                    Given I have entered 50 into the calculator
                    When I press add
                    Then the result should be 50
                """);

            // Create step definitions
            AddBindingClass(
                """
                namespace Calculator.StepDefinitions
                {
                    [Binding]
                    public class CalculatorStepDefinitions
                    {
                        [Given("I have entered (.*) into the calculator")]
                        public void GivenIHaveEnteredIntoTheCalculator(int number)
                        {
                            // Simple step implementation
                        }
                        
                        [When("I press add")]
                        public void WhenIPressAdd()
                        {
                            // Simple step implementation
                        }
                        
                        [Then("the result should be (.*)")]
                        public void ThenTheResultShouldBe(int expectedResult)
                        {
                            // Simple step implementation
                        }
                    }
                }
                """);

            // Create hooks class with BeforeFeature hook that calls Assert.Ignore
            AddBindingClass(
                """
                using NUnit.Framework;
                
                namespace Calculator.Hooks
                {
                    [Binding]
                    public class CalculatorHooks
                    {
                        [BeforeFeature("Calculator")]
                        public static void BeforeFeatureToIgnore()
                        {
                            Assert.Ignore("for testing");
                        }
                    }
                }
                """);

            // Execute the tests
            ExecuteTests();

            // The key assertion: The test execution should complete without throwing a NullReferenceException
            // The original issue was that during teardown, a NullReferenceException would be thrown
            // With the fix from PR #560, this should be handled gracefully
            
            // We expect the test to be ignored (not passed), but it should not crash with NullReferenceException
            _vsTestExecutionDriver.LastTestExecutionResult.Output.Should().NotContain("NullReferenceException");
            _vsTestExecutionDriver.LastTestExecutionResult.Output.Should().NotContain("The previous ScenarioContext was already disposed.");
            
            // The test should be marked as ignored/skipped, not failed due to infrastructure errors
            _vsTestExecutionDriver.LastTestExecutionResult.Ignored.Should().BeGreaterThan(0, "Test should be ignored due to Assert.Ignore in BeforeFeature hook");
        }

        [TestMethod]
        public void Should_handle_pending_step_exception_in_before_feature_hook_without_null_reference()
        {
            // This test simulates the MSTest scenario mentioned in the issue comments:
            // FermJacob reported similar issues with PendingStepException in MSTest

            // Create a feature file
            AddFeatureFile(
                """
                Feature: Calculator MSTest
                
                Scenario: Add two numbers with MSTest
                    Given I have entered 50 into the calculator
                    When I press add
                    Then the result should be 50
                """);

            // Create step definitions with a pending step
            AddBindingClass(
                """
                namespace Calculator.StepDefinitions
                {
                    [Binding]
                    public class CalculatorStepDefinitions
                    {
                        [Given("I have entered (.*) into the calculator")]
                        public void GivenIHaveEnteredIntoTheCalculator(int number)
                        {
                            throw new PendingStepException();
                        }
                        
                        [When("I press add")]
                        public void WhenIPressAdd()
                        {
                            // Simple step implementation
                        }
                        
                        [Then("the result should be (.*)")]
                        public void ThenTheResultShouldBe(int expectedResult)
                        {
                            // Simple step implementation
                        }
                    }
                }
                """);

            // Create hooks class with BeforeFeature hook that throws PendingStepException
            AddBindingClass(
                """
                namespace Calculator.Hooks
                {
                    [Binding]
                    public class CalculatorHooks
                    {
                        [BeforeFeature]
                        public static void BeforeFeatureToIgnore()
                        {
                            throw new PendingStepException("Feature is pending");
                        }
                    }
                }
                """);

            // Execute the tests
            ExecuteTests();

            // The key assertion: The test execution should complete without throwing a NullReferenceException
            _vsTestExecutionDriver.LastTestExecutionResult.Output.Should().NotContain("NullReferenceException");
            _vsTestExecutionDriver.LastTestExecutionResult.Output.Should().NotContain("The previous ScenarioContext was already disposed.");
            
            // The test should fail due to PendingStepException, but not crash with infrastructure errors
            _vsTestExecutionDriver.LastTestExecutionResult.Failed.Should().BeGreaterThan(0, "Test should fail due to PendingStepException in BeforeFeature hook");
        }
    }
}
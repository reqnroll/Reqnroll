using System;
using Reqnroll.ErrorHandling;

namespace Reqnroll.UnitTestProvider;

/// <summary>
/// Runtime services provided by the test execution framework.
/// </summary>
public interface IUnitTestRuntimeProvider
{
    /// <summary>
    /// Forces the test to be pending. When the provider does not support pending tests, this method should throw an <see cref="PendingScenarioException"/>.
    /// </summary>
    /// <param name="message">The message to be included in the test result.</param>
    void TestPending(string message);

    /// <summary>
    /// Forces the test to be inconclusive. When the provider does not support inconclusive tests it can also delegate to <see cref="TestIgnore"/>.
    /// </summary>
    /// <param name="message">The message to be included in the test result.</param>
    void TestInconclusive(string message);

    /// <summary>
    /// Forces the test to be ignored. When the provider does not support ignored tests it can also delegate to <see cref="TestInconclusive"/>.
    /// </summary>
    /// <param name="message">The message to be included in the test result.</param>
    void TestIgnore(string message);

    /// <summary>
    /// Detects dynamically skipped or pending tests based on the exception type.
    /// Should return <see cref="ScenarioExecutionStatus.Skipped"/>, if the exception represents a skipped tests (e.g. ignored or inconclusive); <see cref="ScenarioExecutionStatus.StepDefinitionPending"/> when the exception represents a pending state and <c>null</c> in all other cases.
    /// </summary>
    /// <param name="exception">The exception that has been thrown during the step execution.</param>
    /// <returns>The detected <see cref="ScenarioExecutionStatus"/> or <c>null</c> in all other cases.</returns>
    ScenarioExecutionStatus? DetectExecutionStatus(Exception exception);
}

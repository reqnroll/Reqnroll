using System;
using System.Collections.Generic;
using FluentAssertions;
using Gherkin.CucumberMessages;
using Moq;
using Reqnroll.Bindings;
using Reqnroll.Bindings.Reflection;
using Reqnroll.Formatters.ExecutionTracking;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;
using Reqnroll.Events;
using Reqnroll.Infrastructure;
using Xunit;
using System.Collections.ObjectModel;
using Reqnroll.Formatters.PubSub;

namespace Reqnroll.RuntimeTests.Formatters.ExecutionTracking;

public class TestStepTrackerTests
{
    private readonly Mock<ICucumberMessageFactory> _messageFactoryMock = new();
    private readonly Mock<ITestCaseExecutionTrackerFactory> _testCaseExecutionTrackerFactoryMock = new();
    private readonly Mock<IMessagePublisher> _publishMessageMock = new();
    private readonly Mock<IIdGenerator> _idGeneratorMock = new();
    private readonly TestCaseTracker _testCaseTracker;
    private readonly PickleExecutionTracker _pickleExecutionTracker;
    public TestStepTrackerTests()
    {

        _pickleExecutionTracker = new PickleExecutionTracker("testCasePickle", "runStartedId", "featureName", true, _idGeneratorMock.Object, null, DateTime.Now, _messageFactoryMock.Object, _testCaseExecutionTrackerFactoryMock.Object, _publishMessageMock.Object);
        _testCaseTracker = new TestCaseTracker("testCaseId", "testCasePickle", _pickleExecutionTracker);
    }

    [Fact]
    public void PopulateStepDefinitionFromExecutionResult_Should_Set_Bound_And_Arguments_When_Bound()
    {
        // Arrange
        var methodMock = SetupMockMethodWithParameters([("arg1", typeof(string)), ("arg2", typeof(string))]);
        var stepBindingMock = new Mock<IStepDefinitionBinding>();
        stepBindingMock.Setup(b => b.StepDefinitionType).Returns(StepDefinitionType.Given);
        stepBindingMock.Setup(b => b.Method).Returns(methodMock.Object);

        var stepInfo = new StepInfo(StepDefinitionType.Given, "step text", null, null, "stepPickleId")
        {
            BindingMatch = new BindingMatch(
                stepBindingMock.Object, // stepBinding
                0,                     // scopeMatches
                [
                    new MatchArgument("arg1", 1 ),
                    new MatchArgument("arg2", 2 )
                ],
                null                   // stepContext
            )
        };
        var stepContextMock = new Mock<IScenarioStepContext>();
        stepContextMock.SetupGet(x => x.StepInfo).Returns(stepInfo);
        stepContextMock.SetupGet(x => x.Status).Returns(ScenarioExecutionStatus.OK);

        _testCaseTracker.Steps.Add(new TestStepTracker("stepDefId", "stepPickleId", _testCaseTracker));
        _pickleExecutionTracker.StepDefinitionsByBinding = new ReadOnlyDictionary<IBinding, string>(
            new Dictionary<IBinding, string> { { stepBindingMock.Object, "stepPickleId" } });
                
        var evt = new StepFinishedEvent(null, null, stepContextMock.Object);

        var def = new TestStepTracker("stepDefId", "stepPickleId", _testCaseTracker);

        // Act
        def.ProcessEvent(evt);

        // Assert
        def.IsBound.Should().BeTrue();
        def.StepArgumentsLists.Should().HaveCount(1);
        var argumentList = def.StepArgumentsLists[0];
        argumentList.Should().HaveCount(2);
    }

    [Fact]
    public void PopulateStepDefinitionFromExecutionResult_Should_Set_Bound_Arguments_When_Step_Uses_A_DocString()
    {
        // Arrange
        var methodMock = SetupMockMethodWithParameters([("arg1", typeof(int)), ("arg2", typeof(string))]);
        var stepBindingMock = new Mock<IStepDefinitionBinding>();
        stepBindingMock.Setup(b => b.StepDefinitionType).Returns(StepDefinitionType.Given);
        stepBindingMock.Setup(b => b.Method).Returns(methodMock.Object);

        var multilineText =
            """
            This is a multiline
            text argument used in a step.
            """;
        var stepInfo = new StepInfo(StepDefinitionType.Given, "step {int} text", null, multilineText , "stepPickleId")
        {
            BindingMatch = new BindingMatch(
                stepBindingMock.Object, // stepBinding
                0,                     // scopeMatches
                [
                    new MatchArgument(53, 1 ),
                    new MatchArgument(multilineText, 2 )
                ],
                null                   // stepContext
            )
        };
        var stepContextMock = new Mock<IScenarioStepContext>();
        stepContextMock.SetupGet(x => x.StepInfo).Returns(stepInfo);
        stepContextMock.SetupGet(x => x.Status).Returns(ScenarioExecutionStatus.OK);

        _testCaseTracker.Steps.Add(new TestStepTracker("stepDefId", "stepPickleId", _testCaseTracker));
        _pickleExecutionTracker.StepDefinitionsByBinding = new ReadOnlyDictionary<IBinding, string>(
            new Dictionary<IBinding, string> { { stepBindingMock.Object, "stepPickleId" } });

        var evt = new StepFinishedEvent(null, null, stepContextMock.Object);

        var def = new TestStepTracker("stepDefId", "stepPickleId", _testCaseTracker);

        // Act
        def.ProcessEvent(evt);

        // Assert
        def.IsBound.Should().BeTrue();
        def.StepArgumentsLists.Should().HaveCount(1);
    }

    private Mock<IBindingMethod> SetupMockMethodWithParameters(IEnumerable<(string, Type)> paramList)
    {
        var methodMock = new Mock<IBindingMethod>();
        var pList = new List<IBindingParameter>();
        foreach (var (name, t) in paramList)
        {
            var p = new BindingParameter(new RuntimeBindingType(t), name);
            pList.Add(p);
        }
        methodMock.Setup(m => m.Parameters).Returns(pList);
        return methodMock;
    }

    [Fact]
    public void PopulateStepDefinitionFromExecutionResult_Should_Set_Bound_False_When_Not_Bound()
    {
        // Arrange
        var stepInfo = new StepInfo(
            StepDefinitionType.Given, // stepDefinitionType
            "Step Text",              // text
            null,                     // table
            null,                     // multilineText
            "pickleStepId"            // pickleStepId
        )
        {
            BindingMatch = BindingMatch.NonMatching
        };
        var stepContextMock = new Mock<IScenarioStepContext>();
        stepContextMock.SetupGet(x => x.StepInfo).Returns(stepInfo);
        stepContextMock.SetupGet(x => x.Status).Returns(ScenarioExecutionStatus.OK);

        var evt = new StepFinishedEvent(null, null, stepContextMock.Object);

        var def = new TestStepTracker("stepDefId", "pickleStepId", _testCaseTracker);

        // Act
        def.ProcessEvent(evt);

        // Assert
        def.IsBound.Should().BeFalse();
        def.StepArgumentsLists.Should().BeEmpty();
        def.StepDefinitionIds.Should().BeNullOrEmpty();
    }

    [Fact]
    public void PopulateStepDefinitionFromExecutionResult_Should_Set_Ambiguous_When_AmbiguousBindingException()
    {
        // Arrange
        var methodMock = SetupMockMethodWithParameters([("arg1", typeof(string)), ("arg2", typeof(string))]);
        var stepBindingMock = new Mock<IStepDefinitionBinding>();
        stepBindingMock.Setup(b => b.StepDefinitionType).Returns(StepDefinitionType.Given);
        stepBindingMock.Setup(b => b.Method).Returns(methodMock.Object);
        var bindingMatch = new BindingMatch(
            stepBindingMock.Object, // stepBinding
            0,                     // scopeMatches
            [
                new MatchArgument("arg1", 1 ),
                new MatchArgument("arg2", 2 )
            ],
            null                   // stepContext
        );

        var ambiguousMatchException = new AmbiguousBindingException("error message", 
                                                                    new List<BindingMatch> { 
                                                                        bindingMatch,
                                                                        bindingMatch
                                                                    }
        );

        var stepInfo = new StepInfo(
            StepDefinitionType.Given, // stepDefinitionType
            "Step Text",              // text
            null,                     // table
            null,                     // multilineText
            "pickleStepId"            // pickleStepId
        )

        {
            BindingMatch = BindingMatch.NonMatching
        };
        var scenarioContextMock = new Mock<IScenarioContext>();
        scenarioContextMock.SetupGet(x => x.TestError).Returns(ambiguousMatchException);

        var stepContextMock = new Mock<IScenarioStepContext>();
        stepContextMock.SetupGet(x => x.StepInfo).Returns(stepInfo);
        stepContextMock.SetupGet(x => x.Status).Returns(ScenarioExecutionStatus.BindingError);

        //_messageFactoryMock.Setup(f => f.CanonicalizeStepDefinitionPattern(It.IsAny<IStepDefinitionBinding>()))
        //    .Returns("ambiguousPattern");
        _testCaseTracker.Steps.Add(new TestStepTracker("ambiguousId", "pickleStepId", _testCaseTracker));
        _pickleExecutionTracker.StepDefinitionsByBinding = new ReadOnlyDictionary<IBinding, string>(
            new Dictionary<IBinding, string> { { stepBindingMock.Object, "pickleStepId" } });

        var evt = new StepFinishedEvent(null, scenarioContextMock.Object, stepContextMock.Object);

        var def = new TestStepTracker("stepDefId", "pickleStepId", _testCaseTracker);

        // Act
        def.ProcessEvent(evt);

        // Assert
        def.IsAmbiguous.Should().BeTrue();
        def.StepDefinitionIds.Should().NotBeNull();
        def.StepDefinitionIds.Should().Contain("pickleStepId");
    }

}
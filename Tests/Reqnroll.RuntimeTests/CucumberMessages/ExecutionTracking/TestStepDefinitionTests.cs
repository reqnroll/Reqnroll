using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Reqnroll.RuntimeTests.CucumberMessages.ExecutionTracking
{
    public class TestStepDefinitionTests
    {
        private readonly Mock<ICucumberMessageFactory> _messageFactoryMock = new();
        private readonly Mock<IIdGenerator> _idGeneratorMock = new();
        private readonly TestCaseDefinition _testCaseDefinition;
        private readonly TestCaseTracker _testCaseTracker;
        public TestStepDefinitionTests()
        {

            _testCaseTracker = new TestCaseTracker("testCasePickle", "runStartedId", "featureName", true, _idGeneratorMock.Object, new(), DateTime.Now, _messageFactoryMock.Object);
            _testCaseDefinition = new TestCaseDefinition("testCaseId", "testCasePickle", _testCaseTracker);
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
                    new MatchArgument[]    // arguments
                    {
                        new MatchArgument("arg1", 1 ),
                        new MatchArgument("arg2", 2 )
                    },
                    null                   // stepContext
                )
            };
            var stepContextMock = new Mock<IScenarioStepContext>();
            stepContextMock.SetupGet(x => x.StepInfo).Returns(stepInfo);
            stepContextMock.SetupGet(x => x.Status).Returns(ScenarioExecutionStatus.OK);

            _messageFactoryMock.Setup(f => f.CanonicalizeStepDefinitionPattern(It.IsAny<IStepDefinitionBinding>()))
                .Returns("methodSignature");
            _testCaseDefinition.AddStepDefinition(new TestStepDefinition("stepDefId", "stepPickleId", _testCaseDefinition, _messageFactoryMock.Object));
            _testCaseTracker.StepDefinitionsByMethodSignature.TryAdd("methodSignature", "stepPickleId");

            var evt = new StepFinishedEvent(null, null, stepContextMock.Object);

            var def = new TestStepDefinition("stepDefId", "stepPickleId", _testCaseDefinition, _messageFactoryMock.Object);

            // Act
            def.PopulateStepDefinitionFromExecutionResult(evt);

            // Assert
            def.Bound.Should().BeTrue();
            def.StepArguments.Should().HaveCount(2);
        }

        private Mock<IBindingMethod> SetupMockMethodWithParameters(IEnumerable<(string, Type)> paramList)
        {
            var m = new Mock<IBindingMethod>();
            var pList = new List<IBindingParameter>();
            foreach (var (name, t) in paramList)
            {
                var p = new BindingParameter(new RuntimeBindingType(t), name);
                pList.Add(p);
            }
            m.Setup(m => m.Parameters).Returns(pList);
            return m;
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

            var def = new TestStepDefinition("stepDefId", "pickleStepId", _testCaseDefinition, _messageFactoryMock.Object);

            // Act
            def.PopulateStepDefinitionFromExecutionResult(evt);

            // Assert
            def.Bound.Should().BeFalse();
            def.StepArguments.Should().BeEmpty();
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
                    new MatchArgument[]    // arguments
                    {
                        new MatchArgument("arg1", 1 ),
                        new MatchArgument("arg2", 2 )
                    },
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
            stepContextMock.SetupGet(x => x.Status).Returns(ScenarioExecutionStatus.TestError);

            _messageFactoryMock.Setup(f => f.CanonicalizeStepDefinitionPattern(It.IsAny<IStepDefinitionBinding>()))
                .Returns("ambiguousPattern");
            _testCaseDefinition.AddStepDefinition(new TestStepDefinition("ambiguousId", "pickleStepId", _testCaseDefinition, _messageFactoryMock.Object));
            _testCaseTracker.StepDefinitionsByMethodSignature.TryAdd("ambiguousPattern", "pickleStepId");

            var evt = new StepFinishedEvent(null, scenarioContextMock.Object, stepContextMock.Object);

            var def = new TestStepDefinition("stepDefId", "pickleStepId", _testCaseDefinition, _messageFactoryMock.Object);

            // Act
            def.PopulateStepDefinitionFromExecutionResult(evt);

            // Assert
            def.Ambiguous.Should().BeTrue();
            def.StepDefinitionIds.Should().NotBeNull();
            def.StepDefinitionIds.Should().Contain("pickleStepId");
        }

    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using Moq;
using Reqnroll.BindingSkeletons;
using Reqnroll.Bindings;
using Reqnroll.Bindings.Reflection;
using Reqnroll.Configuration;
using Reqnroll.EnvironmentAccess;
using Reqnroll.Infrastructure;
using Reqnroll.Tracing;
using Reqnroll.Tracing.AnsiColor;
using Xunit;

namespace Reqnroll.RuntimeTests.Tracing
{
    public class TestTracerTraceLevelTests
    {
        private readonly Mock<ITraceListener> _traceListenerMock = new();
        private readonly Mock<IStepFormatter> _stepFormatterMock = new();
        private readonly Mock<IStepDefinitionSkeletonProvider> _skeletonProviderMock = new();
        private readonly Mock<IColorOutputTheme> _colorThemeMock = new();
        private readonly Mock<IColorOutputHelper> _colorHelperMock = new();
        private readonly Mock<IEnvironmentOptions> _environmentOptionsMock = new();

        private TestTracer CreateTracer(TraceLevel configLevel, TraceLevel? envOverride = null)
        {
            var config = ConfigurationLoader.GetDefault();
            config.TraceLevel = configLevel;

            _environmentOptionsMock.Setup(e => e.TraceLevel).Returns(envOverride);
            _stepFormatterMock.Setup(f => f.GetStepText(It.IsAny<StepInstance>())).Returns("Given a step");
            _stepFormatterMock.Setup(f => f.GetMatchText(It.IsAny<BindingMatch>(), It.IsAny<object[]>())).Returns("StepClass.Method()");
            _stepFormatterMock.Setup(f => f.GetMatchText(It.IsAny<IBindingMethod>(), It.IsAny<object[]>())).Returns("StepClass.Method()");
            _colorHelperMock.Setup(c => c.Colorize(It.IsAny<string>(), It.IsAny<AnsiColor>())).Returns<string, AnsiColor>((text, _) => text);

            return new TestTracer(
                _traceListenerMock.Object,
                _stepFormatterMock.Object,
                _skeletonProviderMock.Object,
                config,
                _colorThemeMock.Object,
                _colorHelperMock.Object,
                _environmentOptionsMock.Object);
        }

        private static BindingMatch CreateBindingMatch()
        {
            var methodMock = new Mock<IBindingMethod>();
            var stepDefMock = new Mock<IStepDefinitionBinding>();
            stepDefMock.Setup(sd => sd.Method).Returns(methodMock.Object);

            return new BindingMatch(stepDefMock.Object, 0, Array.Empty<MatchArgument>(),
                new StepContext("feature", "scenario", new List<string>(), CultureInfo.InvariantCulture));
        }

        // === TraceLevel.None: nothing should be output ===

        [Fact]
        public void TraceLevel_None_SuppressesAllOutput()
        {
            var tracer = CreateTracer(TraceLevel.None);

            tracer.TraceStep(new StepInstance(StepDefinitionType.Given, StepDefinitionKeyword.Given, "Given", "a step", null, null, null), true);
            tracer.TraceWarning("test warning");
            tracer.TraceStepDone(CreateBindingMatch(), Array.Empty<object>(), TimeSpan.FromSeconds(1));
            tracer.TraceStepSkipped(new Exception("skipped"));
            tracer.TraceStepSkippedBecauseOfPreviousErrors();
            tracer.TraceStepPending(CreateBindingMatch(), Array.Empty<object>(), new PendingStepException());
            tracer.TraceBindingError(new Exception("binding error"));
            tracer.TraceError(new Exception("error"), TimeSpan.FromSeconds(1));
            tracer.TraceDuration(TimeSpan.FromSeconds(1), "test");

            _traceListenerMock.Verify(t => t.WriteTestOutput(It.IsAny<string>()), Times.Never);
            _traceListenerMock.Verify(t => t.WriteToolOutput(It.IsAny<string>()), Times.Never);
        }

        // === TraceLevel.Minimal: errors, warnings, pending, undefined ===

        [Fact]
        public void TraceLevel_Minimal_ShowsWarning()
        {
            var tracer = CreateTracer(TraceLevel.Minimal);
            tracer.TraceWarning("test warning");
            _traceListenerMock.Verify(t => t.WriteToolOutput(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void TraceLevel_Minimal_ShowsError()
        {
            var tracer = CreateTracer(TraceLevel.Minimal);
            tracer.TraceError(new Exception("error"), TimeSpan.FromSeconds(1));
            _traceListenerMock.Verify(t => t.WriteToolOutput(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public void TraceLevel_Minimal_ShowsBindingError()
        {
            var tracer = CreateTracer(TraceLevel.Minimal);
            tracer.TraceBindingError(new Exception("binding error"));
            _traceListenerMock.Verify(t => t.WriteToolOutput(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void TraceLevel_Minimal_ShowsStepPending()
        {
            var tracer = CreateTracer(TraceLevel.Minimal);
            tracer.TraceStepPending(CreateBindingMatch(), Array.Empty<object>(), new PendingStepException());
            _traceListenerMock.Verify(t => t.WriteToolOutput(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void TraceLevel_Minimal_SuppressesStepText()
        {
            var tracer = CreateTracer(TraceLevel.Minimal);
            tracer.TraceStep(new StepInstance(StepDefinitionType.Given, StepDefinitionKeyword.Given, "Given", "a step", null, null, null), true);
            _traceListenerMock.Verify(t => t.WriteTestOutput(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void TraceLevel_Minimal_SuppressesStepDone()
        {
            var tracer = CreateTracer(TraceLevel.Minimal);
            tracer.TraceStepDone(CreateBindingMatch(), Array.Empty<object>(), TimeSpan.FromSeconds(1));
            _traceListenerMock.Verify(t => t.WriteToolOutput(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void TraceLevel_Minimal_SuppressesDuration()
        {
            var tracer = CreateTracer(TraceLevel.Minimal);
            tracer.TraceDuration(TimeSpan.FromSeconds(1), "test");
            _traceListenerMock.Verify(t => t.WriteToolOutput(It.IsAny<string>()), Times.Never);
        }

        // === TraceLevel.Normal: adds step text + skip info ===

        [Fact]
        public void TraceLevel_Normal_ShowsStepText()
        {
            var tracer = CreateTracer(TraceLevel.Normal);
            tracer.TraceStep(new StepInstance(StepDefinitionType.Given, StepDefinitionKeyword.Given, "Given", "a step", null, null, null), true);
            _traceListenerMock.Verify(t => t.WriteTestOutput(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void TraceLevel_Normal_ShowsStepSkipped()
        {
            var tracer = CreateTracer(TraceLevel.Normal);
            tracer.TraceStepSkipped(new Exception("skipped"));
            _traceListenerMock.Verify(t => t.WriteToolOutput(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void TraceLevel_Normal_ShowsStepSkippedBecauseOfPreviousErrors()
        {
            var tracer = CreateTracer(TraceLevel.Normal);
            tracer.TraceStepSkippedBecauseOfPreviousErrors();
            _traceListenerMock.Verify(t => t.WriteToolOutput(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void TraceLevel_Normal_SuppressesStepDone()
        {
            var tracer = CreateTracer(TraceLevel.Normal);
            tracer.TraceStepDone(CreateBindingMatch(), Array.Empty<object>(), TimeSpan.FromSeconds(1));
            _traceListenerMock.Verify(t => t.WriteToolOutput(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void TraceLevel_Normal_SuppressesDuration()
        {
            var tracer = CreateTracer(TraceLevel.Normal);
            tracer.TraceDuration(TimeSpan.FromSeconds(1), "test");
            _traceListenerMock.Verify(t => t.WriteToolOutput(It.IsAny<string>()), Times.Never);
        }

        // === TraceLevel.Detailed: adds step done ===

        [Fact]
        public void TraceLevel_Detailed_ShowsStepDone()
        {
            var tracer = CreateTracer(TraceLevel.Detailed);
            tracer.TraceStepDone(CreateBindingMatch(), Array.Empty<object>(), TimeSpan.FromSeconds(1));
            _traceListenerMock.Verify(t => t.WriteToolOutput(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void TraceLevel_Detailed_SuppressesDuration()
        {
            var tracer = CreateTracer(TraceLevel.Detailed);
            tracer.TraceDuration(TimeSpan.FromSeconds(1), "test");
            _traceListenerMock.Verify(t => t.WriteToolOutput(It.IsAny<string>()), Times.Never);
        }

        // === TraceLevel.Diagnostic: shows everything ===

        [Fact]
        public void TraceLevel_Diagnostic_ShowsDuration()
        {
            var tracer = CreateTracer(TraceLevel.Diagnostic);
            tracer.TraceDuration(TimeSpan.FromSeconds(1), "test");
            _traceListenerMock.Verify(t => t.WriteToolOutput(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void TraceLevel_Diagnostic_ShowsDurationWithMethod()
        {
            var tracer = CreateTracer(TraceLevel.Diagnostic);
            var methodMock = new Mock<IBindingMethod>();
            tracer.TraceDuration(TimeSpan.FromSeconds(1), methodMock.Object, Array.Empty<object>());
            _traceListenerMock.Verify(t => t.WriteToolOutput(It.IsAny<string>()), Times.Once);
        }
        
        // === Environment variable override ===

        [Fact]
        public void EnvironmentVariable_OverridesConfig()
        {
            // Config says Diagnostic, but env var says None
            var tracer = CreateTracer(TraceLevel.Diagnostic, envOverride: TraceLevel.None);

            tracer.TraceWarning("test warning");
            tracer.TraceStep(new StepInstance(StepDefinitionType.Given, StepDefinitionKeyword.Given, "Given", "a step", null, null, null), true);

            _traceListenerMock.Verify(t => t.WriteTestOutput(It.IsAny<string>()), Times.Never);
            _traceListenerMock.Verify(t => t.WriteToolOutput(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void EnvironmentVariable_Null_UsesConfigLevel()
        {
            // Config says Minimal, env var not set (null)
            var tracer = CreateTracer(TraceLevel.Minimal, envOverride: null);

            tracer.TraceWarning("test warning");
            _traceListenerMock.Verify(t => t.WriteToolOutput(It.IsAny<string>()), Times.Once);

            tracer.TraceStep(new StepInstance(StepDefinitionType.Given, StepDefinitionKeyword.Given, "Given", "a step", null, null, null), true);
            _traceListenerMock.Verify(t => t.WriteTestOutput(It.IsAny<string>()), Times.Never);
        }
    }
}

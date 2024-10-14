using Io.Cucumber.Messages.Types;
using System;
using System.Collections.Generic;

namespace Reqnroll.CucumberMessages.PayloadProcessing.Cucumber
{
    public static class CucumberMessageExtensions
    {
        public static List<Type> MessagesWithIds = new(){   typeof(Background),
                                                            typeof(Examples),
                                                            typeof(Hook),
                                                            typeof(ParameterType),
                                                            typeof(Pickle),
                                                            typeof(PickleStep),
                                                            typeof(Rule),
                                                            typeof(Scenario),
                                                            typeof(Step),
                                                            typeof(StepDefinition),
                                                            typeof(TableRow),
                                                            typeof(Tag),
                                                            typeof(TestCase),
                                                            typeof(TestCaseStarted),
                                                            typeof(TestStep)
        };

        public static bool HasId(this object element)
        {
            return MessagesWithIds.Contains(element.GetType());
        }
        public static string Id(this object message)
        {
            return message switch
            {
                Background bgd => bgd.Id,
                Examples ex => ex.Id,
                Hook hook => hook.Id,
                ParameterType pt => pt.Id,
                Pickle p => p.Id,
                PickleStep ps => ps.Id,
                Rule r => r.Id,
                Scenario sc => sc.Id,
                Step st => st.Id,
                StepDefinition sd => sd.Id,
                TableRow tr => tr.Id,
                Tag tag => tag.Id,
                TestCase tc => tc.Id,
                TestCaseStarted tcs => tcs.Id,
                TestStep ts => ts.Id,
                _ => throw new ArgumentException($"Message of type: {message.GetType()} has no ID")
            };
        }
        public static List<Type> EnvelopeContentTypes = new()
        {
            typeof(Attachment),
            typeof(GherkinDocument),
            typeof(Hook),
            typeof(Meta),
            typeof(ParameterType),
            typeof(ParseError),
            typeof(Pickle),
            typeof(Source),
            typeof(StepDefinition),
            typeof(TestCase),
            typeof(TestCaseFinished),
            typeof(TestCaseStarted),
            typeof(TestRunFinished),
            typeof(TestRunStarted),
            typeof(TestStepFinished),
            typeof(TestStepStarted),
            typeof(UndefinedParameterType)
        };
        public static object Content(this Envelope envelope)
        {
            object result = null;
            if (envelope.Attachment != null) { result = envelope.Attachment; }
            else if (envelope.GherkinDocument != null) { result = envelope.GherkinDocument; }
            else if (envelope.Hook != null) { result = envelope.Hook; }
            else if (envelope.Meta != null) { result = envelope.Meta; }
            else if (envelope.ParameterType != null) { result = envelope.ParameterType; }
            else if (envelope.ParseError != null) { result = envelope.ParseError; }
            else if (envelope.Pickle != null) { result = envelope.Pickle; }
            else if (envelope.Source != null) { result = envelope.Source; }
            else if (envelope.StepDefinition != null) { result = envelope.StepDefinition; }
            else if (envelope.TestCase != null) { result = envelope.TestCase; }
            else if (envelope.TestCaseFinished != null) { result = envelope.TestCaseFinished; }
            else if (envelope.TestCaseStarted != null) { result = envelope.TestCaseStarted; }
            else if (envelope.TestRunFinished != null) { result = envelope.TestRunFinished; }
            else if (envelope.TestRunStarted != null) { result = envelope.TestRunStarted; }
            else if (envelope.TestStepFinished != null) { result = envelope.TestStepFinished; }
            else if (envelope.TestStepStarted != null) { result = envelope.TestStepStarted; }
            else if (envelope.UndefinedParameterType != null) { result = envelope.UndefinedParameterType; }
            return result;
        }
    }
}
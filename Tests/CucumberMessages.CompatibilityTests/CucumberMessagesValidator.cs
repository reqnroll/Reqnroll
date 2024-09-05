using FluentAssertions;
using Io.Cucumber.Messages.Types;
using System.Diagnostics.Eventing.Reader;

namespace CucumberMessages.CompatibilityTests
{
    internal class CucumberMessagesValidator
    {
        private IEnumerable<Envelope> actualEnvelopes;
        private IEnumerable<Envelope> expectedEnvelopes;

        // cross-reference metadata
        private Dictionary<Type, HashSet<string>> actuals_IDsByType = new();
        private Dictionary<Type, HashSet<string>> expecteds_IDsByType = new();
        private Dictionary<Type, HashSet<object>> actuals_elementsByType = new();
        private Dictionary<Type, HashSet<object>> expecteds_elementsByType = new();
        private Dictionary<string, HashSet<object>> actuals_elementsByID = new();
        private Dictionary<string, HashSet<object>> expecteds_elementsByID = new();

        public CucumberMessagesValidator(IEnumerable<Envelope> actual, IEnumerable<Envelope> expected)
        {
            actualEnvelopes = actual;
            expectedEnvelopes = expected;

            SetupCrossReferences(actual, actuals_IDsByType, actuals_elementsByType, actuals_elementsByID);
            SetupCrossReferences(expected, expecteds_IDsByType, expecteds_elementsByType, expecteds_elementsByID);

        }
        private void SetupCrossReferences(IEnumerable<Envelope> messages, Dictionary<Type, HashSet<string>> IDsByType, Dictionary<Type, HashSet<object>> elementsByType, Dictionary<string, HashSet<object>> elementsByID)
        {
            foreach (var message in messages)
            {
                var msg = message.Content();
                InsertIntoElementsByType(msg, elementsByType);

                if (msg.HasId())
                {
                    InsertIntoElementsById(msg, elementsByID);
                    InsertIntoIDsByType(msg, IDsByType);
                }
            }
        }
        private static void InsertIntoIDsByType(object msg, Dictionary<Type, HashSet<string>> IDsByType)
        {
            if (!IDsByType.ContainsKey(msg.GetType()))
            {
                IDsByType.Add(msg.GetType(), new HashSet<string>());
            }
            IDsByType[msg.GetType()].Add(msg.Id());
        }

        private static void InsertIntoElementsById(object msg, Dictionary<string, HashSet<object>> elementsByID)
        {
            if (!elementsByID.ContainsKey(msg.Id()))
            {
                elementsByID.Add(msg.Id(), new HashSet<object>());
            }
            elementsByID[msg.Id()].Add(msg);
        }

        private static void InsertIntoElementsByType(object msg, Dictionary<Type, HashSet<object>> elementsByType)
        {
            if (!elementsByType.ContainsKey(msg.GetType()))
            {
                elementsByType.Add(msg.GetType(), new HashSet<object>());
            }
            elementsByType[msg.GetType()].Add(msg);
        }

        internal void ResultShouldPassAllComparisonTests()
        {
            ShouldPassBasicStructuralChecks(actualEnvelopes, actualEnvelopes);
        }

        internal void ResultShouldPassBasicSanityChecks()
        {
            throw new NotImplementedException();
        }
        internal void ShouldPassBasicStructuralChecks(IEnumerable<Envelope> actual, IEnumerable<Envelope> expected)
        {
            actual.Count().Should().BeGreaterThanOrEqualTo(expected.Count());

            //todo: modify this to use lists of types from actual and expected and use FluentAssertions directly
            foreach (var messageType in MessageExtensions.EnvelopeContentTypes)
            {
                if ( actuals_elementsByType.ContainsKey(messageType) && !expecteds_elementsByType.ContainsKey(messageType))
                {
                    throw new System.Exception($"{messageType} present in the actual but not in the expected.");
                }
                if (!actuals_elementsByType.ContainsKey(messageType) && expecteds_elementsByType.ContainsKey(messageType))
                {
                    throw new System.Exception($"{messageType} present in the expected but not in the actual.");
                }
                if (messageType != typeof(Hook) && actuals_elementsByType.ContainsKey(messageType))
                { 
                    actuals_elementsByType[messageType].Count().Should().Be(expecteds_elementsByType[messageType].Count());
                }
                if (messageType == typeof(Hook) && actuals_elementsByType.ContainsKey(messageType))
                    actuals_elementsByType[messageType].Count().Should().BeGreaterThanOrEqualTo(expecteds_elementsByType[messageType].Count());
            }
        }
    }

    internal static class MessageExtensions
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

        internal static bool HasId(this object element)
        {
            return MessagesWithIds.Contains(element.GetType());
        }
        internal static string Id(this object message)
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
        internal static List<Type> EnvelopeContentTypes = new()
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
        internal static object Content(this Envelope envelope)
        {
            object? result = null;
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
            return result!;
        }
    }
}
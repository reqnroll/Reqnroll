using FluentAssertions;
using Reqnroll.CucumberMessages;
using Io.Cucumber.Messages.Types;

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
            var xrefBuilder = new CrossReferenceBuilder( msg =>
                {
                    InsertIntoElementsByType(msg, elementsByType);

                    if (msg.HasId())
                    {
                        InsertIntoElementsById(msg, elementsByID);
                        InsertIntoIDsByType(msg, IDsByType);
                    }
                });
            foreach (var message in messages)
            {
                var msg = message.Content();
                CucumberMessageVisitor.Accept(xrefBuilder, msg);
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

            foreach (var messageType in CucumberMessageExtensions.EnvelopeContentTypes)
            {
                if (actuals_elementsByType.ContainsKey(messageType) && !expecteds_elementsByType.ContainsKey(messageType))
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
}
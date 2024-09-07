using FluentAssertions;
using Reqnroll.CucumberMessages;
using Io.Cucumber.Messages.Types;

namespace CucumberMessages.CompatibilityTests
{
    public class CucumberMessagesValidator
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

        public void ResultShouldPassAllComparisonTests()
        {
            SourceContentShouldBeIdentical();
            GherkinDocumentShouldBeComparable( );
            PicklesShouldBeComparable();
            StepDefinitionsShouldBeComparable( );
            // Hooks are not comparable
            TestCasesShouldBeComparable( );
        }

        private void TestCasesShouldBeComparable()
        {
        }

        private void StepDefinitionsShouldBeComparable()
        {
        }

        private void PicklesShouldBeComparable()
        {
        }

        private void GherkinDocumentShouldBeComparable()
        {
            var actualGherkinDocument = actuals_elementsByType[typeof(GherkinDocument)].First().As<GherkinDocument>();
            var expectedGherkinDocument = expecteds_elementsByType[typeof(GherkinDocument)].First().As<GherkinDocument>();

            //check top-level items first
            // ignore Uri
            // comments should be present in the same order; so a simple list comparison should work
            actualGherkinDocument.Comments.Should().BeEquivalentTo(expectedGherkinDocument.Comments, options => options.Including(c => c.Text));
            FeatureShouldBeComparable(actualGherkinDocument.Feature, expectedGherkinDocument.Feature);
        }

        private void FeatureShouldBeComparable(Feature actual, Feature expected)
        {
            // ingore Location elements and Id values
            actual.Tags.Should().BeEquivalentTo(expected.Tags, options => options.Including(t => t.Name));

            // CCK expects only the language code, not the language and culture codes
            actual.Language.Split('-')[0].Should().Be(expected.Language);
            actual.Name.Should().Be(expected.Name);
            actual.Description.Replace("\r\n", "\n").Should().Be(expected.Description.Replace("\r\n", "\n"));
            actual.Keyword.Should().Be(expected.Keyword);
            // expecting that the children are in the same order
            

        }

        private void SourceContentShouldBeIdentical()
        {
            var actualSource = actuals_elementsByType[typeof(Source)].First().As<Source>();
            var expectedSource = expecteds_elementsByType[typeof(Source)].First().As<Source>();
            actualSource.Data.Replace("\r\n", "\n").Should().Be(expectedSource.Data.Replace("\r\n", "\n"));
            actualSource.MediaType.Should().Be(expectedSource.MediaType);
        }

        public void ResultShouldPassBasicSanityChecks()
        {
            EachTestStepShouldProperlyReferToAPickleAndStepDefinitionOrHook();
            EachPickleAndPickleStepShouldBeReferencedByTestStepsAtLeastOnce();
        }

        private void EachTestStepShouldProperlyReferToAPickleAndStepDefinitionOrHook()
        {
        }

        private void EachPickleAndPickleStepShouldBeReferencedByTestStepsAtLeastOnce()
        {
        }

        public void ShouldPassBasicStructuralChecks()
        {
            var actual = actualEnvelopes;
            var expected = expectedEnvelopes;
            actual.Count().Should().BeGreaterThanOrEqualTo(expected.Count());

            // This checks that each top level Envelope content type present in the actual is present in the expected in the same number (except for hooks)
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
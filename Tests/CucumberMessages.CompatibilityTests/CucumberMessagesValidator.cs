using FluentAssertions;
using Reqnroll.CucumberMessages;
using Io.Cucumber.Messages.Types;
using System.ComponentModel.Design;

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
        private readonly FluentAsssertionCucumberMessagePropertySelectionRule FA_CustomCucumberMessagesPropertySelector;

        public CucumberMessagesValidator(IEnumerable<Envelope> actual, IEnumerable<Envelope> expected)
        {
            actualEnvelopes = actual;
            expectedEnvelopes = expected;

            SetupCrossReferences(actual, actuals_IDsByType, actuals_elementsByType, actuals_elementsByID);
            SetupCrossReferences(expected, expecteds_IDsByType, expecteds_elementsByType, expecteds_elementsByID);

            FA_CustomCucumberMessagesPropertySelector = new FluentAsssertionCucumberMessagePropertySelectionRule(expecteds_elementsByType.Keys.ToList());
            AssertionOptions.AssertEquivalencyUsing(options => options
                            // invoking these for each Type in CucumberMessages so that FluentAssertions DOES NOT call .Equal wwhen comparing instances
                                                                    .ComparingByValue<Attachment>()
                                                                    .ComparingByMembers<Background>()
                                                                    .ComparingByMembers<Ci>()
                                                                    .ComparingByMembers<Comment>()
                                                                    .ComparingByMembers<DataTable>()
                                                                    .ComparingByMembers<DocString>()
                                                                    .ComparingByMembers<Envelope>()
                                                                    .ComparingByMembers<Examples>()
                                                                    .ComparingByMembers<Feature>()
                                                                    .ComparingByMembers<FeatureChild>()
                                                                    .ComparingByMembers<GherkinDocument>()
                                                                    .ComparingByMembers<Group>()
                                                                    .ComparingByMembers<Hook>()
                                                                    .ComparingByMembers<Location>()
                                                                    .ComparingByMembers<Meta>()
                                                                    .ComparingByMembers<ParameterType>()
                                                                    .ComparingByMembers<ParseError>()
                                                                    .ComparingByMembers<Pickle>()
                                                                    .ComparingByMembers<PickleDocString>()
                                                                    .ComparingByMembers<PickleStep>()
                                                                    .ComparingByMembers<PickleStepArgument>()
                                                                    .ComparingByMembers<PickleTable>()
                                                                    .ComparingByMembers<PickleTableCell>()
                                                                    .ComparingByMembers<PickleTableRow>()
                                                                    .ComparingByMembers<PickleTag>()
                                                                    .ComparingByMembers<Product>()
                                                                    .ComparingByMembers<Rule>()
                                                                    .ComparingByMembers<RuleChild>()
                                                                    .ComparingByMembers<Scenario>()
                                                                    .ComparingByMembers<Source>()
                                                                    .ComparingByMembers<SourceReference>()
                                                                    .ComparingByMembers<Step>()
                                                                    .ComparingByMembers<StepDefinition>()
                                                                    .ComparingByMembers<StepDefinitionPattern>()
                                                                    .ComparingByMembers<StepMatchArgument>()
                                                                    .ComparingByMembers<StepMatchArgumentsList>()
                                                                    .ComparingByMembers<TableCell>()
                                                                    .ComparingByMembers<TableRow>()
                                                                    .ComparingByMembers<Tag>()
                                                                    .ComparingByMembers<TestCase>()
                                                                    .ComparingByMembers<TestCaseFinished>()
                                                                    .ComparingByMembers<TestCaseStarted>()
                                                                    .ComparingByMembers<TestRunFinished>()
                                                                    .ComparingByMembers<TestRunStarted>()
                                                                    .ComparingByMembers<TestStep>()
                                                                    .ComparingByMembers<TestStepFinished>()
                                                                    .ComparingByMembers<TestStepResult>()
                                                                    .ComparingByMembers<TestStepStarted>()
                                                                    .ComparingByMembers<UndefinedParameterType>()
                                     // Using a custom Property Selector so that we can ignore the following properties (Id, Uri, and Location); these will always be different
                                                                    .Using(FA_CustomCucumberMessagesPropertySelector)
                                       // Using a custom string comparison to ignore the differences in platform line endings
                                                                    .Using<string>(new FluentAssertionsCustomStringComparisons())
                                                                    .AllowingInfiniteRecursion()
                                                                    .RespectingRuntimeTypes()
                                                                    );
        }
        private void SetupCrossReferences(IEnumerable<Envelope> messages, Dictionary<Type, HashSet<string>> IDsByType, Dictionary<Type, HashSet<object>> elementsByType, Dictionary<string, HashSet<object>> elementsByID)
        {
            var xrefBuilder = new CrossReferenceBuilder(msg =>
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
            GherkinDocumentShouldBeComparable();
            PicklesShouldBeComparable();
            StepDefinitionsShouldBeComparable();
            // Hooks are not comparable
            TestCasesShouldBeComparable();
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

            actualGherkinDocument.Should().BeEquivalentTo(expectedGherkinDocument, options => options
            .Using<string>(ctx =>
            {
                var actual = ctx.Subject.Split("-")[0];
                var expected = ctx.Expectation.Split("-")[0];
                actual.Should().Be(expected);
            })
            .When(inf => inf.Path.EndsWith("Language"))
            .WithTracing());

        }


        private void SourceContentShouldBeIdentical()
        {
            var actualSource = actuals_elementsByType[typeof(Source)].First() as Source;
            var expectedSource = expecteds_elementsByType[typeof(Source)].First() as Source;

            actualSource.Should().BeEquivalentTo(expectedSource, options => options.WithTracing()        );
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
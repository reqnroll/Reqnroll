using FluentAssertions;
using FluentAssertions.Equivalency;
using FluentAssertions.Execution;
using Io.Cucumber.Messages.Types;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Versioning;

namespace Reqnroll.Formatters.Tests;

public partial class CucumberMessagesValidator
{
    private readonly IEnumerable<Envelope> _actualEnvelopes;
    private readonly IEnumerable<Envelope> _expectedEnvelopes;

    // cross-reference metadata
    private readonly Dictionary<Type, HashSet<string>> _actualIDsByType = new();
    private readonly Dictionary<Type, HashSet<string>> _expectedIDsByType = new();
    private readonly Dictionary<Type, HashSet<object>> _actualElementsByType = new();
    private readonly Dictionary<Type, HashSet<object>> _expectedElementsByType = new();
    private readonly Dictionary<string, object> _actualElementsById = new();
    private readonly Dictionary<string, object> _expectedElementsById = new();
    private readonly FluentAssertionCucumberMessagePropertySelectionRule _customCucumberMessagesPropertySelector;

    Dictionary<object, int> _actualPartitions;
    Dictionary<object, int> _expectedPartitions;
    private readonly int _numPartitions;


    // Envelope types - these are the top level types in CucumberMessages
    // Meta is excluded from the list as there is nothing there for us to compare
    private IEnumerable<Type> EnvelopeTypes
    {
        get
        {
            var ect = EnvelopeExtensions.EnvelopeContentTypes.ToList();
            ect.Remove(typeof(Meta));
            return ect;
        }
    }

    public CucumberMessagesValidator(Envelope[] actual, Envelope[] expected)
    {
        _actualEnvelopes = actual;
        _expectedEnvelopes = expected;

        SetupCrossReferences(_actualEnvelopes, _actualIDsByType, _actualElementsByType, _actualElementsById);
        SetupCrossReferences(expected, _expectedIDsByType, _expectedElementsByType, _expectedElementsById);

        // assign a 'partition' number to each Envelope id so that envelopes can be split apart by Feature
        // this will allow Envelope sequence comparisons to be isolated to each feature (ensuring that parallel feature execution won't interfere with comparisons)
        _actualPartitions = Partition(actual);
        _expectedPartitions = Partition(expected);
        _numPartitions = _expectedPartitions.Values.Max();

        _customCucumberMessagesPropertySelector = new FluentAssertionCucumberMessagePropertySelectionRule(_expectedElementsByType.Keys.ToList());
    }

    private Dictionary<object, int> Partition(Envelope[] envelopes)
    {
        var partitioner = new EnvelopePartitioner();
        var result = new Dictionary<object, int>();
        foreach (var envelope in envelopes)
        {
            result.Add(envelope.Content(), partitioner.AssignPartition(envelope));
        }
        return result;
    }

    private void SetupCrossReferences(IEnumerable<Envelope> messages, Dictionary<Type, HashSet<string>> idsByType, Dictionary<Type, HashSet<object>> elementsByType, Dictionary<string, object> elementsById)
    {
        var xrefBuilder = new CrossReferenceBuilder(msg =>
        {
            InsertIntoElementsByType(msg, elementsByType);

            if (msg.HasId())
            {
                InsertIntoElementsById(msg, elementsById);
                InsertIntoIDsByType(msg, idsByType);
            }
        });
        foreach (var message in messages)
        {
            var msg = message.Content();
            xrefBuilder.Accept(msg);
        }
    }
    private static void InsertIntoIDsByType(object msg, Dictionary<Type, HashSet<string>> idsByType)
    {
        if (!idsByType.ContainsKey(msg.GetType()))
        {
            idsByType.Add(msg.GetType(), new HashSet<string>());
        }
        idsByType[msg.GetType()].Add(msg.Id());
    }

    private static void InsertIntoElementsById(object msg, Dictionary<string, object> elementsById)
    {
        elementsById.Add(msg.Id(), msg);
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
        var method = typeof(CucumberMessagesValidator).GetMethod(nameof(CompareMessageType), BindingFlags.NonPublic | BindingFlags.Instance);
        using (new AssertionScope("Comparing Actual to Expected Envelopes"))
        {
            foreach (Type t in EnvelopeTypes)
            {
                var genMethod = method!.MakeGenericMethod(t);
                for (int i = 0; i <= _numPartitions; i++)
                {
                    genMethod.Invoke(this, [i]);
                }
            }
        }
    }

    private int MapPartitionNumber(int partitionNumber)
    {
        if (partitionNumber == 0) return 0;

        var gd = _expectedPartitions.First(kvp => kvp.Value == partitionNumber && kvp.Key is GherkinDocument).Key as GherkinDocument;
        var featureName = gd!.Feature.Name;
        return _actualPartitions.First(kvp => kvp.Key is GherkinDocument document && document.Feature.Name == featureName).Value;
    }
    // by partition
    private void CompareMessageType<T>(int partitionNumber)
    {
        if (!_expectedElementsByType.ContainsKey(typeof(T)))
            return;

        int actualsPartitionNumber = MapPartitionNumber(partitionNumber);

        var actual = _actualElementsByType.TryGetValue(typeof(T), out HashSet<object>? actualElements) && actualElements.Count > 0 ?
            actualElements.OfType<T>().Where(e => _actualPartitions[e!] == actualsPartitionNumber).ToList() : new List<T>();

        var expected = _expectedElementsByType[typeof(T)].AsEnumerable().OfType<T>().Where(e => _expectedPartitions[e!] == partitionNumber).ToList();

        if (!(typeof(T) == typeof(TestStepFinished)))
        {
            actual.Should().BeEquivalentTo(expected, options => ArrangeFluentAssertionOptions(options).WithTracing(), "When comparing " + typeof(T).Name + "s");
        }
        else
        {
            // For TestStepFinished, we will separate out those related to hooks; 
            // the regular comparison will be done for TestStepFinished related to PickleSteps/StepDefinitions
            // Hook related TestStepFinished - the order is indeterminate, so we will check quantity, and count of Statuses
            // Hook related TestSteps are found by following the testStepId of the Finished message to the related TestStep. If the TestStep has a value for pickleStepId, then it is a regular step.
            // if it has a hookId, it is a hook step

            var actualHookRelatedTestStepFinished = actual.OfType<TestStepFinished>().Where(tsf => _actualElementsById[tsf.TestStepId].As<TestStep>().HookId != null).ToList();
            var actualStepRelatedTestStepFinished = actual.OfType<TestStepFinished>().Where(tsf => _actualElementsById[tsf.TestStepId].As<TestStep>().HookId == null).ToList();
            var expectedHookRelatedTestStepFinished = expected.OfType<TestStepFinished>().Where(tsf => _expectedElementsById[tsf.TestStepId].As<TestStep>().HookId != null).ToList();
            var expectedStepRelatedTestStepFinished = expected.OfType<TestStepFinished>().Where(tsf => _expectedElementsById[tsf.TestStepId].As<TestStep>().HookId == null).ToList();

            actualStepRelatedTestStepFinished.Should().BeEquivalentTo(expectedStepRelatedTestStepFinished, options => ArrangeFluentAssertionOptions(options).WithTracing(), "when comparing TestStepFinished messages");

            actualHookRelatedTestStepFinished.Should().BeEquivalentTo(expectedHookRelatedTestStepFinished, options => ArrangeFluentAssertionOptions(options).WithoutStrictOrdering().WithTracing(), "when comparing Hook TestStepFinished messages");
        }
    }

    public void ResultShouldPassSanityChecks()
    {
        using (new AssertionScope("Sanity Checks"))
        {
            EachTestCaseAndStepsShouldProperlyReferToAPickleAndStepDefinitionOrHook();
            EachPickleAndPickleStepShouldBeReferencedByTestStepsAtLeastOnce();
            TestExecutionStepsShouldProperlyReferenceTestCases();
            TestExecutionMessagesShouldProperlyNest();
            ActualTestExecutionMessagesShouldReferBackToTheSameStepTextAsExpected();
        }
    }


    // by partition
    private void ActualTestExecutionMessagesShouldReferBackToTheSameStepTextAsExpected()
    {
        // IF the expected results contains no TestStepStarted messages, then there is nothing to check
        if (!_expectedElementsByType.Keys.Contains(typeof(TestStepStarted)))
            return;
        for (int i = 0; i < _numPartitions; i++)
        {
            var partitionNumber = i + 1;
            int actualsPartitionNumber = MapPartitionNumber(partitionNumber);

            // For each TestStepStarted message, ensure that the pickle step referred to is the same in Actual and Expected for the corresponding testStepStarted message
            var actualTestStepStartedTestStepIds = _actualElementsByType[typeof(TestStepStarted)].OfType<TestStepStarted>().Where(e => _actualPartitions[e!] == actualsPartitionNumber).Select(tss => tss.TestStepId).ToList();
            var expectedTestStepStartedTestStepIds = _expectedElementsByType[typeof(TestStepStarted)].OfType<TestStepStarted>().Where(e => _expectedPartitions[e!] == partitionNumber).Select(tss => tss.TestStepId).ToList();

            // Making the assumption here that the order of TestStepStarted messages is the same in both Actual and Expected within a Partition
            // pair these up, and walk back to the pickle step text and compare

            actualTestStepStartedTestStepIds
                .Zip(expectedTestStepStartedTestStepIds, (a, e) => (a, e))
                .ToList()
                .ForEach(t =>
                {
                    _actualElementsById[t.a].Should().BeAssignableTo<TestStep>();
                    var actualTestStep = _actualElementsById[t.a] as TestStep;
                    _expectedElementsById[t.e].Should().BeAssignableTo<TestStep>();
                    var expectedTestStep = _expectedElementsById[t.e] as TestStep;
                    if (actualTestStep!.PickleStepId != null && expectedTestStep!.PickleStepId != null)
                    {
                        _actualElementsById[actualTestStep.PickleStepId].Should().BeAssignableTo<PickleStep>();
                        var actualPickleStep = _actualElementsById[actualTestStep.PickleStepId] as PickleStep;
                        _expectedElementsById[expectedTestStep.PickleStepId].Should().BeAssignableTo<PickleStep>();
                        var expectedPickleStep = _expectedElementsById[expectedTestStep.PickleStepId] as PickleStep;
                        actualPickleStep!.Text.Should().Be(expectedPickleStep!.Text, $"expecting the text of the pickle step {actualPickleStep.Id} to match that of {expectedPickleStep.Id}");
                    }
                    else
                    { // confirm that both are null or not null, if one is null, throw an exception
                        actualTestStep.PickleStepId.Should().Be(expectedTestStep!.PickleStepId, "expecting both PickleStepIds to be null or not null");
                    }
                });
        }
    }

    private void TestExecutionStepsShouldProperlyReferenceTestCases()
    {
        var testCaseIds = _actualElementsByType[typeof(TestCase)].OfType<TestCase>().Select(tc => tc.Id).ToList();

        var testCaseStartedItems = _actualElementsByType[typeof(TestCaseStarted)].OfType<TestCaseStarted>().ToList();
        testCaseIds.Should().Contain(id => testCaseStartedItems.Any(tcs => tcs.TestCaseId == id), "a test case should be referenced by a test case started message");

        var testCaseStartedIds = testCaseStartedItems.Select(tcs => tcs.Id).ToList();

        var testCaseFinishedItems = _actualElementsByType[typeof(TestCaseFinished)].OfType<TestCaseFinished>().ToList();
        testCaseStartedIds.Should().Contain(id => testCaseFinishedItems.Any(tcf => tcf.TestCaseStartedId == id), "a test case started should be referenced by a test case finished message");

        // IF the Scenario has no steps, return early.
        if (!_actualElementsByType.Keys.Contains(typeof(TestStepStarted)))
            return;
        var testStepStartedItems = _actualElementsByType[typeof(TestStepStarted)].OfType<TestStepStarted>().ToList();
        var testStepFinishedItems = _actualElementsByType[typeof(TestStepFinished)].OfType<TestStepFinished>().ToList();

        testCaseStartedIds.Should().Contain(id => testStepStartedItems.Any(tss => tss.TestCaseStartedId == id), "a test case started should be referenced by at least one test step started message");
        testCaseStartedIds.Should().Contain(id => testStepFinishedItems.Any(tsf => tsf.TestCaseStartedId == id), "a test case started should be referenced by at least one test step finished message");
    }

    private void TestExecutionMessagesShouldProperlyNest()
    {
        var closedIds = new List<string>();
        var openTestCaseStartedIds = new List<string>();
        var openTestStepIds = new List<string>();
        var numberOfEnvelopes = _actualEnvelopes.Count();
        var testRunStartedSeenAtEnvelopeIndex = numberOfEnvelopes + 1;
        var testRunFinishedSeenAtEnvelopeIndex = -1;
        int currentIndex = 0;
        foreach (object msg in _actualEnvelopes.Select(e => e.Content()))
        {
            switch (msg)
            {
                case TestRunStarted:
                    testRunStartedSeenAtEnvelopeIndex = currentIndex;
                    if (testRunFinishedSeenAtEnvelopeIndex != -1)
                        testRunStartedSeenAtEnvelopeIndex.Should().BeLessThan(testRunFinishedSeenAtEnvelopeIndex, "TestRunStarted events must be before TestRunFinished event");
                    break;
                case TestRunFinished:
                    testRunFinishedSeenAtEnvelopeIndex = currentIndex;
                    testRunFinishedSeenAtEnvelopeIndex.Should().BeGreaterThan(testRunStartedSeenAtEnvelopeIndex, "TestRunFinished events must be after TestRunStarted event");
                    testRunFinishedSeenAtEnvelopeIndex.Should().Be(numberOfEnvelopes - 1, "TestRunFinished events must be the last event");
                    break;
                case TestCaseStarted testCaseStarted:
                    currentIndex.Should().BeGreaterThan(testRunStartedSeenAtEnvelopeIndex, "TestCaseStarted events must be after TestRunStarted event");
                    if (testRunFinishedSeenAtEnvelopeIndex != -1)
                        currentIndex.Should().BeLessThan(testRunFinishedSeenAtEnvelopeIndex, "TestCaseStarted events must be before TestRunFinished event");
                    closedIds.Should().NotContain(testCaseStarted.Id, "a test case should not be Started twice");
                    openTestCaseStartedIds.Add(testCaseStarted.Id);
                    break;
                case TestCaseFinished testCaseFinished:
                    currentIndex.Should().BeGreaterThan(testRunStartedSeenAtEnvelopeIndex, "TestCaseFinished events must be after TestRunStarted event");
                    if (testRunFinishedSeenAtEnvelopeIndex != -1)
                        currentIndex.Should().BeLessThan(testRunFinishedSeenAtEnvelopeIndex, "TestCaseFinished events must be before TestRunFinished event");
                    closedIds.Should().NotContain(testCaseFinished.TestCaseStartedId, "a test case should not be Finished twice");
                    openTestCaseStartedIds.Should().Contain(testCaseFinished.TestCaseStartedId, "a test case should be Started and active before it is Finished");
                    openTestCaseStartedIds.Remove(testCaseFinished.TestCaseStartedId);
                    closedIds.Add(testCaseFinished.TestCaseStartedId);
                    openTestCaseStartedIds.Remove(testCaseFinished.TestCaseStartedId);
                    break;
                case TestStepStarted testStepStarted:
                    currentIndex.Should().BeGreaterThan(testRunStartedSeenAtEnvelopeIndex, "TestStepStarted events must be after TestRunStarted event");
                    if (testRunFinishedSeenAtEnvelopeIndex != -1)
                        currentIndex.Should().BeLessThan(testRunFinishedSeenAtEnvelopeIndex, "TestStepStarted events must be before TestRunFinished event");
                    closedIds.Should().NotContain(testStepStarted.TestCaseStartedId, "a TestStepStarted event must refer to an active test case");
                    openTestCaseStartedIds.Should().Contain(testStepStarted.TestCaseStartedId, "a TestStepStarted event must refer to an active test case");
                    openTestStepIds.Add(testStepStarted.TestStepId);
                    break;
                case TestStepFinished testStepFinished:
                    currentIndex.Should().BeGreaterThan(testRunStartedSeenAtEnvelopeIndex, "TestStepFinished events must be after TestRunStarted event");
                    if (testRunFinishedSeenAtEnvelopeIndex != -1)
                        currentIndex.Should().BeLessThan(testRunFinishedSeenAtEnvelopeIndex, "TestStepFinished events must be before TestRunFinished event");
                    closedIds.Should().NotContain(testStepFinished.TestCaseStartedId, "a TestStepFinished event must refer to an active test case");

                    // Can not assert the following as TestStepIds are re-used when a step is re-executed during a Retry
                    // ClosedIDs.Should().NotContain(testStepFinished.TestStepId, "a TestStepFinished event must refer to an active test step");
                    openTestCaseStartedIds.Should().Contain(testStepFinished.TestCaseStartedId, "a TestStepFinished event must refer to an active test case");
                    openTestStepIds.Should().Contain(testStepFinished.TestStepId, "a TestStepFinished event must refer to an active test step");
                    closedIds.Add(testStepFinished.TestStepId);
                    openTestStepIds.Remove(testStepFinished.TestStepId);
                    break;
            }
            currentIndex++;
        }
    }

    private void EachTestCaseAndStepsShouldProperlyReferToAPickleAndStepDefinitionOrHook()
    {
        var testCases = _actualElementsByType[typeof(TestCase)].OfType<TestCase>();
        foreach (var testCase in testCases)
        {
            var pickle = testCase.PickleId;
            _actualElementsById.Should().ContainKey(pickle, "a pickle should be referenced by the test case");

            var steps = testCase.TestSteps.OfType<TestStep>();
            foreach (var step in steps)
            {
                if (step.HookId != null)
                    _actualElementsById.Should().ContainKey(step.HookId, "a step references a hook that doesn't exist");

                if (step.PickleStepId != null)
                    _actualElementsById.Should().ContainKey(step.PickleStepId, "a step references a pickle step that doesn't exist");

                if (step.StepDefinitionIds != null && step.StepDefinitionIds.Count > 0)
                {
                    foreach (var stepDefinitionId in step.StepDefinitionIds)
                        _actualElementsById.Should().ContainKey(stepDefinitionId, "a step references a step definition that doesn't exist");
                }
            }
        }
    }

    private void EachPickleAndPickleStepShouldBeReferencedByTestStepsAtLeastOnce()
    {
        var pickles = _actualElementsByType[typeof(Pickle)].OfType<Pickle>();
        foreach (var pickle in pickles)
        {
            var testCases = _actualElementsByType[typeof(TestCase)].OfType<TestCase>();
            testCases.Should().Contain(tc => tc.PickleId == pickle.Id, "a pickle should be referenced by a test case");

            var pickleSteps = pickle.Steps.OfType<PickleStep>();
            foreach (var pickleStep in pickleSteps)
            {
                var testSteps = _actualElementsByType[typeof(TestStep)].OfType<TestStep>();
                testSteps.Should().Contain(ts => ts.PickleStepId == pickleStep.Id, "a pickle step should be referenced by a test step");
            }
        }
    }

    public void ShouldPassBasicStructuralChecks()
    {
        var actual = _actualEnvelopes;
        var expected = _expectedEnvelopes;

        using (new AssertionScope("Basic Structural Comparison Tests"))
        {
            // This checks that each top level Envelope content type present in the actual is present in the expected in the same number (except for hooks)
            foreach (var messageType in EnvelopeExtensions.EnvelopeContentTypes)
            {
                if (_actualElementsByType.ContainsKey(messageType) && !_expectedElementsByType.ContainsKey(messageType))
                {
                    throw new System.Exception($"{messageType} present in the actual but not in the expected.");
                }
                if (!_actualElementsByType.ContainsKey(messageType) && _expectedElementsByType.ContainsKey(messageType))
                {
                    throw new System.Exception($"{messageType} present in the expected but not in the actual.");
                }
                if (messageType != typeof(Hook) && _actualElementsByType.ContainsKey(messageType))
                {
                    _actualElementsByType[messageType].Should().HaveCount(_expectedElementsByType[messageType].Count());
                }
                if (messageType == typeof(Hook) && _actualElementsByType.ContainsKey(messageType))
                    _actualElementsByType[messageType].Should().HaveCountGreaterThanOrEqualTo(_expectedElementsByType[messageType].Count());
            }

            actual.Should().HaveCountGreaterThanOrEqualTo(expected.Count(), "the total number of envelopes in the actual should be at least as many as in the expected");
        }
    }

    private bool GroupListIsEmpty(List<Group> groups)
    {
        if (groups == null || groups.Count == 0) return true;
        foreach (var group in groups)
        {
            if (!string.IsNullOrEmpty(group.Value) || group.Start.HasValue)
                return false;
            if (!GroupListIsEmpty(group.Children)) return false;
        }
        return true;
    }

    private EquivalencyAssertionOptions<T> ArrangeFluentAssertionOptions<T>(EquivalencyAssertionOptions<T> options)
    {
        // invoking these for each Type in CucumberMessages so that FluentAssertions DOES NOT call .Equal when comparing instances
        return options
               .ComparingByMembers<Attachment>()
               .ComparingByMembers<Background>()
               .ComparingByMembers<Ci>()
               .ComparingByMembers<Comment>()
               .ComparingByMembers<Io.Cucumber.Messages.Types.DataTable>()
               .ComparingByMembers<Io.Cucumber.Messages.Types.DocString>()
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
               .ComparingByMembers<TestRunHookStarted>()
               .ComparingByMembers<TestRunHookFinished>()
               .ComparingByMembers<Suggestion>()
               .ComparingByMembers<Snippet>()

               // Using a custom Property Selector so that we can ignore the properties that are not comparable
               .Using(_customCucumberMessagesPropertySelector)
               // Using a custom string comparison that deals with ISO langauge codes when the property name ends with "Language"
               //.Using<string>(ctx =>
               //{
               //    var actual = ctx.Subject.Split("-")[0];
               //    var expected = ctx.Expectation.Split("-")[0];
               //    actual.Should().Be(expected);
               //})

               // Using special logic to assert that suggestions must contain at least one snippets among those specified in the Expected set
               // We can't compare snippet content as the Language and Code properties won't match
               .Using<Suggestion>(ctx =>
               {
                   var actualSnippets = ctx.Subject.Snippets ?? new List<Snippet>();
                   var expectedSnippets = ctx.Expectation.Snippets ?? new List<Snippet>();
                   actualSnippets.Should().HaveCountGreaterThanOrEqualTo(1).And.HaveCountLessThanOrEqualTo(expectedSnippets.Count);
               })
               .WhenTypeIs<Suggestion>()

               // Using special logic to compare regular expression strings (ignoring the differences of the regex anchor characters)
               .Using<List<string>>(ctx =>
               {
                   var subjects = ctx.Subject;
                   var expectations = ctx.Expectation;
                   subjects.Should().HaveSameCount(expectations);
                   int count = subjects.Count;
                   for (int i = 0; i < count; i++)
                   {
                       string subject = subjects[i];
                       string expectation = expectations[i];
                       if (subject.Length > 0 && subject[0] == '^' || expectation.Length > 0 && expectation[0] == '^' ||
                           subject.Length > 0 && subject[^1] == '$' || expectation.Length > 0 && expectation[^1] == '$')
                       {
                           // If the first or last character is '^' or '$', remove it before comparing
                           subject = subject.Length > 0 && subject[0] == '^' ? subject.Substring(1) : subject;
                           subject = subject.Length > 0 && subject[^1] == '$' ? subject.Substring(0, subject.Length - 1) : subject;
                           expectation = expectation.Length > 0 && expectation[0] == '^' ? expectation.Substring(1) : expectation;
                           expectation = expectation.Length > 0 && expectation[^1] == '$' ? expectation.Substring(0, expectation.Length - 1) : expectation;
                       }
                       subject.Should().Be(expectation);
                   }
               })
               .When(info => info.Path.EndsWith("RegularExpressions"))

               // Using special logic to ignore ParameterTypeName except when the value is one of the basic types
               .Using<string>((ctx) =>
               {
                   if (ctx.Expectation == "string" || ctx.Expectation == "int" || ctx.Expectation == "long" || ctx.Expectation == "double" || ctx.Expectation == "float"
                       || ctx.Expectation == "short" || ctx.Expectation == "byte" || ctx.Expectation == "biginteger")
                   {
                       ctx.Subject.Should().Be(ctx.Expectation);
                   }
                   // Any other ParameterTypeName should be ignored, including {word} (no .NET equivalent) and custom type names
                   else
                   {
                       1.Should().Be(1);
                   }
               })
               .When(info => info.Path.EndsWith("ParameterTypeName"))

               // Using a custom string comparison to ignore the differences in platform line endings
               .Using<string>((ctx) =>
               {
                   var subject = ctx.Subject ?? string.Empty;
                   var expectation = ctx.Expectation ?? string.Empty;
                   subject = subject.Replace("\r\n", "\n");
                   expectation = expectation.Replace("\r\n", "\n");
                   subject.Should().Be(expectation);
               })
               .When(info => info.Path.EndsWith("Description") || info.Path.EndsWith("Text") || info.Path.EndsWith("Data") || info.Path.EndsWith("Content"))

               // The list of hooks should contain at least as many items as the list of expected hooks
               // Because Reqnroll does not support Tag Expressions, these are represented in RnR as multiple Hooks or multiple Tags on Hooks Binding methods
               // which result in multiple Hook messages.
               .Using<List<Hook>>(ctx =>
               {
                   if (ctx.SelectedNode.IsRoot)
                   {
                       var actualList = ctx.Subject;
                       var expectedList = ctx.Expectation;

                       if (expectedList == null || !expectedList.Any())
                       {
                           return; // If expected is null or empty, we don't need to check anything
                       }

                       actualList.Should().NotBeNull();
                       actualList.Should().HaveCountGreaterThanOrEqualTo(expectedList.Count,
                                                                         "actual collection should have at least as many items as expected");

                       // Impossible to compare individual Hook messages (Ids aren't comparable, the Source references aren't compatible, 
                       // and the Scope tags won't line up because the CCK uses tag expressions and RnR does not support them)
                       // and After Hook execution ordering is different between Reqnroll and CCK.
                       /*
                           foreach (var expectedItem in expectedList)
                           {
                               actualList.Should().Contain(actualItem =>
                                   AssertionExtensions.Should(actualItem).BeEquivalentTo(expectedItem, "").And.Subject == actualItem,
                                   "actual collection should contain an item equivalent to {0}", expectedItem);
                           }
                       */
                   }
               })
               .WhenTypeIs<List<Hook>>()

               // Groups are nested self-referential objects inside StepMatchArgument(s). Other Cucumber implementations support a more sophisticated
               // version of this structure in which multiple regex capture groups are conveyed inside a single StepMatchArgument
               // For Reqnroll, we will only compare the outermost Group of the actual.
               .Using<Group>((ctx) =>
               {
                   var actual = ctx.Subject;
                   var expected = ctx.Expectation;
                   if (expected != null
                        && expected.Value.Length > 2
                        && ((expected.Value.StartsWith("\"") && expected.Value.EndsWith("\""))
                            || (expected.Value.StartsWith("'") && expected.Value.EndsWith("'")))
                       && !GroupListIsEmpty(expected.Children))
                   {
                       actual.Value.Should().Be(expected.Children[0].Value);
                       actual.Start.Should().Be(expected.Children[0].Start);
                   }
                   else
                   {
                       actual.Value.Should().Be(ctx.Expectation.Value);
                       actual.Start.Should().Be(ctx.Expectation.Start);
                   }
               })
               .WhenTypeIs<Group>()

               // A bit of trickery here to tell FluentAssertions that Timestamps are always equal
               // We can't simply omit Timestamp from comparison because then TestRunStarted has nothing else to compare (which causes an error)
               .Using<Timestamp>(_ => 1.Should().Be(1))
               .WhenTypeIs<Timestamp>()

               .Using<List<Group>>((ctx) =>
               {
                   var actual = ctx.Subject;
                   var expected = ctx.Expectation;
                   // The CCK cucumber implementation sometimes renders empty nested groups, which we will ignore
                   if (GroupListIsEmpty(expected)) { 1.Should().Be(1); }
                   actual.Should().BeEquivalentTo(expected);
               })
               .WhenTypeIs<List<Group>>()

               // TestStepResult.Message contains Exception message string. If one is expected, then the result should have content
               .Using<string>((ctx) =>
               {
                   var actual = ctx.Subject;
                   var expected = ctx.Expectation;
                   if (!string.IsNullOrEmpty(expected))
                   {
                       actual.Should().NotBeNullOrEmpty();
                   }
                   else
                   {
                       actual.Should().BeNullOrEmpty();
                   }
               })
               .When(info => info.Path.EndsWith("Message"))

               .AllowingInfiniteRecursion()
               //.RespectingRuntimeTypes()
               .ExcludingFields()
               .WithStrictOrdering();
    }

}
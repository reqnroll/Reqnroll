using Io.Cucumber.Messages.Types;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;
using System.Diagnostics;

namespace Reqnroll.Formatters.Tests;

public partial class CucumberMessagesValidator
{
    public class EnvelopePartitioner : CucumberMessageVisitorBase
    {
        int partitionCount = 1;
        int assignedPartition = -1;
        Dictionary<string, int> partitionIdAssignedToUri = new Dictionary<string, int>();
        Dictionary<string, int> partitionAssignedToPreviouslySeenIds = new Dictionary<string, int>();
        public int AssignPartition(Envelope env)
        {
            assignedPartition = -1;
            Accept(env);
            Debug.Assert(assignedPartition != -1);
            return assignedPartition;
        }
        public override void OnVisiting(GherkinDocument gherkinDocument)
        {
            assignedPartition = partitionIdAssignedToUri[gherkinDocument.Uri];
            partitionCount++;
        }
        public override void OnVisiting(Pickle pickle)
        {
            assignedPartition = partitionIdAssignedToUri[pickle.Uri];
            partitionAssignedToPreviouslySeenIds.Add(pickle.Id, assignedPartition);
            foreach (var ps in pickle.Steps)
            {
                partitionAssignedToPreviouslySeenIds.Add(ps.Id, assignedPartition);
            }
        }
        public override void OnVisiting(Io.Cucumber.Messages.Types.TestCase testCase)
        {
            assignedPartition = partitionAssignedToPreviouslySeenIds[testCase.PickleId];
            partitionAssignedToPreviouslySeenIds.Add(testCase.Id, assignedPartition);
        }
        public override void OnVisiting(TestCaseStarted testCaseStarted)
        {
            assignedPartition = partitionAssignedToPreviouslySeenIds[testCaseStarted.TestCaseId];
            partitionAssignedToPreviouslySeenIds.Add(testCaseStarted.Id, assignedPartition);
        }
        public override void OnVisiting(TestCaseFinished testCaseFinished)
        {
            assignedPartition = partitionAssignedToPreviouslySeenIds[testCaseFinished.TestCaseStartedId];
        }
        public override void OnVisiting(TestStepStarted testStepStarted)
        {
            assignedPartition = partitionAssignedToPreviouslySeenIds[testStepStarted.TestCaseStartedId];
        }
        public override void OnVisiting(TestStepFinished testStepFinished)
        {
            assignedPartition = partitionAssignedToPreviouslySeenIds[testStepFinished.TestCaseStartedId];
        }
        public override void OnVisiting(Attachment attachment)
        {
            if (!String.IsNullOrEmpty(attachment.TestCaseStartedId)) {
                assignedPartition = partitionAssignedToPreviouslySeenIds[attachment.TestCaseStartedId];
            }
            assignedPartition = 0;
        }
        public override void OnVisiting(Hook hook) { assignedPartition = 0; }
        public override void OnVisiting(StepDefinition stepDefinition) { assignedPartition = 0; }
        public override void OnVisiting(ParameterType parameterType) { assignedPartition = 0; }
        public override void OnVisiting(UndefinedParameterType undefinedParameterType) { assignedPartition = 0; }
        public override void OnVisiting(Source source)
        {
            assignedPartition = partitionCount;
            partitionIdAssignedToUri.Add(source.Uri, assignedPartition);
        }
        public override void OnVisiting(Meta meta) { assignedPartition = 0; }
        public override void OnVisiting(TestRunStarted testRunStarted) { assignedPartition = 0; }
        public override void OnVisiting(TestRunFinished testRunFinished) { assignedPartition = 0; }
        public override void OnVisiting(Suggestion suggestion) { assignedPartition = partitionAssignedToPreviouslySeenIds[suggestion.PickleStepId]; }
        public override void OnVisiting(ParseError error) { assignedPartition = 0; }
        public override void OnVisiting(TestRunHookStarted testRunHookStarted) { assignedPartition = 0; }
        public override void OnVisiting(TestRunHookFinished testRunHookFinished) { assignedPartition = 0; }

    }

}
using Io.Cucumber.Messages.Types;

namespace Reqnroll.CucumberMessages;

// This interface is used to support the implementation of an External Vistor pattern against the Cucumber Messages.
// Visitors impmlement this interface and then invoke it using the helper class below.

public interface ICucumberMessageVisitor
{
    // Existing methods
    void Visit(Envelope envelope);
    void Visit(Attachment attachment);
    void Visit(GherkinDocument gherkinDocument);
    void Visit(Feature feature);
    void Visit(FeatureChild featureChild);
    void Visit(Rule rule);
    void Visit(RuleChild ruleChild);
    void Visit(Background background);
    void Visit(Scenario scenario);
    void Visit(Examples examples);
    void Visit(Step step);
    void Visit(TableRow tableRow);
    void Visit(TableCell tableCell);
    void Visit(Tag tag);
    void Visit(Pickle pickle);
    void Visit(PickleStep pickleStep);
    void Visit(PickleStepArgument pickleStepArgument);
    void Visit(PickleTable pickleTable);
    void Visit(PickleTableRow pickleTableRow);
    void Visit(PickleTableCell pickleTableCell);
    void Visit(PickleTag pickleTag);
    void Visit(TestCase testCase);
    void Visit(TestCaseStarted testCaseStarted);
    void Visit(TestCaseFinished testCaseFinished);
    void Visit(TestStep testStep);
    void Visit(TestStepStarted testStepStarted);
    void Visit(TestStepFinished testStepFinished);
    void Visit(TestStepResult testStepResult);
    void Visit(Hook hook);
    void Visit(StepDefinition stepDefinition);
    void Visit(ParameterType parameterType);
    void Visit(UndefinedParameterType undefinedParameterType);
    void Visit(SourceReference sourceReference);
    void Visit(Duration duration);
    void Visit(Timestamp timestamp);
    void Visit(Io.Cucumber.Messages.Types.Exception exception);
    void Visit(Meta meta);
    void Visit(Product product);
    void Visit(Ci ci);
    void Visit(Git git);
    void Visit(Source source);
    void Visit(Comment comment);
    void Visit(Io.Cucumber.Messages.Types.DataTable dataTable);
    void Visit(DocString docString);
    void Visit(Group group);
    void Visit(JavaMethod javaMethod);
    void Visit(JavaStackTraceElement javaStackTraceElement);
    void Visit(Location location);
    void Visit(ParseError parseError);
    void Visit(PickleDocString pickleDocString);
    void Visit(StepDefinitionPattern stepDefinitionPattern);
    void Visit(StepMatchArgument stepMatchArgument);
    void Visit(StepMatchArgumentsList stepMatchArgumentsList);
    void Visit(TestRunStarted testRunStarted);
    void Visit(TestRunFinished testRunFinished);
}

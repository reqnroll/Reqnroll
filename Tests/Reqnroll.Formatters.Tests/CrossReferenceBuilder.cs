using Io.Cucumber.Messages.Types;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;

namespace Reqnroll.Formatters.Tests;

internal class CrossReferenceBuilder : CucumberMessageVisitorBase
{
    private readonly Action<object> _buildCrossReferences;

    public CrossReferenceBuilder(Action<object> buildCrossReferences)
    {
        _buildCrossReferences = buildCrossReferences;
    }

    public override void OnVisiting(Attachment attachment)
    {
        _buildCrossReferences(attachment);
        base.OnVisiting(attachment);
    }
    public override void OnVisiting(Background background)
    {
        _buildCrossReferences(background);
        base.OnVisiting(background);
    }
    public override void OnVisiting(Ci ci)
    {
        _buildCrossReferences(ci);
        base.OnVisiting(ci);
    }
    public override void OnVisiting(Comment comment)
    {
        _buildCrossReferences(comment);
        base.OnVisiting(comment);
    }
    public override void OnVisiting(Duration duration)
    {
        _buildCrossReferences(duration);
        base.OnVisiting(duration);
    }

    public override void OnVisiting(Examples examples)
    {
        _buildCrossReferences(examples);
        base.OnVisiting(examples);
    }
    public override void OnVisiting(Io.Cucumber.Messages.Types.Exception exception)
    {
        _buildCrossReferences(exception);
        base.OnVisiting(exception);
    }
    public override void OnVisiting(GherkinDocument gherkinDocument)
    {
        _buildCrossReferences(gherkinDocument);
        base.OnVisiting(gherkinDocument);
    }

    public override void OnVisiting(Feature feature)
    {
        _buildCrossReferences(feature);
        base.OnVisiting(feature);
    }
    public override void OnVisiting(FeatureChild featureChild)
    {
        _buildCrossReferences(featureChild);
        base.OnVisiting(featureChild);
    }
    public override void OnVisiting(Git git)
    {
        _buildCrossReferences(git);
        base.OnVisiting(git);
    }
    public override void OnVisiting(Group group)
    {
        _buildCrossReferences(group);
        base.OnVisiting(group);
    }
    public override void OnVisiting(JavaMethod javaMethod)
    {
        _buildCrossReferences(javaMethod);
        base.OnVisiting(javaMethod);
    }
    public override void OnVisiting(JavaStackTraceElement javaStackTraceElement)
    {
        _buildCrossReferences(javaStackTraceElement);
        base.OnVisiting(javaStackTraceElement);
    }
    public override void OnVisiting(Location location)
    {
        _buildCrossReferences(location);
        base.OnVisiting(location);
    }
    public override void OnVisiting(Meta meta)
    {
        _buildCrossReferences(meta);
        base.OnVisiting(meta);
    }
    public override void OnVisiting(ParameterType parameterType)
    {
        _buildCrossReferences(parameterType);
        base.OnVisiting(parameterType);
    }
    public override void OnVisiting(ParseError parseError)
    {
        _buildCrossReferences(parseError);
        base.OnVisiting(parseError);
    }
    public override void OnVisiting(PickleStepArgument pickleStepArgument)
    {
        _buildCrossReferences(pickleStepArgument);
        base.OnVisiting(pickleStepArgument);
    }
    public override void OnVisiting(PickleTable pickleTable)
    {
        _buildCrossReferences(pickleTable);
        base.OnVisiting(pickleTable);
    }

    public override void OnVisiting(PickleTableRow pickleTableRow)
    {
        _buildCrossReferences(pickleTableRow);
        base.OnVisiting(pickleTableRow);
    }
    public override void OnVisiting(PickleTableCell pickleTableCell)
    {
        _buildCrossReferences(pickleTableCell);
        base.OnVisiting(pickleTableCell);
    }
    public override void OnVisiting(PickleTag pickleTag)
    {
        _buildCrossReferences(pickleTag);
        base.OnVisiting(pickleTag);
    }
    public override void OnVisiting(Product product)
    {
        _buildCrossReferences(product);
        base.OnVisiting(product);
    }
    public override void OnVisiting(Rule rule)
    {
        _buildCrossReferences(rule);
        base.OnVisiting(rule);
    }
    public override void OnVisiting(RuleChild ruleChild)
    {
        _buildCrossReferences(ruleChild);
        base.OnVisiting(ruleChild);
    }
    public override void OnVisiting(Scenario scenario)
    {
        _buildCrossReferences(scenario);
        base.OnVisiting(scenario);
    }
    public override void OnVisiting(Source source)
    {  
        _buildCrossReferences(source);
        base.OnVisiting(source);
    }
    public override void OnVisiting(SourceReference sourceReference)
    {
        _buildCrossReferences(sourceReference);
        base.OnVisiting(sourceReference);
    }
    public override void OnVisiting(Step step)
    {
        _buildCrossReferences(step);
        base.OnVisiting(step);
    }
    public override void OnVisiting(StepDefinition stepDefinition)
    {
        _buildCrossReferences(stepDefinition);
        base.OnVisiting(stepDefinition);
    }
    public override void OnVisiting(StepDefinitionPattern stepDefinitionPattern)
    {
        _buildCrossReferences(stepDefinitionPattern);
        base.OnVisiting(stepDefinitionPattern);
    }
    public override void OnVisiting(StepMatchArgument stepMatchArgument)
    {
        _buildCrossReferences(stepMatchArgument);
        base.OnVisiting(stepMatchArgument);
    }

    public override void OnVisiting(StepMatchArgumentsList stepMatchArgumentsList)
    {
        _buildCrossReferences(stepMatchArgumentsList);
        base.OnVisiting(stepMatchArgumentsList);
    }
    public override void OnVisiting(Io.Cucumber.Messages.Types.DocString docString)
    {
        _buildCrossReferences(docString);
        base.OnVisiting(docString);
    }

    public override void OnVisiting(Io.Cucumber.Messages.Types.DataTable dataTable)
    {
        _buildCrossReferences(dataTable);
        base.OnVisiting(dataTable);
    }

    public override void OnVisiting(TableCell tableCell)
    {
        _buildCrossReferences(tableCell);
        base.OnVisiting(tableCell);
    }
    public override void OnVisiting(TableRow tableRow)
    {
        _buildCrossReferences(tableRow);
        base.OnVisiting(tableRow);
    }
    public override void OnVisiting(Tag tag)
    {
        _buildCrossReferences(tag);
        base.OnVisiting(tag);
    }
    public override void OnVisiting(TestCase testCase)
    {   
        _buildCrossReferences(testCase);
        base.OnVisiting(testCase);
    }
    public override void OnVisiting(TestCaseFinished testCaseFinished)
    {
        _buildCrossReferences(testCaseFinished);
        base.OnVisiting(testCaseFinished);
    }

    public override void OnVisiting(TestRunFinished testRunFinished)
    {
        _buildCrossReferences(testRunFinished);
        base.OnVisiting(testRunFinished);
    }

    public override void OnVisiting(TestRunStarted testRunStarted)
    {
        _buildCrossReferences(testRunStarted);
        base.OnVisiting(testRunStarted);
    }

    public override void OnVisiting(TestCaseStarted testCaseStarted)
    {
        _buildCrossReferences(testCaseStarted);
        base.OnVisiting(testCaseStarted);
    }
    public override void OnVisiting(TestStep testStep)
    {
        _buildCrossReferences(testStep);
        base.OnVisiting(testStep);
    }
    public override void OnVisiting(TestStepResult testStepResult)
    {
        _buildCrossReferences(testStepResult);
        base.OnVisiting(testStepResult);
    }
    public override void OnVisiting(TestStepFinished testStepFinished)
    {
        _buildCrossReferences(testStepFinished);
        base.OnVisiting(testStepFinished);
    }

    public override void OnVisiting(TestStepStarted testStepStarted)
    {
        _buildCrossReferences(testStepStarted);
        base.OnVisiting(testStepStarted);
    }

    public override void OnVisiting(Hook hook)
    {
        _buildCrossReferences(hook);
        base.OnVisiting(hook);
    }

    public override void OnVisiting(Pickle pickle)
    {
        _buildCrossReferences(pickle);
        base.OnVisiting(pickle);
    }

    public override void OnVisiting(PickleStep pickleStep)
    {
        _buildCrossReferences(pickleStep);
        base.OnVisiting(pickleStep);
    }
    public override void OnVisiting(PickleDocString pickleDocString)
    {
        _buildCrossReferences(pickleDocString);
        base.OnVisiting(pickleDocString);
    }
    public override void OnVisiting(Timestamp timestamp)
    {
        _buildCrossReferences(timestamp);
        base.OnVisiting(timestamp);
    }
    public override void OnVisiting(UndefinedParameterType undefinedParameterType)
    {
        _buildCrossReferences(undefinedParameterType);
        base.OnVisiting(undefinedParameterType);
    }

    public override void OnVisiting(TestRunHookStarted testRunHookStarted)
    {
        _buildCrossReferences(testRunHookStarted);
        base.OnVisiting(testRunHookStarted);
    }

    public override void OnVisiting(TestRunHookFinished testRunHookFinished)
    {
        _buildCrossReferences(testRunHookFinished);
        base.OnVisiting(testRunHookFinished);
    }

    public override void OnVisiting(Suggestion suggestion)
    {
        _buildCrossReferences(suggestion);
        base.OnVisiting(suggestion);
    }

    public override void OnVisiting(Snippet snippet)
    {
        _buildCrossReferences(snippet);
        base.OnVisiting(snippet);
    }
}
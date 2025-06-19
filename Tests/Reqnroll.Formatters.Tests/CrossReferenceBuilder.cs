using Io.Cucumber.Messages.Types;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;

namespace Reqnroll.Formatters.Tests
{
    internal class CrossReferenceBuilder : CucumberMessageVisitorBase
    {
        private Action<object> buildCrossReferences;
        public CrossReferenceBuilder(Action<object> buildCrossReferences)
        {
            this.buildCrossReferences = buildCrossReferences;
        }

        public override void OnVisiting(Attachment attachment)
        {
            buildCrossReferences(attachment);
            base.OnVisiting(attachment);
        }
        public override void OnVisiting(Background background)
        {
            buildCrossReferences(background);
            base.OnVisiting(background);
        }
        public override void OnVisiting(Ci ci)
        {
            buildCrossReferences(ci);
            base.OnVisiting(ci);
        }
        public override void OnVisiting(Comment comment)
        {
            buildCrossReferences(comment);
            base.OnVisiting(comment);
        }
        public override void OnVisiting(Duration duration)
        {
            buildCrossReferences(duration);
            base.OnVisiting(duration);
        }

        public override void OnVisiting(Examples examples)
        {
            buildCrossReferences(examples);
            base.OnVisiting(examples);
        }
        public override void OnVisiting(Io.Cucumber.Messages.Types.Exception exception)
        {
            buildCrossReferences(exception);
            base.OnVisiting(exception);
        }
        public override void OnVisiting(GherkinDocument gherkinDocument)
        {
            buildCrossReferences(gherkinDocument);
            base.OnVisiting(gherkinDocument);
        }

        public override void OnVisiting(Feature feature)
        {
            buildCrossReferences(feature);
            base.OnVisiting(feature);
        }
        public override void OnVisiting(FeatureChild featureChild)
        {
            buildCrossReferences(featureChild);
            base.OnVisiting(featureChild);
        }
        public override void OnVisiting(Git git)
        {
            buildCrossReferences(git);
            base.OnVisiting(git);
        }
        public override void OnVisiting(Group group)
        {
            buildCrossReferences(group);
            base.OnVisiting(group);
        }
        public override void OnVisiting(JavaMethod javaMethod)
        {
            buildCrossReferences(javaMethod);
            base.OnVisiting(javaMethod);
        }
        public override void OnVisiting(JavaStackTraceElement javaStackTraceElement)
        {
            buildCrossReferences(javaStackTraceElement);
            base.OnVisiting(javaStackTraceElement);
        }
        public override void OnVisiting(Location location)
        {
            buildCrossReferences(location);
            base.OnVisiting(location);
        }
        public override void OnVisiting(Meta meta)
        {
            buildCrossReferences(meta);
            base.OnVisiting(meta);
        }
        public override void OnVisiting(ParameterType parameterType)
        {
            buildCrossReferences(parameterType);
            base.OnVisiting(parameterType);
        }
        public override void OnVisiting(ParseError parseError)
        {
            buildCrossReferences(parseError);
            base.OnVisiting(parseError);
        }
        public override void OnVisiting(PickleStepArgument pickleStepArgument)
        {
            buildCrossReferences(pickleStepArgument);
            base.OnVisiting(pickleStepArgument);
        }
        public override void OnVisiting(PickleTable pickleTable)
        {
            buildCrossReferences(pickleTable);
            base.OnVisiting(pickleTable);
        }

        public override void OnVisiting(PickleTableRow pickleTableRow)
        {
            buildCrossReferences(pickleTableRow);
            base.OnVisiting(pickleTableRow);
        }
        public override void OnVisiting(PickleTableCell pickleTableCell)
        {
            buildCrossReferences(pickleTableCell);
            base.OnVisiting(pickleTableCell);
        }
        public override void OnVisiting(PickleTag pickleTag)
        {
            buildCrossReferences(pickleTag);
            base.OnVisiting(pickleTag);
        }
        public override void OnVisiting(Product product)
        {
            buildCrossReferences(product);
            base.OnVisiting(product);
        }
        public override void OnVisiting(Rule rule)
        {
            buildCrossReferences(rule);
            base.OnVisiting(rule);
        }
        public override void OnVisiting(RuleChild ruleChild)
        {
            buildCrossReferences(ruleChild);
            base.OnVisiting(ruleChild);
        }
        public override void OnVisiting(Scenario scenario)
        {
            buildCrossReferences(scenario);
            base.OnVisiting(scenario);
        }
        public override void OnVisiting(Source source)
        {  
            buildCrossReferences(source);
            base.OnVisiting(source);
        }
        public override void OnVisiting(SourceReference sourceReference)
        {
            buildCrossReferences(sourceReference);
            base.OnVisiting(sourceReference);
        }
        public override void OnVisiting(Step step)
        {
            buildCrossReferences(step);
            base.OnVisiting(step);
        }
        public override void OnVisiting(StepDefinition stepDefinition)
        {
            buildCrossReferences(stepDefinition);
            base.OnVisiting(stepDefinition);
        }
        public override void OnVisiting(StepDefinitionPattern stepDefinitionPattern)
        {
            buildCrossReferences(stepDefinitionPattern);
            base.OnVisiting(stepDefinitionPattern);
        }
        public override void OnVisiting(StepMatchArgument stepMatchArgument)
        {
            buildCrossReferences(stepMatchArgument);
            base.OnVisiting(stepMatchArgument);
        }

        public override void OnVisiting(StepMatchArgumentsList stepMatchArgumentsList)
        {
            buildCrossReferences(stepMatchArgumentsList);
            base.OnVisiting(stepMatchArgumentsList);
        }
        public override void OnVisiting(DocString docString)
        {
            buildCrossReferences(docString);
            base.OnVisiting(docString);
        }

        public override void OnVisiting(Io.Cucumber.Messages.Types.DataTable dataTable)
        {
            buildCrossReferences(dataTable);
            base.OnVisiting(dataTable);
        }

        public override void OnVisiting(TableCell tableCell)
        {
            buildCrossReferences(tableCell);
            base.OnVisiting(tableCell);
        }
        public override void OnVisiting(TableRow tableRow)
        {
            buildCrossReferences(tableRow);
            base.OnVisiting(tableRow);
        }
        public override void OnVisiting(Tag tag)
        {
            buildCrossReferences(tag);
            base.OnVisiting(tag);
        }
        public override void OnVisiting(TestCase testCase)
        {   
            buildCrossReferences(testCase);
            base.OnVisiting(testCase);
        }
        public override void OnVisiting(TestCaseFinished testCaseFinished)
        {
            buildCrossReferences(testCaseFinished);
            base.OnVisiting(testCaseFinished);
        }

        public override void OnVisiting(TestRunFinished testRunFinished)
        {
            buildCrossReferences(testRunFinished);
            base.OnVisiting(testRunFinished);
        }

        public override void OnVisiting(TestRunStarted testRunStarted)
        {
            buildCrossReferences(testRunStarted);
            base.OnVisiting(testRunStarted);
        }

        public override void OnVisiting(TestCaseStarted testCaseStarted)
        {
            buildCrossReferences(testCaseStarted);
            base.OnVisiting(testCaseStarted);
        }
        public override void OnVisiting(TestStep testStep)
        {
            buildCrossReferences(testStep);
            base.OnVisiting(testStep);
        }
        public override void OnVisiting(TestStepResult testStepResult)
        {
            buildCrossReferences(testStepResult);
            base.OnVisiting(testStepResult);
        }
        public override void OnVisiting(TestStepFinished testStepFinished)
        {
            buildCrossReferences(testStepFinished);
            base.OnVisiting(testStepFinished);
        }

        public override void OnVisiting(TestStepStarted testStepStarted)
        {
            buildCrossReferences(testStepStarted);
            base.OnVisiting(testStepStarted);
        }

        public override void OnVisiting(Hook hook)
        {
            buildCrossReferences(hook);
            base.OnVisiting(hook);
        }

        public override void OnVisiting(Pickle pickle)
        {
            buildCrossReferences(pickle);
            base.OnVisiting(pickle);
        }

        public override void OnVisiting(PickleStep pickleStep)
        {
            buildCrossReferences(pickleStep);
            base.OnVisiting(pickleStep);
        }
        public override void OnVisiting(PickleDocString pickleDocString)
        {
            buildCrossReferences(pickleDocString);
            base.OnVisiting(pickleDocString);
        }
        public override void OnVisiting(Timestamp timestamp)
        {
            buildCrossReferences(timestamp);
            base.OnVisiting(timestamp);
        }
        public override void OnVisiting(UndefinedParameterType undefinedParameterType)
        {
            buildCrossReferences(undefinedParameterType);
            base.OnVisiting(undefinedParameterType);
        }

        public override void OnVisiting(TestRunHookStarted testRunHookStarted)
        {
            buildCrossReferences(testRunHookStarted);
            base.OnVisiting(testRunHookStarted);
        }

        public override void OnVisiting(TestRunHookFinished testRunHookFinished)
        {
            buildCrossReferences(testRunHookFinished);
            base.OnVisiting(testRunHookFinished);
        }
    }
}

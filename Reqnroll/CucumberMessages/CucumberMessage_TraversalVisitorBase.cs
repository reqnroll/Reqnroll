using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Io.Cucumber.Messages.Types;

namespace Reqnroll.CucumberMessages
{
    public abstract class CucumberMessage_TraversalVisitorBase : ICucumberMessageVisitor
    {
        private void Accept(object message)
        {
            if (message != null) CucumberMessageVisitor.Accept(this, message);
        }

        public virtual void Visit(Envelope envelope)
        {
            OnVisiting(envelope);
            Accept(envelope.Content());
            OnVisited(envelope);
        }

        public virtual void Visit(Attachment attachment)
        {
            OnVisiting(attachment);
            OnVisited(attachment);
        }

        public virtual void Visit(GherkinDocument gherkinDocument)
        {
            OnVisiting(gherkinDocument);

            if (gherkinDocument.Feature != null)
                Accept(gherkinDocument.Feature);

            OnVisited(gherkinDocument);
        }

        public virtual void Visit(Feature feature)
        {
            OnVisiting(feature);
            foreach (var featureChild in feature.Children)
            {
                Accept(featureChild);
            }
            OnVisited(feature);
        }

        public virtual void Visit(FeatureChild featureChild)
        {
            OnVisiting(featureChild);
            if (featureChild.Rule != null)
                Accept(featureChild.Rule);
            else if (featureChild.Background != null)
                Accept(featureChild.Background);
            else if (featureChild.Scenario != null)
                Accept(featureChild.Scenario);
            OnVisited(featureChild);
        }

        public virtual void Visit(Rule rule)
        {
            OnVisiting(rule);
            foreach (var ruleChild in rule.Children)
            {
                Accept(ruleChild);
            }
            foreach (var tag in rule.Tags)
            {
                Accept(tag);
            }
            OnVisited(rule);
        }

        public virtual void Visit(RuleChild ruleChild)
        {
            OnVisiting(ruleChild);
            if (ruleChild.Background != null)
                Accept(ruleChild.Background);
            else if (ruleChild.Scenario != null)
                Accept(ruleChild.Scenario);
            OnVisited(ruleChild);
        }

        public virtual void Visit(Background background)
        {
            OnVisiting(background);
            Accept(background.Location);
            foreach (var step in background.Steps)
            {
                Accept(step);
            }
            OnVisited(background);
        }

        public virtual void Visit(Scenario scenario)
        {
            OnVisiting(scenario);
            Accept(scenario.Location);
            foreach (var tag in scenario.Tags)
            {
                Accept(tag);
            }
            foreach (var step in scenario.Steps)
            {
                Accept(step);
            }
            foreach (var example in scenario.Examples)
            {
                Accept(example);
            }
            OnVisited(scenario);
        }

        public virtual void Visit(Examples examples)
        {
            OnVisiting(examples);
            Accept(examples.Location);
            foreach (var tag in examples.Tags)
            {
                Accept(tag);
            }
            Accept(examples.TableHeader);
            foreach (var tableRow in examples.TableBody)
            {
                Accept(tableRow);
            }
            OnVisited(examples);
        }

        public virtual void Visit(Step step)
        {
            OnVisiting(step);
            Accept(step.Location);
            Accept(step.DocString);
            Accept(step.DataTable);
            OnVisited(step);
        }

        public virtual void Visit(TableRow tableRow)
        {
            OnVisiting(tableRow);
            Accept(tableRow.Location);
            foreach (var tableCell in tableRow.Cells)
            {
                Accept(tableCell);
            }
            OnVisited(tableRow);
        }

        public virtual void Visit(TableCell tableCell)
        {
            OnVisiting(tableCell);
            Accept(tableCell.Location);
            OnVisited(tableCell);
        }

        public virtual void Visit(Tag tag)
        {
            OnVisiting(tag);
            Accept(tag.Location);
            OnVisited(tag);
        }

        public virtual void Visit(Pickle pickle)
        {
            OnVisiting(pickle);
            foreach (var pickleStep in pickle.Steps)
            {
                Accept(pickleStep);
            }
            foreach (var tag in pickle.Tags)
            {
                Accept(tag);
            }
            OnVisited(pickle);
        }

        public virtual void Visit(PickleStep pickleStep)
        {
            OnVisiting(pickleStep);
            Accept(pickleStep.Argument);
            OnVisited(pickleStep);
        }

        public virtual void Visit(PickleStepArgument pickleStepArgument)
        {
            OnVisiting(pickleStepArgument);
            if (pickleStepArgument.DataTable != null)
                Accept(pickleStepArgument.DataTable);
            else if (pickleStepArgument.DocString != null)
                Accept(pickleStepArgument.DocString);
            OnVisited(pickleStepArgument);
        }

        public virtual void Visit(PickleTable pickleTable)
        {
            OnVisiting(pickleTable);
            foreach (var pickleTableRow in pickleTable.Rows)
            {
                Accept(pickleTableRow);
            }
            OnVisited(pickleTable);
        }

        public virtual void Visit(PickleTableRow pickleTableRow)
        {
            OnVisiting(pickleTableRow);
            foreach (var pickleTableCell in pickleTableRow.Cells)
            {
                Accept(pickleTableCell);
            }
            OnVisited(pickleTableRow);
        }

        public virtual void Visit(PickleTableCell pickleTableCell)
        {
            OnVisiting(pickleTableCell);
            OnVisited(pickleTableCell);
        }

        public virtual void Visit(PickleTag pickleTag)
        {
            OnVisiting(pickleTag);
            OnVisited(pickleTag);
        }

        public virtual void Visit(TestCase testCase)
        {
            OnVisiting(testCase);
            foreach (var step in testCase.TestSteps)
            {
                Accept(step);
            }
            OnVisited(testCase);
        }

        public virtual void Visit(TestCaseStarted testCaseStarted)
        {
            OnVisiting(testCaseStarted);
            Accept(testCaseStarted.Timestamp);
            OnVisited(testCaseStarted);
        }

        public virtual void Visit(TestCaseFinished testCaseFinished)
        {
            OnVisiting(testCaseFinished);
            Accept(testCaseFinished.Timestamp);
            OnVisited(testCaseFinished);
        }

        public virtual void Visit(TestStep testStep)
        {
            OnVisiting(testStep);
            foreach (var argumentList in testStep.StepMatchArgumentsLists)
            {
                Accept(argumentList);
            }
            OnVisited(testStep);
        }

        public virtual void Visit(TestStepStarted testStepStarted)
        {
            OnVisiting(testStepStarted);
            Accept(testStepStarted.Timestamp);
            OnVisited(testStepStarted);
        }

        public virtual void Visit(TestStepFinished testStepFinished)
        {
            OnVisiting(testStepFinished);
            Accept(testStepFinished.TestStepResult);
            Accept(testStepFinished.Timestamp);
            OnVisited(testStepFinished);
        }

        public virtual void Visit(TestStepResult testStepResult)
        {
            OnVisiting(testStepResult);
            Accept(testStepResult.Duration);
            Accept(testStepResult.Exception);
            OnVisited(testStepResult);
        }

        public virtual void Visit(Hook hook)
        {
            OnVisiting(hook);
            Accept(hook.SourceReference);
            OnVisited(hook);
        }

        public virtual void Visit(StepDefinition stepDefinition)
        {
            OnVisiting(stepDefinition);
            Accept(stepDefinition.Pattern);
            Accept(stepDefinition.SourceReference);
            OnVisited(stepDefinition);
        }

        public virtual void Visit(ParameterType parameterType)
        {
            OnVisiting(parameterType);
            Accept(parameterType.SourceReference);
            OnVisited(parameterType);
        }

        public virtual void Visit(UndefinedParameterType undefinedParameterType)
        {
            OnVisiting(undefinedParameterType);
            OnVisited(undefinedParameterType);
        }

        public virtual void Visit(SourceReference sourceReference)
        {
            OnVisiting(sourceReference);
            if (sourceReference.Location != null) Accept(sourceReference.Location);
            else if (sourceReference.JavaMethod != null) Accept(sourceReference.JavaMethod);
            else if (sourceReference.JavaStackTraceElement != null) Accept(sourceReference.JavaStackTraceElement);
            OnVisited(sourceReference);
        }

        public virtual void Visit(Duration duration)
        {
            OnVisiting(duration);
            OnVisited(duration);
        }

        public virtual void Visit(Timestamp timestamp)
        {
            OnVisiting(timestamp);
            OnVisited(timestamp);
        }

        public virtual void Visit(Io.Cucumber.Messages.Types.Exception exception)
        {
            OnVisiting(exception);
            OnVisited(exception);
        }

        public virtual void Visit(Meta meta)
        {
            OnVisiting(meta);
            Accept(meta.Implementation);
            Accept(meta.Runtime);
            Accept(meta.Os);
            Accept(meta.Cpu);
            Accept(meta.Ci);
            OnVisited(meta);
        }

        public virtual void Visit(Product product)
        {
            OnVisiting(product);
            OnVisited(product);
        }

        public virtual void Visit(Ci ci)
        {
            OnVisiting(ci);
            Accept(ci.Git);
            OnVisited(ci);
        }

        public virtual void Visit(Git git)
        {
            OnVisiting(git);
            OnVisited(git);
        }

        public virtual void Visit(Source source)
        {
            OnVisiting(source);
            OnVisited(source);
        }

        public virtual void Visit(Comment comment)
        {
            OnVisiting(comment);
            Accept(comment.Location);
            OnVisited(comment);
        }

        public virtual void Visit(Io.Cucumber.Messages.Types.DataTable dataTable)
        {
            OnVisiting(dataTable);
            Accept(dataTable.Location);
            foreach (var row in dataTable.Rows)
            {
                Accept(row);
            }
            OnVisited(dataTable);
        }

        public virtual void Visit(DocString docString)
        {
            OnVisiting(docString);
            Accept(docString.Location);
            OnVisited(docString);
        }

        public virtual void Visit(Group group)
        {
            OnVisiting(group);
            foreach (var child in group.Children)
            {
                Accept(child);
            }
            OnVisited(group);
        }

        public virtual void Visit(JavaMethod javaMethod)
        {
            OnVisiting(javaMethod);
            OnVisited(javaMethod);
        }

        public virtual void Visit(JavaStackTraceElement javaStackTraceElement)
        {
            OnVisiting(javaStackTraceElement);
            OnVisited(javaStackTraceElement);
        }

        public virtual void Visit(Location location)
        {
            OnVisiting(location);
            OnVisited(location);
        }

        public virtual void Visit(ParseError parseError)
        {
            OnVisiting(parseError);
            Accept(parseError.Source);
            OnVisited(parseError);
        }

        public virtual void Visit(PickleDocString pickleDocString)
        {
            OnVisiting(pickleDocString);
            OnVisited(pickleDocString);
        }

        public virtual void Visit(StepDefinitionPattern stepDefinitionPattern)
        {
            OnVisiting(stepDefinitionPattern);
            OnVisited(stepDefinitionPattern);
        }

        public virtual void Visit(StepMatchArgument stepMatchArgument)
        {
            OnVisiting(stepMatchArgument);
            Accept(stepMatchArgument.Group);
            OnVisited(stepMatchArgument);
        }

        public virtual void Visit(StepMatchArgumentsList stepMatchArgumentsList)
        {
            OnVisiting(stepMatchArgumentsList);
            foreach (var stepMatchArgument in stepMatchArgumentsList.StepMatchArguments)
            {
                Accept(stepMatchArgument);
            }
            OnVisited(stepMatchArgumentsList);
        }

        public virtual void Visit(TestRunStarted testRunStarted)
        {
            OnVisiting(testRunStarted);
            Accept(testRunStarted.Timestamp);
            OnVisited(testRunStarted);
        }

        public virtual void Visit(TestRunFinished testRunFinished)
        {
            OnVisiting(testRunFinished);
            Accept(testRunFinished.Timestamp);
            Accept(testRunFinished.Exception);
            OnVisited(testRunFinished);
        }

        public virtual void OnVisiting(Attachment attachment)
        { }

        public virtual void OnVisited(Attachment attachment)
        { }

        public virtual void OnVisiting(Envelope envelope)
        { }

        public virtual void OnVisited(Envelope envelope)
        { }

        public virtual void OnVisiting(Feature feature)
        { }

        public virtual void OnVisited(Feature feature)
        { }

        public virtual void OnVisiting(FeatureChild featureChild)
        { }

        public virtual void OnVisited(FeatureChild featureChild)
        { }

        public virtual void OnVisiting(Examples examples)
        { }

        public virtual void OnVisited(Examples examples)
        { }

        public virtual void OnVisiting(Step step)
        { }

        public virtual void OnVisited(Step step)
        { }

        public virtual void OnVisiting(TableRow tableRow)
        { }

        public virtual void OnVisited(TableRow tableRow)
        { }

        public virtual void OnVisiting(TableCell tableCell)
        { }

        public virtual void OnVisited(TableCell tableCell)
        { }

        public virtual void OnVisiting(Tag tag)
        { }

        public virtual void OnVisited(Tag tag)
        { }

        public virtual void OnVisiting(Pickle pickle)
        { }

        public virtual void OnVisited(Pickle pickle)
        { }

        public virtual void OnVisiting(PickleStep pickleStep)
        { }

        public virtual void OnVisited(PickleStep pickleStep)
        { }

        public virtual void OnVisiting(PickleStepArgument pickleStepArgument)
        { }

        public virtual void OnVisited(PickleStepArgument pickleStepArgument)
        { }

        public virtual void OnVisiting(PickleTable pickleTable)
        { }

        public virtual void OnVisited(PickleTable pickleTable)
        { }

        public virtual void OnVisiting(PickleTableRow pickleTableRow)
        { }

        public virtual void OnVisited(PickleTableRow pickleTableRow)
        { }

        public virtual void OnVisiting(PickleTableCell pickleTableCell)
        { }

        public virtual void OnVisited(PickleTableCell pickleTableCell)
        { }

        public virtual void OnVisiting(PickleTag pickelTag)
        { }

        public virtual void OnVisited(PickleTag pickelTag)
        { }

        public virtual void OnVisiting(Rule rule)
        { }

        public virtual void OnVisited(Rule rule)
        { }

        public virtual void OnVisiting(RuleChild ruleChild)
        { }

        public virtual void OnVisited(RuleChild ruleChild)
        { }

        public virtual void OnVisiting(Background background)
        { }

        public virtual void OnVisited(Background background)
        { }

        public virtual void OnVisiting(Scenario scenario)
        { }

        public virtual void OnVisited(Scenario scenario)
        { }

        public virtual void OnVisiting(GherkinDocument gherkinDocument)
        { }

        public virtual void OnVisited(GherkinDocument gherkinDocument)
        { }

        public virtual void OnVisiting(TestCaseFinished testCaseFinished)
        { }

        public virtual void OnVisited(TestCaseFinished testCaseFinished)
        { }

        public virtual void OnVisiting(TestCaseStarted testCaseStarted)
        { }

        public virtual void OnVisited(TestCaseStarted testCaseStarted)
        { }

        public virtual void OnVisiting(TestStep testStep)
        { }

        public virtual void OnVisited(TestStep testStep)
        { }

        public virtual void OnVisiting(TestStepFinished testStepFinished)
        { }

        public virtual void OnVisited(TestStepFinished testStepFinished)
        { }

        public virtual void OnVisiting(TestStepStarted testStepStarted)
        { }

        public virtual void OnVisited(TestStepStarted testStepStarted)
        { }

        public virtual void OnVisiting(TestStepResult testStepResult)
        { }

        public virtual void OnVisited(TestStepResult testStepResult)
        { }

        public virtual void OnVisiting(TestCase testCase)
        { }

        public virtual void OnVisited(TestCase testCase)
        { }

        public virtual void OnVisiting(StepDefinition stepDefinition)
        { }

        public virtual void OnVisited(StepDefinition stepDefinition)
        { }

        public virtual void OnVisiting(UndefinedParameterType undefinedParameterType)
        { }

        public virtual void OnVisited(UndefinedParameterType undefinedParameterType)
        { }

        public virtual void OnVisiting(ParameterType parameterType)
        { }

        public virtual void OnVisited(ParameterType parameterType)
        { }

        public virtual void OnVisiting(ParseError parseError)
        { }

        public virtual void OnVisited(ParseError parseError)
        { }

        public virtual void OnVisiting(Source source)
        { }

        public virtual void OnVisited(Source source)
        { }

        public virtual void OnVisiting(Hook hook)
        { }

        public virtual void OnVisited(Hook hook)
        { }

        public virtual void OnVisiting(Meta meta)
        { }

        public virtual void OnVisited(Meta meta)
        { }

        public virtual void OnVisiting(Ci ci)
        { }

        public virtual void OnVisited(Ci ci)
        { }

        public virtual void OnVisiting(Comment comment)
        { }

        public virtual void OnVisited(Comment comment)
        { }

        public virtual void OnVisiting(DocString docString)
        { }

        public virtual void OnVisited(DocString docString)
        { }

        public virtual void OnVisiting(Duration duration)
        { }

        public virtual void OnVisited(Duration duration)
        { }

        public virtual void OnVisiting(Io.Cucumber.Messages.Types.DataTable dataTable)
        { }

        public virtual void OnVisited(Io.Cucumber.Messages.Types.DataTable dataTable)
        { }

        public virtual void OnVisiting(Io.Cucumber.Messages.Types.Exception exception)
        { }

        public virtual void OnVisited(Io.Cucumber.Messages.Types.Exception exception)
        { }

        public virtual void OnVisiting(JavaMethod javaMethod)
        { }

        public virtual void OnVisited(JavaMethod javaMethod)
        { }

        public virtual void OnVisiting(JavaStackTraceElement javaStackTraceElement)
        { }

        public virtual void OnVisited(JavaStackTraceElement javaStackTraceElement)
        { }

        public virtual void OnVisiting(Location location)
        { }

        public virtual void OnVisited(Location location)
        { }

        public virtual void OnVisiting(Product product)
        { }

        public virtual void OnVisited(Product product)
        { }

        public virtual void OnVisiting(SourceReference sourceReference)
        { }

        public virtual void OnVisited(SourceReference sourceReference)
        { }

        public virtual void OnVisiting(StepDefinitionPattern stepDefinitionPattern)
        { }

        public virtual void OnVisited(StepDefinitionPattern stepDefinitionPattern)
        { }

        public virtual void OnVisiting(StepMatchArgument stepMatchArgument)
        { }

        public virtual void OnVisited(StepMatchArgument stepMatchArgument)
        { }

        public virtual void OnVisiting(StepMatchArgumentsList stepMatchArgumentsList)
        { }

        public virtual void OnVisited(StepMatchArgumentsList stepMatchArgumentsList)
        { }

        public virtual void OnVisiting(Timestamp timestamp)
        { }

        public virtual void OnVisited(Timestamp timestamp)
        { }

        public virtual void OnVisiting(Git git)
        { }

        public virtual void OnVisited(Git git)
        { }

        public virtual void OnVisiting(Group group)
        { }

        public virtual void OnVisited(Group group)
        { }

        public virtual void OnVisiting(PickleDocString pickleDocString)
        { }

        public virtual void OnVisited(PickleDocString pickleDocString)
        { }

        public virtual void OnVisiting(TestRunStarted testRunStarted)
        { }

        public virtual void OnVisited(TestRunStarted testRunStarted)
        { }

        public virtual void OnVisiting(TestRunFinished testRunFinished)
        { }

        public virtual void OnVisited(TestRunFinished testRunFinished)
        { }
    }
}
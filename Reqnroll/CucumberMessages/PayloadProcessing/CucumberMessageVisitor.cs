using System;
using System.Collections.Generic;
using System.Text;

using Io.Cucumber.Messages.Types;

namespace Reqnroll.CucumberMessages.PayloadPatching;

public class CucumberMessageVisitor
{
    public static void Accept(ICucumberMessageVisitor visitor, object message)
    {
        switch (message)
        {
            // Existing cases
            case Envelope envelope:
                visitor.Visit(envelope);
                break;
            case Attachment attachment:
                visitor.Visit(attachment);
                break;
            case GherkinDocument gherkinDocument:
                visitor.Visit(gherkinDocument);
                break;
            case Feature feature:
                visitor.Visit(feature);
                break;
            case FeatureChild featureChild:
                visitor.Visit(featureChild);
                break;
            case Rule rule:
                visitor.Visit(rule);
                break;
            case RuleChild ruleChild:
                visitor.Visit(ruleChild);
                break;
            case Background background:
                visitor.Visit(background);
                break;
            case Scenario scenario:
                visitor.Visit(scenario);
                break;
            case Examples examples:
                visitor.Visit(examples);
                break;
            case Step step:
                visitor.Visit(step);
                break;
            case TableRow tableRow:
                visitor.Visit(tableRow);
                break;
            case TableCell tableCell:
                visitor.Visit(tableCell);
                break;
            case Tag tag:
                visitor.Visit(tag);
                break;
            case Pickle pickle:
                visitor.Visit(pickle);
                break;
            case PickleStep pickleStep:
                visitor.Visit(pickleStep);
                break;
            case PickleStepArgument pickleStepArgument:
                visitor.Visit(pickleStepArgument);
                break;
            case PickleTable pickleTable:
                visitor.Visit(pickleTable);
                break;
            case PickleTableRow pickleTableRow:
                visitor.Visit(pickleTableRow);
                break;
            case PickleTableCell pickleTableCell:
                visitor.Visit(pickleTableCell);
                break;
            case PickleTag pickleTag:
                visitor.Visit(pickleTag);
                break;
            case TestCase testCase:
                visitor.Visit(testCase);
                break;
            case TestCaseStarted testCaseStarted:
                visitor.Visit(testCaseStarted);
                break;
            case TestCaseFinished testCaseFinished:
                visitor.Visit(testCaseFinished);
                break;
            case TestStep testStep:
                visitor.Visit(testStep);
                break;
            case TestStepStarted testStepStarted:
                visitor.Visit(testStepStarted);
                break;
            case TestStepFinished testStepFinished:
                visitor.Visit(testStepFinished);
                break;
            case TestStepResult testStepResult:
                visitor.Visit(testStepResult);
                break;
            case Hook hook:
                visitor.Visit(hook);
                break;
            case StepDefinition stepDefinition:
                visitor.Visit(stepDefinition);
                break;
            case ParameterType parameterType:
                visitor.Visit(parameterType);
                break;
            case UndefinedParameterType undefinedParameterType:
                visitor.Visit(undefinedParameterType);
                break;
            case SourceReference sourceReference:
                visitor.Visit(sourceReference);
                break;
            case Duration duration:
                visitor.Visit(duration);
                break;
            case Timestamp timestamp:
                visitor.Visit(timestamp);
                break;
            case Io.Cucumber.Messages.Types.Exception exception:
                visitor.Visit(exception);
                break;
            case Meta meta:
                visitor.Visit(meta);
                break;
            case Product product:
                visitor.Visit(product);
                break;
            case Ci ci:
                visitor.Visit(ci);
                break;
            case Git git:
                visitor.Visit(git);
                break;
            case Source source:
                visitor.Visit(source);
                break;
            case Comment comment:
                visitor.Visit(comment);
                break;
            case Io.Cucumber.Messages.Types.DataTable dataTable:
                visitor.Visit(dataTable);
                break;
            case DocString docString:
                visitor.Visit(docString);
                break;
            case Group group:
                visitor.Visit(group);
                break;
            case JavaMethod javaMethod:
                visitor.Visit(javaMethod);
                break;
            case JavaStackTraceElement javaStackTraceElement:
                visitor.Visit(javaStackTraceElement);
                break;
            case Location location:
                visitor.Visit(location);
                break;
            case ParseError parseError:
                visitor.Visit(parseError);
                break;
            case PickleDocString pickleDocString:
                visitor.Visit(pickleDocString);
                break;
            case StepDefinitionPattern stepDefinitionPattern:
                visitor.Visit(stepDefinitionPattern);
                break;
            case StepMatchArgument stepMatchArgument:
                visitor.Visit(stepMatchArgument);
                break;
            case StepMatchArgumentsList stepMatchArgumentsList:
                visitor.Visit(stepMatchArgumentsList);
                break;
            case TestRunStarted testRunStarted:
                visitor.Visit(testRunStarted);
                break;
            case TestRunFinished testRunFinished:
                visitor.Visit(testRunFinished);
                break;

            default:
                throw new ArgumentException($"Unsupported message type:{message.GetType().Name}", nameof(message));
        }
    }
}
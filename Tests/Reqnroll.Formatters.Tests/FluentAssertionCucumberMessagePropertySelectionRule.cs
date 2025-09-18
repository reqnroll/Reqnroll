using FluentAssertions.Equivalency;

namespace Reqnroll.Formatters.Tests;

/// <summary>
/// Fluent Assertion Cucumber Message Property Selection Rule
/// This class is used by Fluent Assertions to override which properties will be compared. 
/// These properties will be skipped because they are not comparable across platforms
/// </summary>
public class FluentAssertionCucumberMessagePropertySelectionRule : IMemberSelectionRule
{
    // Properties to skip - this is the default set of properties that are not comparable across platforms
    // Ids (UUIDs are not comparable): AstNodeId, AstNodeIds, PickleIdIndex, HookId, PickleStepId, StepDefinitionIds, TestStepId, TestCaseStartedId, TestCaseId, WorkerId, TestRunHookStartedId 
    // Location, Line and Column (in Location elements) are not always comparable (eg, CCK refers to source line #s in typescript)
    // Uri is not always comparable (eg, CCK refers to source file paths in typescript)
    // JavaMethod and JavaStackTraceElement contents are specific to the platform. CCK does not include these as it generates Uri references to source rather than Method references
    // Exception: Exceptions are not comparable
    // Duration: time values are not comparable
    // UseForSnippets: Reqnroll defaults to false always regardless of what is in the CCK
    // Start: Start refers to a column position in source code, which may not be comparable across platforms.
    // FileName: CCK does not provide the file name of attachments but Reqnroll does
    // ProtocolVersion, Implementation, Runtime, Cpu, Os, Ci: These properties of the Meta message are not comparable across platforms.
    // Name is used for named Hooks, which Reqnroll doesn't have.

    // Line, Column, Seconds and Nanos are skipped, rather than their container types (Location and TimeStamp & Duration, respectively), 
    // because that way we can assert that those container types exist in the actual CucumberMessage (without requiring that the details match the expected CucumberMessage)

    private readonly List<string> _propertiesToSkip = new List<string>() {    
        "Location", "Line", "Column", "Uri", "JavaMethod", "JavaStackTraceElement", "Exception",
        "Duration", "Start", "FileName", "Type", "StackTrace", "UseForSnippets",
        "Id", "AstNodeId", "AstNodeIds", "StepDefinitionIds", "HookId", "PickleStepId", "PickleId", 
        "TestRunStartedId", "TestCaseStartedId", "TestStepId", "TestCaseId", "WorkerId", "TestRunHookStartedId",
        "ProtocolVersion", "Implementation", "Runtime", "Cpu", "Os", "Ci", "Code", "Language", "Name"
    };

    public FluentAssertionCucumberMessagePropertySelectionRule(IEnumerable<Type> cucumberMessageTypes, IEnumerable<string>? proportiesToSkip = null)
    { 
        CucumberMessageTypes = cucumberMessageTypes; 

        if (proportiesToSkip != null)
        {
            _propertiesToSkip = proportiesToSkip.ToList();
        }
    }

    public IEnumerable<Type> CucumberMessageTypes { get; }

    public bool IncludesMembers => false;

    public IEnumerable<IMember> SelectMembers(INode currentNode, IEnumerable<IMember> selectedMembers, MemberSelectionContext context)
    {
        if (CucumberMessageTypes.Contains(context.Type))
        {
            var propertiesToSelect = new List<IMember>();
            foreach (var prop in selectedMembers)
            {
                if (!_propertiesToSkip.Contains(prop.Name))
                    propertiesToSelect.Add(prop);
            }
            return propertiesToSelect;
        }

        return selectedMembers;
    }
    public override string ToString()
    {
        return "Include only relevant CucumberMessage properties";
    }
}
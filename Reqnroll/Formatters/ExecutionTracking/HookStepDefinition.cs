using System;
using System.Collections.Generic;
using System.Text;

namespace Reqnroll.Formatters.ExecutionTracking
{
    public class HookStepDefinition : TestStepDefinition
    {
        internal string HookId { get; }

        internal HookStepDefinition(string testStepDefinitionId, string hookId, TestCaseDefinition parentTestCaseDefinition, ICucumberMessageFactory messageFactory) : base(testStepDefinitionId, hookId, parentTestCaseDefinition, messageFactory)
        {
            HookId = hookId;
        }
    }
}

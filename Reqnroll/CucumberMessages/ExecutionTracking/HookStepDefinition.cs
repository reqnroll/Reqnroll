using System;
using System.Collections.Generic;
using System.Text;

namespace Reqnroll.CucumberMessages.ExecutionTracking
{
    internal class HookStepDefinition : TestStepDefinition
    {
        internal string HookId { get; }

        internal HookStepDefinition(string testStepDefinitionId, string hookId, TestCaseDefinition parentTestCaseDefinition) : base(testStepDefinitionId, hookId, parentTestCaseDefinition)
        {
            HookId = hookId;
        }
    }
}

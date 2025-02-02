using System;
using System.Collections.Generic;
using System.Text;

namespace Reqnroll.CucumberMessages.ExecutionTracking
{
    public class HookStepDefinition : TestStepDefinition
    {
        public string HookId { get; }

        public HookStepDefinition(string testStepDefinitionId, string hookId, TestCaseDefinition parentTestCaseDefinition) : base(testStepDefinitionId, hookId, parentTestCaseDefinition)
        {
            HookId = hookId;
        }
    }
}

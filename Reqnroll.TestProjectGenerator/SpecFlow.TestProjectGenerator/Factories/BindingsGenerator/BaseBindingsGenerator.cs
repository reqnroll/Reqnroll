using System.Collections.Generic;
using TechTalk.SpecFlow.TestProjectGenerator.Data;
using TechTalk.SpecFlow.TestProjectGenerator.Driver;

namespace TechTalk.SpecFlow.TestProjectGenerator.Factories.BindingsGenerator
{
    public abstract class BaseBindingsGenerator
    {
        public abstract ProjectFile GenerateBindingClassFile(string fileContent);

        public abstract ProjectFile GenerateStepDefinition(string method);

        public ProjectFile GenerateStepDefinition(string methodName, string methodImplementation, string attributeName, string regex, ParameterType parameterType = ParameterType.Normal, string argumentName = null)
        {
            string method = GetBindingCode(methodName, methodImplementation, attributeName, regex, parameterType, argumentName);
            return GenerateStepDefinition(method);
        }

        public ProjectFile GenerateLoggingStepDefinition(string methodName, string pathToLogFile, string attributeName, string regex, ParameterType parameterType = ParameterType.Normal, string argumentName = null)
        {
            string method = GetLoggingStepDefinitionCode(methodName, attributeName, regex, parameterType, argumentName);
            return GenerateStepDefinition(method);
        }

        public ProjectFile GenerateHookBinding(string eventType, string name, string code = null, int? order = null, IList<string> hookTypeAttributeTags = null, IList<string> methodScopeAttributeTags = null, IList<string> classScopeAttributeTags = null)
        {
            string hookClass = GetHookBindingClass(eventType, name, code, order, hookTypeAttributeTags, methodScopeAttributeTags, classScopeAttributeTags);
            return GenerateBindingClassFile(hookClass);
        }

        public ProjectFile GenerateAsyncHookBindingIncludingLocking(string eventType, string name, string code = null, int? order = null, IList<string> hookTypeAttributeTags = null, IList<string> methodScopeAttributeTags = null, IList<string> classScopeAttributeTags = null)
        {
            string hookClass = GetAsyncHookIncludingLockingBindingClass(eventType, name, code, order, hookTypeAttributeTags, methodScopeAttributeTags, classScopeAttributeTags);
            return GenerateBindingClassFile(hookClass);
        }

        public abstract ProjectFile GenerateLoggerClass(string pathToLogFile);

        protected abstract string GetBindingCode(string methodName, string methodImplementation, string attributeName, string regex, ParameterType parameterType, string argumentName);

        protected abstract string GetLoggingStepDefinitionCode(string methodName, string attributeName, string regex, ParameterType parameterType, string argumentName);

        protected abstract string GetHookBindingClass(
            string hookType,
            string name,
            string code = "",
            int? order = null,
            IList<string> hookTypeAttributeTags = null,
            IList<string> methodScopeAttributeTags = null,
            IList<string> classScopeAttributeTags = null);

        protected abstract string GetAsyncHookIncludingLockingBindingClass(
            string hookType,
            string name,
            string code = "",
            int? order = null,
            IList<string> hookTypeAttributeTags = null,
            IList<string> methodScopeAttributeTags = null,
            IList<string> classScopeAttributeTags = null);

        protected bool IsStaticEvent(string eventType)
        {
            return eventType == "BeforeFeature" || eventType == "AfterFeature" || eventType == "BeforeTestRun" || eventType == "AfterTestRun";
        }

        protected bool EventSupportsTagsParameter(string eventType)
        {
            return eventType != "AfterTestRun" && eventType != "BeforeTestRun";
        }
    }
}

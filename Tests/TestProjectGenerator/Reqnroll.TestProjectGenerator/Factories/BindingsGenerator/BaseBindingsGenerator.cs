using System.Collections.Generic;
using Reqnroll.TestProjectGenerator.Data;
using Reqnroll.TestProjectGenerator.Driver;

namespace Reqnroll.TestProjectGenerator.Factories.BindingsGenerator
{
    public abstract class BaseBindingsGenerator
    {
        public const bool DefaultAsyncHook = false;

        public abstract ProjectFile GenerateBindingClassFile(string fileContent);

        public abstract ProjectFile GenerateStepDefinition(string method);

        public ProjectFile GenerateStepDefinition(string methodName, string methodImplementation, string attributeName, string regex, ParameterType parameterType = ParameterType.Normal, string argumentName = null)
        {
            string method = GetBindingCode(methodName, methodImplementation, attributeName, regex, parameterType, argumentName);
            return GenerateStepDefinition(method);
        }

        public ProjectFile GenerateLoggingStepDefinition(string methodName, string attributeName, string regex, ParameterType parameterType = ParameterType.Normal, string argumentName = null)
        {
            string method = GetLoggingStepDefinitionCode(methodName, attributeName, regex, parameterType, argumentName);
            return GenerateStepDefinition(method);
        }

        public ProjectFile GenerateHookBinding(string eventType, string name, string code = null, bool? asyncHook = null, int? order = null, IList<string> hookTypeAttributeTags = null, IList<string> methodScopeAttributeTags = null, IList<string> classScopeAttributeTags = null)
        {
            string hookClass = GetHookBindingClass(eventType, name, code, asyncHook, order, hookTypeAttributeTags, methodScopeAttributeTags, classScopeAttributeTags);
            return GenerateBindingClassFile(hookClass);
        }

        public abstract ProjectFile GenerateLoggerClass(string pathToLogFile);

        protected abstract string GetBindingCode(string methodName, string methodImplementation, string attributeName, string regex, ParameterType parameterType, string argumentName);

        protected abstract string GetLoggingStepDefinitionCode(string methodName, string attributeName, string regex, ParameterType parameterType, string argumentName);

        protected abstract string GetHookBindingClass(
            string hookType,
            string name,
            string code = "",
            bool? asyncHook = null,
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

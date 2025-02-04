using System;

namespace Reqnroll.RuntimeTests
{
    internal static class ScenarioContextExtensions
    {
        public static void SetBindingInstance(this ScenarioContext scenarioContext, Type bindingType, object instance)
        {
            scenarioContext.ScenarioContainer.RegisterInstanceAs(instance, bindingType);
        }
    }
}

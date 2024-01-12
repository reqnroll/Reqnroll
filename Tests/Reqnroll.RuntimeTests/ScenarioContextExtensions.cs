using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

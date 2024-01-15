using System;
using TechTalk.SpecFlow.TestProjectGenerator.Data;

namespace TechTalk.SpecFlow.TestProjectGenerator.Extensions
{
    public static class SpecFlowPluginTypeExtensions
    {
        public static string ToPluginTypeString(this SpecFlowPluginType pluginType)
        {
            switch (pluginType)
            {
                case SpecFlowPluginType.Generator | SpecFlowPluginType.Runtime:
                    return "GeneratorAndRuntime";
                case SpecFlowPluginType.Generator:
                    return "Generator";
                case SpecFlowPluginType.Runtime:
                    return "Runtime";
                default:
                    throw new ArgumentOutOfRangeException(nameof(pluginType));
            }
        }
    }
}

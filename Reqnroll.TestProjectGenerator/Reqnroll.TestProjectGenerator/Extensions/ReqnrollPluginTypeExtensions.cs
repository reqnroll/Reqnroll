using System;
using Reqnroll.TestProjectGenerator.Data;

namespace Reqnroll.TestProjectGenerator.Extensions
{
    public static class ReqnrollPluginTypeExtensions
    {
        public static string ToPluginTypeString(this ReqnrollPluginType pluginType)
        {
            switch (pluginType)
            {
                case ReqnrollPluginType.Generator | ReqnrollPluginType.Runtime:
                    return "GeneratorAndRuntime";
                case ReqnrollPluginType.Generator:
                    return "Generator";
                case ReqnrollPluginType.Runtime:
                    return "Runtime";
                default:
                    throw new ArgumentOutOfRangeException(nameof(pluginType));
            }
        }
    }
}

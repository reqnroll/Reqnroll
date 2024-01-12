using System;

namespace Reqnroll.Plugins
{
    [Flags]
    public enum PluginType
    {
        Generator = 1,
        Runtime = 2,
        GeneratorAndRuntime = Generator | Runtime
    }
}
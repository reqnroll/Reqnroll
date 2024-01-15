using System;

namespace TechTalk.SpecFlow.TestProjectGenerator.Data
{
    [Flags]
    public enum SpecFlowPluginType
    {
        Generator = 0b01,
        Runtime = 0b10
    }
}

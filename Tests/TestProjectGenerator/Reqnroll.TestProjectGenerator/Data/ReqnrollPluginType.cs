using System;

namespace Reqnroll.TestProjectGenerator.Data
{
    [Flags]
    public enum ReqnrollPluginType
    {
        Generator = 0b01,
        Runtime = 0b10
    }
}

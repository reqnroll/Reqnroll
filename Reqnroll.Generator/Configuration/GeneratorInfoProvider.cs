using System;

namespace Reqnroll.Generator.Configuration;

public static class GeneratorInfoProvider
{
    public static readonly Version GeneratorVersion = typeof(GeneratorInfoProvider).Assembly.GetName().Version;

    public static GeneratorInfo GetGeneratorInfo()
    {
        return new GeneratorInfo
        {
            GeneratorVersion = GeneratorVersion
        };
    }
}
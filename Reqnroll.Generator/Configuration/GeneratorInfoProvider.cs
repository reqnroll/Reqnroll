using System;

namespace Reqnroll.Generator.Configuration;

public class GeneratorInfoProvider : IGeneratorInfoProvider
{
    public static readonly Version GeneratorVersion = typeof(GeneratorInfoProvider).Assembly.GetName().Version;

    public GeneratorInfo GetGeneratorInfo()
    {
        return new GeneratorInfo
        {
            GeneratorVersion = GeneratorVersion
        };
    }
}
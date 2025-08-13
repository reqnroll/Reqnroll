using System;
using System.Collections.Generic;

namespace Reqnroll.Generator.Interfaces;

public interface ITestGeneratorFactory
{
    Version GetGeneratorVersion();
    ITestGenerator CreateGenerator(ProjectSettings projectSettings, IEnumerable<GeneratorPluginInfo> generatorPlugins);
}
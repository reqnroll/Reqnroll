using Reqnroll.Analytics;
using Reqnroll.BoDi;

namespace Reqnroll.Tools.MsBuild.Generation;

public interface IGenerateFeatureFileCodeBehindTaskDependencyCustomizations
{
    void CustomizeTaskContainerDependencies(IObjectContainer taskContainer);
    void CustomizeGeneratorContainerDependencies(IObjectContainer generatorContainer);
}

public class NullGenerateFeatureFileCodeBehindTaskDependencyCustomizations : IGenerateFeatureFileCodeBehindTaskDependencyCustomizations
{
    public void CustomizeTaskContainerDependencies(IObjectContainer taskContainer)
    {
        //nop
    }

    public void CustomizeGeneratorContainerDependencies(IObjectContainer generatorContainer)
    {
        //nop
    }
}

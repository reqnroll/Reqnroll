using Reqnroll.Infrastructure;
using Reqnroll.Plugins;
using Reqnroll.Tracing;
using Reqnroll.TUnit.ReqnrollPlugin;
using Reqnroll.UnitTestProvider;

[assembly: RuntimePlugin(typeof(RuntimePlugin))]

namespace Reqnroll.TUnit.ReqnrollPlugin;

public class RuntimePlugin : IRuntimePlugin
{
    public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
    {
        runtimePluginEvents.RegisterGlobalDependencies += RuntimePluginEvents_RegisterGlobalDependencies;
        runtimePluginEvents.CustomizeTestThreadDependencies += RuntimePluginEvents_CustomizeTestThreadDependencies;
        unitTestProviderConfiguration.UseUnitTestProvider("tunit");
    }

    private void RuntimePluginEvents_RegisterGlobalDependencies(object sender, RegisterGlobalDependenciesEventArgs e)
    {
        e.ObjectContainer.RegisterTypeAs<TUnitRuntimeProvider, IUnitTestRuntimeProvider>("tunit");
    }
    private void RuntimePluginEvents_CustomizeTestThreadDependencies(object sender, CustomizeTestThreadDependenciesEventArgs e)
    {
        var container = e.ObjectContainer;
        container.RegisterTypeAs<TUnitTraceListener, ITraceListener>();
        container.RegisterTypeAs<TUnitAttachmentHandler, IReqnrollAttachmentHandler>();
    }
}
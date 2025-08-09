using Reqnroll.Infrastructure;
using Reqnroll.Plugins;
using Reqnroll.Tracing;
using Reqnroll.UnitTestProvider;
using Reqnroll.xUnit3.ReqnrollPlugin;

[assembly: RuntimePlugin(typeof(RuntimePlugin))]

namespace Reqnroll.xUnit3.ReqnrollPlugin;

public class RuntimePlugin : IRuntimePlugin
{
    private const string UnitTestProviderName = "xunit3";

    public void Initialize(
        RuntimePluginEvents runtimePluginEvents,
        RuntimePluginParameters runtimePluginParameters,
        UnitTestProviderConfiguration unitTestProviderConfiguration)
    {
        runtimePluginEvents.RegisterGlobalDependencies += RuntimePluginEvents_RegisterGlobalDependencies;
        runtimePluginEvents.CustomizeTestThreadDependencies += RuntimePluginEvents_CustomizeTestThreadDependencies;
        unitTestProviderConfiguration.UseUnitTestProvider(UnitTestProviderName);
    }

    private void RuntimePluginEvents_RegisterGlobalDependencies(object sender, RegisterGlobalDependenciesEventArgs e)
    {
        e.ObjectContainer.RegisterTypeAs<XUnit3RuntimeProvider, IUnitTestRuntimeProvider>(UnitTestProviderName);
    }

    private void RuntimePluginEvents_CustomizeTestThreadDependencies(object sender, CustomizeTestThreadDependenciesEventArgs e)
    {
        var container = e.ObjectContainer;
        container.RegisterTypeAs<XUnit3TraceListener, ITraceListener>();
        container.RegisterTypeAs<ReqnrollAttachmentHandler, IReqnrollAttachmentHandler>();
    }
}
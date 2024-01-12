using Reqnroll.Infrastructure;
using Reqnroll.MSTest.ReqnrollPlugin;
using Reqnroll.Plugins;
using Reqnroll.Tracing;
using Reqnroll.UnitTestProvider;


[assembly: RuntimePlugin(typeof(RuntimePlugin))]

namespace Reqnroll.MSTest.ReqnrollPlugin
{
    public class RuntimePlugin : IRuntimePlugin
    {
        public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
        {
            unitTestProviderConfiguration.UseUnitTestProvider("mstest");
            runtimePluginEvents.RegisterGlobalDependencies += RuntimePluginEvents_RegisterGlobalDependencies;
            runtimePluginEvents.CustomizeTestThreadDependencies += RuntimePluginEventsOnCustomizeTestThreadDependencies;
        }

        private void RuntimePluginEvents_RegisterGlobalDependencies(object sender, RegisterGlobalDependenciesEventArgs e)
        {
            e.ObjectContainer.RegisterTypeAs<MsTestRuntimeProvider, IUnitTestRuntimeProvider>("mstest");
        }

        private void RuntimePluginEventsOnCustomizeTestThreadDependencies(object sender, CustomizeTestThreadDependenciesEventArgs e)
        {
            e.ObjectContainer.RegisterTypeAs<MSTestTraceListener, ITraceListener>();
            e.ObjectContainer.RegisterTypeAs<MSTestAttachmentHandler, IReqnrollAttachmentHandler>();
            e.ObjectContainer.RegisterTypeAs<MSTestTestContextProvider, IMSTestTestContextProvider>();
        }
    }
}

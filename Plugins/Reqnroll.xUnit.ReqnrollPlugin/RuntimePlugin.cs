using Reqnroll.Infrastructure;
using Reqnroll.Plugins;
using Reqnroll.Tracing;
using Reqnroll.UnitTestProvider;
using Reqnroll.xUnit.ReqnrollPlugin;


[assembly: RuntimePlugin(typeof(RuntimePlugin))]


namespace Reqnroll.xUnit.ReqnrollPlugin
{
    public class RuntimePlugin : IRuntimePlugin
    {
        public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
        {
            runtimePluginEvents.RegisterGlobalDependencies += RuntimePluginEvents_RegisterGlobalDependencies;
            runtimePluginEvents.CustomizeTestThreadDependencies += RuntimePluginEvents_CustomizeTestThreadDependencies;
            unitTestProviderConfiguration.UseUnitTestProvider("xunit");
        }

        private void RuntimePluginEvents_RegisterGlobalDependencies(object sender, RegisterGlobalDependenciesEventArgs e)
        {
            e.ObjectContainer.RegisterTypeAs<XUnitRuntimeProvider, IUnitTestRuntimeProvider>("xunit");
        }

        private void RuntimePluginEvents_CustomizeTestThreadDependencies(object sender, CustomizeTestThreadDependenciesEventArgs e)
        {
            var container = e.ObjectContainer;

            container.RegisterTypeAs<XUnitTraceListener, ITraceListener>();
            container.RegisterTypeAs<ReqnrollAttachmentHandler, IReqnrollAttachmentHandler>();
        }
    }
}

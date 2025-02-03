using Reqnroll.Infrastructure;
using Reqnroll.NUnit.ReqnrollPlugin;
using Reqnroll.Plugins;
using Reqnroll.Tracing;
using Reqnroll.UnitTestProvider;

[assembly: RuntimePlugin(typeof(RuntimePlugin))]

namespace Reqnroll.NUnit.ReqnrollPlugin
{
    public class RuntimePlugin : IRuntimePlugin
    {
        public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
        {
            runtimePluginEvents.RegisterGlobalDependencies += RuntimePluginEventsOnRegisterGlobalDependencies;
            runtimePluginEvents.CustomizeGlobalDependencies += RuntimePluginEvents_CustomizeGlobalDependencies;
            runtimePluginEvents.CustomizeTestThreadDependencies += RuntimePluginEventsOnCustomizeTestThreadDependencies;
            unitTestProviderConfiguration.UseUnitTestProvider("nunit");
        }

        private void RuntimePluginEventsOnRegisterGlobalDependencies(object sender, RegisterGlobalDependenciesEventArgs e)
        {
            e.ObjectContainer.RegisterTypeAs<NUnitRuntimeProvider, IUnitTestRuntimeProvider>("nunit");
        }

        private void RuntimePluginEvents_CustomizeGlobalDependencies(object sender, CustomizeGlobalDependenciesEventArgs e)
        {
#if NETFRAMEWORK
            e.ObjectContainer.RegisterTypeAs<NUnitNetFrameworkTestRunSettingsProvider, ITestRunSettingsProvider>();
#endif
        }

        private void RuntimePluginEventsOnCustomizeTestThreadDependencies(object sender, CustomizeTestThreadDependenciesEventArgs e)
        {
            e.ObjectContainer.RegisterTypeAs<NUnitTraceListener, ITraceListener>();
            e.ObjectContainer.RegisterTypeAs<NUnitAttachmentHandler, IReqnrollAttachmentHandler>();
        }
    }
}

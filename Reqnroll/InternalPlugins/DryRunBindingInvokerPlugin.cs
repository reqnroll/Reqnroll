using Reqnroll.Bindings;
using Reqnroll.CommonModels;
using Reqnroll.EnvironmentAccess;
using Reqnroll.Plugins;
using Reqnroll.UnitTestProvider;

namespace Reqnroll.InternalPlugins;

internal class DryRunBindingInvokerPlugin : IRuntimePlugin
{
    internal const string DryRunEnvVarName = "REQNROLL__DRY_RUN";

    public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
    {
        runtimePluginEvents.CustomizeGlobalDependencies += (_, args) =>
        {
            var environment = args.ObjectContainer.Resolve<IEnvironmentWrapper>();
            if (environment.GetEnvironmentVariable(DryRunEnvVarName) is ISuccess<string> dryRunEnvVar
                && bool.TryParse(dryRunEnvVar.Result, out bool useDryRun)
                && useDryRun)
            {
                args.ObjectContainer.RegisterTypeAs<DryRunBindingInvoker, IAsyncBindingInvoker>();
#pragma warning disable CS0618
                args.ObjectContainer.RegisterTypeAs<DryRunBindingInvoker, IBindingInvoker>();
#pragma warning restore CS0618
            }
        };
    }
}

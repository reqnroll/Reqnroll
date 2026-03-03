using Reqnroll.Bindings;
using Reqnroll.BoDi;
using Reqnroll.Configuration;
using Reqnroll.Formatters.Configuration;
using Reqnroll.Infrastructure;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Reqnroll.ServiceAPI
{
    public partial class ReqnrollServiceAPIEndpoint
    {
        private static IObjectContainer _globalContainer;

        /// <summary>
        /// Invoked by the Visual Studio Extension service. Do not remove or change the signature.
        /// </summary>
        public static string Initialize(Assembly testAssembly, string jsonConfiguration)
        {
            if (string.IsNullOrWhiteSpace(jsonConfiguration)) jsonConfiguration = "{}";
            _globalContainer = CreateGlobalContainer(testAssembly, jsonConfiguration);
            return "success";
        }

        /// <summary>
        /// Invoked by the Visual Studio Extension service. Do not remove or change the signature.
        /// </summary>
        public static string IsReqrnollInitialized()
        {
            return _globalContainer != null ? "true" : "false";
        }  
        class ServiceAPIDependencyProvider : DefaultDependencyProvider
        {
            public override void RegisterGlobalContainerDefaults(ObjectContainer container)
            {
                base.RegisterGlobalContainerDefaults(container);
                container.RegisterTypeAs<DryRunBindingInvoker, IAsyncBindingInvoker>();
                container.RegisterTypeAs<Formatters.Configuration.FormattersForcedDisabledOverrideProvider, IFormattersConfigurationDisableOverrideProvider>();
            }
        }

        private static IObjectContainer CreateGlobalContainer(Assembly testAssembly, string jsonConfiguration)
        {
            var containerBuilder = new ContainerBuilder(new ServiceAPIDependencyProvider())
            {
                SkipLoadingProvider = true
            };
            var configurationProvider = new JsonStringRuntimeConfigurationProvider(jsonConfiguration);
            return containerBuilder.CreateGlobalContainer(testAssembly, configurationProvider);
        }

    }
}

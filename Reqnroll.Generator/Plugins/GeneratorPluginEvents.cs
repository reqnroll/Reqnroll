using System;
using Reqnroll.BoDi;
using Reqnroll.Generator.Configuration;

namespace Reqnroll.Generator.Plugins
{
    public class GeneratorPluginEvents
    {
        public event EventHandler<RegisterDependenciesEventArgs> RegisterDependencies;
        public event EventHandler<CustomizeDependenciesEventArgs> CustomizeDependencies;
        public event EventHandler<ConfigurationDefaultsEventArgs> ConfigurationDefaults;

        public void RaiseRegisterDependencies(ObjectContainer objectContainer)
        {
            RegisterDependencies?.Invoke(this, new RegisterDependenciesEventArgs(objectContainer));
        }

        public void RaiseConfigurationDefaults(ReqnrollProjectConfiguration reqnrollProjectConfiguration)
        {
            ConfigurationDefaults?.Invoke(this, new ConfigurationDefaultsEventArgs(reqnrollProjectConfiguration));
        }

        public void RaiseCustomizeDependencies(ObjectContainer container, ReqnrollProjectConfiguration reqnrollProjectConfiguration)
        {
            CustomizeDependencies?.Invoke(this, new CustomizeDependenciesEventArgs(container, reqnrollProjectConfiguration));
        }
    }
}

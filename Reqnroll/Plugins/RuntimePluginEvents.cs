using System;
using Reqnroll.BoDi;
using Reqnroll.Configuration;

namespace Reqnroll.Plugins
{
    public class RuntimePluginEvents
    {
        public event EventHandler<RegisterGlobalDependenciesEventArgs> RegisterGlobalDependencies;
        public event EventHandler<CustomizeGlobalDependenciesEventArgs> CustomizeGlobalDependencies;
        public event EventHandler<ConfigurationDefaultsEventArgs> ConfigurationDefaults;
        public event EventHandler<CustomizeTestThreadDependenciesEventArgs> CustomizeTestThreadDependencies;
        public event EventHandler<CustomizeFeatureDependenciesEventArgs> CustomizeFeatureDependencies;
        public event EventHandler<CustomizeScenarioDependenciesEventArgs> CustomizeScenarioDependencies;

        public void RaiseRegisterGlobalDependencies(ObjectContainer objectContainer)
        {
            RegisterGlobalDependencies?.Invoke(this, new RegisterGlobalDependenciesEventArgs(objectContainer));
        }

        public void RaiseConfigurationDefaults(Configuration.ReqnrollConfiguration reqnrollConfiguration)
        {
            ConfigurationDefaults?.Invoke(this, new ConfigurationDefaultsEventArgs(reqnrollConfiguration));
        }

        public void RaiseCustomizeGlobalDependencies(ObjectContainer container, ReqnrollConfiguration reqnrollConfiguration)
        {
            CustomizeGlobalDependencies?.Invoke(this, new CustomizeGlobalDependenciesEventArgs(container, reqnrollConfiguration));
        }

        public void RaiseCustomizeTestThreadDependencies(ObjectContainer testThreadContainer)
        {
            CustomizeTestThreadDependencies?.Invoke(this, new CustomizeTestThreadDependenciesEventArgs(testThreadContainer));
        }

        public void RaiseCustomizeFeatureDependencies(ObjectContainer featureContainer)
        {
            CustomizeFeatureDependencies?.Invoke(this, new CustomizeFeatureDependenciesEventArgs(featureContainer));
        }

        public void RaiseCustomizeScenarioDependencies(ObjectContainer scenarioContainer)
        {
            CustomizeScenarioDependencies?.Invoke(this, new CustomizeScenarioDependenciesEventArgs(scenarioContainer));
        }
    }
}
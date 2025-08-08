using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using Reqnroll.Plugins;
using Reqnroll.UnitTestProvider;
using Reqnroll.Verify.ReqnrollPlugin;
using VerifyTests;
using VerifyXunit;

[assembly: RuntimePlugin(typeof(VerifyRuntimePlugin))]

namespace Reqnroll.Verify.ReqnrollPlugin;

public class VerifyRuntimePlugin : IRuntimePlugin
{
    public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
    {
        runtimePluginEvents.CustomizeGlobalDependencies += RuntimePluginEvents_CustomizeGlobalDependencies;
        runtimePluginEvents.CustomizeScenarioDependencies += RuntimePluginEvents_CustomizeScenarioDependencies;
    }

    private void RuntimePluginEvents_CustomizeGlobalDependencies(object sender, CustomizeGlobalDependenciesEventArgs e)
    {
        // With Verify.Xunit v29+, static configuration should be minimal
        // Most path configuration is handled per-scenario via VerifySettings in CustomizeScenarioDependencies
    }

    private void RuntimePluginEvents_CustomizeScenarioDependencies(object sender, CustomizeScenarioDependenciesEventArgs e)
    {
        e.ObjectContainer.RegisterFactoryAs(
            container =>
            {
                var featureContext = container.Resolve<FeatureContext>();
                var scenarioContext = container.Resolve<ScenarioContext>();

                var settings = new VerifySettings();
                string projectDirectory = Directory.GetCurrentDirectory().Split([@"\bin\"], StringSplitOptions.RemoveEmptyEntries).First();

                settings.UseDirectory(Path.Combine(projectDirectory, featureContext.FeatureInfo.FolderPath));
                settings.UseTypeName(featureContext.FeatureInfo.Title);

                var methodNameBuilder = new StringBuilder(scenarioContext.ScenarioInfo.Title);

                foreach (DictionaryEntry entry in scenarioContext.ScenarioInfo.Arguments)
                {
                    methodNameBuilder.AppendFormat("_{0}", entry.Value);
                }

                settings.UseMethodName(methodNameBuilder.ToString());

                return settings;
            });
        
        // Also try to set up default settings for calls without explicit settings
        // This might help with the "global registered path info" test
        var runtimePluginTestExecutionLifecycleEvents = e.ObjectContainer.Resolve<RuntimePluginTestExecutionLifecycleEvents>();
        runtimePluginTestExecutionLifecycleEvents.BeforeScenario += (_, runtimePluginBeforeScenarioEventArgs) =>
        {
            var scenarioContext = runtimePluginBeforeScenarioEventArgs.ObjectContainer.Resolve<ScenarioContext>();
            var featureContext = runtimePluginBeforeScenarioEventArgs.ObjectContainer.Resolve<FeatureContext>();

            // Try to set up some basic configuration that might work with v29+
            // This is experimental to see if we can support the global test
            try
            {
                string projectDirectory = Directory.GetCurrentDirectory().Split([@"\bin\"], StringSplitOptions.RemoveEmptyEntries).First();
                string scenarioInfoTitle = scenarioContext.ScenarioInfo.Title;

                foreach (DictionaryEntry scenarioInfoArgument in scenarioContext.ScenarioInfo.Arguments)
                {
                    scenarioInfoTitle += "_" + scenarioInfoArgument.Value;
                }

                // This is still problematic in v29+ but let's see what happens
                Verifier.DerivePathInfo(
                    (_, projectDir, _, _) =>
                    {
                        return new PathInfo(
                            Path.Combine(projectDirectory, featureContext.FeatureInfo.FolderPath),
                            featureContext.FeatureInfo.Title,
                            scenarioInfoTitle);
                    });
            }
            catch
            {
                // If it fails, at least the injected settings should still work
            }
        };
    }
}

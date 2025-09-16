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
    private static FeatureContext featureContext;
    private static ScenarioContext scenarioContext;

    public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
    {
        runtimePluginEvents.CustomizeGlobalDependencies += RuntimePluginEvents_CustomizeGlobalDependencies;
        runtimePluginEvents.CustomizeScenarioDependencies += RuntimePluginEvents_CustomizeScenarioDependencies;
        Verifier.DerivePathInfo(
            (_, projectDirectory, _, _) =>
            {
                string scenarioInfoTitle = scenarioContext.ScenarioInfo.Title;

                foreach (DictionaryEntry scenarioInfoArgument in scenarioContext.ScenarioInfo.Arguments)
                {
                    scenarioInfoTitle += "_" + scenarioInfoArgument.Value;
                }

                return new PathInfo(
                    Path.Combine(projectDirectory, featureContext.FeatureInfo.FolderPath),
                    featureContext.FeatureInfo.Title,
                    scenarioInfoTitle);
            });
    }

    private void RuntimePluginEvents_CustomizeGlobalDependencies(object sender, CustomizeGlobalDependenciesEventArgs e)
    {
        var runtimePluginTestExecutionLifecycleEvents = e.ObjectContainer.Resolve<RuntimePluginTestExecutionLifecycleEvents>();
        runtimePluginTestExecutionLifecycleEvents.BeforeScenario += (_, runtimePluginBeforeScenarioEventArgs) =>
        {
            scenarioContext = runtimePluginBeforeScenarioEventArgs.ObjectContainer.Resolve<ScenarioContext>();
            featureContext = runtimePluginBeforeScenarioEventArgs.ObjectContainer.Resolve<FeatureContext>();
        };
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
    }
}

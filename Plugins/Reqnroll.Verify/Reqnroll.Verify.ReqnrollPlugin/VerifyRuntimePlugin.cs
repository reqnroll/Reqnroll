using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
    }

    private void RuntimePluginEvents_CustomizeGlobalDependencies(object sender, CustomizeGlobalDependenciesEventArgs e)
    {
        var runtimePluginTestExecutionLifecycleEvents = e.ObjectContainer.Resolve<RuntimePluginTestExecutionLifecycleEvents>();
        runtimePluginTestExecutionLifecycleEvents.BeforeScenario += RuntimePluginTestExecutionLifecycleEvents_BeforeScenario;
    }

    private void RuntimePluginTestExecutionLifecycleEvents_BeforeScenario(object sender, RuntimePluginBeforeScenarioEventArgs e)
    {
        RegisterGlobal(e);

        RegisterPerScenario(e);
    }

    private static void RegisterPerScenario(RuntimePluginBeforeScenarioEventArgs e)
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

    private static void RegisterGlobal(RuntimePluginBeforeScenarioEventArgs e)
    {
        var scenarioContext = e.ObjectContainer.Resolve<ScenarioContext>();
        var featureContext = e.ObjectContainer.Resolve<FeatureContext>();

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
}

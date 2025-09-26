using Reqnroll.Plugins;
using Reqnroll.UnitTestProvider;
using Reqnroll.Verify.ReqnrollPlugin;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using VerifyTests;

[assembly: RuntimePlugin(typeof(VerifyRuntimePlugin))]

namespace Reqnroll.Verify.ReqnrollPlugin;

public class VerifyRuntimePlugin : IRuntimePlugin
{
    public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
    {
        runtimePluginEvents.CustomizeScenarioDependencies += RuntimePluginEvents_CustomizeScenarioDependencies;
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

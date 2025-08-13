using Reqnroll.Bindings.Discovery;
using Reqnroll.Bindings.Provider.Data;
using Reqnroll.Bindings.Reflection;
using Reqnroll.BoDi;
using Reqnroll.Configuration;
using Reqnroll.Formatters.Configuration;
using Reqnroll.Infrastructure;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace Reqnroll.Bindings.Provider;

public class BindingProviderService
{
    public static string DiscoverBindings(Assembly testAssembly, string jsonConfiguration)
    {
        if (string.IsNullOrWhiteSpace(jsonConfiguration)) jsonConfiguration = "{}";
        var globalContainer = CreateGlobalContainer(testAssembly, jsonConfiguration);
        var bindingRegistryBuilder = globalContainer.Resolve<IRuntimeBindingRegistryBuilder>();
        BuildBindingRegistry(testAssembly, bindingRegistryBuilder);
        var bindingRegistry = globalContainer.Resolve<IBindingRegistry>();
        var resultData = ParseDiscoveryResult(bindingRegistry, testAssembly);
        var jsonString = JsonSerializer.Serialize(resultData, BindingJsonSourceGenerator.Default.BindingData);
        return jsonString;
    }

    private static BindingData ParseDiscoveryResult(IBindingRegistry bindingRegistry, Assembly testAssembly)
    {
        var resultData = new BindingData();

        StepDefinitionData CreateStepDefinition(IStepDefinitionBinding stepDefinitionBinding)
        {
            var stepDefinition = new StepDefinitionData
            {
                Source = GetSource(stepDefinitionBinding.Method),
                Type = stepDefinitionBinding.StepDefinitionType.ToString(),
                Regex = stepDefinitionBinding.Regex?.ToString(),
                ParamTypes = GetParamTypes(stepDefinitionBinding.Method),
                Scope = GetScope(stepDefinitionBinding),
                Expression = stepDefinitionBinding.SourceExpression,
                Error = stepDefinitionBinding.ErrorMessage
            };

            return stepDefinition;
        }

        HookData CreateHook(IHookBinding hookBinding)
        {
            var hook = new HookData
            {
                Source = GetSource(hookBinding.Method),
                Type = hookBinding.HookType.ToString(),
                HookOrder = hookBinding.HookOrder,
                Scope = GetScope(hookBinding),
            };

            return hook;
        }

        StepArgumentTransformationData CreateStepArgumentTransformation(IStepArgumentTransformationBinding stepArgumentTransformationBinding)
        {
            var stepArgumentTransformation = new StepArgumentTransformationData
            {
                Source = GetSource(stepArgumentTransformationBinding.Method),
                Name = stepArgumentTransformationBinding.Name,
                Regex = stepArgumentTransformationBinding.Regex?.ToString(),
                ParamTypes = GetParamTypes(stepArgumentTransformationBinding.Method),
            };

            return stepArgumentTransformation;
        }

        string[] GetParamTypes(IBindingMethod bindingMethod)
        {
            return bindingMethod.Parameters.Select(p => p.Type.FullName).ToArray();
        }

        BindingScopeData GetScope(IScopedBinding scopedBinding)
        {
            if (!scopedBinding.IsScoped)
                return null;

            return new BindingScopeData
            {
                Tag = scopedBinding.BindingScope.Tag == null
                    ? null
                    : "@" + scopedBinding.BindingScope.Tag,
                FeatureTitle = scopedBinding.BindingScope.FeatureTitle,
                ScenarioTitle = scopedBinding.BindingScope.ScenarioTitle
            };
        }

        BindingSourceData GetSource(IBindingMethod bindingMethod)
        {
            if (bindingMethod is not RuntimeBindingMethod runtimeBindingMethod ||
                runtimeBindingMethod.MethodInfo.DeclaringType == null) return null;

            var methodInfo = runtimeBindingMethod.MethodInfo;
            return new BindingSourceData
            {
                Method = new BindingSourceMethodData
                {
                    Type = methodInfo.DeclaringType!.FullName,
                    Assembly = methodInfo.DeclaringType.Assembly == testAssembly ? null : methodInfo.DeclaringType.Assembly.FullName,
                    MetadataToken = methodInfo.MetadataToken,
                    FullName = methodInfo.ToString()
                }
            };
        }

        resultData.StepDefinitions = bindingRegistry.GetStepDefinitions().Select(CreateStepDefinition).ToArray();
        resultData.Hooks = bindingRegistry.GetHooks().Select(CreateHook).ToArray();
        resultData.StepArgumentTransformations = bindingRegistry.GetStepTransformations().Select(CreateStepArgumentTransformation).ToArray();
        resultData.Errors = bindingRegistry.GetErrorMessages().Select(e => $"{e.Type}: {e.Message}").ToArray();
        return resultData;
    }

    private static void BuildBindingRegistry(Assembly testAssembly, IRuntimeBindingRegistryBuilder bindingRegistryBuilder)
    {
        var bindingAssemblies = bindingRegistryBuilder.GetBindingAssemblies(testAssembly);
        foreach (Assembly assembly in bindingAssemblies)
        {
            bindingRegistryBuilder.BuildBindingsFromAssembly(assembly);
        }
        bindingRegistryBuilder.BuildingCompleted();
    }

    class BindingDiscoveryDependencyProvider : DefaultDependencyProvider
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
        var containerBuilder = new ContainerBuilder(new BindingDiscoveryDependencyProvider())
        {
            SkipLoadingProvider = true
        };
        var configurationProvider = new JsonStringRuntimeConfigurationProvider(jsonConfiguration);
        return containerBuilder.CreateGlobalContainer(testAssembly, configurationProvider);
    }

}

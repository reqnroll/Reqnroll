using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Reqnroll.Autofac.ReqnrollPlugin;
using ContainerBuilder = Autofac.ContainerBuilder;

namespace Reqnroll.Autofac;

public class ContainerBuilderFinder : IContainerBuilderFinder
{
    private readonly IConfigurationMethodsProvider _configurationMethodsProvider;
    private readonly Lazy<Func<ContainerBuilder, ContainerBuilder>> _createConfigureGlobalContainer;
    private readonly Lazy<Func<ContainerBuilder, ContainerBuilder>> _createConfigureScenarioContainer;
    private readonly Lazy<Func<ContainerBuilder, ContainerBuilder>> _legacyCreateScenarioContainerBuilder;
    private readonly Lazy<Func<ILifetimeScope>> _getFeatureLifetimeScope;

    public ContainerBuilderFinder(IConfigurationMethodsProvider configurationMethodsProvider)
    {
        _configurationMethodsProvider = configurationMethodsProvider;

        static ContainerBuilder InvokeVoidAndReturnBuilder(ContainerBuilder containerBuilder, MethodInfo methodInfo)
        {
            methodInfo.Invoke(null, [ containerBuilder ]);
            return containerBuilder;
        }
        _createConfigureGlobalContainer = new Lazy<Func<ContainerBuilder, ContainerBuilder>>(() => FindCreateScenarioContainerBuilder(typeof(GlobalDependenciesAttribute), typeof(void), InvokeVoidAndReturnBuilder), true);
        _createConfigureScenarioContainer = new Lazy<Func<ContainerBuilder, ContainerBuilder>>(() => FindCreateScenarioContainerBuilder(typeof(ScenarioDependenciesAttribute), typeof(void), InvokeVoidAndReturnBuilder), true);
        _legacyCreateScenarioContainerBuilder = new Lazy<Func<ContainerBuilder, ContainerBuilder>>(() => FindCreateScenarioContainerBuilder(typeof(ScenarioDependenciesAttribute), typeof(ContainerBuilder), (_, m) => (ContainerBuilder)m.Invoke(null, null)), true);
        _getFeatureLifetimeScope = new Lazy<Func<ILifetimeScope>>(() => FindLifetimeScope(typeof(FeatureDependenciesAttribute), typeof(ILifetimeScope), m => (ILifetimeScope)m.Invoke(null, null)));
    }

    public Func<ContainerBuilder, ContainerBuilder> GetConfigureGlobalContainer()
    {
        return _createConfigureGlobalContainer.Value;
    }

    public Func<ContainerBuilder, ContainerBuilder> GetConfigureScenarioContainer()
    {
        return _createConfigureScenarioContainer.Value;
    }

    // For legacy support: configuration methods that return a container builder.
    // It is recommended to use the void methods that get a container builder as a parameter
    public Func<ContainerBuilder, ContainerBuilder> GetLegacyCreateScenarioContainerBuilder()
    {
        return _legacyCreateScenarioContainerBuilder.Value;
    }

    public Func<ILifetimeScope> GetFeatureLifetimeScope()
    {
        return _getFeatureLifetimeScope.Value;
    }

    protected virtual Func<ILifetimeScope> FindLifetimeScope(Type attributeType, Type returnType, Func<MethodInfo, ILifetimeScope> invoke)
    {
        var method = GetMethod(attributeType, returnType);

        return method == null
            ? null
            : () => invoke(method);
    }

    protected virtual Func<ContainerBuilder, ContainerBuilder> FindCreateScenarioContainerBuilder(Type attributeType, Type returnType, Func<ContainerBuilder, MethodInfo, ContainerBuilder> invoke)
    {
        var method = GetMethod(attributeType, returnType);

        return method == null
            ? null
            : containerBuilder => invoke(containerBuilder, method);
    }

    private MethodInfo GetMethod(Type attributeType, Type returnType)
    {
        return _configurationMethodsProvider.GetConfigurationMethods()
                                            .Where(x => x.ReturnType == returnType)
                                            .FirstOrDefault(x => Attribute.IsDefined(x, attributeType));
    }
}
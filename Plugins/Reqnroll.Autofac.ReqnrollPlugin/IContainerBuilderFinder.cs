using System;
using Autofac;

namespace Reqnroll.Autofac
{
    public interface IContainerBuilderFinder
    {
        Func<ContainerBuilder, ContainerBuilder> GetConfigureScenarioContainer();

        Func<ContainerBuilder, ContainerBuilder> GetConfigureGlobalContainer();

        Func<ContainerBuilder, ContainerBuilder> GetLegacyCreateScenarioContainerBuilder();

        Func<ILifetimeScope> GetFeatureLifetimeScope();
    }
}
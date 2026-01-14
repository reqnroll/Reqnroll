using System;

namespace Reqnroll.Microsoft.Extensions.DependencyInjection
{
    public enum ScopeLevelType
    {
        /// <summary>
        /// Scoping is created for every Scenario and it is destroyed once the Scenario ends.
        /// </summary>
        Scenario,
        /// <summary>
        /// Scoping is created for every Feature and it is destroyed once the Feature ends.
        /// </summary>
        Feature
    }

    public enum ServiceProviderLifetimeType
    {
        /// <summary>
        /// Global lifetime. The container is created once for the entire test run.
        /// </summary>
        Global,
        /// <summary>
        /// Test thread lifetime. The container is created once for each test thread.
        /// </summary>
        TestThread,
        /// <summary>
        /// Feature lifetime. The container is created once for each feature.
        /// </summary>
        Feature,
        /// <summary>
        /// Scenario lifetime. The container is created once for each scenario.
        /// </summary>
        Scenario
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class ScenarioDependenciesAttribute : Attribute
    {
        /// <summary>
        /// Automatically register all Reqnroll bindings.
        /// </summary>
        public bool AutoRegisterBindings { get; set; } = true;

        /// <summary>
        /// Define when to create and destroy scope. 
        /// </summary>
        public ScopeLevelType ScopeLevel { get; set; } = ScopeLevelType.Scenario;

        /// <summary>
        /// Define the lifetime of the Service Provider instance.
        /// </summary>
        public ServiceProviderLifetimeType ServiceProviderLifetime { get; set; } = ServiceProviderLifetimeType.Global;
    }
}

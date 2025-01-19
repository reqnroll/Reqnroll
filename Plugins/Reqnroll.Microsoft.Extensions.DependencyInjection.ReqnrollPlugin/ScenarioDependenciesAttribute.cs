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
    }
}

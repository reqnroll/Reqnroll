using System;

namespace Reqnroll.Windsor
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ScenarioDependenciesAttribute : Attribute
    {
        public bool AutoRegisterBindings { get; set; } = true;
    }
}

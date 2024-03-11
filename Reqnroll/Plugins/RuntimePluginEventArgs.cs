using System;
using Reqnroll.BoDi;
using Reqnroll.Configuration;

namespace Reqnroll.Plugins
{
    public abstract class ObjectContainerEventArgs : EventArgs
    {
        protected ObjectContainerEventArgs(ObjectContainer objectContainer)
        {
            ObjectContainer = objectContainer;
        }

        public ObjectContainer ObjectContainer { get; private set; }
    }

    public class RegisterGlobalDependenciesEventArgs : ObjectContainerEventArgs
    {
        public RegisterGlobalDependenciesEventArgs(ObjectContainer objectContainer) : base(objectContainer)
        {
        }
    }

    public class CustomizeGlobalDependenciesEventArgs : ObjectContainerEventArgs
    {
        public CustomizeGlobalDependenciesEventArgs(ObjectContainer objectContainer, ReqnrollConfiguration reqnrollConfiguration)
            : base(objectContainer)
        {
            ReqnrollConfiguration = reqnrollConfiguration;
        }

        public ReqnrollConfiguration ReqnrollConfiguration { get; private set; }
    }

    public class ConfigurationDefaultsEventArgs : EventArgs
    {
        public ConfigurationDefaultsEventArgs(ReqnrollConfiguration reqnrollConfiguration)
        {
            ReqnrollConfiguration = reqnrollConfiguration;
        }

        public ReqnrollConfiguration ReqnrollConfiguration { get; private set; }
    }

    public class CustomizeTestThreadDependenciesEventArgs : ObjectContainerEventArgs
    {
        public CustomizeTestThreadDependenciesEventArgs(ObjectContainer objectContainer) : base(objectContainer)
        {
        }
    }

    public class CustomizeFeatureDependenciesEventArgs : ObjectContainerEventArgs
    {
        public CustomizeFeatureDependenciesEventArgs(ObjectContainer objectContainer) : base(objectContainer)
        {
        }
    }

    public class CustomizeScenarioDependenciesEventArgs : ObjectContainerEventArgs
    {
        public CustomizeScenarioDependenciesEventArgs(ObjectContainer objectContainer) : base(objectContainer)
        {
        }
    }
}
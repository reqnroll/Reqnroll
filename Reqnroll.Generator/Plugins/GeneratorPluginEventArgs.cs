using System;
using Reqnroll.BoDi;
using Reqnroll.Generator.Configuration;
using Reqnroll.Plugins;

namespace Reqnroll.Generator.Plugins
{
    public class RegisterDependenciesEventArgs : ObjectContainerEventArgs
    {
        public RegisterDependenciesEventArgs(ObjectContainer objectContainer) : base(objectContainer)
        {
        }
    }

    public class CustomizeDependenciesEventArgs : ObjectContainerEventArgs
    {
        public CustomizeDependenciesEventArgs(ObjectContainer objectContainer, ReqnrollProjectConfiguration reqnrollProjectConfiguration)
            : base(objectContainer)
        {
            this.ReqnrollProjectConfiguration = reqnrollProjectConfiguration;
        }

        public ReqnrollProjectConfiguration ReqnrollProjectConfiguration { get; private set; }
    }

    public class ConfigurationDefaultsEventArgs : EventArgs
    {
        public ConfigurationDefaultsEventArgs(ReqnrollProjectConfiguration reqnrollProjectConfiguration)
        {
            this.ReqnrollProjectConfiguration = reqnrollProjectConfiguration;
        }

        public ReqnrollProjectConfiguration ReqnrollProjectConfiguration { get; private set; }
    }
}

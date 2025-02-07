using System;
using Autofac;
using Reqnroll.BoDi;
using Reqnroll.Infrastructure;

namespace Reqnroll.Autofac
{
    public class AutofacTestObjectResolver : ITestObjectResolver
    {
        public object ResolveBindingInstance(Type bindingType, IObjectContainer container)
        {
            var componentContext = container switch
            {
                _ when container.IsRegistered<IComponentContext>() => container.Resolve<IComponentContext>(),
                _ when container.IsRegistered<ILifetimeScope>() => container.Resolve<ILifetimeScope>(),
                _ when container.IsRegistered<IContainer>() => container.Resolve<IContainer>(),
                _ => null
            };

            if (componentContext == null)
                return container.Resolve(bindingType);

            if (container.IsRegistered(bindingType))
            {
                // If the requested object is registered in the Reqnroll container, 
                // we give a chance to the Autofac container to resolve
                // but fall back to the Reqnroll one.
                if (componentContext.TryResolve(bindingType, out object instance))
                    return instance;
                return container.Resolve(bindingType);
            }

            // If this is not in Reqnroll container we get it from Autofac and let it fail if it's not there.
            return componentContext.Resolve(bindingType);
        }
    }
}

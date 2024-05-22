using System;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll.BoDi;
using Reqnroll.Infrastructure;

namespace Reqnroll.Microsoft.Extensions.DependencyInjection
{
    public class DependencyInjectionTestObjectResolver : ITestObjectResolver
    {
        public object ResolveBindingInstance(Type bindingType, IObjectContainer container)
        {
            var registered = IsRegistered(container, bindingType);

            return registered
                ? container.Resolve(bindingType)
                : container.Resolve<IServiceProvider>().GetRequiredService(bindingType);
        }

        private bool IsRegistered(IObjectContainer container, Type type)
        {
            if (container.IsRegistered(type))
            {
                return true;
            }

            // IObjectContainer.IsRegistered is not recursive, it will only check the current container
            if (container is ObjectContainer c && c.BaseContainer != null)
            {
                return IsRegistered(c.BaseContainer, type);
            }

            return false;
        }
    }
}

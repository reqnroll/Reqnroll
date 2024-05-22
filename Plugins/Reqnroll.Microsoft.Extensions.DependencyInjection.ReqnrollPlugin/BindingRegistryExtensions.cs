﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Reqnroll.Bindings;
using Reqnroll.Bindings.Reflection;

namespace Reqnroll.Microsoft.Extensions.DependencyInjection
{
    public static class BindingRegistryExtensions
    {
        public static IEnumerable<IBindingType> GetBindingTypes(this IBindingRegistry bindingRegistry)
        {
            return bindingRegistry.GetStepDefinitions().Cast<IBinding>()
                .Concat(bindingRegistry.GetHooks().Cast<IBinding>())
                .Concat(bindingRegistry.GetStepTransformations())
                .Select(b => b.Method.Type)
                .Distinct();
        }

        public static IEnumerable<Assembly> GetBindingAssemblies(this IBindingRegistry bindingRegistry)
        {
            return bindingRegistry.GetBindingTypes().OfType<RuntimeBindingType>()
                .Select(t => t.Type.Assembly)
                .Distinct();
        }
    }
}

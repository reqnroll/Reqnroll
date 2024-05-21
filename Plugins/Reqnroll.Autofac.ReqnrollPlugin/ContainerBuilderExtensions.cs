using System;
using System.Reflection;
using Autofac;

namespace Reqnroll.Autofac.ReqnrollPlugin
{
    /// <summary>
    /// Container builder extension methods.
    /// </summary>
    public static class ContainerBuilderExtensions
    {
        /// <summary>
        /// Add Reqnroll binding for classes in the assembly where typeof TAssemblyType resides.
        /// </summary>
        /// <typeparam name="TAssemblyType">Any type in an assembly to search for bindings.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <returns>The builder.</returns>
        public static ContainerBuilder AddReqnrollBindings<TAssemblyType>(this ContainerBuilder builder) => builder.AddReqnrollBindings(typeof(TAssemblyType));

        /// <summary>
        /// Add Reqnroll binding for classes in the assembly where the type resides.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="type">Any type in an assembly to search for bindings.</param>
        /// <returns>The builder.</returns>
        public static ContainerBuilder AddReqnrollBindings(this ContainerBuilder builder, Type type) => builder.AddReqnrollBindings(type.Assembly);

        /// <summary>
        /// Add Reqnroll binding for classes in an assembly.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="assembly">The assembly to search for bindings.</param>
        /// <returns>The builder.</returns>
        public static ContainerBuilder AddReqnrollBindings(this ContainerBuilder builder, Assembly assembly)
        {
            builder
               .RegisterAssemblyTypes(assembly)
               .Where(t => Attribute.IsDefined(t, typeof(BindingAttribute)))
               .SingleInstance();
            return builder;
        }
    }
}

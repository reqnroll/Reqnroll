using System.Collections.Generic;
using System.Reflection;
using Reqnroll.Configuration;

namespace Reqnroll.RuntimeTests.Infrastructure
{
    internal static class RuntimeConfigurationExtensions
    {
        public static void AddAdditionalStepAssembly(this Reqnroll.Configuration.ReqnrollConfiguration reqnrollConfiguration, Assembly assembly)
        {
            reqnrollConfiguration.AdditionalStepAssemblies.Add(assembly.FullName);
        }
    }
}
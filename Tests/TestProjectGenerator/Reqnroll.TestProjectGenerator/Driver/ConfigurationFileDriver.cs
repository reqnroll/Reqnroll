using System;
using System.Globalization;
using Reqnroll.TestProjectGenerator.Data;

namespace Reqnroll.TestProjectGenerator.Driver
{
    public class ConfigurationFileDriver
    {
        private readonly SolutionDriver _solutionDriver;

        public ConfigurationFileDriver(SolutionDriver solutionDriver)
        {
            _solutionDriver = solutionDriver;
        }

        public void AddStepAssembly(BindingAssembly stepAssembly)
        {
            AddStepAssembly(_solutionDriver.DefaultProject, stepAssembly);
        }

        public void SetUnitTestProvider(string unitTestProviderName)
        {
            SetUnitTestProvider(_solutionDriver.DefaultProject, unitTestProviderName);
            _solutionDriver.DefaultProject.Configuration.UnitTestProvider = GetUnitTestProvider(unitTestProviderName);
        }

        public void SetConfigurationFormat(ConfigurationFormat configurationFormat) => SetConfigurationFormat(_solutionDriver.DefaultProject, configurationFormat);

        public void SetIsRowTestsAllowed(bool isAllowed) => SetIsRowTestsAllowed(_solutionDriver.DefaultProject, isAllowed);

        public void AddNonParallelizableMarkerForTag(string tagName) => AddNonParallelizableMarkerForTag(_solutionDriver.DefaultProject, tagName);

        public void AddRuntimeRegisterDependency(string type, string @as) => AddRuntimeRegisterDependency(_solutionDriver.DefaultProject, type, @as);

        private (string type, string @as) GetFullTypeAs(ProjectBuilder project, string type, string @as)
        {
            return ($"{type}, {project.ProjectName}", $"{@as}, Reqnroll");
        }

        public void AddRuntimeRegisterDependency(ProjectBuilder project, string type, string @as)
        {
            (type, @as) = GetFullTypeAs(project, type, @as);
            project.Configuration.Runtime.Value.AddRegisterDependency(type, @as);
        }

        public void SetBindingCulture(ProjectBuilder project, CultureInfo bindingCulture)
        {
            project.Configuration.BindingCulture = bindingCulture;
        }

        public void SetFeatureLanguage(ProjectBuilder project, CultureInfo featureLanguage)
        {
            project.Configuration.FeatureLanguage = featureLanguage;
        }

        public void SetFeatureLanguage(string featureLanguage)
        {
            var featureLanguageCultureInfo = CultureInfo.GetCultureInfo(featureLanguage);
            SetFeatureLanguage(_solutionDriver.DefaultProject, featureLanguageCultureInfo);
        }

        public void SetUnitTestProvider(ProjectBuilder project, string unitTestProviderName)
        {
            project.Configuration.UnitTestProvider = GetUnitTestProvider(unitTestProviderName);
        }

        public void AddStepAssembly(ProjectBuilder project, BindingAssembly stepAssembly)
        {
            project.Configuration.BindingAssemblies.Add(stepAssembly);
        }

        public void SetConfigurationFormat(ProjectBuilder project, ConfigurationFormat configurationFormat)
        {
            project.ConfigurationFormat = configurationFormat;
        }

        public void SetIsRowTestsAllowed(ProjectBuilder project, bool isAllowed)
        {
            project.Configuration.Generator.Value.AllowRowTests = isAllowed;
        }

        public void AddNonParallelizableMarkerForTag(ProjectBuilder project, string tagName)
        {
            project.Configuration.Generator.Value.AddNonParallelizableMarkerForTag(tagName);
        }

        private UnitTestProvider GetUnitTestProvider(string providerName)
        {
            switch (providerName.ToLower())
            {
                case "mstest": return UnitTestProvider.MSTest;
                case "mstest4": return UnitTestProvider.MSTest4;
                case "xunit": return UnitTestProvider.xUnit;
                case "xunit3": return UnitTestProvider.xUnit3;
                case "nunit": return UnitTestProvider.NUnit3;
                case "tunit": return UnitTestProvider.TUnit;
                default: throw new ArgumentOutOfRangeException(nameof(providerName), providerName, "Unknown unit test provider");
            }
        }

        public void SetRuntimeObsoleteBehavior(string obsoleteBehaviorValue)
        {
            _solutionDriver.DefaultProject.Configuration.Runtime.Value.ObsoleteBehavior = obsoleteBehaviorValue;
        }
    }
}

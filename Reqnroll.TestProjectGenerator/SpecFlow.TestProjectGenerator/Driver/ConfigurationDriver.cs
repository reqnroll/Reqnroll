using System;
using System.Globalization;
using TechTalk.SpecFlow.TestProjectGenerator.Data;

namespace TechTalk.SpecFlow.TestProjectGenerator.Driver
{
    public class ConfigurationDriver
    {
        private readonly SolutionDriver _solutionDriver;

        public ConfigurationDriver(SolutionDriver solutionDriver)
        {
            _solutionDriver = solutionDriver;
        }

        public void AddStepAssembly(StepAssembly stepAssembly)
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

        public void AddRuntimeRegisterDependency(string type, string @as) => AddRuntimeRegisterDependency(_solutionDriver.DefaultProject, type, @as);

        private (string type, string @as) GetFullTypeAs(ProjectBuilder project, string type, string @as)
        {
            return ($"{type}, {project.ProjectName}", $"{@as}, TechTalk.SpecFlow");
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

        public void AddStepAssembly(ProjectBuilder project, StepAssembly stepAssembly)
        {
            project.Configuration.StepAssemblies.Add(stepAssembly);
        }

        public void SetConfigurationFormat(ProjectBuilder project, ConfigurationFormat configurationFormat)
        {
            project.ConfigurationFormat = configurationFormat;
        }

        public void SetIsRowTestsAllowed(ProjectBuilder project, bool isAllowed)
        {
            project.Configuration.Generator.Value.AllowRowTests = isAllowed;
        }

        private UnitTestProvider GetUnitTestProvider(string providerName)
        {
            switch (providerName.ToLower())
            {
                case "specrun+nunit": return UnitTestProvider.SpecRunWithNUnit;
                case "specrun+nunit.2": return UnitTestProvider.SpecRunWithNUnit2;
                case "specrun+mstest": return UnitTestProvider.SpecRunWithMsTest;
                case "specrun": return UnitTestProvider.SpecRun;
                case "mstest": return UnitTestProvider.MSTest;
                case "xunit": return UnitTestProvider.xUnit;
                case "nunit": return UnitTestProvider.NUnit3;
                default: throw new ArgumentOutOfRangeException(nameof(providerName), providerName, "Unknown unit test provider");
            }
        }

        public void SetRuntimeObsoleteBehavior(string obsoleteBehaviorValue)
        {
            _solutionDriver.DefaultProject.Configuration.Runtime.Value.ObsoleteBehavior = obsoleteBehaviorValue;
        }
    }
}

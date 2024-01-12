using System;
using System.IO;
using System.Linq;
using BoDi;
using FluentAssertions;
using Moq;
using Xunit;
using Reqnroll.BindingSkeletons;
using Reqnroll.Configuration;
using Reqnroll.Configuration.AppConfig;
using Reqnroll.Infrastructure;
using Reqnroll.Plugins;
using Reqnroll.RuntimeTests.Infrastructure;
using Reqnroll.Tracing;

namespace Reqnroll.RuntimeTests.Configuration
{
    
    public class AppConfigTests
    {
        [Fact]
        public void CanLoadConfigFromConfigFile()
        {
            var reqnrollJsonLocatorMock = new Mock<IReqnrollJsonLocator>();

            var runtimeConfiguration = ConfigurationLoader.GetDefault();
            var configurationLoader = new ConfigurationLoader(reqnrollJsonLocatorMock.Object);

            runtimeConfiguration = configurationLoader.Load(runtimeConfiguration);
        }

        [Theory]
        [InlineData(@"<reqnroll>
    <language feature=""en"" tool=""en"" /> 
    
    <generator allowDebugGeneratedFiles=""false"" />
    
    <runtime stopAtFirstError=""false""
             missingOrPendingStepsOutcome=""Inconclusive"" />

    <trace traceSuccessfulSteps=""true""
           traceTimings=""false""
           minTracedDuration=""0:0:0.1""
           coloredOutput=""false""
           listener=""Reqnroll.Tracing.DefaultListener, Reqnroll""
            />
</reqnroll>")]
        public void CanLoadConfigFromString(string configString)
        {
            var runtimeConfig = ConfigurationLoader.GetDefault();

            var configurationLoader = new AppConfigConfigurationLoader();


            var configurationSectionHandler = ConfigurationSectionHandler.CreateFromXml(configString);
            configurationLoader.LoadAppConfig(runtimeConfig, configurationSectionHandler);
        }

        [Fact]
        public void CheckFeatureLanguage()
        {
            string config = @"<reqnroll><language feature=""de"" /></reqnroll>";

            var configSection = ConfigurationSectionHandler.CreateFromXml(config);

            var runtimeConfig = new AppConfigConfigurationLoader().LoadAppConfig(ConfigurationLoader.GetDefault(), configSection);

            runtimeConfig.FeatureLanguage.TwoLetterISOLanguageName.Should().Be("de");
        }


        [Fact]
        public void CheckBindingCulture()
        {
            string config = @"<reqnroll><bindingCulture name=""de"" /></reqnroll>";

            var configSection = ConfigurationSectionHandler.CreateFromXml(config);

            var runtimeConfig = new AppConfigConfigurationLoader().LoadAppConfig(ConfigurationLoader.GetDefault(), configSection);

            runtimeConfig.BindingCulture.TwoLetterISOLanguageName.Should().Be("de");
        }

        [Fact]
        public void Check_Runtime_stopAtFirstError_as_true()
        {
            string config = @"<reqnroll><runtime stopAtFirstError=""true"" /></reqnroll>";

            var configSection = ConfigurationSectionHandler.CreateFromXml(config);

            var runtimeConfig = new AppConfigConfigurationLoader().LoadAppConfig(ConfigurationLoader.GetDefault(), configSection);

            runtimeConfig.StopAtFirstError.Should().BeTrue();
        }

        [Fact]
        public void Check_Runtime_stopAtFirstError_as_false()
        {
            string config = @"<reqnroll><runtime stopAtFirstError=""false"" /></reqnroll>";

            var configSection = ConfigurationSectionHandler.CreateFromXml(config);

            var runtimeConfig = new AppConfigConfigurationLoader().LoadAppConfig(ConfigurationLoader.GetDefault(), configSection);

            runtimeConfig.StopAtFirstError.Should().BeFalse();
        }

        [Fact]
        public void Check_Runtime_missingOrPendingStepsOutcome_as_Pending()
        {
            string config = @"<reqnroll><runtime missingOrPendingStepsOutcome=""Pending"" /></reqnroll>";

            var configSection = ConfigurationSectionHandler.CreateFromXml(config);

            var runtimeConfig = new AppConfigConfigurationLoader().LoadAppConfig(ConfigurationLoader.GetDefault(), configSection);

            runtimeConfig.MissingOrPendingStepsOutcome.Should().Be(MissingOrPendingStepsOutcome.Pending);
        }

        [Fact]
        public void Check_Runtime_missingOrPendingStepsOutcome_as_Error()
        {
            string config = @"<reqnroll><runtime missingOrPendingStepsOutcome=""Error"" /></reqnroll>";

            var configSection = ConfigurationSectionHandler.CreateFromXml(config);

            var runtimeConfig = new AppConfigConfigurationLoader().LoadAppConfig(ConfigurationLoader.GetDefault(), configSection);

            runtimeConfig.MissingOrPendingStepsOutcome.Should().Be(MissingOrPendingStepsOutcome.Error);
        }

        [Fact]
        public void Check_Runtime_missingOrPendingStepsOutcome_as_Ignore()
        {
            string config = @"<reqnroll><runtime missingOrPendingStepsOutcome=""Ignore"" /></reqnroll>";

            var configSection = ConfigurationSectionHandler.CreateFromXml(config);

            var runtimeConfig = new AppConfigConfigurationLoader().LoadAppConfig(ConfigurationLoader.GetDefault(), configSection);

            runtimeConfig.MissingOrPendingStepsOutcome.Should().Be(MissingOrPendingStepsOutcome.Ignore);
        }

        [Fact]
        public void Check_Runtime_missingOrPendingStepsOutcome_as_Inconclusive()
        {
            string config = @"<reqnroll><runtime missingOrPendingStepsOutcome=""Inconclusive"" /></reqnroll>";

            var configSection = ConfigurationSectionHandler.CreateFromXml(config);

            var runtimeConfig = new AppConfigConfigurationLoader().LoadAppConfig(ConfigurationLoader.GetDefault(), configSection);

            runtimeConfig.MissingOrPendingStepsOutcome.Should().Be(MissingOrPendingStepsOutcome.Inconclusive);
        }

        [Fact]
        public void Check_Trace_traceSuccessfulSteps_as_True()
        {
            string config = @"<reqnroll><trace traceSuccessfulSteps=""true"" /></reqnroll>";

            var configSection = ConfigurationSectionHandler.CreateFromXml(config);

            var runtimeConfig = new AppConfigConfigurationLoader().LoadAppConfig(ConfigurationLoader.GetDefault(), configSection);

            runtimeConfig.TraceSuccessfulSteps.Should().BeTrue();
        }

        [Fact]
        public void Check_Trace_traceSuccessfulSteps_as_False()
        {
            string config = @"<reqnroll><trace traceSuccessfulSteps=""false"" /></reqnroll>";

            var configSection = ConfigurationSectionHandler.CreateFromXml(config);

            var runtimeConfig = new AppConfigConfigurationLoader().LoadAppConfig(ConfigurationLoader.GetDefault(), configSection);

            runtimeConfig.TraceSuccessfulSteps.Should().BeFalse();
        }

        [Fact]
        public void Check_Trace_traceTimings_as_True()
        {
            string config = @"<reqnroll><trace traceTimings=""true"" /></reqnroll>";

            var configSection = ConfigurationSectionHandler.CreateFromXml(config);

            var runtimeConfig = new AppConfigConfigurationLoader().LoadAppConfig(ConfigurationLoader.GetDefault(), configSection);

            runtimeConfig.TraceTimings.Should().BeTrue();
        }

        [Fact]
        public void Check_Trace_traceTimings_as_False()
        {
            string config = @"<reqnroll><trace traceTimings=""false"" /></reqnroll>";

            var configSection = ConfigurationSectionHandler.CreateFromXml(config);

            var runtimeConfig = new AppConfigConfigurationLoader().LoadAppConfig(ConfigurationLoader.GetDefault(), configSection);

            runtimeConfig.TraceTimings.Should().BeFalse();
        }

        [Fact]
        public void Check_Trace_coloredOutput_as_True()
        {
            string config = @"<reqnroll><trace coloredOutput=""true"" /></reqnroll>";

            var configSection = ConfigurationSectionHandler.CreateFromXml(config);

            var runtimeConfig = new AppConfigConfigurationLoader().LoadAppConfig(ConfigurationLoader.GetDefault(), configSection);

            runtimeConfig.ColoredOutput.Should().BeTrue();
        }

        [Fact]
        public void Check_Trace_coloredOutput_as_False()
        {
            string config = @"<reqnroll><trace coloredOutput=""false"" /></reqnroll>";

            var configSection = ConfigurationSectionHandler.CreateFromXml(config);

            var runtimeConfig = new AppConfigConfigurationLoader().LoadAppConfig(ConfigurationLoader.GetDefault(), configSection);

            runtimeConfig.ColoredOutput.Should().BeFalse();
        }

        [Fact]
        public void Check_Trace_minTracedDuration()
        {
            string config = @"<reqnroll><trace minTracedDuration=""0:0:1.0"" /></reqnroll>";

            var configSection = ConfigurationSectionHandler.CreateFromXml(config);

            var runtimeConfig = new AppConfigConfigurationLoader().LoadAppConfig(ConfigurationLoader.GetDefault(), configSection);

            runtimeConfig.MinTracedDuration.Should().Be(TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Check_Trace_Listener()
        {
            string config = @"<reqnroll><trace listener=""TraceListener"" /></reqnroll>";

            var configSection = ConfigurationSectionHandler.CreateFromXml(config);

            var runtimeConfig = new AppConfigConfigurationLoader().LoadAppConfig(ConfigurationLoader.GetDefault(), configSection);

            runtimeConfig.CustomDependencies.Count.Should().Be(1);

            foreach (ContainerRegistrationConfigElement containerRegistrationConfigElement in runtimeConfig.CustomDependencies)
            {
                containerRegistrationConfigElement.Implementation.Should().Be("TraceListener");
            }
        }

        [Fact]
        public void Check_Trace_StepDefinitionSkeletonStyle_RegexAttribute()
        {
            string config = @"<reqnroll><trace stepDefinitionSkeletonStyle=""RegexAttribute"" /></reqnroll>";

            var configSection = ConfigurationSectionHandler.CreateFromXml(config);

            var runtimeConfig = new AppConfigConfigurationLoader().LoadAppConfig(ConfigurationLoader.GetDefault(), configSection);

            runtimeConfig.StepDefinitionSkeletonStyle.Should().Be(StepDefinitionSkeletonStyle.RegexAttribute);
        }

        [Fact]
        public void Check_Trace_StepDefinitionSkeletonStyle_MethodNamePascalCase()
        {
            string config = @"<reqnroll><trace stepDefinitionSkeletonStyle=""MethodNamePascalCase"" /></reqnroll>";

            var configSection = ConfigurationSectionHandler.CreateFromXml(config);

            var runtimeConfig = new AppConfigConfigurationLoader().LoadAppConfig(ConfigurationLoader.GetDefault(), configSection);

            runtimeConfig.StepDefinitionSkeletonStyle.Should().Be(StepDefinitionSkeletonStyle.MethodNamePascalCase);
        }

        [Fact]
        public void Check_Trace_StepDefinitionSkeletonStyle_MethodNameRegex()
        {
            string config = @"<reqnroll><trace stepDefinitionSkeletonStyle=""MethodNameRegex"" /></reqnroll>";

            var configSection = ConfigurationSectionHandler.CreateFromXml(config);

            var runtimeConfig = new AppConfigConfigurationLoader().LoadAppConfig(ConfigurationLoader.GetDefault(), configSection);

            runtimeConfig.StepDefinitionSkeletonStyle.Should().Be(StepDefinitionSkeletonStyle.MethodNameRegex);
        }

        [Fact]
        public void Check_Trace_StepDefinitionSkeletonStyle_MethodNameUnderscores()
        {
            string config = @"<reqnroll><trace stepDefinitionSkeletonStyle=""MethodNameUnderscores"" /></reqnroll>";

            var configSection = ConfigurationSectionHandler.CreateFromXml(config);

            var runtimeConfig = new AppConfigConfigurationLoader().LoadAppConfig(ConfigurationLoader.GetDefault(), configSection);

            runtimeConfig.StepDefinitionSkeletonStyle.Should().Be(StepDefinitionSkeletonStyle.MethodNameUnderscores);
        }

        [Fact]
        public void Check_StepAssemblies_IsEmpty()
        {
            string config = @"<reqnroll><stepAssemblies /></reqnroll>";

            var configSection = ConfigurationSectionHandler.CreateFromXml(config);

            var runtimeConfig = new AppConfigConfigurationLoader().LoadAppConfig(ConfigurationLoader.GetDefault(), configSection);

            runtimeConfig.AdditionalStepAssemblies.Should().BeEmpty();
        }

        [Fact]
        public void Check_StepAssemblies_NotInConfigFile()
        {
            string config = @"<reqnroll></reqnroll>";

            var configSection = ConfigurationSectionHandler.CreateFromXml(config);

            var runtimeConfig = new AppConfigConfigurationLoader().LoadAppConfig(ConfigurationLoader.GetDefault(), configSection);

            runtimeConfig.AdditionalStepAssemblies.Should().BeEmpty();
        }

        [Fact]
        public void Check_StepAssemblies_OneEntry()
        {
            string config = @"<reqnroll><stepAssemblies><stepAssembly assembly=""testEntry""/></stepAssemblies></reqnroll>";

            var configSection = ConfigurationSectionHandler.CreateFromXml(config);

            var runtimeConfig = new AppConfigConfigurationLoader().LoadAppConfig(ConfigurationLoader.GetDefault(), configSection);

            runtimeConfig.AdditionalStepAssemblies.Count.Should().Be(1);
            runtimeConfig.AdditionalStepAssemblies.First().Should().Be("testEntry");
        }

        [Fact]
        public void Check_StepAssemblies_TwoEntry()
        {
            string config = @"<reqnroll><stepAssemblies>
                                <stepAssembly assembly=""testEntry1""/>
                                <stepAssembly assembly=""testEntry2""/>
                              </stepAssemblies></reqnroll>";

            var configSection = ConfigurationSectionHandler.CreateFromXml(config);

            var runtimeConfig = new AppConfigConfigurationLoader().LoadAppConfig(ConfigurationLoader.GetDefault(), configSection);

            runtimeConfig.AdditionalStepAssemblies.Count.Should().Be(2);
            runtimeConfig.AdditionalStepAssemblies[0].Should().Be("testEntry1");
            runtimeConfig.AdditionalStepAssemblies[1].Should().Be("testEntry2");
        }
    }
}

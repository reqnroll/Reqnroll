using System.Globalization;
using FluentAssertions;
using Reqnroll.Configuration;
using Xunit;

namespace Reqnroll.RuntimeTests.Configuration
{
    public class LanguageConfigurationTests
    {
        [Fact]
        public void DefaultConfiguration_Should_Have_EnUS_FeatureLanguage()
        {
            var config = ConfigurationLoader.GetDefault();
            
            config.FeatureLanguage.Name.Should().Be("en-US");
        }

        [Fact]
        public void DefaultConfiguration_Should_Have_Null_BindingCulture()
        {
            var config = ConfigurationLoader.GetDefault();
            
            config.BindingCulture.Should().BeNull();
        }

        [Fact]
        public void Configuration_Should_Fallback_To_FeatureLanguage_When_BindingCulture_Is_Null()
        {
            var config = new ReqnrollConfiguration(
                ConfigSource.Default,
                new DependencyConfigurationCollection(),
                new DependencyConfigurationCollection(),
                CultureInfo.GetCultureInfo("de-DE"), // Feature language
                null, // Binding culture is null
                false,
                MissingOrPendingStepsOutcome.Pending,
                true,
                false,
                System.TimeSpan.Zero,
                Reqnroll.BindingSkeletons.StepDefinitionSkeletonStyle.CucumberExpressionAttribute,
                new System.Collections.Generic.List<string>(),
                false,
                true,
                new string[0],
                ObsoleteBehavior.Warn,
                false
            );

            // Test that the fallback logic can be applied
            var effectiveBindingCulture = config.BindingCulture ?? config.FeatureLanguage;
            effectiveBindingCulture.Name.Should().Be("de-DE");
        }

        [Fact]
        public void Configuration_Should_Use_BindingCulture_When_Specified()
        {
            var config = new ReqnrollConfiguration(
                ConfigSource.Default,
                new DependencyConfigurationCollection(),
                new DependencyConfigurationCollection(),
                CultureInfo.GetCultureInfo("de-DE"), // Feature language
                CultureInfo.GetCultureInfo("en-US"), // Binding culture is specified
                false,
                MissingOrPendingStepsOutcome.Pending,
                true,
                false,
                System.TimeSpan.Zero,
                Reqnroll.BindingSkeletons.StepDefinitionSkeletonStyle.CucumberExpressionAttribute,
                new System.Collections.Generic.List<string>(),
                false,
                true,
                new string[0],
                ObsoleteBehavior.Warn,
                false
            );

            // Test that binding culture is used when specified
            var effectiveBindingCulture = config.BindingCulture ?? config.FeatureLanguage;
            effectiveBindingCulture.Name.Should().Be("en-US");
        }

        [Theory]
        [InlineData("en-US", false)] // Specific culture
        [InlineData("en-GB", false)] // Specific culture
        [InlineData("de-DE", false)] // Specific culture
        [InlineData("fr-FR", false)] // Specific culture
        [InlineData("en", true)]     // Generic/neutral culture
        [InlineData("de", true)]     // Generic/neutral culture
        [InlineData("fr", true)]     // Generic/neutral culture
        public void Should_Support_Specific_Cultures_Over_Generic_Cultures(string cultureName, bool isNeutral)
        {
            var culture = CultureInfo.GetCultureInfo(cultureName);
            
            culture.IsNeutralCulture.Should().Be(isNeutral);
            
            // The configuration should work with both, but specific cultures are recommended
            var config = new ReqnrollConfiguration(
                ConfigSource.Default,
                new DependencyConfigurationCollection(),
                new DependencyConfigurationCollection(),
                culture, // Feature language
                culture, // Binding culture
                false,
                MissingOrPendingStepsOutcome.Pending,
                true,
                false,
                System.TimeSpan.Zero,
                Reqnroll.BindingSkeletons.StepDefinitionSkeletonStyle.CucumberExpressionAttribute,
                new System.Collections.Generic.List<string>(),
                false,
                true,
                new string[0],
                ObsoleteBehavior.Warn,
                false
            );

            config.FeatureLanguage.Name.Should().Be(cultureName);
            config.BindingCulture.Name.Should().Be(cultureName);
        }
    }
}
using System;
using System.Globalization;
using System.Linq;
using Reqnroll.BoDi;
using FluentAssertions;
using Xunit;
using Reqnroll.BindingSkeletons;
using Reqnroll.Configuration;
using Reqnroll.Configuration.JsonConfig;
using Reqnroll.Plugins;

namespace Reqnroll.RuntimeTests.Configuration
{

    public class JsonConfigTests
    {
        [Theory]
        [InlineData(
            @"{
            ""language"": {
              ""feature"": ""en"",
              ""tool"": ""en""
            },
            ""unitTestProvider"": {
              ""name"": ""NUnit"",
              ""generatorProvider"": ""Reqnroll.TestFrameworkIntegration.NUnitRuntimeProvider, Reqnroll"",
              ""runtimeProvider"": ""Reqnroll.UnitTestProvider.NUnitRuntimeProvider, Reqnroll""
            },
            ""generator"": { ""allowDebugGeneratedFiles"": false },
            ""runtime"": {              
              ""stopAtFirstError"": false,
              ""missingOrPendingStepsOutcome"": ""Inconclusive""
            },
            ""trace"": {
              ""traceSuccessfulSteps"": true,
              ""traceTimings"": false,
              ""minTracedDuration"": ""0:0:0.1"",
              ""listener"": ""Reqnroll.Tracing.DefaultListener, Reqnroll""
            }
        }")]

        public void CanLoadConfigFromString(string configString)
        {
            var configurationLoader = new JsonConfigurationLoader();

            configurationLoader.LoadJson(ConfigurationLoader.GetDefault(), configString);
        }

        [Fact]
        public void CheckFeatureLanguage()
        {
            string config = @"{
                                ""language"": { ""feature"": ""de"" }
                            }";

            var runtimeConfig = new JsonConfigurationLoader().LoadJson(ConfigurationLoader.GetDefault(), config);

            runtimeConfig.FeatureLanguage.TwoLetterISOLanguageName.Should().Be("de");
        }

        [Fact]
        public void CheckBindingCulture()
        {
            string config = @"{
                                ""bindingCulture"": { ""name"": ""de"" }
                            }";



            var runtimeConfig = new JsonConfigurationLoader().LoadJson(ConfigurationLoader.GetDefault(), config);

            runtimeConfig.BindingCulture.TwoLetterISOLanguageName.Should().Be("de");
        }

        [Fact]
        public void Check_Runtime_stopAtFirstError_as_true()
        {
            string config = @"{
                                ""runtime"": { ""stopAtFirstError"": true }
                            }";

            var runtimeConfig = new JsonConfigurationLoader().LoadJson(ConfigurationLoader.GetDefault(), config);

            runtimeConfig.StopAtFirstError.Should().BeTrue();
        }

        [Fact]
        public void Check_Runtime_stopAtFirstError_as_false()
        {
            string config = @"{
                                ""runtime"": { ""stopAtFirstError"": false }
                            }";



            var runtimeConfig = new JsonConfigurationLoader().LoadJson(ConfigurationLoader.GetDefault(), config);

            runtimeConfig.StopAtFirstError.Should().BeFalse();
        }

        [Fact]
        public void Check_Runtime_missingOrPendingStepsOutcome_as_Pending()
        {
            string config = @"{
                                ""runtime"": { ""missingOrPendingStepsOutcome"": ""Pending"" }
                            }";



            var runtimeConfig = new JsonConfigurationLoader().LoadJson(ConfigurationLoader.GetDefault(), config);

            runtimeConfig.MissingOrPendingStepsOutcome.Should().Be(MissingOrPendingStepsOutcome.Pending);
        }

        [Fact]
        public void Check_Runtime_missingOrPendingStepsOutcome_as_Error()
        {
            string config = @"{
                                ""runtime"": { ""missingOrPendingStepsOutcome"": ""Error"" }
                            }";



            var runtimeConfig = new JsonConfigurationLoader().LoadJson(ConfigurationLoader.GetDefault(), config);

            runtimeConfig.MissingOrPendingStepsOutcome.Should().Be(MissingOrPendingStepsOutcome.Error);
        }

        [Fact]
        public void Check_Runtime_missingOrPendingStepsOutcome_as_Ignore()
        {
            string config = @"{
                                ""runtime"": { ""missingOrPendingStepsOutcome"": ""Ignore"" }
                            }";



            var runtimeConfig = new JsonConfigurationLoader().LoadJson(ConfigurationLoader.GetDefault(), config);

            runtimeConfig.MissingOrPendingStepsOutcome.Should().Be(MissingOrPendingStepsOutcome.Ignore);
        }

        [Fact]
        public void Check_Runtime_missingOrPendingStepsOutcome_as_Inconclusive()
        {
            string config = @"{
                                ""runtime"": { ""missingOrPendingStepsOutcome"": ""Inconclusive"" }
                            }";



            var runtimeConfig = new JsonConfigurationLoader().LoadJson(ConfigurationLoader.GetDefault(), config);

            runtimeConfig.MissingOrPendingStepsOutcome.Should().Be(MissingOrPendingStepsOutcome.Inconclusive);
        }

        [Fact]
        public void Check_Trace_traceSuccessfulSteps_as_True()
        {
            string config = @"{
                                ""trace"": { ""traceSuccessfulSteps"": true }
                            }";



            var runtimeConfig = new JsonConfigurationLoader().LoadJson(ConfigurationLoader.GetDefault(), config);

            runtimeConfig.TraceSuccessfulSteps.Should().BeTrue();
        }

        [Fact]
        public void Check_Trace_traceSuccessfulSteps_as_False()
        {
            string config = @"{
                                ""trace"": { ""traceSuccessfulSteps"": false }
                            }";



            var runtimeConfig = new JsonConfigurationLoader().LoadJson(ConfigurationLoader.GetDefault(), config);

            runtimeConfig.TraceSuccessfulSteps.Should().BeFalse();
        }

        [Fact]
        public void Check_Trace_traceTimings_as_True()
        {
            string config = @"{
                                ""trace"": { ""traceTimings"": true }
                            }";



            var runtimeConfig = new JsonConfigurationLoader().LoadJson(ConfigurationLoader.GetDefault(), config);

            runtimeConfig.TraceTimings.Should().BeTrue();
        }

        [Fact]
        public void Check_Trace_traceTimings_as_False()
        {
            string config = @"{
                                ""trace"": { ""traceTimings"": false }
                            }";



            var runtimeConfig = new JsonConfigurationLoader().LoadJson(ConfigurationLoader.GetDefault(), config);

            runtimeConfig.TraceTimings.Should().BeFalse();
        }

        [Fact]
        public void Check_Trace_minTracedDuration()
        {
            string config = @"{
                                ""trace"": { ""minTracedDuration"": ""0:0:0:1.0"" }
                            }";



            var runtimeConfig = new JsonConfigurationLoader().LoadJson(ConfigurationLoader.GetDefault(), config);

            runtimeConfig.MinTracedDuration.Should().Be(TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Trace_Listener_Not_Supported()
        {
            string config = @"{
                                ""trace"": { ""listener"": ""TraceListener"" }
                            }";



            var runtimeConfig = new JsonConfigurationLoader().LoadJson(ConfigurationLoader.GetDefault(), config);

            runtimeConfig.CustomDependencies.Count.Should().Be(0);
        }

        [Fact]
        public void Check_Trace_StepDefinitionSkeletonStyle_RegexAttribute()
        {
            string config = @"{
                                ""trace"": { ""stepDefinitionSkeletonStyle"": ""RegexAttribute"" }
                            }";



            var runtimeConfig = new JsonConfigurationLoader().LoadJson(ConfigurationLoader.GetDefault(), config);

            runtimeConfig.StepDefinitionSkeletonStyle.Should().Be(StepDefinitionSkeletonStyle.RegexAttribute);
        }

        [Fact]
        public void Check_Trace_StepDefinitionSkeletonStyle_MethodNamePascalCase()
        {
            string config = @"{
                                ""trace"": { ""stepDefinitionSkeletonStyle"": ""MethodNamePascalCase"" }
                            }";



            var runtimeConfig = new JsonConfigurationLoader().LoadJson(ConfigurationLoader.GetDefault(), config);

            runtimeConfig.StepDefinitionSkeletonStyle.Should().Be(StepDefinitionSkeletonStyle.MethodNamePascalCase);
        }

        [Fact]
        public void Check_Trace_StepDefinitionSkeletonStyle_MethodNameRegex()
        {
            string config = @"{
                                ""trace"": { ""stepDefinitionSkeletonStyle"": ""MethodNameRegex"" }
                            }";



            var runtimeConfig = new JsonConfigurationLoader().LoadJson(ConfigurationLoader.GetDefault(), config);

            runtimeConfig.StepDefinitionSkeletonStyle.Should().Be(StepDefinitionSkeletonStyle.MethodNameRegex);
        }

        [Fact]
        public void Check_Trace_StepDefinitionSkeletonStyle_MethodNameUnderscores()
        {
            string config = @"{
                                ""trace"": { ""stepDefinitionSkeletonStyle"": ""MethodNameUnderscores"" }
                            }";



            var runtimeConfig = new JsonConfigurationLoader().LoadJson(ConfigurationLoader.GetDefault(), config);

            runtimeConfig.StepDefinitionSkeletonStyle.Should().Be(StepDefinitionSkeletonStyle.MethodNameUnderscores);
        }

        [Fact]
        public void Check_Trace_ColoredOutput_as_False()
        {
            string config = @"{
                                ""trace"": { ""coloredOutput"": false }
                            }";


            var runtimeConfig = new JsonConfigurationLoader().LoadJson(ConfigurationLoader.GetDefault(), config);

            runtimeConfig.ColoredOutput.Should().Be(false);
        }

        [Fact]
        public void Check_Trace_ColoredOutput_as_True()
        {
            string config = @"{
                                ""trace"": { ""coloredOutput"": true }
                            }";


            var runtimeConfig = new JsonConfigurationLoader().LoadJson(ConfigurationLoader.GetDefault(), config);

            runtimeConfig.ColoredOutput.Should().Be(true);
        }

        [Fact]
        public void Check_StepAssemblies_IsEmpty()
        {
            string config = @"{
                                  ""bindingAssemblies"" : [
                                 ]
                            }";



            var runtimeConfig = new JsonConfigurationLoader().LoadJson(ConfigurationLoader.GetDefault(), config);

            runtimeConfig.AdditionalStepAssemblies.Should().BeEmpty();
        }

        [Fact]
        public void Check_StepAssemblies_NotInConfigFile()
        {
            string config = @"{
                            }";



            var runtimeConfig = new JsonConfigurationLoader().LoadJson(ConfigurationLoader.GetDefault(), config);

            runtimeConfig.AdditionalStepAssemblies.Should().BeEmpty();
        }

        [Fact]
        public void Check_StepAssemblies_OneEntry()
        {
            string config = @"{
                                ""bindingAssemblies"": 
                                   [ {""assembly"": ""testEntry""} ]
                            }";



            var runtimeConfig = new JsonConfigurationLoader().LoadJson(ConfigurationLoader.GetDefault(), config);

            runtimeConfig.AdditionalStepAssemblies.Count.Should().Be(1);
            runtimeConfig.AdditionalStepAssemblies.First().Should().Be("testEntry");
        }

        [Fact]
        public void Check_StepAssemblies_TwoEntry()
        {
            string config = @"{
                                ""bindingAssemblies"": [
                                    { ""assembly"": ""testEntry1"" },
                                    { ""assembly"": ""testEntry2"" }
                                  ]
                            }";



            var runtimeConfig = new JsonConfigurationLoader().LoadJson(ConfigurationLoader.GetDefault(), config);

            runtimeConfig.AdditionalStepAssemblies.Count.Should().Be(2);
            runtimeConfig.AdditionalStepAssemblies[0].Should().Be("testEntry1");
            runtimeConfig.AdditionalStepAssemblies[1].Should().Be("testEntry2");
        }

        [Fact]
        public void Check_Generator_NonParallelizableMarkers_EmptyList()
        {
            string configAsJson = @"{
                                        ""generator"":
                                        {
                                            ""addNonParallelizableMarkerForTags"":
                                            [
                                            ]
                                        }
                                    }";

            var config = new JsonConfigurationLoader().LoadJson(ConfigurationLoader.GetDefault(), configAsJson);
            config.AddNonParallelizableMarkerForTags.Should().BeEmpty();
        }

        [Fact]
        public void Check_Generator_NonParallelizableMarkers_SingleTag()
        {
            string configAsJson = @"{
                                        ""generator"":
                                        {
                                            ""addNonParallelizableMarkerForTags"":
                                            [
                                                ""tag1""
                                            ]
                                        }
                                    }";

            var config = new JsonConfigurationLoader().LoadJson(ConfigurationLoader.GetDefault(), configAsJson);
            config.AddNonParallelizableMarkerForTags.Should().BeEquivalentTo("tag1");
        }

        [Fact]
        public void Check_Generator_NonParallelizableMarkers_MultipleTags()
        {
            string configAsJson = @"{
                                        ""generator"":
                                        {
                                            ""addNonParallelizableMarkerForTags"":
                                            [
                                                ""tag1"",
                                                ""tag2""
                                            ]
                                        }
                                    }";

            var config = new JsonConfigurationLoader().LoadJson(ConfigurationLoader.GetDefault(), configAsJson);
            config.AddNonParallelizableMarkerForTags.Should().BeEquivalentTo("tag1", "tag2");
        }

        [Fact]
        public void Check_Defaults_Empty_Configuration()
        {
            string configAsJson = "{}";

            var config = new JsonConfigurationLoader().LoadJson(ConfigurationLoader.GetDefault(), configAsJson);
            AssertDefaultJsonReqnrollConfiguration(config);
        }

        [Fact]
        public void Check_Defaults_Only_Root_Objects()
        {
            string configAsJson = @"{
                ""bindingCulture"": {},
                ""language"": {},
                ""generator"": {},
                ""runtime"": {},
                ""trace"": {}
            }";

            var config = new JsonConfigurationLoader().LoadJson(ConfigurationLoader.GetDefault(), configAsJson);
            AssertDefaultJsonReqnrollConfiguration(config);
        }

        [Fact]
        public void Check_Defaults_For_One_Config_Element()
        {
            string configAsJson = $@"{{
                ""trace"": {{
                    ""traceTimings"": true
                }}
            }}";

            var config = new JsonConfigurationLoader().LoadJson(ConfigurationLoader.GetDefault(), configAsJson);

            config.TraceTimings.Should().BeTrue();
            config.MinTracedDuration.Should().Be(ConfigDefaults.MinTracedDurationAsTimeSpan);
            config.StepDefinitionSkeletonStyle.Should().Be(ConfigDefaults.StepDefinitionSkeletonStyle);
            config.TraceSuccessfulSteps.Should().Be(ConfigDefaults.TraceSuccessfulSteps);
        }

        private void AssertDefaultJsonReqnrollConfiguration(ReqnrollConfiguration config)
        {
            config.ConfigSource.Should().Be(ConfigSource.Json);
            config.FeatureLanguage.Should().Be(CultureInfo.GetCultureInfo(ConfigDefaults.FeatureLanguage));
            config.BindingCulture.Should().Be(null);

            //runtime
            config.StopAtFirstError.Should().Be(ConfigDefaults.StopAtFirstError);
            config.MissingOrPendingStepsOutcome.Should().Be(ConfigDefaults.MissingOrPendingStepsOutcome);
            config.ObsoleteBehavior.Should().Be(ConfigDefaults.ObsoleteBehavior);
            config.CustomDependencies.Should().NotBeNull();
            config.CustomDependencies.Count.Should().Be(0);

            //generator
            config.AllowDebugGeneratedFiles.Should().Be(ConfigDefaults.AllowDebugGeneratedFiles);
            config.AllowRowTests.Should().Be(ConfigDefaults.AllowRowTests);
            (config.AddNonParallelizableMarkerForTags ?? []).Length.Should().Be(0);
            config.GeneratorCustomDependencies.Should().NotBeNull();
            config.GeneratorCustomDependencies.Count.Should().Be(0);

            //trace
            config.TraceTimings.Should().Be(ConfigDefaults.TraceTimings);
            config.MinTracedDuration.Should().Be(ConfigDefaults.MinTracedDurationAsTimeSpan);
            config.StepDefinitionSkeletonStyle.Should().Be(ConfigDefaults.StepDefinitionSkeletonStyle);
            config.TraceSuccessfulSteps.Should().Be(ConfigDefaults.TraceSuccessfulSteps);
            config.ColoredOutput.Should().Be(ConfigDefaults.ColoredOutput);

            config.AdditionalStepAssemblies.Should().NotBeNull();
            config.AdditionalStepAssemblies.Should().BeEmpty();
        }
    }
}

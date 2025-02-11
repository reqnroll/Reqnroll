using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using Reqnroll.TestProjectGenerator.ConfigurationModel;
using Reqnroll.TestProjectGenerator.ConfigurationModel.Dependencies;
using Reqnroll.TestProjectGenerator.Data;
using Reqnroll.TestProjectGenerator.Helpers;

namespace Reqnroll.TestProjectGenerator.Factories.ConfigurationGenerator
{
    public class JsonConfigGenerator : IConfigurationGenerator
    {
        private readonly CurrentVersionDriver _currentVersionDriver;

        public JsonConfigGenerator(CurrentVersionDriver currentVersionDriver)
        {
            _currentVersionDriver = currentVersionDriver;
        }

        public ProjectFile Generate(Configuration configuration)
        {
            using (var stringWriter = new StringWriter())
            {
                using (var jsonWriter = new JsonTextWriter(stringWriter))
                {
                    // open root object
                    jsonWriter.WriteStartObject();

                    WriteReqnroll(jsonWriter, configuration);

                    // close root object
                    jsonWriter.WriteEndObject();
                    jsonWriter.Flush();

                    return new ProjectFile("reqnroll.json", "None", stringWriter.ToString(), CopyToOutputDirectory.CopyIfNewer);
                }
            }
        }

        private void WriteReqnroll(JsonWriter jsonWriter, Configuration configuration)
        {
            configuration.FeatureLanguage = configuration.FeatureLanguage ?? CultureInfo.GetCultureInfo("en-US");
            
            if (configuration.FeatureLanguage != null)
            {
                WriteLanguage(jsonWriter, configuration.FeatureLanguage);
            }

            if (configuration.BindingCulture != null)
            {
                WriteBindingCulture(jsonWriter, configuration.BindingCulture);
            }

            if (configuration.Generator.IsValueCreated)
            {
                WriteGenerator(jsonWriter, configuration.Generator.Value);
            }

            if (configuration.Runtime.IsValueCreated)
            {
                WriteRuntime(jsonWriter, configuration.Runtime.Value);
            }

            WriteStepAssemblies(jsonWriter, configuration.BindingAssemblies);
        }

        private void WriteLanguage(JsonWriter jsonWriter, CultureInfo featureLanguage)
        {
            // open language object
            jsonWriter.WritePropertyName("language");
            jsonWriter.WriteStartObject();

            jsonWriter.WritePropertyName("feature");
            jsonWriter.WriteValue(featureLanguage.Name);

            // close language object
            jsonWriter.WriteEndObject();
        }

        private void WriteBindingCulture(JsonWriter jsonWriter, CultureInfo bindingCulture)
        {
            jsonWriter.WritePropertyName("bindingCulture");
            jsonWriter.WriteStartObject();

            jsonWriter.WritePropertyName("name");
            jsonWriter.WriteValue(bindingCulture.Name);
            jsonWriter.WriteEndObject();
        }

        private void WriteStepAssemblies(JsonWriter jsonWriter, IEnumerable<BindingAssembly> stepAssemblies)
        {
            if (!(stepAssemblies is null))
            {
                // open stepAssemblies array
                jsonWriter.WritePropertyName("stepAssemblies");
                jsonWriter.WriteStartArray();

                foreach (var stepAssembly in stepAssemblies)
                {
                    WriteStepAssembly(jsonWriter, stepAssembly);
                }

                // close stepAssemblies array
                jsonWriter.WriteEndArray();
            }
        }

        private void WriteStepAssembly(JsonWriter jsonWriter, BindingAssembly stepAssembly)
        {
            // open stepAssembly object
            jsonWriter.WriteStartObject();

            jsonWriter.WritePropertyName("assembly");
            jsonWriter.WriteValue(stepAssembly.Assembly);

            // clase stepAssembly object
            jsonWriter.WriteEndObject();
        }

        private void WriteGenerator(JsonWriter jsonWriter, Generator generator)
        {
            jsonWriter.WritePropertyName("generator");
            jsonWriter.WriteStartObject();

            jsonWriter.WritePropertyName("allowDebugGeneratedFiles");
            jsonWriter.WriteValue(generator.AllowDebugGeneratedFiles);

            jsonWriter.WritePropertyName("allowRowTests");
            jsonWriter.WriteValue(generator.AllowRowTests);

            jsonWriter.WritePropertyName("generateAsyncTests");
            jsonWriter.WriteValue(generator.GenerateAsyncTests);

            jsonWriter.WritePropertyName("path");
            jsonWriter.WriteValue(generator.Path);

            if (generator.Dependencies.Count > 0)
            {
                jsonWriter.WritePropertyName("dependencies");
                WriteDependencies(jsonWriter, generator.Dependencies);
            }

            if (generator.AddNonParallelizableMarkerForTags.Count > 0)
            {
                jsonWriter.WritePropertyName("addNonParallelizableMarkerForTags");
                jsonWriter.WriteStartArray();
                foreach (var tagName in generator.AddNonParallelizableMarkerForTags)
                {
                    jsonWriter.WriteValue(tagName);
                }
                jsonWriter.WriteEndArray();
            }

            jsonWriter.WriteEndObject();
        }

        private void WriteRuntime(JsonWriter jsonWriter, Runtime runtime)
        {
            jsonWriter.WritePropertyName("runtime");
            jsonWriter.WriteStartObject();

            jsonWriter.WritePropertyName("missingOrPendingStepsOutcome");
            jsonWriter.WriteValue(runtime.MissingOrPendingStepsOutcome.ToString());

            jsonWriter.WritePropertyName("stopAtFirstError");
            jsonWriter.WriteValue(runtime.StopAtFirstError);

            if (runtime.ObsoleteBehavior.IsNotNullOrWhiteSpace())
            {
                jsonWriter.WritePropertyName("obsoleteBehavior");
                jsonWriter.WriteValue(runtime.ObsoleteBehavior);
            }

            if (runtime.Dependencies.Count > 0)
            {
                jsonWriter.WritePropertyName("dependencies");
                WriteDependencies(jsonWriter, runtime.Dependencies);
            }

            jsonWriter.WriteEndObject();
        }

        private void WriteDependencies(JsonWriter jsonWriter, IEnumerable<IDependency> dependencies)
        {
            jsonWriter.WriteStartArray();

            foreach (var dependency in dependencies)
            {
                switch (dependency)
                {
                    case RegisterDependency registerDependency:
                        WriteRegisterDependency(jsonWriter, registerDependency);
                        break;
                    case null: throw new InvalidOperationException("null is not supported as dependency.");
                    default: throw new NotSupportedException($"Dependency type {dependency.GetType()} is not supported.");
                }
            }

            jsonWriter.WriteEndArray();
        }

        private void WriteRegisterDependency(JsonWriter jsonWriter, RegisterDependency dependency)
        {
            jsonWriter.WriteStartObject();

            jsonWriter.WritePropertyName("type");
            jsonWriter.WriteValue(dependency.Type);

            jsonWriter.WritePropertyName("as");
            jsonWriter.WriteValue(dependency.As);

            if (dependency.Name.IsNotNullOrWhiteSpace())
            {
                jsonWriter.WritePropertyName("name");
                jsonWriter.WriteValue(dependency.Name);
            }

            jsonWriter.WriteEndObject();
        }
    }
}

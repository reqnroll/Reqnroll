using Reqnroll.Bindings.Provider;
using Reqnroll.Bindings.Reflection;
using Reqnroll.Configuration;
using Reqnroll.Infrastructure;
using Reqnroll.PlatformCompatibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.IO;

namespace Reqnroll.Bindings.Discovery
{
    /// <summary>
    /// Implementation of IRuntimeBindingRegistryBuilder that uses MetadataLoadContext
    /// for assembly inspection without loading assemblies into the runtime context.
    /// </summary>
    public class MetadataLoadContextBindingRegistryBuilder : IRuntimeBindingRegistryBuilder, IDisposable
    {
        private readonly IRuntimeBindingSourceProcessor _bindingSourceProcessor;
        private readonly IReqnrollAttributesFilter _reqnrollAttributesFilter;
        private readonly IBindingAssemblyLoader _bindingAssemblyLoader;
        private readonly ReqnrollConfiguration _reqnrollConfiguration;
        private readonly IBindingProviderService _bindingProviderService;
        private MetadataLoadContext _metadataLoadContext;
        private readonly HashSet<string> _loadedAssemblyPaths;

        public MetadataLoadContextBindingRegistryBuilder(
            IRuntimeBindingSourceProcessor bindingSourceProcessor,
            IReqnrollAttributesFilter reqnrollAttributesFilter,
            IBindingAssemblyLoader bindingAssemblyLoader,
            ReqnrollConfiguration reqnrollConfiguration,
            IBindingProviderService bindingProviderService)
        {
            _bindingSourceProcessor = bindingSourceProcessor;
            _reqnrollAttributesFilter = reqnrollAttributesFilter;
            _bindingAssemblyLoader = bindingAssemblyLoader;
            _reqnrollConfiguration = reqnrollConfiguration;
            _bindingProviderService = bindingProviderService;
            _loadedAssemblyPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public Assembly[] GetBindingAssemblies(Assembly testAssembly)
        {
            var bindingAssemblies = new List<Assembly> { testAssembly };

            bindingAssemblies.AddRange(
                _reqnrollConfiguration.AdditionalStepAssemblies.Select(_bindingAssemblyLoader.Load));
            
            return bindingAssemblies.ToArray();
        }

        public void BuildBindingsFromAssembly(Assembly assembly)
        {
            // Initialize MetadataLoadContext if not already created
            if (_metadataLoadContext == null)
            {
                _metadataLoadContext = CreateMetadataLoadContext(assembly);
            }

            // Load the assembly into the MetadataLoadContext
            Assembly mlcAssembly = LoadAssemblyIntoMetadataContext(assembly);
            
            var assemblyTypes = GetAssemblyTypes(mlcAssembly, out var typeLoadErrors);
            
            foreach (var type in assemblyTypes)
            {
                BuildBindingsFromType(type);
            }
            
            if (typeLoadErrors != null)
            {
                foreach (string typeLoadError in typeLoadErrors)
                {
                    _bindingSourceProcessor.RegisterTypeLoadError(typeLoadError);
                }
            }
        }

        public void BuildingCompleted()
        {
            _bindingSourceProcessor.BuildingCompleted();
            _bindingProviderService.OnBindingRegistryBuildingCompleted();
        }

        private MetadataLoadContext CreateMetadataLoadContext(Assembly seedAssembly)
        {
            // Build a list of reference assembly paths
            var assemblyPaths = new List<string>();
            
            // Add the seed assembly
            if (!string.IsNullOrEmpty(seedAssembly.Location))
            {
                assemblyPaths.Add(seedAssembly.Location);
                
                // Add referenced assemblies
                var assemblyDirectory = Path.GetDirectoryName(seedAssembly.Location);
                if (!string.IsNullOrEmpty(assemblyDirectory))
                {
                    // Add all DLLs in the same directory
                    assemblyPaths.AddRange(Directory.GetFiles(assemblyDirectory, "*.dll"));
                }
            }

            // Add runtime assemblies for core types
            var runtimeAssemblies = Directory.GetFiles(
                Path.GetDirectoryName(typeof(object).Assembly.Location),
                "*.dll");
            assemblyPaths.AddRange(runtimeAssemblies);

            var resolver = new PathAssemblyResolver(assemblyPaths.Distinct());
            return new MetadataLoadContext(resolver, typeof(object).Assembly.GetName().Name);
        }

        private Assembly LoadAssemblyIntoMetadataContext(Assembly runtimeAssembly)
        {
            if (string.IsNullOrEmpty(runtimeAssembly.Location))
            {
                // For in-memory assemblies, we may need to fall back to runtime reflection
                // or load by name if available in the resolver
                return _metadataLoadContext.LoadFromAssemblyName(runtimeAssembly.GetName());
            }

            if (_loadedAssemblyPaths.Contains(runtimeAssembly.Location))
            {
                return _metadataLoadContext.LoadFromAssemblyPath(runtimeAssembly.Location);
            }

            _loadedAssemblyPaths.Add(runtimeAssembly.Location);
            return _metadataLoadContext.LoadFromAssemblyPath(runtimeAssembly.Location);
        }

        protected virtual Type[] GetAssemblyTypes(Assembly assembly, out string[] typeLoadErrors)
        {
            typeLoadErrors = Array.Empty<string>();
            
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                typeLoadErrors = e.LoaderExceptions?
                    .Where(ex => ex != null)
                    .Select(loaderException => loaderException.ToString())
                    .ToArray() ?? Array.Empty<string>();
                
                return e.Types.Where(t => t != null).ToArray();
            }
            catch (Exception ex)
            {
                typeLoadErrors = new[] { ex.ToString() };
                return Array.Empty<Type>();
            }
        }

        internal bool BuildBindingsFromType(Type type)
        {
            // Get custom attribute data (metadata) instead of materialized attributes
            var customAttributesData = type.GetCustomAttributesData();
            
            // DIAGNOSTIC: Log what we're finding
            System.Diagnostics.Debug.WriteLine($"[MLC] Processing type: {type.FullName}");
            System.Diagnostics.Debug.WriteLine($"[MLC] Found {customAttributesData.Count} total attributes");
            foreach (var attr in customAttributesData)
            {
                System.Diagnostics.Debug.WriteLine($"[MLC]   - {attr.AttributeType.FullName}");
            }
            
            var filteredAttributeData = customAttributesData
                .Where(attrData => _bindingSourceProcessor.CanProcessTypeAttribute(attrData.AttributeType.FullName))
                .ToList();

            System.Diagnostics.Debug.WriteLine($"[MLC] After CanProcessTypeAttribute filter: {filteredAttributeData.Count} attributes");
            
            var fullNames = filteredAttributeData.Select(ad => ad.AttributeType.FullName).ToList();
            System.Diagnostics.Debug.WriteLine($"[MLC] Full names: {string.Join(", ", fullNames)}");
            
            if (!_bindingSourceProcessor.PreFilterType(fullNames))
            {
                System.Diagnostics.Debug.WriteLine($"[MLC] PreFilterType returned FALSE - type rejected");
                return false;
            }

            System.Diagnostics.Debug.WriteLine($"[MLC] PreFilterType returned TRUE - processing type");
            
            var bindingSourceType = CreateBindingSourceType(type, filteredAttributeData);
            System.Diagnostics.Debug.WriteLine($"[MLC] Created BindingSourceType, calling ProcessType");

            bool processTypeResult = _bindingSourceProcessor.ProcessType(bindingSourceType);
            System.Diagnostics.Debug.WriteLine($"[MLC] ProcessType returned: {processTypeResult}");
            
            if (!processTypeResult)
                return false;

            var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            System.Diagnostics.Debug.WriteLine($"[MLC] Found {methods.Length} methods on type");
            
            foreach (var methodInfo in methods)
            {
                System.Diagnostics.Debug.WriteLine($"[MLC]   Processing method: {methodInfo.Name}");
                var bindingMethod = CreateBindingSourceMethod(methodInfo);
                if (bindingMethod != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[MLC]     Method has {bindingMethod.Attributes.Length} attributes");
                    _bindingSourceProcessor.ProcessMethod(bindingMethod);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[MLC]     Method has no binding attributes - skipped");
                }
            }

            _bindingSourceProcessor.ProcessTypeDone();
            return true;
        }

        private BindingSourceMethod CreateBindingSourceMethod(MethodInfo methodDefinition)
        {
            var customAttributesData = methodDefinition.GetCustomAttributesData()
                .Where(attrData => _bindingSourceProcessor.CanProcessTypeAttribute(attrData.AttributeType.FullName))
                .ToList();

            // Filter to only Reqnroll binding method attributes (or those that derive from them)
            var bindingMethodAttributes = customAttributesData
                .Where(attrData => IsBindingMethodAttribute(attrData.AttributeType))
                .ToList();

            // If no binding method attributes found, skip this method
            if (bindingMethodAttributes.Count == 0)
                return null;

            return new BindingSourceMethod
            {
                BindingMethod = new RuntimeBindingMethod(methodDefinition),
                IsPublic = methodDefinition.IsPublic,
                IsStatic = methodDefinition.IsStatic,
                Attributes = GetAttributes(bindingMethodAttributes)
            };
        }

        private IBindingType CreateBindingType(Type type)
        {
            return new RuntimeBindingType(type);
        }

        private BindingSourceType CreateBindingSourceType(Type type, IEnumerable<CustomAttributeData> filteredAttributeData)
        {
            var attributes = GetAttributes(filteredAttributeData);
            System.Diagnostics.Debug.WriteLine($"[MLC] CreateBindingSourceType: created {attributes.Length} BindingSourceAttribute objects");
            
            return new BindingSourceType
            {
                BindingType = CreateBindingType(type),
                IsAbstract = type.IsAbstract,
                IsClass = type.IsClass,
                IsPublic = type.IsPublic,
                IsNested = TypeHelper.IsNested(type),
                IsGenericTypeDefinition = type.IsGenericTypeDefinition,
                Attributes = attributes
            };
        }

        /// <summary>
        /// Creates a BindingSourceAttribute from CustomAttributeData.
        /// This extracts attribute metadata without materializing the attribute instance.
        /// Mimics the RuntimeBindingRegistryBuilder approach of reading fields/properties and matching constructor parameters.
        /// </summary>
        private BindingSourceAttribute CreateAttribute(CustomAttributeData attributeData)
        {
            var attributeType = attributeData.AttributeType;

            // Build named attribute values dictionary
            // We cannot read runtime values from fields/properties with MetadataLoadContext,
            // so we infer them from constructor arguments and named arguments
            var namedAttributeValues = new Dictionary<string, IBindingSourceAttributeValueProvider>();

            // Populate values from constructor arguments
            if (attributeData.Constructor != null)
            {
                var ctorParams = attributeData.Constructor.GetParameters();
                for (int i = 0; i < Math.Min(ctorParams.Length, attributeData.ConstructorArguments.Count); i++)
                {
                    var param = ctorParams[i];
                    var arg = attributeData.ConstructorArguments[i];
                    var value = ExtractAttributeValue(arg);

                    // Try to find matching field/property name by capitalizing the parameter name
                    // Constructor parameters are typically lowercase versions of the property/field names
                    if (!string.IsNullOrEmpty(param.Name))
                    {
                        var capitalizedName = char.ToUpper(param.Name[0]) + param.Name.Substring(1);
                        namedAttributeValues[capitalizedName] = new BindingSourceAttributeValueProvider(value);
                        
                        // Also store with lowercase name in case there's a field with that name
                        namedAttributeValues[param.Name] = new BindingSourceAttributeValueProvider(value);
                    }
                }
            }

            // Override with explicitly set named arguments (properties and fields set in attribute declaration)
            foreach (var namedArg in attributeData.NamedArguments)
            {
                var value = ExtractAttributeValue(namedArg.TypedValue);
                namedAttributeValues[namedArg.MemberName] = new BindingSourceAttributeValueProvider(value);
            }

            // NOTE: We do NOT add fields/properties that weren't set to the dictionary with null values.
            // If a key is missing from NamedAttributeValues, TryGetAttributeValue will return the default value.
            // This is important for value types like Order (int) - storing null would cause NullReferenceException
            // when casting null to int.

            // Build AttributeValues array using the most complex constructor approach
            // Find the most complex constructor (the one with the most parameters)
            var mostComplexCtor = attributeType.GetConstructors(BindingFlags.Instance | BindingFlags.Public)
                .OrderByDescending(ctor => ctor.GetParameters().Length)
                .FirstOrDefault();

            IBindingSourceAttributeValueProvider[] attributeValues;
            if (mostComplexCtor == null)
            {
                attributeValues = Array.Empty<IBindingSourceAttributeValueProvider>();
            }
            else
            {
                // Match constructor parameters to field/property values
                attributeValues = mostComplexCtor.GetParameters()
                    .Select(p => FindAttributeConstructorArg(p, namedAttributeValues))
                    .ToArray();
            }

            return new BindingSourceAttribute
            {
                AttributeType = CreateBindingType(attributeType),
                AttributeValues = attributeValues,
                NamedAttributeValues = namedAttributeValues
            };
        }

        /// <summary>
        /// Finds the attribute constructor argument value by matching parameter name to field/property names.
        /// Mimics RuntimeBindingRegistryBuilder.FindAttributeConstructorArg
        /// </summary>
        private IBindingSourceAttributeValueProvider FindAttributeConstructorArg(ParameterInfo parameterInfo, Dictionary<string, IBindingSourceAttributeValueProvider> namedAttributeValues)
        {
            var paramName = parameterInfo.Name;
            if (string.IsNullOrEmpty(paramName))
                return new BindingSourceAttributeValueProvider(null);
                
            if (namedAttributeValues.TryGetValue(paramName, out var result))
                return result;
            if (namedAttributeValues.TryGetValue(paramName.Substring(0, 1).ToUpper() + paramName.Substring(1), out result))
                return result;

            return new BindingSourceAttributeValueProvider(null);
        }

        /// <summary>
        /// Extracts the actual value from a CustomAttributeTypedArgument.
        /// Handles arrays, nested types, and primitive values.
        /// </summary>
        private object ExtractAttributeValue(CustomAttributeTypedArgument argument)
        {
            if (argument.Value == null)
                return null;

            // Handle arrays - need to create a properly typed array, not object[]
            if (argument.Value is IReadOnlyCollection<CustomAttributeTypedArgument> arrayValues)
            {
                // Extract element type from the array type
                var elementType = argument.ArgumentType.GetElementType();
                if (elementType != null)
                {
                    // Create a strongly-typed array
                    var extractedValues = arrayValues.Select(ExtractAttributeValue).ToArray();
                    var typedArray = Array.CreateInstance(elementType, extractedValues.Length);
                    for (int i = 0; i < extractedValues.Length; i++)
                    {
                        typedArray.SetValue(extractedValues[i], i);
                    }
                    return typedArray;
                }
                
                // Fallback to object array if we can't determine element type
                return arrayValues.Select(ExtractAttributeValue).ToArray();
            }

            // Handle Type arguments
            if (argument.ArgumentType.FullName == "System.Type" || argument.ArgumentType.Name == "Type")
            {
                // The value is already a Type from the MetadataLoadContext
                return argument.Value;
            }

            // For enums, the value is the underlying integer
            if (argument.ArgumentType.IsEnum)
            {
                return argument.Value;
            }

            // Primitive types and strings
            return argument.Value;
        }

        /// <summary>
        /// Converts a constructor parameter name to a likely property name.
        /// Example: "pattern" -> "Pattern"
        /// </summary>
        private string GetPropertyNameFromParameter(string parameterName)
        {
            if (string.IsNullOrEmpty(parameterName))
                return string.Empty;

            // Capitalize first letter
            return char.ToUpper(parameterName[0]) + parameterName.Substring(1);
        }

        private BindingSourceAttribute[] GetAttributes(IEnumerable<CustomAttributeData> customAttributesData)
        {
            // Note: IReqnrollAttributesFilter expects Attribute instances, not CustomAttributeData
            // We need to create a workaround or use a different filtering approach
            
            // For now, we'll filter based on known Reqnroll attribute types
            // In a full implementation, you might need to adapt IReqnrollAttributesFilter
            // to work with CustomAttributeData or Type information
            
            return customAttributesData
                .Where(ad => IsReqnrollAttribute(ad.AttributeType))
                .Select(CreateAttribute)
                .ToArray();
        }

        /// <summary>
        /// Determines if an attribute type is a Reqnroll binding method attribute,
        /// or derives from one (e.g., custom step definition attributes that inherit from StepDefinitionBaseAttribute).
        /// </summary>
        private bool IsBindingMethodAttribute(Type attributeType)
        {
            if (attributeType == null)
                return false;

            var fullName = attributeType.FullName;
            if (fullName == null)
                return false;

            // Check known Reqnroll method attribute types
            // Step definition attributes
            if (fullName == "Reqnroll.GivenAttribute" ||
                fullName == "Reqnroll.WhenAttribute" ||
                fullName == "Reqnroll.ThenAttribute" ||
                fullName == "Reqnroll.StepDefinitionAttribute" ||
                fullName == "Reqnroll.StepDefinitionBaseAttribute")
                return true;

            // Step argument transformation
            if (fullName == "Reqnroll.StepArgumentTransformationAttribute")
                return true;

            // Hook attributes
            if (fullName == "Reqnroll.BeforeScenarioAttribute" ||
                fullName == "Reqnroll.AfterScenarioAttribute" ||
                fullName == "Reqnroll.BeforeFeatureAttribute" ||
                fullName == "Reqnroll.AfterFeatureAttribute" ||
                fullName == "Reqnroll.BeforeStepAttribute" ||
                fullName == "Reqnroll.AfterStepAttribute" ||
                fullName == "Reqnroll.BeforeTestRunAttribute" ||
                fullName == "Reqnroll.AfterTestRunAttribute" ||
                fullName == "Reqnroll.BeforeScenarioBlockAttribute" ||
                fullName == "Reqnroll.AfterScenarioBlockAttribute")
                return true;

            // Check for SpecFlow compatibility (backward compatibility)
            if (fullName.StartsWith("TechTalk.SpecFlow."))
            {
                var specFlowName = fullName.Substring("TechTalk.SpecFlow.".Length);
                if (specFlowName == "GivenAttribute" ||
                    specFlowName == "WhenAttribute" ||
                    specFlowName == "ThenAttribute" ||
                    specFlowName == "StepDefinitionAttribute" ||
                    specFlowName == "BeforeScenarioAttribute" ||
                    specFlowName == "AfterScenarioAttribute" ||
                    specFlowName == "BeforeFeatureAttribute" ||
                    specFlowName == "AfterFeatureAttribute" ||
                    specFlowName == "BeforeStepAttribute" ||
                    specFlowName == "AfterStepAttribute" ||
                    specFlowName == "BeforeTestRunAttribute" ||
                    specFlowName == "AfterTestRunAttribute" ||
                    specFlowName == "StepArgumentTransformationAttribute")
                    return true;
            }

            // Check if it inherits from a known base attribute (for custom attributes like GivenAndWhenAttribute)
            // Walk up the inheritance chain using MLC type's BaseType
            var baseType = attributeType.BaseType;
            while (baseType != null && baseType.FullName != "System.Attribute" && baseType.FullName != "System.Object")
            {
                if (IsBindingMethodAttribute(baseType))
                    return true;
                baseType = baseType.BaseType;
            }

            return false;
        }

        /// <summary>
        /// Determines if an attribute type is a Reqnroll attribute.
        /// This is a simplified version that checks namespace.
        /// A full implementation would coordinate with IReqnrollAttributesFilter.
        /// </summary>
        private bool IsReqnrollAttribute(Type attributeType)
        {
            if (attributeType == null)
                return false;

            var fullName = attributeType.FullName;
            if (fullName == null)
                return false;

            // Check if it's in a Reqnroll namespace
            return fullName.StartsWith("Reqnroll.") ||
                   fullName.StartsWith("TechTalk.SpecFlow.") // For backwards compatibility
                   || attributeType.Name.EndsWith("Attribute");
        }

        public void Dispose()
        {
            _metadataLoadContext?.Dispose();
            _metadataLoadContext = null;
        }
    }
}

using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Reqnroll.Assist.ValueRetrievers;
using Reqnroll.Bindings.Reflection;
using Reqnroll.Infrastructure;
using Reqnroll.Tracing;

namespace Reqnroll.Bindings
{
    public class StepArgumentTypeConverter : IStepArgumentTypeConverter
    {
        private readonly ITestTracer testTracer;
        private readonly IBindingRegistry bindingRegistry;
        private readonly IAsyncBindingInvoker bindingInvoker;

        public StepArgumentTypeConverter(ITestTracer testTracer, IBindingRegistry bindingRegistry, IAsyncBindingInvoker bindingInvoker)
        {
            this.testTracer = testTracer;
            this.bindingRegistry = bindingRegistry;
            this.bindingInvoker = bindingInvoker;
        }

        protected virtual IStepArgumentTransformationBinding GetMatchingStepTransformation(object value, IBindingType typeToConvertTo, bool traceWarning)
        {
            var stepTransformations = bindingRegistry.GetStepTransformations().Where(t => CanConvert(t, value, typeToConvertTo)).ToArray();
            if (stepTransformations.Length > 1 && traceWarning)
            {
                testTracer.TraceWarning($"Multiple step transformation matches to the input ({value}, target type: {typeToConvertTo}). We use the first.");
            }

            return stepTransformations.Length > 0 ? stepTransformations[0] : null;
        }

        public async Task<object> ConvertAsync(object value, IBindingType typeToConvertTo, IContextManager contextManager, CultureInfo cultureInfo)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            var stepTransformation = GetMatchingStepTransformation(value, typeToConvertTo, true);
            if (stepTransformation != null)
                return await DoTransformAsync(stepTransformation, value, contextManager, cultureInfo);

            if (typeToConvertTo is RuntimeBindingType convertToType && convertToType.Type.IsInstanceOfType(value))
                return value;

            return ConvertSimple(typeToConvertTo, value, cultureInfo);
        }

        private async Task<object> DoTransformAsync(IStepArgumentTransformationBinding stepTransformation, object value, IContextManager contextManager, CultureInfo cultureInfo)
        {
            object[] arguments;
            if (stepTransformation.Regex != null && value is string stringValue)
                arguments = await GetStepTransformationArgumentsFromRegexAsync(stepTransformation, stringValue, contextManager, cultureInfo);
            else
                arguments = new[] { await ConvertAsync(value, stepTransformation.Method.Parameters.ElementAtOrDefault(0)?.Type ?? new RuntimeBindingType(typeof(object)), contextManager, cultureInfo)};

            var result = await bindingInvoker.InvokeBindingAsync(stepTransformation, contextManager, arguments, testTracer, new DurationHolder());

            return result;
        }

        private async Task<object[]> GetStepTransformationArgumentsFromRegexAsync(IStepArgumentTransformationBinding stepTransformation, string stepSnippet, IContextManager contextManager, CultureInfo cultureInfo)
        {
            var match = stepTransformation.Regex.Match(stepSnippet);
            var argumentStrings = match.Groups.Cast<Group>().Skip(1).Select(g => g.Value).ToList();
            var bindingParameters = stepTransformation.Method.Parameters.ToArray();
            
            var result = new object[argumentStrings.Count];

            for (int i = 0; i < argumentStrings.Count; i++)
            {
                result[i] = await ConvertAsync(argumentStrings[i], bindingParameters[i].Type, contextManager, cultureInfo);
            }

            return result;
        }

        public bool CanConvert(object value, IBindingType typeToConvertTo, CultureInfo cultureInfo)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            var stepTransformation = GetMatchingStepTransformation(value, typeToConvertTo, false);
            if (stepTransformation != null)
                return true;

            if (typeToConvertTo is RuntimeBindingType convertToType && convertToType.Type.IsInstanceOfType(value))
                return true;

            return CanConvertSimple(typeToConvertTo, value, cultureInfo);
        }

        private bool CanConvert(IStepArgumentTransformationBinding stepTransformationBinding, object valueToConvert, IBindingType typeToConvertTo)
        {
            var awaitableReturnType = stepTransformationBinding.Method.GetAwaitableReturnType();
            if (!awaitableReturnType.TypeEquals(typeToConvertTo))
                return false;

            if (stepTransformationBinding.Regex != null && valueToConvert is string stringValue)
                return stepTransformationBinding.Regex.IsMatch(stringValue);

            var transformationFirstArgumentType = stepTransformationBinding.Method.Parameters.FirstOrDefault()?.Type;

            var isTableStepTransformation = transformationFirstArgumentType != null &&
                                            IsDataTableType(transformationFirstArgumentType);
            var valueIsTable = valueToConvert is Table;

            return isTableStepTransformation == valueIsTable;
        }

        protected virtual bool IsDataTableType(IBindingType bindingType)
        {
            return bindingType.FullName == typeof(Table).FullName || 
                   (bindingType is RuntimeBindingType runtimeBindingType && 
                    typeof(Table).IsAssignableFrom(runtimeBindingType));
        }

        private static object ConvertSimple(IBindingType typeToConvertTo, object value, CultureInfo cultureInfo)
        {
            if (typeToConvertTo is not RuntimeBindingType runtimeBindingType)
                throw new ReqnrollException("The StepArgumentTypeConverter can be used with runtime types only.");

            return ConvertSimple(runtimeBindingType.Type, value, cultureInfo);
        }

        private static object ConvertSimple(Type typeToConvertTo, object value, CultureInfo cultureInfo)
        {
            if (typeToConvertTo.IsEnum && value is string stringValue)
                return ConvertToAnEnum(typeToConvertTo, stringValue);

            if (typeToConvertTo == typeof(Guid?) && string.IsNullOrEmpty(value as string))
                return null;

            if (typeToConvertTo == typeof(Guid) || typeToConvertTo == typeof(Guid?))
                return new GuidValueRetriever().GetValue(value as string);

            return TryConvertWithTypeConverter(typeToConvertTo, value, cultureInfo, out var convertedValue)
                ? convertedValue :
                System.Convert.ChangeType(value, typeToConvertTo, cultureInfo);
        }

        private static bool TryConvertWithTypeConverter(Type typeToConvertTo, object value, CultureInfo cultureInfo, out object result)
        {
            var typeConverter = TypeDescriptor.GetConverter(typeToConvertTo);

            if (typeConverter.CanConvertFrom(value.GetType()))
            {
                try
                {
                    result = typeConverter.ConvertFrom(null, cultureInfo, value);
                    return true;
                }
                catch
                {
                    // Ignore any exceptions.
                }
            }

            result = null;
            return false;
        }

        public static object ConvertToAnEnum(Type enumType, string value)
        {
            return Enum.Parse(enumType, RemoveWhitespace(value), true);
        }

        private static string RemoveWhitespace(string value)
        {
            return value.Replace(" ", string.Empty);
        }

        public static bool CanConvertSimple(IBindingType typeToConvertTo, object value, CultureInfo cultureInfo)
        {
            try
            {
                ConvertSimple(typeToConvertTo, value, cultureInfo);
                return true;
            }
            catch (InvalidCastException)
            {
                return false;
            }
            catch (OverflowException)
            {
                return false;
            }
            catch (FormatException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }
    }
}

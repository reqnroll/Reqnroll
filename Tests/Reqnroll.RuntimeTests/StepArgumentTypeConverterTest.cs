using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using Reqnroll.Bindings;
using Reqnroll.Bindings.Reflection;
using Reqnroll.Infrastructure;
using Reqnroll.Tracing;

namespace Reqnroll.RuntimeTests
{
    public class StepArgumentTypeConverterTests
    {
        private readonly Mock<ITestTracer> _testTracer;
        private readonly List<IStepArgumentTransformationBinding> _stepTransformations;
        private readonly IStepArgumentTypeConverter _stepArgumentTypeConverter;
        private readonly Mock<IAsyncBindingInvoker> methodBindingInvokerStub = new();
        private readonly CultureInfo _enUSCulture;

        public StepArgumentTypeConverterTests()
        {
            Mock<IBindingRegistry> bindingRegistryStub = new Mock<IBindingRegistry>();
            _stepTransformations = new List<IStepArgumentTransformationBinding>();
            bindingRegistryStub.Setup(br => br.GetStepTransformations()).Returns(_stepTransformations);
            _testTracer = new Mock<ITestTracer>();

            _stepArgumentTypeConverter = new StepArgumentTypeConverter(_testTracer.Object, bindingRegistryStub.Object, new Mock<IContextManager>().Object, methodBindingInvokerStub.Object);
            _enUSCulture = new CultureInfo("en-US", false);
        }

        [Fact]
        public async Task ShouldConvertStringToStringType()
        {
            var typeToConvertTo = new RuntimeBindingType(typeof(string));
            var result = await _stepArgumentTypeConverter.ConvertAsync("testValue", typeToConvertTo, _enUSCulture);
            result.Should().Be("testValue");
        }

        [Fact]
        public async Task ShouldConvertStringToIntType()
        {
            var typeToConvertTo = new RuntimeBindingType(typeof(int));
            var result = await _stepArgumentTypeConverter.ConvertAsync("10", typeToConvertTo, _enUSCulture);
            result.Should().Be(10);
        }

        [Fact]
        public async Task ShouldConvertStringToDateType()
        {
            var typeToConvertTo = new RuntimeBindingType(typeof(DateTime));
            var result = await _stepArgumentTypeConverter.ConvertAsync("2009/10/06", typeToConvertTo, _enUSCulture);
            result.Should().Be(new DateTime(2009, 10, 06));
        }

        [Fact]
        public async Task ShouldConvertStringToFloatType()
        {
            var typeToConvertTo = new RuntimeBindingType(typeof(float));
            var result = await _stepArgumentTypeConverter.ConvertAsync("10.01", typeToConvertTo, _enUSCulture);
            result.Should().Be(10.01f);
        }

        private enum TestEnumeration
        {
            Value1
        }

        [Fact]
        public async Task ShouldConvertStringToEnumerationType()
        {
            var typeToConvertTo = new RuntimeBindingType(typeof(TestEnumeration));
            var result = await _stepArgumentTypeConverter.ConvertAsync("Value1", typeToConvertTo, _enUSCulture);
            result.Should().Be(TestEnumeration.Value1);
        }

        [Fact]
        public async Task ShouldConvertStringToEnumerationTypeWithDifferingCase()
        {
            var typeToConvertTo = new RuntimeBindingType(typeof(TestEnumeration));
            var result = await _stepArgumentTypeConverter.ConvertAsync("vALUE1", typeToConvertTo, _enUSCulture);
            result.Should().Be(TestEnumeration.Value1);
        }

        [Fact]
        public async Task ShouldConvertStringToEnumerationTypeWithWhitespace()
        {
            var typeToConvertTo = new RuntimeBindingType(typeof(TestEnumeration));
            var result = await _stepArgumentTypeConverter.ConvertAsync("Value 1", typeToConvertTo, _enUSCulture);
            result.Should().Be(TestEnumeration.Value1);
        }

        [Fact]
        public async Task ShouldConvertGuidToGuidType()
        {
            var typeToConvertTo = new RuntimeBindingType(typeof (Guid));
            var result = await _stepArgumentTypeConverter.ConvertAsync("{EF338B79-FD29-488F-8CA7-39C67C2B8874}", typeToConvertTo, _enUSCulture);
            result.Should().Be(new Guid("{EF338B79-FD29-488F-8CA7-39C67C2B8874}"));           
        }

        [Fact]
        public async Task ShouldConvertNullableGuidToGuidType()
        {
            var typeToConvertTo = new RuntimeBindingType(typeof(Guid?));
            var result = await _stepArgumentTypeConverter.ConvertAsync("{1081CFD1-F31F-420F-9360-40590ABEF887}", typeToConvertTo, _enUSCulture);
            result.Should().Be(new Guid("{1081CFD1-F31F-420F-9360-40590ABEF887}"));
        }

        [Fact]
        public async Task ShouldConvertNullableGuidWithEmptyValueToNull()
        {
            var typeToConvertTo = new RuntimeBindingType(typeof(Guid?));
            var result = await _stepArgumentTypeConverter.ConvertAsync("", typeToConvertTo, _enUSCulture);
            result.Should().BeNull();
        }

        [Fact]
        public async Task ShouldConvertLooseGuids()
        {
            var typeToConvertTo = new RuntimeBindingType(typeof (Guid));
            var result = await _stepArgumentTypeConverter.ConvertAsync("1", typeToConvertTo, _enUSCulture);
            result.Should().Be(new Guid("10000000-0000-0000-0000-000000000000"));
        }
        
        [Fact]
        public async Task ShouldUseATypeConverterWhenAvailable()
        {
            var originalValue = new DateTimeOffset(2019, 7, 29, 0, 0, 0, TimeSpan.Zero);
            var typeToConvertTo = new RuntimeBindingType(typeof (TestClass));
            var result = await _stepArgumentTypeConverter.ConvertAsync(originalValue, typeToConvertTo, _enUSCulture);
            result.Should().BeOfType(typeof(TestClass));
            result.As<TestClass>().Time.Should().Be(originalValue);
        }

        [Fact]
        public async Task ShouldTraceWarningIfMultipleTransformationsFound()
        {
            var method = typeof(TestClass).GetMethod(nameof(TestClass.StringToIntConverter));
            _stepTransformations.Add(new StepArgumentTransformationBinding(@"\d+", new RuntimeBindingMethod(method)));
            _stepTransformations.Add(new StepArgumentTransformationBinding(@".*", new RuntimeBindingMethod(method)));

            var typeToConvertTo = new RuntimeBindingType(typeof(int));
            await _stepArgumentTypeConverter.ConvertAsync("1", typeToConvertTo, _enUSCulture);
            
            _testTracer.Verify(c => c.TraceWarning(It.IsAny<string>()), Times.Once);
        }
        
        [Fact]
        public async Task ShouldNotTraceWarningIfTransformationsHaveDifferentOrder()
        {
            var method = typeof(TestClass).GetMethod(nameof(TestClass.StringToIntConverter));

            _stepTransformations.Add(new StepArgumentTransformationBinding(@"\d+", new RuntimeBindingMethod(method)));
            _stepTransformations.Add(new StepArgumentTransformationBinding(@".*", new RuntimeBindingMethod(method), order: 10));

            var typeToConvertTo = new RuntimeBindingType(typeof(int));
            await _stepArgumentTypeConverter.ConvertAsync("1", typeToConvertTo, _enUSCulture);
            
            _testTracer.Verify(c => c.TraceWarning(It.IsAny<string>()), Times.Never);
        }

        [TypeConverter(typeof(TessClassTypeConverter))]
        class TestClass
        {
            public DateTimeOffset Time { get; set; }
            
            class TessClassTypeConverter : TypeConverter
            {
                public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
                {
                    return sourceType == typeof(DateTimeOffset);
                }

                public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
                {
                    return new TestClass { Time = (DateTimeOffset) value };
                }
            }

            [StepArgumentTransformation]
            public int StringToIntConverter(string value) => int.Parse(value);
        }
    }
}

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Reqnroll.Bindings;
using Reqnroll.Bindings.Reflection;
using Reqnroll.BoDi;
using Reqnroll.Configuration;
using Reqnroll.EnvironmentAccess;
using Reqnroll.ErrorHandling;
using Reqnroll.Infrastructure;
using Reqnroll.Tracing;
using Xunit;

namespace Reqnroll.RuntimeTests
{
    public class User
    {
        public string Name { get; set; }
    }

    [Binding]
    public class UserCreator
    {
        [StepArgumentTransformation("user (w+)")]
        public User Create(string name)
        {
            return new User {Name = name};
        }

        [StepArgumentTransformation("user (w+)")]
        public async Task<User> AsyncCreate(string name)
        {
            await Task.Yield();
            return new User {Name = name};
        }

        [StepArgumentTransformation("user (w+)")]
        public async ValueTask<User> AsyncCreateValueTask(string name)
        {
            await Task.Yield();
            return new User {Name = name};
        }

        [StepArgumentTransformation]
        public IEnumerable<User> CreateUsers(Table table)
        {
            return table.Rows.Select(tableRow =>
                new User { Name = tableRow["Name"] });
        }
    }

    [Binding]
    public class TypeToTypeConverter
    {
        [StepArgumentTransformation("string (w+)")]
        public string StringToStringConvertRegex(string value)
        {
            return string.Concat("prefix ", value);
        }

        [StepArgumentTransformation]
        public string StringToStringConvert(string value)
        {
            return string.Concat("prefix ", value);
        }

        [StepArgumentTransformation]
        public Table TableToTableConvert(Table table)
        {
            var transformedTable = new List<string>();
            transformedTable.Add("transformed column");
            transformedTable.AddRange(table.Header);

            return new Table(transformedTable.ToArray());
        }
    }

    public class StepArgumentTransformationTests
    {
        private readonly Mock<IBindingRegistry> bindingRegistryStub = new Mock<IBindingRegistry>();
        private readonly Mock<IContextManager> contextManagerStub = new Mock<IContextManager>();
        private readonly Mock<IAsyncBindingInvoker> methodBindingInvokerStub = new Mock<IAsyncBindingInvoker>();
        private readonly List<IStepArgumentTransformationBinding> stepTransformations = new List<IStepArgumentTransformationBinding>();
        private readonly CultureInfo _enUSCulture = new CultureInfo("en-US", false);

        public StepArgumentTransformationTests()
        {
            // ScenarioContext is needed, because the [Binding]-instances live there
            var scenarioContext = new ScenarioContext(new ObjectContainer(), null, null, new TestObjectResolver());
            contextManagerStub.Setup(cm => cm.ScenarioContext).Returns(scenarioContext);

            bindingRegistryStub.Setup(br => br.GetStepTransformations()).Returns(stepTransformations);
        }

        private static IStepArgumentTransformationBinding CreateStepTransformationBinding(string regexString, IBindingMethod transformMethod)
        {
            return new StepArgumentTransformationBinding(regexString, transformMethod);
        }

        private static IStepArgumentTransformationBinding CreateStepTransformationBinding(string regexString, MethodInfo transformMethod)
        {
            return new StepArgumentTransformationBinding(regexString, new RuntimeBindingMethod(transformMethod));
        }

        private static IObjectContainer GetMockObjectContainerWithEnvWrapper()
        {
            var mockContainer = new Mock<IObjectContainer>();
            mockContainer.Setup(ctx => ctx.Resolve<IEnvironmentWrapper>()).Returns(new EnvironmentWrapperStub());
            return mockContainer.Object;
        }

        [Fact]
        public async Task UserConverterShouldConvertStringToUser()
        {
            UserCreator stepTransformationInstance = new UserCreator();
            var transformMethod = stepTransformationInstance.GetType().GetMethod("Create");
            var stepTransformationBinding = CreateStepTransformationBinding(@"user (\w+)", transformMethod);

            stepTransformationBinding.Regex.IsMatch("user xyz").Should().BeTrue();

            var invoker = new BindingInvoker(ConfigurationLoader.GetDefault(), new Mock<IErrorProvider>().Object, new BindingDelegateInvoker(), new EnvironmentOptions(new EnvironmentWrapperStub()));
            var result = await invoker.InvokeBindingAsync(stepTransformationBinding, contextManagerStub.Object, new object[] { "xyz" }, new Mock<ITestTracer>().Object, new DurationHolder());
            Assert.NotNull(result);
            result.Should().BeOfType<User>();
            ((User) result).Name.Should().Be("xyz");
        }

        [Fact]
        public async Task TypeToTypeConverterShouldConvertStringToStringUsingRegex()
        {
            TypeToTypeConverter stepTransformationInstance = new TypeToTypeConverter();
            var transformMethod = stepTransformationInstance.GetType().GetMethod("StringToStringConvertRegex");
            var stepTransformationBinding = CreateStepTransformationBinding(@"string (\w+)", transformMethod);

            Assert.Matches(stepTransformationBinding.Regex, "string xyz");

            var invoker = new BindingInvoker(ConfigurationLoader.GetDefault(), new Mock<IErrorProvider>().Object, new BindingDelegateInvoker(), new EnvironmentOptions(new EnvironmentWrapperStub()));
            var result  = await invoker.InvokeBindingAsync(stepTransformationBinding, contextManagerStub.Object, new object[] { "xyz" }, new Mock<ITestTracer>().Object, new DurationHolder());
            Assert.NotNull(result);
            result.GetType().Should().Be<string>();
            result.Should().Be("prefix xyz");
        }

        [Fact]
        public async Task TypeToTypeConverterShouldConvertStringToString()
        {
            TypeToTypeConverter stepTransformationInstance = new TypeToTypeConverter();
            var transformMethod = stepTransformationInstance.GetType().GetMethod("StringToStringConvert");
            var stepTransformationBinding = CreateStepTransformationBinding(@"", transformMethod);

            var invoker = new BindingInvoker(ConfigurationLoader.GetDefault(), new Mock<IErrorProvider>().Object, new BindingDelegateInvoker(), new EnvironmentOptions(new EnvironmentWrapperStub()));
            var result = await invoker.InvokeBindingAsync(stepTransformationBinding, contextManagerStub.Object, new object[] { "xyz" }, new Mock<ITestTracer>().Object, new DurationHolder());
            Assert.NotNull(result);
            result.GetType().Should().Be<string>();
            result.Should().Be("prefix xyz");
        }

        [Fact]
        public async Task TypeToTypeConverterShouldConvertTableToTable()
        {
            TypeToTypeConverter stepTransformationInstance = new TypeToTypeConverter();
            var transformMethod = stepTransformationInstance.GetType().GetMethod("TableToTableConvert");
            var stepTransformationBinding = CreateStepTransformationBinding(@"", transformMethod);

            var invoker = new BindingInvoker(ConfigurationLoader.GetDefault(), new Mock<IErrorProvider>().Object, new BindingDelegateInvoker(), new EnvironmentOptions(new EnvironmentWrapperStub()));
            var result = await invoker.InvokeBindingAsync(stepTransformationBinding, contextManagerStub.Object, new object[] { new Table("h1") }, new Mock<ITestTracer>().Object, new DurationHolder());
            Assert.NotNull(result);

            result.GetType().Should().Be<Table>();
            ((Table)result).Header.Should().BeEquivalentTo(new string[] { "transformed column", "h1" });
        }

        [Fact]
        public async Task StepArgumentTypeConverterShouldUseUserConverterForConversion()
        {
            UserCreator stepTransformationInstance = new UserCreator();
            var transformMethod = new RuntimeBindingMethod(stepTransformationInstance.GetType().GetMethod(nameof(UserCreator.Create)));
            var stepTransformationBinding = CreateStepTransformationBinding(@"user (\w+)", transformMethod);
            stepTransformations.Add(stepTransformationBinding);
            var resultUser = new User();
            methodBindingInvokerStub
                .Setup(i => i.InvokeBindingAsync(stepTransformationBinding, It.IsAny<IContextManager>(), It.IsAny<object[]>(), It.IsAny<ITestTracer>(), It.IsAny<DurationHolder>()))
                .ReturnsAsync(resultUser);

            var stepArgumentTypeConverter = CreateStepArgumentTypeConverter();

            var runtimeBindingType = new RuntimeBindingType(typeof(User));
            var result = await stepArgumentTypeConverter.ConvertAsync("user xyz", runtimeBindingType, _enUSCulture);
            result.Should().Be(resultUser);
        }

        [Fact]
        public async Task StepArgumentTypeConverterShouldUseAsyncUserConverterForConversion()
        {
            UserCreator stepTransformationInstance = new UserCreator();
            var transformMethod = new RuntimeBindingMethod(stepTransformationInstance.GetType().GetMethod(nameof(UserCreator.AsyncCreate)));
            var stepTransformationBinding = CreateStepTransformationBinding(@"user (\w+)", transformMethod);
            stepTransformations.Add(stepTransformationBinding);
            var resultUser = new User();
            methodBindingInvokerStub
                .Setup(i => i.InvokeBindingAsync(stepTransformationBinding, It.IsAny<IContextManager>(), It.IsAny<object[]>(), It.IsAny<ITestTracer>(), It.IsAny<DurationHolder>()))
                .ReturnsAsync(resultUser);

            var stepArgumentTypeConverter = CreateStepArgumentTypeConverter();

            var runtimeBindingType = new RuntimeBindingType(typeof(User));
            var result = await stepArgumentTypeConverter.ConvertAsync("user xyz", runtimeBindingType, _enUSCulture);
            result.Should().Be(resultUser);
        }

        [Fact]
        public async Task StepArgumentTypeConverterShouldUseAsyncValueTaskUserConverterForConversion()
        {
            UserCreator stepTransformationInstance = new UserCreator();
            var transformMethod = new RuntimeBindingMethod(stepTransformationInstance.GetType().GetMethod(nameof(UserCreator.AsyncCreateValueTask)));
            var stepTransformationBinding = CreateStepTransformationBinding(@"user (\w+)", transformMethod);
            stepTransformations.Add(stepTransformationBinding);
            var resultUser = new User();
            methodBindingInvokerStub
                .Setup(i => i.InvokeBindingAsync(stepTransformationBinding, It.IsAny<IContextManager>(), It.IsAny<object[]>(), It.IsAny<ITestTracer>(), It.IsAny<DurationHolder>()))
                .ReturnsAsync(resultUser);

            var stepArgumentTypeConverter = CreateStepArgumentTypeConverter();

            var typeToConvertTo = new RuntimeBindingType(typeof(User));
            var result = await stepArgumentTypeConverter.ConvertAsync("user xyz", typeToConvertTo, _enUSCulture);
            result.Should().Be(resultUser);
        }

        private StepArgumentTypeConverter CreateStepArgumentTypeConverter()
        {
            return new StepArgumentTypeConverter(new Mock<ITestTracer>().Object, bindingRegistryStub.Object, contextManagerStub.Object, methodBindingInvokerStub.Object);
        }

        [Fact]
        public async Task ShouldUseStepArgumentTransformationToConvertTable()
        {
            var table = new Table("Name");
            
            UserCreator stepTransformationInstance = new UserCreator();
            var transformMethod = new RuntimeBindingMethod(stepTransformationInstance.GetType().GetMethod(nameof(UserCreator.CreateUsers)));
            var stepTransformationBinding = CreateStepTransformationBinding(@"", transformMethod);
            stepTransformations.Add(stepTransformationBinding);
            var resultUsers = new User[3];
            methodBindingInvokerStub
                .Setup(i => i.InvokeBindingAsync(stepTransformationBinding, It.IsAny<IContextManager>(), new object[] { table }, It.IsAny<ITestTracer>(), It.IsAny<DurationHolder>()))
                .ReturnsAsync(resultUsers);

            var stepArgumentTypeConverter = CreateStepArgumentTypeConverter();

            var typeToConvertTo = new RuntimeBindingType(typeof(IEnumerable<User>));
            var result = await stepArgumentTypeConverter.ConvertAsync(table, typeToConvertTo, _enUSCulture);

            result.Should().NotBeNull();
            result.Should().Be(resultUsers);

        }
    }

}

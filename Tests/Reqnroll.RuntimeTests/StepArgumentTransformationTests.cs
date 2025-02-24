using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Reqnroll.BoDi;
using FluentAssertions;
using NSubstitute;
using Xunit;
using Reqnroll.Bindings;
using Reqnroll.Bindings.Reflection;
using Reqnroll.Configuration;
using Reqnroll.ErrorHandling;
using Reqnroll.Infrastructure;
using Reqnroll.Tracing;

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
        private readonly IBindingRegistry bindingRegistryStub = Substitute.For<IBindingRegistry>();
        private readonly IContextManager contextManagerStub = Substitute.For<IContextManager>();
        private readonly IAsyncBindingInvoker methodBindingInvokerStub = Substitute.For<IAsyncBindingInvoker>();
        private readonly List<IStepArgumentTransformationBinding> stepTransformations = new List<IStepArgumentTransformationBinding>();

        public StepArgumentTransformationTests()
        {
            // ScenarioContext is needed, because the [Binding]-instances live there
            var scenarioContext = new ScenarioContext(new ObjectContainer(), null, new TestObjectResolver());
            contextManagerStub.ScenarioContext.Returns(scenarioContext);

            bindingRegistryStub.GetStepTransformations().Returns(stepTransformations);
        }

        private IStepArgumentTransformationBinding CreateStepTransformationBinding(string regexString, IBindingMethod transformMethod)
        {
            return new StepArgumentTransformationBinding(regexString, transformMethod);
        }

        private IStepArgumentTransformationBinding CreateStepTransformationBinding(string regexString, MethodInfo transformMethod)
        {
            return new StepArgumentTransformationBinding(regexString, new RuntimeBindingMethod(transformMethod));
        }

        [Fact]
        public async Task UserConverterShouldConvertStringToUser()
        {
            UserCreator stepTransformationInstance = new UserCreator();
            var transformMethod = stepTransformationInstance.GetType().GetMethod("Create");
            var stepTransformationBinding = CreateStepTransformationBinding(@"user (\w+)", transformMethod);

            stepTransformationBinding.Regex.IsMatch("user xyz").Should().BeTrue();

            var invoker = new BindingInvoker(ConfigurationLoader.GetDefault(), Substitute.For<IErrorProvider>(), new BindingDelegateInvoker());
            var result = await invoker.InvokeBindingAsync(stepTransformationBinding, contextManagerStub, new object[] { "xyz" }, Substitute.For<ITestTracer>(), new DurationHolder());
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

            var invoker = new BindingInvoker(ConfigurationLoader.GetDefault(), Substitute.For<IErrorProvider>(), new BindingDelegateInvoker());
            var result  = await invoker.InvokeBindingAsync(stepTransformationBinding, contextManagerStub, new object[] { "xyz" }, Substitute.For<ITestTracer>(), new DurationHolder());
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

            var invoker = new BindingInvoker(ConfigurationLoader.GetDefault(), Substitute.For<IErrorProvider>(), new BindingDelegateInvoker());
            var result = await invoker.InvokeBindingAsync(stepTransformationBinding, contextManagerStub, new object[] { "xyz" }, Substitute.For<ITestTracer>(), new DurationHolder());
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

            var invoker = new BindingInvoker(ConfigurationLoader.GetDefault(), Substitute.For<IErrorProvider>(), new BindingDelegateInvoker());
            var result = await invoker.InvokeBindingAsync(stepTransformationBinding, contextManagerStub, new object[] { new Table("h1") }, Substitute.For<ITestTracer>(), new DurationHolder());
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
                .InvokeBindingAsync(stepTransformationBinding, Arg.Any<IContextManager>(), Arg.Any<object[]>(), Arg.Any<ITestTracer>(), Arg.Any<DurationHolder>())
                .Returns(resultUser);

            var stepArgumentTypeConverter = CreateStepArgumentTypeConverter();

            //TODO NSub fix - typeof(...) is not a IBindingType
            //var result = await stepArgumentTypeConverter.ConvertAsync("user xyz", typeof(User), new CultureInfo("en-US", false));
            //result.Should().Be(resultUser);
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
                .InvokeBindingAsync(stepTransformationBinding, Arg.Any<IContextManager>(), Arg.Any<object[]>(), Arg.Any<ITestTracer>(), Arg.Any<DurationHolder>())
                .Returns(resultUser);

            var stepArgumentTypeConverter = CreateStepArgumentTypeConverter();

            //TODO NSub fix - typeof(...) is not a IBindingType
            //var result = await stepArgumentTypeConverter.ConvertAsync("user xyz", typeof(User), new CultureInfo("en-US", false));
            //result.Should().Be(resultUser);
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
                .InvokeBindingAsync(stepTransformationBinding, Arg.Any<IContextManager>(), Arg.Any<object[]>(), Arg.Any<ITestTracer>(), Arg.Any<DurationHolder>())
                .Returns(resultUser);

            var stepArgumentTypeConverter = CreateStepArgumentTypeConverter();

            //TODO NSub fix - typeof(...) is not a IBindingType
            //var result = await stepArgumentTypeConverter.ConvertAsync("user xyz", typeof(User), new CultureInfo("en-US", false));
            //result.Should().Be(resultUser);
        }

        private StepArgumentTypeConverter CreateStepArgumentTypeConverter()
        {
            return new StepArgumentTypeConverter(Substitute.For<ITestTracer>(), bindingRegistryStub, contextManagerStub, methodBindingInvokerStub);
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
                .InvokeBindingAsync(stepTransformationBinding, Arg.Any<IContextManager>(), new object[] { table }, Arg.Any<ITestTracer>(), Arg.Any<DurationHolder>())
                .Returns(resultUsers);

            var stepArgumentTypeConverter = CreateStepArgumentTypeConverter();

            
            //TODO NSub fix - typeof(...) is not a IBindingType
            //var result = await stepArgumentTypeConverter.ConvertAsync(table, typeof(IEnumerable<User>), new CultureInfo("en-US", false));

            //result.Should().NotBeNull();
            //result.Should().Be(resultUsers);

        }
    }

}

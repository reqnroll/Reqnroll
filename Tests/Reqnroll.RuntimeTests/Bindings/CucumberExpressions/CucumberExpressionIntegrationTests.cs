using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Reqnroll.BoDi;
using FluentAssertions;
using Moq;
using Reqnroll.Bindings;
using Reqnroll.Bindings.Discovery;
using Reqnroll.Bindings.Reflection;
using Reqnroll.Infrastructure;
using Reqnroll.UnitTestProvider;
using Xunit;

namespace Reqnroll.RuntimeTests.Bindings.CucumberExpressions;

public enum SampleColorEnum
{
    Yellow,
    Brown
}

public class SampleUser
{
    public string UserName { get; }

    public SampleUser(string userName)
    {
        UserName = userName;
    }

    public static SampleUser Create(string userName) => new(userName);

    protected bool Equals(SampleUser other) => UserName == other.UserName;

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((SampleUser)obj);
    }

    public override int GetHashCode() => UserName.GetHashCode();
}

public record SampleComplexUser(string Firstname, string Surname, int Age, int Height)
{
    public static SampleComplexUser Create(string firstname, string surname, int age, int height) => new(firstname, surname, age, height);
}

public class CucumberExpressionIntegrationTests
{
    public class SampleBindings
    {
        public bool WasStepDefWithNoParamExecuted;
        public List<(object ParamValue, Type ParamType)> ExecutedParams = new();

        public void StepDefWithNoParam()
        {
            WasStepDefWithNoParamExecuted = true;
        }

        public void StepDefWithStringParam(string stringParam)
        {
            ExecutedParams.Add((stringParam, typeof(string)));
        }

        public void StepDefWithIntParam(int intParam)
        {
            ExecutedParams.Add((intParam, typeof(int)));
        }

        public void StepDefWithFloatParam(float doubleParam)
        {
            ExecutedParams.Add((doubleParam, typeof(float)));
        }

        public void StepDefWithDoubleParam(double doubleParam)
        {
            ExecutedParams.Add((doubleParam, typeof(double)));
        }

        public void StepDefWithDecimalParam(decimal decimalParam)
        {
            ExecutedParams.Add((decimalParam, typeof(decimal)));
        }

        public void StepDefWithEnumParam(SampleColorEnum enumParam)
        {
            ExecutedParams.Add((enumParam, typeof(SampleColorEnum)));
        }

        public void StepDefWithDateTimeParam(DateTime dateTimeParam)
        {
            ExecutedParams.Add((dateTimeParam, typeof(DateTime)));
        }

        public void StepDefWithDateOnlyParam(DateOnly dateParam)
        {
            ExecutedParams.Add((dateParam, typeof(DateOnly)));
        }

        public void StepDefWithTimeOnlyParam(TimeOnly timeParam)
        {
            ExecutedParams.Add((timeParam, typeof(TimeOnly)));
        }

        public void StepDefWithCustomClassParam(SampleUser userParam)
        {
            ExecutedParams.Add((userParam, typeof(SampleUser)));
        }

        public void StepDefWithCustomComplexClassParam(SampleComplexUser userParam)
        {
            ExecutedParams.Add((userParam, typeof(SampleComplexUser)));
        }

        public int ConvertFortyTwo()
        {
            return 42;
        }

        public string ConvertToStringLowercase(string str)
        {
            return str.ToLower();
        }

        public int ConvertToIntPlus100(int nr)
        {
            return nr + 100;
        }
    }

    public class TestDependencyProvider : DefaultDependencyProvider
    {
        public override void RegisterGlobalContainerDefaults(ObjectContainer container)
        {
            base.RegisterGlobalContainerDefaults(container);
            var stubUintTestProvider = new Mock<IUnitTestRuntimeProvider>();
            container.RegisterInstanceAs(stubUintTestProvider.Object, "nunit");
        }
    }

    private async Task<SampleBindings> PerformStepExecution(string methodName, string expression, string stepText, IStepArgumentTransformationBinding[] transformations = null, Action<IBindingRegistry> onBindingRegistryPreparation = null, string culture = "en-US")
    {
        var containerBuilder = new ContainerBuilder(new TestDependencyProvider());
        var globalContainer = containerBuilder.CreateGlobalContainer(GetType().Assembly);
        var testThreadContainer = containerBuilder.CreateTestThreadContainer(globalContainer);
        var engine = testThreadContainer.Resolve<ITestExecutionEngine>();

        var bindingSourceProcessor = globalContainer.Resolve<IRuntimeBindingSourceProcessor>();

        var bindingRegistry = globalContainer.Resolve<IBindingRegistry>();
        transformations?.ToList().ForEach(binding => bindingRegistry.RegisterStepArgumentTransformationBinding(binding));
        onBindingRegistryPreparation?.Invoke(bindingRegistry);

        var bindingSourceMethod = new BindingSourceMethod
        {
            BindingMethod = new RuntimeBindingMethod(typeof(SampleBindings).GetMethod(methodName)),
            IsPublic = true,
            Attributes = new[]
            {
                new BindingSourceAttribute
                {
                    AttributeType = new RuntimeBindingType(typeof(GivenAttribute)),
                    AttributeValues = new IBindingSourceAttributeValueProvider[]
                    {
                        new BindingSourceAttributeValueProvider(expression)
                    }
                }
            }
        };
        bindingSourceProcessor.ProcessType(
            new BindingSourceType
            {
                BindingType = new RuntimeBindingType(typeof(SampleBindings)),
                Attributes = new[]
                {
                    new BindingSourceAttribute
                    {
                        AttributeType = new RuntimeBindingType(typeof(BindingAttribute))
                    }
                },
                IsPublic = true,
                IsClass = true
            });
        bindingSourceProcessor.ProcessMethod(bindingSourceMethod);
        bindingSourceProcessor.BuildingCompleted();

        await engine.OnTestRunStartAsync();
        await engine.OnFeatureStartAsync(new FeatureInfo(CultureInfo.GetCultureInfo(culture), ".", "Sample feature", null, ProgrammingLanguage.CSharp));
        await engine.OnScenarioStartAsync();
        engine.OnScenarioInitialize(new ScenarioInfo("Sample scenario", null, null, null), null);
        await engine.StepAsync(StepDefinitionKeyword.Given, "Given ", stepText, null, null);

        var contextManager = testThreadContainer.Resolve<IContextManager>();
        contextManager.ScenarioContext.ScenarioExecutionStatus.Should().Be(ScenarioExecutionStatus.OK, $"should not fail with '{contextManager.ScenarioContext.TestError?.Message}'");

        return contextManager.ScenarioContext.ScenarioContainer.Resolve<SampleBindings>();
    }

    [Fact]
    public async Task Should_match_step_with_simple_cucumber_expression()
    {
        var expression = "there is something";
        var stepText = "there is something";
        var methodName = nameof(SampleBindings.StepDefWithNoParam);

        var sampleBindings = await PerformStepExecution(methodName, expression, stepText);

        sampleBindings.WasStepDefWithNoParamExecuted.Should().BeTrue();
    }

    [Fact]
    public async Task Should_match_step_with_parameterless_cucumber_expression()
    {
        var expression = "there is/are something(s) here \\/ now";
        var stepText = "there are something here / now";
        var methodName = nameof(SampleBindings.StepDefWithNoParam);

        var sampleBindings = await PerformStepExecution(methodName, expression, stepText);

        sampleBindings.WasStepDefWithNoParamExecuted.Should().BeTrue();
    }

    [Fact]
    public async Task Should_match_step_with_string_parameter_using_apostrophe()
    {
        var expression = "there is a user {string} registered";
        var stepText = "there is a user 'Marvin' registered";
        var expectedParam = ("Marvin", typeof(string));
        var methodName = nameof(SampleBindings.StepDefWithStringParam);

        var sampleBindings = await PerformStepExecution(methodName, expression, stepText);

        sampleBindings.ExecutedParams.Should().Contain(expectedParam);
    }

    [Fact]
    public async Task Should_match_step_with_string_parameter_using_quotes()
    {
        var expression = "there is a user {string} registered";
        var stepText = "there is a user \"Marvin\" registered";
        var expectedParam = ("Marvin", typeof(string));
        var methodName = nameof(SampleBindings.StepDefWithStringParam);

        var sampleBindings = await PerformStepExecution(methodName, expression, stepText);

        sampleBindings.ExecutedParams.Should().Contain(expectedParam);
    }

    [Fact]
    public async Task Should_match_step_with_word_parameter()
    {
        var expression = "there is a user {word} registered";
        var stepText = "there is a user Marvin registered";
        var expectedParam = ("Marvin", typeof(string));
        var methodName = nameof(SampleBindings.StepDefWithStringParam);

        var sampleBindings = await PerformStepExecution(methodName, expression, stepText);

        sampleBindings.ExecutedParams.Should().Contain(expectedParam);
    }

    [Fact]
    public async Task Should_match_step_with_int_parameter()
    {
        var expression = "I have {int} cucumbers in my belly";
        var stepText = "I have 42 cucumbers in my belly";
        var expectedParam = (42, typeof(int));
        var methodName = nameof(SampleBindings.StepDefWithIntParam);

        var sampleBindings = await PerformStepExecution(methodName, expression, stepText);

        sampleBindings.ExecutedParams.Should().Contain(expectedParam);
    }

    [Fact]
    public async Task Should_match_step_with_float_parameter()
    {
        var expression = "I have {float} cucumbers in my belly";
        var stepText = "I have 42.1 cucumbers in my belly";
        var expectedParam = ((float)42.1, typeof(float));
        var methodName = nameof(SampleBindings.StepDefWithFloatParam);

        var sampleBindings = await PerformStepExecution(methodName, expression, stepText);

        sampleBindings.ExecutedParams.Should().Contain(expectedParam);
    }

    [Fact]
    public async Task Should_match_step_with_float_parameter_to_double()
    {
        var expression = "I have {float} cucumbers in my belly";
        var stepText = "I have 42.1 cucumbers in my belly";
        var expectedParam = (42.1, typeof(double));
        var methodName = nameof(SampleBindings.StepDefWithDoubleParam);

        var sampleBindings = await PerformStepExecution(methodName, expression, stepText);

        sampleBindings.ExecutedParams.Should().Contain(expectedParam);
    }

    [Fact]
    public async Task Should_match_step_with_double_parameter()
    {
        var expression = "I have {double} cucumbers in my belly";
        var stepText = "I have 42.1 cucumbers in my belly";
        var expectedParam = (42.1, typeof(double));
        var methodName = nameof(SampleBindings.StepDefWithDoubleParam);

        var sampleBindings = await PerformStepExecution(methodName, expression, stepText);

        sampleBindings.ExecutedParams.Should().Contain(expectedParam);
    }

    [Fact]
    public async Task Should_match_step_with_decimal_parameter()
    {
        var expression = "I have {decimal} cucumbers in my belly";
        var stepText = "I have 42.1 cucumbers in my belly";
        var expectedParam = (42.1m, typeof(decimal));
        var methodName = nameof(SampleBindings.StepDefWithDecimalParam);

        var sampleBindings = await PerformStepExecution(methodName, expression, stepText);

        sampleBindings.ExecutedParams.Should().Contain(expectedParam);
    }

    [Fact]
    public async Task Should_match_step_with_float_parameter_to_decimal()
    {
        var expression = "I have {float} cucumbers in my belly";
        var stepText = "I have 42.1 cucumbers in my belly";
        var expectedParam = (42.1m, typeof(decimal));
        var methodName = nameof(SampleBindings.StepDefWithDecimalParam);

        var sampleBindings = await PerformStepExecution(methodName, expression, stepText);

        sampleBindings.ExecutedParams.Should().Contain(expectedParam);
    }

    [Fact]
    public async Task Should_match_step_with_joker_parameter_to_dateTime()
    {
        var expression = "The datetime of the system is {}";
        var stepText = "The datetime of the system is 2024-12-31 23:25";
        var expectedParam = (new DateTime(2024, 12, 31, 23, 25, 0), typeof(DateTime));
        var methodName = nameof(SampleBindings.StepDefWithDateTimeParam);

        var sampleBindings = await PerformStepExecution(methodName, expression, stepText);

        sampleBindings.ExecutedParams.Should().Contain(expectedParam);
    }

    [Fact]
    public async Task Should_match_step_with_joker_parameter_to_dateTime_dutch()
    {
        var expression = "The datetime of the system is {}";
        var stepText = "The datetime of the system is 31-12-2024 23:25";
        var expectedParam = (new DateTime(2024, 12, 31, 23, 25, 0), typeof(DateTime));
        var methodName = nameof(SampleBindings.StepDefWithDateTimeParam);

        var sampleBindings = await PerformStepExecution(methodName, expression, stepText, culture: "nl-NL");

        sampleBindings.ExecutedParams.Should().Contain(expectedParam);
    }

    [Fact]
    public async Task Should_match_step_with_joker_parameter_to_dateOnly()
    {
        var expression = "The date of the system is {}";
        var stepText = "The date of the system is 2024-12-31";
        var expectedParam = (new DateOnly(2024, 12, 31), typeof(DateOnly));
        var methodName = nameof(SampleBindings.StepDefWithDateOnlyParam);

        var sampleBindings = await PerformStepExecution(methodName, expression, stepText);

        sampleBindings.ExecutedParams.Should().Contain(expectedParam);
    }

    [Fact]
    public async Task Should_match_step_with_joker_parameter_to_timeOnly()
    {
        var expression = "The time of the system is {}";
        var stepText = "The time of the system is 23:25";
        var expectedParam = (new TimeOnly(23, 25, 0), typeof(TimeOnly));
        var methodName = nameof(SampleBindings.StepDefWithTimeOnlyParam);

        var sampleBindings = await PerformStepExecution(methodName, expression, stepText);

        sampleBindings.ExecutedParams.Should().Contain(expectedParam);
    }

    [Fact]
    public async Task Should_match_step_with_joker_parameter()
    {
        var expression = "there is a user {} registered";
        var stepText = "there is a user Marvin registered";
        var expectedParam = ("Marvin", typeof(string));
        var methodName = nameof(SampleBindings.StepDefWithStringParam);

        var sampleBindings = await PerformStepExecution(methodName, expression, stepText);

        sampleBindings.ExecutedParams.Should().Contain(expectedParam);
    }

    // build-in types supported by Reqnroll
    [Fact]
    public async Task Should_match_step_with_Int32_parameter()
    {
        var expression = "I have {Int32} cucumbers in my belly";
        var stepText = "I have 42 cucumbers in my belly";
        var expectedParam = (42, typeof(int));
        var methodName = nameof(SampleBindings.StepDefWithIntParam);

        var sampleBindings = await PerformStepExecution(methodName, expression, stepText);

        sampleBindings.ExecutedParams.Should().Contain(expectedParam);
    }

    // enum support
    [Fact]
    public async Task Should_match_step_with_enum_parameter()
    {
        var expression = "I have {SampleColorEnum} cucumbers in my belly";
        var stepText = "I have Yellow cucumbers in my belly";
        var expectedParam = (SampleColorEnum.Yellow, typeof(SampleColorEnum));
        var methodName = nameof(SampleBindings.StepDefWithEnumParam);

        var sampleBindings = await PerformStepExecution(methodName, expression, stepText);

        sampleBindings.ExecutedParams.Should().Contain(expectedParam);
    }

    // custom type conversion support

    [Fact]
    public async Task Should_match_step_with_custom_parameter_with_type_name()
    {
        var expression = "there is a {SampleUser} registered";
        var stepText = "there is a user Marvin registered";
        var expectedParam = (new SampleUser("Marvin"), typeof(SampleUser));
        var methodName = nameof(SampleBindings.StepDefWithCustomClassParam);
        IStepArgumentTransformationBinding transformation = new StepArgumentTransformationBinding(
            "user ([A-Z][a-z]+)",
            new RuntimeBindingMethod(typeof(SampleUser).GetMethod(nameof(SampleUser.Create))));

        var sampleBindings = await PerformStepExecution(methodName, expression, stepText, new[] { transformation });

        sampleBindings.ExecutedParams.Should().Contain(expectedParam);
    }


    [Fact]
    public async Task Should_match_step_with_custom_parameter_with_custom_name()
    {
        var expression = "there is a {user} registered";
        var stepText = "there is a user Marvin registered";
        var expectedParam = (new SampleUser("Marvin"), typeof(SampleUser));
        var methodName = nameof(SampleBindings.StepDefWithCustomClassParam);
        IStepArgumentTransformationBinding transformation = new StepArgumentTransformationBinding(
            "user ([A-Z][a-z]+)",
            new RuntimeBindingMethod(typeof(SampleUser).GetMethod(nameof(SampleUser.Create))),
            "user");

        var sampleBindings = await PerformStepExecution(methodName, expression, stepText, new[] { transformation });

        sampleBindings.ExecutedParams.Should().Contain(expectedParam);
    }

    [Fact]
    public async Task Should_match_step_with_customized_built_in_parameter_with_type_name()
    {
        var expression = "I have {Int32} cucumbers in my belly";
        var stepText = "I have forty two cucumbers in my belly";
        var expectedParam = (42, typeof(int));
        var methodName = nameof(SampleBindings.StepDefWithIntParam);
        IStepArgumentTransformationBinding transformation = new StepArgumentTransformationBinding(
            "forty two",
            new RuntimeBindingMethod(typeof(SampleBindings).GetMethod(nameof(SampleBindings.ConvertFortyTwo))));

        var sampleBindings = await PerformStepExecution(methodName, expression, stepText, new[] { transformation });

        sampleBindings.ExecutedParams.Should().Contain(expectedParam);
    }

    [Fact]
    public async Task Should_match_step_with_customized_built_in_parameter_with_simple_name()
    {
        var expression = "I have {int} cucumbers in my belly";
        var stepText = "I have forty two cucumbers in my belly";
        var expectedParam = (42, typeof(int));
        var methodName = nameof(SampleBindings.StepDefWithIntParam);
        IStepArgumentTransformationBinding transformation = new StepArgumentTransformationBinding(
            "forty two",
            new RuntimeBindingMethod(typeof(SampleBindings).GetMethod(nameof(SampleBindings.ConvertFortyTwo))));

        var sampleBindings = await PerformStepExecution(methodName, expression, stepText, new[] { transformation });

        sampleBindings.ExecutedParams.Should().Contain(expectedParam);
    }

    [Fact]
    public async Task Should_match_step_with_customized_built_in_parameter_without_recursion_string()
    {
        var expression = "there is a user {string} registered";
        var stepText = "there is a user 'Marvin' registered";
        var expectedParam = ("marvin", typeof(string));
        var methodName = nameof(SampleBindings.StepDefWithStringParam);

        IStepArgumentTransformationBinding transformation = new StepArgumentTransformationBinding(
            (string)null,
            new RuntimeBindingMethod(typeof(SampleBindings).GetMethod(nameof(SampleBindings.ConvertToStringLowercase))));

        var sampleBindings = await PerformStepExecution(methodName, expression, stepText, new[] { transformation });

        sampleBindings.ExecutedParams.Should().Contain(expectedParam);
    }

    [Fact]
    public async Task Should_match_step_with_customized_built_in_parameter_without_recursion_int32()
    {
        var expression = "I have {int} cucumbers in my belly";
        var stepText = "I have 43 cucumbers in my belly";
        var expectedParam = (143, typeof(int));
        var methodName = nameof(SampleBindings.StepDefWithIntParam);

        IStepArgumentTransformationBinding transformation = new StepArgumentTransformationBinding(
            (string)null,
            new RuntimeBindingMethod(typeof(SampleBindings).GetMethod(nameof(SampleBindings.ConvertToIntPlus100))));

        var sampleBindings = await PerformStepExecution(methodName, expression, stepText, new[] { transformation });

        sampleBindings.ExecutedParams.Should().Contain(expectedParam);
    }

    [Fact]
    public async Task Should_match_step_with_custom_parameter_with_additional_step_arguments()
    {
        var expression = "there is a {user} registered";
        var stepText = "there is a user Marvin Smith he is 27 years old and 175 height registered";
        var expectedParam = (new SampleComplexUser("marvin", "smith", 127, 275), typeof(SampleComplexUser));
        var methodName = nameof(SampleBindings.StepDefWithCustomComplexClassParam);

        IStepArgumentTransformationBinding transformationStringWithouthBlanks = new StepArgumentTransformationBinding(
            (string)null,
            new RuntimeBindingMethod(typeof(SampleBindings).GetMethod(nameof(SampleBindings.ConvertToStringLowercase))));

        IStepArgumentTransformationBinding transformationIntPlus100 = new StepArgumentTransformationBinding(
            (string)null,
            new RuntimeBindingMethod(typeof(SampleBindings).GetMethod(nameof(SampleBindings.ConvertToIntPlus100))));

        IStepArgumentTransformationBinding transformationSampleComplexUser = new StepArgumentTransformationBinding(
            "user ([A-Za-z]+) ([A-Za-z]+) he is ([0-9]+) years old and ([0-9]+) height",
            new RuntimeBindingMethod(typeof(SampleComplexUser).GetMethod(nameof(SampleComplexUser.Create))),
            "user");

        var sampleBindings = await PerformStepExecution(methodName, expression, stepText, new[] { transformationStringWithouthBlanks, transformationIntPlus100, transformationSampleComplexUser });

        sampleBindings.ExecutedParams.Should().Contain(expectedParam);
    }
}

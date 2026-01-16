using System;
using System.Linq;
using System.Reflection;
using Moq;
using Xunit;
using Reqnroll.Bindings;
using Reqnroll.Bindings.Discovery;
using Reqnroll.Bindings.Provider;
using Reqnroll.Configuration;
using Reqnroll.Infrastructure;

namespace Reqnroll.RuntimeTests
{
    
    public class MetadataLoadContextBindingRegistryBuilderTests : IDisposable
    {
        private BindingSourceProcessorStub bindingSourceProcessorStub;
        private MetadataLoadContextBindingRegistryBuilder lastBuilder;

        public MetadataLoadContextBindingRegistryBuilderTests()
        {
            bindingSourceProcessorStub = new BindingSourceProcessorStub();
        }

        private MetadataLoadContextBindingRegistryBuilder CreateSut()
        {
            lastBuilder = new MetadataLoadContextBindingRegistryBuilder(bindingSourceProcessorStub, new ReqnrollAttributesFilter(), new Mock<IBindingAssemblyLoader>().Object, ConfigurationLoader.GetDefault(), new Mock<IBindingProviderService>().Object);
            return lastBuilder;
        }

        [Fact]
        public void ShouldFindBinding_WithDefaultOrder()
        {
            var builder = CreateSut();

            BuildCompleteBindingFromType(builder, typeof(BindingRegistryBuilderTestFixtures.ScopedHookExample));

            Assert.Equal(4, bindingSourceProcessorStub.HookBindings.Count(s => s.HookOrder == 10000));
        }

        [Fact]
        public void ShouldFindBinding_WithSpecifiedPriorities()
        {
            var builder = CreateSut();

            BuildCompleteBindingFromType(builder, typeof(BindingRegistryBuilderTestFixtures.PrioritizedHookExample));

            Assert.Equal(1,
                bindingSourceProcessorStub.HookBindings.Count(
                    s =>
                        s.HookType == HookType.BeforeScenario && s.Method.Name == "OrderTenThousand" &&
                        s.HookOrder == 10000));
            Assert.Equal(1,
                bindingSourceProcessorStub.HookBindings.Count(
                    s =>
                        s.HookType == HookType.BeforeScenario && s.Method.Name == "OrderNineThousand" &&
                        s.HookOrder == 9000));
            Assert.Equal(1,
                bindingSourceProcessorStub.HookBindings.Count(
                    s =>
                        s.HookType == HookType.BeforeScenarioBlock && s.Method.Name == "OrderTenThousandAnd1" &&
                        s.HookOrder == 10001));
            Assert.Equal(1,
                bindingSourceProcessorStub.HookBindings.Count(
                    s =>
                        s.HookType == HookType.BeforeFeature && s.Method.Name == "OrderTenThousandAnd2" &&
                        s.HookOrder == 10002));
            Assert.Equal(1,
                bindingSourceProcessorStub.HookBindings.Count(
                    s =>
                        s.HookType == HookType.BeforeStep && s.Method.Name == "OrderTenThousandAnd3" &&
                        s.HookOrder == 10003));
            Assert.Equal(1,
                bindingSourceProcessorStub.HookBindings.Count(
                    s =>
                        s.HookType == HookType.BeforeTestRun && s.Method.Name == "OrderTenThousandAnd4" &&
                        s.HookOrder == 10004));

            Assert.Equal(1,
                bindingSourceProcessorStub.HookBindings.Count(
                    s =>
                        s.HookType == HookType.AfterScenario && s.Method.Name == "AfterOrderTenThousand" &&
                        s.HookOrder == 10000));
            Assert.Equal(1,
                bindingSourceProcessorStub.HookBindings.Count(
                    s =>
                        s.HookType == HookType.AfterScenario && s.Method.Name == "AfterOrderNineThousand" &&
                        s.HookOrder == 9000));
            Assert.Equal(1,
                bindingSourceProcessorStub.HookBindings.Count(
                    s =>
                        s.HookType == HookType.AfterScenarioBlock && s.Method.Name == "AfterOrderTenThousandAnd1" &&
                        s.HookOrder == 10001));
            Assert.Equal(1,
                bindingSourceProcessorStub.HookBindings.Count(
                    s =>
                        s.HookType == HookType.AfterFeature && s.Method.Name == "AfterOrderTenThousandAnd2" &&
                        s.HookOrder == 10002));
            Assert.Equal(1,
                bindingSourceProcessorStub.HookBindings.Count(
                    s =>
                        s.HookType == HookType.AfterStep && s.Method.Name == "AfterOrderTenThousandAnd3" &&
                        s.HookOrder == 10003));
            Assert.Equal(1,
                bindingSourceProcessorStub.HookBindings.Count(
                    s =>
                        s.HookType == HookType.AfterTestRun && s.Method.Name == "AfterOrderTenThousandAnd4" &&
                        s.HookOrder == 10004));
        }
        
         [Fact]
        public void ShouldFindStepArgumentTransformations_WithSpecifiedOrder()
        {
            var builder = CreateSut();

            BuildCompleteBindingFromType(builder, typeof(BindingRegistryBuilderTestFixtures.StepTransformationExample));

            Assert.Single(
                bindingSourceProcessorStub.StepArgumentTransformationBindings,
                sat =>
                    sat.Method.Name == nameof(BindingRegistryBuilderTestFixtures.StepTransformationExample.Transform) && sat.Order == StepArgumentTransformationAttribute.DefaultOrder);
            
            Assert.Single(
                bindingSourceProcessorStub.StepArgumentTransformationBindings,
                sat =>
                    sat.Method.Name == nameof(BindingRegistryBuilderTestFixtures.StepTransformationExample.TransformWithRegexAndOrder) && sat.Order == 5);
            
            Assert.Single(
                bindingSourceProcessorStub.StepArgumentTransformationBindings,
                sat =>
                    sat.Method.Name == nameof(BindingRegistryBuilderTestFixtures.StepTransformationExample.TransformWithOrderAndWithoutRegex) && sat.Order == 10);
        }

        [Fact]
        public void ShouldFindExampleConverter_FromType()
        {
            var builder = CreateSut();
            BuildCompleteBindingFromType(builder, typeof(BindingRegistryBuilderTestFixtures.StepTransformationExample));
            Assert.Equal(1,
                bindingSourceProcessorStub.StepArgumentTransformationBindings.Count(
                    s =>
                        s.Regex != null && s.Regex.Match("BindingRegistryTests").Success &&
                        s.Regex.Match("").Success == false));
        }

        [Fact]
        public void ShouldFindExampleConverter()
        {
            var builder = CreateSut();

            BuildCompleteBindingFromAssembly(builder);
            Assert.Equal(1,
                bindingSourceProcessorStub.StepArgumentTransformationBindings.Count(
                    s =>
                        s.Regex != null && s.Regex.Match("BindingRegistryTests").Success &&
                        s.Regex.Match("").Success == false));
        }

        private static void BuildCompleteBindingFromAssembly(MetadataLoadContextBindingRegistryBuilder builder)
        {
            builder.BuildBindingsFromAssembly(Assembly.GetExecutingAssembly());
            builder.BuildingCompleted();
        }

        private static void BuildCompleteBindingFromType(MetadataLoadContextBindingRegistryBuilder builder, Type type)
        {
            builder.BuildBindingsFromType(type);
            builder.BuildingCompleted();
        }

        [Fact]
        public void ShouldFindScopedExampleConverter()
        {
            var builder = CreateSut();

            BuildCompleteBindingFromAssembly(builder);

            Assert.Equal(2,
                         bindingSourceProcessorStub.StepDefinitionBindings.Count(
                             s =>
                                 s.StepDefinitionType == StepDefinitionType.Then &&
                                 s.Regex.Match("SpecificBindingRegistryTests").Success && s.IsScoped));
            Assert.Equal(0,
                bindingSourceProcessorStub.StepDefinitionBindings.Count(
                    s =>
                        s.StepDefinitionType == StepDefinitionType.Then &&
                        s.Regex.Match("SpecificBindingRegistryTests").Success && s.IsScoped == false));
        }

        [Fact]
        public void ShouldFindScopedHook_WithCtorArg()
        {
            var builder = CreateSut();

            BuildCompleteBindingFromAssembly(builder);

            Assert.Equal(1,
                bindingSourceProcessorStub.HookBindings.Count(s => s.Method.Name == "Tag2BeforeScenario" && s.IsScoped));
        }

        [Fact]
        public void ShouldFindScopedHook_WithMultipleCtorArg()
        {
            var builder = CreateSut();

            BuildCompleteBindingFromAssembly(builder);

            Assert.Equal(2,
                bindingSourceProcessorStub.HookBindings.Count(s => s.Method.Name == "Tag34BeforeScenario" && s.IsScoped));
        }

        [Fact]
        public void ShouldFindScopedHook_WithScopeAttribute()
        {
            var builder = CreateSut();

            BuildCompleteBindingFromAssembly(builder);

            Assert.Equal(1,
                bindingSourceProcessorStub.HookBindings.Count(s => s.Method.Name == "Tag1BeforeScenario" && s.IsScoped));
        }

        [Fact]
        public void ShouldFindStepDefinitionsWithStepDefinitionAttributes()
        {
            var builder = CreateSut();

            BuildCompleteBindingFromType(builder, typeof(BindingRegistryBuilderTestFixtures.BindingClassWithStepDefinitionAttributes));

            Assert.Equal(3, bindingSourceProcessorStub.StepDefinitionBindings.Count);
            Assert.Equal(1, bindingSourceProcessorStub.StepDefinitionBindings.Count(b => b.StepDefinitionType == StepDefinitionType.Given));
            Assert.Equal(1, bindingSourceProcessorStub.StepDefinitionBindings.Count(b => b.StepDefinitionType == StepDefinitionType.When));
            Assert.Equal(1, bindingSourceProcessorStub.StepDefinitionBindings.Count(b => b.StepDefinitionType == StepDefinitionType.Then));
        }

        [Fact]
        public void ShouldFindStepDefinitionsWithTranslatedAttributes()
        {
            var builder = CreateSut();

            BuildCompleteBindingFromType(builder, typeof(BindingRegistryBuilderTestFixtures.BindingClassWithTranslatedStepDefinitionAttributes));

            Assert.Equal(3, bindingSourceProcessorStub.StepDefinitionBindings.Count);
            Assert.Equal(1, bindingSourceProcessorStub.StepDefinitionBindings.Count(b => b.StepDefinitionType == StepDefinitionType.Given));
            Assert.Equal(1, bindingSourceProcessorStub.StepDefinitionBindings.Count(b => b.StepDefinitionType == StepDefinitionType.When));
            Assert.Equal(1, bindingSourceProcessorStub.StepDefinitionBindings.Count(b => b.StepDefinitionType == StepDefinitionType.Then));
        }

        [Fact]
        public void ShouldFindStepDefinitionsWithCustomAttribute()
        {
            var builder = CreateSut();

            BuildCompleteBindingFromType(builder, typeof(BindingRegistryBuilderTestFixtures.BindingClassWithCustomStepDefinitionAttribute));

            Assert.Equal(2, bindingSourceProcessorStub.StepDefinitionBindings.Count);
            Assert.Equal(1, bindingSourceProcessorStub.StepDefinitionBindings.Count(b => b.StepDefinitionType == StepDefinitionType.Given));
            Assert.Equal(1, bindingSourceProcessorStub.StepDefinitionBindings.Count(b => b.StepDefinitionType == StepDefinitionType.When));
            Assert.Equal(0, bindingSourceProcessorStub.StepDefinitionBindings.Count(b => b.StepDefinitionType == StepDefinitionType.Then));
        }

        public void Dispose()
        {
            lastBuilder?.Dispose();
        }
    }
}

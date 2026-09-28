using FluentAssertions;
using Reqnroll.Bindings.Discovery;
using Reqnroll.RuntimeTests.Bindings.Discovery.TestSubjects;
using System;
using System.Collections.Generic;
using Xunit;

namespace Reqnroll.RuntimeTests.Bindings.Discovery;

public class BindingSourceProcessorTestsWithInheritedBindingClasses
{
    private BindingSourceProcessorStub CreateBindingSourceProcessor()
    {
        //NOTE: BindingSourceProcessor is abstract, to test its base functionality we need to instantiate a subclass
        return new BindingSourceProcessorStub();
    }

    [Theory]
    [InlineData(typeof(TestSubjects.DerivedBoundClassBoundMethods_InheritsFromBaseClassNoBindingNoBoundMethods), true, 1, new string[] { })]
    [InlineData(typeof(TestSubjects.DerivedBoundClassBoundMethods_InheritsFromBaseClassNoBindingWithBoundMethods), true, 2, new string[] { })]
    [InlineData(typeof(TestSubjects.DerivedBoundClassBoundMethods_InheritsFromBaseClassWithBindingNoBoundMethods), false, 0, new string[] { "Binding types cannot inherit from other binding types" })]
    [InlineData(typeof(TestSubjects.DerivedBoundClassBoundMethods_InheritsFromBaseClassWithBindingWithBoundMethods), false, 0, new string[] { "Binding types cannot inherit from other binding types" })]
    [InlineData(typeof(TestSubjects.DerivedBoundClassBoundMethodswithBindingaAttributes_InheritsFromBaseClassNoBindingNoBoundMethod), true, 1, new string[] { })]
    [InlineData(typeof(TestSubjects.DerivedBoundClassBoundMethodswithBindingaAttributes_InheritsFromBaseClassNoBindingBoundMethod), true, 1, new string[] { })]
    [InlineData(typeof(TestSubjects.DerivedBoundClassBoundMethodswithBindingaAttributes_InheritsFromBaseClassWithBindingNoBoundMethod), false, 0, new string[] { "Binding types cannot inherit from other binding types" })]
    [InlineData(typeof(TestSubjects.DerivedBoundClassBoundMethodswithBindingaAttributes_InheritsFromBaseClassWithBindingBoundMethod), false, 0, new string[] { "Binding types cannot inherit from other binding types" })]
    [InlineData(typeof(TestSubjects.DerivedBoundClassOverriddenMethodsWithoutBindingAttributes_InheritsFromBaseClassNoBindingBoundMethods), true, 1, new string[] { })]
    [InlineData(typeof(TestSubjects.DerivedBoundClassOverriddenMethodsWithoutBindingAttributes_InheritsFromBaseClassWithBindingBoundMethods), false, 0, new string[] { "Binding types cannot inherit from other binding types" })]
    [InlineData(typeof(TestSubjects.DerivedUnBoundClassNoBoundMethods_InheritsFromBaseClassNoBindingNoBoundMethods), false, 0, new string[] { })]
    [InlineData(typeof(TestSubjects.DerivedUnBoundClassNoBoundMethods_InheritsFromBaseClassNoBindingWithBoundMethods), false, 0, new string[] { })]
    [InlineData(typeof(TestSubjects.DerivedUnBoundClassNoBoundMethods_InheritsFromBaseClassWithBindingNoBoundMethods), true, 0, new string[] { })]
    [InlineData(typeof(TestSubjects.DerivedUnBoundClassNoBoundMethods_InheritsFromBaseClassWithBindingWithBoundMethods), true, 1, new string[] { })]
    [InlineData(typeof(TestSubjects.DerivedUnboundClassWithBoundMethods_InheritsFromBaseClassNoBindingNoBoundMethods), false, 0, new string[] { })]
    [InlineData(typeof(TestSubjects.DerivedUnboundClassWithBoundMethods_InheritsFromBaseClassNoBindingWithBoundMethods), false, 0, new string[] { })]
    [InlineData(typeof(TestSubjects.DerivedUnboundClassWithBoundMethods_InheritsFromBaseClassWithBindingNoBoundMethods), true, 1, new string[] { })]
    [InlineData(typeof(TestSubjects.DerivedUnboundClassWithBoundMethods_InheritsFromBaseClassWithBindingWithBoundMethods), true, 2, new string[] { })]
    [InlineData(typeof(TestSubjects.DerivedUnBoundClassOveriddenMethodsWithBindingAttributes_InheritsFromBaseClassNoBindingNoBoundMethods), false, 0, new string[] { })]
    [InlineData(typeof(TestSubjects.DerivedUnBoundClassOveriddenMethodsWithBindingAttributes_InheritsFromBaseClassNoBindingWithBoundMethods), false, 0, new string[] { })]
    [InlineData(typeof(TestSubjects.DerivedUnBoundClassOveriddenMethodsWithBindingAttributes_InheritsFromBaseClassWithBindingNoBoundMethods), true, 1, new string[] { })]
    [InlineData(typeof(TestSubjects.DerivedUnBoundClassOveriddenMethodsWithBindingAttributes_InheritsFromBaseClassWithBindingWithBoundMethods), true, 1, new string[] { })]
    [InlineData(typeof(TestSubjects.DerivedUnBoundClassOverriddenMethodsWithoutBindingAttributes_InheritsFromBaseClassNoBindingNoBoundMethods), false, 0, new string[] { })]
    [InlineData(typeof(TestSubjects.DerivedUnBoundClassOverriddenMethodsWithoutBindingAttributes_InheritsFromBaseClassNoBindingWithBoundMethods), false, 0, new string[] { })]
    [InlineData(typeof(TestSubjects.DerivedUnBoundClassOverriddenMethodsWithoutBindingAttributes_InheritsFromBaseClassWithBindingNoBoundMethods), true, 0, new string[] { })]
    [InlineData(typeof(TestSubjects.DerivedUnBoundClassOverriddenMethodsWithoutBindingAttributes_InheritsFromBaseClassWithBindingWithBoundMethods), true, 1, new string[] { })]
    public void ProcessType_WithInheritedBindingClasses_ShouldFindBoundMethods(Type bindingClassType, bool outcome, int expectedBoundMethodCount, IEnumerable<string> expectedErrors)
    {
        //ARRANGE
        var sut = CreateBindingSourceProcessor();
        var builder = new RuntimeBindingRegistryBuilder(sut, new ReqnrollAttributesFilter(), null, null, null);
        //ACT
        builder.BuildBindingsFromType(bindingClassType).Should().Be(outcome);
        sut.BuildingCompleted();
        //ASSERT
        sut.StepDefinitionBindings.Should().HaveCount(expectedBoundMethodCount);

        foreach (var expectedError in expectedErrors)
        {
            sut.ValidationErrors.Should().Contain(m => m.Contains(expectedError));
        }
    }



}
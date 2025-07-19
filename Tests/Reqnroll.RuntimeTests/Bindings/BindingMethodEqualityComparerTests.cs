using System.Reflection;
using FluentAssertions;
using Reqnroll.Bindings.Reflection;
using Xunit;

namespace Reqnroll.RuntimeTests.Bindings
{
    public class BindingMethodEqualityComparerTests
    {
        private static IBindingMethod CreateBindingMethod(MethodInfo methodInfo)
            => new RuntimeBindingMethod(methodInfo);

        [Fact]
        public void Equals_ShouldReturnTrue_ForSameMethodInfo()
        {
            var methodInfo = typeof(SampleClass).GetMethod(nameof(SampleClass.MethodA));
            var bindingMethod1 = CreateBindingMethod(methodInfo);
            var bindingMethod2 = CreateBindingMethod(methodInfo);
            var comparer = new BindingMethodEqualityComparer();

            comparer.Equals(bindingMethod1, bindingMethod2).Should().BeTrue();
        }

        [Fact]
        public void Equals_ShouldReturnFalse_ForDifferentMethodInfos()
        {
            var methodInfo1 = typeof(SampleClass).GetMethod(nameof(SampleClass.MethodA));
            var methodInfo2 = typeof(SampleClass).GetMethod(nameof(SampleClass.MethodB));
            var bindingMethod1 = CreateBindingMethod(methodInfo1);
            var bindingMethod2 = CreateBindingMethod(methodInfo2);
            var comparer = new BindingMethodEqualityComparer();

            comparer.Equals(bindingMethod1, bindingMethod2).Should().BeFalse();
        }

        [Fact]
        public void Equals_ShouldReturnTrue_ForBaseAndDerivedReflectedMethod()
        {
            var baseMethod = typeof(BaseClass).GetMethod(nameof(BaseClass.SharedMethod));
            var derivedMethod = typeof(DerivedClass).GetMethod(nameof(BaseClass.SharedMethod));
            var comparer = new BindingMethodEqualityComparer();

            baseMethod.Should().NotBeNull();
            derivedMethod.Should().NotBeNull();
            baseMethod.Equals(derivedMethod).Should().BeFalse("MethodInfo.Equals is false for base/derived reflection");

            var bindingMethod1 = CreateBindingMethod(baseMethod);
            var bindingMethod2 = CreateBindingMethod(derivedMethod);

            comparer.Equals(bindingMethod1, bindingMethod2).Should().BeTrue("comparer should treat base/derived reflected method as equal");
        }

        [Fact]
        public void Equals_ShouldReturnFalse_ForOverriddenMethodInDerived()
        {
            var baseMethod = typeof(BaseWithVirtual).GetMethod(nameof(BaseWithVirtual.VirtualMethod));
            var derivedMethod = typeof(DerivedWithOverride).GetMethod(nameof(BaseWithVirtual.VirtualMethod));
            var comparer = new BindingMethodEqualityComparer();

            baseMethod.Should().NotBeNull();
            derivedMethod.Should().NotBeNull();
            baseMethod.Equals(derivedMethod).Should().BeFalse("MethodInfo.Equals is false for base/derived override");

            var bindingMethod1 = CreateBindingMethod(baseMethod);
            var bindingMethod2 = CreateBindingMethod(derivedMethod);

            comparer.Equals(bindingMethod1, bindingMethod2).Should().BeFalse("comparer should treat base/derived override as not equal");
        }

        [Fact]
        public void GetHashCode_ShouldBeEqual_ForEquivalentMethods()
        {
            var methodInfo = typeof(SampleClass).GetMethod(nameof(SampleClass.MethodA));
            var bindingMethod1 = CreateBindingMethod(methodInfo);
            var bindingMethod2 = CreateBindingMethod(methodInfo);
            var comparer = new BindingMethodEqualityComparer();

            comparer.GetHashCode(bindingMethod1).Should().Be(comparer.GetHashCode(bindingMethod2));
        }

        [Fact]
        public void GetHashCode_ShouldDiffer_ForDifferentMethods()
        {
            var methodInfo1 = typeof(SampleClass).GetMethod(nameof(SampleClass.MethodA));
            var methodInfo2 = typeof(SampleClass).GetMethod(nameof(SampleClass.MethodB));
            var bindingMethod1 = CreateBindingMethod(methodInfo1);
            var bindingMethod2 = CreateBindingMethod(methodInfo2);
            var comparer = new BindingMethodEqualityComparer();

            comparer.GetHashCode(bindingMethod1).Should().NotBe(comparer.GetHashCode(bindingMethod2));
        }

        [Fact]
        public void GetHashCode_ShouldBeEqual_ForBaseAndDerivedReflectedMethod()
        {
            var baseMethod = typeof(BaseClass).GetMethod(nameof(BaseClass.SharedMethod));
            var derivedMethod = typeof(DerivedClass).GetMethod(nameof(BaseClass.SharedMethod));
            var comparer = new BindingMethodEqualityComparer();

            var bindingMethod1 = CreateBindingMethod(baseMethod);
            var bindingMethod2 = CreateBindingMethod(derivedMethod);

            comparer.GetHashCode(bindingMethod1).Should().Be(comparer.GetHashCode(bindingMethod2));
        }

        // Helper classes for testing
        private class SampleClass
        {
            public void MethodA() { }
            public void MethodB() { }
        }

        private class BaseClass
        {
            public void SharedMethod() { }
        }

        private class DerivedClass : BaseClass
        {
            // Inherits SharedMethod as-is
        }

        private class BaseWithVirtual
        {
            public virtual void VirtualMethod() { }
        }

        private class DerivedWithOverride : BaseWithVirtual
        {
            public override void VirtualMethod() { }
        }
    }
}
using System;
using FluentAssertions;
using Reqnroll.BoDi;
using Xunit;

namespace Reqnroll.RuntimeTests.BoDi
{
    public class RegisterFactoryDelegateTests
    {
        [Fact]
        public void ShouldBeAbleToRegisterAFactoryDelegate()
        {
            // given
            var container = new ObjectContainer();
            container.RegisterFactoryAs<IInterface1>(() => new VerySimpleClass());

            // when
            var obj = container.Resolve<IInterface1>();

            // then
            obj.Should().NotBeNull();
            obj.Should().BeOfType<VerySimpleClass>();
        }

        [Fact]
        public void ShouldReturnTheSameIfResolvedTwice()
        {
            // given
            var container = new ObjectContainer();
            container.RegisterFactoryAs<IInterface1>(() => new VerySimpleClass());

            // when
            var obj1 = container.Resolve<IInterface1>();
            var obj2 = container.Resolve<IInterface1>();

            // then
            obj1.Should().BeSameAs(obj2);
        }

        [Fact]
        public void ShouldBeAbleToRegisterAFactoryDelegateWithDependencies()
        {
            // given
            var container = new ObjectContainer();
            var dependency = new VerySimpleClass();
            container.RegisterInstanceAs<IInterface1>(dependency);
            container.RegisterFactoryAs<IInterface3>(new Func<IInterface1, IInterface3>(if1 => new ClassWithSimpleDependency(if1)));

            // when
            var obj = container.Resolve<IInterface3>();

            // then
            obj.Should().NotBeNull();
            obj.Should().BeOfType<ClassWithSimpleDependency>();
            dependency.Should().BeSameAs(((ClassWithSimpleDependency)obj).Dependency);
        }

        [Fact]
        public void ShouldBeAbleToRegisterAFactoryDelegateWithDependencyToTheContainer()
        {
            // given
            var container = new ObjectContainer();
            var dependency = new VerySimpleClass();
            container.RegisterInstanceAs<IInterface1>(dependency);
            container.RegisterFactoryAs<IInterface3>(c => new ClassWithSimpleDependency(c.Resolve<IInterface1>()));

            // when
            var obj = container.Resolve<IInterface3>();

            // then
            obj.Should().NotBeNull();
            obj.Should().BeOfType<ClassWithSimpleDependency>();
            dependency.Should().BeSameAs(((ClassWithSimpleDependency)obj).Dependency);
        }

        [Fact(Skip = "dynamic circles not detected yet, this leads to stack overflow")]
        public void ShouldThrowExceptionForDynamicCircularDependencies()
        {
            // given
            var container = new ObjectContainer();
            container.RegisterFactoryAs<ClassWithCircularDependency1>(c => new ClassWithCircularDependency1(c.Resolve<ClassWithCircularDependency2>()));

            // when 
            Action act = () => container.Resolve<ClassWithCircularDependency1>();
            act.Should().ThrowExactly<ObjectContainerException>("Circular dependency");
        }

        [Fact]
        public void ShouldThrowExceptionForStaticCircularDependencies()
        {
            // given
            var container = new ObjectContainer();
            container.RegisterFactoryAs<ClassWithCircularDependency1>(new Func<ClassWithCircularDependency2, ClassWithCircularDependency1>(dep1 => new ClassWithCircularDependency1(dep1)));

            // when 
            Action act = () => container.Resolve<ClassWithCircularDependency1>();
            act.Should().ThrowExactly<ObjectContainerException>("Circular dependency");
        }

        [Fact]
        public void ShouldThrowExceptionForStaticCircularDependenciesWithMultipleFactoriesInPath()
        {
            // given
            var container = new ObjectContainer();
            container.RegisterFactoryAs<ClassWithCircularDependency1>(new Func<ClassWithCircularDependency2, ClassWithCircularDependency1>(dep2 => new ClassWithCircularDependency1(dep2)));
            container.RegisterFactoryAs<ClassWithCircularDependency2>(new Func<ClassWithCircularDependency1, ClassWithCircularDependency2>(dep1 => new ClassWithCircularDependency2(dep1)));

            // when 
            Action act = () => container.Resolve<ClassWithCircularDependency1>();
            act.Should().ThrowExactly<ObjectContainerException>("Circular dependency");
        }

        [Fact]
        public void ShouldAlwaysCreateInstanceOnPerRequestStrategy()
        {
            // given
            var container = new ObjectContainer();

            // when 
            container.RegisterFactoryAs<IInterface1>(() => new SimpleClassWithDefaultCtor()).InstancePerDependency();

            // then
            var obj1 = (SimpleClassWithDefaultCtor)container.Resolve<IInterface1>();
            var obj2 = (SimpleClassWithDefaultCtor)container.Resolve<IInterface1>();
            obj1.Should().NotBeSameAs(obj2);
        }

        [Fact]
        public void ShouldAlwaysCreateSameObjectOnPerContextStrategy()
        {
            // given
            var container = new ObjectContainer();

            // when 
            container.RegisterFactoryAs<IInterface1>(() => new SimpleClassWithDefaultCtor()).InstancePerContext();

            // then
            var obj1 = (SimpleClassWithDefaultCtor)container.Resolve<IInterface1>();
            var obj2 = (SimpleClassWithDefaultCtor)container.Resolve<IInterface1>();
            obj1.Should().BeSameAs(obj2);
        }
    }
}

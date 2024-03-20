using System;
using FluentAssertions;
using Reqnroll.BoDi;
using Xunit;

namespace Reqnroll.RuntimeTests.BoDi
{
    public class ResolveTests
    {
        [Fact]
        public void ShouldResolveVerySimpleClass()
        {
            // given
            var container = new ObjectContainer();
            container.RegisterTypeAs<VerySimpleClass, IInterface1>();

            // when
            var obj = container.Resolve<IInterface1>();

            // then
            obj.Should().NotBeNull();
            obj.Should().BeOfType<VerySimpleClass>();
        }

        [Fact]
        public void ShouldNotBeAbleToResolveStructsWithoutCtor()
        {
            // given
            var container = new ObjectContainer();

            // when
            Action act = () => container.Resolve<MyStructWithoutCtor>();
            act.Should().ThrowExactly<ObjectContainerException>();
        }

        [Fact]
        public void ShouldNotBeAbleToResolveStructsWithCtor()
        {
            // given
            var container = new ObjectContainer();

            // when
            Action act = () => container.Resolve<MyStructWithDependencies>();
            act.Should().ThrowExactly<ObjectContainerException>();
        }

        [Fact]
        public void ShouldResolveRegisteredInstance()
        {
            // given
            var instance = new VerySimpleClass();
            var container = new ObjectContainer();
            container.RegisterInstanceAs<IInterface1>(instance);

            // when
            var obj = container.Resolve<IInterface1>();

            // then
            obj.Should().BeSameAs(instance);
        }

        [Fact]
        public void ShouldResolveSimpleClassWithCtor()
        {
            // given
            var container = new ObjectContainer();
            container.RegisterTypeAs<SimpleClassWithDefaultCtor, IInterface1>();

            // when
            var obj = container.Resolve<IInterface1>();

            // then
            obj.Should().NotBeNull();
            obj.Should().BeOfType<SimpleClassWithDefaultCtor>();
            ((SimpleClassWithDefaultCtor)obj).Status.Should().Be("Initialized");
        }

        [Fact]
        public void ShouldResolveSimpleClassWithInternalCtor()
        {
            // given
            var container = new ObjectContainer();
            container.RegisterTypeAs<SimpleClassWithInternalCtor, IInterface1>();

            // when
            var obj = container.Resolve<IInterface1>();

            // then
            obj.Should().NotBeNull();
            obj.Should().BeOfType<SimpleClassWithInternalCtor>();
            ((SimpleClassWithInternalCtor)obj).Status.Should().Be("Initialized");
        }

        [Fact]
        public void ShouldReturnTheSameIfResolvedTwice()
        {
            // given
            var container = new ObjectContainer();
            container.RegisterTypeAs<VerySimpleClass, IInterface1>();

            // when
            var obj1 = container.Resolve<IInterface1>();
            var obj2 = container.Resolve<IInterface1>();

            // then
            obj1.Should().BeSameAs(obj2);
        }

        [Fact]
        public void ShouldResolveClassesWithoutRegstration()
        {
            // given
            var container = new ObjectContainer();

            // when
            var obj = container.Resolve<VerySimpleClass>();

            // then
            obj.Should().NotBeNull();
            obj.Should().BeOfType<VerySimpleClass>();
        }

        [Fact]
        public void ShouldThrowErrorIfInterfaceCannotBeResolved()
        {
            // given
            var container = new ObjectContainer();

            // when
            Action act = () => container.Resolve<IInterface1>();
            act.Should().ThrowExactly<ObjectContainerException>();
        }

        [Fact]
        public void ShouldResolveClassWithSimpleDependency()
        {
            // given
            var dependency = new VerySimpleClass();
            var container = new ObjectContainer();
            container.RegisterTypeAs<ClassWithSimpleDependency, IInterface3>();
            container.RegisterInstanceAs<IInterface1>(dependency);

            // when
            var obj = container.Resolve<IInterface3>();

            // then
            obj.Should().NotBeNull();
            obj.Should().BeOfType<ClassWithSimpleDependency>();
            ((ClassWithSimpleDependency)obj).Dependency.Should().BeSameAs(dependency);
        }

        [Fact]
        public void ShouldResolveClassWithSimpleDependencies()
        {
            // given
            var dependency1 = new VerySimpleClass();
            var dependency2 = new AnotherVerySimpleClass();
            var container = new ObjectContainer();
            container.RegisterTypeAs<ClassWithSimpleDependencies, IInterface3>();
            container.RegisterInstanceAs<IInterface1>(dependency1);
            container.RegisterInstanceAs<IInterface2>(dependency2);

            // when
            var obj = container.Resolve<IInterface3>();

            // then
            obj.Should().NotBeNull();
            obj.Should().BeOfType<ClassWithSimpleDependencies>();
            ((ClassWithSimpleDependencies)obj).Dependency1.Should().BeSameAs(dependency1);
            ((ClassWithSimpleDependencies)obj).Dependency2.Should().BeSameAs(dependency2);
        }

        [Fact]
        public void ShouldResolveClassWithDeeperDependencies()
        {
            // given
            var dependency1 = new VerySimpleClass();
            var container = new ObjectContainer();
            container.RegisterTypeAs<ClassWithDeeperDependency, IInterface4>();
            container.RegisterInstanceAs<IInterface1>(dependency1);
            container.RegisterTypeAs<ClassWithSimpleDependency, IInterface3>();

            // when
            var obj = container.Resolve<IInterface4>();
            var dependency2 = container.Resolve<IInterface3>();

            // then
            obj.Should().NotBeNull();
            obj.Should().BeOfType<ClassWithDeeperDependency>();
            ((ClassWithDeeperDependency)obj).Dependency.Should().BeSameAs(dependency2);
            ((ClassWithSimpleDependency)((ClassWithDeeperDependency)obj).Dependency).Dependency.Should().BeSameAs(dependency1);
        }

        [Fact]
        public void ShouldResolveClassWithDeeperRedundantDependencies()
        {
            // given
            var dependency1 = new VerySimpleClass();
            var container = new ObjectContainer();
            container.RegisterTypeAs<ClassWithDeeperRedundantDependencies, IInterface4>();
            container.RegisterInstanceAs<IInterface1>(dependency1);
            container.RegisterTypeAs<ClassWithSimpleDependency, IInterface3>();

            // when
            var obj = container.Resolve<IInterface4>();
            var dependency2 = container.Resolve<IInterface3>();

            // then
            obj.Should().NotBeNull();
            obj.Should().BeOfType<ClassWithDeeperRedundantDependencies>();
            ((ClassWithDeeperRedundantDependencies)obj).Dependency1.Should().BeSameAs(dependency2);
            ((ClassWithDeeperRedundantDependencies)obj).Dependency2.Should().BeSameAs(dependency1);
            ((ClassWithSimpleDependency)((ClassWithDeeperRedundantDependencies)obj).Dependency1).Dependency.Should().BeSameAs(dependency1);
        }

        [Fact]
        public void ShouldResolveSameInstanceWhenTypeIsRegisteredAsTwoInterface()
        {
            // given
            var container = new ObjectContainer();
            container.RegisterTypeAs<ClassWithTwoInterface, IInterface1>();
            container.RegisterTypeAs<ClassWithTwoInterface, IInterface2>();

            // when
            var obj1 = container.Resolve<IInterface1>();
            var obj2 = container.Resolve<IInterface2>();

            // then
            obj1.Should().BeSameAs(obj2);
        }

        [Fact]
        public void ShouldResolveRegisteredInstanceIfItsTypeIsAlsoRegistered()
        {
            // given
            var obj1 = new ClassWithTwoInterface();
            var container = new ObjectContainer();
            container.RegisterInstanceAs<IInterface1>(obj1);
            container.RegisterTypeAs<ClassWithTwoInterface, IInterface2>();

            // when
            var obj2 = container.Resolve<IInterface2>();

            // then
            obj1.Should().BeSameAs(obj2);
        }

        [Fact]
        public void ShouldResolveTheContainerItself()
        {
            // given
            var container = new ObjectContainer();

            // when 
            var obj = container.Resolve<ObjectContainer>();

            // then
            obj.Should().NotBeNull();
            obj.Should().BeSameAs(container);
        }

        [Fact]
        public void ShouldResolveTheContainerItselfAsInterface()
        {
            // given
            var container = new ObjectContainer();

            // when 
            var obj = container.Resolve<IObjectContainer>();

            // then
            obj.Should().NotBeNull();
            obj.Should().BeSameAs(container);
        }

        [Fact]
        public void ShouldNotBeAbleToResolveStringTypes()
        {
            // given
            var container = new ObjectContainer();

            // when 
            Action act = () => container.Resolve<string>();
            act.Should().ThrowExactly<ObjectContainerException>("Primitive types or structs cannot be resolved");
        }

        [Fact]
        public void ShouldNotBeAbleToResolvePrimitiveTypes()
        {
            // given
            var container = new ObjectContainer();

            // when 
            Action act = () => container.Resolve<int>();
            act.Should().ThrowExactly<ObjectContainerException>("Primitive types or structs cannot be resolved");
        }

        [Fact]
        public void ShouldThrowExceptionForCircularDependencies()
        {
            // given
            var container = new ObjectContainer();

            // when 
            Action act = () => container.Resolve<ClassWithCircularDependency1>();
            act.Should().ThrowExactly<ObjectContainerException>("Circular dependency");
        }

        [Fact]
        public void ShouldBeAbleToResolveStaticCirclesWhenNamedRegistrationsAreUsed()
        {
            // given
            var container = new ObjectContainer();
            container.RegisterTypeAs<ClassWithCircularDependencyThroughInterfaces1, IInterface1>("a_name");
            container.RegisterTypeAs<ClassWithCircularDependencyThroughInterfaces2, IInterface2>();
            container.RegisterTypeAs<VerySimpleClass, IInterface1>();

            // when 
            var result = container.Resolve<IInterface1>("a_name");

            // then
            result.Should().NotBeNull();
            result.Should().BeOfType<ClassWithCircularDependencyThroughInterfaces1>();
        }

        [Fact]
        public void ShouldThrowExceptionForMultipleConstructorsWithSameNumberOfMaximumParameters()
        {
            // given
            var container = new ObjectContainer();

            // when 
            Action act = () => container.Resolve<ClassWithTwoConstructorSameParamCount>();
            act.Should().ThrowExactly<ObjectContainerException>("Multiple public constructors");
        }
    }
}

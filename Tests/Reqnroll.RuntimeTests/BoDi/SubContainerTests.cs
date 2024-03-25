using System;
using FluentAssertions;
using Moq;
using Reqnroll.BoDi;
using Xunit;

namespace Reqnroll.RuntimeTests.BoDi
{
    public class SubContainerTests
    {
        [Fact]
        public void ShouldBeAbleToResolveFromBaseContainer()
        {
            // given
            var baseContainer = new ObjectContainer();
            baseContainer.RegisterTypeAs<VerySimpleClass, IInterface1>();
            var container = new ObjectContainer(baseContainer);

            // when
            var obj = container.Resolve<IInterface1>();

            // then
            obj.Should().NotBeNull();
            obj.Should().BeOfType<VerySimpleClass>();
        }

        [Fact]
        public void ShouldBeAbleToResolveFromChildContainer()
        {
            // given
            var baseContainer = new ObjectContainer();
            var container = new ObjectContainer(baseContainer);
            container.RegisterTypeAs<VerySimpleClass, IInterface1>();

            // when
            var obj = container.Resolve<IInterface1>();

            // then
            obj.Should().NotBeNull();
            obj.Should().BeOfType<VerySimpleClass>();
        }

        [Fact]
        public void ShouldResolveFromBaseContainer()
        {
            // given
            var baseContainer = new ObjectContainer();
            baseContainer.RegisterTypeAs<VerySimpleClass, IInterface1>();
            var container = new ObjectContainer(baseContainer);

            // when
            var objFromChild = container.Resolve<IInterface1>();
            var objFromBase = baseContainer.Resolve<IInterface1>();

            // then
            objFromBase.Should().BeSameAs(objFromChild);
        }

        [Fact]
        public void ShouldResolveExistingObjectFromBaseWithoutTypeRegistration()
        {
            // given
            var baseContainer = new ObjectContainer();
            var container = new ObjectContainer(baseContainer);

            // when
            var objFromBase = baseContainer.Resolve<VerySimpleClass>();
            var objFromChild = container.Resolve<VerySimpleClass>();

            // then
            objFromBase.Should().BeSameAs(objFromChild);
        }

        [Fact]
        public void ShouldBeAbleToOverrideBaseContainerRegistration()
        {
            // given
            var baseContainer = new ObjectContainer();
            baseContainer.RegisterTypeAs<SimpleClassWithDefaultCtor, IInterface1>();
            var container = new ObjectContainer(baseContainer);
            container.RegisterTypeAs<VerySimpleClass, IInterface1>();

            // when
            var obj = container.Resolve<IInterface1>();
            var baseObj = baseContainer.Resolve<IInterface1>();

            // then
            obj.Should().NotBeNull();
            obj.Should().BeOfType<VerySimpleClass>();
            baseObj.Should().NotBeSameAs(obj);
        }

        [Fact]
        public void BaseContainerMustBeAnObjectContainer()
        {
            var otherContainer = new Mock<IObjectContainer>();

            Action act = () => new ObjectContainer(otherContainer.Object);
            act.Should().ThrowExactly<ArgumentException>();
        }

        [Fact]
        public void ParentObjectPoolShouldNotBeConsideredWhenReRegisteredInChild()
        {
            // given
            var baseContainer = new ObjectContainer();
            baseContainer.RegisterTypeAs<VerySimpleClass, IInterface1>();
            var container = new ObjectContainer(baseContainer);
            container.RegisterTypeAs<VerySimpleClass, IInterface1>();

            // when
            var objFromBase = baseContainer.Resolve<IInterface1>();
            var objFromChild = container.Resolve<IInterface1>();

            // then
            objFromBase.Should().NotBeSameAs(objFromChild);
        }

        public interface IParentInterface
        {
            IInterface1 Interface1 { get; }
        }

        private class ParentClass : IParentInterface
        {
            public IInterface1 Interface1 { get; set; }
            public ParentClass(IInterface1 interface1)
            {
                Interface1 = interface1;
            }
        }

        public interface IChildInterface
        {
            IInterface1 Interface1 { get; }
        }

        private class ChildClass : IChildInterface
        {
            public IInterface1 Interface1 { get; set; }
            public ChildClass(IInterface1 interface1)
            {
                Interface1 = interface1;
            }
        }

        private class DelegatingInterfce1 : IInterface1
        {
            public DelegatingInterfce1(IParentInterface parentInterface)
            {

            }
        }

        [Fact]
        public void ShouldNotDetectCircularDependencyForOverriddenObjectRegistrations()
        {
            // given
            var baseContainer = new ObjectContainer();
            baseContainer.RegisterTypeAs<VerySimpleClass, IInterface1>();
            baseContainer.RegisterTypeAs<ParentClass, IParentInterface>();
            var container = new ObjectContainer(baseContainer);
            container.RegisterTypeAs<DelegatingInterfce1, IInterface1>();
            container.RegisterTypeAs<ChildClass, IChildInterface>();

            // when
            var objFromChild = container.Resolve<IChildInterface>();
            var objFromParent = container.Resolve<IParentInterface>();

            // then
            objFromParent.Interface1.Should().NotBeSameAs(objFromChild.Interface1);
        }
    }
}

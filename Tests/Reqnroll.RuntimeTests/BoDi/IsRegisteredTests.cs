using FluentAssertions;
using Reqnroll.BoDi;
using Xunit;

namespace Reqnroll.RuntimeTests.BoDi
{
    public class IsRegisteredTests
    {
        [Fact]
        public void ShouldReturnFalseIfInterfaceNotRegistered()
        {
            // given
            var container = new ObjectContainer();

            // then
            bool isRegistered = container.IsRegistered<IInterface1>();

            isRegistered.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalseIfTypeNotRegistered()
        {
            // given
            var container = new ObjectContainer();

            // then
            bool isRegistered = container.IsRegistered<VerySimpleClass>();

            isRegistered.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalseIfTypeNotRegisteredWithParent()
        {
            // given
            var parentContainer = new ObjectContainer();
            var container = new ObjectContainer(parentContainer);

            // then
            bool isRegistered = container.IsRegistered<SimpleClassWithDefaultCtor>();
            isRegistered.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnTrueIfInterfaceRegistered()
        {
            // given
            var container = new ObjectContainer();

            // when 
            container.RegisterTypeAs<VerySimpleClass, IInterface1>();

            // then
            bool isRegistered = container.IsRegistered<IInterface1>();

            isRegistered.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnTrueIfTypeRegistered()
        {
            // given
            var container = new ObjectContainer();

            // when 
            container.RegisterInstanceAs(new SimpleClassWithDefaultCtor());

            // then
            bool isRegistered = container.IsRegistered<SimpleClassWithDefaultCtor>();

            isRegistered.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnTrueIfTypeRegisteredInParent()
        {
            // given
            var grandparentContainer = new ObjectContainer();
            var parentContainer = new ObjectContainer(grandparentContainer);
            var container = new ObjectContainer(parentContainer);

            // when
            parentContainer.RegisterInstanceAs(new SimpleClassWithDefaultCtor());

            // then
            bool isRegistered = container.IsRegistered<SimpleClassWithDefaultCtor>();
            isRegistered.Should().BeTrue();

            isRegistered = parentContainer.IsRegistered<SimpleClassWithDefaultCtor>();
            isRegistered.Should().BeTrue();

            isRegistered = grandparentContainer.IsRegistered<SimpleClassWithDefaultCtor>();
            isRegistered.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnTrueIfTypeRegisteredInGrandparent()
        {
            // given
            var grandparentContainer = new ObjectContainer();
            var parentContainer = new ObjectContainer(grandparentContainer);
            var container = new ObjectContainer(parentContainer);

            // when
            grandparentContainer.RegisterInstanceAs(new SimpleClassWithDefaultCtor());

            // then
            bool isRegistered = container.IsRegistered<SimpleClassWithDefaultCtor>();
            isRegistered.Should().BeTrue();

            isRegistered = parentContainer.IsRegistered<SimpleClassWithDefaultCtor>();
            isRegistered.Should().BeTrue();

            isRegistered = grandparentContainer.IsRegistered<SimpleClassWithDefaultCtor>();
            isRegistered.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnTrueIfRegisteredInSelfButFalseInParent()
        {
            // given
            var parentContainer = new ObjectContainer();
            var container = new ObjectContainer(parentContainer);

            // when
            container.RegisterInstanceAs(new SimpleClassWithDefaultCtor());

            // then
            bool isRegistered = container.IsRegistered<SimpleClassWithDefaultCtor>();
            isRegistered.Should().BeTrue();

            isRegistered = parentContainer.IsRegistered<SimpleClassWithDefaultCtor>();
            isRegistered.Should().BeFalse();
        }
    }
}

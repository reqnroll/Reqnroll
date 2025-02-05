using System;
using FluentAssertions;
using Reqnroll.BoDi;
using Xunit;

namespace Reqnroll.RuntimeTests.BoDi
{
    public class RegisterInstanceTests
    {
        [Fact]
        public void ShouldThrowArgumentExceptionWhenCalledWithNull()
        {
            // given
            var container = new ObjectContainer();

            // when
            Action act = () => container.RegisterInstanceAs((IInterface1)null);
            act.Should().ThrowExactly<ArgumentNullException>();
        }

        [Fact]
        public void ShouldAllowOverrideRegistrationBeforeResolve()
        {
            // given
            var container = new ObjectContainer();
            container.RegisterTypeAs<VerySimpleClass, IInterface1>();
            var instance = new SimpleClassWithDefaultCtor();

            // when 
            container.RegisterInstanceAs<IInterface1>(instance);

            // then
            var obj = container.Resolve<IInterface1>();
            obj.Should().NotBeNull();
            obj.Should().BeOfType<SimpleClassWithDefaultCtor>();
        }

        [Fact]
        public void ShouldAllowOverrideInstanceRegistrationBeforeResolve()
        {
            // given
            var container = new ObjectContainer();
            container.RegisterInstanceAs<IInterface1>(new VerySimpleClass());
            var instance = new SimpleClassWithDefaultCtor();

            // when 
            container.RegisterInstanceAs<IInterface1>(instance);

            // then
            var obj = container.Resolve<IInterface1>();
            obj.Should().NotBeNull();
            obj.Should().BeOfType<SimpleClassWithDefaultCtor>();
        }

        [Fact]
        public void ShouldNotAllowOverrideRegistrationAfterResolve()
        {
            // given
            var container = new ObjectContainer();
            container.RegisterTypeAs<VerySimpleClass, IInterface1>();
            container.Resolve<IInterface1>();
            var instance = new SimpleClassWithDefaultCtor();

            // when 
            Action act = () => container.RegisterInstanceAs<IInterface1>(instance);
            act.Should().ThrowExactly<ObjectContainerException>();
        }

        [Fact]
        public void ShouldNotAllowOverrideInstanceRegistrationAfterResolve()
        {
            // given
            var container = new ObjectContainer();
            container.RegisterInstanceAs<IInterface1>(new VerySimpleClass());
            container.Resolve<IInterface1>();
            var instance = new SimpleClassWithDefaultCtor();

            // when 
            Action act = () => container.RegisterInstanceAs<IInterface1>(instance);
            act.Should().ThrowExactly<ObjectContainerException>();
        }
    }
}

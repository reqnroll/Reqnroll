using System;
using FluentAssertions;
using Reqnroll.BoDi;
using Xunit;

namespace Reqnroll.RuntimeTests.BoDi
{
    public class RegisterTypeTests
    {
        [Fact]
        public void ShouldRegisterTypeDynamically()
        {
            // given
            var container = new ObjectContainer();
            container.RegisterTypeAs<IInterface1>(typeof(VerySimpleClass));

            // when
            var obj = container.Resolve<IInterface1>();

            // then
            obj.Should().NotBeNull();
            obj.Should().BeOfType<VerySimpleClass>();
        }

        [Fact]
        public void ShouldAllowOverrideRegistrationBeforeResolve()
        {
            // given
            var container = new ObjectContainer();
            container.RegisterTypeAs<VerySimpleClass, IInterface1>();

            // when 
            container.RegisterTypeAs<SimpleClassWithDefaultCtor, IInterface1>();

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

            // when 
            container.RegisterTypeAs<SimpleClassWithDefaultCtor, IInterface1>();

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

            // when 
            Action act = () => container.RegisterTypeAs<SimpleClassWithDefaultCtor, IInterface1>();
            act.Should().ThrowExactly<ObjectContainerException>()
                .And.Message.Should().Contain("IInterface1");
        }

        [Fact]
        public void ShouldNotAllowOverrideInstanceRegistrationAfterResolve()
        {
            // given

            var container = new ObjectContainer();
            container.RegisterInstanceAs<IInterface1>(new VerySimpleClass());
            container.Resolve<IInterface1>();

            // when 
            Action act = () => container.RegisterTypeAs<SimpleClassWithDefaultCtor, IInterface1>();
            act.Should().ThrowExactly<ObjectContainerException>();
        }

        [Fact]
        public void ShouldRegisterGenericTypeDefinitions()
        {
            // given
            var container = new ObjectContainer();
            container.RegisterTypeAs(typeof(SimpleGenericClass<>), typeof(IGenericInterface<>));

            // when
            var obj = container.Resolve<IGenericInterface<VerySimpleClass>>();

            // then
            obj.Should().NotBeNull();
            obj.Should().BeOfType<SimpleGenericClass<VerySimpleClass>>();
        }

        [Fact]
        public void ShouldNotRegisterInvalidTypeMapping()
        {
            // given
            var container = new ObjectContainer();

            // then
            Action act = () => container.RegisterTypeAs(typeof(SimpleClassExtendingGenericInterface), typeof(IGenericInterface<>));
            act.Should().ThrowExactly<InvalidOperationException>();
        }

        [Fact]
        public void ShouldAlwaysCreateInstanceOnPerRequestStrategy()
        {
            // given
            var container = new ObjectContainer();

            // when 
            container.RegisterTypeAs<SimpleClassWithDefaultCtor, IInterface1>().InstancePerDependency();

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
            container.RegisterTypeAs<SimpleClassWithDefaultCtor, IInterface1>().InstancePerContext();

            // then
            var obj1 = (SimpleClassWithDefaultCtor)container.Resolve<IInterface1>();
            var obj2 = (SimpleClassWithDefaultCtor)container.Resolve<IInterface1>();
            obj1.Should().BeSameAs(obj2);
        }
    }
}

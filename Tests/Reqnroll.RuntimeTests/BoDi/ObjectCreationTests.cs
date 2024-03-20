using System;
using System.Collections.Generic;
using FluentAssertions;
using Reqnroll.BoDi;
using Xunit;

namespace Reqnroll.RuntimeTests.BoDi
{
    public class ObjectCreationTests : IDisposable
    {
        public class ConstructorTrackingClass : IInterface1
        {
            public static event Action<ConstructorTrackingClass> ConstructorCalled;

            public ConstructorTrackingClass()
            {
                if (ConstructorCalled != null)
                    ConstructorCalled(this);
            }
        }

        private ObjectContainer container;
        private List<ConstructorTrackingClass> calledConstructors;
        private Action<ConstructorTrackingClass> constructorTrackingClassOnConstructorCalled;

        public ObjectCreationTests()
        {
            // given
            container = new ObjectContainer();
            container.RegisterTypeAs<IInterface1>(typeof(ConstructorTrackingClass));

            calledConstructors = new List<ConstructorTrackingClass>();
            constructorTrackingClassOnConstructorCalled = ctc => calledConstructors.Add(ctc);
            ConstructorTrackingClass.ConstructorCalled += constructorTrackingClassOnConstructorCalled;
        }

        public void Dispose()
        {
            if (constructorTrackingClassOnConstructorCalled != null)
                ConstructorTrackingClass.ConstructorCalled -= constructorTrackingClassOnConstructorCalled;
        }

        [Fact]
        public void ShouldCreateObjectOnFirstResolve()
        {
            // given

            // when
            var obj = container.Resolve<IInterface1>();

            // then
            obj.Should().NotBeNull();
            calledConstructors.Should().BeEquivalentTo(new object[] { obj });
        }


        [Fact]
        public void ShouldNotCreateObjectOnSecondResolve()
        {
            // given

            // when
            var obj1 = container.Resolve<IInterface1>();
            calledConstructors.Clear();
            var obj2 = container.Resolve<IInterface1>();

            // then
            calledConstructors.Should().BeEmpty();
        }

        [Fact]
        public void ShouldFireObjectCreatedEventWhenObjectIsCreated()
        {
            // given
            object objectCreated = null;
            container.ObjectCreated += o => objectCreated = o;

            // when
            var obj = container.Resolve<IInterface1>();

            // then
            objectCreated.Should().NotBeNull();
            objectCreated.Should().BeSameAs(obj);
        }

        [Fact]
        public void ShouldNotFireObjectCreatedEventOnSecondResolve()
        {
            // given
            object objectCreated = null;
            container.ObjectCreated += o => objectCreated = o;

            // when
            var obj1 = container.Resolve<IInterface1>();
            objectCreated = null;
            var obj2 = container.Resolve<IInterface1>();

            // then
            objectCreated.Should().BeNull();
        }

        [Fact]
        public void ShouldNotFireObjectCreatedEventOnResolvingInstanceRegistrations()
        {
            // given
            var obj = new ConstructorTrackingClass();
            container.RegisterInstanceAs<IInterface1>(obj);

            object objectCreated = null;
            container.ObjectCreated += o => objectCreated = o;

            // when
            container.Resolve<IInterface1>();

            // then
            objectCreated.Should().BeNull();
        }

        [Fact]
        public void ShouldNotFireObjectCreatedEventOnResolvingFactoryRegistrations()
        {
            // given
            container.RegisterFactoryAs<IInterface1>(() => new ConstructorTrackingClass());

            object objectCreated = null;
            container.ObjectCreated += o => objectCreated = o;

            // when
            container.Resolve<IInterface1>();

            // then
            objectCreated.Should().BeNull();
        }
    }
}

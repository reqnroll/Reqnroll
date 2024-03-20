using System;
using FluentAssertions;
using Reqnroll.BoDi;
using Xunit;

namespace Reqnroll.RuntimeTests.BoDi
{
    public class DisposeTests
    {
        [Fact]
        public void ContainerShouldBeDisposable()
        {
            var container = new ObjectContainer();

            container.Should().BeAssignableTo<IDisposable>();
        }

        [Fact]
        public void ContainerShouldThrowExceptionWhenDisposed()
        {
            var container = new ObjectContainer();
            container.Dispose();

            Action act = () => container.Resolve<IObjectContainer>();
            act.Should().ThrowExactly<ObjectContainerException>("Object container disposed");
        }

        [Fact]
        public void ShouldDisposeCreatedObjects()
        {
            var container = new ObjectContainer();
            container.RegisterTypeAs<DisposableClass1, IDisposableClass>();

            var obj = container.Resolve<IDisposableClass>();

            container.Dispose();

            obj.WasDisposed.Should().BeTrue();
        }

        [Fact]
        public void ShouldDisposeInstanceRegistrations()
        {
            var container = new ObjectContainer();
            var obj = new DisposableClass1();
            container.RegisterInstanceAs<IDisposableClass>(obj, dispose: true);

            container.Resolve<IDisposableClass>();

            container.Dispose();

            obj.WasDisposed.Should().BeTrue();
        }

        [Fact]
        public void ShouldNotDisposeObjectsRegisteredAsInstance()
        {
            var container = new ObjectContainer();
            var obj = new DisposableClass1();
            container.RegisterInstanceAs<IDisposableClass>(obj);

            container.Resolve<IDisposableClass>();

            container.Dispose();

            obj.WasDisposed.Should().BeFalse();
        }

        [Fact]
        public void ShouldNotDisposeObjectsFromBaseContainer()
        {
            var baseContainer = new ObjectContainer();
            baseContainer.RegisterTypeAs<DisposableClass1, IDisposableClass>();
            var container = new ObjectContainer(baseContainer);

            baseContainer.Resolve<IDisposableClass>();
            var obj = container.Resolve<IDisposableClass>();

            container.Dispose();

            obj.WasDisposed.Should().BeFalse();
        }
    }
}

using System;
using System.Threading.Tasks;
using FluentAssertions;
using Reqnroll.BoDi;
using Xunit;

namespace Reqnroll.RuntimeTests.BoDi
{
    public class DisposeTests
    {
        [Fact]
        public void ContainerShouldBeAsyncDisposable()
        {
            var container = new ObjectContainer();

            container.Should().BeAssignableTo<IAsyncDisposable>();
        }

        [Fact]
        public async Task ContainerShouldThrowExceptionWhenAsyncDisposedAndCallingResolve()
        {
            var container = new ObjectContainer();
            await container.DisposeAsync();

            Action act = () => container.Resolve<IObjectContainer>();
            act.Should().ThrowExactly<ObjectContainerException>("Object container disposed");
        }

        [Fact]
        public async Task ContainerShouldThrowExceptionWhenAsyncDisposedAndCallingRegisterInstanceAs()
        {
            var container = new ObjectContainer();
            await container.DisposeAsync();

            Action act = () => container.RegisterInstanceAs<IAsyncDisposableClass>(new AsyncDisposableClass1());
            act.Should().ThrowExactly<ObjectContainerException>("Object container disposed");
        }

        [Fact]
        public async Task ContainerShouldThrowExceptionWhenAsyncDisposedAndCallingRegisterFactoryAs()
        {
            var container = new ObjectContainer();
            await container.DisposeAsync();

            Action act = () => container.RegisterFactoryAs<IAsyncDisposableClass>(() => new AsyncDisposableClass1());
            act.Should().ThrowExactly<ObjectContainerException>("Object container disposed");
        }

        [Fact]
        public async Task ContainerShouldThrowExceptionWhenAsyncDisposedAndCallingRegisterTypeAs()
        {
            var container = new ObjectContainer();
            await container.DisposeAsync();

            Action act = () => container.RegisterTypeAs<IAsyncDisposableClass>(typeof(AsyncDisposableClass1));
            act.Should().ThrowExactly<ObjectContainerException>("Object container disposed");
        }

        [Fact]
        public async Task ShouldDisposeCreatedObjects()
        {
            var container = new ObjectContainer();
            container.RegisterTypeAs<DisposableClass1, IDisposableClass>();

            var obj = container.Resolve<IDisposableClass>();

            await container.DisposeAsync();

            obj.WasDisposed.Should().BeTrue();
        }

        [Fact]
        public async Task ShouldDisposeInstanceRegistrations()
        {
            var container = new ObjectContainer();
            var obj = new DisposableClass1();
            container.RegisterInstanceAs<IDisposableClass>(obj, dispose: true);

            container.Resolve<IDisposableClass>();

            await container.DisposeAsync();

            obj.WasDisposed.Should().BeTrue();
        }

        [Fact]
        public async Task ShouldNotDisposeObjectsRegisteredAsInstance()
        {
            var container = new ObjectContainer();
            var obj = new DisposableClass1();
            container.RegisterInstanceAs<IDisposableClass>(obj);

            container.Resolve<IDisposableClass>();

            await container.DisposeAsync();

            obj.WasDisposed.Should().BeFalse();
        }

        [Fact]
        public async Task ShouldNotDisposeObjectsFromBaseContainer()
        {
            var baseContainer = new ObjectContainer();
            baseContainer.RegisterTypeAs<DisposableClass1, IDisposableClass>();
            var container = new ObjectContainer(baseContainer);

            baseContainer.Resolve<IDisposableClass>();
            var obj = container.Resolve<IDisposableClass>();

            await container.DisposeAsync();

            obj.WasDisposed.Should().BeFalse();
        }

        [Fact]
        public async Task ShouldAsyncDisposeCreatedObjects()
        {
            var container = new ObjectContainer();
            container.RegisterTypeAs<AsyncDisposableClass1, IAsyncDisposableClass>();

            var obj = container.Resolve<IAsyncDisposableClass>();

            await container.DisposeAsync();

            obj.WasDisposed.Should().BeFalse();
            obj.WasDisposedAsync.Should().BeTrue();
        }

        [Fact]
        public async Task ShouldAsyncDisposeInstanceRegistrations()
        {
            var container = new ObjectContainer();
            var obj = new AsyncDisposableClass1();
            container.RegisterInstanceAs<IAsyncDisposableClass>(obj, dispose: true);

            container.Resolve<IAsyncDisposableClass>();

            await container.DisposeAsync();

            obj.WasDisposed.Should().BeFalse();
            obj.WasDisposedAsync.Should().BeTrue();
        }

        [Fact]
        public async Task ShouldNotAsyncDisposeObjectsRegisteredAsInstance()
        {
            var container = new ObjectContainer();
            var obj = new AsyncDisposableClass1();
            container.RegisterInstanceAs<IAsyncDisposableClass>(obj);

            container.Resolve<IAsyncDisposableClass>();

            await container.DisposeAsync();

            obj.WasDisposed.Should().BeFalse();
            obj.WasDisposedAsync.Should().BeFalse();
        }

        [Fact]
        public async Task ShouldNotAsyncDisposeObjectsFromBaseContainer()
        {
            var baseContainer = new ObjectContainer();
            baseContainer.RegisterTypeAs<AsyncDisposableClass1, IAsyncDisposableClass>();
            var container = new ObjectContainer(baseContainer);

            baseContainer.Resolve<IAsyncDisposableClass>();
            var obj = container.Resolve<IAsyncDisposableClass>();

            await container.DisposeAsync();

            obj.WasDisposed.Should().BeFalse();
            obj.WasDisposedAsync.Should().BeFalse();
        }
    }
}

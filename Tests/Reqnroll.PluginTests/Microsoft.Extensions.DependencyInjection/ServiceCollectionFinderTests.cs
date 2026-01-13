using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Reqnroll.Bindings.Discovery;
using Reqnroll.Infrastructure;
using Reqnroll.Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Reqnroll.PluginTests.Microsoft.Extensions.DependencyInjection;

public class ServiceCollectionFinderTests
{
    private readonly Mock<ITestRunnerManager> _testRunnerManagerMock = new();
    private readonly Mock<IRuntimeBindingRegistryBuilder> _bindingRegistryBuilderMock = new();
    private readonly Mock<ITestAssemblyProvider> _testAssemblyProviderMock = new();
    private readonly Mock<IBindingAssemblyLoader> _bindingAssemblyLoaderMock = new();

    [Fact]
    public void GetServiceCollection_HappyPath_ResolvesCorrectServiceCollection()
    {
        // Arrange
        var sut = CreateServiceCollectionFinderWithMocks(typeof(ValidStartWithAutoRegister));

        // Act
        var (serviceCollection, _) = sut.GetServiceCollection();

        // Assert
        serviceCollection.Should().NotBeNull();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var testService = serviceProvider.GetRequiredService<ITestInterface>();
        testService.Name.Should().Be("ValidStartWithAutoRegister");
    }

    [Fact]
    public void GetServiceCollection_MethodIsVoid_ThrowsInvalidScenarioDependenciesException()
    {
        // Arrange
        var sut = CreateServiceCollectionFinderWithMocks(typeof(InvalidStartVoid));

        // Act
        var act = () => sut.GetServiceCollection();

        // Assert
        act.Should()
           .Throw<InvalidScenarioDependenciesException>()
           .WithMessage("[ScenarioDependencies] should return IServiceCollection but the method doesn't return a value.");
    }

    [Fact]
    public void GetServiceCollection_MethodReturnsNull_ThrowsInvalidScenarioDependenciesException()
    {
        // Arrange
        var sut = CreateServiceCollectionFinderWithMocks(typeof(InvalidStartNull));

        // Act
        var act = () => sut.GetServiceCollection();

        // Assert
        act.Should()
           .Throw<InvalidScenarioDependenciesException>()
           .WithMessage("[ScenarioDependencies] should return IServiceCollection but returned null.");
    }

    [Fact]
    public void GetServiceCollection_MethodReturnsInvalidType_ThrowsInvalidScenarioDependenciesException()
    {
        // Arrange
        var sut = CreateServiceCollectionFinderWithMocks(typeof(InvalidStartWrongType));

        // Act
        var act = () => sut.GetServiceCollection();

        // Assert
        act.Should()
           .Throw<InvalidScenarioDependenciesException>()
           .WithMessage("[ScenarioDependencies] should return IServiceCollection but returned System.Collections.Generic.List`1[System.String].");
    }

    [Fact]
    public void GetServiceCollection_NotFound_ThrowsMissingScenarioDependenciesException()
    {
        // Arrange
        var sut = CreateServiceCollectionFinderWithMocks(typeof(ServiceCollectionFinderTests));

        // Act
        var act = () => sut.GetServiceCollection();

        // Assert
        act.Should()
           .Throw<MissingScenarioDependenciesException>()
           .WithMessage("No method marked with [ScenarioDependencies] attribute found. It should be a (public or non-public) static method. Scanned assemblies: Reqnroll.PluginTests.");
    }

    [Fact]
    public void GetServiceCollection_AutoRegisterBindingsTrue_RegisterBindingsAsScoped()
    {
        // Arrange
        var sut = CreateServiceCollectionFinderWithMocks(typeof(ValidStartWithAutoRegister), typeof(Binding1));

        // Act
        var (serviceCollection, _) = sut.GetServiceCollection();

        // Assert
        serviceCollection.Should().NotBeNull();
        serviceCollection.Should().Contain(d => d.ImplementationType == typeof(Binding1) && d.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact]
    public void GetServiceCollection_AutoRegisterBindingsFalse_DoNotRegisterBindings()
    {
        // Arrange
        var sut = CreateServiceCollectionFinderWithMocks(typeof(ValidStartWithoutAutoRegister), typeof(Binding1));

        // Act
        var (serviceCollection, _) = sut.GetServiceCollection();

        // Assert
        serviceCollection.Should().NotBeNull();
        serviceCollection.Should().NotContain(d => d.ImplementationType == typeof(Binding1));
    }

    [Fact]
    public void GetServiceCollection_CustomLifetime_ReturnsCorrectLifetime()
    {
        // Arrange
        var sut = CreateServiceCollectionFinderWithMocks(typeof(ValidStartWithFeatureLifetime));

        // Act
        var (_, lifetime) = sut.GetServiceCollection();

        // Assert
        lifetime.Should().Be(ScopeLevelType.Feature);
    }

    [Fact]
    public void GetServiceCollection_BindingAssembliesNull_FallbacksToBindingRegistryBuilder()
    {
        // Arrange
        var types = new[] { typeof(ValidStartWithAutoRegister) };
        var assemblyMock = new Mock<Assembly>();
        assemblyMock.Setup(m => m.GetTypes()).Returns(types.ToArray());
        var assembly = types[0].Assembly;
        assemblyMock.Setup(m => m.GetName()).Returns(assembly.GetName());

        _testRunnerManagerMock.Setup(m => m.BindingAssemblies).Returns((Assembly[])null);
        _testAssemblyProviderMock.Setup(m => m.TestAssembly).Returns(assemblyMock.Object);
        _bindingRegistryBuilderMock.Setup(m => m.GetBindingAssemblies(assemblyMock.Object)).Returns([assemblyMock.Object]);

        var sut = new ServiceCollectionFinder(_testRunnerManagerMock.Object, _bindingRegistryBuilderMock.Object, _testAssemblyProviderMock.Object, _bindingAssemblyLoaderMock.Object);

        // Act
        var (serviceCollection, _) = sut.GetServiceCollection();

        // Assert
        serviceCollection.Should().NotBeNull();
        _bindingRegistryBuilderMock.Verify(m => m.GetBindingAssemblies(assemblyMock.Object), Times.Once);
    }

    [Fact]
    public void GetServiceProviderLifetime_ReturnsCorrectLifetime()
    {
        // Arrange
        var sut = CreateServiceCollectionFinderWithMocks(typeof(ValidStartWithFeatureLifetime));

        // Act
        var lifetime = sut.GetServiceProviderLifetime();

        // Assert
        lifetime.Should().Be(ServiceProviderLifetimeType.Feature);
    }

    private ServiceCollectionFinder CreateServiceCollectionFinderWithMocks(params Type[] types)
    {
        var assemblyMock = new Mock<Assembly>();
        assemblyMock.Setup(m => m.GetTypes()).Returns(types.ToArray());
        if (types.Length > 0)
        {
            var assembly = types[0].Assembly;
            assemblyMock.Setup(m => m.GetName()).Returns(assembly.GetName());
        }

        _testRunnerManagerMock.Setup(m => m.BindingAssemblies).Returns([assemblyMock.Object]);
        _testAssemblyProviderMock.Setup(m => m.TestAssembly).Returns(assemblyMock.Object);
        _bindingRegistryBuilderMock.Setup(m => m.GetBindingAssemblies(It.IsAny<Assembly>())).Returns([assemblyMock.Object]);

        return new ServiceCollectionFinder(_testRunnerManagerMock.Object, _bindingRegistryBuilderMock.Object, _testAssemblyProviderMock.Object, _bindingAssemblyLoaderMock.Object);
    }

    private class ValidStartWithFeatureLifetime
    {
        [ScenarioDependencies(ScopeLevel = ScopeLevelType.Feature, ServiceProviderLifetime = ServiceProviderLifetimeType.Feature)]
        public static IServiceCollection GetServices()
        {
            return new ServiceCollection();
        }
    }

    private interface ITestInterface
    {
        string Name { get; }
    }

    private record TestInterface(string Name) : ITestInterface;

    private class ValidStartWithAutoRegister
    {
        [ScenarioDependencies]
        public static IServiceCollection GetServices()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<ITestInterface>(new TestInterface("ValidStartWithAutoRegister"));
            return serviceCollection;
        }
    }

    private class ValidStartWithoutAutoRegister
    {
        [ScenarioDependencies(AutoRegisterBindings = false)]
        public static IServiceCollection GetServices()
        {
            var serviceCollection = new ServiceCollection();
            return serviceCollection;
        }
    }

    private class InvalidStartVoid
    {
        [ScenarioDependencies]
        public static void GetServices()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<ITestInterface>(new TestInterface("ValidStartWithAutoRegister"));
        }
    }

    private class InvalidStartNull
    {
        [ScenarioDependencies]
        public static IServiceCollection GetServices()
        {
            return null;
        }
    }

    private class InvalidStartWrongType
    {
        [ScenarioDependencies]
        public static object GetServices()
        {
            return new List<string>();
        }
    }

    [Binding]
    private class Binding1;
}

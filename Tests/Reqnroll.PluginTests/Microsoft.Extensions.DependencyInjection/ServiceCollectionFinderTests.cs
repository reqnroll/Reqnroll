using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Reqnroll.Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Reqnroll.PluginTests.Microsoft.Extensions.DependencyInjection;

public class ServiceCollectionFinderTests
{
    [Fact]
    public void GetServiceCollection_HappyPath_ResolvesCorrectServiceCollection()
    {
        // Arrange
        var testRunnerManagerMock = CreateTestRunnerManagerMock(typeof(ValidStartWithAutoRegister));
        var sut = new ServiceCollectionFinder(testRunnerManagerMock.Object);

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
        var testRunnerManagerMock = CreateTestRunnerManagerMock(typeof(InvalidStartVoid));
        var sut = new ServiceCollectionFinder(testRunnerManagerMock.Object);

        // Act
        var act = () => sut.GetServiceCollection();

        // Assert
        act.Should().Throw<InvalidScenarioDependenciesException>()
           .WithMessage("[ScenarioDependencies] should return IServiceCollection but the method doesn't return a value.");
    }

    [Fact]
    public void GetServiceCollection_MethodReturnsNull_ThrowsInvalidScenarioDependenciesException()
    {
        // Arrange
        var testRunnerManagerMock = CreateTestRunnerManagerMock(typeof(InvalidStartNull));
        var sut = new ServiceCollectionFinder(testRunnerManagerMock.Object);

        // Act
        var act = () => sut.GetServiceCollection();

        // Assert
        act.Should().Throw<InvalidScenarioDependenciesException>()
           .WithMessage("[ScenarioDependencies] should return IServiceCollection but returned null.");
    }


    [Fact]
    public void GetServiceCollection_MethodReturnsInvalidType_ThrowsInvalidScenarioDependenciesException()
    {
        // Arrange
        var testRunnerManagerMock = CreateTestRunnerManagerMock(typeof(InvalidStartWrongType));
        var sut = new ServiceCollectionFinder(testRunnerManagerMock.Object);

        // Act
        var act = () => sut.GetServiceCollection();

        // Assert
        act.Should().Throw<InvalidScenarioDependenciesException>()
           .WithMessage("[ScenarioDependencies] should return IServiceCollection but returned System.Collections.Generic.List`1[System.String].");
    }

    [Fact]
    public void GetServiceCollection_NotFound_ThrowsMissingScenarioDependenciesException()
    {
        // Arrange
        var testRunnerManagerMock = CreateTestRunnerManagerMock(typeof(ServiceCollectionFinderTests));
        var sut = new ServiceCollectionFinder(testRunnerManagerMock.Object);

        // Act
        var act = () => sut.GetServiceCollection();

        // Assert
        act.Should().Throw<MissingScenarioDependenciesException>()
           .WithMessage("No method marked with [ScenarioDependencies] attribute found. It should be a (public or non-public) static method. Scanned assemblies: Reqnroll.PluginTests.");
    }

    [Fact]
    public void GetServiceCollection_AutoRegisterBindingsTrue_RegisterBindingsAsScoped()
    {
        // Arrange
        var testRunnerManagerMock = CreateTestRunnerManagerMock(typeof(ValidStartWithAutoRegister), typeof(Binding1));
        var sut = new ServiceCollectionFinder(testRunnerManagerMock.Object);

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
        var testRunnerManagerMock = CreateTestRunnerManagerMock(typeof(ValidStartWithoutAutoRegister), typeof(Binding1));
        var sut = new ServiceCollectionFinder(testRunnerManagerMock.Object);

        // Act
        var (serviceCollection, _) = sut.GetServiceCollection();

        // Assert
        serviceCollection.Should().NotBeNull();
        serviceCollection.Should().NotContain(d => d.ImplementationType == typeof(Binding1));
    }

    private static Mock<ITestRunnerManager> CreateTestRunnerManagerMock(params Type[] types)
    {
        var assemblyMock = new Mock<Assembly>();
        assemblyMock.Setup(m => m.GetTypes()).Returns(types.ToArray());
        if (types.Length > 0)
        {
            var assembly = types[0].Assembly;
            assemblyMock.Setup(m => m.GetName()).Returns(assembly.GetName());
        }

        var testRunnerManagerMock = new Mock<ITestRunnerManager>();
        testRunnerManagerMock.Setup(m => m.BindingAssemblies).Returns([assemblyMock.Object]);
        return testRunnerManagerMock;
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

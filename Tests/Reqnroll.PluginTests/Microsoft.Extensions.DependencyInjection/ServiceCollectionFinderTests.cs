﻿using System;
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
    [Binding]
    private class Binding1;
}

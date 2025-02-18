using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll.Microsoft.Extensions.DependencyInjection;
using Reqnroll.Microsoft.Extensions.DependencyInjection.Internal;
using Xunit;

namespace Reqnroll.PluginTests.Microsoft.Extensions.DependencyInjection;

public class ServiceCollectionFinderTests
{
    [Fact]
    public void GetServiceCollection_HappyPath_ResolvesCorrectServiceCollection()
    {
        // Arrange
        var resolver = new MyAssemblyTypeResolver([typeof(ValidStart1)]);
        var sut = new ServiceCollectionFinder(resolver);

        // Act
        var (serviceCollection, _) = sut.GetServiceCollection();

        // Assert
        serviceCollection.Should().NotBeNull();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var testService = serviceProvider.GetRequiredService<ITestInterface>();
        testService.Name.Should().Be("ValidStart1");
    }

    private interface ITestInterface
    {
        string Name { get; }
    }

    private record TestInterface(string Name) : ITestInterface;

    private class ValidStart1
    {
        [ScenarioDependencies]
        public static IServiceCollection GetServices()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<ITestInterface>(new TestInterface("ValidStart1"));
            return serviceCollection;
        }
    }

    private class MyAssemblyTypeResolver(IEnumerable<Type> types) : IAssemblyTypeResolver
    {
        public IEnumerable<Type> GetAssemblyTypes() => types;
    }
}
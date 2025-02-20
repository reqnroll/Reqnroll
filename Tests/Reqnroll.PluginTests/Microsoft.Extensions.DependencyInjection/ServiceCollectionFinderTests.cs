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
        var assemblyMock = new Mock<Assembly>();
        assemblyMock.Setup(m => m.GetTypes()).Returns([typeof(ValidStart1)]);

        var testRunnerManagerMock = new Mock<ITestRunnerManager>();
        testRunnerManagerMock.Setup(m => m.BindingAssemblies).Returns([assemblyMock.Object]);
        var sut = new ServiceCollectionFinder(testRunnerManagerMock.Object);

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

}

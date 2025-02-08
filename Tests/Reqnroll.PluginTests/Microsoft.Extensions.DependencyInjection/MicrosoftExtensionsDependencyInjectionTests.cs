using FluentAssertions;
using Moq;
using Reqnroll.Plugins;
using Reqnroll.Tracing;
using Xunit;

namespace Reqnroll.PluginTests.Microsoft.Extensions.DependencyInjection;

public class MicrosoftExtensionsDependencyInjectionTests
{
    [Fact]
    public void LoadPlugin_MicrosoftExtensionsDependencyInjection_ShouldNotBeNull()
    {
        var loader = new RuntimePluginLoader(new DotNetCorePluginAssemblyLoader());
        var listener = new Mock<ITraceListener>();

        var plugin = loader.LoadPlugin("Reqnroll.Microsoft.Extensions.DependencyInjection.ReqnrollPlugin.dll", listener.Object, It.IsAny<bool>());

        plugin.Should().NotBeNull();
    }
}
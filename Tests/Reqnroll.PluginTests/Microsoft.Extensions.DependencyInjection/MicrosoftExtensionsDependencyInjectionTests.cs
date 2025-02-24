using FluentAssertions;
using NSubstitute;
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
        var listener = Substitute.For<ITraceListener>();

        var plugin = loader.LoadPlugin("Reqnroll.Microsoft.Extensions.DependencyInjection.ReqnrollPlugin.dll", listener, Arg.Any<bool>());

        plugin.Should().NotBeNull();
    }
}
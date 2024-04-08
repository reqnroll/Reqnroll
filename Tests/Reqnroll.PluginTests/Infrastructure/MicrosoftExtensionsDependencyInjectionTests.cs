using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Reqnroll.Plugins;
using Reqnroll.Tracing;
using Xunit;

namespace Reqnroll.PluginTests.Infrastructure
{
    public class MicrosoftExtensionsDependencyInjectionTests
    {
        [Fact]
        public void LoadPlugin_MicrosoftExtensionsDependencyInjection_ShouldNotBeNull()
        {
            var loader = new RuntimePluginLoader();
            var listener = new Mock<ITraceListener>();

            var plugin = loader.LoadPlugin("Reqnroll.Microsoft.Extensions.DependencyInjection.ReqnrollPlugin.dll", listener.Object, It.IsAny<bool>());

            plugin.Should().NotBeNull();
        }
    }
}

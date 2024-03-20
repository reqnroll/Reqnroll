#if !BODI_LIMITEDRUNTIME
using System.Configuration;
using FluentAssertions;
using Reqnroll.BoDi;
using Xunit;

namespace Reqnroll.RuntimeTests.BoDi
{
    public class RegisterFromConfigTests
    {
#if !NETCOREAPP
        //Disable this tests, because of problem with dotnet test and app.configs - https://github.com/dotnet/corefx/issues/22101#partial-timeline-marker

        [Fact]
        public void ShouldResolveFromDefaultSection()
        {
            // given
            var container = new ObjectContainer();
            container.RegisterFromConfiguration();

            // when
            var obj = container.Resolve<IInterface1>();

            // then
            obj.Should().NotBeNull();
            obj.Should().BeOfType<VerySimpleClass>();
        }

        [Fact]
        public void ShouldResolveFromCustomSection()
        {
            // given
            var section = (TestConfigSection)ConfigurationManager.GetSection("testSection");
            section.Should().NotBeNull();

            var container = new ObjectContainer();
            container.RegisterFromConfiguration(section.Dependencies);

            // when
            var obj = container.Resolve<IInterface1>();

            // then
            obj.Should().NotBeNull();
            obj.Should().BeOfType<VerySimpleClass>();
        }
#endif
    }
}
#endif

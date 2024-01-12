using System;
using System.Reflection;
using FluentAssertions;
using Reqnroll.Infrastructure;
using Reqnroll.Plugins;
using Xunit;

namespace Reqnroll.RuntimeTests.Container
{
    public class RuntimePluginLocatorTests
    {
        [Fact]
        public void LoadPlugins_Doesnt_load_too_much_assemblies()
        {
            //ARRANGE
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            var testAssemblyProvider = new TestAssemblyProvider();
            testAssemblyProvider.RegisterTestAssembly(Assembly.GetExecutingAssembly());
            var runtimePluginLocator = new RuntimePluginLocator(new RuntimePluginLocationMerger(), new ReqnrollPath(), testAssemblyProvider);

            //ACT
            runtimePluginLocator.GetAllRuntimePlugins();


            //ASSERT
            var nowLoadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            nowLoadedAssemblies.Should().BeEquivalentTo(loadedAssemblies);
        }
    }
}

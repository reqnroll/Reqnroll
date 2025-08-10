using System;
using System.Reflection;
using Xunit;
using Reqnroll.Configuration;
using FluentAssertions;
using Reqnroll.Infrastructure;
using Reqnroll.UnitTestProvider;

namespace Reqnroll.RuntimeTests.Infrastructure
{
    
    public class TestRunContainerBuilderTests : IDisposable
    {
        [Fact]
        public void Should_create_a_container()
        {
            var container = TestObjectFactories.CreateDefaultGlobalContainer();
            container.Should().NotBeNull();
        }

        [Fact]
        public void Should_register_runtime_configuration_with_default_config()
        {
            var container = TestObjectFactories.CreateDefaultGlobalContainer();
            container.Resolve<Reqnroll.Configuration.ReqnrollConfiguration>().Should().NotBeNull();
        }

        private class DummyTestRunnerFactory : ITestRunnerFactory
        {
            public ITestRunner Create(Assembly testAssembly)
            {
                throw new NotImplementedException();
            }
        }

        #region Fix: ReSharper test runner assemby loading issue workaround
        public TestRunContainerBuilderTests()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
        }
        

        private Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var thisAssembly = Assembly.GetExecutingAssembly();
            if (args.Name.Contains(thisAssembly.GetName().Name))
                return thisAssembly;
            return null;
        }
        #endregion

        [Fact]
        public void Should_be_able_to_customize_dependencies_from_config()
        {
            var configurationHolder = new JsonStringRuntimeConfigurationProvider(
                $$"""
                {
                    "runtime": {
                        "dependencies": [
                            {
                                "type": "{{typeof(DummyTestRunnerFactory).AssemblyQualifiedName}}",
                                "as": "{{typeof(ITestRunnerFactory).AssemblyQualifiedName}}"
                            }
                        ]
                    }
                }
                """);

            var container = TestObjectFactories.CreateDefaultGlobalContainer(configurationHolder);
            var testRunnerFactory = container.Resolve<ITestRunnerFactory>();
            testRunnerFactory.Should().BeOfType(typeof(DummyTestRunnerFactory));
        }

        [Fact]
        public void Should_be_able_to_customize_dependencies_from_json_config()
        {
            var expectedInterface = typeof(ITestRunnerFactory).AssemblyQualifiedName;
            var expectedImplementation = typeof(DummyTestRunnerFactory).AssemblyQualifiedName;
            
            var configurationHolder = new JsonStringRuntimeConfigurationProvider(
            $@"{{
                ""runtime"": {{ 
                    ""dependencies"": [
                        {{
                            ""type"": ""{expectedImplementation}"",
                            ""as"": ""{expectedInterface}""
                        }}
                    ]
                }}
            }}");

            var container = TestObjectFactories.CreateDefaultGlobalContainer(configurationHolder);
            var testRunnerFactory = container.Resolve<ITestRunnerFactory>();
            testRunnerFactory.Should().BeOfType(typeof(DummyTestRunnerFactory));
        }

        public class CustomUnitTestProvider : IUnitTestRuntimeProvider
        {
            public void TestPending(string message)
            {
                throw new NotImplementedException();
            }

            public void TestInconclusive(string message)
            {
                throw new NotImplementedException();
            }

            public void TestIgnore(string message)
            {
                throw new NotImplementedException();
            }

            public ScenarioExecutionStatus? DetectExecutionStatus(Exception exception) => null;
        }

        public void Dispose()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomainOnAssemblyResolve;
        }
    }
}

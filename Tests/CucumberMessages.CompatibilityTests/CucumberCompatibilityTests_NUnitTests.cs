using Reqnroll.TestProjectGenerator.Data;
using Reqnroll.TestProjectGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CucumberMessages.Tests
{
    [TestClass]
    public class CucumberCompatibilityTests_NUnitTests : CucumberCompatibilityTestBase
    {
        protected override void TestInitialize()
        {
            base.TestInitialize();
            _testRunConfiguration.UnitTestProvider = UnitTestProvider.NUnit4;
        }


        [TestMethod]
        //[DataRow("xRetry")]
        [DataRow("NUnitRetry")]
        // These tests attempt to execute the Retry scenario from the CCK using the known open-source plugins for Reqnroll that integrate Retry functionality into Reqnroll.
        public void CCKRetryScenario(string pluginName)
        {
            // force the creation of the default project before adding another
            var def = _solutionDriver.DefaultProject;
            const string testDecPluginSource = @"
using Reqnroll.Generator;
using Reqnroll.Generator.Plugins;
using Reqnroll.Generator.UnitTestConverter;
using Reqnroll.Infrastructure;
using Reqnroll.UnitTestProvider;
using System;
using System.CodeDom;

[assembly: GeneratorPlugin(typeof(TestOrderDecorator.TestOrderDecoratorGenerator))]
namespace TestOrderDecorator
{
    public class TestOrderDecoratorGenerator : IGeneratorPlugin
    {
        public void Initialize(GeneratorPluginEvents generatorPluginEvents, GeneratorPluginParameters generatorPluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
        {
            System.Diagnostics.Debugger.Launch();

            generatorPluginEvents.RegisterDependencies += GeneratorPluginEvents_RegisterDependencies;
        }

        private void GeneratorPluginEvents_RegisterDependencies(object sender, RegisterDependenciesEventArgs e)
        {
            e.ObjectContainer.RegisterTypeAs<TestOrderDecorator, ITestMethodDecorator>();
        }
    }
    public class TestOrderDecorator : ITestMethodDecorator
    {
        public int Priority => 0;
        private enum Framework
        {
            nunit,
            xunit,
            mstest
        }

        private Framework _framework;

        public bool CanDecorateFrom(TestClassGenerationContext generationContext, CodeMemberMethod testMethod)
        {
            // Discover which test framework (NUnit, xUnit, or MSTest) has already generated attributes on the testMethod
            foreach (CodeAttributeDeclaration attribute in testMethod.CustomAttributes)
            {
                if (attribute.Name.Contains(""NUnit"") || attribute.Name.Contains(""Xunit""))
                {
                    // save value in a field;
                    if (attribute.Name.Contains(""NUnit""))
                        _framework = Framework.nunit;
                    if (attribute.Name.Contains(""Xunit""))
                        _framework = Framework.xunit;
                    // If MSTest return false, otherwise return true
                    if (attribute.Name.Contains(""MSTest""))
                    {
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }

        private static int invocationSequenceNumber = 0;
        private string NUNIT_ORDER_DECORATOR_ATTRIBUTE = ""NUnit.Framework.OrderAttribute"";
        private string XUNIT_ORDER_DECORATOR_ATTRIBUTE = """";

        public void DecorateFrom(TestClassGenerationContext generationContext, CodeMemberMethod testMethod)
        {
            // Using the previously saved indication of which test framework is being used, determine which string to use as the template for the new attribute
            var attributeName = _framework switch
            {
                Framework.xunit => XUNIT_ORDER_DECORATOR_ATTRIBUTE,
                Framework.nunit => NUNIT_ORDER_DECORATOR_ATTRIBUTE,
                _ => String.Empty
            };
            if (attributeName == String.Empty)
                return;
            // Using the template, create an attribute
            var attribute = new CodeAttributeDeclaration(
                attributeName,
                new CodeAttributeArgument(
                    new CodePrimitiveExpression(invocationSequenceNumber)));
            // Add the new attribute to the testMethod
            testMethod.CustomAttributes.Add(attribute);
            // increment invocationSequenceNumber

            invocationSequenceNumber++;
        }
    }

}
";

            const string pluginTargetContent = @"<Project ToolsVersion=""12.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
	<PropertyGroup>
        <_TestOrderDecoratorGeneratorPlugin>netstandard2.0</_TestOrderDecoratorGeneratorPlugin>
		<_TestOrderDecoratorPluginGeneratorPath>$(MSBuildThisFileDirectory)\$(_TestOrderDecoratorGeneratorPlugin)\TestOrderDecorator.ReqnrollPlugin.dll</_TestOrderDecoratorPluginGeneratorPath>
	</PropertyGroup>
</Project>";

            const string pluginPropsContent = @"<Project ToolsVersion=""12.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"" TreatAsLocalProperty=""TaskFolder;TaskAssembly"">
	<ItemGroup>
	  <ReqnrollGeneratorPlugins Include=""$(_TestOrderDecoratorPluginGeneratorPath)"" />
  </ItemGroup>

</Project>";
            var (plugin, version, unitTestProvider) = pluginName switch
            {
                "NUnitRetry" => ("NUnitRetry.ReqnrollPlugin", "1.0.100", UnitTestProvider.NUnit4),
                "xRetry" => ("xRetry.Reqnroll", "1.0.0", UnitTestProvider.xUnit),
                _ => throw new NotImplementedException("unknown plugin name")
            };

            //_testRunConfiguration.UnitTestProvider = unitTestProvider;
            _projectsDriver.AddNuGetPackage(plugin, version);

            var tdp = _projectsDriver.CreateProject("TestOrderDecorator.ReqnrollPlugin", "C#", ProjectType.Library);
            tdp.TargetFramework = TargetFramework.NetStandard20;

            tdp.AddFile(new ProjectFile("TestOrderDecorator.ReqnrollPlugin.props", "None", pluginPropsContent, CopyToOutputDirectory.DoNotCopy));
            tdp.AddFile(new ProjectFile("TestOrderDecorator.ReqnrollPlugin.targets", "None", pluginTargetContent, CopyToOutputDirectory.DoNotCopy));
            tdp.AddNuGetPackage("Reqnroll.CustomPlugin", "2.2.2-local");

            tdp.AddFile(new ProjectFile("TestOrderDecorator.cs", "Compile", testDecPluginSource, CopyToOutputDirectory.DoNotCopy));
            def.AddProjectReference("..\\TestOrderDecorator.ReqnrollPlugin", tdp);

            var testName = $"retry-{pluginName}";
            ExecuteCCKScenario(testName, testName);

        }

    }
}

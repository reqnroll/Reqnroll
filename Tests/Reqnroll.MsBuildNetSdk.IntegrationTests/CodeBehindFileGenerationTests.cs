using System.Reflection;

using Reqnroll.MsBuildNetSdk.IntegrationTests.Features;
using Xunit;

namespace Reqnroll.MsBuildNetSdk.IntegrationTests
{
    
    public class CodeBehindFileGenerationTests
    {
        [Fact]
        public void TestIfCodeBehindFilesWasGeneratedAndCompiled()
        {
            var assemblyHoldingThisClass = Assembly.GetExecutingAssembly();
            var typeOfGeneratedFeatureFile = assemblyHoldingThisClass.GetType(typeof(DummyFeatureFileToTestMSBuildNetsdkCodebehindFileGenerationFeature).FullName);
            Assert.NotNull(typeOfGeneratedFeatureFile);
        }
    }
}

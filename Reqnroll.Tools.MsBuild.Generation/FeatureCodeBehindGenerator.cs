using System;
using System.IO;
using Reqnroll.Generator.Interfaces;

namespace Reqnroll.Tools.MsBuild.Generation
{
    public class FeatureCodeBehindGenerator : IDisposable
    {
        private readonly ITestGenerator _testGenerator;

        public FeatureCodeBehindGenerator(ITestGenerator testGenerator)
        {
            _testGenerator = testGenerator;
        }

        public TestFileGeneratorResult GenerateCodeBehindFile(string featureFile, bool featureFilesEmbedded = false)
        {
            var featureFileInput = new FeatureFileInput(featureFile);

            var generatedFeatureFileName = Path.GetFileName(_testGenerator.GetTestFullPath(featureFileInput));

            var testGeneratorResult = _testGenerator.GenerateTestFile(featureFileInput, new GenerationSettings() { FeatureFilesEmbedded = featureFilesEmbedded});

            return new TestFileGeneratorResult(testGeneratorResult, generatedFeatureFileName);
        }

        public void Dispose()
        {
            _testGenerator?.Dispose();
        }

    }
}
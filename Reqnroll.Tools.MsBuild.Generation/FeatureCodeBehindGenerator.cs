using System;
using System.IO;
using Reqnroll.Generator.Interfaces;

namespace Reqnroll.Tools.MsBuild.Generation
{
    public class FeatureCodeBehindGenerator : IDisposable
    {
        private readonly ITestGenerator _testGenerator;
        private readonly GenerationSettings _generationSettings;

        public FeatureCodeBehindGenerator(ITestGenerator testGenerator, GenerationSettings generationSettings)
        {
            _testGenerator = testGenerator;
            _generationSettings = generationSettings;
        }

        public TestFileGeneratorResult GenerateCodeBehindFile(string featureFile)
        {
            var featureFileInput = new FeatureFileInput(featureFile);

            var generatedFeatureFileName = Path.GetFileName(_testGenerator.GetTestFullPath(featureFileInput));

            var testGeneratorResult = _testGenerator.GenerateTestFile(featureFileInput, _generationSettings);

            return new TestFileGeneratorResult(testGeneratorResult, generatedFeatureFileName);
        }

        public void Dispose()
        {
            _testGenerator?.Dispose();
        }

    }
}
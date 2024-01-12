using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BoDi;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Reqnroll.Generator;
using Reqnroll.Generator.Interfaces;
using Reqnroll.Generator.Project;

namespace Reqnroll.Tools.MsBuild.Generation
{
    public class FeatureCodeBehindGenerator : IDisposable
    {
        private readonly ITestGenerator _testGenerator;

        public FeatureCodeBehindGenerator(ITestGenerator testGenerator)
        {
            _testGenerator = testGenerator;
        }

        public TestFileGeneratorResult GenerateCodeBehindFile(string featureFile)
        {
            var featureFileInput = new FeatureFileInput(featureFile);

            var generatedFeatureFileName = Path.GetFileName(_testGenerator.GetTestFullPath(featureFileInput));

            var testGeneratorResult = _testGenerator.GenerateTestFile(featureFileInput, new GenerationSettings());

            return new TestFileGeneratorResult(testGeneratorResult, generatedFeatureFileName);
        }

        public void Dispose()
        {
            _testGenerator?.Dispose();
        }

    }
}
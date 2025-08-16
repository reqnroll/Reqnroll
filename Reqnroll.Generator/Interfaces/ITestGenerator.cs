using System;

namespace Reqnroll.Generator.Interfaces;

public interface ITestGenerator : IDisposable
{
    TestGeneratorResult GenerateTestFile(FeatureFileInput featureFileInput, GenerationSettings settings);
    Version DetectGeneratedTestVersion(FeatureFileInput featureFileInput);
    string GetTestFullPath(FeatureFileInput featureFileInput);
}
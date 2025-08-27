using System;

namespace Reqnroll.Generator.Interfaces;

public interface ITestGenerator : IDisposable
{
    TestGeneratorResult GenerateTestFile(FeatureFileInput featureFileInput, GenerationSettings settings);
    string GetTestFullPath(FeatureFileInput featureFileInput);
}
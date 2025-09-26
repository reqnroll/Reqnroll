namespace Reqnroll.Generator.Interfaces;

public interface ITestGenerator
{
    TestGeneratorResult GenerateTestFile(FeatureFileInput featureFileInput, GenerationSettings settings);
}
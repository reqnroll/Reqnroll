using System;
using System.Diagnostics;
using System.Linq;
using Gherkin;
using Reqnroll.Generator.Interfaces;
using Reqnroll.Parser;

namespace Reqnroll.Generator;

public abstract class ErrorHandlingTestGenerator
{
    public TestGeneratorResult GenerateTestFile(FeatureFileInput featureFileInput, GenerationSettings settings)
    {
        if (featureFileInput == null) throw new ArgumentNullException(nameof(featureFileInput));
        if (settings == null) throw new ArgumentNullException(nameof(settings));

        try
        {
            return GenerateTestFileWithExceptions(featureFileInput, settings);
        }
        catch (ParserException parserException)
        {
            return new TestGeneratorResult(parserException.GetParserExceptions().Select(
                                               ex => new TestGenerationError(ex.Location?.Line ?? 0, ex.Location?.Column ?? 0, ex.Message)));
        }
        catch (Exception exception)
        {
            return new TestGeneratorResult(new TestGenerationError(exception));
        }
    }

    public Version DetectGeneratedTestVersion(FeatureFileInput featureFileInput)
    {
        if (featureFileInput == null) throw new ArgumentNullException(nameof(featureFileInput));

        try
        {
            return DetectGeneratedTestVersionWithExceptions(featureFileInput);
        }
        catch(Exception exception)
        {
            Debug.WriteLine(exception, "ErrorHandlingTestGenerator.DetectGeneratedTestVersion");
            return null;
        }
    }

    protected abstract TestGeneratorResult GenerateTestFileWithExceptions(FeatureFileInput featureFileInput, GenerationSettings settings);
    protected abstract Version DetectGeneratedTestVersionWithExceptions(FeatureFileInput featureFileInput);

    public virtual void Dispose()
    {
        //nop;
    }
}
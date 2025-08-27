using System;
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

    protected abstract TestGeneratorResult GenerateTestFileWithExceptions(FeatureFileInput featureFileInput, GenerationSettings settings);

    public virtual void Dispose()
    {
        //nop;
    }
}
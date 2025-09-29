using System;
using System.Collections.Generic;
using System.Linq;

namespace Reqnroll.Generator.Interfaces;

public class TestGeneratorResult
{
    /// <summary>
    /// The errors, if any.
    /// </summary>
    public IEnumerable<TestGenerationError> Errors { get; }
    /// <summary>
    /// The generated test code.
    /// </summary>
    public string GeneratedTestCode { get; private set; }

    public bool Success => Errors == null || !Errors.Any();

    /// <summary>
    /// Warning messages from code generation, if any.
    /// </summary>
    public IEnumerable<string> Warnings { get; private set; }

    /// <summary>
    /// The collection of feature-level Messages.
    /// </summary>
    public string FeatureMessages { get; private set; }

    public string FeatureMessagesResourceName { get; }

    private TestGeneratorResult(IEnumerable<TestGenerationError> errors)
    {
        if (errors == null) throw new ArgumentNullException(nameof(errors));
        var errorsArray = errors.ToArray();
        if (!errorsArray.Any()) throw new ArgumentException("no errors provided", nameof(errors));

        Errors = errorsArray;
        Warnings = [];
    }

    public TestGeneratorResult(string generatedTestCode, IEnumerable<string> warnings, string featureMessages, string featureMessagesResourceName)
    {
        GeneratedTestCode = generatedTestCode;
        Warnings = warnings is null ? Array.Empty<string>() : warnings.ToList();
        FeatureMessages = featureMessages;
        FeatureMessagesResourceName = featureMessagesResourceName;
    }

    public static TestGeneratorResult FromErrors(IEnumerable<TestGenerationError> errors) => new(errors);
}
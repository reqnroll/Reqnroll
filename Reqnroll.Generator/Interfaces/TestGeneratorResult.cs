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
    /// The generated file was up-to-date.
    /// </summary>
    public bool IsUpToDate { get; private set; }
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
    /// The collection of feature-level Messages in Newline Delimited JSON (NDJSON) format.
    /// </summary>
    public string FeatureMessages { get; private set; }
    public TestGeneratorResult(params TestGenerationError[] errors)
        : this((IEnumerable<TestGenerationError>)errors)
    {
    }

    public TestGeneratorResult(IEnumerable<TestGenerationError> errors)
    {
        if (errors == null) throw new ArgumentNullException(nameof(errors));
        var errorsArray = errors.ToArray();
        if (!errorsArray.Any()) throw new ArgumentException("no errors provided", nameof(errors));

        Errors = errorsArray;
        Warnings = Array.Empty<string>();
    }

    public TestGeneratorResult(string generatedTestCode, bool isUpToDate, IEnumerable<string> warnings, string featureNdjsonMessages)
    {
        IsUpToDate = isUpToDate;
        GeneratedTestCode = generatedTestCode;
        Warnings = warnings is null ? Array.Empty<string>() : warnings.ToList();
        FeatureMessages = featureNdjsonMessages;
    }
}
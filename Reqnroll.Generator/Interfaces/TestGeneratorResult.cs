using System;
using System.Collections.Generic;
using System.Linq;

namespace Reqnroll.Generator.Interfaces
{
    /// IMPORTANT
    /// This class is used for interop with the Visual Studio Extension
    /// DO NOT REMOVE OR RENAME FIELDS!
    /// This breaks binary serialization accross appdomains
    [Serializable]
    public class TestGeneratorResult
    {
        /// <summary>
        /// The errors, if any.
        /// </summary>
        public IEnumerable<TestGenerationError> Errors { get; private set; }
        /// <summary>
        /// The generated file was up-to-date.
        /// </summary>
        public bool IsUpToDate { get; private set; }
        /// <summary>
        /// The generated test code.
        /// </summary>
        public string GeneratedTestCode { get; private set; }

        public bool Success { get { return Errors == null || !Errors.Any(); } }

        /// <summary>
        /// Warning messages from code generation, if any.
        /// </summary>
        public IEnumerable<string> Warnings { get; private set; }
        public TestGeneratorResult(params TestGenerationError[] errors)
            : this((IEnumerable<TestGenerationError>)errors)
        {
        }

        public TestGeneratorResult(IEnumerable<TestGenerationError> errors)
        {
            if (errors == null) throw new ArgumentNullException("errors");
            if (errors.Count() == 0) throw new ArgumentException("no errors provided", "errors");

            Errors = errors.ToArray();
            Warnings = new List<string>();
        }

        public TestGeneratorResult(string generatedTestCode, bool isUpToDate, IEnumerable<string> warnings)
        {
            IsUpToDate = isUpToDate;
            GeneratedTestCode = generatedTestCode;
            Warnings = warnings is null ? Array.Empty<string>() : warnings.ToList();
        }
    }
}
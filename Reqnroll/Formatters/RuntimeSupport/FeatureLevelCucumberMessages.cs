using Io.Cucumber.Messages.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Reqnroll.Formatters.RuntimeSupport;

/// <summary>
/// This class is used at Code Generation time to provide serialized representations of the Source, GherkinDocument, and Pickles 
/// to be used at runtime.
/// </summary>
public class FeatureLevelCucumberMessages
{
    private readonly Func<Source> _sourceFunc;
    private readonly Func<GherkinDocument> _gherkinDocumentFunc;
    private readonly Func<IEnumerable<Pickle>> _picklesFunc;

    private Lazy<Source> _source;
    private Lazy<GherkinDocument> _gherkinDocument;
    private Lazy<IEnumerable<Pickle>> _pickles;

    public FeatureLevelCucumberMessages(Func<Source> source, Func<GherkinDocument> gherkinDocument, Func<IEnumerable<Pickle>> pickles)
    {
        _sourceFunc = source;
        _gherkinDocumentFunc = gherkinDocument;
        _picklesFunc = pickles;

        _source = new Lazy<Source>(() => ProcessSource(_sourceFunc));
        _gherkinDocument = new Lazy<GherkinDocument>(_gherkinDocumentFunc);
        _pickles = new Lazy<IEnumerable<Pickle>>(_picklesFunc);
    }

    public bool HasMessages => Source is not null && GherkinDocument is not null && Pickles is not null;
    public Source Source { get => _source.Value; }
    public GherkinDocument GherkinDocument { get => _gherkinDocument.Value; }
    public IEnumerable<Pickle> Pickles { get => _pickles.Value; }

    internal Source ProcessSource(Func<Source> sourceGeneratorFunc)
    {
        var fromCompiler = sourceGeneratorFunc();
        if (fromCompiler != null && !String.IsNullOrEmpty(fromCompiler.Data) )
            return fromCompiler;

        // If the SourceFunc returns a Source object with an empty Data field (the source text),
        // then the source of the feature has been embedded in the assembly

        // Determine the assembly that contains the provided delegate (feature class)
        var assembly = sourceGeneratorFunc?.Method?.DeclaringType?.Assembly
                        ?? sourceGeneratorFunc?.Method?.Module?.Assembly;

        if (fromCompiler == null || string.IsNullOrEmpty(fromCompiler.Uri))
            throw new InvalidOperationException("Compiler-provided Source is missing or has no Uri.");

        return CreateSourceFromEmbeddedResource(fromCompiler.Uri.Replace('/', '\\'), assembly);
    }

    // Retrieves text from an embedded resource and returns a Cucumber Messages Source.
    // The resourceName can be the full manifest resource name or a suffix; 
    internal Source CreateSourceFromEmbeddedResource(string resourceName, Assembly assembly)
    {
        if (string.IsNullOrWhiteSpace(resourceName))
            throw new ArgumentNullException(nameof(resourceName));

        if (assembly == null)
            throw new ArgumentNullException(nameof(assembly));

        // Find and read the resource text from the most likely assemblies first
        string ReadResourceFrom(Assembly asm, string resName)
        {
            // exact match
            using (var exact = asm.GetManifestResourceStream(resName))
            {
                if (exact != null)
                    using (var sr = new StreamReader(exact))
                        return sr.ReadToEnd();
            }

            // suffix match (resource names are namespace-prefixed)
            var manifestName = asm.GetManifestResourceNames()
                                   .FirstOrDefault(n => n.Equals(resName, StringComparison.Ordinal) || n.EndsWith(resName, StringComparison.Ordinal));
            if (manifestName != null)
            {
                using (var stream = asm.GetManifestResourceStream(manifestName))
                using (var sr = new StreamReader(stream))
                {
                    return sr.ReadToEnd();
                }
            }

            return null;
        }

        var text = ReadResourceFrom(assembly, resourceName);

        if (text == null)
            throw new InvalidOperationException($"Embedded resource '{resourceName}' was not found in any loaded assembly.");

        // Convert the parsed AST into a Source message (Source contains raw text plus media type)
        return new Source(resourceName, text, SourceMediaType.TEXT_X_CUCUMBER_GHERKIN_PLAIN);
    }
}
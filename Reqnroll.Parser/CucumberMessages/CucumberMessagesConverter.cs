using System.Collections.Generic;
using System.IO;
using Gherkin.CucumberMessages;
using Io.Cucumber.Messages.Types;

namespace Reqnroll.Parser.CucumberMessages;

/// <summary>
/// Utility class that converts Reqnroll AST types in to 'CucumberMessages.Types' types.
/// It uses two classes from the Gherkin project: AstMessagesConverter and PickleCompiler
/// 
/// </summary>
public class CucumberMessagesConverter
{
    private readonly IIdGenerator _idGenerator;

    public CucumberMessagesConverter(IIdGenerator idGenerator)
    {
        _idGenerator = idGenerator;
    }

    private static string CombinePathsWithSlash(string path1, string path2)
    {
        if (string.IsNullOrEmpty(path1) || path1 == ".")
            return path2;
        if (string.IsNullOrEmpty(path2) || path2 == ".")
            return path1;

        return $"{path1}/{path2}";
    }

    /// <summary>
    /// This method transforms an AST <see cref="ReqnrollDocument"/> into a <see cref="GherkinDocument"/>.
    /// Before doing so, it patches any missing Location elements in the AST. These might be missing because our ExternalData Plugin
    /// does not emit Location elements for the Example table rows it generates.
    /// </summary>
    public GherkinDocument ConvertToCucumberMessagesGherkinDocument(ReqnrollDocument gherkinDocument)
    {
        var nullLocationPatcher = new PatchMissingLocationElementsTransformation();
        var gherkinDocumentWithLocation = nullLocationPatcher.TransformDocument(gherkinDocument);
        var converter = new AstMessagesConverter(_idGenerator);
        var location = CombinePathsWithSlash(gherkinDocument.DocumentLocation.FeatureFolderPath, Path.GetFileName(gherkinDocument.SourceFilePath));
        return converter.ConvertGherkinDocumentToEventArgs(gherkinDocumentWithLocation, location);
    }

    public static Source ConvertToCucumberMessagesSource(ReqnrollDocument gherkinDocument)
    {
        if (File.Exists(gherkinDocument.SourceFilePath))
        {
            var sourceText = File.ReadAllText(gherkinDocument.SourceFilePath);
            return new Source(
                CombinePathsWithSlash(gherkinDocument.DocumentLocation.FeatureFolderPath, Path.GetFileName(gherkinDocument.SourceFilePath)),
                sourceText,
                SourceMediaType.TEXT_X_CUCUMBER_GHERKIN_PLAIN
            );
        }

        return new Source(
            "Unknown",
            $"Source Document: {gherkinDocument.SourceFilePath} not found.",
            SourceMediaType.TEXT_X_CUCUMBER_GHERKIN_PLAIN
        );
    }

    public IEnumerable<Pickle> ConvertToCucumberMessagesPickles(GherkinDocument gherkinDocument)
    {
        var pickleCompiler = new Gherkin.CucumberMessages.Pickles.PickleCompiler(_idGenerator);
        return pickleCompiler.Compile(gherkinDocument);
    }
}
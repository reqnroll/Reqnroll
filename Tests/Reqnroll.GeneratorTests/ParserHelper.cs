using System;
using System.Linq;
using Gherkin.Ast;
using Reqnroll.Parser;

namespace Reqnroll.GeneratorTests;

class ParserHelper
{
    public static ReqnrollDocument CreateAnyDocument(string[] tags = null, ReqnrollDocumentLocation documentLocation = null)
    {
        var reqnrollFeature = new ReqnrollFeature(GetTags(tags), new Location(0), null, null, null, null, []);
        return new ReqnrollDocument(reqnrollFeature, Array.Empty<Comment>(), documentLocation ?? CreateDummyReqnrollLocation());
    }

    public static Tag[] GetTags(params string[] tags)
    {
        return tags == null ? [] : tags.Select(t => new Tag(new Location(0), t)).ToArray();
    }

    public static ReqnrollDocument CreateDocument(string[] tags = null, string[] scenarioTags = null)
    {
        tags ??= [];

        var scenario1 = new Scenario(GetTags(scenarioTags), new Location(0), "Scenario", "scenario1 title", "", Array.Empty<Step>(), Array.Empty<Examples>());

        var reqnrollFeature = new ReqnrollFeature(GetTags(tags), new Location(0), "en", "feature", "title", "desc", new StepsContainer[] {scenario1});
        return new ReqnrollDocument(reqnrollFeature, Array.Empty<Comment>(), CreateDummyReqnrollLocation());
    }

    public static ReqnrollDocument CreateDocumentWithScenarioOutline(string[] tags = null, string[] scenarioOutlineTags = null, string[] examplesTags = null)
    {
        tags ??= [];

        var scenario1 = new ScenarioOutline(GetTags(scenarioOutlineTags), new Location(0), "Scenario Outline", "scenario outline1 title", "", Array.Empty<Step>(),
        [
            new Examples(GetTags(examplesTags), default, "Examples", "examples name", "", new TableRow(default, [new TableCell(default, "col1")]), Array.Empty<TableRow>())
        ]);

        var reqnrollFeature = new ReqnrollFeature(GetTags(tags), new Location(0), "en", "feature", "title", "desc", new StepsContainer[] {scenario1});
        return new ReqnrollDocument(reqnrollFeature, Array.Empty<Comment>(), CreateDummyReqnrollLocation());
    }

    private static ReqnrollDocumentLocation CreateDummyReqnrollLocation()
    {
        return new ReqnrollDocumentLocation("dummy_location");
    }
}
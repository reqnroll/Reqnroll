using System.Linq;
using Gherkin.Ast;
using Reqnroll.Parser;

namespace Reqnroll.GeneratorTests
{
    class ParserHelper
    {
        public static ReqnrollDocument CreateAnyDocument(string[] tags = null)
        {
            var reqnrollFeature = new ReqnrollFeature(GetTags(tags), new Location(0), null, null, null, null, null);
            return new ReqnrollDocument(reqnrollFeature, new Comment[0], CreateDummyReqnrollLocation());
        }

        public static Tag[] GetTags(params string[] tags)
        {
            return tags == null ? new Tag[0] : tags.Select(t => new Tag(new Location(0), t)).ToArray();
        }

        public static ReqnrollDocument CreateDocument(string[] tags = null, string[] scenarioTags = null)
        {
            tags = tags ?? new string[0];

            var scenario1 = new Scenario(GetTags(scenarioTags), new Location(0), "Scenario", "scenario1 title", "", new Step[0], new Examples[0]);

            var reqnrollFeature = new ReqnrollFeature(GetTags(tags), new Location(0), "en", "feature", "title", "desc", new StepsContainer[] {scenario1});
            return new ReqnrollDocument(reqnrollFeature, new Comment[0], CreateDummyReqnrollLocation());
        }
        public static ReqnrollDocument CreateDocumentWithScenarioOutline(string[] tags = null, string[] scenarioOutlineTags = null, string[] examplesTags = null)
        {
            tags = tags ?? new string[0];

            var scenario1 = new ScenarioOutline(GetTags(scenarioOutlineTags), new Location(0), "Scenario Outline", "scenario outline1 title", "", new Step[0], new []
            {
                new Examples(GetTags(examplesTags), null, "Examples", "examples name", "", new Gherkin.Ast.TableRow(null, new []{ new TableCell(null, "col1"), }), new Gherkin.Ast.TableRow[0])
            });

            var reqnrollFeature = new ReqnrollFeature(GetTags(tags), new Location(0), "en", "feature", "title", "desc", new StepsContainer[] {scenario1});
            return new ReqnrollDocument(reqnrollFeature, new Comment[0], CreateDummyReqnrollLocation());
        }
        private static ReqnrollDocumentLocation CreateDummyReqnrollLocation()
        {
            return new ReqnrollDocumentLocation("dummy_location");
        }
    }
}

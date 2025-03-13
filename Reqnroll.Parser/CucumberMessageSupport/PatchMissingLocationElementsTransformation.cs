using Gherkin.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Reqnroll.Parser.CucmberMessageSupport
{
    /// <summary>
    /// This class is used to patch missing location elements for features, scenarios and scenario outlines.
    /// It is based upon the AST visitor implementation found in the External Data plugin. It may be worth finding a way to generalize the visitor base classes but this is sufficient for now.
    /// </summary>
    internal class PatchMissingLocationElementsTransformation : ScenarioTransformationVisitor
    {
        protected override void OnFeatureVisited(Feature feature)
        {
            var patchedFeatureLocation = PatchLocation(feature.Location);
            var patchedFeature = new Feature(
                feature.Tags.Select(t => new Tag(PatchLocation(t.Location), t.Name)).ToArray(),
                patchedFeatureLocation,
                feature.Language,
                feature.Keyword,
                feature.Name,
                feature.Description,
                feature.Children.ToArray());
            base.OnFeatureVisited(patchedFeature);

        }
        protected override Scenario GetTransformedScenario(Scenario scenario)
        {
            return new Scenario(
                scenario.Tags.Select(t => new Tag(PatchLocation(t.Location), t.Name)).ToArray(),
                PatchLocation(scenario.Location),
                scenario.Keyword,
                scenario.Name,
                scenario.Description,
                scenario.Steps.Select(s => new Step(PatchLocation(s.Location), s.Keyword, s.KeywordType, s.Text, s.Argument)).ToArray(),
                scenario.Examples.ToArray());
        }

        protected override Scenario GetTransformedScenarioOutline(ScenarioOutline scenarioOutline)
        {
            if (scenarioOutline.Examples == null || !scenarioOutline.Examples.Any())
                return null;

            var exampleTables = scenarioOutline.Examples;
            List<Examples> transformedExamples = new List<Examples>();

            transformedExamples.AddRange(exampleTables.Select(e => PatchExamplesLocations(e)));
            return new ScenarioOutline(
                scenarioOutline.Tags.Select(t => new Tag(PatchLocation(t.Location), t.Name)).ToArray(),
                PatchLocation(scenarioOutline.Location),
                scenarioOutline.Keyword,
                scenarioOutline.Name,
                scenarioOutline.Description,
                scenarioOutline.Steps.Select(s => new Step(PatchLocation(s.Location), s.Keyword, s.KeywordType, s.Text, s.Argument)).ToArray(),
                transformedExamples.ToArray());
        }

        private Examples PatchExamplesLocations(Examples e)
        {
            var headerCells = e.TableHeader.Cells;
            var tableHeader = new TableRow(PatchLocation(e.TableHeader.Location), headerCells.Select(hc => new TableCell(PatchLocation(hc.Location), hc.Value)).ToArray());
            var rows = e.TableBody.Select(r => new TableRow(PatchLocation(r.Location), r.Cells.Select(c => new TableCell(PatchLocation(c.Location), c.Value)).ToArray())).ToArray();
            return new Examples(e.Tags.Select(t => new Tag(PatchLocation(t.Location), t.Name)).ToArray(), PatchLocation(e.Location), e.Keyword, e.Name, e.Description, tableHeader, rows);
        }

        private static Location PatchLocation(Location l)
        {
            return l ?? new Location(0, 0);
        }


    }
}

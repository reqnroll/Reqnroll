using Gherkin.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Reqnroll.Parser
{
    internal class PatchMissingLocationElementsTransformation : ScenarioTransformationVisitor
    {
        protected override void OnFeatureVisited(Feature feature)
        {
            var patchedFeatureLocation = PatchLocation(feature.Location);
            var patchedFeature = new Feature(
                feature.Tags.ToArray(),
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
                scenario.Tags.ToArray(),
                PatchLocation(scenario.Location),
                scenario.Keyword,
                scenario.Name,
                scenario.Description,
                scenario.Steps.ToArray(),
                scenario.Examples.ToArray());
        }

        protected override Scenario GetTransformedScenarioOutline(ScenarioOutline scenarioOutline)
        {
            if (scenarioOutline.Examples == null || !scenarioOutline.Examples.Any())
                return null;

            var exampleTables = scenarioOutline.Examples;
            List<Examples> transformedExamples = new List<Examples>();

            transformedExamples.AddRange(exampleTables.Select(e => PatchLocations(e)));
            return new ScenarioOutline(
                scenarioOutline.Tags.ToArray(),
                PatchLocation(scenarioOutline.Location),
                scenarioOutline.Keyword,
                scenarioOutline.Name,
                scenarioOutline.Description,
                scenarioOutline.Steps.ToArray(),
                transformedExamples.ToArray());
        }

        private Examples PatchLocations(Examples e)
        {
            var headerCells = e.TableHeader.Cells;
            var tableHeader = new Gherkin.Ast.TableRow(PatchLocation(e.TableHeader.Location), headerCells.Select(hc => new Gherkin.Ast.TableCell(PatchLocation(hc.Location), hc.Value)).ToArray());
            var rows = e.TableBody.Select(r => new Gherkin.Ast.TableRow(PatchLocation(r.Location), r.Cells.Select(c => new Gherkin.Ast.TableCell(PatchLocation(c.Location), c.Value)).ToArray())).ToArray();
            return new Examples(e.Tags.ToArray(), PatchLocation(e.Location), e.Keyword, e.Name, e.Description, tableHeader, rows);
        }

        private static Location PatchLocation(Location l)
        {
            return l ?? new Location(0, 0);
        }


    }
}

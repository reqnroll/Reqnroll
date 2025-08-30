using System.Linq;
using Gherkin.Ast;

namespace Reqnroll.Parser.CucumberMessages;

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

        // Patch the location of the examples
        // When the examples are provided by the External Data plugin, they are not provided with a location.
        // We will use the location of the Examples table header, if it exists, or the location of the last step in the scenario outline (plus one).
        var lastExampleLocation = exampleTables.FirstOrDefault()?.TableHeader?.Location;
        var lastStepLocation = scenarioOutline.Steps.LastOrDefault()?.Location;
        var defaultLocation = lastExampleLocation != null && lastExampleLocation.Value.Line > 0 ? lastExampleLocation  // If the Examples table has a header and that header row has a location, use it
            : lastStepLocation == null ? new Location(0)  // If the ScenarioOutline has an Examples table but NO steps, use a default Location of line 0
            : new Location((int)lastStepLocation?.Line + 1); // Else use the location of the last step, plus 1 line

        var transformedExamples = exampleTables.Select(ext => PatchExamplesLocations(ext, defaultLocation)).ToArray();

        return new ScenarioOutline(
            scenarioOutline.Tags.Select(t => new Tag(PatchLocation(t.Location), t.Name)).ToArray(),
            PatchLocation(scenarioOutline.Location),
            scenarioOutline.Keyword,
            scenarioOutline.Name,
            scenarioOutline.Description,
            scenarioOutline.Steps.Select(s => new Step(PatchLocation(s.Location), s.Keyword, s.KeywordType, s.Text, s.Argument)).ToArray(),
            transformedExamples);
    }

    private Examples PatchExamplesLocations(Examples e, Location? defaultLocation)
    {
        var headerCells = e.TableHeader.Cells;
        var tableHeader = new TableRow(PatchLocation(e.TableHeader.Location, defaultLocation), headerCells.Select(hc => new TableCell(PatchLocation(hc.Location, defaultLocation), hc.Value)).ToArray());
        var rows = e.TableBody.Select(r => new TableRow(PatchLocation(r.Location, defaultLocation), r.Cells.Select(c => new TableCell(PatchLocation(c.Location, defaultLocation), c.Value)).ToArray())).ToArray();
        return new Examples(e.Tags.Select(t => new Tag(PatchLocation(t.Location, defaultLocation), t.Name)).ToArray(), PatchLocation(e.Location, defaultLocation), e.Keyword, e.Name, e.Description, tableHeader, rows);
    }

    private static Location PatchLocation(Location? l, Location? defaultLocation = null)
    {
        if (l == null  &&  defaultLocation == null)
            return new Location(0, 0);
        if (l == null || l?.Line == 0)
            return defaultLocation ?? new Location(0, 0);
        return (Location)l;
    }
}
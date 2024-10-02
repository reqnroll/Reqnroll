using Gherkin.CucumberMessages;
using Gherkin.CucumberMessages.Types;
using Io.Cucumber.Messages.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reqnroll.CucumberMessages.PayloadProcessing
{
    /// <summary>
    /// The purpose of this class is to transform Cucumber messages from the Gherkin.CucumberMessages.Types namespace to the Io.Cucumber.Messages.Types namespace
    /// 
    /// once the Gherkin project is updated to directly consume and produce Cucumber messages, this class can be removed
    /// </summary>
    internal class CucumberMessageTransformer
    {
        internal static Io.Cucumber.Messages.Types.Source ToSource(Gherkin.CucumberMessages.Types.Source gherkinSource)
        {
            var result = new Io.Cucumber.Messages.Types.Source
            (
                gherkinSource.Uri,
                gherkinSource.Data,
                gherkinSource.MediaType == "text/x.cucumber.gherkin+plain" ? SourceMediaType.TEXT_X_CUCUMBER_GHERKIN_PLAIN : SourceMediaType.TEXT_X_CUCUMBER_GHERKIN_MARKDOWN
            );
            return result;
        }

        internal static Io.Cucumber.Messages.Types.GherkinDocument ToGherkinDocument(Gherkin.CucumberMessages.Types.GherkinDocument gherkinDoc)
        {
            var result = new Io.Cucumber.Messages.Types.GherkinDocument
            (
                gherkinDoc.Uri,
                ToFeature(gherkinDoc.Feature),
                ToComments(gherkinDoc.Comments)
            );
            return result;
        }

        private static Io.Cucumber.Messages.Types.Feature ToFeature(Gherkin.CucumberMessages.Types.Feature feature)
        {
            if (feature == null)
            {
                return null;
            }

            var children = feature.Children.Select(ToFeatureChild).ToList();
            var tags = feature.Tags.Select(ToTag).ToList();

            return new Io.Cucumber.Messages.Types.Feature(
                ToLocation(feature.Location),
                tags,
                feature.Language,
                feature.Keyword,
                feature.Name,
                feature.Description,
                children)
            ;
        }

        private static Io.Cucumber.Messages.Types.Location ToLocation(Gherkin.CucumberMessages.Types.Location location)
        {
            if (location == null)
            {
                return null;
            }
            return new Io.Cucumber.Messages.Types.Location(location.Line, location.Column);
        }


        private static Io.Cucumber.Messages.Types.Tag ToTag(Gherkin.CucumberMessages.Types.Tag tag)
        {
            if (tag == null)
            {
                return null;
            }
            return new Io.Cucumber.Messages.Types.Tag(ToLocation(tag.Location), tag.Name, tag.Id);
        }

        private static Io.Cucumber.Messages.Types.FeatureChild ToFeatureChild(Gherkin.CucumberMessages.Types.FeatureChild child)
        {
            if (child == null)
            {
                return null;
            }
            return new Io.Cucumber.Messages.Types.FeatureChild
            (
                ToRule(child.Rule),
                ToBackground(child.Background),
                ToScenario(child.Scenario)
            );
        }

        private static Io.Cucumber.Messages.Types.Scenario ToScenario(Gherkin.CucumberMessages.Types.Scenario scenario)
        {
            if (scenario == null)
            {
                return null;
            }

            return new Io.Cucumber.Messages.Types.Scenario
            (
                ToLocation(scenario.Location),
                scenario.Tags.Select(ToTag).ToList(),
                scenario.Keyword,
                scenario.Name,
                scenario.Description,
                scenario.Steps.Select(ToStep).ToList(),
                scenario.Examples.Select(ToExamples).ToList(),
                scenario.Id
            );
        }

        private static Io.Cucumber.Messages.Types.Examples ToExamples(Gherkin.CucumberMessages.Types.Examples examples)
        {
            if (examples == null)
            {
                return null;
            }

            return new Io.Cucumber.Messages.Types.Examples(
                ToLocation(examples.Location),
                examples.Tags.Select(ToTag).ToList(),
                examples.Keyword,
                examples.Name,
                examples.Description,
                ToTableRow(examples.TableHeader),
                examples.TableBody.Select(ToTableRow).ToList(),
                examples.Id
            );
        }
        private static Io.Cucumber.Messages.Types.TableCell ToTableCell(Gherkin.CucumberMessages.Types.TableCell cell)
        {
            return new Io.Cucumber.Messages.Types.TableCell(
                ToLocation(cell.Location),
                cell.Value
            );
        }

        private static Io.Cucumber.Messages.Types.TableRow ToTableRow(Gherkin.CucumberMessages.Types.TableRow row)
        {
            return new Io.Cucumber.Messages.Types.TableRow(
                ToLocation(row.Location),
                row.Cells.Select(ToTableCell).ToList(),
                row.Id
            );
        }
        private static Io.Cucumber.Messages.Types.Step ToStep(Gherkin.CucumberMessages.Types.Step step)
        {
            if (step == null)
            {
                return null;
            }

            return new Io.Cucumber.Messages.Types.Step(
                ToLocation(step.Location),
                step.Keyword,
                ToKeyWordType(step.KeywordType),
                step.Text,
                step.DocString == null ? null : ToDocString(step.DocString),
                step.DataTable == null ? null : ToDataTable(step.DataTable),
                step.Id
            );
        }

        private static Io.Cucumber.Messages.Types.Background ToBackground(Gherkin.CucumberMessages.Types.Background background)
        {
            if (background == null)
            {
                return null;
            }
            return new Io.Cucumber.Messages.Types.Background(
                ToLocation(background.Location),
                background.Keyword,
                background.Name,
                background.Description,
                background.Steps.Select(ToStep).ToList(),
                background.Id
            );
        }

        private static Io.Cucumber.Messages.Types.Rule ToRule(Gherkin.CucumberMessages.Types.Rule rule)
        {
            if (rule == null)
            {
                return null;
            }
            return new Io.Cucumber.Messages.Types.Rule(
                ToLocation(rule.Location),
                rule.Tags.Select(ToTag).ToList(),
                rule.Keyword,
                rule.Name,
                rule.Description,
                rule.Children.Select(ToRuleChild).ToList(),
                rule.Id
            );
        }

        private static Io.Cucumber.Messages.Types.RuleChild ToRuleChild(Gherkin.CucumberMessages.Types.RuleChild child)
        {
            return new Io.Cucumber.Messages.Types.RuleChild(
                ToBackground(child.Background),
                ToScenario(child.Scenario)
            );
        }

        private static List<Io.Cucumber.Messages.Types.Comment> ToComments(IReadOnlyCollection<Gherkin.CucumberMessages.Types.Comment> comments)
        {
            return comments.Select(ToComment).ToList();
        }

        private static Io.Cucumber.Messages.Types.Comment ToComment(Gherkin.CucumberMessages.Types.Comment comment)
        {
            return new Io.Cucumber.Messages.Types.Comment(
                ToLocation(comment.Location),
                comment.Text
            );
        }
        private static StepKeywordType ToKeyWordType(Gherkin.StepKeywordType keywordType)
        {
            return keywordType switch
            {
                //case Gherkin.StepKeywordType.Unspecified:
                //    return Io.Cucumber.Messages.Types.StepKeywordType.UNKNOWN;
                Gherkin.StepKeywordType.Context => StepKeywordType.CONTEXT,
                Gherkin.StepKeywordType.Conjunction => StepKeywordType.CONJUNCTION,
                Gherkin.StepKeywordType.Action => StepKeywordType.ACTION,
                Gherkin.StepKeywordType.Outcome => StepKeywordType.OUTCOME,
                Gherkin.StepKeywordType.Unknown => StepKeywordType.UNKNOWN,
                _ => throw new ArgumentException($"Invalid keyword type: {keywordType}"),
            };
        }

        private static Io.Cucumber.Messages.Types.DocString ToDocString(Gherkin.CucumberMessages.Types.DocString docString)
        {
            return new Io.Cucumber.Messages.Types.DocString(
                ToLocation(docString.Location),
                docString.MediaType,
                docString.Content,
                docString.Delimiter
            );
        }

        private static Io.Cucumber.Messages.Types.DataTable ToDataTable(Gherkin.CucumberMessages.Types.DataTable dataTable)
        {
            return new Io.Cucumber.Messages.Types.DataTable(
                ToLocation(dataTable.Location),
                dataTable.Rows.Select(ToTableRow).ToList()
            );
        }

        internal static List<Io.Cucumber.Messages.Types.Pickle> ToPickles(IReadOnlyCollection<Gherkin.CucumberMessages.Types.Pickle> pickles)
        {
            return pickles.Select(ToPickle).ToList();
        }

        private static Io.Cucumber.Messages.Types.Pickle ToPickle(Gherkin.CucumberMessages.Types.Pickle pickle)
        {
            return new Io.Cucumber.Messages.Types.Pickle(
                pickle.Id,
                pickle.Uri,
                pickle.Name,
                pickle.Language,
                pickle.Steps.Select(ToPickleStep).ToList(),
                pickle.Tags.Select(ToPickleTag).ToList(),
                pickle.AstNodeIds.ToList()
            );
        }
        private static Io.Cucumber.Messages.Types.PickleTag ToPickleTag(Gherkin.CucumberMessages.Types.PickleTag pickleTag)
        {
            return new Io.Cucumber.Messages.Types.PickleTag(
                pickleTag.Name,
                pickleTag.AstNodeId
            );
        }
        private static Io.Cucumber.Messages.Types.PickleStep ToPickleStep(Gherkin.CucumberMessages.Types.PickleStep pickleStep)
        {
            return new Io.Cucumber.Messages.Types.PickleStep(
                ToPickleStepArgument(pickleStep.Argument),
                pickleStep.AstNodeIds.ToList(),
                pickleStep.Id,
                ToPickleStepType(pickleStep.Type),
                pickleStep.Text
            );
        }
        private static Io.Cucumber.Messages.Types.PickleStepArgument ToPickleStepArgument(Gherkin.CucumberMessages.Types.PickleStepArgument pickleStepArgument)
        {
            if (pickleStepArgument == null)
            {
                return null;
            }
            return new Io.Cucumber.Messages.Types.PickleStepArgument(
                ToPickleDocString(pickleStepArgument.DocString),
                ToPickleTable(pickleStepArgument.DataTable)
            );
        }

        private static PickleStepType ToPickleStepType(Gherkin.StepKeywordType pickleStepType)
        {
            return pickleStepType switch
            {
                Gherkin.StepKeywordType.Unknown => PickleStepType.UNKNOWN,
                Gherkin.StepKeywordType.Action => PickleStepType.ACTION,
                Gherkin.StepKeywordType.Outcome => PickleStepType.OUTCOME,
                Gherkin.StepKeywordType.Context => PickleStepType.CONTEXT,
                _ => throw new ArgumentException($"Invalid pickle step type: {pickleStepType}")
            };
        }
        private static Io.Cucumber.Messages.Types.PickleDocString ToPickleDocString(Gherkin.CucumberMessages.Types.PickleDocString pickleDocString)
        {
            if (pickleDocString == null)
            {
                return null;
            }
            return new Io.Cucumber.Messages.Types.PickleDocString(
                pickleDocString.MediaType,
                pickleDocString.Content
            );
        }

        private static Io.Cucumber.Messages.Types.PickleTable ToPickleTable(Gherkin.CucumberMessages.Types.PickleTable pickleTable)
        {
            if (pickleTable == null)
            {
                return null;
            }
            return new Io.Cucumber.Messages.Types.PickleTable(
                pickleTable.Rows.Select(ToPickleTableRow).ToList()
            );
        }

        private static Io.Cucumber.Messages.Types.PickleTableRow ToPickleTableRow(Gherkin.CucumberMessages.Types.PickleTableRow pickleTableRow)
        {
            return new Io.Cucumber.Messages.Types.PickleTableRow(
                pickleTableRow.Cells.Select(ToPickleTableCell).ToList()
            );
        }

        private static Io.Cucumber.Messages.Types.PickleTableCell ToPickleTableCell(Gherkin.CucumberMessages.Types.PickleTableCell pickleTableCell)
        {
            return new Io.Cucumber.Messages.Types.PickleTableCell(
                pickleTableCell.Value
            );
        }
    }
}
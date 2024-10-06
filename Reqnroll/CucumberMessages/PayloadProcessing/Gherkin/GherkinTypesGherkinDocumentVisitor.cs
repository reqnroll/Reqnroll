using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Gherkin.CucumberMessages.Types;

namespace Reqnroll.CucumberMessages.PayloadProcessing.Gherkin
{
    /// <summary>
    /// Absstract base class for implementing a visitor for <see cref="GherkinDocument"/>
    /// </summary>
    abstract class GherkinTypesGherkinDocumentVisitor
    {
        protected virtual void AcceptDocument(GherkinDocument document)
        {
            OnDocumentVisiting(document);
            if (document.Feature != null)
            {
                AcceptFeature(document.Feature);
            }
            OnDocumentVisited(document);
        }

        protected virtual void AcceptFeature(Feature feature)
        {
            OnFeatureVisiting(feature);
            foreach (var tag in feature.Tags)
                AcceptTag(tag);

            foreach (var featureChild in feature.Children)
            {
                if (featureChild.Rule != null) AcceptRule(featureChild.Rule);
                else if (featureChild.Background != null) AcceptBackground(featureChild.Background);
                else if (featureChild.Scenario != null && IsScenarioOutline(featureChild.Scenario)) AcceptScenarioOutline(featureChild.Scenario);
                else if (featureChild.Scenario != null) AcceptScenario(featureChild.Scenario);
            }
            OnFeatureVisited(feature);
        }
        private bool IsScenarioOutline(Scenario scenario)
        {
            return scenario.Examples != null && scenario.Examples.Any();
        }
        protected virtual void AcceptStep(Step step)
        {
            OnStepVisiting(step);
            if (step.DataTable != null)
                AcceptDataTable(step.DataTable);
            OnStepVisited(step);
        }

        protected virtual void AcceptDataTable(global::Gherkin.CucumberMessages.Types.DataTable dataTable)
        {
            OnDataTableVisiting(dataTable);
            foreach (var row in dataTable.Rows)
            {
                AcceptTableRow(row);
            }
            OnDataTableVisited(dataTable);
        }

        protected virtual void AcceptScenario(Scenario scenario)
        {
            OnScenarioVisiting(scenario);
            NavigateScenarioInner(scenario);
            OnScenarioVisited(scenario);
        }

        private void NavigateScenarioInner(Scenario scenario)
        {
            foreach (var tag in scenario.Tags)
            {
                AcceptTag(tag);
            }
            foreach (var step in scenario.Steps)
            {
                AcceptStep(step);
            }
        }

        protected virtual void AcceptScenarioOutline(Scenario scenarioOutline)
        {
            OnScenarioOutlineVisiting(scenarioOutline);
            foreach (var examples in scenarioOutline.Examples)
            {
                AcceptExamples(examples);
            }

            NavigateScenarioInner(scenarioOutline);
            OnScenarioOutlineVisited(scenarioOutline);
        }

        protected virtual void AcceptBackground(Background background)
        {
            OnBackgroundVisiting(background);
            foreach (var step in background.Steps)
            {
                AcceptStep(step);
            }
            OnBackgroundVisited(background);
        }

        protected virtual void AcceptRule(Rule rule)
        {
            OnRuleVisiting(rule);
            foreach (var tag in rule.Tags)
                AcceptTag(tag);

            foreach (var ruleChild in rule.Children)
            {
                if (ruleChild.Background != null) AcceptBackground(ruleChild.Background);
                else if (ruleChild.Scenario != null && IsScenarioOutline(ruleChild.Scenario)) AcceptScenarioOutline(ruleChild.Scenario);
                else if (ruleChild.Scenario != null) AcceptScenario(ruleChild.Scenario);
            }
            OnRuleVisited(rule);
        }

        protected virtual void AcceptTableRow(TableRow row)
        {
            OnTableRowVisited(row);
        }

        protected virtual void AcceptTag(Tag tag)
        {
            OnTagVisited(tag);
        }

        protected virtual void AcceptExamples(Examples examples)
        {
            OnExamplesVisiting(examples);
            foreach (var tag in examples.Tags)
                AcceptTag(tag);
            AcceptTableHeader(examples.TableHeader);
            foreach (var row in examples.TableBody)
                AcceptTableRow(row);
            OnExamplesVisited(examples);
        }

        protected virtual void AcceptTableHeader(TableRow header)
        {
            OnTableHeaderVisited(header);
        }

        protected virtual void OnDocumentVisiting(GherkinDocument document)
        {
            //nop
        }

        protected virtual void OnDocumentVisited(GherkinDocument document)
        {
            //nop
        }

        protected virtual void OnFeatureVisiting(Feature feature)
        {
            //nop
        }

        protected virtual void OnFeatureVisited(Feature feature)
        {
            //nop
        }

        protected virtual void OnBackgroundVisiting(Background background)
        {
            //nop
        }

        protected virtual void OnBackgroundVisited(Background background)
        {
            //nop
        }

        protected virtual void OnRuleVisiting(Rule rule)
        {
            //nop
        }

        protected virtual void OnRuleVisited(Rule rule)
        {
            //nop
        }

        protected virtual void OnScenarioOutlineVisiting(Scenario scenarioOutline)
        {
            //nop
        }

        protected virtual void OnScenarioOutlineVisited(Scenario scenarioOutline)
        {
            //nop
        }

        protected virtual void OnScenarioVisiting(Scenario scenario)
        {
            //nop
        }

        protected virtual void OnScenarioVisited(Scenario scenario)
        {
            //nop
        }

        protected virtual void OnStepVisiting(Step step)
        {
            //nop
        }

        protected virtual void OnStepVisited(Step step)
        {
            //nop
        }

        protected virtual void OnDataTableVisiting(global::Gherkin.CucumberMessages.Types.DataTable dataTable)
        {
            //nop
        }

        protected virtual void OnDataTableVisited(global::Gherkin.CucumberMessages.Types.DataTable dataTable)
        {
            //nop
        }

        protected virtual void OnTableRowVisited(TableRow row)
        {
            //nop
        }

        protected virtual void OnTagVisited(Tag tag)
        {
            //nop
        }

        protected virtual void OnExamplesVisiting(Examples examples)
        {
            //nop
        }

        protected virtual void OnExamplesVisited(Examples examples)
        {
            //nop
        }

        protected virtual void OnTableHeaderVisited(TableRow header)
        {
            //nop
        }
    }
}

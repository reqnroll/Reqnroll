using System;
using System.Linq;
using Gherkin.Ast;

namespace Reqnroll.CucumberMessages.PayloadProcessing.Gherkin
{
    abstract class GherkinDocumentVisitor
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
            foreach (var featureChild in feature.Children)
            {
                if (featureChild is Rule rule) AcceptRule(rule);
                else if (featureChild is Background background) AcceptBackground(background);
                else if (featureChild is Scenario scenarioOutline && scenarioOutline.Examples != null && scenarioOutline.Examples.Count() > 0) AcceptScenarioOutline(scenarioOutline);
                else if (featureChild is Scenario scenario) AcceptScenario(scenario);
            }
            OnFeatureVisited(feature);
        }

        protected virtual void AcceptStep(Step step)
        {
            OnStepVisited(step);
        }

        protected virtual void AcceptScenario(Scenario scenario)
        {
            OnScenarioVisiting(scenario);
            foreach (var step in scenario.Steps)
            {
                AcceptStep(step);
            }
            OnScenarioVisited(scenario);
        }

        protected virtual void AcceptScenarioOutline(Scenario scenarioOutline)
        {
            OnScenarioOutlineVisiting(scenarioOutline);
            foreach (var step in scenarioOutline.Steps)
            {
                AcceptStep(step);
            }
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
            foreach (var ruleChild in rule.Children)
            {
                if (ruleChild is Background background) AcceptBackground(background);
                else if (ruleChild is Scenario scenarioOutline && scenarioOutline.Examples != null && scenarioOutline.Examples.Count() > 0) AcceptScenarioOutline(scenarioOutline);
                else if (ruleChild is Scenario scenario) AcceptScenario(scenario);
            }
            OnRuleVisited(rule);
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

        protected virtual void OnStepVisited(Step step)
        {
            //nop
        }
    }
}

using Gherkin.CucumberMessages;
using Gherkin.CucumberMessages.Types;
using Reqnroll.CucumberMessages.Configuration;
using Reqnroll.CucumberMessages.RuntimeSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Reqnroll.CucumberMessages.PayloadProcessing.Gherkin
{
    /// <summary>
    /// The ID Generation Style is configurable (by either the config file or by Environment Variable override).
    /// 
    /// This class confirms that the ID Style that was used during code generation is consistent 
    /// with that which is configured to be used during TEST execution.
    /// While it's not likely they would be different, it's possible.
    /// 
    /// If they are possible, we use a visitor pattern to re-write the IDs to the test-time chosen style.
    /// </summary>
    internal class GherkinDocumentIDStyleReWriter : GherkinTypesGherkinDocumentVisitor
    {
        private IIdGenerator _idGenerator;
        public Dictionary<string, string> IdMap = new();

        public GherkinDocument ReWriteIds(GherkinDocument document, IDGenerationStyle targetStyle)
        {
            var existingIdStyle = ProbeForIdGenerationStyle(document);

            if (existingIdStyle == targetStyle)
                return document;

            _idGenerator = IdGeneratorFactory.Create(targetStyle);

            AcceptDocument(document);
            return document;
        }

        private IDGenerationStyle ProbeForIdGenerationStyle(GherkinDocument document)
        {
            if (document.Feature == null) return IDGenerationStyle.UUID;
            var child = document.Feature.Children.FirstOrDefault();
            if (child == null) return IDGenerationStyle.UUID;

            if (child.Rule != null)
                return ParseStyle(child.Rule.Id);

            if (child.Background != null)
                return ParseStyle(child.Background.Id);

            if (child.Scenario != null)
                return ParseStyle(child.Scenario.Id);

            return IDGenerationStyle.UUID;
        }

        private IDGenerationStyle ParseStyle(string id)
        {
            if (Guid.TryParse(id, out var _)) 
                return IDGenerationStyle.UUID;

            return IDGenerationStyle.Incrementing;
        }

        protected override void OnTagVisited(Tag tag)
        {
            base.OnTagVisited(tag);
            var oldId = tag.Id;
            var newId = _idGenerator.GetNewId();
            IdMap[oldId] = newId;
            tag.Id = newId;
        }
        protected override void OnScenarioOutlineVisited(Scenario scenarioOutline)
        {
            base.OnScenarioOutlineVisited(scenarioOutline);
            var oldId = scenarioOutline.Id;
            var newId = _idGenerator.GetNewId();
            IdMap[oldId] = newId;
            scenarioOutline.Id = newId;
        }

        protected override void OnScenarioVisited(Scenario scenario)
        {
            base.OnScenarioVisited(scenario);
            var oldId = scenario.Id;
            var newId = _idGenerator.GetNewId();
            IdMap[oldId] = newId;
            scenario.Id = newId;
        }

        protected override void OnRuleVisited(Rule rule)
        {
            base.OnRuleVisited(rule);
            var oldId = rule.Id;
            var newId = _idGenerator.GetNewId();
            IdMap[oldId] = newId;
            rule.Id = newId;
        }
        protected override void OnBackgroundVisited(Background background)
        {
            base.OnBackgroundVisited(background);
            var oldId = background.Id;
            var newId = _idGenerator.GetNewId();
            IdMap[oldId] = newId;
            background.Id = newId;

        }
        protected override void OnStepVisited(Step step)
        {
            base.OnStepVisited(step);
            var oldId = step.Id;
            var newId = _idGenerator.GetNewId();
            IdMap[oldId] = newId;
            step.Id = newId;
        }
        protected override void OnExamplesVisited(Examples examples)
        {
            base.OnExamplesVisited(examples);
            var oldId = examples.Id;
            var newId = _idGenerator.GetNewId();
            IdMap[oldId] = newId;
            examples.Id = newId;
        }
        protected override void OnTableHeaderVisited(TableRow header)
        {
            base.OnTableHeaderVisited(header);
            var oldId = header.Id;
            var newId = _idGenerator.GetNewId();
            IdMap[oldId] = newId;
            header.Id = newId;
        }
        protected override void OnTableRowVisited(TableRow row)
        {
            base.OnTableRowVisited(row);
            var oldId = row.Id;
            var newId = _idGenerator.GetNewId();
            IdMap[oldId] = newId;
            row.Id = newId;
        }
    }
}

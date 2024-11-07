﻿using Gherkin.CucumberMessages;
using Io.Cucumber.Messages.Types;
using Reqnroll.CucumberMessages.Configuration;
using Reqnroll.CucumberMessages.PayloadProcessing.Cucumber;
using Reqnroll.CucumberMessages.RuntimeSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    /// If they are different, we use a visitor pattern to re-write the IDs to the run-time chosen style.
    /// 
    /// If the styles are the same AND we're using an incrementing generator, then we need to regenerate with the next series of IDs.
    /// 
    /// </summary>
    internal class GherkinDocumentIDReWriter : CucumberMessage_TraversalVisitorBase
    {
        internal static object _sharedLockObject = new();

        private IIdGenerator _idGenerator;
        public Dictionary<string, string> IdMap = new();
        public GherkinDocumentIDReWriter(IIdGenerator idGenerator)
        {
            _idGenerator = idGenerator;
        }
        public GherkinDocument ReWriteIds(GherkinDocument document, IDGenerationStyle targetStyle)
        {
            var existingIdStyle = ProbeForIdGenerationStyle(document);

            if (targetStyle == IDGenerationStyle.Incrementing)
            {
                switch ((((SeedableIncrementingIdGenerator)_idGenerator).HasBeenUsed, existingIdStyle))
                {
                    case (true, IDGenerationStyle.Incrementing):
                    case (true, IDGenerationStyle.UUID):
                    case (false, IDGenerationStyle.UUID):
                        return ReWrite(document);

                    case (false, IDGenerationStyle.Incrementing):
                        var lastId = ProbeForLastUsedId(document);
                        var anotherThreadSetTheSeed = false;
                        lock (_sharedLockObject)
                        {
                            if (((SeedableIncrementingIdGenerator)_idGenerator).HasBeenUsed)
                                anotherThreadSetTheSeed = true;
                            else
                            {
                                ((SeedableIncrementingIdGenerator)_idGenerator).SetSeed(lastId);
                            }
                        }

                        if (anotherThreadSetTheSeed)
                            return ReWrite(document);
                        return document;
                }
            }
            // else targetStyle is IDGenerationStyle.UUID
            return document;
        }

        private GherkinDocument ReWrite(GherkinDocument document)
        {
            Visit(document);
            return document;
        }

        private int ProbeForLastUsedId(GherkinDocument document)
        {
            if (document.Feature == null) return 0;

            var child = document.Feature.Children.LastOrDefault();
            var tags = document.Feature.Tags;
            var highestTagId = tags.Count > 0 ? tags.Max(t => int.Parse(t.Id)) : 0;

            if (child == null) return highestTagId;

            if (child.Rule != null)
                highestTagId = Math.Max(highestTagId, int.Parse(child.Rule.Id));

            if (child.Background != null)
                highestTagId = Math.Max(highestTagId, int.Parse(child.Background.Id));

            if (child.Scenario != null)
                highestTagId = Math.Max(highestTagId, int.Parse(child.Scenario.Id));

            return highestTagId;
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

        public override void OnVisited(Tag tag)
        {
            base.OnVisited(tag);
            var oldId = tag.Id;
            var newId = _idGenerator.GetNewId();
            IdMap[oldId] = newId;
            //tag.Id = newId;
            SetPrivateProperty<Tag>(tag, "Id", newId);
        }

        public override void OnVisited(Scenario scenario)
        {
            base.OnVisited(scenario);
            var oldId = scenario.Id;
            var newId = _idGenerator.GetNewId();
            IdMap[oldId] = newId;
            //scenario.Id = newId;

            SetPrivateProperty<Scenario>(scenario, "Id", newId);
        }

        public override void OnVisited(Rule rule)
        {
            base.OnVisited(rule);
            var oldId = rule.Id;
            var newId = _idGenerator.GetNewId();
            IdMap[oldId] = newId;
            //rule.Id = newId;
            SetPrivateProperty<Rule>(rule, "Id", newId);
        }
        public override void OnVisited(Background background)
        {
            base.OnVisited(background);
            var oldId = background.Id;
            var newId = _idGenerator.GetNewId();
            IdMap[oldId] = newId;
            //background.Id = newId;

            SetPrivateProperty<Background>(background, "Id", newId);

        }
        public override void OnVisited(Step step)
        {
            base.OnVisited(step);
            var oldId = step.Id;
            var newId = _idGenerator.GetNewId();
            IdMap[oldId] = newId;
            //step.Id = newId;

            SetPrivateProperty<Step>(step, "Id", newId);
        }
        public override void OnVisited(Examples examples)
        {
            base.OnVisited(examples);
            var oldId = examples.Id;
            var newId = _idGenerator.GetNewId();
            IdMap[oldId] = newId;
            //examples.Id = newId;

            SetPrivateProperty<Examples>(examples, "Id", newId);
        }
        public override void OnVisited(TableRow row)
        {
            base.OnVisited(row);
            var oldId = row.Id;
            var newId = _idGenerator.GetNewId();
            IdMap[oldId] = newId;
            //row.Id = newId;

            SetPrivateProperty<TableRow>(row, "Id", newId);
        }
        public static void SetPrivateProperty<T>(T instance, string propertyName, string newValue)
        {
            // Get the PropertyInfo object for the property
            PropertyInfo propInfo = typeof(T).GetProperty(propertyName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (propInfo == null)
            {
                throw new ArgumentException($"Property {propertyName} not found.");
            }

            // Get the SetMethod (setter) of the property
            MethodInfo setMethod = propInfo.GetSetMethod(true);

            if (setMethod == null)
            {
                throw new ArgumentException($"Property {propertyName} does not have a setter.");
            }

            // Invoke the setter method to set the new value
            setMethod.Invoke(instance, new object[] { newValue });
        }
    }
}
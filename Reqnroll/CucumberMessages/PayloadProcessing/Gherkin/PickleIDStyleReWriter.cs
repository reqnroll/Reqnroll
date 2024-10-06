using Gherkin.CucumberMessages;
using Gherkin.CucumberMessages.Types;
using Reqnroll.CucumberMessages.Configuration;
using Reqnroll.CucumberMessages.RuntimeSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Reqnroll.CucumberMessages.PayloadProcessing.Gherkin
{
    /// <summary>
    /// If ID rewriting is required (see cref="GherkinDocumentIDStyleReWriter"), 
    /// this class will re-write the IDs in the given <see cref="Pickle"/>s.
    /// </summary>
    internal class PickleIDStyleReWriter : GherkinTypesPickleVisitor
    {
        private Dictionary<string, string> _idMap;
        private IEnumerable<Pickle> _originalPickles;
        private IDGenerationStyle _idStyle;
        private IIdGenerator _idGenerator;

        public IEnumerable<Pickle> ReWriteIds(IEnumerable<Pickle> pickles, Dictionary<string, string> idMap, IDGenerationStyle targetStyle)
        {
            if (pickles == null || pickles.Count() == 0) return pickles;

            _idMap = idMap;
            _originalPickles = pickles;
            _idStyle = targetStyle;
            var existingIdStyle = ProbeForIdGenerationStyle(pickles.First());

            if (existingIdStyle == targetStyle)
                return pickles;

            _idGenerator = IdGeneratorFactory.Create(targetStyle);


            foreach (var pickle in _originalPickles)
            {
                AcceptPickle(pickle);
            }

            return _originalPickles;
        }

        private IDGenerationStyle ProbeForIdGenerationStyle(Pickle pickle)
        {
            if (Guid.TryParse(pickle.Id, out var _))
                return IDGenerationStyle.UUID;

            return IDGenerationStyle.Incrementing;
        }

        protected override void OnVisitedPickle(Pickle pickle)
        {
            base.OnVisitedPickle(pickle);

            if (_idMap.TryGetValue(pickle.Id, out var newId))
                pickle.Id = newId;
            pickle.AstNodeIds = pickle.AstNodeIds.Select(id => _idMap.TryGetValue(id, out var newId2) ? newId2 : id).ToList().AsReadOnly();
        }

        protected override void OnVisitedPickleStep(PickleStep step)
        {
            base.OnVisitedPickleStep(step);
            if (_idMap.TryGetValue(step.Id, out var newId))
                step.Id = newId;
            step.AstNodeIds = step.AstNodeIds.Select(id => _idMap.TryGetValue(id, out var newId2) ? newId2 : id).ToList().AsReadOnly();
        }

        protected override void OnVisitedPickleTag(PickleTag tag)
        {
            base.OnVisitedPickleTag(tag);

            if (_idMap.TryGetValue(tag.AstNodeId, out var newId))
                tag.AstNodeId = newId;
        }
    }
}

using Gherkin.CucumberMessages;
using Io.Cucumber.Messages.Types;
using Reqnroll.CucumberMessages.Configuration;
using Reqnroll.CucumberMessages.PayloadProcessing.Cucumber;
using Reqnroll.CucumberMessages.RuntimeSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace Reqnroll.CucumberMessages.PayloadProcessing.Gherkin
{
    /// <summary>
    /// If ID rewriting is required (see cref="GherkinDocumentIDReWriter"), 
    /// this class will re-write the IDs in the given <see cref="Pickle"/>s.
    /// </summary>
    internal class PickleIDReWriter : CucumberMessage_TraversalVisitorBase
    {
        private Dictionary<string, string> _idMap;
        private IEnumerable<Pickle> _originalPickles;
        private IDGenerationStyle _targetIdStyle;
        private IDGenerationStyle _existingIdStyle;
        private IIdGenerator _idGenerator;

        public PickleIDReWriter(IIdGenerator idGenerator)
        {
            _idGenerator = idGenerator;
        }

        public IEnumerable<Pickle> ReWriteIds(IEnumerable<Pickle> pickles, Dictionary<string, string> idMap, IDGenerationStyle targetStyle)
        {
            if (pickles == null || pickles.Count() == 0) return pickles;

            _idMap = idMap;
            _originalPickles = pickles;
            _targetIdStyle = targetStyle;
            _existingIdStyle = ProbeForIdGenerationStyle(pickles.First());

            if (_existingIdStyle == IDGenerationStyle.UUID && targetStyle == IDGenerationStyle.UUID)
                return pickles;

            //re-write the IDs (either int->UUID or UUID->int or int->int starting at a new seed)
            foreach (var pickle in _originalPickles)
            {
                Accept(pickle);
            }

            return _originalPickles;
        }

        private IDGenerationStyle ProbeForIdGenerationStyle(Pickle pickle)
        {
            if (Guid.TryParse(pickle.Id, out var _))
                return IDGenerationStyle.UUID;

            return IDGenerationStyle.Incrementing;
        }

        public override void OnVisited(Pickle pickle)
        {
            base.OnVisited(pickle);

            
            SetPrivateProperty(pickle, "Id", _idGenerator.GetNewId()); //pickle.Id = newId;

            // if the AstNodeIds are in the idMap, that means they were rewrittten by the GerkinDocumentIDReWriter
            // otherwise, we can continue to use the ID we already have
            var mappedAstIds = pickle.AstNodeIds.Select(id => _idMap.TryGetValue(id, out var newId2) ? newId2 : id).ToList();
            pickle.AstNodeIds.Clear();
            pickle.AstNodeIds.AddRange(mappedAstIds);
        }

        public override void OnVisited(PickleStep step)
        {
            base.OnVisited(step);
            SetPrivateProperty(step, "Id", _idGenerator.GetNewId()); //step.Id = newId;y
            var mappedAstIds = step.AstNodeIds.Select(id => _idMap.TryGetValue(id, out var newId2) ? newId2 : id).ToList();
            step.AstNodeIds.Clear();
            step.AstNodeIds.AddRange(mappedAstIds);
        }

        public override void OnVisited(PickleTag tag)
        {
            base.OnVisited(tag);

            if (_idMap.TryGetValue(tag.AstNodeId, out var newId))
                SetPrivateProperty(tag, "AstNodeId", newId); //tag.AstNodeId = newId;
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

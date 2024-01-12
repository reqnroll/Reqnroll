using System;
using System.Collections.Generic;
using System.Linq;
using Reqnroll.Assist.Attributes;

namespace Reqnroll.Bindings.Discovery
{
    public class ReqnrollAttributesFilter : IReqnrollAttributesFilter
    {
        private readonly IReadOnlyCollection<Type> _validAttributeTypes = new[]
        {
            typeof(BindingAttribute),
            typeof(HookAttribute),
            typeof(StepDefinitionBaseAttribute),
            typeof(StepArgumentTransformationAttribute),
            typeof(TableAliasesAttribute),
            typeof(ScopeAttribute),
            typeof(BeforeTestRunAttribute),
            typeof(AfterTestRunAttribute),
            typeof(BeforeFeatureAttribute),
            typeof(AfterFeatureAttribute),
            typeof(BeforeScenarioAttribute),
            typeof(AfterScenarioAttribute),
            typeof(BeforeScenarioBlockAttribute),
            typeof(AfterScenarioBlockAttribute),
            typeof(BeforeAttribute),
            typeof(AfterAttribute)
        };

        public IEnumerable<Attribute> FilterForReqnrollAttributes(IEnumerable<Attribute> customAttributes)
        {
            return customAttributes.Where(a => _validAttributeTypes.Any(t => t.IsAssignableFrom(a?.GetType())));
        }
    }
}

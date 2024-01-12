using Reqnroll.Configuration;
using Reqnroll.Generator;
using Reqnroll.Generator.CodeDom;
using Reqnroll.Generator.Generation;
using Reqnroll.Generator.Interfaces;
using Reqnroll.Generator.UnitTestConverter;

namespace Reqnroll.Specs.Generator.ReqnrollPlugin
{
    public class CombinationFeatureGenerator : UnitTestFeatureGenerator
    {
        
        public CombinationFeatureGenerator(CodeDomHelper codeDomHelper, ReqnrollConfiguration reqnrollConfiguration, IDecoratorRegistry decoratorRegistry, Combination combination, ProjectSettings projectSettings) :
            base(new CustomXUnitGeneratorProvider(codeDomHelper, combination, projectSettings), codeDomHelper, reqnrollConfiguration, decoratorRegistry)
        {
        
        }
    }
}

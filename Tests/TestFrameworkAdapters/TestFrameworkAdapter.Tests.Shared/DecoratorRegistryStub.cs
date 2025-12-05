using Gherkin.Ast;
using Reqnroll.Generator;
using Reqnroll.Generator.UnitTestConverter;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reqnroll.xUnit3.Generator.ReqnrollPluginTests;

internal class DecoratorRegistryStub : IDecoratorRegistry
{
    public void DecorateTestClass(TestClassGenerationContext generationContext, out List<string> unprocessedTags)
    {
        unprocessedTags = new List<string>();
    }

    public void DecorateTestMethod(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, IEnumerable<Tag> tags, out List<string> unprocessedTags)
    {
        unprocessedTags = new List<string>();
    }
}

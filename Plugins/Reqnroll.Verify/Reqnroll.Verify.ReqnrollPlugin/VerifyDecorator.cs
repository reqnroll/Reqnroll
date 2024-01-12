using Reqnroll.Generator;
using Reqnroll.Generator.CodeDom;
using Reqnroll.Generator.UnitTestConverter;

namespace Reqnroll.Verify.ReqnrollPlugin;

public class VerifyDecorator : ITestClassDecorator
{
    private readonly CodeDomHelper _codeDomHelper;

    public VerifyDecorator(CodeDomHelper codeDomHelper)
    {
        _codeDomHelper = codeDomHelper;
    }

    public int Priority { get; } = 0;

    public bool CanDecorateFrom(TestClassGenerationContext generationContext) => true;

    public void DecorateFrom(TestClassGenerationContext generationContext)
    {
        _codeDomHelper.AddAttribute(generationContext.TestClass, "UsesVerify");

    }
}

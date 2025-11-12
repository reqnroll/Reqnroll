using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reqnroll.RuntimeTests.Bindings.Discovery.TestSubjects;

internal class DerivedUnBoundClassOveriddenMethodsWithBindingAttributes_InheritsFromBaseClassNoBindingNoBoundMethods : BaseClassNoBindingNoBoundMethods
{
    [Given("derived something")]
    public override void UnBoundMethod()
    {
    }
}

internal class DerivedUnBoundClassOveriddenMethodsWithBindingAttributes_InheritsFromBaseClassNoBindingWithBoundMethods : BaseClassNoBindingWithBoundMethods
{
    [Given("derived something")]
    public override void GivenSomething()
    {
    }
}


internal class DerivedUnBoundClassOveriddenMethodsWithBindingAttributes_InheritsFromBaseClassWithBindingNoBoundMethods : BaseClassWithBindingNoBoundMethods
{
    [Given("derived something")]
    public override void UnBoundMethod()
    {
    }
}

internal class DerivedUnBoundClassOveriddenMethodsWithBindingAttributes_InheritsFromBaseClassWithBindingWithBoundMethods : BaseClassWithBindingWithBoundMethods
{
    [Given("derived something")]
    public override void GivenSomething()
    {
    }
}

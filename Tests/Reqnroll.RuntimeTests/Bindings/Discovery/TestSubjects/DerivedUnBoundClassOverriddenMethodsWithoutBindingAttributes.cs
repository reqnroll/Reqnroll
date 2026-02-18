using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reqnroll.RuntimeTests.Bindings.Discovery.TestSubjects;

internal class DerivedUnBoundClassOverriddenMethodsWithoutBindingAttributes_InheritsFromBaseClassNoBindingNoBoundMethods : BaseClassNoBindingNoBoundMethods
{
    public override void UnBoundMethod()
    {
    }
}

internal class DerivedUnBoundClassOverriddenMethodsWithoutBindingAttributes_InheritsFromBaseClassNoBindingWithBoundMethods : BaseClassNoBindingWithBoundMethods
{
    public override void GivenSomething()
    {
    }
}

internal class DerivedUnBoundClassOverriddenMethodsWithoutBindingAttributes_InheritsFromBaseClassWithBindingNoBoundMethods : BaseClassWithBindingNoBoundMethods
{
    public override void UnBoundMethod()
    {
    }
}

internal class DerivedUnBoundClassOverriddenMethodsWithoutBindingAttributes_InheritsFromBaseClassWithBindingWithBoundMethods : BaseClassWithBindingWithBoundMethods
{
    public override void GivenSomething()
    {
    }
}

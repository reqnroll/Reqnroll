using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reqnroll.RuntimeTests.Bindings.Discovery.TestSubjects;

[Binding]
internal class DerivedBoundClassOverriddenMethodsWithoutBindingAttributes_InheritsFromBaseClassNoBindingBoundMethods : BaseClassNoBindingWithBoundMethods
{
    public override void GivenSomething()
    {
    }
}

[Binding]
internal class DerivedBoundClassOverriddenMethodsWithoutBindingAttributes_InheritsFromBaseClassWithBindingBoundMethods : BaseClassWithBindingWithBoundMethods
{
    public override void GivenSomething()
    {
    }
}

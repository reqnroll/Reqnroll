using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reqnroll.RuntimeTests.Bindings.Discovery.TestSubjects;

internal class DerivedUnboundClassWithBoundMethods_InheritsFromBaseClassNoBindingNoBoundMethods : BaseClassNoBindingNoBoundMethods
{
    [Given("something else")]
    public void BoundMethodInDerivedUnboundClass() { }
}

internal class DerivedUnboundClassWithBoundMethods_InheritsFromBaseClassNoBindingWithBoundMethods : BaseClassNoBindingWithBoundMethods
{
    [Given("something else")]
    public void BoundMethodInDerivedUnboundClass() { }

}

internal class DerivedUnboundClassWithBoundMethods_InheritsFromBaseClassWithBindingNoBoundMethods : BaseClassWithBindingNoBoundMethods
{
    [Given("something else")]
    public void BoundMethodInDerivedUnboundClass() { }

}

internal class DerivedUnboundClassWithBoundMethods_InheritsFromBaseClassWithBindingWithBoundMethods : BaseClassWithBindingWithBoundMethods
{
    [Given("something else")]
    public void BoundMethodInDerivedUnboundClass() { }

}


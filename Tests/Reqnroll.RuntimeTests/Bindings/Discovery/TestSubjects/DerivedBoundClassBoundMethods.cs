using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reqnroll.RuntimeTests.Bindings.Discovery.TestSubjects;

[Binding]
internal class DerivedBoundClassBoundMethods_InheritsFromBaseClassNoBindingNoBoundMethods : BaseClassNoBindingNoBoundMethods
{
    public override void UnBoundMethod() { }

    [Given("derived something")]
    public void GivenDerivedSomething() { }
}

[Binding]
internal class DerivedBoundClassBoundMethods_InheritsFromBaseClassWithBindingNoBoundMethods : BaseClassWithBindingNoBoundMethods
{
    public override void UnBoundMethod() { }

    [Given("derived something")]
    public void GivenDerivedSomething() { }
}

[Binding]
internal class DerivedBoundClassBoundMethods_InheritsFromBaseClassNoBindingWithBoundMethods : BaseClassNoBindingWithBoundMethods
{
    [Given("derived something")]
    public void GivenDerivedSomething() { }
}

[Binding]
internal class DerivedBoundClassBoundMethods_InheritsFromBaseClassWithBindingWithBoundMethods : BaseClassWithBindingWithBoundMethods
{
    [Given("derived something")]
    public void GivenDerivedSomething() { }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reqnroll.RuntimeTests.Bindings.Discovery.TestSubjects;

[Binding]
internal class DerivedBoundClassBoundMethodswithBindingaAttributes_InheritsFromBaseClassNoBindingNoBoundMethod : BaseClassNoBindingNoBoundMethods
{
    [Given("derived something")]
    public override void UnBoundMethod()
    {
        base.UnBoundMethod();
    }
}

[Binding]
internal class DerivedBoundClassBoundMethodswithBindingaAttributes_InheritsFromBaseClassNoBindingBoundMethod : BaseClassNoBindingWithBoundMethods
{
    [Given("something")]
    public override void GivenSomething()
    {
        base.GivenSomething();
    }
}

[Binding]
internal class DerivedBoundClassBoundMethodswithBindingaAttributes_InheritsFromBaseClassWithBindingNoBoundMethod : BaseClassWithBindingNoBoundMethods
{
    [Given("derived something")]
    public override void UnBoundMethod()
    {
        base.UnBoundMethod();
    }
}

[Binding]
internal class DerivedBoundClassBoundMethodswithBindingaAttributes_InheritsFromBaseClassWithBindingBoundMethod : BaseClassWithBindingWithBoundMethods
{
    [Given("something")]
    public override void GivenSomething()
    {
        base.GivenSomething();
    }
}

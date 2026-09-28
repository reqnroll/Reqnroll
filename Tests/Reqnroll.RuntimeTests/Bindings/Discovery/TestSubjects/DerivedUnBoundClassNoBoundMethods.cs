using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reqnroll.RuntimeTests.Bindings.Discovery.TestSubjects;

internal class DerivedUnBoundClassNoBoundMethods_InheritsFromBaseClassNoBindingNoBoundMethods : BaseClassNoBindingNoBoundMethods
{
    public void AnotherUnBoundMethod()
    {
    }
}

internal class DerivedUnBoundClassNoBoundMethods_InheritsFromBaseClassNoBindingWithBoundMethods : BaseClassNoBindingWithBoundMethods
{
    public void AnotherUnBoundMethod()
    {
    }
}

internal class DerivedUnBoundClassNoBoundMethods_InheritsFromBaseClassWithBindingNoBoundMethods : BaseClassWithBindingNoBoundMethods
{
    public void AnotherUnBoundMethod()
    {
    }
}

internal class DerivedUnBoundClassNoBoundMethods_InheritsFromBaseClassWithBindingWithBoundMethods : BaseClassWithBindingWithBoundMethods
{
    public void AnotherUnBoundMethod()
    {
    }
}

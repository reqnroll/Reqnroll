using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reqnroll.RuntimeTests.Bindings.Discovery.TestSubjects;

internal class BaseClassNoBindingNoBoundMethods
{
    public virtual void UnBoundMethod()
    {
    }
}

internal class BaseClassNoBindingWithBoundMethods
{
    [Given("something")]
    public virtual void GivenSomething() { }

    public virtual void UnBoundMethod()
    {
    }
}

[Binding]
internal class BaseClassWithBindingNoBoundMethods
{
    public virtual void UnBoundMethod()
    {
    }
}

[Binding]
internal class BaseClassWithBindingWithBoundMethods
{
    [Given("something")]
    public virtual void GivenSomething() { }
}

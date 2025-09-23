using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reqnroll;

namespace Reqnroll.Formatters.Tests.Samples.Resources.unused_steps;

[Binding]
internal class unused_steps
{
    [Given("a step that is used")]
    public void GivenAStepThatIsUsed() { }

    [Given("a step that is not used")]
    public void GivenAStepThatIsNotUsed() { }
}

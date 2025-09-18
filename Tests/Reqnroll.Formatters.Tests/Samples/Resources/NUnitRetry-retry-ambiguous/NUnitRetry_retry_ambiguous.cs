using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reqnroll;

namespace Reqnroll.Formatters.Tests.Samples.Resources.NUnitRetry_retry_ambiguous;

[Binding]
internal class NUnitRetry_retry_ambiguous
{
    [Given("an ambiguous step")]
    public void StepOne() { }

    [Given("an ambiguous step")]
    public void StepTwo() { }

}

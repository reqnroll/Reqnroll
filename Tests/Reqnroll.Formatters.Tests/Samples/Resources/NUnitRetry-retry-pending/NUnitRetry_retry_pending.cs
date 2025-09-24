using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reqnroll;

namespace Reqnroll.Formatters.Tests.Samples.Resources.NUnitRetry_retry_pending;

[Binding]
internal class NUnitRetry_retry_pending
{
    [Given("a pending step")]
    public void StepOne() {
        throw new PendingStepException();
    }


}

using System;
using System.Collections.Generic;
using System.Text;
using Reqnroll;
using Reqnroll.UnitTestProvider;

namespace Reqnroll.Formatters.Tests.Samples.Resources.all_statuses;

[Binding]
internal class all_statuses
{
    private readonly IUnitTestRuntimeProvider _unitTestRuntimeProvider;
    public all_statuses(IUnitTestRuntimeProvider unitTestRuntimeProvider)
    {
        _unitTestRuntimeProvider = unitTestRuntimeProvider;
    }

    [Given("a step")]
    public void GivenAStep()
    {
    }

    [Given("a failing step")]
    public void GivenAFailingStep()
    {
        throw new Exception("whoops");
    }

    [Given("a pending step")]
    public void GivenAPendingStep()
    {
        //_unitTestRuntimeProvider.TestPending("pending");
        throw new PendingStepException("pending");
    }

    [Given("a skipped step")]
    public void GivenASkippedStep()
    {
        _unitTestRuntimeProvider.TestIgnore("skipped");
    }

    [Given("an ambiguous step")]
    public void GivenAnAmbiguousStep()
    {
    }

    [Given("an ambiguous step")]
    public void GivenAnAmbiguousStep_Duplicate()
    {
    }
}

using System;
using System.Collections.Generic;

namespace Reqnroll.Assist.Dynamic;

public class DynamicInstanceComparisonException(IList<string> diffs)
    : Exception("There were some difference between the table and the instance")
{
    public IList<string> Differences { get; private set; } = diffs;
}
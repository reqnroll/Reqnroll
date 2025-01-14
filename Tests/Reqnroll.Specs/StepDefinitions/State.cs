using System;
using System.Collections.Generic;

namespace Reqnroll.Specs.StepDefinitions;

public class State
{
    public required dynamic OriginalInstance;

    public required IList<dynamic> OriginalSet;

    public required Exception CurrentException;
}
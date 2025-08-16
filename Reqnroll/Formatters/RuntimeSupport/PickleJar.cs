using Io.Cucumber.Messages.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Reqnroll.Formatters.RuntimeSupport;

/// <summary>
/// This class is used at runtime to provide the appropriate PickleId and PickleStepId to the steps.
/// </summary>
public class PickleJar
{
    private bool HasPickles => _pickles != null && _pickles.Any();

    private readonly IEnumerable<Pickle> _pickles;

    internal PickleJar(IEnumerable<Pickle> pickles)
    {
        _pickles = pickles;
    }

    internal PickleStepSequence PickleStepSequenceFor(string pickleIndex)
    {
        var pickleIndexInt = int.Parse(pickleIndex);
        if (HasPickles && (pickleIndexInt < 0 || pickleIndexInt >= _pickles.Count()))
            throw new ArgumentException("Invalid pickle index: " + pickleIndex);

        return new PickleStepSequence(HasPickles, HasPickles ? _pickles.ElementAt(pickleIndexInt) : null);
    }
}
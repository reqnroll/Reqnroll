using Io.Cucumber.Messages.Types;
using System.Linq;

namespace Reqnroll.Formatters.RuntimeSupport;

public class PickleStepSequence(bool hasPickles, Pickle pickle)
{
    public bool HasPickles { get; } = hasPickles;

    public Pickle CurrentPickle { get; } = pickle;

    private int _pickleStepCounter = 0;

    public void NextStep()
    {
        _pickleStepCounter++;
    }

    public string CurrentPickleStepId
    {
        get
        {
            if (!HasPickles) return null;
            return (_pickleStepCounter < CurrentPickle.Steps.Count) ? CurrentPickle.Steps.ElementAt(_pickleStepCounter).Id : null;
        }
    }
}
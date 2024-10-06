using Reqnroll.Events;

namespace Reqnroll.CucumberMessages.RuntimeSupport
{
    /// <summary>
    /// Wraps an <see cref="OutputAddedEvent"/> to provide a <see cref="PickleStepID"/>
    /// </summary>
    internal class OutputAddedEventWrapper : ExecutionEvent
    {
        internal OutputAddedEvent OutputAddedEvent;
        internal string PickleStepID;
        internal string TestCaseStepID;
        internal string TestCaseStartedID;

        public OutputAddedEventWrapper(OutputAddedEvent outputAddedEvent, string pickleStepId)
        {
            OutputAddedEvent = outputAddedEvent;
            PickleStepID = pickleStepId;
        }
    }
}
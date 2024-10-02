﻿using Reqnroll.Events;

namespace Reqnroll.CucumberMessages
{
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
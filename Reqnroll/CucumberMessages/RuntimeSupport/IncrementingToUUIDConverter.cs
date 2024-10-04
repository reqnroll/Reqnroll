using Reqnroll.CucumberMessages.PayloadProcessing.Cucumber;
using System;
using System.Collections.Generic;
using System.Text;

namespace Reqnroll.CucumberMessages.RuntimeSupport
{
    internal class IncrementingToUUIDConverter : CucumberMessage_TraversalVisitorBase
    {
        Dictionary<string, string> _mapping = new Dictionary<string, string>();
        public IncrementingToUUIDConverter()
        {
        }

    }
}

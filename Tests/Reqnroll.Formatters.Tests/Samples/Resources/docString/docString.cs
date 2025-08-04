using System;
using Reqnroll;

namespace Reqnroll.Formatters.Tests.DocString
{
    [Binding]
    public class DocStringStepDefinitions
    {
        [Given("A step with a doc string")]
        public void GivenAStepWithADocString(string multilineText)
        {
        }
    }
}

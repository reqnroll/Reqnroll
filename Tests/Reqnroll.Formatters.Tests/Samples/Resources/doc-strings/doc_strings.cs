using System;
using Reqnroll;

namespace Reqnroll.Formatters.Tests.DocString
{
    [Binding]
    public class DocStringStepDefinitions
    {
        [Given("a doc string:")]
        public void GivenAStepWithADocString(string multilineText)
        {
        }
    }
}

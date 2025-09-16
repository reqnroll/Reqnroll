using Reqnroll;
using Reqnroll.Configuration;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CucumberMessages.Tests.Samples.retry
{
    [Binding]
    internal class Retry
    {
        [Given("a step that always passes")]
        public void AStepThatAlwaysPasses()
        {

        }

        static int secondTimePass = 0;
        [Given("a step that passes the second time")]
        public void AStepThatPassesTheSecondTime()
        {
            secondTimePass++;
            if (secondTimePass < 2)
            {
                Assert.Fail("Exception in step");
            }
        }

        static int thirdTimePass = 0;
        [Given("a step that passes the third time")]
        public void AStepThatPassesTheThirdTime()
        {
            thirdTimePass++;
            if (thirdTimePass < 3)
            {
                Assert.Fail("Exception in step");
            }
        }

        [Given("a step that always fails")]
        public void AStepThatAlwaysFails()
        {
            Assert.Fail("Exception in step");
        }
    }
}

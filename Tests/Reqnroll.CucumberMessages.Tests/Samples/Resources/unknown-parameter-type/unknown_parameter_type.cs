using Reqnroll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CucumberMessages.CompatibilityTests.CCK.unknown_parameter_type
{
    [Binding]
    internal class Unknown_parameter_type
    {
        [Given("{airport} is closed because of a strike")]
        public void GivenAirportIsClosedBecauseOfStrike(Airport airport)
        {
            throw new Exception("Should not be called because airport parameter type has not been defined");
        }
    }

    public class Airport
    {
    }
}

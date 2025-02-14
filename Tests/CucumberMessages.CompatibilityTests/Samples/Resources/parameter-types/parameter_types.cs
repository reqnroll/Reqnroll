using Reqnroll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CucumberMessages.CompatibilityTests.CCK.parameter_types
{
    [Binding]
    internal class Parameter_types
    {
        [StepArgumentTransformation(Name ="flight", Regex = @"([A-Z]{3})-([A-Z]{3})")]
        public Flight FlightConverter(string from, string to)
        {
            return new Flight
            {
                From = from,
                To = to
            };
        }

        [Given("{flight} has been delayed")]
        public void GivenFlightHasBeenDelayed(Flight flight)
        {
            if (flight.From == "LHR" && flight.To == "CDG") { } 
            else throw new Exception();
        }
    }

    public class Flight
    {
        public string? From { get; internal set; }
        public string? To { get; internal set; }
    }
}

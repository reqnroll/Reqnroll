using System;

namespace Reqnroll.RuntimeTests.AssistTests.ExampleEntities
{
    class PersonWithMandatoryLastName : Person
    {
        /// <inheritdoc />
        public PersonWithMandatoryLastName(string lastName)
        {
            LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
        }
    }
}

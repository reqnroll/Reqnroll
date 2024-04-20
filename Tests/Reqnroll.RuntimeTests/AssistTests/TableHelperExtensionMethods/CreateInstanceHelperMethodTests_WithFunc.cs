using System;
using Xunit;
using FluentAssertions;
using Reqnroll.Assist;
using Reqnroll.RuntimeTests.AssistTests.ExampleEntities;

namespace Reqnroll.RuntimeTests.AssistTests.TableHelperExtensionMethods
{
    
    [Obsolete]
    public class CreateInstanceHelperMethodTests_WithFunc
    {
        [Fact]
        public void CreateInstance_returns_the_object_returned_from_the_func()
        {
            var table = new Table("Field", "Value");
            var expectedPerson = new Person();
            var person = table.CreateInstance(() => expectedPerson);
            person.Should().Be(expectedPerson);
        }

        [Fact]
        public void Create_instance_will_fill_the_instance_()
        {
            var table = new Table("Field", "Value");
            table.AddRow("FirstName", "John");
            table.AddRow("LastName", "Galt");

            var expectedPerson = new Person { FirstName = "Ellsworth", LastName = "Toohey" };
            var person = table.CreateInstance(() => expectedPerson);

            person.FirstName.Should().Be("John");
            person.LastName.Should().Be("Galt");
        }
    }
}
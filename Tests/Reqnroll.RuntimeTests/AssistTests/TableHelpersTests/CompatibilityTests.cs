using System;
using FluentAssertions;
using Reqnroll.Assist;
using Reqnroll.RuntimeTests.AssistTests.ExampleEntities;
using Xunit;

namespace Reqnroll.RuntimeTests.AssistTests.TableHelpersTests;

public class CompatibilityTests
{
    [Fact]
    [Obsolete("Compatibility tests for Obsolete code")]
    public virtual void Create_instance_will_return_an_instance_of_T()
    {
        var tableHelpers = new TableHelpers(new Service());
        var table = new Table("Field", "Value");

        var personOld = table.CreateInstance<Person>();
        var personNew = tableHelpers.CreateInstance<Person>(table);

        personNew.Should().BeEquivalentTo(personOld);
    }

    [Fact]
    [Obsolete("Compatibility tests for Obsolete code")]
    public void Returns_empty_set_of_type_when_there_are_no_rows()
    {
        var tableHelpers = new TableHelpers(new Service());
        var table = new Table("FirstName");

        var personOld = table.CreateSet<Person>();
        var peopleNew = tableHelpers.CreateSet<Person>(table);

        peopleNew.Should().BeEquivalentTo(personOld);
    }
}

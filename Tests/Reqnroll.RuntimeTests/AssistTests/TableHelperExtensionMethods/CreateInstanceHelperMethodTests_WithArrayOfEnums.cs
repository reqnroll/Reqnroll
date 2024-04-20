using System;
using FluentAssertions;
using Reqnroll.Assist;
using Reqnroll.RuntimeTests.AssistTests.ExampleEntities;
using Xunit;

namespace Reqnroll.RuntimeTests.AssistTests.TableHelperExtensionMethods
{
    [Obsolete]
    public class CreateInstanceHelperMethodTests_WithArrayOfEnums
    {
        [Fact]
        public void Can_create_an_instance_with_enum_array_from_comma_separated_list_of_strings()
        {
            var table = new Table("Field", "Value");
            table.AddRow("Languages", $"Finnish, English, Swedish");
 
            var @class = table.CreateInstance<Person>();

            @class.Languages[0].Should().Be(Language.Finnish);
            @class.Languages[1].Should().Be(Language.English);
            @class.Languages[2].Should().Be(Language.Swedish);
            @class.Languages.Length.Should().Be(3);
        }
    }
}

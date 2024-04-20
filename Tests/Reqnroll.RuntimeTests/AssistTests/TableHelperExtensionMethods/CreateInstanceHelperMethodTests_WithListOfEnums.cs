using System;
using FluentAssertions;
using Reqnroll.Assist;
using Reqnroll.RuntimeTests.AssistTests.ExampleEntities;
using Xunit;

namespace Reqnroll.RuntimeTests.AssistTests.TableHelperExtensionMethods
{
    [Obsolete]
    public class CreateInstanceHelperMethodTests_WithListOfEnums
    {
        [Fact]
        public void Can_create_an_instance_with_enum_list_from_comma_separated_list_of_strings()
        {
            var table = new Table("Field", "Value");
            table.AddRow("LanguageList", $"Finnish, English, Swedish");
 
            var @class = table.CreateInstance<Person>();

            @class.LanguageList[0].Should().Be(Language.Finnish);
            @class.LanguageList[1].Should().Be(Language.English);
            @class.LanguageList[2].Should().Be(Language.Swedish);
            @class.LanguageList.Count.Should().Be(3);
        }
    }
}

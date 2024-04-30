using System;
using Xunit;
using FluentAssertions;
using Reqnroll.Assist;

namespace Reqnroll.RuntimeTests.AssistTests.SituationalTests
{

    public class NullableEnumTests
    {
        public enum TestEnum
        {
            Value1,
            Value2,
            Value3
        }

        public class TestEntity
        {
            public TestEnum? TestProperty { get; set; }
        }

        [Fact]
        public void The_value_should_be_set_if_it_is_in_the_table()
        {
            var service = new Service();
            var tableHelpers = new TableHelpers(service);
            
            var table = new Table("Field", "Value");
            table.AddRow("TestProperty", "Value2");

            var test = tableHelpers.CreateInstance<TestEntity>(table);
            test.TestProperty.Should().Be(TestEnum.Value2);
        }

        [Fact]
        public void The_value_should_be_NULL_if_it_is_not_filled_in_the_table()
        {
            var service = new Service();
            var tableHelpers = new TableHelpers(service);
            
            var table = new Table("Field", "Value");
            table.AddRow("TestProperty", "");

            var test = tableHelpers.CreateInstance<TestEntity>(table);
            test.TestProperty.Should().BeNull();
        }

        [Fact]
        public void The_value_should_be_NULL_if_it_is_not_in_the_table()
        {
            var service = new Service();
            var tableHelpers = new TableHelpers(service);
            
            var table = new Table("Field", "Value");

            var test = tableHelpers.CreateInstance<TestEntity>(table);
            test.TestProperty.Should().BeNull();
        }

        [Fact]
        public void There_should_be_an_error_if_in_the_table_is_no_valid_Enum_value()
        {
            var service = new Service();
            var tableHelpers = new TableHelpers(service);
            
            var table = new Table("Field", "Value");
            table.AddRow("TestProperty", "NotAnEnumValue");

            Action x = () => { tableHelpers.CreateInstance<TestEntity>(table); };

            x.Should().Throw<InvalidOperationException>();
        }
    }
}

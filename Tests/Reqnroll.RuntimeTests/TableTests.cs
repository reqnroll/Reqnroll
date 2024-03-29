using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Reqnroll.RuntimeTests
{
    
    public class TableTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public TableTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void should_return_nice_error_message_when_column_not_is_found_in_table()
        {
            // Arrange
            var t = CreateDemoTable();

            // Act
            var mess = string.Empty;
            try
            {
                var h = t.Rows[0]["DontExsist"];
            }
            catch (IndexOutOfRangeException ex)
            {
                mess = ex.Message;
                _testOutputHelper.WriteLine(mess);
            }

            // Assert
            var expected = string.Format(Table.ERROR_COLUMN_NAME_NOT_FOUND, "DontExsist");

            mess.StartsWith(expected).Should().BeTrue();
        }

        [Fact]
        public void should_return_nice_errormessage_when_adding_null_as_headers()
        {
            // Act
            var mess = string.Empty;
            try
            {
                new Table((string[])null);
            }
            catch (ArgumentException ex)
            {
                mess = ex.Message;
                _testOutputHelper.WriteLine(mess);
            }

            // Assert
            Assert.StartsWith(Table.ERROR_NO_HEADER_TO_ADD, mess);
        }

        [Fact]
        public void should_return_nice_errormessage_when_adding_0_headers()
        {
            // Act
            var mess = string.Empty;
            try
            {
                var headers = new string[] { };
                new Table(headers);
            }
            catch (ArgumentException ex)
            {
                mess = ex.Message;
                _testOutputHelper.WriteLine(mess);
            }

            // Assert
            Assert.StartsWith(Table.ERROR_NO_HEADER_TO_ADD, mess);

        }

        [Fact]
        public void should_return_nice_errormessage_when_adding_null_as_new_tablerow()
        {
            // Arrange
            var t = CreateDemoTable();

            // Act
            var mess = string.Empty;
            try
            {
                t.AddRow((string[])null);
            }
            catch (Exception ex)
            {
                mess = ex.Message;
                Console.WriteLine(mess);
            }

            // Assert
            Assert.StartsWith(Table.ERROR_NO_CELLS_TO_ADD, mess);

        }

        [Fact]
        public void should_return_nice_errormessage_when_cells_that_doesnt_match_headers()
        {
            // Arrange
            var t = CreateDemoTable();

            // Act
            var mess = string.Empty;
            try
            {
                t.AddRow("Only", "Two");
            }
            catch (ArgumentException ex)
            {
                mess = ex.Message;
                Console.WriteLine(mess);
            }

            // Assert
            var expected = string.Format(Table.ERROR_CELLS_NOT_MATCHING_HEADERS, 2, 3);
            Assert.StartsWith(expected, mess);
        }



        private static Table CreateDemoTable()
        {
            var t = new Table("Column 1", "Column 2", "Column 3");
            t.AddRow("Value 1:1", "Value 1:2", "Value 1:3");
            t.AddRow("Value 2:1", "Value 2:2", "Value 2:3");
            t.AddRow("Value 3:1", "Value 3:2", "Value 3:3");

            return t;
        }

        [Fact]
        public void should_be_able_to_set_cell_by_indexing_the_row_with_header()
        {
            var table = CreateDemoTable();
            table.Rows[0]["Column 2"] = "newvalue";
            Assert.Equal("newvalue", table.Rows[0]["Column 2"]);
        }

        [Fact]
        public void should_be_able_to_get_cell_by_a_valid_header_with_TryGetValue()
        {
            var table = CreateDemoTable();
            string cellValue;
            var result = table.Rows[0].TryGetValue("Column 2", out cellValue);
            Assert.True(result);
            Assert.Equal("Value 1:2", cellValue);
        }

        [Fact]
        public void should_be_able_to_get_false_by_an_invalid_header_with_TryGetValue()
        {
            var table = CreateDemoTable();
            string cellValue;
            var result = table.Rows[0].TryGetValue("nosuchcolumn", out cellValue);
            Assert.False(result);
        }

        [Fact]
        public void should_be_able_to_add_row_from_a_dictionary()
        {
            var table = CreateDemoTable();

            table.AddRow(new Dictionary<string, string>() { {"Column 1", "v1"}, {"Column 3", "v3"}, {"Column 2", "v2"} });

            table.Rows[table.RowCount - 1].Values.Should().BeEquivalentTo(new string[] {"v1", "v2", "v3"});
        }

        [Fact]
        public void should_be_able_to_add_row_from_a_partial_dictionary()
        {
            var table = CreateDemoTable();

            table.AddRow(new Dictionary<string, string>() { {"Column 1", "v1"}, {"Column 3", "v3"} });

            table.Rows[table.RowCount - 1].Values.Should().BeEquivalentTo(new string[] { "v1", "", "v3" });
            
        }

        [Fact]
        public void should_be_able_to_rename_column()
        {
            var table = CreateDemoTable();
            table.RenameColumn("Column 1", "c1");
            table.Header.Should().BeEquivalentTo(new string[] {"c1", "Column 2", "Column 3"});
        }
    }
}
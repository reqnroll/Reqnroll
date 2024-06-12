using System;
using System.Globalization;
using System.Threading;
using Xunit;
using FluentAssertions;
using Reqnroll.Assist.ValueComparers;

namespace Reqnroll.RuntimeTests.AssistTests.ValueComparerTests
{
    public class DateTimeOffsetValueComparerTests
    {
        public DateTimeOffsetValueComparerTests()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US", false);
        }

        [Fact]
        public void Can_compare_if_the_value_is_a_datetimeoffset()
        {
            new DateTimeOffsetValueComparer()
                .CanCompare(new DateTimeOffset())
                .Should()
                .BeTrue();
        }

        [Fact]
        public void Cannot_compare_if_the_value_is_not_a_datetimeoffset()
        {
            var comparer = new DateTimeOffsetValueComparer();
            comparer.CanCompare(3).Should().BeFalse();
            comparer.CanCompare("x").Should().BeFalse();
            comparer.CanCompare(4.3).Should().BeFalse();
        }

        [Fact]
        public void Cannot_compare_if_the_value_is_null()
        {
            new DateTimeOffsetValueComparer()
                .CanCompare(null)
                .Should()
                .BeFalse();
        }

        [Fact]
        public void Returns_true_when_the_string_and_values_match_exactly()
        {
            var comparer = new DateTimeOffsetValueComparer();
            comparer.Compare(new DateTimeOffset(new DateTime(2011, 1, 2)).ToString(), new DateTimeOffset(new DateTime(2011, 1, 2)))
                    .Should()
                    .BeTrue();
            comparer.Compare(new DateTimeOffset(new DateTime(2020, 12, 31)).ToString(), new DateTimeOffset(new DateTime(2020, 12, 31)))
                    .Should()
                    .BeTrue();
        }

        [Fact]
        public void Returns_false_when_the_string_and_values_match_for_different_dates()
        {
            var comparer = new DateTimeOffsetValueComparer();
            comparer.Compare(new DateTimeOffset(new DateTime(2011, 1, 3)).ToString(), new DateTimeOffset(new DateTime(2011, 1, 2)))
                    .Should()
                    .BeFalse();
            comparer.Compare(new DateTimeOffset(new DateTime(2011, 1, 2)).ToString(), new DateTimeOffset(new DateTime(2011, 1, 3)))
                    .Should()
                    .BeFalse();
            comparer.Compare(new DateTimeOffset(new DateTime(2011, 1, 1)).ToString(), new DateTimeOffset(new DateTime(2012, 1, 1)))
                    .Should()
                    .BeFalse();
            comparer.Compare(new DateTimeOffset(new DateTime(2011, 1, 1)).ToString(), new DateTimeOffset(new DateTime(2011, 2, 1)))
                    .Should()
                    .BeFalse();
        }

        [Fact]
        public void Returns_false_when_the_expected_value_is_not_a_valid_datetimeoffset()
        {
            var comparer = new DateTimeOffsetValueComparer();
            comparer.Compare("x", new DateTimeOffset(new DateTime(1990, 1, 1)))
                    .Should()
                    .BeFalse();
            comparer.Compare("January1", new DateTimeOffset(new DateTime(1990, 1, 1)))
                    .Should()
                    .BeFalse();
        }

        [Fact]
        public void Returns_false_the_value_is_correct_format_but_not_a_valid_date()
        {
            var comparer = new DateTimeOffsetValueComparer();
            comparer.Compare(new DateTimeOffset(new DateTime(2011, 2, 28)).ToString().Replace("28", "29"), new DateTimeOffset(new DateTime(2011, 2, 28)))
                    .Should()
                    .BeFalse();
            comparer.Compare(new DateTimeOffset(new DateTime(2011, 12, 1)).ToString().Replace("12", "13"), new DateTimeOffset(new DateTime(2011, 12, 1)))
                    .Should()
                    .BeFalse();
        }
    }
}

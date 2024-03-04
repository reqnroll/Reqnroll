using System;
using System.Collections.Generic;
using System.Linq;
using Reqnroll.Assist;

// ReSharper disable once CheckNamespace
namespace Reqnroll
{
    public static class InstanceComparisonExtensionMethods
    {
        public static void CompareToInstance<T>(this Table table, T instance)
        {
            AssertThatTheInstanceExists(instance);

            var instanceTable = TEHelpers.GetTheProperInstanceTable(table, typeof(T));

            var differences = FindAnyDifferences(Service.Instance, instanceTable, instance);

            if (ThereAreAnyDifferences(differences)) ThrowAnExceptionThatDescribesThoseDifferences(differences);
        }

        /// <summary>
        /// Indicates whether the table is equivalent to the specified instance by comparing the values of all
        /// columns against the properties of the instance.  Will return false after finding the first difference.
        /// </summary>
        public static bool IsEquivalentToInstance<T>(this Table table, T instance)
        {
            AssertThatTheInstanceExists(instance);

            var instanceTable = TEHelpers.GetTheProperInstanceTable(table, typeof(T));

            return HasDifference(Service.Instance, instanceTable, instance) == false;
        }

        private static void AssertThatTheInstanceExists<T>(T instance)
        {
            if (instance == null) throw new ComparisonException("The item to compare was null.");
        }

        private static void ThrowAnExceptionThatDescribesThoseDifferences(IEnumerable<Difference> differences)
        {
            throw new ComparisonException(CreateDescriptiveErrorMessage(differences));
        }

        private static string CreateDescriptiveErrorMessage(IEnumerable<Difference> differences)
        {
            return differences.Aggregate(
                @"The following fields did not match:",
                (sum, next) => sum + (Environment.NewLine + next.Description));
        }

        private static Difference[] FindAnyDifferences<T>(Service service, Table table, T instance)
        {
            return (from row in table.Rows
                    where ThePropertyDoesNotExist(instance, row) || TheValuesDoNotMatch(service, instance, row)
                    select CreateDifferenceForThisRow(service, instance, row)).ToArray();
        }

        private static bool HasDifference<T>(Service service, Table table, T instance)
        {
            // This method exists so it will stop evaluating the instance (hence stop using Reflection)
            // after the first difference is found.
            return (from row in table.Rows
                    select row)
                .Any(row => ThePropertyDoesNotExist(instance, row) || TheValuesDoNotMatch(service, instance, row));
        }

        private static bool ThereAreAnyDifferences(IEnumerable<Difference> differences)
        {
            return differences.Any();
        }

        internal static bool ThePropertyDoesNotExist<T>(T instance, DataTableRow row)
        {
            return instance.GetType()
                           .GetProperties()
                           .Any(property => TEHelpers.IsMemberMatchingToColumnName(property, row.Id()))
                   == false;
        }

        internal static bool ThereIsADifference<T>(Service service, T instance, DataTableRow row)
        {
            return ThePropertyDoesNotExist(instance, row) || TheValuesDoNotMatch(service, instance, row);
        }

        private static bool TheValuesDoNotMatch<T>(Service service, T instance, DataTableRow row)
        {
            var expected = GetTheExpectedValue(row);
            var propertyValue = instance.GetPropertyValue(row.Id());
            var comparer = FindValueComparerForValue(service, propertyValue);

            return comparer
                       .Compare(expected, propertyValue)
                   == false;
        }

        private static string GetTheExpectedValue(DataTableRow row)
        {
            return row.Value();
        }

        private static Difference CreateDifferenceForThisRow<T>(Service service, T instance, DataTableRow row)
        {
            var propertyName = row.Id();

            if (ThePropertyDoesNotExist(instance, row)) return new PropertyDoesNotExist(propertyName);

            var expected = row.Value();
            var actual = instance.GetPropertyValue(propertyName);
            var comparer = FindValueComparerForProperty(service, instance, propertyName);
            return new PropertyDiffers(propertyName, expected, actual, comparer);
        }

        private static IValueComparer FindValueComparerForProperty<T>(Service service, T instance, string propertyName) =>
            FindValueComparerForValue(service, instance.GetPropertyValue(propertyName));

        private static IValueComparer FindValueComparerForValue(Service service, object propertyValue) =>
            service.ValueComparers.FirstOrDefault(x => x.CanCompare(propertyValue));

        private abstract class Difference
        {
            public abstract string Description { get; }
        }

        private class PropertyDoesNotExist : Difference
        {
            private readonly string propertyName;

            public PropertyDoesNotExist(string propertyName)
            {
                this.propertyName = propertyName;
            }

            public override string Description =>
                $"{propertyName}: Property does not exist";
        }

        private class PropertyDiffers : Difference
        {
            private readonly string propertyName;
            private readonly object expected;
            private readonly object actual;
            private readonly IValueComparer comparer;

            public PropertyDiffers(string propertyName, object expected, object actual, IValueComparer comparer)
            {
                this.propertyName = propertyName;
                this.expected = expected;
                this.actual = actual;
                this.comparer = comparer;
            }

            public override string Description =>
                $"{propertyName}: Expected <{expected}>, Actual <{actual}>, Using '{comparer.GetType().FullName}'";
        }
    }

    public static class SetComparerTableHelpers
    {
        public static string Id(this DataTableRow row)
        {
            return row[0];
        }

        public static string Value(this DataTableRow row)
        {
            return row[1];
        }
    }

    public class ComparisonException : Exception
    {
        public ComparisonException(string message)
            : base(message)
        {
        }
    }
}

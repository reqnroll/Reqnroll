using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Reqnroll.Assist.Attributes;

namespace Reqnroll.Assist;

public class TableHelpers
{
    private readonly Service _service;

    public TableHelpers(Service service)
    {
        _service = service;
    }

    public IEnumerable<T> CreateSet<T>(Table table)
    {
        return CreateSet<T>(table, null);
    }

    public IEnumerable<T> CreateSet<T>(Table table, InstanceCreationOptions creationOptions)
    {
        int count = table.Rows.Count;

        var list = new List<T>(count);

        var pivotTable = new PivotTable(table);
        for (var index = 0; index < count; index++)
        {
            var instanceTable = pivotTable.GetInstanceTable(index);
            var instance = CreateInstance<T>(instanceTable, creationOptions);
            list.Add(instance);
        }

        return list;
    }

    public T CreateInstance<T>(Table table)
    {
        var instanceTable = TEHelpers.GetTheProperInstanceTable(table, typeof(T));
        return TEHelpers.ThisTypeHasADefaultConstructor<T>()
            ? TEHelpers.CreateTheInstanceWithTheDefaultConstructor<T>(_service, instanceTable, null)
            : TEHelpers.CreateTheInstanceWithTheValuesFromTheTable<T>(_service, instanceTable, null);
    }

    public T CreateInstance<T>(Table table, InstanceCreationOptions creationOptions)
    {
        var instanceTable = TEHelpers.GetTheProperInstanceTable(table, typeof(T));
        return TEHelpers.ThisTypeHasADefaultConstructor<T>()
            ? TEHelpers.CreateTheInstanceWithTheDefaultConstructor<T>(_service, instanceTable, creationOptions)
            : TEHelpers.CreateTheInstanceWithTheValuesFromTheTable<T>(_service, instanceTable, creationOptions);
    }

    public T CreateInstance<T>(Table table, Func<T> methodToCreateTheInstance)
    {
        return CreateInstance(table, methodToCreateTheInstance, null);
    }

    public T CreateInstance<T>(Table table, Func<T> methodToCreateTheInstance, InstanceCreationOptions creationOptions)
    {
        var instance = methodToCreateTheInstance();
        FillInstance(table, instance, creationOptions);
        return instance;
    }

    public void FillInstance(Table table, object instance, InstanceCreationOptions creationOptions)
    {
        var instanceTable = TEHelpers.GetTheProperInstanceTable(table, instance.GetType());
        TEHelpers.LoadInstanceWithKeyValuePairs(_service, instanceTable, instance, creationOptions);
    }

    /// <summary>
    /// Indicates whether the table is equivalent to the specified instance by comparing the values of all
    /// columns against the properties of the instance.  Will return false after finding the first difference.
    /// </summary>
    public bool IsEquivalentToInstance<T>(Table table, T instance)
    {
        AssertThatTheInstanceExists(instance);

        var instanceTable = TEHelpers.GetTheProperInstanceTable(table, typeof(T));

        return HasDifference(_service, instanceTable, instance) == false;
    }

    public void CompareToInstance<T>(Table table, T instance)
    {
        AssertThatTheInstanceExists(instance);

        var instanceTable = TEHelpers.GetTheProperInstanceTable(table, typeof(T));

        var differences = FindAnyDifferences(_service, instanceTable, instance);

        if (differences.Any()) throw new ComparisonException(CreateDescriptiveErrorMessage(differences));
    }

    private Difference[] FindAnyDifferences<T>(Service service, Table table, T instance)
    {
        return (from row in table.Rows
                where ThePropertyDoesNotExist(instance, row) || TheValuesDoNotMatch(instance, row)
                select CreateDifferenceForThisRow(service, instance, row)).ToArray();
    }

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

    private static string CreateDescriptiveErrorMessage(IEnumerable<Difference> differences)
    {
        return differences.Aggregate(
            @"The following fields did not match:",
            (sum, next) => sum + (Environment.NewLine + next.Description));
    }

    private Difference CreateDifferenceForThisRow<T>(Service service, T instance, DataTableRow row)
    {
        var propertyName = row.Id();

        if (ThePropertyDoesNotExist(instance, row)) return new PropertyDoesNotExist(propertyName);

        var expected = row.Value();
        var actual = instance.GetPropertyValue(propertyName);
        var comparer = FindValueComparerForValue(instance.GetPropertyValue(propertyName));
        return new PropertyDiffers(propertyName, expected, actual, comparer);
    }

    internal bool ThePropertyDoesNotExist<T>(T instance, DataTableRow row)
    {
        return instance.GetType()
                       .GetProperties()
                       .Any(property => IsMemberMatchingToColumnName(property, row.Id()))
               == false;
    }

    internal static bool IsMemberMatchingToColumnName(MemberInfo member, string columnName)
    {
        return member.Name.MatchesThisColumnName(columnName)
               || IsMatchingAlias(member, columnName);
    }

    private static bool IsMatchingAlias(MemberInfo field, string id)
    {
        var aliases = field.GetCustomAttributes().OfType<TableAliasesAttribute>();
        return aliases.Any(a => a.Aliases.Any(al => Regex.Match(id, al).Success));
    }

    private bool TheValuesDoNotMatch<T>(T instance, DataTableRow row)
    {
        var expected = GetTheExpectedValue(row);
        var propertyValue = instance.GetPropertyValue(row.Id());
        var comparer = FindValueComparerForValue(propertyValue);

        return comparer
                   .Compare(expected, propertyValue)
               == false;
    }

    private IValueComparer FindValueComparerForValue(object propertyValue) =>
        _service.ValueComparers.FirstOrDefault(x => x.CanCompare(propertyValue));

    private static string GetTheExpectedValue(DataTableRow row)
    {
        return row.Value();
    }

    private static void AssertThatTheInstanceExists<T>(T instance)
    {
        if (instance == null) throw new ComparisonException("The item to compare was null.");
    }

    private bool HasDifference<T>(Service service, Table table, T instance)
    {
        // This method exists so it will stop evaluating the instance (hence stop using Reflection)
        // after the first difference is found.
        return (from row in table.Rows
                select row)
            .Any(row => ThePropertyDoesNotExist(instance, row) || TheValuesDoNotMatch(instance, row));
    }
}

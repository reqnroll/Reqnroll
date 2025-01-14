using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.RegularExpressions;
using static Dynamitey.Dynamic;

namespace Reqnroll.Assist.Dynamic;

public static class DynamicTableHelpers
{
    private const string ErrorMessagePropertyDiffSet = "Properties differs between the table and the set";
    private const string ErrorMessageInstanceTableFormat = "Can only create instances of tables with one row, or exactly 2 columns and several rows";
    private const string ErrorMessageNotOnTable = "The '{0}' value not present in the table, but on the instance";
    private const string ErrorMessageNotOnInstance = "The '{0}' value not present on the instance, but in the table";
    private const string ErrorMessageValueDiffers =
        "The '{0}' value differs from table and instance.\n\tInstance:\t'{1}'(type: {2}).\n\tTable:\t\t'{3}'(type: {4})";

    private const string ErrorMessageNumberOfRowsDiffers =
        "Number of rows for table ({0} rows) and set ({1} rows) differs";

    private const string ErrorMessageSetValuesDiffers =
        "A difference was found on row '{0}' for column '{1}' (property '{2}').\n\tInstance:\t'{3}'(type: {4}).\n\tTable:\t\t'{5}'(type: {6})";

    /// <summary>
    /// Create a dynamic object from the headers and values of the <paramref name="table"/>
    /// </summary>
    /// <param name="table">the table to create a dynamic object from</param>
    /// <param name="doTypeConversion">should types be converted according to conventions described in https://github.com/RajeshPoola/Reqnroll.Assist.Dynamic/wiki/Conversion-conventions#property-type-conversions</param>
    /// <returns>the created object</returns>
    public static ExpandoObject CreateDynamicInstance(this Table table, bool doTypeConversion = true)
    {
        if (table.Header.Count == 2 && table.RowCount > 1)
        {
            var horizontalTable = CreateHorizontalTable(table);
            return CreateDynamicInstance(horizontalTable.Rows[0], doTypeConversion);
        }

        if (table.RowCount == 1)
        {
            return CreateDynamicInstance(table.Rows[0], doTypeConversion);
        }

        throw new DynamicInstanceFromTableException(ErrorMessageInstanceTableFormat);
    }


    /// <summary>
    /// Creates a set of dynamic objects based of the <paramref name="table"/> headers and values
    /// </summary>
    /// <param name="table">the table to create a set of dynamics from</param>
    /// <param name="doTypeConversion">should types be converted according to conventions described in https://github.com/RajeshPoola/Reqnroll.Assist.Dynamic/wiki/Conversion-conventions#property-type-conversions</param>
    /// <returns>a set of dynamics</returns>
    public static IEnumerable<dynamic> CreateDynamicSet(this Table table, bool doTypeConversion = true)
    {
        return from r in table.Rows
               select CreateDynamicInstance(r, doTypeConversion);
    }

    /// <summary>
    /// Validates if a dynamic instance <paramref name="instance"/> matches the <paramref name="table"/>
    /// Throws descriptive exception if not
    /// </summary>
    /// <param name="table">the table to compare the instance against</param>
    /// <param name="instance">the instance to compare the table against</param>
    /// <param name="doTypeConversion">should types be converted according to conventions described in https://github.com/RajeshPoola/Reqnroll.Assist.Dynamic/wiki/Conversion-conventions#property-type-conversions</param>
    public static void CompareToDynamicInstance(this Table table, dynamic instance, bool doTypeConversion = true)
    {
        IList<string> propDiffs = GetPropertyDifferences(table, instance);
        if (propDiffs.Any())
            throw new DynamicInstanceComparisonException(propDiffs);

        AssertValuesOfRowDifference(table.Rows[0], instance, doTypeConversion);
    }

    /// <summary>
    /// Validates that the dynamic set <paramref name="set"/> matches the <paramref name="table"/>
    /// Throws descriptive exception if not
    /// </summary>
    /// <param name="table">the table to compare the set against</param>
    /// <param name="set">the set to compare the table against</param>
    /// <param name="doTypeConversion">should types be converted according to conventions described in https://github.com/RajeshPoola/Reqnroll.Assist.Dynamic/wiki/Conversion-conventions#property-type-conversions</param>
    public static void CompareToDynamicSet(this Table table, IList<dynamic> set, bool doTypeConversion = true)
    {
        AssertEqualNumberOfRows(table, set);

        IList<string> propDiffs = GetPropertyDifferences(table, set[0]);
        if (propDiffs.Any())
        {
            throw new DynamicSetComparisonException(ErrorMessagePropertyDiffSet, propDiffs);
        }

        // Now we know that the table and the list has the same number of rows and properties

        var valueDifference = GetSetValueDifferences(table, set, doTypeConversion);

        if (valueDifference.Any())
        {
            throw new DynamicSetComparisonException(ErrorMessagePropertyDiffSet, valueDifference);
        }
    }

    private static List<string> GetSetValueDifferences(Table table, IList<object> set, bool doTypeConversion = true)
    {
        var memberNames = GetMemberNames(set[0]);
        var valueDifference = new List<string>();

        for (var i = 0; i < set.Count; i++)
        {
            foreach (string memberName in memberNames)
            {
                var currentHeader = string.Empty;
                object rowValue = GetRowValue(i, table, memberName, out currentHeader, doTypeConversion);
                string rowType = rowValue.GetType().Name;
                dynamic instanceValue = InvokeGet(set[i], memberName);
                dynamic instanceType = instanceValue.GetType().Name;

                if (!instanceValue.Equals(rowValue))
                {
                    dynamic difference = string.Format(ErrorMessageSetValuesDiffers,
                                                       i + 1,
                                                       currentHeader,
                                                       memberName,
                                                       instanceValue,
                                                       instanceType,
                                                       rowValue,
                                                       rowType);

                    valueDifference.Add(difference);
                }
            }
        }
        return valueDifference;
    }

    private static object GetRowValue(int rowIndex, Table table, string memberName, out string currentHeader, bool doTypeConversion = true)
    {
        object rowValue = null;
        currentHeader = string.Empty;
        foreach (string header in table.Header)
        {
            if (CreatePropertyName(header) == memberName)
            {
                currentHeader = header;
                rowValue = CreateTypedValue(table.Rows[rowIndex][header], doTypeConversion);
                break;
            }
        }
        return rowValue;
    }

    private static void AssertValuesOfRowDifference(Reqnroll.DataTableRow tableRow, dynamic instance, bool doTypeConversion = true)
    {
        IList<string> valueDiffs = ValidateValuesOfRow(tableRow, instance, doTypeConversion);
        if (valueDiffs.Any())
            throw new DynamicInstanceComparisonException(valueDiffs);
    }

    private static IList<string> GetPropertyDifferences(Table table, dynamic instance, bool doTypeConversion = true)
    {
        var tableHeadersAsPropertyNames = table.Header.Select(CreatePropertyName);
        IEnumerable<string> instanceMembers = GetMemberNames(instance);

        return GetPropertyNameDifferences(tableHeadersAsPropertyNames, instanceMembers);
    }

    private static void AssertEqualNumberOfRows(Table table, IList<object> set)
    {
        if (table.RowCount != set.Count)
        {
            string mess = string.Format(ErrorMessageNumberOfRowsDiffers, table.RowCount, set.Count);
            throw new DynamicSetComparisonException(mess);
        }
    }

    private static IList<string> ValidateValuesOfRow(DataTableRow tableRow, dynamic instance, bool doTypeConversion = true)
    {
        var valueDiffs = new List<string>();

        foreach (string header in tableRow.Keys)
        {
            string propertyName = CreatePropertyName(header);
            dynamic valueFromInstance = InvokeGet(instance, propertyName);
            dynamic typeFromInstance = valueFromInstance.GetType().Name;
            object valueFromTable = CreateTypedValue(tableRow[header], doTypeConversion);
            string typeFromTable = valueFromTable.GetType().Name;

            if (!valueFromInstance.Equals(valueFromTable))
            {
                dynamic mess = string.Format(ErrorMessageValueDiffers, propertyName, valueFromInstance, typeFromInstance, valueFromTable, typeFromTable);
                valueDiffs.Add(mess);
            }
        }
        return valueDiffs;
    }

    private static IList<string> GetPropertyNameDifferences(IEnumerable<string> tableHeadersAsPropertyNames, IEnumerable<string> instanceMembers)
    {
        var allMembersInTableButNotInInstance = tableHeadersAsPropertyNames.Except(instanceMembers);
        var allMembersInInstanceButNotInTable = instanceMembers.Except(tableHeadersAsPropertyNames);

        var diffs = new List<string>();

        diffs.AddRange(
            allMembersInInstanceButNotInTable.Select(
                m => string.Format(ErrorMessageNotOnTable, m)));

        diffs.AddRange(
            allMembersInTableButNotInInstance.Select(
                m => string.Format(ErrorMessageNotOnInstance, m)));

        return diffs;
    }

    private static Table CreateHorizontalTable(Table verticalFieldValueTable)
    {
        var dic = verticalFieldValueTable.
            Rows.ToDictionary(row => row[0], row => row[1]);

        var horizontalTable = new Table(dic.Keys.ToArray());
        horizontalTable.AddRow(dic);
        return horizontalTable;
    }

    private static ExpandoObject CreateDynamicInstance(DataTableRow tablerow, bool doTypeConversion = true)
    {
        dynamic expando = new ExpandoObject();
        var dicExpando = expando as IDictionary<string, object>;

        foreach (string header in tablerow.Keys)
        {
            string propName = CreatePropertyName(header);
            object propValue = CreateTypedValue(tablerow[header], doTypeConversion);
            dicExpando.Add(propName, propValue);
        }

        return expando;
    }

    private static object CreateTypedValue(string valueFromTable, bool doTypeConversion = true)
    {
        if (!doTypeConversion)
            return valueFromTable;

        if (int.TryParse(valueFromTable, out int i))
            return i;

        if (double.TryParse(valueFromTable, out double db))
        {
            decimal d;
            if (decimal.TryParse(valueFromTable, out d) && d.Equals((decimal)db))
            {
                return db;
            }
            return d;
        }

        if (bool.TryParse(valueFromTable, out bool b))
            return b;

        if (DateTime.TryParse(valueFromTable, out var dt))
            return dt;

        return valueFromTable;
    }

    private static string CreatePropertyName(string header)
    {
        string cleanedHeader = RemoveReservedChars(header);
        string propName = FixCasing(cleanedHeader);

        // Throw if no chars in string
        if (propName.Length != 0) return propName;

        var mess = $"Property '{header}' only contains reserved C# characters";
        throw new DynamicInstanceFromTableException(mess);
    }

    private static string FixCasing(string header)
    {
        string[] arr = header.Split(' ');
        string propName = arr[0]; // leave the first element as is, since it might be correct cased...

        for (var i = 1; i < arr.Length; i++)
        {
            string s = arr[i];
            if (s.Length > 0)
            {
                propName += s[0].ToString().ToUpperInvariant() +
                            s.Substring(1).ToLowerInvariant();
            }
        }

        return propName;
    }

    private static string RemoveReservedChars(string orgPropertyName)
    {
        const string pattern = @"[^\w\s]";
        const string replacement = "";
        return Regex.Replace(orgPropertyName, pattern, replacement);
    }
}
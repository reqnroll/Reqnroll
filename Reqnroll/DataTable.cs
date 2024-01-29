using System;
using System.ComponentModel;
using System.Globalization;

namespace Reqnroll;

/// <summary>
/// Represents a Gherkin Data Table.
/// </summary>
[TypeConverter(typeof(DataTableConverter))]
public class DataTable : Table
{
    public DataTable(params string[] header) : base(header)
    {
    }

    protected internal DataTable(string[] header, string[][] dataRows) : base(header, dataRows)
    {
    }

    protected internal DataTable(Table copyFrom) : base(copyFrom)
    {
    }

    public static DataTable FromTable(Table table) => new(table);

    private class DataTableConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return typeof(Table).IsAssignableFrom(sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is DataTable dataTable)
                return dataTable;

            if (value is Table tableValue)
            {
                return new DataTable(tableValue);
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(Table);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(DataTable) || value == null)
                return value;

            if (destinationType == typeof(Table) && value is DataTable dataTable)
            {
                return new Table(dataTable);
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

}

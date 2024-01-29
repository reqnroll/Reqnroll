using System.ComponentModel;
using System.Globalization;
using System;

// ReSharper disable once CheckNamespace
namespace TechTalk.SpecFlow;

[TypeConverter(typeof(SpecFlowTableConverter))]
public class Table : Reqnroll.Table
{
    public Table(params string[] header) : base(header)
    {
    }

    protected internal Table(Reqnroll.Table copyFrom) : base(copyFrom)
    {
    }

    private class SpecFlowTableConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return typeof(Reqnroll.Table).IsAssignableFrom(sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is Table specFlowTable)
                return specFlowTable;

            if (value is Reqnroll.Table tableValue)
            {
                return new Table(tableValue);
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(Reqnroll.Table) || 
                   destinationType == typeof(Reqnroll.DataTable);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(Table) || value == null)
                return value;

            if (destinationType == typeof(Reqnroll.Table) && value is Table specFlowTable1)
            {
                return new Reqnroll.Table(specFlowTable1);
            }

            if (destinationType == typeof(Reqnroll.DataTable) && value is Table specFlowTable2)
            {
                return new Reqnroll.DataTable(specFlowTable2);
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}


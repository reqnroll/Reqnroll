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
}

internal class SpecFlowTableConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
        return typeof(Reqnroll.Table).IsAssignableFrom(sourceType);
    }

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        if (value == null) 
            return null;

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
        return destinationType == typeof(Reqnroll.Table);
    }

    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
        if (destinationType == typeof(Table) || value == null) 
            return value;

        if (destinationType == typeof(Reqnroll.Table) && value is Table specFlowTable)
        {
            return new Reqnroll.Table(specFlowTable);
        }

        return base.ConvertTo(context, culture, value, destinationType);
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Reqnroll.Formatters.PayloadProcessing;

public class DescriptionEnumConverter<T> : JsonConverter<T> where T : struct, Enum
{
    private readonly Dictionary<T, string> _enumToString = new();
    private readonly Dictionary<string, T> _stringToEnum = new();

    public DescriptionEnumConverter()
    {
        var type = typeof(T);
        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
#pragma warning disable CS8605 // Unboxing a possibly null value.
            var value = (T)field.GetValue(null);
#pragma warning restore CS8605 // Unboxing a possibly null value.
            var attribute = field.GetCustomAttribute<DescriptionAttribute>();
            if (attribute == null || string.IsNullOrEmpty(attribute.Description))
                throw new InvalidOperationException($"Enum {type.Name} field {field.Name} does not have a Description attribute or the Description attribute is empty.");
            var name = attribute.Description;
            _enumToString[value] = name;
            _stringToEnum[name] = value;
        }
    }

    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var stringValue = reader.GetString();
        return _stringToEnum.TryGetValue(stringValue!, out var enumValue) ? enumValue : default;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(_enumToString.TryGetValue(value, out var stringValue) ? stringValue : value.ToString());
    }
}
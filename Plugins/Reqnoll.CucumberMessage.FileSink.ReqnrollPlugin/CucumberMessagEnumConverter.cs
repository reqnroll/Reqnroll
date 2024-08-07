using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cucumber.Messages
{
    internal class CucumberMessageEnumConverter<T> : JsonConverter<T> where T : struct, Enum
    {
        private readonly Dictionary<T, string> _enumToString = new();
        private readonly Dictionary<string, T> _stringToEnum = new();

        protected internal CucumberMessageEnumConverter()
        {
            var type = typeof(T);
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var value = (T)field.GetValue(null)!;
                var attribute = field.GetCustomAttribute<DescriptionAttribute>();
                var name = attribute?.Description ?? field.Name;
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

}

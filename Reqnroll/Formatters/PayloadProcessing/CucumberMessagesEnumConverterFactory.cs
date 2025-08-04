using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Reqnroll.Formatters.PayloadProcessing;

public class CucumberMessagesEnumConverterFactory : JsonConverterFactory
{
    private static readonly ConcurrentDictionary<Type, JsonConverter> _cache = new();
    private static readonly HashSet<Type> _enumTypes;

    static CucumberMessagesEnumConverterFactory()
    {
        // Discover all enums in Io.Cucumber.Messages.Types
        var typesNamespace = "Io.Cucumber.Messages.Types";
        var enumTypes = AppDomain.CurrentDomain.GetAssemblies()
                                 .SelectMany(a => SafeGetTypes(a))
                                 .Where(t => t.IsEnum && t.Namespace == typesNamespace)
                                 .ToList();
        _enumTypes = new HashSet<Type>(enumTypes);
    }

    private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
    {
        try { return assembly.GetTypes(); } catch { return Array.Empty<Type>(); }
    }

    public override bool CanConvert(Type typeToConvert)
    {
        return _enumTypes.Contains(typeToConvert);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return _cache.GetOrAdd(typeToConvert, t =>
        {
            var converterType = typeof(DescriptionEnumConverter<>).MakeGenericType(t);
            return (JsonConverter)Activator.CreateInstance(converterType);
        });
    }
}
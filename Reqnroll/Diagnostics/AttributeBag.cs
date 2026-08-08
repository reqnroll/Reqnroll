#nullable enable

using System.Collections.Generic;

namespace Reqnroll.Diagnostics;

/// <summary>
/// A bag of attributes that add context to an operation.
/// </summary>
public class AttributeBag
{
    private readonly Dictionary<string, object> _values = [];

    public void Add(string key, object value) => _values.Add(key, value);

    public IReadOnlyDictionary<string, object> AsDictionary() => _values;
}

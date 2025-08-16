using System;
using System.Collections.Generic;
using Reqnroll.Bindings;

// the exceptions are part of the public API, keep them in Reqnroll namespace
// ReSharper disable once CheckNamespace
namespace Reqnroll;

/// <summary>
/// This subclass is added for support of Cucumber Messages.
/// When emitting the Cucumber Message that describes an ambiguous matching situation, the Message will contain the list of possible matches.
/// We use this subclass of BindingException to convey that information.
/// </summary>
public class AmbiguousBindingException : BindingException
{
    public IEnumerable<BindingMatch> Matches { get; private set; } = [];

    public AmbiguousBindingException(string message) : base(message)
    {
    }

    public AmbiguousBindingException(string message, Exception inner) : base(message, inner)
    {
    }

    public AmbiguousBindingException(string message, IEnumerable<BindingMatch> matches) : base(message)
    {
        Matches = [.. matches];
    }
}

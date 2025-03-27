// Contains code from https://github.com/xunit/samples.xunit
// originally published under Apache 2.0 license
// For more information see aforementioned repository
using System;

namespace Reqnroll.xUnit3.ReqnrollPlugin;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class AssemblyFixtureAttribute(Type fixtureType) : Attribute
{
    public Type FixtureType { get; } = fixtureType;
}
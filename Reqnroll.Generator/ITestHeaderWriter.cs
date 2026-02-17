using System;

namespace Reqnroll.Generator;

[Obsolete("This interface is not used anymore and will be removed in v4")]
public interface ITestHeaderWriter;

#pragma warning disable CS0618 // Type or member is obsolete
public class ObsoleteTestHeaderWriter : ITestHeaderWriter;
#pragma warning restore CS0618 // Type or member is obsolete

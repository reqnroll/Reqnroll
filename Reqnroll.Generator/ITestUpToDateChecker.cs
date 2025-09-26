using System;

namespace Reqnroll.Generator;

[Obsolete("This interface is not used anymore and will be removed in v4")]
public interface ITestUpToDateChecker;

#pragma warning disable CS0618 // Type or member is obsolete
public class ObsoleteTestUpToDateChecker : ITestUpToDateChecker;
#pragma warning restore CS0618 // Type or member is obsolete

using System;
using System.Globalization;
using Reqnroll.BoDi;

namespace Reqnroll;

public interface IFeatureContext : IReqnrollContext
{
    FeatureInfo FeatureInfo { get; }

    CultureInfo BindingCulture { get; }

    IObjectContainer FeatureContainer { get; }

    Exception BeforeFeatureHookError { get; }
    bool BeforeFeatureHookFailed { get; }
}

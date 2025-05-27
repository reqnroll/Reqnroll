using System;
using System.Globalization;
using Reqnroll.BoDi;
using Reqnroll;

// ReSharper disable once CheckNamespace
namespace TechTalk.SpecFlow;

public class FeatureContext(Reqnroll.FeatureContext originalContext) : IFeatureContext
{
    #region Singleton
    [Obsolete("Please get the FeatureContext via Context Injection - https://go.reqnroll.net/doc-migrate-fc-current")]
    public static Reqnroll.FeatureContext Current => Reqnroll.FeatureContext.Current;
    #endregion

    public Exception TestError => originalContext.TestError;

    public FeatureInfo FeatureInfo => originalContext.FeatureInfo;

    public CultureInfo BindingCulture => originalContext.BindingCulture;

    public IObjectContainer FeatureContainer => originalContext.FeatureContainer;

    public Exception BeforeFeatureHookError => originalContext.BeforeFeatureHookError;

    public bool BeforeFeatureHookFailed => originalContext.BeforeFeatureHookFailed;
}

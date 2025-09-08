using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using Reqnroll.BoDi;
using Reqnroll.Configuration;

namespace Reqnroll;

public class FeatureContext : ReqnrollContext, IFeatureContext
{
    internal FeatureContext(IObjectContainer featureContainer, FeatureInfo featureInfo, ReqnrollConfiguration reqnrollConfiguration)
    {
        Stopwatch = new Stopwatch();
        Stopwatch.Start();

        FeatureContainer = featureContainer;
        FeatureInfo = featureInfo;
        // The Generator defines the value of FeatureInfo.Language: either feature-language or language from App.config or the default
        // The runtime can define the binding-culture: Value is configured on App.config, else it is null
        BindingCulture = reqnrollConfiguration.BindingCulture ?? featureInfo.Language;
    }

    #region Singleton
    private static bool _isCurrentDisabled = false;
    private static FeatureContext _current;

    [Obsolete("Please get the FeatureContext via Context Injection - https://go.reqnroll.net/doc-migrate-fc-current")]
    public static FeatureContext Current
    {
        get
        {
            if (_isCurrentDisabled)
                throw new ReqnrollException("The FeatureContext.Current static accessor cannot be used in multi-threaded execution. Try injecting the feature context to the binding class. See https://go.reqnroll.net/doc-parallel-execution for details.");
            if (_current == null)
            {
                Debug.WriteLine("Accessing NULL FeatureContext");
            }
            return _current;
        }
        internal set
        {
            if (!_isCurrentDisabled)
                _current = value;
        }
    }

    internal static void DisableSingletonInstance()
    {
        _isCurrentDisabled = true;
        Thread.MemoryBarrier();
        _current = null;
    }
    #endregion

    public FeatureInfo FeatureInfo { get; }
    public CultureInfo BindingCulture { get; }
    public IObjectContainer FeatureContainer { get; }
    internal Stopwatch Stopwatch { get; }

    // these properties are used by the generated code to skip scenario execution of features with a failing before feature hook
    public Exception BeforeFeatureHookError { get; internal set; }
    public bool BeforeFeatureHookFailed => BeforeFeatureHookError != null;
}
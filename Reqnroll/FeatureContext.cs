using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using BoDi;
using Reqnroll.Configuration;

namespace Reqnroll
{
    public interface IFeatureContext : IReqnrollContext
    {
        FeatureInfo FeatureInfo { get; }

        CultureInfo BindingCulture { get; }

        IObjectContainer FeatureContainer { get; }
    }

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
        private static bool isCurrentDisabled = false;
        private static FeatureContext current;

        [Obsolete("Please get the FeatureContext via Context Injection - https://go.reqnroll.net/doc-migrate-fc-current")]
        public static FeatureContext Current
        {
            get
            {
                if (isCurrentDisabled)
                    throw new ReqnrollException("The FeatureContext.Current static accessor cannot be used in multi-threaded execution. Try injecting the feature context to the binding class. See https://go.reqnroll.net/doc-parallel-execution for details.");
                if (current == null)
                {
                    Debug.WriteLine("Accessing NULL FeatureContext");
                }
                return current;
            }
            internal set
            {
                if (!isCurrentDisabled)
                    current = value;
            }
        }

        internal static void DisableSingletonInstance()
        {
            isCurrentDisabled = true;
            Thread.MemoryBarrier();
            current = null;
        }
        #endregion

        public FeatureInfo FeatureInfo { get; }
        public CultureInfo BindingCulture { get; }
        public IObjectContainer FeatureContainer { get; }
        internal Stopwatch Stopwatch { get; }
    }
}
using System;
using System.Globalization;
using System.Threading;
using Reqnroll.Tracing;

namespace Reqnroll
{
    public readonly struct CultureInfoScope : IDisposable
    {
        private readonly CultureInfo originalCultureInfo;

        public CultureInfoScope(FeatureContext featureContext)
        {
            originalCultureInfo = null;
            var cultureInfo = featureContext?.BindingCulture;
            if (cultureInfo is not null)
            {
                var current = Thread.CurrentThread.CurrentCulture;
                if (!cultureInfo.Equals(current))
                {
                    if (cultureInfo.IsNeutralCulture)
                    {
                        cultureInfo = LanguageHelper.GetSpecificCultureInfo(cultureInfo);
                    }

                    originalCultureInfo = current;
                    Thread.CurrentThread.CurrentCulture = cultureInfo;
                }
            }
        }

        public void Dispose()
        {
            if (originalCultureInfo is not null)
            {
                Thread.CurrentThread.CurrentCulture = originalCultureInfo;
            }
        }
    }
}
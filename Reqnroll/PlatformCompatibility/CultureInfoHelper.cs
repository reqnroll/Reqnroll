using System.Globalization;

namespace Reqnroll.PlatformCompatibility
{
	internal static class CultureInfoHelper
    {
        public static CultureInfo GetCultureInfo(string cultureName)
        {
            return CultureInfo.GetCultureInfo(cultureName);
        }
    }
}

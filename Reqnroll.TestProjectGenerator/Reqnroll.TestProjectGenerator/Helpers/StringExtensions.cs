using System;

namespace Reqnroll.TestProjectGenerator.Helpers
{
    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string value)
        {
            return String.IsNullOrEmpty(value);
        }

        public static bool IsNotNullOrEmpty(this string value)
        {
            return !String.IsNullOrEmpty(value);
        }

        public static bool IsNullOrWhiteSpace(this String value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        public static bool IsNotNullOrWhiteSpace(this String value)
        {
            return !value.IsNullOrWhiteSpace();
        }

        public static string StripWhitespaces(this String value)
        {
            return value.Replace(" ", "").Replace("\n", "").Replace("\r", "").Replace("\t", "");
        }

        public static string[] SplitByString(this string value, string separator, StringSplitOptions options)
        {
            return value.Split(new string[] { separator }, options);
        }
    }
}

using System;
using System.Text.RegularExpressions;

namespace Reqnroll.RuntimeTests.BindingSkeletons
{
    internal static class StringHelpers
    {
        static private readonly Regex lineEndingRe = new Regex(@"\r?\n");
        public static string ConsolidateVerbatimStringLineEndings(string message)
        {
            return lineEndingRe.Replace(message, Environment.NewLine);
        }
    }
}

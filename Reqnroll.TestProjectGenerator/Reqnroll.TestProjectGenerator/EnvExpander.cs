using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Reqnroll.TestProjectGenerator
{
    public static class EnvironmentExtensions
    {
        private static readonly List<string> _envVariablesWithPaths = new List<string>
        {
            "TMP",
            "TEMP"
        };

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern int GetLongPathName(string path, StringBuilder longPath, int longPathLength);

        public static string ExpandEnvironmentWithLongFilenames(string text, bool escapeBackslash = true)
        {
            var result = text;
            foreach (var envVariablesWithPath in _envVariablesWithPaths)
            {
                if (result.Contains(envVariablesWithPath))
                {
                    var envVariableName = $"%{envVariablesWithPath}%";
                    var envVarValue = Environment.ExpandEnvironmentVariables(envVariableName);

                    var longPath = new StringBuilder(255);
                    GetLongPathName(envVarValue, longPath, longPath.Capacity);

                    if (escapeBackslash)
                    {
                        result = result.Replace(envVariableName, longPath.ToString().Replace("\\", "\\\\"));
                    }
                }
            }

            result = Environment.ExpandEnvironmentVariables(result);

            return result;
        }
    }
}
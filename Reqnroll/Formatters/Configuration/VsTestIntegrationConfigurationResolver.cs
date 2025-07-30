using Reqnroll.MessagesLogger.TestLogger;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;


namespace Reqnroll.Formatters.Configuration
{
    public class VsTestIntegrationConfigurationResolver : FormattersConfigurationResolverBase, IFormattersConfigurationResolver
    {
 
        protected override JsonDocument GetJsonDocument()
        {
            var hasOverrides = FormattersLogger.IsInitialized && FormattersLogger.HasParameters;

            if (hasOverrides)
            {
                var parameters = FormattersLogger.Parameters;
                var jsonBuilder = new StringBuilder();
                jsonBuilder.Append("{");

                foreach (var parameter in parameters)
                {
                    jsonBuilder.Append($"\"{parameter.Key}\": {ReconstructJsonValue(parameter.Value)},");
                }

                if (jsonBuilder.Length > 1)
                    jsonBuilder.Remove(jsonBuilder.Length - 1, 1);

                jsonBuilder.Append("}");
                var jsonString = jsonBuilder.ToString();
                return JsonDocument.Parse(jsonString);
            }

            return JsonDocument.Parse("{}");
        }

        /// <summary>
        /// Reconstructs a JSON value from a string that is missing braces and quotes, recursively.
        /// </summary>
        internal string ReconstructJsonValue(string value)
        {
            value = value.Trim();

            // Try to split at the first unescaped colon at top level
            var kv = SplitTopLevel(value, ':');
            if (kv.Length == 2 && !kv[0].Contains(",") && !kv[0].Contains("{") && !kv[0].Contains("["))
            {
                // Looks like a single key-value pair, treat as object
                var key = kv[0].Trim();
                var val = kv[1].Trim();
                return $"{{\"{RemoveEscapes(key)}\": {ReconstructJsonValue(val)}}}";
            }
            else if (value.Contains(":") && value.Contains(","))
            {
                // Looks like multiple key-value pairs, treat as object
                var objBuilder = new StringBuilder();
                objBuilder.Append("{");

                var pairs = SplitTopLevel(value, ',');
                foreach (var pair in pairs)
                {
                    var pairKv = SplitTopLevel(pair, ':');
                    if (pairKv.Length == 2)
                    {
                        var key = pairKv[0].Trim();
                        var val = pairKv[1].Trim();
                        objBuilder.Append($"\"{RemoveEscapes(key)}\": {ReconstructJsonValue(val)},");
                    }
                    else
                    {
                        // fallback: treat as string
                        objBuilder.Append($"\"{RemoveEscapes(pair.Trim())}\",");
                    }
                }
                if (objBuilder.Length > 1)
                    objBuilder.Remove(objBuilder.Length - 1, 1);
                objBuilder.Append("}");
                return objBuilder.ToString();
            }
            else
            {
                // treat as string value, escape quotes and remove escape characters
                return $"\"{RemoveEscapes(value.Replace("\"", "\\\""))}\"";
            }
        }

        /// <summary>
        /// Removes escape characters (back-tick) used for separators.
        /// </summary>
        private string RemoveEscapes(string input)
        {
            // Remove back-tick used for escaping separators
            return input.Replace("\\,", ",").Replace("\\:", ":").Replace("\\{", "{").Replace("\\}", "}").Replace("\\[", "[").Replace("\\]", "]").Replace("\\\\", "\\");
        }

        /// <summary>
        /// Splits a string by a separator, but only at the top level (not inside nested braces).
        /// </summary>
        private string[] SplitTopLevel(string input, char separator)
        {
            var result = new List<string>();
            int depth = 0;
            int lastPos = 0;
            bool escape = false;
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (escape)
                {
                    escape = false;
                    continue;
                }
                if (c == '\\')
                {
                    escape = true;
                    continue;
                }
                if (c == '{' || c == '[')
                    depth++;
                else if (c == '}' || c == ']')
                    depth--;
                else if (c == separator && depth == 0)
                {
                    result.Add(input.Substring(lastPos, i - lastPos));
                    lastPos = i + 1;
                }
            }
            if (lastPos < input.Length)
                result.Add(input.Substring(lastPos));
            return result.ToArray();
        }
    }
}

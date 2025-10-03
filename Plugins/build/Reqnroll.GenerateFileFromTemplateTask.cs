// Read the entire template file
var template = File.ReadAllText(TemplateFile);

// Build a dictionary of tokens for quick lookup
var dict = new Dictionary<string, string>(StringComparer.Ordinal);
foreach (var token in Tokens ?? Array.Empty<Microsoft.Build.Framework.ITaskItem>())
{
    var key = token.ItemSpec;
    var value = token.GetMetadata("Value");
    if (!string.IsNullOrEmpty(key))
    {
        dict[key] = value ?? string.Empty;
    }
}

// Single-pass replacement to avoid compound replacements
var sb = new StringBuilder(template.Length * 2);
for (int i = 0; i < template.Length;)
{
    bool matched = false;
    foreach (var kvp in dict)
    {
        var token = kvp.Key;
        if (i + token.Length <= template.Length && string.CompareOrdinal(template, i, token, 0, token.Length) == 0)
        {
            Log.LogMessage($"Replaced token {token}");

            sb.Append(kvp.Value);
            i += token.Length;
            matched = true;
            break;
        }
    }

    if (!matched)
    {
        sb.Append(template[i]);
        i++;
    }
}

// Ensure output directory exists
Directory.CreateDirectory(Path.GetDirectoryName(OutputFile) ?? ".");
File.WriteAllText(OutputFile, sb.ToString());

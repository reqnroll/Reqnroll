
using System.Text;

namespace Reqnroll.CodeAnalysis.Gherkin.SyntaxGenerator;

internal static class NamingHelper
{
    public static string PascalCaseToCamelCase(string name)
    {
        var sb = new StringBuilder(name.Length);

        sb.Append(char.ToLowerInvariant(name[0]));

        for (var i = 1; i < name.Length; i++)
        {
            sb.Append(name[i]);
        }

        return sb.ToString();
    }

    public static string PascalCaseToLowercaseWords(string name)
    {
        var sb = new StringBuilder();

        if (string.IsNullOrEmpty(name))
        {
            return sb.ToString();
        }

        var wordStartIndex = 0;
        var first = true;

        for (var i = 1; i < name.Length; i++)
        {
            if (!char.IsUpper(name[i]))
            {
                continue;
            }

            if (first)
            {
                first = false;
            }
            else
            {
                sb.Append(' ');
            }

            foreach (var c in name.AsSpan(wordStartIndex, i - wordStartIndex))
            {
                sb.Append(char.ToLowerInvariant(c));
            }

            wordStartIndex = i;
        }

        if (wordStartIndex < name.Length - 1)
        {
            sb.Append(' ');

            foreach (var c in name.AsSpan(wordStartIndex))
            {
                sb.Append(char.ToLowerInvariant(c));
            }
        }

        return sb.ToString();
    }
}

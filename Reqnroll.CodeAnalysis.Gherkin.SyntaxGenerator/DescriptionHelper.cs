using System.Diagnostics;
using System.Text;

namespace Reqnroll.CodeAnalysis.Gherkin.SyntaxGenerator;

internal static class DescriptionHelper
{
    internal static string DescribeSlot(SyntaxSlotPropertyInfo slotInfo)
    {
        var summary = slotInfo.Description;

        if (string.IsNullOrEmpty(summary))
        {
            return $"The {NamingHelper.PascalCaseToLowercaseWords(slotInfo.Name)}.";
        }

        if (summary!.StartsWith("Gets ", StringComparison.InvariantCultureIgnoreCase) && summary.Length > 5)
        {
            var sb = new StringBuilder();

            sb.Append(char.ToUpperInvariant(summary[5]));

            for (var i = 6; i < summary.Length; i++)
            {
                sb.Append(summary[i]);
            }

            return sb.ToString();
        }

        return summary;
    }
}

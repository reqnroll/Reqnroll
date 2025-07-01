using System.Globalization;

namespace Reqnroll.CodeAnalysis.Gherkin;

public sealed class GherkinParseOptions
{
    /// <summary>
    /// The default parse options.
    /// </summary>
    public static GherkinParseOptions Default { get; } = new();

    /// <summary>
    /// Gets the culture to use when parsing text. This determines the language that will be used for the Gherkin keywords.
    /// If the document contains a language directive, the directive takes precedent.
    /// </summary>
    public CultureInfo Culture { get; private set; }

    internal GherkinParseOptions(CultureInfo? culture = null)
    {
        Culture = culture ?? CultureInfo.CurrentCulture;
    }

    private GherkinParseOptions(GherkinParseOptions other)
    {
        Culture = other.Culture;
    }

    /// <summary>
    /// Returns a <see cref="GherkinParseOptions"/> instance for a specified culture.
    /// </summary>
    /// <param name="culture">The culture to use.</param>
    /// <returns>If the culture is different, a new instance of <see cref="GherkinParseOptions"/> with the specified culture;
    /// otherwise the current options instance.</returns>
    public GherkinParseOptions WithCulture(CultureInfo culture)
    {
        if (culture == Culture)
        {
            return this;
        }

        return new GherkinParseOptions(this) { Culture = culture };
    }
}

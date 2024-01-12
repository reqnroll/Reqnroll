using System;
using Reqnroll.Configuration;

namespace Reqnroll.Tracing;

public interface IColorOutputHelper
{
    bool EmitAnsiColorCodes { get; }

    string Colorize(string text, AnsiColor.AnsiColor color);
}

public class ColorOutputHelper : IColorOutputHelper
{
    private readonly ReqnrollConfiguration _reqnrollConfiguration;
    private readonly bool _forceNoColor;

    public bool EmitAnsiColorCodes => _reqnrollConfiguration.ColoredOutput && !_forceNoColor;

    public ColorOutputHelper(ReqnrollConfiguration reqnrollConfiguration)
    {
        _reqnrollConfiguration = reqnrollConfiguration;
        _forceNoColor = Environment.GetEnvironmentVariable("NO_COLOR") is not null;
    }

    public string Colorize(string text, AnsiColor.AnsiColor color)
    {
        return !EmitAnsiColorCodes ? text : color.Colorize(text);
    }
}

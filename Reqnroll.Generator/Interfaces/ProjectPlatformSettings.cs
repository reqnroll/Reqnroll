namespace Reqnroll.Generator.Interfaces;

public class ProjectPlatformSettings
{
    /// <summary>
    /// Specifies the programming language of the project. Optional, defaults to C# 3.0.
    /// </summary>
    public string Language { get; set; } = GenerationTargetLanguage.CSharp;
}
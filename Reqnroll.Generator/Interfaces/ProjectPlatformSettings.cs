using System;

namespace Reqnroll.Generator.Interfaces
{
    /// IMPORTANT
    /// This class is used for interop with the Visual Studio Extension
    /// DO NOT REMOVE OR RENAME FIELDS!
    /// This breaks binary serialization across appdomains
    [Serializable]
    public class ProjectPlatformSettings
    {
        /// <summary>
        /// Specifies the programming language of the project. Optional, defaults to C# 3.0.
        /// </summary>
        public string Language { get; set; } = GenerationTargetLanguage.CSharp;
    }
}
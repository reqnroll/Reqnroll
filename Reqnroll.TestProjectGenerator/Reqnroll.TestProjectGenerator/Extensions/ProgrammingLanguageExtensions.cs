using System;

namespace Reqnroll.TestProjectGenerator.Extensions
{
    public static class ProgrammingLanguageExtensions
    {

        public static string ToProjectFileExtension(this ProgrammingLanguage programmingLanguage)
        {
            switch (programmingLanguage)
            {
                case ProgrammingLanguage.CSharp73: return "csproj";
                case ProgrammingLanguage.CSharp: return "csproj";
                case ProgrammingLanguage.FSharp: return "fsproj";
                case ProgrammingLanguage.VB: return "vbproj";
                default:
                    throw new NotSupportedException(
                        "There is no known project file extension for the programming language.");
            }
        }

        public static string ToCodeFileExtension(this ProgrammingLanguage programmingLanguage)
        {
            switch (programmingLanguage)
            {
                case ProgrammingLanguage.CSharp73: return "cs";
                case ProgrammingLanguage.CSharp: return "cs";
                case ProgrammingLanguage.FSharp: return "fs";
                case ProgrammingLanguage.VB: return "vb";
                default: throw new NotSupportedException("There is no known file extension for the programming language.");
            }
        }
    }
}

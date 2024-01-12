using System;
using Reqnroll.Generator.CodeDom;
using Reqnroll.Utils;

namespace Reqnroll.Generator
{
    public static class GenerationTargetLanguage
    {
        public const string CSharp = "C#";
        public const string VB = "VB";

        public static string GetExtension(string programmingLanguage)
        {
            switch (programmingLanguage)
            {
                case CSharp:
                    return ".cs";
                case VB:
                    return ".vb";
                default:
                    throw new NotSupportedException("Programming language not supported: " + programmingLanguage);
            }
        }

        public static CodeDomHelper CreateCodeDomHelper(string programmingLanguage)
        {
            switch (programmingLanguage)
            {
                case CSharp:
                    return new CodeDomHelper(CodeDomProviderLanguage.CSharp);
                case VB:
                    return new CodeDomHelper(CodeDomProviderLanguage.VB);
                default:
                    throw new NotSupportedException("Programming language not supported: " + programmingLanguage);
            }
        }
    }
}
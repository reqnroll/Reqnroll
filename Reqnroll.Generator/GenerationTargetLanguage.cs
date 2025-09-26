using System;
using Reqnroll.Generator.CodeDom;

namespace Reqnroll.Generator;

public static class GenerationTargetLanguage
{
    public const string CSharp = "C#";
    public const string VB = "VB";

    public static void AssertSupported(string programmingLanguage)
    {
        if (programmingLanguage != CSharp && programmingLanguage != VB)
        {
            throw new NotSupportedException("Programming language not supported: " + programmingLanguage);
        }
    }

    public static CodeDomHelper CreateCodeDomHelper(string programmingLanguage)
    {
        AssertSupported(programmingLanguage);
        switch (programmingLanguage)
        {
            case CSharp:
                return new CodeDomHelper(CodeDomProviderLanguage.CSharp);
            case VB:
                return new CodeDomHelper(CodeDomProviderLanguage.VB);
            default:
                throw new NotSupportedException(); // never happens
        }
    }
}
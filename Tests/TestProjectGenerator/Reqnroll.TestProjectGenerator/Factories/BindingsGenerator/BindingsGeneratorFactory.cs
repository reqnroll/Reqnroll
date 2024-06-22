using System;

namespace Reqnroll.TestProjectGenerator.Factories.BindingsGenerator
{
    public class BindingsGeneratorFactory
    {
        public BaseBindingsGenerator FromLanguage(ProgrammingLanguage targetLanguage)
        {
            switch (targetLanguage)
            {
                case ProgrammingLanguage.CSharp73: return new CSharpBindingsGenerator();
                case ProgrammingLanguage.CSharp: return new CSharp10BindingsGenerator();
                case ProgrammingLanguage.FSharp: return new FSharpBindingsGenerator();
                case ProgrammingLanguage.VB: return new VbBindingsGenerator();
                default: throw new ArgumentException(
                        $"Target language generator not defined for {targetLanguage}.",
                        nameof(targetLanguage));
            }
        }
    }
}

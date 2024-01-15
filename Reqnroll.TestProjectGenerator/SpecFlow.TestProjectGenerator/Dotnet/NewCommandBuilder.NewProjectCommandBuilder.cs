using System.IO;

namespace TechTalk.SpecFlow.TestProjectGenerator.Dotnet
{
    public partial class NewCommandBuilder
    {
        public class NewProjectCommandBuilder : BaseCommandBuilder
        {
            private string _templateName = "classlib";
            private string _name = "ClassLib";
            private string _folder;
            private ProgrammingLanguage _language = ProgrammingLanguage.CSharp;


            public NewProjectCommandBuilder(IOutputWriter outputWriter) : base(outputWriter)
            {
            }

            public NewProjectCommandBuilder UsingTemplate(string templateName)
            {
                _templateName = templateName;
                return this;
            }

            public NewProjectCommandBuilder WithName(string name)
            {
                _name = name;
                return this;
            }

            public NewProjectCommandBuilder InFolder(string folder)
            {
                _folder = folder;
                return this;
            }

            public NewProjectCommandBuilder WithLanguage(ProgrammingLanguage language)
            {
                _language = language;
                return this;
            }

            protected override string GetWorkingDirectory()
            {
                return Path.GetDirectoryName(_folder);
            }

            protected override string BuildArguments()
            {
                var arguments = AddArgument($"new {_templateName}", "-o", "\"" + _folder + "\"");
                arguments = AddArgument(
                    arguments,
                    "-lang",
                    _language == ProgrammingLanguage.CSharp ? "\"C#\"" :
                    _language == ProgrammingLanguage.CSharp10 ? "\"C#\"" :
                    _language == ProgrammingLanguage.VB ? "VB" :
                    _language == ProgrammingLanguage.FSharp ? "\"F#\"" : string.Empty);

                return arguments;
            }
        }
    }

}

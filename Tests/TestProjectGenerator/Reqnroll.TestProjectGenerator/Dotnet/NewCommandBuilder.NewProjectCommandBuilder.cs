using System.IO;

namespace Reqnroll.TestProjectGenerator.Dotnet
{
    public partial class NewCommandBuilder
    {
        public class NewProjectCommandBuilder : BaseCommandBuilder
        {
            protected string _templateName = "classlib";
            protected string _name = "ClassLib";
            protected string _folder;
            protected ProgrammingLanguage _language = ProgrammingLanguage.CSharp;


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
                var arguments = AddArgument($"new {_templateName} --no-update-check --no-restore", "-o", "\"" + _folder + "\"");
                arguments = AddArgument(
                    arguments,
                    "-lang",
                    _language == ProgrammingLanguage.CSharp73 ? "\"C#\"" :
                    _language == ProgrammingLanguage.CSharp ? "\"C#\"" :
                    _language == ProgrammingLanguage.VB ? "VB" :
                    _language == ProgrammingLanguage.FSharp ? "\"F#\"" : string.Empty);

                if (_language == ProgrammingLanguage.CSharp73)
                {
                    arguments = AddArgument(arguments, "--langVersion", "7.3");
                }

                return arguments;
            }
        }
    }

}

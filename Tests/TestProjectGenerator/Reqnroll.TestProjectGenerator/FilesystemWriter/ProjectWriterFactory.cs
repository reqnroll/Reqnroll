using Reqnroll.TestProjectGenerator.Data;

namespace Reqnroll.TestProjectGenerator.FilesystemWriter
{
    public class ProjectWriterFactory
    {
        private readonly IOutputWriter _outputWriter;
        private readonly TargetFrameworkMonikerStringBuilder _targetFrameworkMonikerStringBuilder;
        private readonly TargetFrameworkVersionStringBuilder _targetFrameworkVersionStringBuilder;

        public ProjectWriterFactory(IOutputWriter outputWriter, TargetFrameworkMonikerStringBuilder targetFrameworkMonikerStringBuilder, TargetFrameworkVersionStringBuilder targetFrameworkVersionStringBuilder)
        {
            _outputWriter = outputWriter;
            _targetFrameworkMonikerStringBuilder = targetFrameworkMonikerStringBuilder;
            _targetFrameworkVersionStringBuilder = targetFrameworkVersionStringBuilder;
        }

        public IProjectWriter FromProjectFormat(ProjectFormat projectFormat)
        {
            switch (projectFormat)
            {
                case ProjectFormat.Old:
                    return new OldFormatProjectWriter(_outputWriter, _targetFrameworkMonikerStringBuilder, _targetFrameworkVersionStringBuilder);
                case ProjectFormat.New:
                    return new NewFormatProjectWriter(_outputWriter, _targetFrameworkMonikerStringBuilder);
                default:
                    throw new ProjectCreationNotPossibleException("Unknown project format.");
            }
        }
    }
}

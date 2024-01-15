namespace Reqnroll.TestProjectGenerator.Dotnet
{
    public partial class AddCommandBuilder
    {
        private readonly IOutputWriter _outputWriter;

        public AddCommandBuilder(IOutputWriter outputWriter)
        {
            _outputWriter = outputWriter;
        }

        internal static AddCommandBuilder Create(IOutputWriter outputWriter) => new AddCommandBuilder(outputWriter);

        public AddPackageCommandBuilder Package() => new AddPackageCommandBuilder(_outputWriter);

        public AddReferenceCommandBuilder Reference() => new AddReferenceCommandBuilder(_outputWriter);
    }
}

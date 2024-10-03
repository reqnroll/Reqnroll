namespace Reqnroll.CucumberMessages.Configuration
{
    public interface ICucumberConfiguration
    {
        bool Enabled { get; }
        string BaseDirectory { get; }
        public string OutputDirectory { get; }
        public string OutputFileName { get; }
        public IDGenerationStyle IDGenerationStyle { get; }
    }
}
namespace Reqnroll.CucumberMessages.Configuration
{
    public interface ICucumberConfiguration
    {
        bool Enabled { get; }
        string BaseDirectory { get; }
        string OutputDirectory { get; }
        string OutputFileName { get; }
        IDGenerationStyle IDGenerationStyle { get; }
    }
}
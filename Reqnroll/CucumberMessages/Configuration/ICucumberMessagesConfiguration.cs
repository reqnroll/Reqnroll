namespace Reqnroll.CucumberMessages.Configuration
{
    public interface ICucumberMessagesConfiguration
    {
        bool Enabled { get; }
        string BaseDirectory { get; }
        string OutputDirectory { get; }
        string OutputFileName { get; }
        IDGenerationStyle IDGenerationStyle { get; }
    }
}
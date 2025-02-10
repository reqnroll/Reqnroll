namespace Reqnroll.CucumberMessages.Configuration
{
    public interface ICucumberMessagesConfiguration
    {
        bool Enabled { get; }
        string OutputFilePath { get; }
        IDGenerationStyle IDGenerationStyle { get; }
    }
}
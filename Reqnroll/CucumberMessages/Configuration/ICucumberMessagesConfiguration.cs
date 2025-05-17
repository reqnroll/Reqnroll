namespace Reqnroll.CucumberMessages.Configuration
{
    public interface ICucumberMessagesConfiguration
    {
        bool Enabled { get; }
        string GetFormatterConfigurationByName(string formatterName);
    }
}
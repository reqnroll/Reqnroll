namespace Reqnroll.Formatters.Configuration
{
    public interface IFormattersConfiguration
    {
        bool Enabled { get; }
        string GetFormatterConfigurationByName(string formatterName);
    }
}
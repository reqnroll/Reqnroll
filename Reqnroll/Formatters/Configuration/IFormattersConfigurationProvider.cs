namespace Reqnroll.Formatters.Configuration;

public interface IFormattersConfigurationProvider
{
    bool Enabled { get; }
    string GetFormatterConfigurationByName(string formatterName);
}
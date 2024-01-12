namespace Reqnroll.Configuration
{
    public interface IReqnrollConfigurationHolder
    {
        ConfigSource ConfigSource { get; }
        string Content { get; }
        bool HasConfiguration { get; }
    }
}
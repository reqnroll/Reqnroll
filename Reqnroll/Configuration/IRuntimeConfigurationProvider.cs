namespace Reqnroll.Configuration
{
    public interface IRuntimeConfigurationProvider
    {
        ReqnrollConfiguration LoadConfiguration(ReqnrollConfiguration reqnrollConfiguration);
    }
}

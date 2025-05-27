namespace Reqnroll.EnvironmentAccess
{
    public interface IEnvironmentInfoProvider
    {
        string GetOSPlatform();
        string GetBuildServerName();
        bool IsRunningInDockerContainer();
        string GetReqnrollVersion();
        string GetNetCoreVersion();
    }
}
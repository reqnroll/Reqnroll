namespace Reqnroll.Microsoft.Extensions.DependencyInjection
{
    public interface IServiceCollectionFinder
    {
        (ServicesEntryPoint, ScopeLevelType) GetServiceEntryPoint();
    }
}

using Microsoft.Extensions.DependencyInjection;

namespace Reqnroll.Microsoft.Extensions.DependencyInjection
{
    public interface IServiceCollectionFinder
    {
        ServiceProviderLifetimeType GetServiceProviderLifetime();
        (IServiceCollection, ScopeLevelType) GetServiceCollection();
    }
}

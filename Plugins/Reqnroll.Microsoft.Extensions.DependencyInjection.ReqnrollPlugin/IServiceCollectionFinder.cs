using Microsoft.Extensions.DependencyInjection;

namespace Reqnroll.Microsoft.Extensions.DependencyInjection
{
    public interface IServiceCollectionFinder
    {
        (IServiceCollection, ScopeLevelType) GetServiceCollection();
    }
}

using Reqnroll.BoDi;

namespace Reqnroll.Infrastructure
{
    public interface IContainerDependentObject
    {
        void SetObjectContainer(IObjectContainer container);
    }
}

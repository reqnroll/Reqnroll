using System;
using Castle.Windsor;

namespace Reqnroll.Windsor
{
    public interface IContainerFinder
    {
        Func<IWindsorContainer> GetCreateScenarioContainer();
    }
}

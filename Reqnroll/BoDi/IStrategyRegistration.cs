namespace Reqnroll.BoDi;

public interface IStrategyRegistration
{
    /// <summary>
    /// Changes resolving strategy to a new instance per each dependency.
    /// </summary>
    /// <returns></returns>
    IStrategyRegistration InstancePerDependency();
    /// <summary>
    /// Changes resolving strategy to a single instance per object container. This strategy is a default behaviour. 
    /// </summary>
    /// <returns></returns>
    IStrategyRegistration InstancePerContext();
}

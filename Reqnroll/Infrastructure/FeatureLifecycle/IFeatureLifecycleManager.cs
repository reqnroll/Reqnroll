using System;
using System.Threading.Tasks;

namespace Reqnroll.Infrastructure.FeatureLifecycle
{
    /// <summary>
    /// Manages feature lifecycles for scenario-level parallelism.
    /// Coordinates reference-counted BeforeFeature/AfterFeature hook execution
    /// across concurrently executing scenarios within the same feature.
    /// </summary>
    public interface IFeatureLifecycleManager
    {
        /// <summary>
        /// Acquires a reference to the feature lifecycle state.
        /// If this is the first scenario for the feature, executes BeforeFeature hooks.
        /// Subsequent scenarios wait for initialization to complete and share the feature state.
        /// </summary>
        /// <param name="featureInfo">The feature being entered.</param>
        /// <param name="onBeforeFeature">Callback to execute BeforeFeature hooks (invoked at most once). Receives the state for storing shared context.</param>
        /// <returns>The lifecycle state containing the shared feature context.</returns>
        Task<FeatureLifecycleState> AcquireFeatureAsync(FeatureInfo featureInfo, Func<FeatureLifecycleState, Task> onBeforeFeature);

        /// <summary>
        /// Releases a reference to the feature lifecycle state.
        /// If this is the last scenario for the feature, executes AfterFeature hooks.
        /// </summary>
        /// <param name="featureInfo">The feature being exited.</param>
        /// <param name="onAfterFeature">Callback to execute AfterFeature hooks (invoked at most once).</param>
        Task ReleaseFeatureAsync(FeatureInfo featureInfo, Func<Task> onAfterFeature);
    }
}

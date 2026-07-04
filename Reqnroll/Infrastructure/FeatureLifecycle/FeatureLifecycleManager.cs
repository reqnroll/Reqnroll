using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Reqnroll.Infrastructure.FeatureLifecycle
{
    /// <summary>
    /// Manages feature lifecycles across parallel scenarios using reference counting.
    /// Registered in the global container; shared across all test threads.
    /// </summary>
    public class FeatureLifecycleManager : IFeatureLifecycleManager
    {
        private readonly ConcurrentDictionary<FeatureInfo, FeatureLifecycleState> _features = new();

        public async Task<FeatureLifecycleState> AcquireFeatureAsync(FeatureInfo featureInfo, Func<FeatureLifecycleState, Task> onBeforeFeature)
        {
            var state = _features.GetOrAdd(featureInfo, _ => new FeatureLifecycleState());

            // Increment reference count BEFORE initialization to prevent premature finalization
            state.IncrementRef();

            try
            {
                await state.EnsureInitializedAsync(onBeforeFeature).ConfigureAwait(false);
            }
            catch
            {
                // Initialization failed. The ref is still held so that the caller's
                // cleanup path (OnFeatureEndAsync) can decrement it and eventually
                // trigger AfterFeature finalization when all scenarios have completed.
                throw;
            }

            return state;
        }

        public async Task ReleaseFeatureAsync(FeatureInfo featureInfo, Func<Task> onAfterFeature)
        {
            if (!_features.TryGetValue(featureInfo, out var state))
                return;

            var remaining = state.DecrementRef();

            if (remaining == 0)
            {
                // Last scenario out — execute AfterFeature exactly once
                // Remove from dictionary first to prevent new scenarios from acquiring a finalizing state
                _features.TryRemove(featureInfo, out _);

                try
                {
                    await state.EnsureFinalizedAsync(onAfterFeature).ConfigureAwait(false);
                }
                finally
                {
                    state.Dispose();
                }
            }
        }
    }
}

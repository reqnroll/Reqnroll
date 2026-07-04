using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Reqnroll.BoDi;

namespace Reqnroll.Infrastructure.FeatureLifecycle
{
    /// <summary>
    /// Thread-safe state for a single feature's lifecycle under scenario-level parallelism.
    /// Ensures BeforeFeature executes exactly once (first scenario in) and
    /// AfterFeature executes exactly once (last scenario out).
    /// </summary>
    public sealed class FeatureLifecycleState : IDisposable
    {
        private int _referenceCount;
        private int _initialized;
        private int _finalized;
        private readonly SemaphoreSlim _initLock = new(1, 1);
        private readonly SemaphoreSlim _finalizeLock = new(1, 1);
        private ExceptionDispatchInfo _beforeFeatureError;

        /// <summary>
        /// The shared feature container for this feature, created once during initialization.
        /// </summary>
        public IObjectContainer SharedFeatureContainer { get; internal set; }

        /// <summary>
        /// The shared FeatureContext, created once during initialization.
        /// </summary>
        public FeatureContext SharedFeatureContext { get; internal set; }

        /// <summary>
        /// Atomically increments the reference count (scenario entering).
        /// </summary>
        public void IncrementRef()
        {
            Interlocked.Increment(ref _referenceCount);
        }

        /// <summary>
        /// Atomically decrements the reference count (scenario leaving).
        /// Returns the new reference count value.
        /// </summary>
        public int DecrementRef()
        {
            return Interlocked.Decrement(ref _referenceCount);
        }

        /// <summary>
        /// Current reference count (for diagnostics).
        /// </summary>
        public int ReferenceCount => Volatile.Read(ref _referenceCount);

        /// <summary>
        /// Whether the feature has been initialized (BeforeFeature executed).
        /// </summary>
        public bool IsInitialized => Volatile.Read(ref _initialized) == 1;

        /// <summary>
        /// Whether the feature has been finalized (AfterFeature executed).
        /// </summary>
        public bool IsFinalized => Volatile.Read(ref _finalized) == 1;

        /// <summary>
        /// Ensures the initialization callback executes exactly once.
        /// All concurrent callers wait for completion. If initialization fails,
        /// the error is propagated to all callers.
        /// </summary>
        public async Task EnsureInitializedAsync(Func<FeatureLifecycleState, Task> onBeforeFeature)
        {
            if (Interlocked.CompareExchange(ref _initialized, 1, 0) == 0)
            {
                // We won the race — execute the initialization
                await _initLock.WaitAsync().ConfigureAwait(false);
                try
                {
                    await onBeforeFeature(this).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _beforeFeatureError = ExceptionDispatchInfo.Capture(ex);
                    throw;
                }
                finally
                {
                    _initLock.Release();
                }
            }
            else
            {
                // Another thread is initializing or has initialized — wait for completion
                await _initLock.WaitAsync().ConfigureAwait(false);
                _initLock.Release();

                // Propagate initialization failure to all scenarios
                _beforeFeatureError?.Throw();
            }
        }

        /// <summary>
        /// Ensures the finalization callback executes exactly once.
        /// Should only be called when reference count has reached zero.
        /// </summary>
        public async Task EnsureFinalizedAsync(Func<Task> onAfterFeature)
        {
            if (Interlocked.CompareExchange(ref _finalized, 1, 0) == 0)
            {
                await _finalizeLock.WaitAsync().ConfigureAwait(false);
                try
                {
                    await onAfterFeature().ConfigureAwait(false);
                }
                finally
                {
                    _finalizeLock.Release();
                }
            }
        }

        public void Dispose()
        {
            _initLock.Dispose();
            _finalizeLock.Dispose();
        }
    }
}

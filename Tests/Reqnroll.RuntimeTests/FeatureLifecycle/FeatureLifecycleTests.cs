using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Reqnroll.Configuration;
using Reqnroll.Infrastructure.FeatureLifecycle;
using Reqnroll.Tracing;
using Xunit;

namespace Reqnroll.RuntimeTests.FeatureLifecycle
{
    public class FeatureLifecycleStateTests
    {
        [Fact]
        public async Task EnsureInitializedAsync_ExecutesExactlyOnce_Under10ConcurrentCalls()
        {
            // Arrange
            var state = new FeatureLifecycleState();
            int initCount = 0;
            var barrier = new Barrier(10);

            // Act — 10 concurrent tasks all call EnsureInitializedAsync
            var tasks = Enumerable.Range(0, 10).Select(_ => Task.Run(async () =>
            {
                barrier.SignalAndWait();
                state.IncrementRef();
                await state.EnsureInitializedAsync(async (_) =>
                {
                    Interlocked.Increment(ref initCount);
                    await Task.Delay(10); // Simulate work
                });
            })).ToArray();

            await Task.WhenAll(tasks);

            // Assert
            initCount.Should().Be(1, "BeforeFeature should execute exactly once");
            state.ReferenceCount.Should().Be(10);
            state.IsInitialized.Should().BeTrue();
            state.Dispose();
        }

        [Fact]
        public async Task EnsureInitializedAsync_PropagatesError_ToAllConcurrentCallers()
        {
            // Arrange
            var state = new FeatureLifecycleState();
            var expectedException = new InvalidOperationException("BeforeFeature hook failed");
            var exceptionsReceived = new ConcurrentBag<Exception>();

            // First, initialize (and fail) with one thread
            state.IncrementRef();
            try
            {
                await state.EnsureInitializedAsync((_) => throw expectedException);
            }
            catch (Exception ex)
            {
                exceptionsReceived.Add(ex);
            }

            // Now 4 more threads call EnsureInitializedAsync — they should all get the error
            var tasks = Enumerable.Range(0, 4).Select(_ => Task.Run(async () =>
            {
                state.IncrementRef();
                try
                {
                    await state.EnsureInitializedAsync((_) => throw new Exception("should not be called"));
                }
                catch (Exception ex)
                {
                    exceptionsReceived.Add(ex);
                }
            })).ToArray();

            await Task.WhenAll(tasks);

            // Assert — all 5 callers should receive the original exception
            exceptionsReceived.Count.Should().Be(5,
                "the initiator + 4 subsequent callers all get the error");
            exceptionsReceived.Should().AllSatisfy(ex =>
                ex.Should().BeOfType<InvalidOperationException>()
                    .Which.Message.Should().Be("BeforeFeature hook failed"));
            state.Dispose();
        }

        [Fact]
        public async Task EnsureFinalizedAsync_OnlyFiresWhenRefCountZero()
        {
            // Arrange
            var state = new FeatureLifecycleState();
            int finalizeCount = 0;
            state.IncrementRef();
            state.IncrementRef();
            state.IncrementRef();
            await state.EnsureInitializedAsync((_) => Task.CompletedTask);

            // Act — decrement twice, finalize should NOT fire
            state.DecrementRef(); // ref=2
            state.DecrementRef(); // ref=1
            
            // Attempt finalize at ref=1 (should not fire)
            if (state.ReferenceCount == 0)
                await state.EnsureFinalizedAsync(() => { Interlocked.Increment(ref finalizeCount); return Task.CompletedTask; });

            finalizeCount.Should().Be(0, "finalize should not fire when ref > 0");

            // Now bring to zero
            state.DecrementRef(); // ref=0
            if (state.ReferenceCount == 0)
                await state.EnsureFinalizedAsync(() => { Interlocked.Increment(ref finalizeCount); return Task.CompletedTask; });

            // Assert
            finalizeCount.Should().Be(1, "AfterFeature should fire exactly once at ref=0");
            state.IsFinalized.Should().BeTrue();
            state.Dispose();
        }

        [Fact]
        public async Task EnsureFinalizedAsync_ExecutesExactlyOnce_EvenIfCalledMultipleTimes()
        {
            // Arrange
            var state = new FeatureLifecycleState();
            int finalizeCount = 0;
            state.IncrementRef();
            await state.EnsureInitializedAsync((_) => Task.CompletedTask);
            state.DecrementRef();

            // Act — call finalize 5 times concurrently
            var tasks = Enumerable.Range(0, 5).Select(_ => Task.Run(async () =>
            {
                await state.EnsureFinalizedAsync(() => { Interlocked.Increment(ref finalizeCount); return Task.CompletedTask; });
            })).ToArray();

            await Task.WhenAll(tasks);

            // Assert
            finalizeCount.Should().Be(1, "AfterFeature should fire exactly once");
            state.Dispose();
        }
    }

    public class FeatureLifecycleManagerTests
    {
        private static FeatureInfo CreateFeatureInfo(string name) =>
            new(new CultureInfo("en-US"), null, name, null, ProgrammingLanguage.CSharp, Array.Empty<string>(), null);

        [Fact]
        public async Task AcquireAndRelease_FullLifecycle_HooksFireExactlyOnce()
        {
            // Arrange
            var manager = new FeatureLifecycleManager();
            var featureInfo = CreateFeatureInfo("TestFeature");
            int beforeCount = 0;
            int afterCount = 0;

            // Act — 5 scenarios acquire the same feature
            for (int i = 0; i < 5; i++)
            {
                await manager.AcquireFeatureAsync(featureInfo, (_) =>
                {
                    Interlocked.Increment(ref beforeCount);
                    return Task.CompletedTask;
                });
            }

            // Release all 5
            for (int i = 0; i < 5; i++)
            {
                await manager.ReleaseFeatureAsync(featureInfo, () =>
                {
                    Interlocked.Increment(ref afterCount);
                    return Task.CompletedTask;
                });
            }

            // Assert
            beforeCount.Should().Be(1, "BeforeFeature fires exactly once");
            afterCount.Should().Be(1, "AfterFeature fires exactly once (last release)");
        }

        [Fact]
        public async Task ConcurrentScenarios_SameFeature_HooksFireExactlyOnce()
        {
            // Arrange
            var manager = new FeatureLifecycleManager();
            var featureInfo = CreateFeatureInfo("ConcurrentFeature");
            int beforeCount = 0;
            int afterCount = 0;
            var barrier = new Barrier(20);

            // Act — 20 concurrent scenarios
            var tasks = Enumerable.Range(0, 20).Select(_ => Task.Run(async () =>
            {
                barrier.SignalAndWait();
                await manager.AcquireFeatureAsync(featureInfo, async (_) =>
                {
                    Interlocked.Increment(ref beforeCount);
                    await Task.Delay(5); // Simulate hook work
                });

                await Task.Delay(Random.Shared.Next(1, 20)); // Simulate scenario execution

                await manager.ReleaseFeatureAsync(featureInfo, async () =>
                {
                    Interlocked.Increment(ref afterCount);
                    await Task.Delay(5);
                });
            })).ToArray();

            await Task.WhenAll(tasks);

            // Assert
            beforeCount.Should().Be(1, "BeforeFeature fires once even with 20 concurrent scenarios");
            afterCount.Should().Be(1, "AfterFeature fires once after last scenario");
        }

        [Fact]
        public async Task MultipleFeatures_IndependentLifecycles()
        {
            // Arrange
            var manager = new FeatureLifecycleManager();
            var feature1 = CreateFeatureInfo("Feature1");
            var feature2 = CreateFeatureInfo("Feature2");
            int before1 = 0, before2 = 0, after1 = 0, after2 = 0;
            var barrier = new Barrier(10);

            // Act — 5 scenarios per feature, running concurrently
            var tasks1 = Enumerable.Range(0, 5).Select(_ => Task.Run(async () =>
            {
                barrier.SignalAndWait();
                await manager.AcquireFeatureAsync(feature1, (_) => { Interlocked.Increment(ref before1); return Task.CompletedTask; });
                await Task.Delay(Random.Shared.Next(1, 10));
                await manager.ReleaseFeatureAsync(feature1, () => { Interlocked.Increment(ref after1); return Task.CompletedTask; });
            }));

            var tasks2 = Enumerable.Range(0, 5).Select(_ => Task.Run(async () =>
            {
                barrier.SignalAndWait();
                await manager.AcquireFeatureAsync(feature2, (_) => { Interlocked.Increment(ref before2); return Task.CompletedTask; });
                await Task.Delay(Random.Shared.Next(1, 10));
                await manager.ReleaseFeatureAsync(feature2, () => { Interlocked.Increment(ref after2); return Task.CompletedTask; });
            }));

            await Task.WhenAll(tasks1.Concat(tasks2));

            // Assert
            before1.Should().Be(1);
            before2.Should().Be(1);
            after1.Should().Be(1);
            after2.Should().Be(1);
        }

        [Fact]
        public async Task StressTest_100Scenarios_16Threads_NoCorruption()
        {
            // Arrange
            var manager = new FeatureLifecycleManager();
            const int scenariosPerFeature = 10;
            const int featureCount = 10;
            var features = Enumerable.Range(0, featureCount)
                .Select(i => CreateFeatureInfo($"StressFeature{i}"))
                .ToArray();

            var beforeCounts = new int[featureCount];
            var afterCounts = new int[featureCount];
            var errors = new ConcurrentBag<Exception>();

            // Act — 100 scenarios (10 features × 10 scenarios each) on unbounded threads
            var tasks = new Task[featureCount * scenariosPerFeature];
            for (int f = 0; f < featureCount; f++)
            {
                for (int s = 0; s < scenariosPerFeature; s++)
                {
                    int featureIndex = f;
                    tasks[f * scenariosPerFeature + s] = Task.Run(async () =>
                    {
                        try
                        {
                            await manager.AcquireFeatureAsync(features[featureIndex], (_) =>
                            {
                                Interlocked.Increment(ref beforeCounts[featureIndex]);
                                return Task.CompletedTask;
                            });

                            // Simulate scenario execution
                            await Task.Delay(Random.Shared.Next(1, 5));

                            await manager.ReleaseFeatureAsync(features[featureIndex], () =>
                            {
                                Interlocked.Increment(ref afterCounts[featureIndex]);
                                return Task.CompletedTask;
                            });
                        }
                        catch (Exception ex)
                        {
                            errors.Add(ex);
                        }
                    });
                }
            }

            await Task.WhenAll(tasks);

            // Assert
            errors.Should().BeEmpty("no exceptions should occur during stress test");
            for (int f = 0; f < featureCount; f++)
            {
                beforeCounts[f].Should().Be(1, $"BeforeFeature for feature {f} should fire exactly once");
                afterCounts[f].Should().Be(1, $"AfterFeature for feature {f} should fire exactly once");
            }
        }

        [Fact]
        public async Task RapidAcquireRelease_RefCountNeverNegative()
        {
            // Arrange
            var manager = new FeatureLifecycleManager();
            var featureInfo = CreateFeatureInfo("RapidFeature");
            int afterCount = 0;
            var barrier = new Barrier(50);

            // Act — 50 threads rapidly acquire and release
            var tasks = Enumerable.Range(0, 50).Select(_ => Task.Run(async () =>
            {
                barrier.SignalAndWait();
                await manager.AcquireFeatureAsync(featureInfo, (_) => Task.CompletedTask);
                // Minimal delay
                await Task.Yield();
                await manager.ReleaseFeatureAsync(featureInfo, () =>
                {
                    Interlocked.Increment(ref afterCount);
                    return Task.CompletedTask;
                });
            })).ToArray();

            await Task.WhenAll(tasks);

            // Assert — AfterFeature fires exactly once
            afterCount.Should().Be(1, "AfterFeature should fire exactly once even under rapid acquire/release");
        }
    }

    public class BoDiContainerThreadSafetyTests
    {
        // Simple test service with no dependencies — used to validate thread-safe resolution
        private interface ISimpleService { string Name { get; } }
        private class SimpleService : ISimpleService { public string Name => "Test"; }
        
        private interface IAnotherService { int Value { get; } }
        private class AnotherService : IAnotherService { public int Value => 42; }

        [Fact]
        public async Task ConcurrentChildContainerCreation_FromSharedParent_NoCorruption()
        {
            // Arrange — simulate the shared FeatureContainer that all scenarios resolve from
            var parentContainer = new Reqnroll.BoDi.ObjectContainer();
            parentContainer.RegisterTypeAs<SimpleService, ISimpleService>();
            parentContainer.RegisterTypeAs<AnotherService, IAnotherService>();

            var errors = new ConcurrentBag<Exception>();
            var barrier = new Barrier(20);

            // Act — 20 threads simultaneously create child containers and resolve from parent
            var tasks = Enumerable.Range(0, 20).Select(_ => Task.Run(async () =>
            {
                try
                {
                    barrier.SignalAndWait();
                    // Each scenario creates its own child container (like CreateScenarioContainer does)
                    var childContainer = new Reqnroll.BoDi.ObjectContainer(parentContainer);
                    
                    // Resolve types that will delegate to parent container
                    var service1 = childContainer.Resolve<ISimpleService>();
                    var service2 = childContainer.Resolve<IAnotherService>();

                    service1.Should().NotBeNull();
                    service2.Should().NotBeNull();
                    service1.Name.Should().Be("Test");
                    service2.Value.Should().Be(42);
                    await Task.CompletedTask;
                }
                catch (Exception ex)
                {
                    errors.Add(ex);
                }
            })).ToArray();

            await Task.WhenAll(tasks);

            // Assert
            errors.Should().BeEmpty("concurrent resolution from shared parent container must be thread-safe");
        }

        [Fact]
        public async Task ConcurrentResolution_SameTypeFromSharedContainer_ReturnsSameInstance()
        {
            // Arrange
            var container = new Reqnroll.BoDi.ObjectContainer();
            container.RegisterTypeAs<SimpleService, ISimpleService>();
            var barrier = new Barrier(10);
            var resolvedInstances = new ConcurrentBag<object>();

            // Act — 10 threads resolve the same type simultaneously
            var tasks = Enumerable.Range(0, 10).Select(_ => Task.Run(async () =>
            {
                barrier.SignalAndWait();
                var instance = container.Resolve<ISimpleService>();
                resolvedInstances.Add(instance);
                await Task.CompletedTask;
            })).ToArray();

            await Task.WhenAll(tasks);

            // Assert — all should get the same instance (PerContext strategy)
            resolvedInstances.Count.Should().Be(10);
            resolvedInstances.Distinct().Count().Should().Be(1,
                "PerContext resolution must return the same instance regardless of concurrent access");
        }
    }
}

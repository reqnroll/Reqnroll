using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Reqnroll.BoDi;
using Xunit;

namespace Reqnroll.RuntimeTests.BoDi
{
    public class ConcurrencyTests
    {
        [Theory]
        [InlineData(RegistrationStrategy.PerContext, null)]
        [InlineData(RegistrationStrategy.PerContext, "Name")]
        [InlineData(RegistrationStrategy.PerDependency, null)]
        [InlineData(RegistrationStrategy.PerDependency, "Name")]
        public void ShouldBeAbleToResolveFromMultipleThreadsWhenRegisteredAsType(
            RegistrationStrategy registrationStrategy,
            string name)
        {
            try
            {
                IObjectContainer container = new ObjectContainer();
                var registration = container.RegisterTypeAs<BlockingObject, BlockingObject>(name);
                ApplyRegistrationStrategy(registration, registrationStrategy);

                void Resolve(object _)
                {
                    Action act = () => container.Resolve<BlockingObject>(name);
                    act.Should().NotThrow();
                }

                var thread1 = new Thread(Resolve);
                var thread2 = new Thread(Resolve);
                thread1.Start();
                thread2.Start();

                // try to wait until both object constructions are in progress (may not happen if no threading issue)
                BlockingObject.ObjectsCreated.WaitOne(ForHalfASecond);
                BlockingObject.ObjectsCreated.WaitOne(ForHalfASecond);

                // allow constructors to finish
                BlockingObject.ConstructorBlockers[0].Set();
                BlockingObject.ConstructorBlockers[1].Set();

                // complete the threads
                if (!thread1.Join(ForHalfASecond))
                {
                    Assert.Fail("Unable to complete resolution");
                }
                if (!thread2.Join(ForHalfASecond))
                {
                    Assert.Fail("Unable to complete resolution");
                }
            }
            finally
            {
                BlockingObject.ResetBlockingEvents();
            }
        }

        [Fact]
        public void ShouldBeAbleToResolveFromMultipleThreadsWhenImplicitRegisteredAsType()
        {
            try
            {
                IObjectContainer container = new ObjectContainer();

                void Resolve(object _)
                {
                    Action act = () => container.Resolve<BlockingObject>();
                    act.Should().NotThrow();
                }

                var thread1 = new Thread(Resolve);
                var thread2 = new Thread(Resolve);
                thread1.Start();
                thread2.Start();

                // try to wait until both object constructions are in progress (may not happen if no threading issue)
                BlockingObject.ObjectsCreated.WaitOne(ForHalfASecond);
                BlockingObject.ObjectsCreated.WaitOne(ForHalfASecond);

                // allow constructors to finish
                BlockingObject.ConstructorBlockers[0].Set();
                BlockingObject.ConstructorBlockers[1].Set();

                // complete the threads
                if (!thread1.Join(ForHalfASecond))
                {
                    Assert.Fail("Unable to complete resolution");
                }
                if (!thread2.Join(ForHalfASecond))
                {
                    Assert.Fail("Unable to complete resolution");
                }
            }
            finally
            {
                BlockingObject.ResetBlockingEvents();
            }
        }

        [Theory]
        [InlineData(RegistrationStrategy.PerContext, null)]
        [InlineData(RegistrationStrategy.PerContext, "Name")]
        [InlineData(RegistrationStrategy.PerDependency, null)]
        [InlineData(RegistrationStrategy.PerDependency, "Name")]
        public void ShouldBeAbleToResolveFromMultipleThreadsWhenRegisteredAsFactory(
            RegistrationStrategy registrationStrategy,
            string name)
        {
            try
            {
                IObjectContainer container = new ObjectContainer();
                var registration = container.RegisterFactoryAs(_ => new BlockingObject(), name);
                ApplyRegistrationStrategy(registration, registrationStrategy);

                void Resolve(object _)
                {
                    Action act = () => container.Resolve<BlockingObject>(name);
                    act.Should().NotThrow();
                }

                var thread1 = new Thread(Resolve);
                var thread2 = new Thread(Resolve);
                thread1.Start();
                thread2.Start();

                // try to wait until both object constructions are in progress (may not happen if no threading issue)
                BlockingObject.ObjectsCreated.WaitOne(ForHalfASecond);
                BlockingObject.ObjectsCreated.WaitOne(ForHalfASecond);

                // allow constructors to finish
                BlockingObject.ConstructorBlockers[0].Set();
                BlockingObject.ConstructorBlockers[1].Set();

                // complete the threads
                if (!thread1.Join(ForHalfASecond))
                {
                    Assert.Fail("Unable to complete resolution");
                }
                if (!thread2.Join(ForHalfASecond))
                {
                    Assert.Fail("Unable to complete resolution");
                }
            }
            finally
            {
                BlockingObject.ResetBlockingEvents();
            }
        }

        [Theory]
        [InlineData(RegistrationStrategy.PerContext)]
        [InlineData(RegistrationStrategy.PerDependency)]
        public void ShouldBeAbleToResolveAllFromMultipleThreadsWhenRegisteredAsType(
            RegistrationStrategy registrationStrategy)
        {
            try
            {
                IObjectContainer container = new ObjectContainer();
                var registration = container.RegisterTypeAs<BlockingObject, BlockingObject>();
                ApplyRegistrationStrategy(registration, registrationStrategy);

                void Resolve(object _)
                {
                    Action act = () => container.ResolveAll<BlockingObject>().ToList();
                    act.Should().NotThrow();
                }

                var thread1 = new Thread(Resolve);
                var thread2 = new Thread(Resolve);
                thread1.Start();
                thread2.Start();

                // try to wait until both object constructions are in progress (may not happen if no threading issue)
                BlockingObject.ObjectsCreated.WaitOne(ForHalfASecond);
                BlockingObject.ObjectsCreated.WaitOne(ForHalfASecond);

                // allow constructors to finish
                BlockingObject.ConstructorBlockers[0].Set();
                BlockingObject.ConstructorBlockers[1].Set();

                // complete the threads
                if (!thread1.Join(ForHalfASecond))
                {
                    Assert.Fail("Unable to complete resolution");
                }
                if (!thread2.Join(ForHalfASecond))
                {
                    Assert.Fail("Unable to complete resolution");
                }
            }
            finally
            {
                BlockingObject.ResetBlockingEvents();
            }
        }

        [Theory]
        [InlineData(RegistrationStrategy.PerContext)]
        [InlineData(RegistrationStrategy.PerDependency)]
        public void ShouldBeAbleToResolveAllFromMultipleThreadsWhenRegisteredAsFactory(
            RegistrationStrategy registrationStrategy)
        {
            try
            {
                IObjectContainer container = new ObjectContainer();
                var registration = container.RegisterFactoryAs(_ => new BlockingObject());
                ApplyRegistrationStrategy(registration, registrationStrategy);

                void Resolve(object _)
                {
                    Action act = () => container.ResolveAll<BlockingObject>().ToList();
                    act.Should().NotThrow();
                }

                var thread1 = new Thread(Resolve);
                var thread2 = new Thread(Resolve);
                thread1.Start();
                thread2.Start();

                // try to wait until both object constructions are in progress (may not happen if no threading issue)
                BlockingObject.ObjectsCreated.WaitOne(ForHalfASecond);
                BlockingObject.ObjectsCreated.WaitOne(ForHalfASecond);

                // allow constructors to finish
                BlockingObject.ConstructorBlockers[0].Set();
                BlockingObject.ConstructorBlockers[1].Set();

                // complete the threads
                if (!thread1.Join(ForHalfASecond))
                {
                    Assert.Fail("Unable to complete resolution");
                }
                if (!thread2.Join(ForHalfASecond))
                {
                    Assert.Fail("Unable to complete resolution");
                }
            }
            finally
            {
                BlockingObject.ResetBlockingEvents();
            }
        }

        [Theory]
        [InlineData(RegistrationStrategy.PerContext, null)]
        [InlineData(RegistrationStrategy.PerContext, "Name")]
        [InlineData(RegistrationStrategy.PerDependency, null)]
        [InlineData(RegistrationStrategy.PerDependency, "Name")]
        public void ShouldBeAbleToResolveFromMultipleThreadsFromContainerHierarchyWhenRegisteredAsType(
            RegistrationStrategy registrationStrategy,
            string name)
        {
            try
            {
                IObjectContainer baseContainer = new ObjectContainer();
                IObjectContainer childContainer = new ObjectContainer(baseContainer);
                var registration = baseContainer.RegisterTypeAs<BlockingObject, BlockingObject>(name);

                ApplyRegistrationStrategy(registration, registrationStrategy);

                var thread1 = new Thread(_ =>
                {
                    Action act = () => baseContainer.Resolve<BlockingObject>(name);
                    act.Should().NotThrow();
                });
                var thread2 = new Thread(_ =>
                {
                    Action act = () => childContainer.Resolve<BlockingObject>(name);
                    act.Should().NotThrow();
                });
                thread1.Start();
                thread2.Start();

                // try to wait until both object constructions are in progress (may not happen if no threading issue)
                BlockingObject.ObjectsCreated.WaitOne(ForHalfASecond);
                BlockingObject.ObjectsCreated.WaitOne(ForHalfASecond);

                // allow constructors to finish
                BlockingObject.ConstructorBlockers[0].Set();
                BlockingObject.ConstructorBlockers[1].Set();

                // complete the threads
                if (!thread1.Join(ForHalfASecond))
                {
                    Assert.Fail("Unable to complete resolution");
                }
                if (!thread2.Join(ForHalfASecond))
                {
                    Assert.Fail("Unable to complete resolution");
                }
            }
            finally
            {
                BlockingObject.ResetBlockingEvents();
            }
        }

        [Theory]
        [InlineData(RegistrationStrategy.PerContext, null)]
        [InlineData(RegistrationStrategy.PerContext, "Name")]
        [InlineData(RegistrationStrategy.PerDependency, null)]
        [InlineData(RegistrationStrategy.PerDependency, "Name")]
        public void ShouldBeAbleToResolveFromMultipleThreadsFromContainerHierarchyWhenRegisteredAsFactory(
            RegistrationStrategy registrationStrategy,
            string name)
        {
            try
            {
                IObjectContainer baseContainer = new ObjectContainer();
                IObjectContainer childContainer = new ObjectContainer(baseContainer);
                var registration = baseContainer.RegisterFactoryAs(_ => new BlockingObject());

                ApplyRegistrationStrategy(registration, registrationStrategy);

                var thread1 = new Thread(_ =>
                {
                    Action act = () => baseContainer.Resolve<BlockingObject>(name);
                    act.Should().NotThrow();
                });
                var thread2 = new Thread(_ =>
                {
                    Action act = () => childContainer.Resolve<BlockingObject>(name);
                    act.Should().NotThrow();
                });
                thread1.Start();
                thread2.Start();

                // try to wait until both object constructions are in progress (may not happen if no threading issue)
                BlockingObject.ObjectsCreated.WaitOne(ForHalfASecond);
                BlockingObject.ObjectsCreated.WaitOne(ForHalfASecond);

                // allow constructors to finish
                BlockingObject.ConstructorBlockers[0].Set();
                BlockingObject.ConstructorBlockers[1].Set();

                // complete the threads
                if (!thread1.Join(ForHalfASecond))
                {
                    Assert.Fail("Unable to complete resolution");
                }
                if (!thread2.Join(ForHalfASecond))
                {
                    Assert.Fail("Unable to complete resolution");
                }
            }
            finally
            {
                BlockingObject.ResetBlockingEvents();
            }
        }

        [Theory]
        [InlineData(RegistrationStrategy.PerContext, null)]
        [InlineData(RegistrationStrategy.PerContext, "Name")]
        [InlineData(RegistrationStrategy.PerDependency, null)]
        [InlineData(RegistrationStrategy.PerDependency, "Name")]
        public void ShouldBeAbleToResolveAllFromMultipleThreadsFromContainerHierarchyWhenRegisteredAsType(
            RegistrationStrategy registrationStrategy,
            string name)
        {
            try
            {
                IObjectContainer baseContainer = new ObjectContainer();
                IObjectContainer childContainer = new ObjectContainer(baseContainer);
                var registration = baseContainer.RegisterTypeAs<BlockingObject, BlockingObject>(name);

                ApplyRegistrationStrategy(registration, registrationStrategy);

                var thread1 = new Thread(_ =>
                {
                    Action act = () => baseContainer.ResolveAll<BlockingObject>().ToList();
                    act.Should().NotThrow();
                });
                var thread2 = new Thread(_ =>
                {
                    Action act = () => childContainer.ResolveAll<BlockingObject>().ToList();
                    act.Should().NotThrow();
                });
                thread1.Start();
                thread2.Start();

                // try to wait until both object constructions are in progress (may not happen if no threading issue)
                BlockingObject.ObjectsCreated.WaitOne(ForHalfASecond);
                BlockingObject.ObjectsCreated.WaitOne(ForHalfASecond);

                // allow constructors to finish
                BlockingObject.ConstructorBlockers[0].Set();
                BlockingObject.ConstructorBlockers[1].Set();

                // complete the threads
                if (!thread1.Join(ForHalfASecond))
                {
                    Assert.Fail("Unable to complete resolution");
                }
                if (!thread2.Join(ForHalfASecond))
                {
                    Assert.Fail("Unable to complete resolution");
                }
            }
            finally
            {
                BlockingObject.ResetBlockingEvents();
            }
        }

        [Theory]
        [InlineData(RegistrationStrategy.PerContext)]
        [InlineData(RegistrationStrategy.PerDependency)]
        public void ShouldBeAbleToResolveAllFromMultipleThreadsFromContainerHierarchyWhenRegisteredAsFactory(
            RegistrationStrategy registrationStrategy)
        {
            try
            {
                IObjectContainer baseContainer = new ObjectContainer();
                IObjectContainer childContainer = new ObjectContainer(baseContainer);
                var registration = baseContainer.RegisterFactoryAs(_ => new BlockingObject());

                ApplyRegistrationStrategy(registration, registrationStrategy);

                var thread1 = new Thread(_ =>
                {
                    Action act = () => baseContainer.ResolveAll<BlockingObject>().ToList();
                    act.Should().NotThrow();
                });
                var thread2 = new Thread(_ =>
                {
                    Action act = () => childContainer.ResolveAll<BlockingObject>().ToList();
                    act.Should().NotThrow();
                });
                thread1.Start();
                thread2.Start();

                // try to wait until both object constructions are in progress (may not happen if no threading issue)
                BlockingObject.ObjectsCreated.WaitOne(ForHalfASecond);
                BlockingObject.ObjectsCreated.WaitOne(ForHalfASecond);

                // allow constructors to finish
                BlockingObject.ConstructorBlockers[0].Set();
                BlockingObject.ConstructorBlockers[1].Set();

                // complete the threads
                if (!thread1.Join(ForHalfASecond))
                {
                    Assert.Fail("Unable to complete resolution");
                }
                if (!thread2.Join(ForHalfASecond))
                {
                    Assert.Fail("Unable to complete resolution");
                }
            }
            finally
            {
                BlockingObject.ResetBlockingEvents();
            }
        }

        [Theory]
        [InlineData(RegistrationStrategy.PerContext)]
        [InlineData(RegistrationStrategy.PerDependency)]
        public void ShouldBeAbleToDetectCircularDependencyWhenResolvingParallel(
            RegistrationStrategy registrationStrategy)
        {
            try
            {
                IObjectContainer container = new ObjectContainer();
                var registration1 = container.RegisterTypeAs<BlockingCircular1, BlockingCircular1>();
                var registration2 = container.RegisterTypeAs<BlockingCircular2, BlockingCircular2>();

                ApplyRegistrationStrategy(registration1, registrationStrategy);
                ApplyRegistrationStrategy(registration2, registrationStrategy);

                var thread1 = new Thread(_ =>
                {
                    Action act = () => container.Resolve<BlockingCircular1>();
                    act.Should().ThrowExactly<ObjectContainerException>("Concurrent object resolution timeout (potential circular dependency).");
                });
                var thread2 = new Thread(_ =>
                {
                    Action act = () => container.Resolve<BlockingCircular2>();
                    act.Should().ThrowExactly<ObjectContainerException>("Concurrent object resolution timeout (potential circular dependency).");
                });
                thread1.Start();
                thread2.Start();

                // try to wait until both object constructions are in progress (may not happen if no threading issue)
                BlockingObject.ObjectsCreated.WaitOne(ForHalfASecond);
                BlockingObject.ObjectsCreated.WaitOne(ForHalfASecond);

                // allow constructors to finish
                BlockingObject.ConstructorBlockers[0].Set();
                BlockingObject.ConstructorBlockers[1].Set();

                // complete the threads
                if (!thread1.Join(ForHalfASecond))
                {
                    Assert.Fail("Unable to complete resolution");
                }
                if (!thread2.Join(ForHalfASecond))
                {
                    Assert.Fail("Unable to complete resolution");
                }
            }
            finally
            {
                BlockingObject.ResetBlockingEvents();
            }
        }

        [Fact]
        public void ShouldThrowConcurrentObjectResolutionTimeoutErrorIfResolutionBlocksLongerThanObjectResolutionTimeOut()
        {
            try
            {
                var container = new ObjectContainer();
                container.ConcurrentObjectResolutionTimeout = TimeSpan.FromMilliseconds(10);
                container.RegisterTypeAs<BlockingObject, BlockingObject>();

                var thread1 = new Thread(_ =>
                {
                    Action act = () => container.Resolve<BlockingObject>();
                    act.Should().NotThrow();
                });
                var thread2 = new Thread(_ =>
                {
                    Action act = () => container.Resolve<BlockingObject>();
                    act.Should().ThrowExactly<ObjectContainerException>("Concurrent object resolution timeout (potential circular dependency).")
                       .And.Message.Should().Contain(nameof(ObjectContainer.DefaultConcurrentObjectResolutionTimeout))
                       .And.Contain(nameof(ObjectContainer.ConcurrentObjectResolutionTimeout));
                });

                // start first thread and wait until ctor already in progress
                thread1.Start();
                BlockingObject.ObjectsCreated.WaitOne(ForHalfASecond);

                // start second thread now, this should be blocked
                thread2.Start();
                // try to wait until second ctor is in progress (may not happen if no threading issue)
                BlockingObject.ObjectsCreated.WaitOne(ForHalfASecond);

                // allow constructors to finish
                BlockingObject.ConstructorBlockers[0].Set();
                BlockingObject.ConstructorBlockers[1].Set();

                // complete the threads
                if (!thread1.Join(ForHalfASecond))
                {
                    Assert.Fail("Unable to complete resolution");
                }
                if (!thread2.Join(ForHalfASecond))
                {
                    Assert.Fail("Unable to complete resolution");
                }
            }
            finally
            {
                BlockingObject.ResetBlockingEvents();
            }
        }

        private void ApplyRegistrationStrategy(IStrategyRegistration registration, RegistrationStrategy registrationStrategy)
        {
            switch (registrationStrategy)
            {
                case RegistrationStrategy.PerContext:
                    registration.InstancePerContext();
                    break;
                case RegistrationStrategy.PerDependency:
                    registration.InstancePerDependency();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(registrationStrategy), registrationStrategy, null);
            }
        }

        public enum RegistrationStrategy
        {
            PerContext,
            PerDependency
        }

        /// <summary>
        /// This object blocks on construction, to ensure control on multi-threading use cases
        /// </summary>
        private class BlockingObject
        {
            private const int MaxBlockingObjects = 2;

            public static List<EventWaitHandle> ConstructorBlockers;
            private static int _currentConstructorBlockerIndex;
            public static Semaphore ObjectsCreated;

            static BlockingObject()
            {
                ResetBlockingEvents();
            }

            public static void ResetBlockingEvents()
            {
                if (ConstructorBlockers != null)
                {
                    foreach (var evt in ConstructorBlockers)
                    {
                        evt.Set();
                    }
                }
                ConstructorBlockers = Enumerable.Repeat(new ManualResetEvent(false), MaxBlockingObjects).ToList<EventWaitHandle>();
                _currentConstructorBlockerIndex = -1;
                ObjectsCreated = new Semaphore(0, MaxBlockingObjects);
            }

            public BlockingObject()
            {
                var index = Interlocked.Increment(ref _currentConstructorBlockerIndex);
                var evt = ConstructorBlockers[index];
                ObjectsCreated.Release(1);
                evt.WaitOne();
            }
        }

        private class BlockingCircular1 : BlockingObject
        {
            public BlockingCircular1(BlockingCircular2 dep)
            {
            }
        }

        private class BlockingCircular2 : BlockingObject
        {
            public BlockingCircular2(BlockingCircular1 dep)
            {
            }
        }

        private static readonly TimeSpan ForHalfASecond = TimeSpan.FromMilliseconds(500);
    }
}

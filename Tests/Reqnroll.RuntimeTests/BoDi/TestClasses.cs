using System;

namespace Reqnroll.RuntimeTests.BoDi
{
    public interface IInterface1
    {

    }

    public interface IInterface2
    {

    }

    public interface IInterface3
    {

    }

    public interface IInterface4
    {

    }

    public interface IGenericInterface<T>
    {
    }

    /// <summary>
    /// A very simple class without dependencies and ctor
    /// </summary>
    public class VerySimpleClass : IInterface1
    {
    }

    /// <summary>
    /// A simple generic class without dependencies and ctor
    /// </summary>
    public class SimpleGenericClass<T> : IGenericInterface<T>
    {
    }

    public class SimpleClassExtendingGenericInterface : IGenericInterface<VerySimpleClass>
    {
    }

    public struct MyStructWithDependencies
    {
        public MyStructWithDependencies(VerySimpleClass dep)
        {

        }
    }

    public struct MyStructWithoutCtor
    {

    }

    /// <summary>
    /// A very simple class with a dependency on the registered name
    /// </summary>
    public class SimpleClassWithRegisteredNameDependency : IInterface1
    {
        public string RegisteredName { get; private set; }

        public SimpleClassWithRegisteredNameDependency(string registeredName)
        {
            RegisteredName = registeredName;
        }
    }

    /// <summary>
    /// Another very simple class without dependencies and ctor
    /// </summary>
    public class AnotherVerySimpleClass : IInterface2
    {
    }

    /// <summary>
    /// A simple class without dependencies and but a default constructor
    /// </summary>
    public class SimpleClassWithDefaultCtor : IInterface1
    {
        public string Status = "";

        public SimpleClassWithDefaultCtor()
        {
            Status = "Initialized";
        }

        public override string ToString()
        {
            return string.Format("SimpleClassWithDefaultCtor: {0}", Status);
        }
    }

    public class SimpleClassWithFailingToString : IInterface1
    {
        public override string ToString()
        {
            throw new Exception("simulated error");
        }
    }

    /// <summary>
    /// A simple class without dependencies and but an internal default constructor
    /// </summary>
    public class SimpleClassWithInternalCtor : IInterface1
    {
        public readonly string Status = "";

        internal SimpleClassWithInternalCtor()
        {
            Status = "Initialized";
        }
    }

    /// <summary>
    /// A clsss with a dependency that does not have further dependencies
    /// </summary>
    public class ClassWithSimpleDependency : IInterface3
    {
        public readonly IInterface1 Dependency;

        public ClassWithSimpleDependency(IInterface1 dependency)
        {
            Dependency = dependency;
        }
    }

    /// <summary>
    /// A clsss with a dependencies that does not have further dependencies
    /// </summary>
    public class ClassWithSimpleDependencies : IInterface3
    {
        public readonly IInterface1 Dependency1;
        public readonly IInterface2 Dependency2;

        public ClassWithSimpleDependencies(IInterface1 dependency1, IInterface2 dependency2)
        {
            Dependency1 = dependency1;
            Dependency2 = dependency2;
        }
    }

    /// <summary>
    /// A clsss with a dependency that has further dependencies
    /// </summary>
    public class ClassWithDeeperDependency : IInterface4
    {
        public readonly IInterface3 Dependency;

        public ClassWithDeeperDependency(IInterface3 dependency)
        {
            Dependency = dependency;
        }
    }

    /// <summary>
    /// A clsss with a dependency that has further dependencies and another dependency that was also required by the first
    /// </summary>
    public class ClassWithDeeperRedundantDependencies : IInterface4
    {
        public readonly IInterface3 Dependency1;
        public readonly IInterface1 Dependency2;

        public ClassWithDeeperRedundantDependencies(IInterface3 dependency1, IInterface1 dependency2)
        {
            Dependency2 = dependency2;
            Dependency1 = dependency1;
        }
    }

    /// <summary>
    /// A class that implements two interface.
    /// </summary>
    public class ClassWithTwoInterface : IInterface1, IInterface2
    {

    }

    public interface IDisposableClass
    {
        bool WasDisposed { get; }
    }

    public class DisposableClass1 : IDisposableClass, IDisposable
    {
        public bool WasDisposed { get; private set; }

        public void Dispose()
        {
            WasDisposed = true;
        }
    }

    public enum MyEnumKey
    {
        One,
        Two
    }

    public class ClassWithCircularDependency1
    {
        public ClassWithCircularDependency1(ClassWithCircularDependency2 dep2)
        {

        }
    }

    public class ClassWithCircularDependency2
    {
        public ClassWithCircularDependency2(ClassWithCircularDependency1 dep1)
        {

        }
    }

    public class ClassWithCircularDependencyThroughInterfaces1 : IInterface1
    {
        public ClassWithCircularDependencyThroughInterfaces1(IInterface2 dep2)
        {

        }
    }

    public class ClassWithCircularDependencyThroughInterfaces2 : IInterface2
    {
        public ClassWithCircularDependencyThroughInterfaces2(IInterface1 dep1)
        {

        }
    }

    public class ClassWithTwoConstructorSameParamCount
    {
        public ClassWithTwoConstructorSameParamCount(string a, int b)
        {

        }

        public ClassWithTwoConstructorSameParamCount(int a, string b)
        {

        }
    }

    public class Interface1DependingOnAnotherImplementation : IInterface1
    {
        public VerySimpleClass Other { get; }

        public Interface1DependingOnAnotherImplementation(VerySimpleClass other)
        // VerySimpleClass also implements IInterface1
        {
            Other = other;
        }
    }
}

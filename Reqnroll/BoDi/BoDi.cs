﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;

namespace Reqnroll.BoDi
{
    [Serializable]
    public class ObjectContainerException : Exception
    {
        public ObjectContainerException(string message, Type[] resolutionPath) : base(GetMessage(message, resolutionPath))
        {
        }

        protected ObjectContainerException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        static private string GetMessage(string message, Type[] resolutionPath)
        {
            if (resolutionPath == null || resolutionPath.Length == 0)
                return message;

            return string.Format("{0} (resolution path: {1})", message, string.Join("->", resolutionPath.Select(t => t.FullName).ToArray()));
        }
    }

    public interface IObjectContainer : IDisposable
    {
        /// <summary>
        /// Fired when a new object is created directly by the container. It is not invoked for resolving instance and factory registrations.
        /// </summary>
        event Action<object> ObjectCreated;

        /// <summary>
        /// Registers a type as the desired implementation type of an interface.
        /// </summary>
        /// <typeparam name="TType">Implementation type</typeparam>
        /// <typeparam name="TInterface">Interface will be resolved</typeparam>
        /// <returns>An object which allows to change resolving strategy.</returns>
        /// <param name="name">A name to register named instance, otherwise null.</param>
        /// <exception cref="ObjectContainerException">If there was already a resolve for the <typeparamref name="TInterface"/>.</exception>
        /// <remarks>
        ///     <para>Previous registrations can be overridden before the first resolution for the <typeparamref name="TInterface"/>.</para>
        /// </remarks>
        IStrategyRegistration RegisterTypeAs<TType, TInterface>(string name = null) where TType : class, TInterface;

        /// <summary>
        /// Registers an instance 
        /// </summary>
        /// <typeparam name="TInterface">Interface will be resolved</typeparam>
        /// <param name="instance">The instance implements the interface.</param>
        /// <param name="name">A name to register named instance, otherwise null.</param>
        /// <param name="dispose">Whether the instance should be disposed on container dispose, otherwise <c>false</c>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="instance"/> is null.</exception>
        /// <exception cref="ObjectContainerException">If there was already a resolve for the <typeparamref name="TInterface"/>.</exception>
        /// <remarks>
        ///     <para>Previous registrations can be overridden before the first resolution for the <typeparamref name="TInterface"/>.</para>
        ///     <para>The instance will be registered in the object pool, so if a <see cref="Resolve{T}()"/> (for another interface) would require an instance of the dynamic type of the <paramref name="instance"/>, the <paramref name="instance"/> will be returned.</para>
        /// </remarks>
        void RegisterInstanceAs<TInterface>(TInterface instance, string name = null, bool dispose = false) where TInterface : class;

        /// <summary>
        /// Registers an instance 
        /// </summary>
        /// <param name="instance">The instance implements the interface.</param>
        /// <param name="interfaceType">Interface will be resolved</param>
        /// <param name="name">A name to register named instance, otherwise null.</param>
        /// <param name="dispose">Whether the instance should be disposed on container dispose, otherwise <c>false</c>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="instance"/> is null.</exception>
        /// <exception cref="ObjectContainerException">If there was already a resolve for the <paramref name="interfaceType"/>.</exception>
        /// <remarks>
        ///     <para>Previous registrations can be overridden before the first resolution for the <paramref name="interfaceType"/>.</para>
        ///     <para>The instance will be registered in the object pool, so if a <see cref="Resolve{T}()"/> (for another interface) would require an instance of the dynamic type of the <paramref name="instance"/>, the <paramref name="instance"/> will be returned.</para>
        /// </remarks>
        void RegisterInstanceAs(object instance, Type interfaceType, string name = null, bool dispose = false);

        /// <summary>
        /// Registers an instance produced by <paramref name="factoryDelegate"/>. The delegate will be called only once and the instance it returned will be returned in each resolution.
        /// </summary>
        /// <typeparam name="TInterface">Interface to register as.</typeparam>
        /// <param name="factoryDelegate">The function to run to obtain the instance.</param>
        /// <param name="name">A name to resolve named instance, otherwise null.</param>
        IStrategyRegistration RegisterFactoryAs<TInterface>(Func<IObjectContainer, TInterface> factoryDelegate, string name = null);

        /// <summary>
        /// Resolves an implementation object for an interface or type.
        /// </summary>
        /// <typeparam name="T">The interface or type.</typeparam>
        /// <returns>An object implementing <typeparamref name="T"/>.</returns>
        /// <remarks>
        ///     <para>The container pools the objects, so if the interface is resolved twice or the same type is registered for multiple interfaces, a single instance is created and returned.</para>
        /// </remarks>
        T Resolve<T>();

        /// <summary>
        /// Resolves an implementation object for an interface or type.
        /// </summary>
        /// <param name="name">A name to resolve named instance, otherwise null.</param>
        /// <typeparam name="T">The interface or type.</typeparam>
        /// <returns>An object implementing <typeparamref name="T"/>.</returns>
        /// <remarks>
        ///     <para>The container pools the objects, so if the interface is resolved twice or the same type is registered for multiple interfaces, a single instance is created and returned.</para>
        /// </remarks>
        T Resolve<T>(string name);

        /// <summary>
        /// Resolves an implementation object for an interface or type.
        /// </summary>
        /// <param name="typeToResolve">The interface or type.</param>
        /// <param name="name">A name to resolve named instance, otherwise null.</param>
        /// <returns>An object implementing <paramref name="typeToResolve"/>.</returns>
        /// <remarks>
        ///     <para>The container pools the objects, so if the interface is resolved twice or the same type is registered for multiple interfaces, a single instance is created and returned.</para>
        /// </remarks>
        object Resolve(Type typeToResolve, string name = null);

        /// <summary>
        /// Resolves all implementations of an interface or type.
        /// </summary>
        /// <typeparam name="T">The interface or type.</typeparam>
        /// <returns>An object implementing <typeparamref name="T"/>.</returns>
        IEnumerable<T> ResolveAll<T>() where T : class;

        /// <summary>
        /// Determines whether the interface or type is registered.
        /// </summary>
        /// <typeparam name="T">The interface or type.</typeparam>
        /// <returns><c>true</c> if the interface or type is registered; otherwise <c>false</c>.</returns>
        bool IsRegistered<T>();

        /// <summary>
        /// Determines whether the interface or type is registered with the specified name.
        /// </summary>
        /// <typeparam name="T">The interface or type.</typeparam>
        /// <param name="name">The name.</param>
        /// <returns><c>true</c> if the interface or type is registered; otherwise <c>false</c>.</returns>
        bool IsRegistered<T>(string name);
    }
    public interface IContainedInstance
    {
        IObjectContainer Container { get; }
    }
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

    public class ObjectContainer : IObjectContainer
    {
        private const string REGISTERED_NAME_PARAMETER_NAME = "registeredName";

        /// <summary>
        /// A very simple immutable linked list of <see cref="Type"/>.
        /// </summary>
        private class ResolutionList
        {
            private readonly RegistrationKey _currentRegistrationKey;
            private readonly Type _currentResolvedType;
            private readonly ResolutionList _nextNode;
            private bool IsLast => _nextNode == null;

            public ResolutionList()
            {
                Debug.Assert(IsLast);
            }

            private ResolutionList(RegistrationKey currentRegistrationKey, Type currentResolvedType, ResolutionList nextNode)
            {
                if (nextNode == null) throw new ArgumentNullException("nextNode");

                _currentRegistrationKey = currentRegistrationKey;
                _currentResolvedType = currentResolvedType;
                _nextNode = nextNode;
            }

            public ResolutionList AddToEnd(RegistrationKey registrationKey, Type resolvedType)
            {
                return new ResolutionList(registrationKey, resolvedType, this);
            }

            // ReSharper disable once UnusedMember.Local
            public bool Contains(Type resolvedType)
            {
                if (resolvedType == null) throw new ArgumentNullException("resolvedType");
                return GetReverseEnumerable().Any(i => i.Value == resolvedType);
            }

            public bool Contains(RegistrationKey registrationKey)
            {
                return GetReverseEnumerable().Any(i => i.Key.Equals(registrationKey));
            }

            private IEnumerable<KeyValuePair<RegistrationKey, Type>> GetReverseEnumerable()
            {
                var node = this;
                while (!node.IsLast)
                {
                    yield return new KeyValuePair<RegistrationKey, Type>(node._currentRegistrationKey, node._currentResolvedType);
                    node = node._nextNode;
                }
            }

            public Type[] ToTypeList()
            {
                return GetReverseEnumerable().Select(i => i.Value ?? i.Key.Type).Reverse().ToArray();
            }

            public override string ToString()
            {
                return string.Join(",", GetReverseEnumerable().Select(n => string.Format("{0}:{1}", n.Key, n.Value)));
            }
        }

        private struct RegistrationKey
        {
            public readonly Type Type;
            public readonly string Name;

            public RegistrationKey(Type type, string name)
            {
                if (type == null) throw new ArgumentNullException("type");

                Type = type;
                Name = name;
            }

            private Type TypeGroup
            {
                get
                {
                    if (Type.IsGenericType && !Type.IsGenericTypeDefinition)
                        return Type.GetGenericTypeDefinition();
                    return Type;
                }
            }

            public override string ToString()
            {
                Debug.Assert(Type.FullName != null);
                if (Name == null)
                    return Type.FullName;

                return string.Format("{0}('{1}')", Type.FullName, Name);
            }

            bool Equals(RegistrationKey other)
            {
                var isInvertable = other.TypeGroup == Type || other.Type == TypeGroup || other.Type == Type;
                return isInvertable && String.Equals(other.Name, Name, StringComparison.CurrentCultureIgnoreCase);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (obj.GetType() != typeof(RegistrationKey)) return false;
                return Equals((RegistrationKey)obj);
            }

            public override int GetHashCode()
            {
                return TypeGroup.GetHashCode();
            }
        }

        #region Registration types

        private enum SolvingStrategy
        {
            PerContext,
            PerDependency
        }

        private interface IRegistration
        {
            object Resolve(ObjectContainer container, RegistrationKey keyToResolve, ResolutionList resolutionPath);
        }

        private class TypeRegistration : RegistrationWithStrategy, IRegistration
        {
            private readonly Type _implementationType;
            private readonly object _syncRoot = new object();

            public TypeRegistration(Type implementationType)
            {
                _implementationType = implementationType;
            }

            protected override object ResolvePerContext(ObjectContainer container, RegistrationKey keyToResolve, ResolutionList resolutionPath)
            {
                var typeToConstruct = GetTypeToConstruct(keyToResolve);

                var pooledObjectKey = new RegistrationKey(typeToConstruct, keyToResolve.Name);

                var result = ExecuteWithLock(_syncRoot, () => container.GetPooledObject(pooledObjectKey), () =>
                {
                    if (typeToConstruct.IsInterface)
                        throw new ObjectContainerException("Interface cannot be resolved: " + keyToResolve,
                            resolutionPath.ToTypeList());

                    var obj = container.CreateObject(typeToConstruct, resolutionPath, keyToResolve);
                    container._objectPool.Add(pooledObjectKey, obj);
                    return obj;
                }, resolutionPath, container.ConcurrentObjectResolutionTimeout);

                return result;
            }



            protected override object ResolvePerDependency(ObjectContainer container, RegistrationKey keyToResolve, ResolutionList resolutionPath)
            {
                var typeToConstruct = GetTypeToConstruct(keyToResolve);
                if (typeToConstruct.IsInterface)
                    throw new ObjectContainerException("Interface cannot be resolved: " + keyToResolve, resolutionPath.ToTypeList());
                return container.CreateObject(typeToConstruct, resolutionPath, keyToResolve);
            }

            private Type GetTypeToConstruct(RegistrationKey keyToResolve)
            {
                var targetType = _implementationType;
                if (targetType.IsGenericTypeDefinition)
                {
                    var typeArgs = keyToResolve.Type.GetGenericArguments();
                    targetType = targetType.MakeGenericType(typeArgs);
                }
                return targetType;
            }

            public override string ToString()
            {
                return "Type: " + _implementationType.FullName;
            }
        }

        private class InstanceRegistration : IRegistration
        {
            private readonly object _instance;

            public InstanceRegistration(object instance)
            {
                _instance = instance;
            }

            public object Resolve(ObjectContainer container, RegistrationKey keyToResolve, ResolutionList resolutionPath)
            {
                return _instance;
            }

            public override string ToString()
            {
                string instanceText;
                try
                {
                    instanceText = _instance.ToString();
                }
                catch (Exception ex)
                {
                    instanceText = ex.Message;
                }

                return "Instance: " + instanceText;
            }
        }

        private abstract class RegistrationWithStrategy : IStrategyRegistration
        {
            protected SolvingStrategy SolvingStrategy = SolvingStrategy.PerContext;
            public virtual object Resolve(ObjectContainer container, RegistrationKey keyToResolve, ResolutionList resolutionPath)
            {
                if (SolvingStrategy == SolvingStrategy.PerDependency)
                {
                    return ResolvePerDependency(container, keyToResolve, resolutionPath);
                }
                return ResolvePerContext(container, keyToResolve, resolutionPath);
            }

            protected abstract object ResolvePerContext(ObjectContainer container, RegistrationKey keyToResolve, ResolutionList resolutionPath);
            protected abstract object ResolvePerDependency(ObjectContainer container, RegistrationKey keyToResolve, ResolutionList resolutionPath);

            public IStrategyRegistration InstancePerDependency()
            {
                SolvingStrategy = SolvingStrategy.PerDependency;
                return this;
            }

            public IStrategyRegistration InstancePerContext()
            {
                SolvingStrategy = SolvingStrategy.PerContext;
                return this;
            }

            protected static object ExecuteWithLock(object lockObject, Func<object> getter, Func<object> factory, ResolutionList resolutionPath, TimeSpan timeout)
            {
                var obj = getter();

                if (obj != null)
                    return obj;

                if (timeout == TimeSpan.Zero)
                    return factory();

                if (Monitor.TryEnter(lockObject, timeout))
                {
                    try
                    {
                        obj = getter();

                        if (obj != null)
                            return obj;

                        obj = factory();
                        return obj;
                    }
                    finally
                    {
                        Monitor.Exit(lockObject);
                    }
                }

                throw new ObjectContainerException("Concurrent object resolution timeout (potential circular dependency).", resolutionPath.ToTypeList());
            }
        }

        private class FactoryRegistration : RegistrationWithStrategy, IRegistration
        {
            private readonly Delegate _factoryDelegate;
            private readonly object _syncRoot = new object();
            public FactoryRegistration(Delegate factoryDelegate)
            {
                _factoryDelegate = factoryDelegate;
            }

            protected override object ResolvePerContext(ObjectContainer container, RegistrationKey keyToResolve, ResolutionList resolutionPath)
            {
                var result = ExecuteWithLock(_syncRoot, () => container.GetPooledObject(keyToResolve), () =>
                {
                    var obj = container.InvokeFactoryDelegate(_factoryDelegate, resolutionPath, keyToResolve);
                    container._objectPool.Add(keyToResolve, obj);
                    return obj;
                }, resolutionPath, container.ConcurrentObjectResolutionTimeout);

                return result;
            }
            protected override object ResolvePerDependency(ObjectContainer container, RegistrationKey keyToResolve, ResolutionList resolutionPath)
            {
                return container.InvokeFactoryDelegate(_factoryDelegate, resolutionPath, keyToResolve);
            }
        }

        private class NonDisposableWrapper
        {
            public object Object { get; private set; }

            public NonDisposableWrapper(object obj)
            {
                Object = obj;
            }
        }

        private class NamedInstanceDictionaryRegistration : IRegistration
        {
            public object Resolve(ObjectContainer container, RegistrationKey keyToResolve, ResolutionList resolutionPath)
            {
                var typeToResolve = keyToResolve.Type;
                Debug.Assert(typeToResolve.IsGenericType && typeToResolve.GetGenericTypeDefinition() == typeof(IDictionary<,>));

                var genericArguments = typeToResolve.GetGenericArguments();
                var keyType = genericArguments[0];
                var targetType = genericArguments[1];
                var result = (IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(genericArguments));

                foreach (var namedRegistration in container._registrations.Where(r => r.Key.Name != null && r.Key.Type == targetType).Select(r => r.Key).ToList())
                {
                    var convertedKey = ChangeType(namedRegistration.Name, keyType);
                    Debug.Assert(convertedKey != null);
                    result.Add(convertedKey, container.Resolve(namedRegistration.Type, namedRegistration.Name));
                }

                return result;
            }

            private object ChangeType(string name, Type keyType)
            {
                if (keyType.IsEnum)
                    return Enum.Parse(keyType, name, true);

                Debug.Assert(keyType == typeof(string));
                return name;
            }
        }

        #endregion

        public static TimeSpan DefaultConcurrentObjectResolutionTimeout { get; set; } = TimeSpan.FromSeconds(1);
        private bool _isDisposed = false;
        private readonly ObjectContainer _baseContainer;
        private readonly ConcurrentDictionary<RegistrationKey, IRegistration> _registrations = new();
        private readonly List<RegistrationKey> _resolvedKeys = new();
        private readonly Dictionary<RegistrationKey, object> _objectPool = new();

        public event Action<object> ObjectCreated;
        public IObjectContainer BaseContainer => _baseContainer;

        /// <summary>
        /// Sets the timeout for thread-safe object resolution. By default, it uses the value of <see cref="DefaultConcurrentObjectResolutionTimeout"/> that is initialized to 1 second. Setting it to <see cref="TimeSpan.Zero"/> disables thread-safe resolution.
        /// </summary>
        public TimeSpan ConcurrentObjectResolutionTimeout { get; set; } = DefaultConcurrentObjectResolutionTimeout;

        public ObjectContainer(IObjectContainer baseContainer = null)
        {
            if (baseContainer != null && !(baseContainer is ObjectContainer))
                throw new ArgumentException("Base container must be an ObjectContainer", "baseContainer");

            _baseContainer = (ObjectContainer)baseContainer;
            RegisterInstanceAs<IObjectContainer>(this);
        }

        #region Registration

        public IStrategyRegistration RegisterTypeAs<TInterface>(Type implementationType, string name = null) where TInterface : class
        {
            Type interfaceType = typeof(TInterface);
            return RegisterTypeAsInternal(implementationType, interfaceType, name);
        }

        public IStrategyRegistration RegisterTypeAs<TType, TInterface>(string name = null) where TType : class, TInterface
        {
            Type interfaceType = typeof(TInterface);
            Type implementationType = typeof(TType);
            return RegisterTypeAsInternal(implementationType, interfaceType, name);
        }

        public IStrategyRegistration RegisterTypeAs(Type implementationType, Type interfaceType, string name = null)
        {
            if (!IsValidTypeMapping(implementationType, interfaceType))
                throw new InvalidOperationException("type mapping is not valid");
            return RegisterTypeAsInternal(implementationType, interfaceType, name);
        }

        private bool IsValidTypeMapping(Type implementationType, Type interfaceType)
        {
            if (interfaceType.IsAssignableFrom(implementationType))
                return true;

            if (interfaceType.IsGenericTypeDefinition && implementationType.IsGenericTypeDefinition)
            {
                var baseTypes = GetBaseTypes(implementationType).ToArray();
                return baseTypes.Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == interfaceType);
            }

            return false;
        }

        private static IEnumerable<Type> GetBaseTypes(Type type)
        {
            if (type.BaseType == null) return type.GetInterfaces();

            return Enumerable.Repeat(type.BaseType, 1)
                             .Concat(type.GetInterfaces())
                             .Concat(type.GetInterfaces().SelectMany(GetBaseTypes))
                             .Concat(GetBaseTypes(type.BaseType));
        }


        private RegistrationKey CreateNamedInstanceDictionaryKey(Type targetType)
        {
            return new RegistrationKey(typeof(IDictionary<,>).MakeGenericType(typeof(string), targetType), null);
        }

        private void AddRegistration(RegistrationKey key, IRegistration registration)
        {
            _registrations[key] = registration;

            AddNamedDictionaryRegistration(key);
        }

        private IRegistration EnsureImplicitRegistration(RegistrationKey key)
        {
            var registration = _registrations.GetOrAdd(key, (registrationKey => new TypeRegistration(registrationKey.Type)));

            AddNamedDictionaryRegistration(key);

            return registration;
        }

        private void AddNamedDictionaryRegistration(RegistrationKey key)
        {
            if (key.Name != null)
            {
                var dictKey = CreateNamedInstanceDictionaryKey(key.Type);
                _registrations.TryAdd(dictKey, new NamedInstanceDictionaryRegistration());
            }
        }

        private IStrategyRegistration RegisterTypeAsInternal(Type implementationType, Type interfaceType, string name)
        {
            var registrationKey = new RegistrationKey(interfaceType, name);
            AssertNotResolved(registrationKey);

            ClearRegistrations(registrationKey);
            var typeRegistration = new TypeRegistration(implementationType);
            AddRegistration(registrationKey, typeRegistration);

            return typeRegistration;
        }

        public void RegisterInstanceAs(object instance, Type interfaceType, string name = null, bool dispose = false)
        {
            if (instance == null)
                throw new ArgumentNullException("instance");
            var registrationKey = new RegistrationKey(interfaceType, name);
            AssertNotResolved(registrationKey);

            ClearRegistrations(registrationKey);
            AddRegistration(registrationKey, new InstanceRegistration(instance));
            _objectPool[new RegistrationKey(instance.GetType(), name)] = GetPoolableInstance(instance, dispose);
        }

        private static object GetPoolableInstance(object instance, bool dispose)
        {
            return (instance is IDisposable) && !dispose ? new NonDisposableWrapper(instance) : instance;
        }

        public void RegisterInstanceAs<TInterface>(TInterface instance, string name = null, bool dispose = false) where TInterface : class
        {
            RegisterInstanceAs(instance, typeof(TInterface), name, dispose);
        }

        public IStrategyRegistration RegisterFactoryAs<TInterface>(Func<TInterface> factoryDelegate, string name = null)
        {
            return RegisterFactoryAs(factoryDelegate, typeof(TInterface), name);
        }

        public IStrategyRegistration RegisterFactoryAs<TInterface>(Func<IObjectContainer, TInterface> factoryDelegate, string name = null)
        {
            return RegisterFactoryAs(factoryDelegate, typeof(TInterface), name);
        }

        public void RegisterFactoryAs<TInterface>(Delegate factoryDelegate, string name = null)
        {
            RegisterFactoryAs(factoryDelegate, typeof(TInterface), name);
        }

        public IStrategyRegistration RegisterFactoryAs(Delegate factoryDelegate, Type interfaceType, string name = null)
        {
            if (factoryDelegate == null) throw new ArgumentNullException("factoryDelegate");
            if (interfaceType == null) throw new ArgumentNullException("interfaceType");

            var registrationKey = new RegistrationKey(interfaceType, name);
            AssertNotResolved(registrationKey);

            ClearRegistrations(registrationKey);
            var factoryRegistration = new FactoryRegistration(factoryDelegate);
            AddRegistration(registrationKey, factoryRegistration);

            return factoryRegistration;
        }

        public bool IsRegistered<T>()
        {
            return IsRegistered<T>(null);
        }

        public bool IsRegistered<T>(string name)
        {
            Type typeToResolve = typeof(T);

            var keyToResolve = new RegistrationKey(typeToResolve, name);

            return _registrations.ContainsKey(keyToResolve);
        }

        // ReSharper disable once UnusedParameter.Local
        private void AssertNotResolved(RegistrationKey interfaceType)
        {
            if (_resolvedKeys.Contains(interfaceType))
                throw new ObjectContainerException("An object has been resolved for this interface already.", null);
        }

        private void ClearRegistrations(RegistrationKey registrationKey)
        {
            _registrations.TryRemove(registrationKey, out _);
        }


        #endregion

        #region Resolve

        public T Resolve<T>()
        {
            return Resolve<T>(null);
        }

        public T Resolve<T>(string name)
        {
            Type typeToResolve = typeof(T);

            object resolvedObject = Resolve(typeToResolve, name);

            return (T)resolvedObject;
        }

        public object Resolve(Type typeToResolve, string name = null)
        {
            return Resolve(typeToResolve, new ResolutionList(), name);
        }

        public IEnumerable<T> ResolveAll<T>() where T : class
        {
            return _registrations
                .Where(x => x.Key.Type == typeof(T))
                .Select(x => Resolve(x.Key.Type, x.Key.Name) as T);
        }

        private object Resolve(Type typeToResolve, ResolutionList resolutionPath, string name)
        {
            AssertNotDisposed();

            var keyToResolve = new RegistrationKey(typeToResolve, name);
            object resolvedObject = ResolveObject(keyToResolve, resolutionPath);
            if (!_resolvedKeys.Contains(keyToResolve))
            {
                _resolvedKeys.Add(keyToResolve);
            }
            Debug.Assert(typeToResolve.IsInstanceOfType(resolvedObject));
            return resolvedObject;
        }

        private KeyValuePair<ObjectContainer, IRegistration>? GetRegistrationResult(RegistrationKey keyToResolve)
        {
            IRegistration registration;
            if (_registrations.TryGetValue(keyToResolve, out registration))
            {
                return new KeyValuePair<ObjectContainer, IRegistration>(this, registration);
            }

            if (_baseContainer != null)
                return _baseContainer.GetRegistrationResult(keyToResolve);

            if (IsSpecialNamedInstanceDictionaryKey(keyToResolve))
            {
                var targetType = keyToResolve.Type.GetGenericArguments()[1];
                return GetRegistrationResult(CreateNamedInstanceDictionaryKey(targetType));
            }

            // if there was no named registration, we still return an empty dictionary
            if (IsDefaultNamedInstanceDictionaryKey(keyToResolve))
            {
                return new KeyValuePair<ObjectContainer, IRegistration>(this, new NamedInstanceDictionaryRegistration());
            }

            return null;
        }

        private bool IsDefaultNamedInstanceDictionaryKey(RegistrationKey keyToResolve)
        {
            return IsNamedInstanceDictionaryKey(keyToResolve) &&
                   keyToResolve.Type.GetGenericArguments()[0] == typeof(string);
        }

        private bool IsSpecialNamedInstanceDictionaryKey(RegistrationKey keyToResolve)
        {
            return IsNamedInstanceDictionaryKey(keyToResolve) &&
                   keyToResolve.Type.GetGenericArguments()[0].IsEnum;
        }

        private bool IsNamedInstanceDictionaryKey(RegistrationKey keyToResolve)
        {
            return keyToResolve.Name == null && keyToResolve.Type.IsGenericType && keyToResolve.Type.GetGenericTypeDefinition() == typeof(IDictionary<,>);
        }

        private object GetPooledObject(RegistrationKey pooledObjectKey)
        {
            object obj;
            if (GetObjectFromPool(pooledObjectKey, out obj))
                return obj;

            return null;
        }

        private bool GetObjectFromPool(RegistrationKey pooledObjectKey, out object obj)
        {
            if (!_objectPool.TryGetValue(pooledObjectKey, out obj))
                return false;

            var nonDisposableWrapper = obj as NonDisposableWrapper;
            if (nonDisposableWrapper != null)
                obj = nonDisposableWrapper.Object;

            return true;
        }

        private object ResolveObject(RegistrationKey keyToResolve, ResolutionList resolutionPath)
        {
            if (keyToResolve.Type.IsPrimitive || keyToResolve.Type == typeof(string) || keyToResolve.Type.IsValueType)
                throw new ObjectContainerException("Primitive types or structs cannot be resolved: " + keyToResolve.Type.FullName, resolutionPath.ToTypeList());

            var registrationResult = GetRegistrationResult(keyToResolve);

            var registrationToUse = registrationResult ??
                                    new KeyValuePair<ObjectContainer, IRegistration>(this, EnsureImplicitRegistration(keyToResolve));

            var resolutionPathForResolve = registrationToUse.Key == this ?
                resolutionPath : new ResolutionList();
            var result = registrationToUse.Value.Resolve(registrationToUse.Key, keyToResolve, resolutionPathForResolve);

            return result;
        }


        private object CreateObject(Type type, ResolutionList resolutionPath, RegistrationKey keyToResolve)
        {
            var ctors = type.GetConstructors();
            if (ctors.Length == 0)
                ctors = type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);

            Debug.Assert(ctors.Length > 0, "Class must have a constructor!");

            int maxParamCount = ctors.Max(ctor => ctor.GetParameters().Length);
            var maxParamCountCtors = ctors.Where(ctor => ctor.GetParameters().Length == maxParamCount).ToArray();

            object obj;
            if (maxParamCountCtors.Length == 1)
            {
                ConstructorInfo ctor = maxParamCountCtors[0];
                if (resolutionPath.Contains(keyToResolve))
                    throw new ObjectContainerException("Circular dependency found! " + type.FullName, resolutionPath.ToTypeList());

                var args = ResolveArguments(ctor.GetParameters(), keyToResolve, resolutionPath.AddToEnd(keyToResolve, type));
                obj = ctor.Invoke(args);
            }
            else
            {
                throw new ObjectContainerException("Multiple public constructors with same maximum parameter count are not supported! " + type.FullName, resolutionPath.ToTypeList());
            }

            OnObjectCreated(obj);

            return obj;
        }

        protected virtual void OnObjectCreated(object obj)
        {
            var eventHandler = ObjectCreated;
            if (eventHandler != null)
                eventHandler(obj);
        }

        private object InvokeFactoryDelegate(Delegate factoryDelegate, ResolutionList resolutionPath, RegistrationKey keyToResolve)
        {
            if (resolutionPath.Contains(keyToResolve))
                throw new ObjectContainerException("Circular dependency found! " + factoryDelegate, resolutionPath.ToTypeList());

            var args = ResolveArguments(factoryDelegate.Method.GetParameters(), keyToResolve, resolutionPath.AddToEnd(keyToResolve, null));
            return factoryDelegate.DynamicInvoke(args);
        }

        private object[] ResolveArguments(IEnumerable<ParameterInfo> parameters, RegistrationKey keyToResolve, ResolutionList resolutionPath)
        {
            return parameters.Select(p => IsRegisteredNameParameter(p) ? ResolveRegisteredName(keyToResolve) : Resolve(p.ParameterType, resolutionPath, null)).ToArray();
        }

        private object ResolveRegisteredName(RegistrationKey keyToResolve)
        {
            return keyToResolve.Name;
        }

        private bool IsRegisteredNameParameter(ParameterInfo parameterInfo)
        {
            return parameterInfo.ParameterType == typeof(string) &&
                   parameterInfo.Name.Equals(REGISTERED_NAME_PARAMETER_NAME);
        }

        #endregion

        public override string ToString()
        {
            return string.Join(Environment.NewLine,
                _registrations
                    .Where(r => !(r.Value is NamedInstanceDictionaryRegistration))
                    .Select(r => string.Format("{0} -> {1}", r.Key, (r.Key.Type == typeof(IObjectContainer) && r.Key.Name == null) ? "<self>" : r.Value.ToString())));
        }

        private void AssertNotDisposed()
        {
            if (_isDisposed)
                throw new ObjectContainerException("Object container disposed", null);
        }

        public void Dispose()
        {
            _isDisposed = true;

            foreach (var obj in _objectPool.Values.OfType<IDisposable>().Where(o => !ReferenceEquals(o, this)))
                obj.Dispose();

            _objectPool.Clear();
            _registrations.Clear();
            _resolvedKeys.Clear();
        }
    }
}

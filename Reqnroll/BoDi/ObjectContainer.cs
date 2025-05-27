using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Reqnroll.BoDi;

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
            _currentRegistrationKey = currentRegistrationKey;
            _currentResolvedType = currentResolvedType;
            _nextNode = nextNode ?? throw new ArgumentNullException(nameof(nextNode));
        }

        public ResolutionList AddToEnd(RegistrationKey registrationKey, Type resolvedType)
        {
            return new ResolutionList(registrationKey, resolvedType, this);
        }

        // ReSharper disable once UnusedMember.Local
        public bool Contains(Type resolvedType)
        {
            if (resolvedType == null) throw new ArgumentNullException(nameof(resolvedType));
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
            return string.Join(",", GetReverseEnumerable().Select(n => $"{n.Key}:{n.Value}"));
        }
    }

    private readonly struct RegistrationKey(Type type, string name)
    {
        public readonly Type Type = type ?? throw new ArgumentNullException(nameof(type));
        public readonly string Name = name;

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

            return $"{Type.FullName}('{Name}')";
        }

        bool Equals(RegistrationKey other)
        {
            var isInvertible = other.TypeGroup == Type || other.Type == TypeGroup || other.Type == Type;
            return isInvertible && String.Equals(other.Name, Name, StringComparison.CurrentCultureIgnoreCase);
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

    private class TypeRegistration(Type _implementationType) : RegistrationWithStrategy, IRegistration
    {
        private readonly object _syncRoot = new();

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

    private class InstanceRegistration(object _instance) : IRegistration
    {
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
            {
                Monitor.Enter(lockObject);
                try
                {
                    return factory();
                }
                finally
                {
                    Monitor.Exit(lockObject);
                }
            }

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

            throw new ObjectContainerException($"Concurrent object resolution timeout (potential circular dependency). To increase the current timeout ({timeout.TotalSeconds:F} seconds), set the static 'ObjectContainer.DefaultConcurrentObjectResolutionTimeout' property or update the 'ConcurrentObjectResolutionTimeout' property of the current container.", resolutionPath.ToTypeList());
        }
    }

    private class FactoryRegistration(Delegate _factoryDelegate) : RegistrationWithStrategy, IRegistration
    {
        private readonly object _syncRoot = new();

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

    private class NonDisposableWrapper(object obj)
    {
        public object Object { get; } = obj;
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
            var result = (IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(genericArguments))!;

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
    private TimeSpan? _concurrentObjectResolutionTimeout;

    public event Action<object> ObjectCreated;
    public IObjectContainer BaseContainer => _baseContainer;

    /// <summary>
    /// Sets the timeout for thread-safe object resolution. By default, it uses the value of <see cref="DefaultConcurrentObjectResolutionTimeout"/> that is initialized to 1 second. Setting it to <see cref="TimeSpan.Zero"/> disables thread-safe resolution.
    /// </summary>
    public TimeSpan ConcurrentObjectResolutionTimeout
    {
        get => _concurrentObjectResolutionTimeout ?? DefaultConcurrentObjectResolutionTimeout;
        set => _concurrentObjectResolutionTimeout = value;
    }

    public ObjectContainer(IObjectContainer baseContainer = null)
    {
        if (baseContainer != null && !(baseContainer is ObjectContainer))
            throw new ArgumentException("Base container must be an ObjectContainer", nameof(baseContainer));

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
            throw new InvalidOperationException($"The type mapping is not valid. The {implementationType} is not assignable to the interface {interfaceType}");
        return RegisterTypeAsInternal(implementationType, interfaceType, name);
    }

    private bool IsValidTypeMapping(Type implementationType, Type interfaceType)
    {
        if (interfaceType.IsAssignableFrom(implementationType))
            return true;

        if (interfaceType.IsGenericTypeDefinition && implementationType!.IsGenericTypeDefinition)
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
        AssertNotDisposed();

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
            throw new ArgumentNullException(nameof(instance));

        AssertNotDisposed();

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
        if (factoryDelegate == null) throw new ArgumentNullException(nameof(factoryDelegate));
        if (interfaceType == null) throw new ArgumentNullException(nameof(interfaceType));

        AssertNotDisposed();

        var registrationKey = new RegistrationKey(interfaceType, name);
        AssertNotResolved(registrationKey);

        ClearRegistrations(registrationKey);
        var factoryRegistration = new FactoryRegistration(factoryDelegate);
        AddRegistration(registrationKey, factoryRegistration);

        return factoryRegistration;
    }

    /// <inheritdoc/>
    public bool IsRegistered<T>(string name = null) => IsRegistered(typeof(T), name);

    /// <inheritdoc/>
    public bool IsRegistered(Type type, string name = null)
    {
        var keyToResolve = new RegistrationKey(type, name);

        if (_registrations.ContainsKey(keyToResolve))
        {
            return true;
        }
        else if (BaseContainer != null)
        {
            // Recursively check the base container
            return BaseContainer.IsRegistered(type, name);
        }

        // We are at the top of the container hierarchy and the registration is not found
        return false;
    }

    // ReSharper disable once UnusedParameter.Local
    private void AssertNotResolved(RegistrationKey interfaceType)
    {
        if (_resolvedKeys.Contains(interfaceType))
            throw new ObjectContainerException($"An object has already been resolved for the interface \"{interfaceType}\".", null);
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
        if (_registrations.TryGetValue(keyToResolve, out var registration))
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
        if (GetObjectFromPool(pooledObjectKey, out object obj))
            return obj;

        return null;
    }

    private bool GetObjectFromPool(RegistrationKey pooledObjectKey, out object obj)
    {
        if (!_objectPool.TryGetValue(pooledObjectKey, out obj))
            return false;

        if (obj is NonDisposableWrapper nonDisposableWrapper)
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
        var constructors = type.GetConstructors();
        if (constructors.Length == 0)
            constructors = type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);

        Debug.Assert(constructors.Length > 0, "Class must have a constructor!");

        int maxParamCount = constructors.Max(ctor => ctor.GetParameters().Length);
        var maxParamCountConstructors = constructors.Where(ctor => ctor.GetParameters().Length == maxParamCount).ToArray();

        object obj;
        if (maxParamCountConstructors.Length == 1)
        {
            ConstructorInfo ctor = maxParamCountConstructors[0];
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
               parameterInfo!.Name!.Equals(REGISTERED_NAME_PARAMETER_NAME);
    }

    #endregion

    public override string ToString()
    {
        return string.Join(Environment.NewLine,
                           _registrations
                               .Where(r => !(r.Value is NamedInstanceDictionaryRegistration))
                               .Select(r => $"{r.Key} -> {((r.Key.Type == typeof(IObjectContainer) && r.Key.Name == null) ? "<self>" : r.Value.ToString())}"));
    }

    private void AssertNotDisposed()
    {
        if (_isDisposed)
            throw new ObjectContainerException("Object container disposed", null);
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;

        foreach (var obj in _objectPool.Values.OfType<IDisposable>().Where(o => !ReferenceEquals(o, this)))
            obj.Dispose();

        _objectPool.Clear();
        _registrations.Clear();
        _resolvedKeys.Clear();
    }
}

namespace RPGGame.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class GlobalDependencyDatabase
{
    private readonly Dictionary<Type, Type> _typeMappings = new();
    private readonly Dictionary<Type, object> _singletons = new();
    private readonly HashSet<Type> _currentlyResolving = new();

    public void RegisterAssemblyTypes(Assembly assembly)
    {
        var typesWithDependencyAttribute = assembly.GetTypes()
            .Where(t => t.GetCustomAttribute<DependencyAttribute>() != null && !t.IsAbstract && !t.IsInterface)
            .OrderBy(t => t.GetConstructors().FirstOrDefault()?.GetParameters().Length ?? 0)
            .ToList();

        var deferred = new List<(Type type, RegistrationType regType)>();

        // First pass - register what we can
        foreach (Type type in typesWithDependencyAttribute)
        {
            if (_singletons.ContainsKey(type))
                continue;

            var attribute = type.GetCustomAttribute<DependencyAttribute>();
            if (attribute != null)
            {
                if (!TryRegisterType(type, attribute.RegistrationType))
                {
                    deferred.Add((type, attribute.RegistrationType));
                }
            }
        }

        // Keep retrying deferred types until we make no more progress
        int lastCount;
        do
        {
            lastCount = deferred.Count;
            for (int i = deferred.Count - 1; i >= 0; i--)
            {
                var (type, regType) = deferred[i];
                if (_singletons.ContainsKey(type) || TryRegisterType(type, regType))
                {
                    deferred.RemoveAt(i);
                }
            }
        } while (deferred.Count < lastCount && deferred.Count > 0);

        // Report any that still failed
        foreach (var (type, _) in deferred)
        {
            UnityEngine.Debug.LogError($"[DI] Failed to register: {type.Name}");
        }
    }

    private bool TryRegisterType(Type type, RegistrationType registrationType)
    {
        if (_singletons.ContainsKey(type))
            return true;

        if (registrationType == RegistrationType.Singleton || registrationType == RegistrationType.Both)
        {
            var instance = TryCreateInstance(type);
            if (instance == null)
                return false;

            _typeMappings[type] = type;
            _singletons[type] = instance;
            UnityEngine.Debug.Log($"[DI] Registered: {type.Name}");
        }

        if (registrationType == RegistrationType.Interface || registrationType == RegistrationType.Both)
        {
            foreach (Type interfaceType in type.GetInterfaces())
            {
                _typeMappings[interfaceType] = type;
                if (_singletons.TryGetValue(type, out var instance))
                {
                    _singletons[interfaceType] = instance;
                }
            }
        }

        return true;
    }

    private object? TryCreateInstance(Type type)
    {
        if (_currentlyResolving.Contains(type))
            return null;

        _currentlyResolving.Add(type);

        try
        {
            var constructor = type.GetConstructors()
                .OrderBy(c => c.GetParameters().Length)
                .FirstOrDefault();

            if (constructor == null)
                return null;

            var parameters = constructor.GetParameters();
            var args = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                var paramType = parameters[i].ParameterType;

                if (_singletons.TryGetValue(paramType, out var existing))
                {
                    args[i] = existing;
                }
                else if (_typeMappings.TryGetValue(paramType, out var implType) && 
                         _singletons.TryGetValue(implType, out var implInstance))
                {
                    args[i] = implInstance;
                }
                else
                {
                    // Dependency not available yet - defer this type
                    return null;
                }
            }

            UnityEngine.Debug.Log($"[DI] Creating: {type.Name}");
            return constructor.Invoke(args);
        }
        finally
        {
            _currentlyResolving.Remove(type);
        }
    }

    public GlobalDependencyResolver Build()
    {
        UnityEngine.Debug.Log($"[DI] Build complete. {_singletons.Count} singletons.");
        return new GlobalDependencyResolver(_typeMappings, _singletons);
    }
}
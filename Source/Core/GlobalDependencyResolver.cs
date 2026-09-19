using System.Reflection;

namespace RPGGame.Core;

/// <summary>
/// Resolves dependencies and creates instances of registered types.
/// </summary>
public class GlobalDependencyResolver
{
    private readonly Dictionary<Type, Type> k_typeMappings;
    private readonly Dictionary<Type, object> k_singletons;

    public GlobalDependencyResolver(Dictionary<Type, Type> typeMappings, Dictionary<Type, object> singletons)
    {
        k_typeMappings = typeMappings;
        k_singletons = singletons;
    }

    public T Resolve<T>()
    {
        return (T)ResolveInternal(typeof(T));
    }

    private object ResolveInternal(Type type)
    {
        // Return existing singleton
        if (k_singletons.TryGetValue(type, out object resolve))
        {
            return resolve;
        }

        // Check type mappings
        if (k_typeMappings.TryGetValue(type, out Type implementationType))
        {
            // Check if implementation is a singleton
            if (k_singletons.TryGetValue(implementationType, out object implInstance))
            {
                return implInstance;
            }
            return CreateInstance(implementationType);
        }

        // Create transient instance
        if (!type.IsAbstract)
        {
            return CreateInstance(type);
        }

        throw new InvalidOperationException($"Type {type.FullName} not registered in container.");
    }

    public object CreateInstance(Type type)
    {
        // Return singleton if exists
        if (k_singletons.TryGetValue(type, out object instance))
        {
            return instance;
        }

        return CreateInstanceInternal(type, Array.Empty<object>());
    }

    public object CreateInstance(Type type, params object[] args)
    {
        // Return singleton if exists
        if (k_singletons.TryGetValue(type, out object instance))
        {
            return instance;
        }

        return CreateInstanceInternal(type, args);
    }

    private object CreateInstanceInternal(Type type, object[] additionalArgs)
    {
        ConstructorInfo[] constructors = type.GetConstructors();

        if (constructors.Length == 0)
        {
            throw new InvalidOperationException($"No public constructors found for type {type.FullName}.");
        }

        // Try constructors in order of parameter count (most specific first)
        foreach (ConstructorInfo constructor in constructors.OrderByDescending(c => c.GetParameters().Length))
        {
            try
            {
                ParameterInfo[] parameters = constructor.GetParameters();
                object[] parameterInstances = new object[parameters.Length];
                List<object> usedArgs = new List<object>();

                for (int i = 0; i < parameters.Length; i++)
                {
                    Type parameterType = parameters[i].ParameterType;

                    // Try to match with additional arguments first
                    object matchingArg = additionalArgs
                        .Except(usedArgs)
                        .FirstOrDefault(arg => parameterType.IsInstanceOfType(arg));

                    if (matchingArg != null)
                    {
                        parameterInstances[i] = matchingArg;
                        usedArgs.Add(matchingArg);
                    }
                    else
                    {
                        // Resolve via dependency injection
                        parameterInstances[i] = ResolveInternal(parameterType);
                    }
                }

                return constructor.Invoke(parameterInstances);
            }
            catch
            {
                // If this constructor doesn't work, try the next one
                continue;
            }
        }

        throw new InvalidOperationException($"No suitable constructor found for type {type.FullName} with the provided arguments.");
    }
}
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace RPGGame.Core.Save;

/// <summary>
/// Custom contract resolver that handles DoNotSave attributes and tracks
/// fields that need rebuilding after deserialization.
/// </summary>
public class SaveContractResolver : CamelCasePropertyNamesContractResolver {
	/// <summary>
	/// Fields/properties that need rebuilding after deserialization, keyed by type.
	/// </summary>
	private static readonly Dictionary<Type, List<RebuildInfo>> s_rebuildRegistry = new();

	protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization) {
		var property = base.CreateProperty(member, memberSerialization);

		// Check for DoNotSave attribute
		var doNotSave = member.GetCustomAttribute<DoNotSaveAttribute>();
		if (doNotSave != null) {
			property.Ignored = true;

			// Register for rebuild if needed
			if (doNotSave.RecreateAs != RecreationType.None && doNotSave.RecreateAs != RecreationType.LazyOnAccess) {
				RegisterForRebuild(member.DeclaringType!, member, doNotSave);
			}
		}

		return property;
	}

	private static void RegisterForRebuild(Type declaringType, MemberInfo member, DoNotSaveAttribute attr) {
		if (!s_rebuildRegistry.TryGetValue(declaringType, out var list)) {
			list = new List<RebuildInfo>();
			s_rebuildRegistry[declaringType] = list;
		}

		// Avoid duplicates
		if (list.Any(r => r.Member.Name == member.Name)) return;

		list.Add(new RebuildInfo(member, attr.RecreateAs, attr.FactoryMethod, attr.RebuildMethod, attr.RebuildOrder));

		// Keep sorted by order
		list.Sort((a, b) => a.Order.CompareTo(b.Order));
	}

	/// <summary>
	/// Rebuilds non-serialized fields on an object after deserialization.
	/// Call this after JsonConvert.DeserializeObject.
	/// </summary>
	public static void RebuildAfterLoad(object obj) {
		if (obj == null) return;

		var type = obj.GetType();
		RebuildType(obj, type);

		// Also check base types
		var baseType = type.BaseType;
		while (baseType != null && baseType != typeof(object)) {
			RebuildType(obj, baseType);
			baseType = baseType.BaseType;
		}
	}

	private static void RebuildType(object obj, Type type) {
		if (!s_rebuildRegistry.TryGetValue(type, out var rebuildList)) return;

		foreach (var info in rebuildList) {
			try {
				RebuildMember(obj, info);
			} catch (Exception ex) {
				Debug.LogError($"Failed to rebuild {type.Name}.{info.Member.Name}: {ex.Message}");
			}
		}
	}

	private static void RebuildMember(object obj, RebuildInfo info) {
		object? value = info.RecreateAs switch {
			RecreationType.EmptyCollection => CreateEmptyCollection(info.Member),
			RecreationType.LazyFactory => InvokeFactoryMethod(obj, info.FactoryMethod),
			RecreationType.RebuildFromData => InvokeRebuildMethod(obj, info.RebuildMethod),
			//RecreationType.Injected => ResolveFromDI(info.Member),
			_ => null
		};

		if (value != null) {
			SetMemberValue(obj, info.Member, value);
		}
	}

	private static object? CreateEmptyCollection(MemberInfo member) {
		var type = GetMemberType(member);

		// Handle common collection types
		if (type.IsGenericType) {
			var genericDef = type.GetGenericTypeDefinition();

			if (genericDef == typeof(List<>)) {
				return Activator.CreateInstance(type);
			}
			if (genericDef == typeof(Dictionary<,>)) {
				return Activator.CreateInstance(type);
			}
			if (genericDef == typeof(HashSet<>)) {
				return Activator.CreateInstance(type);
			}
			if (genericDef == typeof(Queue<>)) {
				return Activator.CreateInstance(type);
			}
			if (genericDef == typeof(Stack<>)) {
				return Activator.CreateInstance(type);
			}
		}

		// Try default constructor
		if (type.GetConstructor(Type.EmptyTypes) != null) {
			return Activator.CreateInstance(type);
		}

		return null;
	}

	private static object? InvokeFactoryMethod(object obj, string? methodName) {
		if (string.IsNullOrEmpty(methodName)) return null;

		var type = obj.GetType();
		var method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

		if (method == null) {
			Debug.LogWarning($"Factory method '{methodName}' not found on {type.Name}");
			return null;
		}

		return method.IsStatic ? method.Invoke(null, null) : method.Invoke(obj, null);
	}

	private static object? InvokeRebuildMethod(object obj, string? methodName) {
		if (string.IsNullOrEmpty(methodName)) return null;

		var type = obj.GetType();
		var method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

		if (method == null) {
			Debug.LogWarning($"Rebuild method '{methodName}' not found on {type.Name}");
			return null;
		}

		return method.Invoke(obj, null);
	}

	//private static object? ResolveFromDI(MemberInfo member) {
	//	var type = GetMemberType(member);
	//	return GlobalDependencyResolver.TryResolve(type, out var instance) ? instance : null;
	//}

	private static Type GetMemberType(MemberInfo member) {
		return member switch {
			FieldInfo fi => fi.FieldType,
			PropertyInfo pi => pi.PropertyType,
			_ => throw new ArgumentException($"Unsupported member type: {member.GetType()}")
		};
	}

	private static void SetMemberValue(object obj, MemberInfo member, object value) {
		switch (member) {
			case FieldInfo fi:
				fi.SetValue(obj, value);
				break;
			case PropertyInfo pi when pi.CanWrite:
				pi.SetValue(obj, value);
				break;
		}
	}

	/// <summary>
	/// Clears the rebuild registry (for testing).
	/// </summary>
	public static void ClearRegistry() => s_rebuildRegistry.Clear();
}

/// <summary>
/// Info about a field that needs rebuilding after deserialization.
/// </summary>
internal class RebuildInfo {
	public MemberInfo Member { get; }
	public RecreationType RecreateAs { get; }
	public string? FactoryMethod { get; }
	public string? RebuildMethod { get; }
	public int Order { get; }

	public RebuildInfo(
		MemberInfo member,
		RecreationType recreateAs,
		string? factoryMethod,
		string? rebuildMethod,
		int order
	) {
		Member = member ?? throw new ArgumentNullException(nameof(member));
		RecreateAs = recreateAs;
		FactoryMethod = factoryMethod;
		RebuildMethod = rebuildMethod;
		Order = order;
	}
}
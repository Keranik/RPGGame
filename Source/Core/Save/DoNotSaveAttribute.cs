using Newtonsoft.Json;

namespace RPGGame.Core.Save;

/// <summary>
/// Marks a field or property to be excluded from save/load serialization.
/// Wraps JsonIgnore and provides additional metadata for recreation on load.
/// </summary>
/// <remarks>
/// Use cases:
/// - Computed properties (Inverse, Complement, etc.)
/// - Cached collections that rebuild on access
/// - Lazy-initialized fields
/// - Runtime-only state (UI references, event handlers)
/// - Lookup dictionaries built from serialized lists
/// </remarks>
/// <example>
/// <code>
/// // Simple exclusion - no recreation needed
/// [DoNotSave]
/// public bool IsZero => _value == 0;
/// 
/// // Cached dictionary rebuilt from serialized list
/// [DoNotSave(RecreateAs = RecreationType.EmptyCollection)]
/// private Dictionary&lt;string, Item&gt; _itemLookup;
/// 
/// // Lazy initialization with factory method
/// [DoNotSave(RecreateAs = RecreationType.LazyFactory, FactoryMethod = nameof(CreateCache))]
/// private ExpensiveCache? _cache;
/// 
/// // Rebuilt from other serialized data
/// [DoNotSave(RecreateAs = RecreationType.RebuildFromData, RebuildMethod = nameof(RebuildLookup))]
/// private Dictionary&lt;Proto.ID, StatModifier&gt; _modifierLookup;
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public sealed class DoNotSaveAttribute : Attribute {
	/// <summary>
	/// How to recreate this field/property when loading.
	/// </summary>
	public RecreationType RecreateAs { get; init; } = RecreationType.None;

	/// <summary>
	/// Name of the factory method to call for LazyFactory recreation.
	/// Method should be parameterless and return the field's type.
	/// </summary>
	public string? FactoryMethod { get; init; }

	/// <summary>
	/// Name of the method to call after deserialization to rebuild this field.
	/// Method should be parameterless (uses other deserialized fields as input).
	/// </summary>
	public string? RebuildMethod { get; init; }

	/// <summary>
	/// Optional description of why this field is not saved (for documentation).
	/// </summary>
	public string? Reason { get; init; }

	/// <summary>
	/// Priority for rebuild order (lower = earlier). Default is 0.
	/// Use when multiple fields need rebuilding in a specific order.
	/// </summary>
	public int RebuildOrder { get; init; } = 0;
}

/// <summary>
/// How to recreate a non-serialized field after loading.
/// </summary>
public enum RecreationType {
	/// <summary>
	/// No recreation needed - computed property or truly transient state.
	/// Field will be default/null after load.
	/// </summary>
	None,

	/// <summary>
	/// Create an empty collection (List, Dictionary, HashSet, etc.).
	/// The collection will be populated lazily or via RebuildMethod.
	/// </summary>
	EmptyCollection,

	/// <summary>
	/// Call a parameterless factory method to create the initial value.
	/// Specify the method name in FactoryMethod.
	/// </summary>
	LazyFactory,

	/// <summary>
	/// Call a rebuild method after all deserialization is complete.
	/// The method uses other deserialized fields to rebuild this one.
	/// Specify the method name in RebuildMethod.
	/// </summary>
	RebuildFromData,

	/// <summary>
	/// Field is a cached/memoized value that will be recomputed on first access.
	/// No explicit recreation needed - the property getter handles it.
	/// </summary>
	LazyOnAccess,

	/// <summary>
	/// Field references a singleton or injected dependency.
	/// Will be re-injected after load via dependency injection.
	/// </summary>
	Injected
}
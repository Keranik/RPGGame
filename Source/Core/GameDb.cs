using RPGGame.Core.Prototypes;
using UnityEngine;

#nullable disable

namespace RPGGame.Core;

/// <summary>
/// Central database for all game prototypes (immutable data definitions).
/// Provides fast lookup, filtering, and query capabilities for game data.
/// Thread-safe after initialization via LockAndInitialize().
/// </summary>
/// <remarks>
/// AAA Design Principles Applied:
/// - Single source of truth for all prototype data
/// - Fast O(1) lookups by ID
/// - Type-safe generic accessors
/// - Immutable after initialization (lock pattern)
/// - Clear distinction between "must exist" (Get) and "might exist" (TryGet)
/// - Comprehensive query API for filtering and iteration
/// </remarks>
[Dependency(registrationType: RegistrationType.Singleton)]
public class GameDb {
	#region Fields

	/// <summary>
	/// Primary storage: Proto.ID -> Proto (O(1) lookup by ID).
	/// </summary>
	private readonly Dictionary<Proto.ID, Proto> k_protoById = [];

	/// <summary>
	/// Type index: Type -> Set of Protos (fast iteration by type).
	/// </summary>
	private readonly Dictionary<Type, HashSet<Proto>> k_protosByType = [];

	/// <summary>
	/// Whether the database is locked (immutable after lock).
	/// </summary>
	private bool k_isLocked;

	/// <summary>
	/// Count cache for fast Count calls.
	/// </summary>
	private int k_totalCount;

	#endregion

	#region Properties

	/// <summary>
	/// Whether the database is locked and initialized.
	/// Once locked, no modifications are allowed.
	/// </summary>
	public bool IsLocked => k_isLocked;

	/// <summary>
	/// Total number of registered prototypes.
	/// </summary>
	public int Count => k_totalCount;

	/// <summary>
	/// Number of distinct prototype types registered.
	/// </summary>
	public int TypeCount => k_protosByType.Count;

	#endregion

	#region Constructor

	public GameDb() {
		Debug.Log("GameDb initialized.");
	}

	#endregion

	#region Registration

	/// <summary>
	/// Registers a prototype to the database.
	/// </summary>
	/// <typeparam name="T">The prototype type.</typeparam>
	/// <param name="proto">The prototype to register.</param>
	/// <returns>The registered prototype (for chaining).</returns>
	/// <exception cref="InvalidOperationException">If database is locked or ID already exists.</exception>
	public T RegisterProto<T>(T proto) where T : Proto {
		if (k_isLocked) {
			throw new InvalidOperationException(
				$"Cannot register proto '{proto.Id}': GameDb is locked. " +
				"All prototypes must be registered before LockAndInitialize() is called.");
		}

		if (k_protoById.ContainsKey(proto.Id)) {
			throw new InvalidOperationException(
				$"Duplicate Proto ID: '{proto.Id}' already exists. " +
				"Each prototype must have a unique ID.");
		}

		// Add to primary storage
		k_protoById.Add(proto.Id, proto);
		k_totalCount++;

		// Add to type index
		Type protoType = typeof(T);
		if (!k_protosByType.TryGetValue(protoType, out var typeSet)) {
			typeSet = [];
			k_protosByType[protoType] = typeSet;
		}
		typeSet.Add(proto);

		return proto;
	}

	/// <summary>
	/// Registers multiple prototypes at once.
	/// </summary>
	/// <typeparam name="T">The prototype type.</typeparam>
	/// <param name="protos">The prototypes to register.</param>
	/// <returns>Number of prototypes registered.</returns>
	public int RegisterProtos<T>(IEnumerable<T> protos) where T : Proto {
		int count = 0;
		foreach (var proto in protos) {
			RegisterProto(proto);
			count++;
		}
		return count;
	}

	#endregion

	#region Initialization & Locking

	/// <summary>
	/// Locks the database and initializes all prototypes.
	/// After this call, no modifications are allowed.
	/// </summary>
	public void LockAndInitialize() {
		if (k_isLocked) {
			Debug.LogWarning("GameDb.LockAndInitialize() called but database is already locked.");
			return;
		}

		k_isLocked = true;

		// Initialize all prototypes
		foreach (Proto proto in k_protoById.Values) {
			proto.OnInitialize();
		}

		Debug.Log($"GameDb locked and initialized with {k_totalCount} prototypes across {k_protosByType.Count} types.");
	}

	#endregion

	#region Get (Must Exist - Throws if Not Found)

	/// <summary>
	/// Gets a prototype by ID. Throws if not found.
	/// Use this when you KNOW the prototype must exist (e.g., hardcoded IDs).
	/// </summary>
	/// <typeparam name="T">The expected prototype type.</typeparam>
	/// <param name="id">The prototype ID.</param>
	/// <returns>The prototype.</returns>
	/// <exception cref="KeyNotFoundException">If the ID is not found.</exception>
	/// <exception cref="InvalidCastException">If the prototype is not of type T.</exception>
	public T Get<T>(Proto.ID id) where T : Proto {
		if (!k_protoById.TryGetValue(id, out Proto proto)) {
			throw new KeyNotFoundException(
				$"Proto with ID '{id}' not found in GameDb. " +
				$"Requested type: {typeof(T).Name}. " +
				"Ensure the prototype is registered before accessing it.");
		}

		if (proto is not T typed) {
			throw new InvalidCastException(
				$"Proto with ID '{id}' exists but is type '{proto.GetType().Name}', " +
				$"not the requested type '{typeof(T).Name}'.");
		}

		return typed;
	}

	/// <summary>
	/// Gets a prototype by typed ID. Throws if not found.
	/// Convenience overload for strongly-typed IDs.
	/// </summary>
	public T Get<T, TId>(TId id) where T : Proto where TId : struct {
		// Convert typed ID to Proto.ID (assumes ID has implicit conversion or Value property)
		var protoId = new Proto.ID(id.ToString());
		return Get<T>(protoId);
	}

	#endregion

	#region TryGet (Might Not Exist - Returns Bool)
	
	/// <summary>
	/// Attempts to get a prototype by ID.
	/// Use this when the prototype might not exist.
	/// </summary>
	/// <typeparam name="T">The expected prototype type.</typeparam>
	/// <param name="id">The prototype ID.</param>
	/// <param name="proto">The prototype if found, default otherwise.</param>
	/// <returns>True if found and type matches, false otherwise.</returns>
	public bool TryGetProto<T>(Proto.ID id, out T proto) where T : Proto {
		if (k_protoById.TryGetValue(id, out Proto found)) {
			if (found is T typed) {
				proto = typed;
				return true;
			}
			Debug.LogWarning(
				$"TryGetProto: Proto '{id}' found but type mismatch. " +
				$"Expected '{typeof(T).Name}', got '{found.GetType().Name}'.");
		} else {
			// Log missing ID as error - this often indicates a missing definition
			Debug.LogError(
				$"TryGetProto: Proto '{id}' not found in GameDb. " +
				$"Expected type: '{typeof(T).Name}'. " +
				"Ensure the prototype is registered in the appropriate definitions file.");
		}
		proto = default;
		return false;
	}

	/// <summary>
	/// Attempts to get a prototype by ID, returning null if not found.
	/// Convenience method for nullable access patterns.
	/// </summary>
	/// <typeparam name="T">The expected prototype type.</typeparam>
	/// <param name="id">The prototype ID.</param>
	/// <param name="logIfMissing">Whether to log an error if not found.</param>
	/// <returns>The prototype or null if not found.</returns>
	public T GetOrNull<T>(Proto.ID id, bool logIfMissing = true) where T : Proto {
		if (k_protoById.TryGetValue(id, out Proto found)) {
			if (found is T typed) {
				return typed;
			}
			Debug.LogWarning(
				$"GetOrNull: Proto '{id}' found but type mismatch. " +
				$"Expected '{typeof(T).Name}', got '{found.GetType().Name}'.");
			return null;
		}

		if (logIfMissing) {
			Debug.LogError(
				$"GetOrNull: Proto '{id}' not found in GameDb. " +
				$"Expected type: '{typeof(T).Name}'. " +
				"Ensure the prototype is registered in the appropriate definitions file.");
		}
		return null;
	}

	/// <summary>
	/// Gets a prototype or returns a default value if not found.
	/// </summary>
	/// <typeparam name="T">The expected prototype type.</typeparam>
	/// <param name="id">The prototype ID.</param>
	/// <param name="defaultValue">Value to return if not found.</param>
	/// <param name="logIfMissing">Whether to log an error if not found.</param>
	/// <returns>The prototype or the default value.</returns>
	public T GetOrDefault<T>(Proto.ID id, T defaultValue = default, bool logIfMissing = true) where T : Proto {
		if (k_protoById.TryGetValue(id, out Proto found)) {
			if (found is T typed) {
				return typed;
			}
			Debug.LogWarning(
				$"GetOrDefault: Proto '{id}' found but type mismatch. " +
				$"Expected '{typeof(T).Name}', got '{found.GetType().Name}'.");
			return defaultValue;
		}

		if (logIfMissing) {
			Debug.LogError(
				$"GetOrDefault: Proto '{id}' not found in GameDb. " +
				$"Expected type: '{typeof(T).Name}'. " +
				"Ensure the prototype is registered in the appropriate definitions file.");
		}
		return defaultValue;
	}

	#endregion

	#region GetAll (Collection Access)

	/// <summary>
	/// Gets all prototypes of a specific type (including subtypes).
	/// </summary>
	/// <typeparam name="T">The prototype type or interface.</typeparam>
	/// <returns>All matching prototypes.</returns>
	public IEnumerable<T> GetAll<T>() where T : class {
		return GetAllProtosOfType<T>();
	}

	/// <summary>
	/// Gets all prototypes of a specific type (including subtypes).
	/// Alias for GetAll for backward compatibility.
	/// </summary>
	/// <typeparam name="T">The prototype type or interface.</typeparam>
	/// <returns>All matching prototypes.</returns>
	public IEnumerable<T> GetAllProtosOfType<T>() where T : class {
		// Find all types that are assignable to T and collect their protos
		return k_protosByType
			.Where(kvp => typeof(T).IsAssignableFrom(kvp.Key))
			.SelectMany(kvp => kvp.Value.OfType<T>());
	}

	/// <summary>
	/// Gets all prototypes of the exact type (no subtypes).
	/// </summary>
	/// <typeparam name="T">The exact prototype type.</typeparam>
	/// <returns>All prototypes of exactly this type.</returns>
	public IEnumerable<T> GetAllExact<T>() where T : Proto {
		if (k_protosByType.TryGetValue(typeof(T), out var typeSet)) {
			return typeSet.Cast<T>();
		}
		return Enumerable.Empty<T>();
	}

	/// <summary>
	/// Gets all prototypes as a flat enumerable.
	/// </summary>
	/// <returns>All registered prototypes.</returns>
	public IEnumerable<Proto> GetAllProtos() {
		return k_protoById.Values;
	}

	/// <summary>
	/// Gets all prototype IDs.
	/// </summary>
	/// <returns>All registered prototype IDs.</returns>
	public IEnumerable<Proto.ID> GetAllIds() {
		return k_protoById.Keys;
	}

	/// <summary>
	/// Gets all prototype IDs of a specific type.
	/// </summary>
	/// <typeparam name="T">The prototype type.</typeparam>
	/// <returns>All IDs of matching prototypes.</returns>
	public IEnumerable<Proto.ID> GetAllIds<T>() where T : Proto {
		return GetAll<T>().Select(p => p.Id);
	}

	#endregion

	#region Query & Filter

	/// <summary>
	/// Filters prototypes based on a predicate.
	/// </summary>
	/// <typeparam name="T">The prototype type.</typeparam>
	/// <param name="predicate">The filter predicate.</param>
	/// <returns>All prototypes matching the predicate.</returns>
	public IEnumerable<T> Filter<T>(Func<T, bool> predicate) where T : Proto {
		return GetAll<T>().Where(predicate);
	}

	/// <summary>
	/// Filters prototypes based on a predicate.
	/// Alias for Filter for backward compatibility.
	/// </summary>
	public IEnumerable<T> FilterProtos<T>(Func<T, bool> predicate) where T : Proto {
		return Filter(predicate);
	}

	/// <summary>
	/// Finds the first prototype matching a predicate.
	/// </summary>
	/// <typeparam name="T">The prototype type.</typeparam>
	/// <param name="predicate">The filter predicate.</param>
	/// <returns>The first matching prototype or null.</returns>
	public T FindFirst<T>(Func<T, bool> predicate) where T : Proto {
		return GetAll<T>().FirstOrDefault(predicate);
	}

	/// <summary>
	/// Finds all prototypes matching multiple criteria.
	/// </summary>
	/// <typeparam name="T">The prototype type.</typeparam>
	/// <param name="predicates">The filter predicates (all must match).</param>
	/// <returns>All prototypes matching all predicates.</returns>
	public IEnumerable<T> FindAll<T>(params Func<T, bool>[] predicates) where T : Proto {
		var query = GetAll<T>();
		foreach (var predicate in predicates) {
			query = query.Where(predicate);
		}
		return query;
	}

	/// <summary>
	/// Checks if any prototype matches the predicate.
	/// </summary>
	/// <typeparam name="T">The prototype type.</typeparam>
	/// <param name="predicate">The filter predicate.</param>
	/// <returns>True if any prototype matches.</returns>
	public bool Any<T>(Func<T, bool> predicate) where T : Proto {
		return GetAll<T>().Any(predicate);
	}

	/// <summary>
	/// Counts prototypes of a specific type matching an optional predicate.
	/// </summary>
	/// <typeparam name="T">The prototype type.</typeparam>
	/// <param name="predicate">The filter predicate (null = count all of type).</param>
	/// <returns>Number of matching prototypes.</returns>
	public int CountOf<T>(Func<T, bool> predicate = null) where T : Proto {
		var query = GetAll<T>();
		return predicate != null ? query.Count(predicate) : query.Count();
	}

	#endregion

	#region Existence Checks

	/// <summary>
	/// Checks if a prototype with the given ID exists.
	/// </summary>
	/// <param name="id">The prototype ID.</param>
	/// <returns>True if the prototype exists.</returns>
	public bool Contains(Proto.ID id) {
		return k_protoById.ContainsKey(id);
	}

	/// <summary>
	/// Checks if a prototype with the given ID exists.
	/// Alias for Contains for backward compatibility.
	/// </summary>
	public bool ContainsProto(Proto.ID id) {
		return Contains(id);
	}

	/// <summary>
	/// Checks if a prototype with the given ID exists and is of type T.
	/// </summary>
	/// <typeparam name="T">The expected prototype type.</typeparam>
	/// <param name="id">The prototype ID.</param>
	/// <returns>True if the prototype exists and is of type T.</returns>
	public bool Contains<T>(Proto.ID id) where T : Proto {
		return k_protoById.TryGetValue(id, out var proto) && proto is T;
	}

	/// <summary>
	/// Checks if any prototypes of the given type are registered.
	/// </summary>
	/// <typeparam name="T">The prototype type.</typeparam>
	/// <returns>True if any prototypes of this type exist.</returns>
	public bool HasAny<T>() where T : Proto {
		return GetAll<T>().Any();
	}

	#endregion

	#region Grouping & Organization

	/// <summary>
	/// Groups prototypes by their runtime type.
	/// </summary>
	/// <returns>Dictionary of type -> prototypes.</returns>
	public Dictionary<Type, IEnumerable<Proto>> GroupByType() {
		return k_protoById.Values
			.GroupBy(proto => proto.GetType())
			.ToDictionary(g => g.Key, g => g.AsEnumerable());
	}

	/// <summary>
	/// Groups prototypes by their runtime type.
	/// Alias for GroupByType for backward compatibility.
	/// </summary>
	public Dictionary<Type, IEnumerable<Proto>> GroupProtosByType() {
		return GroupByType();
	}

	/// <summary>
	/// Groups prototypes by a custom key selector.
	/// </summary>
	/// <typeparam name="T">The prototype type.</typeparam>
	/// <typeparam name="TKey">The grouping key type.</typeparam>
	/// <param name="keySelector">Function to extract the grouping key.</param>
	/// <returns>Dictionary of key -> prototypes.</returns>
	public Dictionary<TKey, List<T>> GroupBy<T, TKey>(Func<T, TKey> keySelector) where T : Proto {
		return GetAll<T>()
			.GroupBy(keySelector)
			.ToDictionary(g => g.Key, g => g.ToList());
	}

	/// <summary>
	/// Creates a lookup dictionary by a custom key selector.
	/// </summary>
	/// <typeparam name="T">The prototype type.</typeparam>
	/// <typeparam name="TKey">The key type.</typeparam>
	/// <param name="keySelector">Function to extract the key.</param>
	/// <returns>Dictionary for fast lookup by key.</returns>
	public Dictionary<TKey, T> ToLookup<T, TKey>(Func<T, TKey> keySelector) where T : Proto {
		return GetAll<T>().ToDictionary(keySelector);
	}

	#endregion

	#region Removal (Pre-Lock Only)

	/// <summary>
	/// Attempts to remove a prototype by ID.
	/// Only allowed before the database is locked.
	/// </summary>
	/// <param name="id">The prototype ID.</param>
	/// <returns>The removed prototype, or null if not found.</returns>
	/// <exception cref="InvalidOperationException">If database is locked.</exception>
	public Proto TryRemove(Proto.ID id) {
		if (k_isLocked) {
			throw new InvalidOperationException(
				$"Cannot remove proto '{id}': GameDb is locked. " +
				"Modifications are only allowed before LockAndInitialize().");
		}

		if (!k_protoById.TryGetValue(id, out Proto proto)) {
			return null;
		}

		k_protoById.Remove(id);
		k_totalCount--;

		// Remove from type grouping
		Type protoType = proto.GetType();
		if (k_protosByType.TryGetValue(protoType, out var typeSet)) {
			typeSet.Remove(proto);
			if (typeSet.Count == 0) {
				k_protosByType.Remove(protoType);
			}
		}

		return proto;
	}

	/// <summary>
	/// Removes a prototype by ID. Throws if not found.
	/// Only allowed before the database is locked.
	/// </summary>
	/// <param name="id">The prototype ID.</param>
	/// <returns>The removed prototype.</returns>
	/// <exception cref="KeyNotFoundException">If the ID is not found.</exception>
	public Proto Remove(Proto.ID id) {
		var proto = TryRemove(id);
		if (proto == null) {
			throw new KeyNotFoundException($"Cannot remove: Proto '{id}' not found in GameDb.");
		}
		return proto;
	}

	#endregion

	#region Random Selection

	/// <summary>
	/// Gets a random prototype of the specified type.
	/// </summary>
	/// <typeparam name="T">The prototype type.</typeparam>
	/// <returns>A random prototype, or null if none exist.</returns>
	public T GetRandom<T>() where T : Proto {
		var all = GetAll<T>().ToList();
		if (all.Count == 0) return null;
		return all[UnityEngine.Random.Range(0, all.Count)];
	}

	/// <summary>
	/// Gets random prototypes of the specified type.
	/// </summary>
	/// <typeparam name="T">The prototype type.</typeparam>
	/// <param name="count">Number of random items to get.</param>
	/// <param name="allowDuplicates">Whether to allow the same item multiple times.</param>
	/// <returns>Random prototypes.</returns>
	public List<T> GetRandom<T>(int count, bool allowDuplicates = false) where T : Proto {
		var all = GetAll<T>().ToList();
		if (all.Count == 0) return [];

		var result = new List<T>(count);

		if (allowDuplicates) {
			for (int i = 0; i < count; i++) {
				result.Add(all[UnityEngine.Random.Range(0, all.Count)]);
			}
		} else {
			// Fisher-Yates shuffle for unique selection
			var shuffled = new List<T>(all);
			int n = shuffled.Count;
			for (int i = 0; i < Math.Min(count, n); i++) {
				int j = UnityEngine.Random.Range(i, n);
				(shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
				result.Add(shuffled[i]);
			}
		}

		return result;
	}

	/// <summary>
	/// Gets a weighted random prototype.
	/// </summary>
	/// <typeparam name="T">The prototype type.</typeparam>
	/// <param name="weightSelector">Function to get weight for each prototype.</param>
	/// <returns>A weighted random prototype.</returns>
	public T GetWeightedRandom<T>(Func<T, float> weightSelector) where T : Proto {
		var all = GetAll<T>().ToList();
		if (all.Count == 0) return null;

		float totalWeight = all.Sum(weightSelector);
		float roll = UnityEngine.Random.Range(0f, totalWeight);
		float cumulative = 0;

		foreach (var proto in all) {
			cumulative += weightSelector(proto);
			if (roll <= cumulative) {
				return proto;
			}
		}

		return all[^1]; // Fallback to last item
	}

	#endregion

	#region Debug & Diagnostics

	/// <summary>
	/// Gets a summary of the database contents.
	/// </summary>
	/// <returns>A formatted summary string.</returns>
	public string GetSummary() {
		var sb = new System.Text.StringBuilder();
		sb.AppendLine("═══════════════════════════════════════════");
		sb.AppendLine("              GAME DATABASE SUMMARY         ");
		sb.AppendLine("═══════════════════════════════════════════");
		sb.AppendLine($"Status: {(k_isLocked ? "LOCKED" : "UNLOCKED")}");
		sb.AppendLine($"Total Prototypes: {k_totalCount}");
		sb.AppendLine($"Distinct Types: {k_protosByType.Count}");
		sb.AppendLine();
		sb.AppendLine("─── Prototypes by Type ───");

		foreach (var (type, protos) in k_protosByType.OrderByDescending(kvp => kvp.Value.Count)) {
			sb.AppendLine($"  {type.Name}: {protos.Count}");
		}

		sb.AppendLine("═══════════════════════════════════════════");
		return sb.ToString();
	}

	/// <summary>
	/// Logs the database summary to the Unity console.
	/// </summary>
	public void LogSummary() {
		Debug.Log(GetSummary());
	}

	/// <summary>
	/// Validates the database integrity.
	/// </summary>
	/// <returns>List of validation errors (empty if valid).</returns>
	public List<string> Validate() {
		var errors = new List<string>();

		// Check for null IDs
		foreach (var proto in k_protoById.Values) {
			if (proto.Id == null || string.IsNullOrEmpty(proto.Id.Value)) {
				errors.Add($"Proto of type {proto.GetType().Name} has null or empty ID.");
			}
		}

		// Check type index consistency
		foreach (var (type, protos) in k_protosByType) {
			foreach (var proto in protos) {
				if (!k_protoById.ContainsKey(proto.Id)) {
					errors.Add($"Type index contains proto '{proto.Id}' not in main index.");
				}
			}
		}

		return errors;
	}

	#endregion
}
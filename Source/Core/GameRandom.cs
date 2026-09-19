using System;
using System.Runtime.CompilerServices;

namespace RPGGame.Core;

/// <summary>
/// Centralized deterministic random system for roguelike reproducibility.
/// 
/// <para>
/// Initialize once per run with a world seed. All randomness is derived deterministically
/// from the master seed + context identifiers via FNV-1a hashing.
/// </para>
/// 
/// <para>
/// <b>Key Principles:</b>
/// <list type="bullet">
/// <item>Same seed + same context = identical sequence every run</item>
/// <item>Different contexts are isolated (loot can't affect terrain)</item>
/// <item>Zero state to serialize - only save the master seed</item>
/// </list>
/// </para>
/// 
/// <example>
/// <code>
/// // Initialize once at run start
/// GameRandom.Initialize(worldSeed);
/// 
/// // Get isolated streams
/// var terrain = GameRandom.For("Terrain");
/// var loot = GameRandom.For("Loot");
/// var combat = GameRandom.For("Combat", encounterId);
/// 
/// // Use streams
/// float height = terrain.NextFloat();
/// if (loot.Check(0.1f)) { /* rare drop */ }
/// int damage = combat.Roll(HitDice.TwoD6);
/// </code>
/// </example>
/// </summary>
public static class GameRandom {
	#region Constants

	private const uint FNV_OFFSET = 2166136261u;
	private const uint FNV_PRIME = 16777619u;

	#endregion

	#region State

	private static uint _masterSeed;
	private static bool _initialized;

	#endregion

	#region Properties

	/// <summary>
	/// The master seed controlling all randomness.
	/// Only valid after <see cref="Initialize(uint)"/> is called.
	/// </summary>
	public static uint MasterSeed {
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get {
			ThrowIfNotInitialized();
			return _masterSeed;
		}
	}

	/// <summary>
	/// Returns true if <see cref="Initialize(uint)"/> has been called.
	/// </summary>
	public static bool IsInitialized {
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => _initialized;
	}

	#endregion

	#region Initialization

	/// <summary>
	/// Initializes the random system with a master seed.
	/// Call once at game/run start.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Initialize(uint seed) {
		_masterSeed = seed == 0 ? 1u : seed;
		_initialized = true;
	}

	/// <summary>
	/// Initializes with an integer seed.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Initialize(int seed) => Initialize((uint)seed);

	/// <summary>
	/// Resets the system for a new run.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Reset() {
		_initialized = false;
		_masterSeed = 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void ThrowIfNotInitialized() {
		if (!_initialized) {
			throw new InvalidOperationException(
				"GameRandom not initialized. Call GameRandom.Initialize(seed) at run start.");
		}
	}

	#endregion

	#region Stream Creation

	/// <summary>
	/// Creates a deterministic stream for a named context.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static RandomStream For(string context) {
		ThrowIfNotInitialized();
		return new RandomStream(HashString(context));
	}

	/// <summary>
	/// Creates a deterministic stream for a numeric context.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static RandomStream For(int contextId) {
		ThrowIfNotInitialized();
		return new RandomStream(HashCombine(_masterSeed, (uint)contextId));
	}

	/// <summary>
	/// Creates a deterministic stream for context + index.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static RandomStream For(string context, int index) {
		ThrowIfNotInitialized();
		return new RandomStream(HashCombine(HashString(context), (uint)index));
	}

	/// <summary>
	/// Creates a deterministic stream for context + 2D coordinates.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static RandomStream For(string context, int x, int y) {
		ThrowIfNotInitialized();
		uint hash = HashString(context);
		hash = HashCombine(hash, (uint)x);
		hash = HashCombine(hash, (uint)y);
		return new RandomStream(hash);
	}

	/// <summary>
	/// Creates a deterministic stream for context + 3D coordinates.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static RandomStream For(string context, int x, int y, int z) {
		ThrowIfNotInitialized();
		uint hash = HashString(context);
		hash = HashCombine(hash, (uint)x);
		hash = HashCombine(hash, (uint)y);
		hash = HashCombine(hash, (uint)z);
		return new RandomStream(hash);
	}

	/// <summary>
	/// Creates a deterministic stream from an object's type.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static RandomStream For(object obj) {
		ThrowIfNotInitialized();
		return new RandomStream(HashString(obj?.GetType().FullName ?? "null"));
	}

	/// <summary>
	/// Creates a deterministic stream from a Type.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static RandomStream For(Type type) {
		ThrowIfNotInitialized();
		return new RandomStream(HashString(type.FullName ?? type.Name));
	}

	/// <summary>
	/// Creates a stream from an explicit seed (bypasses master seed).
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static RandomStream FromSeed(uint seed) {
		return new RandomStream(seed);
	}

	/// <summary>
	/// Creates a stream from an explicit seed (bypasses master seed).
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static RandomStream FromSeed(int seed) {
		return new RandomStream((uint)seed);
	}

	#endregion

	#region Hashing

	/// <summary>
	/// Combines two hash values using FNV-1a.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint HashCombine(uint hash, uint value) {
		hash ^= value;
		hash *= FNV_PRIME;
		return hash;
	}

	/// <summary>
	/// Hashes a string using FNV-1a, combined with master seed.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint HashString(string? s) {
		uint h = _initialized ? _masterSeed : FNV_OFFSET;
		if (string.IsNullOrEmpty(s)) return h;

		for (int i = 0; i < s.Length; i++) {
			h ^= s[i];
			h *= FNV_PRIME;
		}
		return h == 0 ? 1u : h;
	}

	/// <summary>
	/// Creates a deterministic seed from multiple integers.
	/// </summary>
	public static uint CreateSeed(params int[] values) {
		uint h = FNV_OFFSET;
		foreach (int v in values) {
			h = HashCombine(h, (uint)v);
		}
		return h == 0 ? 1u : h;
	}

	#endregion
}
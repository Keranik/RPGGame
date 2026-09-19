using RPGGame.Core.Expedition;
using RPGGame.Core.Prototypes.Combat;

namespace RPGGame.Core.Prototypes.Locations;

/// <summary>
/// Prototype for dungeon locations - enterable areas with encounters and rewards.
/// </summary>
public class DungeonProto : MapLocationProto {
	new public ID Id { get; }

	new public readonly struct ID : IEquatable<ID>, IComparable<ID> {
		public readonly string Value;
		public ID(string value) => Value = value;

		public bool Equals(ID other) => Value == other.Value;
		public int CompareTo(ID other) => string.Compare(Value, other.Value, StringComparison.Ordinal);
		public override string ToString() => Value;
		public override bool Equals(object? obj) => obj is ID other && Equals(other);
		public override int GetHashCode() => Value?.GetHashCode() ?? 0;

		public static bool operator ==(ID lhs, ID rhs) => lhs.Equals(rhs);
		public static bool operator !=(ID lhs, ID rhs) => !lhs.Equals(rhs);

		public static implicit operator MapLocationProto.ID(ID id) => new MapLocationProto.ID(id.Value);
		public static implicit operator Proto.ID(ID id) => new Proto.ID(id.Value);
	}

	#region Properties

	public override LocationCategory Category => LocationCategory.Dungeon;

	/// <summary>Type of dungeon.</summary>
	public DungeonType DungeonType { get; init; }

	/// <summary>Difficulty tier (1-10).</summary>
	public int DifficultyTier { get; init; } = 1;

	/// <summary>Recommended player level.</summary>
	public int RecommendedLevel { get; init; } = 1;

	/// <summary>Number of floors/rooms.</summary>
	public int FloorCount { get; init; } = 3;

	/// <summary>Whether floors are procedurally generated.</summary>
	public bool IsProceduralLayout { get; init; } = true;

	/// <summary>Tilemap prefab for dungeon layout (if not procedural).</summary>
	public string? LayoutPrefab { get; init; }

	/// <summary>Possible encounters in this dungeon.</summary>
	public List<EncounterProto.ID> PossibleEncounters { get; init; } = [];

	/// <summary>Boss encounter at the end (if any).</summary>
	public EncounterProto.ID? BossEncounter { get; init; }

	/// <summary>Whether dungeon resets after completion.</summary>
	public bool Repeatable { get; init; } = true;

	/// <summary>Reset time in hours (if repeatable).</summary>
	public int ResetHours { get; init; } = 24;

	/// <summary>Guaranteed loot on first clear (uses shared LootEntry from Combat namespace).</summary>
	public List<LootEntry> FirstClearRewards { get; init; } = [];

	/// <summary>Loot table for repeat clears.</summary>
	public List<LootEntry> RepeatRewards { get; init; } = [];

	/// <summary>Experience bonus for clearing.</summary>
	public int ClearExperience { get; init; }

	/// <summary>Gold bonus for clearing.</summary>
	public int ClearGold { get; init; }

	/// <summary>Flag set when cleared (for progression).</summary>
	public string? ClearedFlag { get; init; }

	/// <summary>Environmental hazards in this dungeon.</summary>
	public List<DungeonHazard> Hazards { get; init; } = [];

	/// <summary>Special mechanics for this dungeon.</summary>
	public DungeonMechanics Mechanics { get; init; } = DungeonMechanics.None;

	#endregion

	#region Constructor

	public DungeonProto(ID id, Loc text, DungeonType type) : base(id, text) {
		Id = id;
		DungeonType = type;
	}

	public DungeonProto(ID id, string name, string description, DungeonType type)
		: this(id, CreateText(name, description), type) { }

	#endregion
}

/// <summary>
/// Types of dungeons.
/// </summary>
public enum DungeonType {
	// Natural
	Cave,
	CavernSystem,
	SpiderNest,
	WolfDen,
	BearCave,

	// Man-made
	Mine,
	Crypt,
	Catacomb,
	Temple,
	Tower,
	Fortress,
	Prison,
	Sewer,

	// Ruins
	AncientRuins,
	ForgottenLibrary,
	SunkenTemple,
	CursedManor,

	// Magical
	ArcaneNexus,
	ShadowRealm,
	DemonGate,
	TimeFracture,

	// Special
	BanditCamp,
	CultistLair,
	LichSanctum,
	FogHeart
}

/// <summary>
/// Environmental hazards in dungeons.
/// </summary>
public enum DungeonHazard {
	None,
	Darkness,
	Poison,
	Fire,
	Ice,
	Flooding,
	Collapse,
	Traps,
	Cursed,
	Temporal,
	Fog
}

/// <summary>
/// Special mechanics for dungeons.
/// </summary>
[Flags]
public enum DungeonMechanics {
	None = 0,
	NoEscape = 1 << 0,
	TimedRun = 1 << 1,
	NoHealing = 1 << 2,
	PermaDeath = 1 << 3,
	ScalingEnemies = 1 << 4,
	Stealth = 1 << 5,
	Puzzle = 1 << 6,
	MultiPath = 1 << 7,
	SecretBoss = 1 << 8,
	Phases = 1 << 9,
	Adds = 1 << 10,
	Scripted = 1 << 11
}

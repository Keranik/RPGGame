using RPGGame.Core.Expedition;
using RPGGame.Core.Prototypes.Combat;

namespace RPGGame.Core.Prototypes.Locations;

/// <summary>
/// Prototype for boss arena locations - significant combat encounters.
/// </summary>
public class BossArenaProto : MapLocationProto {
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

	public override LocationCategory Category => LocationCategory.BossArena;

	/// <summary>The boss encounter at this location.</summary>
	public EncounterProto.ID BossEncounterId { get; init; }

	/// <summary>Required level to challenge.</summary>
	public int RequiredLevel { get; init; } = 1;

	/// <summary>Whether boss is visible before engaging.</summary>
	public bool BossVisible { get; init; } = true;

	/// <summary>Whether arena can be left once entered.</summary>
	public bool CanRetreat { get; init; } = true;

	/// <summary>Pre-fight event/dialogue.</summary>
	public Events.EventProto.ID? PreFightEvent { get; init; }

	/// <summary>Post-fight event/dialogue.</summary>
	public Events.EventProto.ID? PostFightEvent { get; init; }

	/// <summary>Flag required to access this boss.</summary>
	public new string? RequiredFlag { get; init; }

	/// <summary>Flag set when boss is defeated.</summary>
	public string? DefeatedFlag { get; init; }

	/// <summary>Whether boss respawns.</summary>
	public bool BossRespawns { get; init; }

	/// <summary>Respawn time in hours (if respawns).</summary>
	public int RespawnHours { get; init; } = 168; // 1 week

	/// <summary>Cinematic/cutscene to play.</summary>
	public string? CinematicId { get; init; }

	/// <summary>Special arena mechanics.</summary>
	public ArenaMechanics Mechanics { get; init; } = ArenaMechanics.None;

	#endregion

	#region Constructor

	public BossArenaProto(ID id, Loc text, EncounterProto.ID bossEncounter) : base(id, text) {
		Id = id;
		BossEncounterId = bossEncounter;
	}

	public BossArenaProto(ID id, string name, string description, EncounterProto.ID bossEncounter)
		: this(id, CreateText(name, description), bossEncounter) { }

	#endregion
}

/// <summary>
/// Special mechanics for boss arenas.
/// </summary>
[Flags]
public enum ArenaMechanics {
	None = 0,
	Phases = 1 << 0,           // Multi-phase fight
	Adds = 1 << 1,             // Spawns additional enemies
	Environment = 1 << 2,       // Environmental hazards
	TimedDPS = 1 << 3,         // Enrage timer
	Scripted = 1 << 4,         // Scripted events during fight
	Positioning = 1 << 5,      // Position matters
	Interrupts = 1 << 6        // Must interrupt abilities
}
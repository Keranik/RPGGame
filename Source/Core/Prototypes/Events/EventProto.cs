using RPGGame.Core.Events;
using RPGGame.Core.Expedition;
using RPGGame.Core.Prototypes.Expedition;
using RPGGame.Core.Simulation;
using RPGGame.Core.Stats;

namespace RPGGame.Core.Prototypes.Events;

/// <summary>
/// Prototype for game event definitions.
/// </summary>
public class EventProto : Proto {
	new public readonly struct ID(string value) : IEquatable<ID>, IComparable<ID> {
		public readonly string Value = value;

		public bool Equals(ID other) => Value == other.Value;
		public int CompareTo(ID other) => string.Compare(Value, other.Value, StringComparison.Ordinal);
		public override string ToString() => Value;
		public override bool Equals(object obj) => obj is ID other && Equals(other);
		public override int GetHashCode() => Value?.GetHashCode() ?? 0;

		public static bool operator ==(Proto.ID lhs, ID rhs) => string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static bool operator ==(ID lhs, Proto.ID rhs) => string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static bool operator !=(Proto.ID lhs, ID rhs) => !string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static bool operator !=(ID lhs, Proto.ID rhs) => !string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static bool operator ==(ID lhs, ID rhs) => string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static bool operator !=(ID lhs, ID rhs) => !string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static implicit operator Proto.ID(ID id) => new Proto.ID(id.Value);
	}

	new public ID Id { get; }

	// ═══════════════════════════════════════════════════════════════
	// Display
	// ═══════════════════════════════════════════════════════════════

	/// <summary>Display title.</summary>
	public string Title { get; init; } = "";

	/// <summary>Main description/narrative text.</summary>
	public string Description { get; init; } = "";

	/// <summary>Optional flavor text or quote.</summary>
	public string? FlavorText { get; init; }

	/// <summary>Icon for this event.</summary>
	public string IconName { get; init; } = "icon_event";

	/// <summary>Background image for event screen.</summary>
	public string? BackgroundImage { get; init; }

	// ═══════════════════════════════════════════════════════════════
	// Classification
	// ═══════════════════════════════════════════════════════════════

	/// <summary>Type of event.</summary>
	public EventType Type { get; init; } = EventType.Choice;

	/// <summary>Rarity of this event.</summary>
	public EventRarity Rarity { get; init; } = EventRarity.Common;

	/// <summary>Tags for filtering and searching.</summary>
	public List<string> Tags { get; init; } = [];

	// ═══════════════════════════════════════════════════════════════
	// Spawn Rules
	// ═══════════════════════════════════════════════════════════════

	/// <summary>Conditions that must be met for this event to spawn.</summary>
	public List<EventCondition> SpawnConditions { get; init; } = [];

	/// <summary>Terrain types where this event can spawn. Empty = any terrain.</summary>
	public List<TerrainProto.ID> ValidTerrains { get; init; } = [];

	/// <summary>Minimum distance from village.</summary>
	public float MinDistance { get; init; }

	/// <summary>Maximum distance from village (-1 = no limit).</summary>
	public float MaxDistance { get; init; } = -1;

	/// <summary>Base spawn weight (modified by rarity).</summary>
	public float SpawnWeight { get; init; } = 10f;

	/// <summary>Whether this event can repeat.</summary>
	public bool IsRepeatable { get; init; } = true;

	/// <summary>Minimum runs between repeats.</summary>
	public int RepeatCooldown { get; init; }

	// ═══════════════════════════════════════════════════════════════
	// Behavior
	// ═══════════════════════════════════════════════════════════════

	/// <summary>Whether this event pauses travel.</summary>
	public bool PausesTravel { get; init; } = true;

	/// <summary>Whether player can leave without choosing.</summary>
	public bool CanSkip { get; init; } = true;

	/// <summary>Whether to auto-resolve if only one choice.</summary>
	public bool AutoResolve { get; init; }

	/// <summary>Time limit to make a choice (0 = no limit).</summary>
	public float TimeLimit { get; init; }

	// ═══════════════════════════════════════════════════════════════
	// Content
	// ═══════════════════════════════════════════════════════════════

	/// <summary>Available choices for the player.</summary>
	public List<EventChoiceProto> Choices { get; init; } = [];

	/// <summary>Effects applied when event starts (before choices).</summary>
	public List<OutcomeEffect> OnStartEffects { get; init; } = [];

	/// <summary>Effects applied when event ends (after choice).</summary>
	public List<OutcomeEffect> OnEndEffects { get; init; } = [];

	// ═══════════════════════════════════════════════════════════════
	// Audio/Visual
	// ═══════════════════════════════════════════════════════════════

	/// <summary>Music to play during event.</summary>
	public string? MusicId { get; init; }

	/// <summary>Ambient sound to play.</summary>
	public string? AmbientId { get; init; }

	/// <summary>Sound effect on event start.</summary>
	public string? StartSfxId { get; init; }

	// ═══════════════════════════════════════════════════════════════
	// Constructor
	// ═══════════════════════════════════════════════════════════════

	public EventProto(ID id) : base(id, Loc.Empty) {
		Id = id;
	}

	// ═══════════════════════════════════════════════════════════════
	// Methods
	// ═══════════════════════════════════════════════════════════════

	/// <summary>Gets the effective spawn weight.</summary>
	public float GetEffectiveWeight() {
		return SpawnWeight * Type.GetBaseWeight() * Rarity.GetWeightMultiplier();
	}

	/// <summary>Checks if this event can spawn in the current context.</summary>
	public bool CanSpawn(RunState runState, MetaProgression meta, TerrainProto.ID terrain, float distance) {
		if (ValidTerrains.Count > 0 && !ValidTerrains.Contains(terrain)) {
			return false;
		}

		if (distance < MinDistance) {
			return false;
		}

		if (MaxDistance >= 0 && distance > MaxDistance) {
			return false;
		}

		if (!IsRepeatable && runState.EncounteredEvents.Contains(Id.Value)) {
			return false;
		}

		foreach (var condition in SpawnConditions) {
			if (!condition.Evaluate(runState, meta).Passed) {
				return false;
			}
		}

		return true;
	}
}



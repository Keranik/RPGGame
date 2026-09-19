using RPGGame.Core.Combat;
using RPGGame.Core.Expedition;
using RPGGame.Core.Prototypes.Expedition;
using RPGGame.Core.Prototypes.Item;

namespace RPGGame.Core.Prototypes.Combat;

/// <summary>
/// Prototype for enemy definitions.
/// </summary>
public class EnemyProto : Proto {
	new public readonly struct ID(string value) : IEquatable<ID>, IComparable<ID> {
		public readonly string Value = value;

		public bool Equals(ID other) => Value == other.Value;
		public int CompareTo(ID other) => string.Compare(Value, other.Value, StringComparison.Ordinal);
		public override string ToString() => Value;
		public override bool Equals(object? obj) => obj is ID other && Equals(other);
		public override int GetHashCode() => Value?.GetHashCode() ?? 0;

		public static bool operator ==(Proto.ID lhs, ID rhs) => string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static bool operator ==(ID lhs, Proto.ID rhs) => string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static bool operator !=(Proto.ID lhs, ID rhs) => !string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static bool operator !=(ID lhs, Proto.ID rhs) => !string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static bool operator ==(ID lhs, ID rhs) => string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static bool operator !=(ID lhs, ID rhs) => !string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static implicit operator Proto.ID(ID enemyId) => new Proto.ID(enemyId.Value);
	}

	new public ID Id { get; }

	// ═══════════════════════════════════════════════════════════════
	// Basic Info
	// ═══════════════════════════════════════════════════════════════
	public string Name { get; init; } = "Unknown";
	public string Description { get; init; } = "";
	public string PortraitName { get; init; } = "enemy_default";
	public EnemyCategory Category { get; init; } = EnemyCategory.Beast;
	public bool IsBoss { get; init; }

	// ═══════════════════════════════════════════════════════════════
	// Combat Stats
	// ═══════════════════════════════════════════════════════════════
	public int MaxHealth { get; init; } = 10;
	public int ArmorClass { get; init; } = 10;
	public int AttackBonus { get; init; }
	public int DamageBonus { get; init; }
	public int InitiativeBonus { get; init; }
	public HitDice AttackDamage { get; init; } = HitDice.D6;
	public DamageType DamageType { get; init; } = DamageType.Physical;

	// ═══════════════════════════════════════════════════════════════
	// Rewards
	// ═══════════════════════════════════════════════════════════════
	public int ExperienceValue { get; init; } = 10;
	public int GoldValue { get; init; }
	public List<LootEntry> LootTable { get; init; } = [];

	// ═══════════════════════════════════════════════════════════════
	// AI & Abilities
	// ═══════════════════════════════════════════════════════════════
	public EnemyAIType AIType { get; init; } = EnemyAIType.Aggressive;
	public List<EnemyAbility> Abilities { get; init; } = [];

	// ═══════════════════════════════════════════════════════════════
	// Resistances & Immunities
	// ═══════════════════════════════════════════════════════════════
	public Dictionary<DamageType, float> Resistances { get; init; } = [];
	public Dictionary<DamageType, float> Vulnerabilities { get; init; } = [];
	public List<StatusCondition> Immunities { get; init; } = [];

	// ═══════════════════════════════════════════════════════════════
	// Spawning
	// ═══════════════════════════════════════════════════════════════
	public float ChallengeRating { get; init; } = 1f;
	public float SpawnWeight { get; init; } = 10f;
	public float MinSpawnDistance { get; init; }
	public bool NightOnly { get; init; }
	public List<TerrainProto.ID> ValidTerrains { get; init; } = [];

	/// <summary>
	/// Creates a new enemy prototype using object initializer syntax.
	/// </summary>
	public EnemyProto(ID id) : base(id, Loc.Empty) {
		Id = id;
	}
}

// NOTE: LootEntry has been moved to Core/Prototypes/Combat/LootEntry.cs

/// <summary>
/// An ability that an enemy can use in combat.
/// </summary>
public class EnemyAbility {
	public string Id { get; init; } = "";
	public string Name { get; init; } = "";
	public string Description { get; init; } = "";
	public EnemyAbilityType Type { get; init; } = EnemyAbilityType.Attack;
	public TargetType TargetType { get; init; } = TargetType.SingleEnemy;
	public int ManaCost { get; init; }
	public int Cooldown { get; init; }
	public HitDice DamageDice { get; init; }
	public DamageType DamageType { get; init; } = DamageType.Physical;
	public List<AbilityEffect> Effects { get; init; } = [];
}
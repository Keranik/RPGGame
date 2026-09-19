using RPGGame.Core.Combat;
using RPGGame.Core.Prototypes.Characters;
using RPGGame.Core.Prototypes.Stats;
using RPGGame.Core.Spells;

namespace RPGGame.Core.Prototypes.Spells;

/// <summary>
/// Prototype for spell definitions.
/// </summary>
public class SpellProto : Proto {
	#region Strongly-Typed ID

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
		public static implicit operator Proto.ID(ID id) => new(id.Value);
	}

	new public ID Id => new(base.Id.Value);

	#endregion

	#region Properties

	public string IconName { get; }
	public SpellSchool School { get; }
	public int Level { get; }
	public int ManaCost { get; }
	public int HealthCost { get; }
	public int Cooldown { get; }
	public int RequiredLevel { get; }
	public TargetType TargetType { get; }
	public int Range { get; }
	public int AreaRadius { get; }
	public HitDice DamageDice { get; }
	public DamageType DamageType { get; }
	public HitDice HealingDice { get; }
	public StatusCondition? AppliesCondition { get; }
	public Duration ConditionDuration { get; }

	/// <summary>The attribute used for saving throws against this spell.</summary>
	public StatProto.ID? SavingThrow { get; }

	public bool UsableOutOfCombat { get; }
	public bool RequiresConcentration { get; }
	public List<CharacterClassProto.ID> ClassRestrictions { get; }

	public bool IsCantrip => Level == 0;

	#endregion

	#region Constructor

	public SpellProto(
		ID id,
		Loc text,
		string iconName = "icon_spell",
		SpellSchool school = SpellSchool.None,
		int level = 1,
		int manaCost = 5,
		int healthCost = 0,
		int cooldown = 0,
		int requiredLevel = 1,
		TargetType targetType = TargetType.SingleEnemy,
		int range = 30,
		int areaRadius = 0,
		HitDice damageDice = default,
		DamageType damageType = DamageType.Arcane,
		HitDice healingDice = default,
		StatusCondition? appliesCondition = null,
		Duration? conditionDuration = null,
		StatProto.ID? savingThrow = null,
		bool usableOutOfCombat = false,
		bool requiresConcentration = false,
		List<CharacterClassProto.ID>? classRestrictions = null
	) : base(id, text) {
		IconName = iconName;
		School = school;
		Level = level;
		ManaCost = manaCost;
		HealthCost = healthCost;
		Cooldown = cooldown;
		RequiredLevel = requiredLevel;
		TargetType = targetType;
		Range = range;
		AreaRadius = areaRadius;
		DamageDice = damageDice;
		DamageType = damageType;
		HealingDice = healingDice;
		AppliesCondition = appliesCondition;
		ConditionDuration = conditionDuration ?? Duration.Infinite;
		SavingThrow = savingThrow;
		UsableOutOfCombat = usableOutOfCombat;
		RequiresConcentration = requiresConcentration;
		ClassRestrictions = classRestrictions ?? [];
	}

	#endregion

	#region Methods

	public int GetEffectiveManaCost() => IsCantrip ? 0 : ManaCost;

	#endregion
}
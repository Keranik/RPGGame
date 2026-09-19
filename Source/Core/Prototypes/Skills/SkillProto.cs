using RPGGame.Core.Effects;
using RPGGame.Core.Generation;
using RPGGame.Core.Prototypes.Stats;
using RPGGame.Core.Stats;

namespace RPGGame.Core.Prototypes.Skills;

/// <summary>
/// Prototype for skill definitions.
/// Skills are trainable abilities that provide passive bonuses or active effects.
/// Categories are defined via Tags using TagProto.ID.
/// </summary>
public class SkillProto : Proto {
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

	#region Core Properties

	/// <summary>Icon for UI display.</summary>
	public string IconName { get; init; } = "icon_skill";

	/// <summary>
	/// Tags for categorization, filtering, and bonuses.
	/// Use Ids.Tags.* for categories (Combat, Stealth, Crafting, etc.)
	/// </summary>
	public List<TagProto.ID> Tags { get; init; } = [];

	/// <summary>Maximum rank this skill can reach.</summary>
	public int MaxRank { get; init; } = 10;

	/// <summary>Base creation point cost to learn this skill (rank 0 → 1).</summary>
	public int BaseCost { get; init; } = 5;

	/// <summary>Cost multiplier per additional rank.</summary>
	public float RankCostMultiplier { get; init; } = 1.0f;

	#endregion

	#region Requirements

	/// <summary>Minimum character level required to learn this skill.</summary>
	public int RequiredLevel { get; init; } = 1;

	/// <summary>Minimum attribute value required to learn this skill.</summary>
	public int RequiredAttributeValue { get; init; } = 0;

	/// <summary>Other skills required before learning this one.</summary>
	public List<ID> PrerequisiteSkills { get; init; } = [];

	/// <summary>Minimum rank in prerequisite skills.</summary>
	public int PrerequisiteRank { get; init; } = 1;

	/// <summary>
	/// Tags that must be unlocked to access this skill.
	/// E.g., must have Ids.Tags.School.Evocation unlocked to learn evocation skills.
	/// </summary>
	public List<TagProto.ID> RequiredTags { get; init; } = [];

	#endregion

	#region Effects

	/// <summary>Whether this skill is passive (always active) or active (must be used).</summary>
	public bool IsPassive { get; init; } = true;

	/// <summary>
	/// Modifiers granted per rank of this skill.
	/// Key = Proto.ID of what's being modified (StatProto.ID or SkillProto.ID)
	/// Value = ValueModifier applied per rank.
	/// 
	/// Examples:
	///   { Ids.Stats.Combat.AttackBonus, 1.Flat() }     // +1 Attack per rank
	///   { Ids.Stats.Combat.CriticalChance, 2.Flat() }  // +2% Crit per rank
	///   { Ids.Skills.Stealth.Hiding, 5.Flat() }        // +5 Hiding per rank
	/// </summary>
	public Dictionary<Proto.ID, ValueModifier> ModifiersPerRank { get; init; } = [];

	/// <summary>Abilities granted at specific ranks. Key = rank, Value = ability ID.</summary>
	public Dictionary<int, Proto.ID> GrantedAbilities { get; init; } = [];

	/// <summary>Effects applied while this skill is active (for active skills).</summary>
	public List<EffectProto.ID> ActiveEffects { get; init; } = [];

	/// <summary>Cooldown in combat rounds (for active skills).</summary>
	public int Cooldown { get; init; } = 0;

	/// <summary>Stamina cost to use (for active skills).</summary>
	public int StaminaCost { get; init; } = 0;

	/// <summary>Mana cost to use (for active skills).</summary>
	public int ManaCost { get; init; } = 0;

	#endregion

	#region Flavor

	/// <summary>Whether this skill is hidden until discovered.</summary>
	public bool IsHidden { get; init; } = false;

	/// <summary>Whether this is a class-exclusive skill.</summary>
	public bool IsClassExclusive { get; init; } = false;

	/// <summary>Classes that have natural access to this skill (empty = all).</summary>
	public List<Characters.CharacterClassProto.ID> ClassAccess { get; init; } = [];

	#endregion

	#region Constructor

	public SkillProto(
		ID id,
		Loc text,
		string iconName = "icon_skill",
		int maxRank = 10,
		int baseCost = 5,
		float rankCostMultiplier = 1.0f,
		int requiredLevel = 1,
		bool isPassive = true
	) : base(id, text) {
		IconName = iconName;
		MaxRank = maxRank;
		BaseCost = baseCost;
		RankCostMultiplier = rankCostMultiplier;
		RequiredLevel = requiredLevel;
		IsPassive = isPassive;
	}

	#endregion

	#region Methods

	/// <summary>
	/// Gets the creation point cost to increase this skill from one rank to the next.
	/// </summary>
	public int GetRankCost(int fromRank) {
		if (fromRank >= MaxRank) return int.MaxValue;

		int cost = fromRank == 0 ? BaseCost : (int)(BaseCost * 0.6f);

		// Apply rank multiplier for higher ranks
		if (fromRank >= 7) {
			cost = (int)(cost * CreationPointCosts.SKILL_GRANDMASTERY_MULTIPLIER);
		} else if (fromRank >= 4) {
			cost = (int)(cost * CreationPointCosts.SKILL_MASTERY_MULTIPLIER);
		}

		return (int)(cost * RankCostMultiplier * CreationPointCosts.BaseScale);
	}

	/// <summary>
	/// Gets the total creation point cost to reach a specific rank from 0.
	/// </summary>
	public int GetTotalCostToRank(int targetRank) {
		int total = 0;
		for (int r = 0; r < targetRank; r++) {
			total += GetRankCost(r);
		}
		return total;
	}

	/// <summary>
	/// Gets the modifiers for a specific rank, scaled by rank.
	/// </summary>
	public Dictionary<Proto.ID, ValueModifier> GetModifiersAtRank(int rank) {
		var result = new Dictionary<Proto.ID, ValueModifier>();

		foreach (var (target, mod) in ModifiersPerRank) {
			float scaledValue = mod.Value * rank;

			var scaledMod = mod.Operation switch {
				ModifierOperation.FlatAdd => ValueModifier.FlatAdd(scaledValue),
				ModifierOperation.FlatSubtract => ValueModifier.FlatSubtract(scaledValue),
				ModifierOperation.PercentIncrease => ValueModifier.PercentIncrease(scaledValue),
				ModifierOperation.PercentReduce => ValueModifier.PercentReduce(scaledValue),
				ModifierOperation.PercentMore => ValueModifier.PercentMore(scaledValue),
				ModifierOperation.PercentLess => ValueModifier.PercentLess(scaledValue),
				_ => ValueModifier.FlatAdd(scaledValue)
			};

			result[target] = scaledMod;
		}

		return result;
	}

	/// <summary>
	/// Gets only stat modifiers at a rank.
	/// </summary>
	public IEnumerable<KeyValuePair<Proto.ID, ValueModifier>> GetStatModifiersAtRank(int rank) {
		return GetModifiersAtRank(rank).Where(m => m.Key.Value.StartsWith("Stat_"));
	}

	/// <summary>
	/// Gets only skill modifiers at a rank.
	/// </summary>
	public IEnumerable<KeyValuePair<Proto.ID, ValueModifier>> GetSkillModifiersAtRank(int rank) {
		return GetModifiersAtRank(rank).Where(m => m.Key.Value.StartsWith("Skill_"));
	}

	/// <summary>
	/// Checks if this skill has a specific tag.
	/// </summary>
	public bool HasTag(TagProto.ID tagId) {
		return Tags.Any(t => t == tagId);
	}

	/// <summary>
	/// Checks if this skill has any of the specified tags.
	/// </summary>
	public bool HasAnyTag(params TagProto.ID[] tagIds) {
		return Tags.Any(t => tagIds.Any(id => id == t));
	}

	/// <summary>
	/// Checks if this skill has all of the specified tags.
	/// </summary>
	public bool HasAllTags(params TagProto.ID[] tagIds) {
		return tagIds.All(id => Tags.Any(t => t == id));
	}

	#endregion
}
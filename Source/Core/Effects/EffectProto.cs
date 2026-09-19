using RPGGame.Core.Combat;
using RPGGame.Core.Generation;
using RPGGame.Core.Prototypes;
using RPGGame.Core.Prototypes.Stats;
using RPGGame.Core.Stats;

namespace RPGGame.Core.Effects;

/// <summary>
/// Prototype for an effect that can be applied to entities.
/// Effects are templates - instances track duration and stacking.
/// </summary>
public class EffectProto : Proto {
	/// <summary>Effect-specific ID.</summary>
	new public ID Id { get; }

	/// <summary>Effect-specific ID struct.</summary>
	public new readonly struct ID : IEquatable<ID>, IComparable<ID> {
		public readonly string Value;

		public ID(string value) => Value = value;

		public bool Equals(ID other) => Value == other.Value;
		public int CompareTo(ID other) => string.Compare(Value, other.Value, StringComparison.Ordinal);
		public override string ToString() => Value;
		public override bool Equals(object? obj) => obj is ID other && Equals(other);
		public override int GetHashCode() => Value?.GetHashCode() ?? 0;

		public static bool operator ==(ID lhs, ID rhs) => lhs.Equals(rhs);
		public static bool operator !=(ID lhs, ID rhs) => !lhs.Equals(rhs);

		public static implicit operator Proto.ID(ID id) => new Proto.ID(id.Value);
	}

	#region Properties

	/// <summary>Icon for UI display.</summary>
	public string IconName { get; init; } = "";
	public bool IsBuff { get; init; }
	public bool CanBeDispelled { get; init; } = true;
	public bool IsHidden { get; init; }
	public Duration DefaultDuration { get; init; } = Duration.Infinite;
	public int MaxStacks { get; init; } = 1;

	/// <summary>Whether stacks refresh duration or are independent.</summary>
	public bool StacksRefreshDuration { get; init; } = true;

	/// <summary>Stat modifiers applied by this effect.</summary>
	public List<EffectStatModifier> StatModifiers { get; init; } = [];

	/// <summary>Status conditions applied by this effect.</summary>
	public List<StatusCondition> AppliedConditions { get; init; } = [];

	/// <summary>Damage per turn (for DoT effects).</summary>
	public HitDice? DamagePerTurn { get; init; }

	/// <summary>Damage type for DoT effects.</summary>
	public DamageType? DamageType { get; init; }

	/// <summary>Healing per turn (for HoT effects).</summary>
	public HitDice? HealingPerTurn { get; init; }

	/// <summary>Tags that affect how this effect interacts with others.</summary>
	public HashSet<TagProto.ID> Tags { get; init; } = [];

	/// <summary>Effect that this replaces/upgrades (for stacking rules).</summary>
	public ID? ReplacesEffect { get; init; }

	/// <summary>Priority for resolution order (higher = resolves first).</summary>
	public int Priority { get; init; }

	#endregion

	#region Tag Helpers

	/// <summary>Checks if this effect has a specific tag.</summary>
	public bool HasTag(TagProto.ID tagId) => Tags.Contains(tagId);

	/// <summary>Checks if this is a fog-related effect.</summary>
	public bool IsFogEffect =>
		HasTag(Ids.Tags.Fog.Lingering) ||
		HasTag(Ids.Tags.Fog.Hazy) ||
		HasTag(Ids.Tags.Fog.Dense) ||
		HasTag(Ids.Tags.Fog.Thick) ||
		HasTag(Ids.Tags.Fog.Suffocating);

	/// <summary>Checks if this effect is magical.</summary>
	public bool IsMagical => HasTag(Ids.Tags.Combat.Magical);

	/// <summary>Checks if this effect is from the environment.</summary>
	public bool IsEnvironmental => HasTag(Ids.Tags.Source.Environment);

	/// <summary>Checks if this effect is a curse.</summary>
	public bool IsCurse => HasTag(Ids.Tags.Item.Cursed) || HasTag(Ids.Tags.Condition.Cursed);

	/// <summary>Checks if this is a DoT effect.</summary>
	public bool IsDoT => HasTag(Ids.Tags.Meta.DoT) || DamagePerTurn.HasValue;

	/// <summary>Checks if this is a HoT effect.</summary>
	public bool IsHoT => HasTag(Ids.Tags.Meta.HoT) || HealingPerTurn.HasValue;

	/// <summary>Checks if this is a control effect.</summary>
	public bool IsControl => HasTag(Ids.Tags.Meta.Control);

	/// <summary>Checks if this effect is divine in nature.</summary>
	public bool IsDivine => HasTag(Ids.Tags.School.Divine);

	/// <summary>Checks if this effect is nature-based.</summary>
	public bool IsNatural => HasTag(Ids.Tags.Element.Nature);

	/// <summary>Checks if this effect is nocturnal.</summary>
	public bool IsNocturnal => HasTag(Ids.Tags.TimeOfDay.Nocturnal);

	#endregion

	#region Constructor

	public EffectProto(ID id, Loc text) : base(id, text) {
		Id = id;
	}

	public EffectProto(ID id, string name, string description)
		: this(id, Proto.CreateText(name, description)) { }

	#endregion
}

/// <summary>
/// A stat modification applied by an effect using self-describing ValueModifier.
/// </summary>
public readonly struct EffectStatModifier {
	/// <summary>Stat being modified.</summary>
	public StatProto.ID Stat { get; }

	/// <summary>The modification to apply.</summary>
	public ValueModifier Modifier { get; }

	/// <summary>Whether this modifier scales with effect stacks/intensity.</summary>
	public bool ScalesWithIntensity { get; }

	private EffectStatModifier(StatProto.ID stat, ValueModifier modifier, bool scales) {
		Stat = stat;
		Modifier = modifier;
		ScalesWithIntensity = scales;
	}

	public static EffectStatModifier Create(StatProto.ID stat, ValueModifier modifier, bool scales = false) =>
		new(stat, modifier, scales);

	public static EffectStatModifier Flat(StatProto.ID stat, int value, bool scales = false) =>
		new(stat, value.Flat(), scales);

	public static EffectStatModifier Penalty(StatProto.ID stat, int value, bool scales = false) =>
		new(stat, value.FlatSubtract(), scales);

	public static EffectStatModifier PercentIncrease(StatProto.ID stat, float percent, bool scales = false) =>
		new(stat, percent.PercentIncrease(), scales);

	public static EffectStatModifier PercentReduce(StatProto.ID stat, float percent, bool scales = false) =>
		new(stat, percent.PercentReduce(), scales);

	public static EffectStatModifier PercentMore(StatProto.ID stat, float percent, bool scales = false) =>
		new(stat, percent.PercentMore(), scales);

	public static EffectStatModifier PercentLess(StatProto.ID stat, float percent, bool scales = false) =>
		new(stat, percent.PercentLess(), scales);

	public override string ToString() => $"{Stat}: {Modifier}{(ScalesWithIntensity ? " (scales)" : "")}";
}
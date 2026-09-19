using RPGGame.Core.Combat;
using RPGGame.Core.Prototypes.Stats;
using RPGGame.Core.Stats;

namespace RPGGame.Core.Prototypes.Combat;

/// <summary>
/// An effect that an ability can apply.
/// This is a value object, not a Proto - it doesn't need an ID.
/// </summary>
public class AbilityEffect {
	public StatusCondition? AppliesCondition { get; init; }
	public Duration ConditionDuration { get; init; }
	public string BuffId { get; init; } = "";
	public int HealAmount { get; init; }
	public int DamageAmount { get; init; }
	public DamageType DamageType { get; init; } = DamageType.Physical;

	/// <summary>
	/// Stat to modify (e.g., for buffs/debuffs).
	/// </summary>
	public StatProto.ID? ModifiesStat { get; init; }
	public float StatModifierValue { get; init; }
	public ModifierOperation ModifierOperation { get; init; } = ModifierOperation.FlatAdd;
}

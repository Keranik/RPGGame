using RPGGame.Core.Combat;
using RPGGame.Core.Prototypes.Stats;
using RPGGame.Core.Stats;

namespace RPGGame.Core.Spells;

/// <summary>
/// Static data definition for a spell.
/// </summary>
public class SpellData {
	#region Identity

	/// <summary>
	/// Unique identifier.
	/// </summary>
	public string Id { get; init; } = "";

	/// <summary>
	/// Display name.
	/// </summary>
	public string Name { get; init; } = "";

	/// <summary>
	/// Description of the spell.
	/// </summary>
	public string Description { get; init; } = "";

	/// <summary>
	/// Icon name.
	/// </summary>
	public string IconName { get; init; } = "icon_spell";

	/// <summary>
	/// School of magic.
	/// </summary>
	public SpellSchool School { get; init; } = SpellSchool.None;

	/// <summary>
	/// Spell level (0 = cantrip, 1-9 = spell levels).
	/// </summary>
	public int Level { get; init; } = 1;

	#endregion

	#region Costs

	/// <summary>
	/// Mana cost to cast.
	/// </summary>
	public int ManaCost { get; init; } = 5;

	/// <summary>
	/// Health cost (for blood magic).
	/// </summary>
	public int HealthCost { get; init; } = 0;

	/// <summary>
	/// Cooldown in combat turns.
	/// </summary>
	public int Cooldown { get; init; } = 0;

	/// <summary>
	/// Required character level to learn.
	/// </summary>
	public int RequiredLevel { get; init; } = 1;

	/// <summary>
	/// Required class to learn.
	/// </summary>
	public List<string> ClassRestrictions { get; init; } = [];

	#endregion

	#region Targeting

	/// <summary>
	/// Target type.
	/// </summary>
	public SpellTargetType TargetType { get; init; } = SpellTargetType.SingleEnemy;

	/// <summary>
	/// Range in feet (0 = touch, -1 = self).
	/// </summary>
	public int Range { get; init; } = 30;

	/// <summary>
	/// Area of effect radius (0 = single target).
	/// </summary>
	public int AreaRadius { get; init; } = 0;

	#endregion

	#region Effects

	/// <summary>
	/// Damage dice (if any).
	/// </summary>
	public string? DamageDice { get; init; }

	/// <summary>
	/// Damage type.
	/// </summary>
	public DamageType DamageType { get; init; } = DamageType.Arcane;

	/// <summary>
	/// Healing amount (if any).
	/// </summary>
	public string? HealingDice { get; init; }

	/// <summary>
	/// Status effect to apply.
	/// </summary>
	public StatusCondition? AppliesCondition { get; init; }

	/// <summary>
	/// Duration of applied effect in turns.
	/// </summary>
	public int ConditionDuration { get; init; } = 0;

	/// <summary>
	/// Buff ID to apply.
	/// </summary>
	public string? AppliesBuff { get; init; }

	/// <summary>
	/// Buff duration in turns.
	/// </summary>
	public int BuffDuration { get; init; } = 0;

	/// <summary>
	/// Stat modifiers while buff is active.
	/// </summary>
	public List<SpellStatModifier> StatModifiers { get; init; } = [];

	/// <summary>
	/// Saving throw stat (if any).
	/// </summary>
	public StatProto.ID? SavingThrow { get; init; }

	/// <summary>
	/// DC for saving throw (0 = use caster's spell DC).
	/// </summary>
	public int SavingThrowDC { get; init; } = 0;

	/// <summary>
	/// Effect on successful save (half damage, no effect, etc.).
	/// </summary>
	public SaveEffect SaveEffect { get; init; } = SaveEffect.HalfDamage;

	#endregion

	#region Scaling

	/// <summary>
	/// Additional damage dice per caster level.
	/// </summary>
	public string? DamagePerLevel { get; init; }

	/// <summary>
	/// Additional healing per caster level.
	/// </summary>
	public string? HealingPerLevel { get; init; }

	/// <summary>
	/// Whether this spell scales with spell power.
	/// </summary>
	public bool ScalesWithSpellPower { get; init; } = true;

	#endregion

	#region Audio/Visual

	/// <summary>
	/// Sound effect on cast.
	/// </summary>
	public string? CastSfxId { get; init; }

	/// <summary>
	/// Sound effect on hit.
	/// </summary>
	public string? HitSfxId { get; init; }

	/// <summary>
	/// Visual effect prefab.
	/// </summary>
	public string? VfxPrefab { get; init; }

	#endregion

	#region Flags

	/// <summary>
	/// Whether this is a cantrip (no mana cost, always available).
	/// </summary>
	public bool IsCantrip => Level == 0;

	/// <summary>
	/// Whether this spell can be used outside combat.
	/// </summary>
	public bool UsableOutOfCombat { get; init; } = false;

	/// <summary>
	/// Whether this spell requires concentration.
	/// </summary>
	public bool RequiresConcentration { get; init; } = false;

	/// <summary>
	/// Whether this spell is a ritual (can be cast without using a spell slot).
	/// </summary>
	public bool IsRitual { get; init; } = false;

	#endregion

	#region Methods

	/// <summary>
	/// Gets the effective mana cost (cantrips are free).
	/// </summary>
	public int GetEffectiveManaCost() => IsCantrip ? 0 : ManaCost;

	/// <summary>
	/// Gets a formatted description including mechanics.
	/// </summary>
	public string GetFullDescription() {
		var parts = new List<string> { Description };

		if (!string.IsNullOrEmpty(DamageDice)) {
			parts.Add($"Damage: {DamageDice} {DamageType}");
		}

		if (!string.IsNullOrEmpty(HealingDice)) {
			parts.Add($"Healing: {HealingDice}");
		}

		if (AppliesCondition.HasValue) {
			parts.Add($"Applies: {AppliesCondition.Value} for {ConditionDuration} turns");
		}

		if (SavingThrow.HasValue) {
			// Extract stat name from ID instead of calling GetName()
			string statName = ExtractStatName(SavingThrow.Value.Value);
			parts.Add($"Save: {statName} ({SaveEffect})");
		}

		return string.Join("\n", parts);
	}

	private static string ExtractStatName(string idValue) {
		var parts = idValue.Split('_');
		return parts.Length >= 2 ? parts[^1] : idValue;
	}

	#endregion
}

#region Supporting Types

/// <summary>
/// A stat modifier applied by a spell.
/// </summary>
public class SpellStatModifier {
	public StatProto.ID Stat { get; init; }
	public float Value { get; init; }
	public ModifierOperation Operation { get; init; } = ModifierOperation.FlatAdd;
}

/// <summary>
/// Effect of a successful saving throw.
/// </summary>
public enum SaveEffect {
	/// <summary>No effect on save.</summary>
	Negates,
	/// <summary>Half damage on save.</summary>
	HalfDamage,
	/// <summary>Half duration on save.</summary>
	HalfDuration,
	/// <summary>Partial effect on save.</summary>
	Partial
}

#endregion
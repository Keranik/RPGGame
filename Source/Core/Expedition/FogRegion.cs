using RPGGame.Core.Effects;
using RPGGame.Core.Generation;
using RPGGame.Core.Prototypes.Stats;
using RPGGame.Core.Stats;

namespace RPGGame.Core.Expedition;

/// <summary>
/// Represents a region of THE fog (not visibility fog of war).
/// Tracks fog strength which affects enemy power and provides meta-progression.
/// </summary>
public class FogRegion {
	#region Properties

	/// <summary>Unique index for this region (0 = closest to village).</summary>
	public int RegionIndex { get; }

	/// <summary>Display name for this region.</summary>
	public string Name { get; set; }

	/// <summary>Starting tile distance for this region.</summary>
	public int TileStart { get; }

	/// <summary>Ending tile distance for this region.</summary>
	public int TileEnd { get; }

	/// <summary>Base fog strength (1.0 = full strength, 0.0 = cleared).</summary>
	public float BaseStrength { get; }

	/// <summary>Current fog strength after weakening from combat.</summary>
	public float CurrentStrength { get; private set; }

	/// <summary>Current fog tier based on strength.</summary>
	public FogTier CurrentTier => GetTierForStrength(CurrentStrength);

	/// <summary>Number of enemies defeated in this region (meta-tracked).</summary>
	public int EnemiesDefeated { get; private set; }

	/// <summary>Total combat power defeated (sum of enemy CR/difficulty).</summary>
	public int CombatPowerDefeated { get; private set; }

	/// <summary>Fog strength required to weaken by 1%.</summary>
	public int PowerToWeakenOnePercent { get; set; } = 100;

	#endregion

	#region Events

	/// <summary>Fired when fog strength changes.</summary>
	public event Action<float, float>? OnStrengthChanged;

	/// <summary>Fired when fog tier changes.</summary>
	public event Action<FogTier, FogTier>? OnTierChanged;

	#endregion

	#region Constructor

	public FogRegion(int regionIndex, int tileStart, int tileEnd, float baseStrength = 1.0f) {
		RegionIndex = regionIndex;
		TileStart = tileStart;
		TileEnd = tileEnd;
		BaseStrength = baseStrength;
		CurrentStrength = baseStrength;
		Name = $"Region {regionIndex + 1}";
	}

	#endregion

	#region Fog Management

	/// <summary>Records a combat victory, weakening the fog.</summary>
	public void RecordCombatVictory(int enemyCount, int totalCombatPower) {
		EnemiesDefeated += enemyCount;
		CombatPowerDefeated += totalCombatPower;

		float oldStrength = CurrentStrength;
		FogTier oldTier = CurrentTier;

		float weakenAmount = (float)totalCombatPower / PowerToWeakenOnePercent / 100f;
		CurrentStrength = Math.Max(0f, CurrentStrength - weakenAmount);

		if (Math.Abs(oldStrength - CurrentStrength) > 0.001f) {
			OnStrengthChanged?.Invoke(oldStrength, CurrentStrength);
		}

		FogTier newTier = CurrentTier;
		if (oldTier != newTier) {
			OnTierChanged?.Invoke(oldTier, newTier);
		}
	}

	/// <summary>Sets the current strength directly (for loading saves).</summary>
	public void SetStrength(float strength) {
		CurrentStrength = Math.Clamp(strength, 0f, BaseStrength);
	}

	/// <summary>Checks if a tile distance falls within this region.</summary>
	public bool ContainsTile(float distanceFromVillage) {
		return distanceFromVillage >= TileStart && distanceFromVillage < TileEnd;
	}

	#endregion

	#region Tier Calculations

	/// <summary>Gets the tier for a given fog strength.</summary>
	public static FogTier GetTierForStrength(float strength) {
		return strength switch {
			<= 0.20f => FogTier.Lingering,
			<= 0.40f => FogTier.Hazy,
			<= 0.60f => FogTier.Dense,
			<= 0.80f => FogTier.Thick,
			_ => FogTier.Suffocating
		};
	}

	/// <summary>Gets the effects for the current fog tier.</summary>
	public FogTierEffects GetCurrentEffects() {
		return CreateEffectsForTier(CurrentTier, CurrentStrength);
	}

	/// <summary>Gets the entity tag for the current fog tier.</summary>
	public TagProto.ID GetCurrentFogTag() {
		return CurrentTier switch {
			FogTier.Lingering => Ids.Tags.Fog.Lingering,
			FogTier.Hazy => Ids.Tags.Fog.Hazy,
			FogTier.Dense => Ids.Tags.Fog.Dense,
			FogTier.Thick => Ids.Tags.Fog.Thick,
			FogTier.Suffocating => Ids.Tags.Fog.Suffocating,
			_ => Ids.Tags.Fog.Lingering
		};
	}

	/// <summary>Gets the effect ID for the current fog tier.</summary>
	public EffectProto.ID GetCurrentFogEffect() {
		return CurrentTier switch {
			FogTier.Lingering => Ids.Effects.Fog.Lingering,
			FogTier.Hazy => Ids.Effects.Fog.Hazy,
			FogTier.Dense => Ids.Effects.Fog.Dense,
			FogTier.Thick => Ids.Effects.Fog.Thick,
			FogTier.Suffocating => Ids.Effects.Fog.Suffocating,
			_ => Ids.Effects.Fog.Lingering
		};
	}

	private static FogTierEffects CreateEffectsForTier(FogTier tier, float strength) {
		// Calculate linear scaling within tier (0-1 range within this tier)
		float tierStrength = tier switch {
			FogTier.Lingering => strength / 0.20f,
			FogTier.Hazy => (strength - 0.20f) / 0.20f,
			FogTier.Dense => (strength - 0.40f) / 0.20f,
			FogTier.Thick => (strength - 0.60f) / 0.20f,
			FogTier.Suffocating => (strength - 0.80f) / 0.20f,
			_ => 0f
		};
		tierStrength = Math.Clamp(tierStrength, 0f, 1f);

		return tier switch {
			FogTier.Lingering => new FogTierEffects {
				Tier = tier,
				Strength = strength,
				TierStrength = tierStrength,
				Description = "The fog is nearly gone, but enemies fight desperately.",
				FlavorText = "The mist clings to the shadows, unwilling to fade completely.",
				EnemyEffectIds = [Ids.Effects.Buffs.LastStand],
				EnemyTagId = Ids.Tags.Fog.Lingering,
				EnemyStatModifiers = [
					// 10-25% damage reduction when low HP, scaling with tier strength
					FogStatModifier.Enemy(
						Ids.Stats.Resistances.Physical,
						(10f + (15f * tierStrength)).PercentIncrease(),
						FogModifierCondition.WhenLowHealth
					)
				]
			},

			FogTier.Hazy => new FogTierEffects {
				Tier = tier,
				Strength = strength,
				TierStrength = tierStrength,
				Description = "A haze obscures enemies, making them harder to hit.",
				FlavorText = "Shapes shimmer and blur at the edges of your vision.",
				EnemyEffectIds = [Ids.Effects.Buffs.Blur],
				EnemyTagId = Ids.Tags.Fog.Hazy,
				EnemyStatModifiers = [
					// +1 to +3 AC, scaling with tier strength
					FogStatModifier.Enemy(
						Ids.Stats.Combat.ArmorClass,
						(1f + (2f * tierStrength)).Flat()
					)
				]
			},

			FogTier.Dense => new FogTierEffects {
				Tier = tier,
				Strength = strength,
				TierStrength = tierStrength,
				Description = "Dense fog impairs your movement and reactions.",
				FlavorText = "Every step feels sluggish, as if moving through water.",
				PlayerEffectIds = [Ids.Effects.Debuffs.Slow],
				EnemyTagId = Ids.Tags.Fog.Dense,
				PlayerStatModifiers = [
					// -1 to -3 DEX, scaling with tier strength
					FogStatModifier.Player(
						Ids.Stats.Attributes.Dexterity,
						(1f + (2f * tierStrength)).FlatSubtract()
					)
				]
			},

			FogTier.Thick => new FogTierEffects {
				Tier = tier,
				Strength = strength,
				TierStrength = tierStrength,
				Description = "Thick fog empowers enemies while hampering your abilities.",
				FlavorText = "The fog pulses with malevolent energy.",
				EnemyTagId = Ids.Tags.Fog.Thick,
				// Enemy effects 25-50% more powerful
				EnemyEffectMultiplier = (25f + (25f * tierStrength)).PercentMore(),
				// Player effects 25-50% less powerful
				PlayerEffectMultiplier = (25f + (25f * tierStrength)).PercentLess()
			},

			FogTier.Suffocating => new FogTierEffects {
				Tier = tier,
				Strength = strength,
				TierStrength = tierStrength,
				Description = "The fog is overwhelming. Combat is treacherous.",
				FlavorText = "You can barely breathe. Each action is a struggle.",
				EnemyEffectIds = [Ids.Effects.Special.Advantage],
				PlayerEffectIds = [Ids.Effects.Debuffs.Disadvantage],
				EnemyTagId = Ids.Tags.Fog.Suffocating,
				PlayerHasDisadvantage = true,
				EnemiesHaveAdvantage = true,
				PlayerStatModifiers = [
					FogStatModifier.Player(Ids.Stats.Attributes.Dexterity, 2.FlatSubtract()),
					FogStatModifier.Player(Ids.Stats.Expedition.VisionRange, 4.FlatSubtract())
				],
				EnemyStatModifiers = [
					// +2 to +5 initiative, scaling with tier strength
					FogStatModifier.Enemy(
						Ids.Stats.Combat.Initiative,
						(2f + (3f * tierStrength)).Flat()
					)
				]
			},

			_ => new FogTierEffects { Tier = tier, Strength = strength }
		};
	}

	#endregion

	#region Queries

	/// <summary>Gets display text for current fog state.</summary>
	public string GetDisplayText() {
		return $"{Name}: {CurrentTier.GetDisplayName()} ({CurrentStrength * 100:F0}%)";
	}

	#endregion

	#region Serialization

	public FogRegionData ToData() {
		return new FogRegionData {
			RegionIndex = RegionIndex,
			Name = Name,
			TileStart = TileStart,
			TileEnd = TileEnd,
			BaseStrength = BaseStrength,
			CurrentStrength = CurrentStrength,
			EnemiesDefeated = EnemiesDefeated,
			CombatPowerDefeated = CombatPowerDefeated,
			PowerToWeakenOnePercent = PowerToWeakenOnePercent
		};
	}

	public static FogRegion FromData(FogRegionData data) {
		return new FogRegion(data.RegionIndex, data.TileStart, data.TileEnd, data.BaseStrength) {
			Name = data.Name,
			CurrentStrength = data.CurrentStrength,
			EnemiesDefeated = data.EnemiesDefeated,
			CombatPowerDefeated = data.CombatPowerDefeated,
			PowerToWeakenOnePercent = data.PowerToWeakenOnePercent
		};
	}

	#endregion
}

#region Fog Tier System

/// <summary>
/// Fog density tiers that determine enemy abilities and player penalties.
/// </summary>
public enum FogTier {
	/// <summary>0-20% fog - Barely there, enemies cling to remaining power.</summary>
	Lingering,

	/// <summary>21-40% fog - Hazy, enemies gain evasion.</summary>
	Hazy,

	/// <summary>41-60% fog - Dense, player suffers penalties.</summary>
	Dense,

	/// <summary>61-80% fog - Thick, effects are amplified/diminished.</summary>
	Thick,

	/// <summary>81-100% fog - Suffocating, severe combat penalties.</summary>
	Suffocating
}

/// <summary>
/// Effects applied by a fog tier using strongly-typed IDs.
/// </summary>
public class FogTierEffects {
	/// <summary>Current tier.</summary>
	public FogTier Tier { get; init; }

	/// <summary>Exact fog strength (0-1).</summary>
	public float Strength { get; init; }

	/// <summary>Strength within the current tier (0-1).</summary>
	public float TierStrength { get; init; }

	/// <summary>Description of the fog's effects.</summary>
	public string Description { get; init; } = "";

	/// <summary>Flavor text for atmosphere.</summary>
	public string FlavorText { get; init; } = "";

	// Enemy modifications
	/// <summary>Effect IDs applied to enemies.</summary>
	public List<EffectProto.ID> EnemyEffectIds { get; init; } = [];

	/// <summary>Tag applied to enemies from this fog tier.</summary>
	public TagProto.ID? EnemyTagId { get; init; }

	/// <summary>Stat modifiers applied to enemies.</summary>
	public List<FogStatModifier> EnemyStatModifiers { get; init; } = [];

	// Player modifications
	/// <summary>Effect IDs applied to player.</summary>
	public List<EffectProto.ID> PlayerEffectIds { get; init; } = [];

	/// <summary>Stat modifiers applied to player.</summary>
	public List<FogStatModifier> PlayerStatModifiers { get; init; } = [];

	// Effect power multipliers
	/// <summary>Multiplier for enemy effect duration/power.</summary>
	public ValueModifier EnemyEffectMultiplier { get; init; } = ValueModifier.None;

	/// <summary>Multiplier for player effect duration/power.</summary>
	public ValueModifier PlayerEffectMultiplier { get; init; } = ValueModifier.None;

	// Combat modifiers
	/// <summary>Whether player has disadvantage on attacks.</summary>
	public bool PlayerHasDisadvantage { get; init; }

	/// <summary>Whether enemies have advantage on attacks.</summary>
	public bool EnemiesHaveAdvantage { get; init; }
}

/// <summary>
/// A stat modification from fog effects with self-describing values.
/// </summary>
public class FogStatModifier {
	public StatProto.ID Stat { get; }
	public ValueModifier Modifier { get; }
	public bool AppliesToPlayer { get; }
	public bool AppliesToEnemies { get; }
	public FogModifierCondition Condition { get; }

	private FogStatModifier(StatProto.ID stat, ValueModifier modifier, bool player, bool enemy, FogModifierCondition condition) {
		Stat = stat;
		Modifier = modifier;
		AppliesToPlayer = player;
		AppliesToEnemies = enemy;
		Condition = condition;
	}

	public static FogStatModifier Player(StatProto.ID stat, ValueModifier modifier, FogModifierCondition condition = FogModifierCondition.Always) {
		return new FogStatModifier(stat, modifier, true, false, condition);
	}

	public static FogStatModifier Enemy(StatProto.ID stat, ValueModifier modifier, FogModifierCondition condition = FogModifierCondition.Always) {
		return new FogStatModifier(stat, modifier, false, true, condition);
	}

	public static FogStatModifier Both(StatProto.ID stat, ValueModifier modifier, FogModifierCondition condition = FogModifierCondition.Always) {
		return new FogStatModifier(stat, modifier, true, true, condition);
	}
}

/// <summary>
/// Target for fog stat modifiers.
/// </summary>
public enum FogModifierTarget {
	/// <summary>Applies to player.</summary>
	Player,

	/// <summary>Applies to enemies.</summary>
	Enemy
}

/// <summary>
/// Condition for when a fog modifier applies.
/// </summary>
public enum FogModifierCondition {
	/// <summary>Always applies.</summary>
	Always,

	/// <summary>Applies when health is below 25%.</summary>
	WhenLowHealth,
	WhenHighHealth,

	/// <summary>Applies when in combat.</summary>
	WhenInCombat,

	/// <summary>Applies when out of combat.</summary>
	WhenOutOfCombat,

	/// <summary>Applies at night.</summary>
	AtNight,

	/// <summary>Applies during the day.</summary>
	DuringDay
}

public static class FogTierExtensions {
	/// <summary>Gets display name for a tier.</summary>
	public static string GetDisplayName(this FogTier tier) {
		return tier switch {
			FogTier.Lingering => "Lingering Fog",
			FogTier.Hazy => "Hazy Fog",
			FogTier.Dense => "Dense Fog",
			FogTier.Thick => "Thick Fog",
			FogTier.Suffocating => "Suffocating Fog",
			_ => tier.ToString()
		};
	}

	/// <summary>Gets icon for a tier.</summary>
	public static string GetIcon(this FogTier tier) {
		return tier switch {
			FogTier.Lingering => "🌫️",
			FogTier.Hazy => "🌁",
			FogTier.Dense => "☁️",
			FogTier.Thick => "🌑",
			FogTier.Suffocating => "💀",
			_ => "🌫️"
		};
	}
}

#endregion

#region Serialization Data

public class FogRegionData {
	public int RegionIndex { get; set; }
	public string Name { get; set; } = "";
	public int TileStart { get; set; }
	public int TileEnd { get; set; }
	public float BaseStrength { get; set; }
	public float CurrentStrength { get; set; }
	public int EnemiesDefeated { get; set; }
	public int CombatPowerDefeated { get; set; }
	public int PowerToWeakenOnePercent { get; set; }
}

#endregion
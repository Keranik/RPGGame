using RPGGame.Core.Combat;
using RPGGame.Core.Prototypes;

namespace RPGGame.Core.Effects;

/// <summary>
/// Defines all effects using the Proto system.
/// </summary>
public class EffectDefinitions : ICoreData {
	public void GameData(GameDb gameDatabase) {
		RegisterBuffEffects(gameDatabase);
		RegisterDebuffEffects(gameDatabase);
		RegisterDoTEffects(gameDatabase);
		RegisterControlEffects(gameDatabase);
		RegisterFogEffects(gameDatabase);
		RegisterSpecialEffects(gameDatabase);
	}

	#region Buff Effects

	private void RegisterBuffEffects(GameDb db) {
		db.RegisterProto(new EffectProto(Ids.Effects.Buffs.Blur, "Blur", "Your form shimmers, making you harder to hit.") {
			IconName = "icon_effect_blur",
			IsBuff = true,
			DefaultDuration = 3.Hours(),
			StatModifiers = [
				EffectStatModifier.Flat(Ids.Stats.Combat.ArmorClass, 2)
			],
			Tags = [Ids.Tags.Combat.Magical]
		});

		db.RegisterProto(new EffectProto(Ids.Effects.Buffs.LastStand, "Last Stand", "When near death, you gain damage reduction.") {
			IconName = "icon_effect_last_stand",
			IsBuff = true,
			DefaultDuration = Duration.Infinite,
			StatModifiers = [
				EffectStatModifier.PercentReduce(Ids.Stats.Resistances.Physical, 25, scales: true)
			],
			Tags = [Ids.Tags.Physical.Endurance]
		});

		db.RegisterProto(new EffectProto(Ids.Effects.Buffs.Empowerment, "Empowered", "Your abilities are enhanced.") {
			IconName = "icon_effect_empower",
			IsBuff = true,
			DefaultDuration = 5.Hours(),
			StatModifiers = [
				EffectStatModifier.Flat(Ids.Stats.Combat.DamBonus, 2),
				EffectStatModifier.Flat(Ids.Stats.Combat.AttackBonus, 1)
			],
			Tags = [Ids.Tags.Combat.Magical, Ids.Tags.Meta.Buff]
		});

		db.RegisterProto(new EffectProto(Ids.Effects.Buffs.Haste, "Haste", "You move with supernatural speed.") {
			IconName = "icon_effect_haste",
			IsBuff = true,
			DefaultDuration = 3.Hours(),
			StatModifiers = [
				EffectStatModifier.PercentIncrease(Ids.Stats.Movement.MovementSpeed, 50),
				EffectStatModifier.Flat(Ids.Stats.Combat.Initiative, 3),
				EffectStatModifier.Flat(Ids.Stats.Combat.ArmorClass, 1)
			],
			Tags = [Ids.Tags.Combat.Magical]
		});

		db.RegisterProto(new EffectProto(Ids.Effects.Buffs.Shield, "Shielded", "A protective barrier absorbs damage.") {
			IconName = "icon_effect_shield",
			IsBuff = true,
			DefaultDuration = 3.Hours(),
			AppliedConditions = [StatusCondition.Shielded],
			Tags = [Ids.Tags.Combat.Magical]
		});

		db.RegisterProto(new EffectProto(Ids.Effects.Buffs.Regeneration, "Regenerating", "Your wounds heal over time.") {
			IconName = "icon_effect_regen",
			IsBuff = true,
			DefaultDuration = 5.Hours(),
			HealingPerTurn = HitDice.D4,
			AppliedConditions = [StatusCondition.Regenerating],
			Tags = [Ids.Tags.Combat.Magical, Ids.Tags.Survival.General]
		});

		db.RegisterProto(new EffectProto(Ids.Effects.Buffs.Blessed, "Blessed", "Divine favor guides your actions.") {
			IconName = "icon_effect_blessed",
			IsBuff = true,
			DefaultDuration = 10.Hours(),
			StatModifiers = [
				EffectStatModifier.Flat(Ids.Stats.Combat.AttackBonus, 1),
				EffectStatModifier.PercentIncrease(Ids.Stats.Combat.SpellPower, 20)
			],
			Tags = [Ids.Tags.School.Divine]
		});

		db.RegisterProto(new EffectProto(Ids.Effects.Buffs.Camouflage, "Camouflaged", "You blend into your surroundings.") {
			IconName = "icon_effect_camo",
			IsBuff = true,
			DefaultDuration = 3.Hours(),
			StatModifiers = [
				EffectStatModifier.PercentIncrease(Ids.Stats.Combat.DodgeChance, 50),
				EffectStatModifier.Flat(Ids.Stats.Combat.ArmorClass, 2)
			],
			Tags = [Ids.Tags.Stealth.Hiding]
		});

		db.RegisterProto(new EffectProto(Ids.Effects.Buffs.DarkVision, "Dark Vision", "You see perfectly in darkness.") {
			IconName = "icon_effect_darkvision",
			IsBuff = true,
			DefaultDuration = Duration.Infinite,
			StatModifiers = [
				EffectStatModifier.PercentIncrease(Ids.Stats.Expedition.VisionRange, 100)
			],
			Tags = [Ids.Tags.Combat.Magical, Ids.Tags.TimeOfDay.Nocturnal]
		});

		db.RegisterProto(new EffectProto(Ids.Effects.Buffs.ThermalSight, "Thermal Sight", "You detect heat signatures through obstacles.") {
			IconName = "icon_effect_thermal",
			IsBuff = true,
			DefaultDuration = Duration.Infinite,
			StatModifiers = [
				EffectStatModifier.PercentIncrease(Ids.Stats.Expedition.VisionRange, 50)
			],
			Tags = [Ids.Tags.Combat.Magical]
		});

		db.RegisterProto(new EffectProto(Ids.Effects.Buffs.Precision, "Precision", "Your strikes find their mark with deadly accuracy.") {
			IconName = "icon_effect_precision",
			IsBuff = true,
			DefaultDuration = 3.Hours(),
			StatModifiers = [
				EffectStatModifier.PercentIncrease(Ids.Stats.Combat.CriticalChance, 15),
				EffectStatModifier.PercentMore(Ids.Stats.Combat.CriticalDamage, 25)
			],
			Tags = [Ids.Tags.Combat.Offense, Ids.Tags.Meta.Buff]
		});

		db.RegisterProto(new EffectProto(Ids.Effects.Buffs.Resistance, "Resistance", "You resist incoming damage.") {
			IconName = "icon_effect_resistance",
			IsBuff = true,
			DefaultDuration = 5.Hours(),
			StatModifiers = [
				EffectStatModifier.PercentIncrease(Ids.Stats.Resistances.Physical, 20),
				EffectStatModifier.PercentIncrease(Ids.Stats.Resistances.Magical, 20)
			],
			Tags = [Ids.Tags.Combat.Magical]
		});
	}

	#endregion

	#region Debuff Effects

	private void RegisterDebuffEffects(GameDb db) {
		db.RegisterProto(new EffectProto(Ids.Effects.Debuffs.Slow, "Slowed", "Your movement is impaired.") {
			IconName = "icon_effect_slow",
			IsBuff = false,
			DefaultDuration = 3.Hours(),
			StatModifiers = [
				EffectStatModifier.PercentReduce(Ids.Stats.Movement.MovementSpeed, 50),
				EffectStatModifier.Penalty(Ids.Stats.Combat.Initiative, 2)
			],
			AppliedConditions = [StatusCondition.Slowed],
			Tags = [Ids.Tags.Combat.Magical]
		});

		db.RegisterProto(new EffectProto(Ids.Effects.Debuffs.Weakness, "Weakened", "Your strength is sapped.") {
			IconName = "icon_effect_weak",
			IsBuff = false,
			DefaultDuration = 3.Hours(),
			StatModifiers = [
				EffectStatModifier.Penalty(Ids.Stats.Combat.DamBonus, 2),
				EffectStatModifier.Penalty(Ids.Stats.Combat.AttackBonus, 2)
			],
			AppliedConditions = [StatusCondition.Weakened],
			Tags = [Ids.Tags.Item.Cursed]
		});

		db.RegisterProto(new EffectProto(Ids.Effects.Debuffs.Blind, "Blinded", "You cannot see properly.") {
			IconName = "icon_effect_blind",
			IsBuff = false,
			DefaultDuration = 2.Hours(),
			StatModifiers = [
				EffectStatModifier.Penalty(Ids.Stats.Combat.AttackBonus, 4),
				EffectStatModifier.PercentReduce(Ids.Stats.Expedition.VisionRange, 75)
			],
			AppliedConditions = [StatusCondition.Blinded],
			Tags = [Ids.Tags.Condition.Blinded]
		});

		db.RegisterProto(new EffectProto(Ids.Effects.Debuffs.Vulnerability, "Vulnerable", "You take increased damage.") {
			IconName = "icon_effect_vulnerable",
			IsBuff = false,
			DefaultDuration = 3.Hours(),
			StatModifiers = [
				EffectStatModifier.Penalty(Ids.Stats.Combat.ArmorClass, 2),
				EffectStatModifier.PercentReduce(Ids.Stats.Resistances.Physical, 25)
			],
			Tags = [Ids.Tags.Item.Cursed]
		});

		db.RegisterProto(new EffectProto(Ids.Effects.Debuffs.Fear, "Frightened", "Terror grips your heart.") {
			IconName = "icon_effect_fear",
			IsBuff = false,
			DefaultDuration = 2.Hours(),
			StatModifiers = [
				EffectStatModifier.Penalty(Ids.Stats.Combat.AttackBonus, 2)
			],
			AppliedConditions = [StatusCondition.Frightened],
			Tags = [Ids.Tags.Combat.Magical, Ids.Tags.Condition.Frightened]
		});

		db.RegisterProto(new EffectProto(Ids.Effects.Debuffs.Silence, "Silenced", "You cannot cast spells.") {
			IconName = "icon_effect_silence",
			IsBuff = false,
			DefaultDuration = 2.Hours(),
			AppliedConditions = [StatusCondition.Silenced],
			Tags = [Ids.Tags.Combat.Magical, Ids.Tags.Condition.Silenced]
		});

		db.RegisterProto(new EffectProto(Ids.Effects.Debuffs.Curse, "Cursed", "A dark curse weakens you.") {
			IconName = "icon_effect_curse",
			IsBuff = false,
			DefaultDuration = 10.Hours(),
			CanBeDispelled = false,
			StatModifiers = [
				EffectStatModifier.Penalty(Ids.Stats.Attributes.Strength, 1),
				EffectStatModifier.Penalty(Ids.Stats.Attributes.Constitution, 1),
				EffectStatModifier.PercentReduce(Ids.Stats.Economy.ExperienceGain, 10)
			],
			Tags = [Ids.Tags.Item.Cursed, Ids.Tags.Combat.Magical, Ids.Tags.Condition.Cursed]
		});

		db.RegisterProto(new EffectProto(Ids.Effects.Debuffs.Confusion, "Confused", "Your actions are erratic.") {
			IconName = "icon_effect_confuse",
			IsBuff = false,
			DefaultDuration = 2.Hours(),
			AppliedConditions = [StatusCondition.Confused],
			Tags = [Ids.Tags.Combat.Magical, Ids.Tags.Condition.Confused]
		});

		db.RegisterProto(new EffectProto(Ids.Effects.Debuffs.Disadvantage, "Disadvantage", "Your attacks are less likely to hit.") {
			IconName = "icon_effect_disadvantage",
			IsBuff = false,
			DefaultDuration = 3.Hours(),
			StatModifiers = [
				EffectStatModifier.Penalty(Ids.Stats.Combat.AttackBonus, 4)
			],
			Tags = [Ids.Tags.Source.Environment]
		});

		db.RegisterProto(new EffectProto(Ids.Effects.Debuffs.Wounds, "Wounded", "Your wounds impair healing.") {
			IconName = "icon_effect_wounds",
			IsBuff = false,
			DefaultDuration = 5.Hours(),
			StatModifiers = [
				EffectStatModifier.PercentReduce(Ids.Stats.Combat.SpellPower, 50)
			],
			Tags = [Ids.Tags.Condition.Bleeding]
		});
	}

	#endregion

	#region DoT Effects

	private void RegisterDoTEffects(GameDb db) {
		db.RegisterProto(new EffectProto(Ids.Effects.DoT.Poison, "Poisoned", "Venom courses through your veins.") {
			IconName = "icon_effect_poison",
			IsBuff = false,
			DefaultDuration = 3.Turns(),
			MaxStacks = 3,
			DamagePerTurn = HitDice.D4,
			DamageType = DamageType.Poison,
			AppliedConditions = [StatusCondition.Poisoned],
			Tags = [Ids.Tags.Condition.Poisoned, Ids.Tags.Meta.DoT]
		});

		db.RegisterProto(new EffectProto(Ids.Effects.DoT.Burning, "Burning", "You are engulfed in flames.") {
			IconName = "icon_effect_burning",
			IsBuff = false,
			DefaultDuration = 2.Turns(),
			MaxStacks = 1,
			DamagePerTurn = HitDice.D6,
			DamageType = DamageType.Fire,
			AppliedConditions = [StatusCondition.Burning],
			Tags = [Ids.Tags.Element.Fire, Ids.Tags.Condition.Burning, Ids.Tags.Meta.DoT]
		});

		db.RegisterProto(new EffectProto(Ids.Effects.DoT.Bleeding, "Bleeding", "You are losing blood.") {
			IconName = "icon_effect_bleed",
			IsBuff = false,
			DefaultDuration = 4.Turns(),
			MaxStacks = 5,
			StacksRefreshDuration = false,
			DamagePerTurn = new HitDice(1, 4),
			DamageType = DamageType.Physical,
			AppliedConditions = [StatusCondition.Bleeding],
			Tags = [Ids.Tags.Condition.Bleeding, Ids.Tags.Meta.DoT]
		});

		db.RegisterProto(new EffectProto(Ids.Effects.DoT.Decay, "Decaying", "Necrotic energy consumes you.") {
			IconName = "icon_effect_decay",
			IsBuff = false,
			DefaultDuration = 3.Turns(),
			DamagePerTurn = HitDice.D6,
			DamageType = DamageType.Necrotic,
			StatModifiers = [
				EffectStatModifier.PercentReduce(Ids.Stats.Combat.SpellPower, 25)
			],
			Tags = [Ids.Tags.Element.Necrotic, Ids.Tags.Item.Cursed, Ids.Tags.Meta.DoT]
		});

		db.RegisterProto(new EffectProto(Ids.Effects.DoT.Frostbite, "Frostbitten", "Bitter cold slows and damages you.") {
			IconName = "icon_effect_frost",
			IsBuff = false,
			DefaultDuration = 3.Turns(),
			DamagePerTurn = HitDice.D4,
			DamageType = DamageType.Cold,
			StatModifiers = [
				EffectStatModifier.PercentReduce(Ids.Stats.Movement.MovementSpeed, 25)
			],
			Tags = [Ids.Tags.Element.Cold, Ids.Tags.Source.Environment, Ids.Tags.Meta.DoT]
		});

		db.RegisterProto(new EffectProto(Ids.Effects.DoT.Shock, "Shocked", "Electrical energy courses through you.") {
			IconName = "icon_effect_shock",
			IsBuff = false,
			DefaultDuration = 2.Turns(),
			DamagePerTurn = HitDice.D6,
			DamageType = DamageType.Lightning,
			StatModifiers = [
				EffectStatModifier.Penalty(Ids.Stats.Combat.Initiative, 3)
			],
			Tags = [Ids.Tags.Element.Lightning, Ids.Tags.Combat.Magical, Ids.Tags.Meta.DoT]
		});
	}

	#endregion

	#region Control Effects

	private void RegisterControlEffects(GameDb db) {
		db.RegisterProto(new EffectProto(Ids.Effects.Control.Root, "Rooted", "You cannot move.") {
			IconName = "icon_effect_root",
			IsBuff = false,
			DefaultDuration = 2.Turns(),
			StatModifiers = [
				EffectStatModifier.PercentReduce(Ids.Stats.Movement.MovementSpeed, 100)
			],
			AppliedConditions = [StatusCondition.Rooted],
			Tags = [Ids.Tags.Combat.Magical, Ids.Tags.Element.Nature, Ids.Tags.Meta.Control]
		});

		db.RegisterProto(new EffectProto(Ids.Effects.Control.Stun, "Stunned", "You cannot act.") {
			IconName = "icon_effect_stun",
			IsBuff = false,
			DefaultDuration = 1.Turns(),
			AppliedConditions = [StatusCondition.Stunned],
			Tags = [Ids.Tags.Condition.Stunned, Ids.Tags.Meta.Control]
		});

		db.RegisterProto(new EffectProto(Ids.Effects.Control.Freeze, "Frozen", "You are frozen in time.") {
			IconName = "icon_effect_frozen",
			IsBuff = false,
			DefaultDuration = 1.Turns(),
			AppliedConditions = [StatusCondition.Frozen],
			Tags = [Ids.Tags.Element.Cold, Ids.Tags.Combat.Magical, Ids.Tags.Condition.Frozen, Ids.Tags.Meta.Control]
		});

		db.RegisterProto(new EffectProto(Ids.Effects.Control.Charm, "Charmed", "You are under another's control.") {
			IconName = "icon_effect_charm",
			IsBuff = false,
			DefaultDuration = 2.Turns(),
			AppliedConditions = [StatusCondition.Charmed],
			Tags = [Ids.Tags.Combat.Magical, Ids.Tags.Condition.Charmed, Ids.Tags.Meta.Control]
		});

		db.RegisterProto(new EffectProto(Ids.Effects.Control.Prone, "Prone", "You are knocked to the ground.") {
			IconName = "icon_effect_prone",
			IsBuff = false,
			DefaultDuration = 1.Turns(),
			StatModifiers = [
				EffectStatModifier.Penalty(Ids.Stats.Combat.ArmorClass, 2),
				EffectStatModifier.Penalty(Ids.Stats.Combat.AttackBonus, 2)
			],
			Tags = [Ids.Tags.Condition.Prone, Ids.Tags.Meta.Control]
		});
	}

	#endregion

	#region Fog Effects

	private void RegisterFogEffects(GameDb db) {
		db.RegisterProto(new EffectProto(Ids.Effects.Fog.Lingering, "Lingering Fog", "The fog clings to enemies, granting them desperate power.") {
			IconName = "icon_fog_lingering",
			IsBuff = true,
			DefaultDuration = Duration.Infinite,
			CanBeDispelled = false,
			IsHidden = true,
			Tags = [Ids.Tags.Fog.Lingering, Ids.Tags.Source.Environment]
		});

		db.RegisterProto(new EffectProto(Ids.Effects.Fog.Hazy, "Hazy Fog", "Enemies shimmer in the haze, harder to hit.") {
			IconName = "icon_fog_hazy",
			IsBuff = true,
			DefaultDuration = Duration.Infinite,
			CanBeDispelled = false,
			Tags = [Ids.Tags.Fog.Hazy, Ids.Tags.Source.Environment]
		});

		db.RegisterProto(new EffectProto(Ids.Effects.Fog.Dense, "Dense Fog", "The thick fog impairs your reactions.") {
			IconName = "icon_fog_dense",
			IsBuff = false,
			DefaultDuration = Duration.Infinite,
			CanBeDispelled = false,
			Tags = [Ids.Tags.Fog.Dense, Ids.Tags.Source.Environment]
		});

		db.RegisterProto(new EffectProto(Ids.Effects.Fog.Thick, "Thick Fog", "The fog amplifies enemy power while weakening yours.") {
			IconName = "icon_fog_thick",
			IsBuff = false,
			DefaultDuration = Duration.Infinite,
			CanBeDispelled = false,
			Tags = [Ids.Tags.Fog.Thick, Ids.Tags.Source.Environment]
		});

		db.RegisterProto(new EffectProto(Ids.Effects.Fog.Suffocating, "Suffocating Fog", "The overwhelming fog grants enemies advantage.") {
			IconName = "icon_fog_suffocating",
			IsBuff = false,
			DefaultDuration = Duration.Infinite,
			CanBeDispelled = false,
			Tags = [Ids.Tags.Fog.Suffocating, Ids.Tags.Source.Environment]
		});
	}

	#endregion

	#region Special Effects

	private void RegisterSpecialEffects(GameDb db) {
		db.RegisterProto(new EffectProto(Ids.Effects.Special.Invisible, "Invisible", "You cannot be seen or targeted.") {
			IconName = "icon_effect_invis",
			IsBuff = true,
			DefaultDuration = 3.Hours(),
			AppliedConditions = [StatusCondition.Invisible],
			Tags = [Ids.Tags.Combat.Magical, Ids.Tags.Condition.Invisible]
		});

		db.RegisterProto(new EffectProto(Ids.Effects.Special.Undying, "Undying", "You cannot fall below 1 HP.") {
			IconName = "icon_effect_undying",
			IsBuff = true,
			DefaultDuration = 2.Turns(),
			AppliedConditions = [StatusCondition.Undying],
			Tags = [Ids.Tags.School.Divine, Ids.Tags.Combat.Magical]
		});

		db.RegisterProto(new EffectProto(Ids.Effects.Special.Advantage, "Advantage", "Your attacks are more likely to hit.") {
			IconName = "icon_effect_advantage",
			IsBuff = true,
			DefaultDuration = 1.Hours(),
			StatModifiers = [
				EffectStatModifier.Flat(Ids.Stats.Combat.AttackBonus, 4)
			],
			Tags = [Ids.Tags.Meta.Buff]
		});
	}

	#endregion
}
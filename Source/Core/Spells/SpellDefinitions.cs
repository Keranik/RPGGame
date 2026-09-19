using RPGGame.Core.Combat;
using RPGGame.Core.Prototypes;
using RPGGame.Core.Prototypes.Spells;

namespace RPGGame.Core.Spells;

/// <summary>
/// Defines all spells using the Proto system.
/// </summary>
public class SpellDefinitions : ICoreData {
	public void GameData(GameDb gameDatabase) {
		RegisterCantrips(gameDatabase);
		RegisterEvocationSpells(gameDatabase);
		RegisterAbjurationSpells(gameDatabase);
		RegisterHealingSpells(gameDatabase);
		RegisterNecromancySpells(gameDatabase);
		RegisterTemporalSpells(gameDatabase);
	}

	#region Cantrips

	private void RegisterCantrips(GameDb db) {
		db.RegisterProto(new SpellProto(
			id: Ids.Spells.Cantrips.MagicMissile,
			text: Proto.CreateText("Magic Missile", "Unerring bolts of magical energy strike your foe."),
			iconName: "icon_spell_magic_missile",
			school: SpellSchool.Evocation,
			level: 0,
			manaCost: 0,
			targetType: TargetType.SingleEnemy,
			range: 60,
			damageDice: new HitDice(1, 4, 1),
			damageType: DamageType.Arcane
		));

		db.RegisterProto(new SpellProto(
			id: Ids.Spells.Cantrips.FireBolt,
			text: Proto.CreateText("Fire Bolt", "Hurl a mote of fire at a creature."),
			iconName: "icon_spell_fire_bolt",
			school: SpellSchool.Evocation,
			level: 0,
			manaCost: 0,
			targetType: TargetType.SingleEnemy,
			range: 60,
			damageDice: HitDice.D10,
			damageType: DamageType.Fire
		));

		db.RegisterProto(new SpellProto(
			id: Ids.Spells.Cantrips.RayOfFrost,
			text: Proto.CreateText("Ray of Frost", "A frigid beam of blue-white light strikes a creature."),
			iconName: "icon_spell_frost",
			school: SpellSchool.Evocation,
			level: 0,
			manaCost: 0,
			targetType: TargetType.SingleEnemy,
			range: 60,
			damageDice: HitDice.D8,
			damageType: DamageType.Cold,
			appliesCondition: StatusCondition.Slowed,
			conditionDuration: 1.Turns()
		));

		db.RegisterProto(new SpellProto(
			id: Ids.Spells.Cantrips.SacredFlame,
			text: Proto.CreateText("Sacred Flame", "Flame-like radiance descends on a creature."),
			iconName: "icon_spell_sacred_flame",
			school: SpellSchool.Holy,
			level: 0,
			manaCost: 0,
			targetType: TargetType.SingleEnemy,
			range: 60,
			damageDice: HitDice.D8,
			damageType: DamageType.Holy,
			savingThrow: Ids.Stats.Attributes.Dexterity
		));

		db.RegisterProto(new SpellProto(
			id: Ids.Spells.Cantrips.MinorHeal,
			text: Proto.CreateText("Minor Heal", "A small surge of healing energy restores vitality."),
			iconName: "icon_spell_minor_heal",
			school: SpellSchool.Holy,
			level: 0,
			manaCost: 0,
			targetType: TargetType.SingleAlly,
			range: 30,
			healingDice: HitDice.D4,
			usableOutOfCombat: true
		));
	}

	#endregion

	#region Evocation Spells

	private void RegisterEvocationSpells(GameDb db) {
		db.RegisterProto(new SpellProto(
			id: Ids.Spells.Evocation.Fireball,
			text: Proto.CreateText("Fireball", "A bright streak flashes to a point and explodes in a sphere of fire."),
			iconName: "icon_spell_fireball",
			school: SpellSchool.Evocation,
			level: 3,
			manaCost: 15,
			requiredLevel: 5,
			targetType: TargetType.AllEnemies,
			range: 120,
			areaRadius: 20,
			damageDice: new HitDice(8, 6),
			damageType: DamageType.Fire,
			savingThrow: Ids.Stats.Attributes.Dexterity
		));

		db.RegisterProto(new SpellProto(
			id: Ids.Spells.Evocation.LightningBolt,
			text: Proto.CreateText("Lightning Bolt", "A stroke of lightning forms a line 100 feet long and 5 feet wide."),
			iconName: "icon_spell_lightning",
			school: SpellSchool.Evocation,
			level: 3,
			manaCost: 15,
			requiredLevel: 5,
			targetType: TargetType.AllEnemies,
			range: 100,
			damageDice: new HitDice(8, 6),
			damageType: DamageType.Lightning,
			savingThrow: Ids.Stats.Attributes.Dexterity
		));

		db.RegisterProto(new SpellProto(
			id: Ids.Spells.Evocation.BurningHands,
			text: Proto.CreateText("Burning Hands", "A thin sheet of flames shoots forth from your fingertips."),
			iconName: "icon_spell_burning_hands",
			school: SpellSchool.Evocation,
			level: 1,
			manaCost: 5,
			targetType: TargetType.AllEnemies,
			range: 15,
			damageDice: new HitDice(3, 6),
			damageType: DamageType.Fire,
			savingThrow: Ids.Stats.Attributes.Dexterity
		));

		db.RegisterProto(new SpellProto(
			id: Ids.Spells.Evocation.IceStorm,
			text: Proto.CreateText("Ice Storm", "A hail of rock-hard ice pounds the ground in a cylinder."),
			iconName: "icon_spell_ice_storm",
			school: SpellSchool.Evocation,
			level: 4,
			manaCost: 20,
			requiredLevel: 7,
			targetType: TargetType.AllEnemies,
			range: 120,
			areaRadius: 20,
			damageDice: new HitDice(2, 8),
			damageType: DamageType.Bludgeoning,
			appliesCondition: StatusCondition.Slowed,
			conditionDuration: 1.Turns(),
			savingThrow: Ids.Stats.Attributes.Dexterity
		));

		db.RegisterProto(new SpellProto(
			id: Ids.Spells.Evocation.ChainLightning,
			text: Proto.CreateText("Chain Lightning", "Lightning arcs from target to target."),
			iconName: "icon_spell_chain_lightning",
			school: SpellSchool.Evocation,
			level: 6,
			manaCost: 30,
			requiredLevel: 11,
			targetType: TargetType.AllEnemies,
			range: 150,
			damageDice: new HitDice(10, 8),
			damageType: DamageType.Lightning,
			savingThrow: Ids.Stats.Attributes.Dexterity,
			cooldown: 3
		));
	}

	#endregion

	#region Abjuration Spells

	private void RegisterAbjurationSpells(GameDb db) {
		db.RegisterProto(new SpellProto(
			id: Ids.Spells.Abjuration.ArcaneShield,
			text: Proto.CreateText("Arcane Shield", "An invisible barrier of magical force protects you."),
			iconName: "icon_spell_shield",
			school: SpellSchool.Abjuration,
			level: 1,
			manaCost: 5,
			targetType: TargetType.Self,
			appliesCondition: StatusCondition.Shielded,
			conditionDuration: 3.Hours()
		));

		db.RegisterProto(new SpellProto(
			id: Ids.Spells.Abjuration.MageArmor,
			text: Proto.CreateText("Mage Armor", "You touch a willing creature and a protective magical force surrounds it."),
			iconName: "icon_spell_mage_armor",
			school: SpellSchool.Abjuration,
			level: 1,
			manaCost: 5,
			targetType: TargetType.SingleAlly,
			range: 0,
			usableOutOfCombat: true
		));

		db.RegisterProto(new SpellProto(
			id: Ids.Spells.Abjuration.DispelMagic,
			text: Proto.CreateText("Dispel Magic", "Choose one creature, object, or magical effect. Any spell ends."),
			iconName: "icon_spell_dispel",
			school: SpellSchool.Abjuration,
			level: 3,
			manaCost: 15,
			requiredLevel: 5,
			targetType: TargetType.SingleEnemy,
			range: 60
		));

		db.RegisterProto(new SpellProto(
			id: Ids.Spells.Abjuration.ProtectionFromEvil,
			text: Proto.CreateText("Protection from Evil", "A protective aura guards against undead and fiends."),
			iconName: "icon_spell_protection",
			school: SpellSchool.Abjuration,
			level: 1,
			manaCost: 5,
			targetType: TargetType.SingleAlly,
			requiresConcentration: true
		));

		db.RegisterProto(new SpellProto(
			id: Ids.Spells.Abjuration.CounterSpell,
			text: Proto.CreateText("Counter Spell", "You attempt to interrupt a creature in the process of casting a spell."),
			iconName: "icon_spell_counter",
			school: SpellSchool.Abjuration,
			level: 3,
			manaCost: 15,
			requiredLevel: 5,
			targetType: TargetType.SingleEnemy,
			range: 60
		));
	}

	#endregion

	#region Healing Spells

	private void RegisterHealingSpells(GameDb db) {
		db.RegisterProto(new SpellProto(
			id: Ids.Spells.Holy.Heal,
			text: Proto.CreateText("Heal", "Restore health to a creature you touch."),
			iconName: "icon_spell_heal",
			school: SpellSchool.Holy,
			level: 1,
			manaCost: 5,
			targetType: TargetType.SingleAlly,
			range: 0,
			healingDice: new HitDice(1, 8, 3),
			usableOutOfCombat: true
		));

		db.RegisterProto(new SpellProto(
			id: Ids.Spells.Holy.CureWounds,
			text: Proto.CreateText("Cure Wounds", "A creature you touch regains hit points."),
			iconName: "icon_spell_cure",
			school: SpellSchool.Holy,
			level: 1,
			manaCost: 5,
			targetType: TargetType.SingleAlly,
			range: 0,
			healingDice: new HitDice(2, 8, 2),
			usableOutOfCombat: true
		));

		db.RegisterProto(new SpellProto(
			id: Ids.Spells.Holy.MassHeal,
			text: Proto.CreateText("Mass Heal", "Healing energy radiates from you to restore all nearby allies."),
			iconName: "icon_spell_mass_heal",
			school: SpellSchool.Holy,
			level: 5,
			manaCost: 30,
			requiredLevel: 9,
			targetType: TargetType.AllAllies,
			healingDice: new HitDice(3, 8, 5)
		));

		db.RegisterProto(new SpellProto(
			id: Ids.Spells.Holy.DivineLight,
			text: Proto.CreateText("Divine Light", "Holy radiance damages undead and heals allies."),
			iconName: "icon_spell_divine_light",
			school: SpellSchool.Holy,
			level: 2,
			manaCost: 10,
			requiredLevel: 3,
			targetType: TargetType.All,
			range: 30,
			areaRadius: 15,
			damageDice: new HitDice(2, 8),
			damageType: DamageType.Holy,
			healingDice: HitDice.D8
		));

		db.RegisterProto(new SpellProto(
			id: Ids.Spells.Holy.HolySmite,
			text: Proto.CreateText("Holy Smite", "Divine wrath smites the unholy."),
			iconName: "icon_spell_smite",
			school: SpellSchool.Holy,
			level: 2,
			manaCost: 10,
			requiredLevel: 3,
			targetType: TargetType.SingleEnemy,
			range: 0,
			damageDice: new HitDice(2, 8),
			damageType: DamageType.Holy
		));

		db.RegisterProto(new SpellProto(
			id: Ids.Spells.Holy.Resurrection,
			text: Proto.CreateText("Resurrection", "You return a dead creature to life."),
			iconName: "icon_spell_resurrection",
			school: SpellSchool.Holy,
			level: 7,
			manaCost: 50,
			requiredLevel: 13,
			targetType: TargetType.SingleAlly,
			range: 0,
			cooldown: 10,
			usableOutOfCombat: true
		));

		db.RegisterProto(new SpellProto(
			id: Ids.Spells.Holy.Bless,
			text: Proto.CreateText("Bless", "You bless up to three creatures, granting them divine favor."),
			iconName: "icon_spell_bless",
			school: SpellSchool.Holy,
			level: 1,
			manaCost: 5,
			targetType: TargetType.AllAllies,
			appliesCondition: StatusCondition.Empowered,
			conditionDuration: 5.Hours(),
			requiresConcentration: true
		));
	}

	#endregion

	#region Necromancy Spells

	private void RegisterNecromancySpells(GameDb db) {
		db.RegisterProto(new SpellProto(
			id: Ids.Spells.Necromancy.LifeDrain,
			text: Proto.CreateText("Life Drain", "Drain the life force from an enemy to heal yourself."),
			iconName: "icon_spell_drain",
			school: SpellSchool.Necromancy,
			level: 2,
			manaCost: 10,
			targetType: TargetType.SingleEnemy,
			range: 30,
			damageDice: new HitDice(2, 8),
			damageType: DamageType.Necrotic,
			healingDice: HitDice.D8
		));

		db.RegisterProto(new SpellProto(
			id: Ids.Spells.Necromancy.SoulRend,
			text: Proto.CreateText("Soul Rend", "Tear at the very essence of a creature."),
			iconName: "icon_spell_soul_rend",
			school: SpellSchool.Necromancy,
			level: 4,
			manaCost: 20,
			requiredLevel: 7,
			targetType: TargetType.SingleEnemy,
			range: 60,
			damageDice: new HitDice(4, 10),
			damageType: DamageType.Necrotic,
			savingThrow: Ids.Stats.Attributes.Constitution
		));

		db.RegisterProto(new SpellProto(
			id: Ids.Spells.Necromancy.AnimateDead,
			text: Proto.CreateText("Animate Dead", "Create an undead servant from a corpse."),
			iconName: "icon_spell_animate",
			school: SpellSchool.Necromancy,
			level: 3,
			manaCost: 15,
			requiredLevel: 5,
			targetType: TargetType.SingleEnemy,
			range: 10,
			usableOutOfCombat: true
		));

		db.RegisterProto(new SpellProto(
			id: Ids.Spells.Necromancy.Fear,
			text: Proto.CreateText("Fear", "Project an image of a creature's worst fears."),
			iconName: "icon_spell_fear",
			school: SpellSchool.Necromancy,
			level: 3,
			manaCost: 15,
			requiredLevel: 5,
			targetType: TargetType.AllEnemies,
			range: 30,
			appliesCondition: StatusCondition.Frightened,
			conditionDuration: 3.Turns(),
			savingThrow: Ids.Stats.Attributes.Wisdom
		));

		db.RegisterProto(new SpellProto(
			id: Ids.Spells.Necromancy.Curse,
			text: Proto.CreateText("Curse", "You curse a creature, weakening it significantly."),
			iconName: "icon_spell_curse",
			school: SpellSchool.Necromancy,
			level: 2,
			manaCost: 10,
			requiredLevel: 3,
			targetType: TargetType.SingleEnemy,
			range: 30,
			appliesCondition: StatusCondition.Weakened,
			conditionDuration: 5.Hours(),
			savingThrow: Ids.Stats.Attributes.Wisdom
		));
	}

	#endregion

	#region Temporal Spells

	private void RegisterTemporalSpells(GameDb db) {
		db.RegisterProto(new SpellProto(
			id: Ids.Spells.Temporal.TimeEcho,
			text: Proto.CreateText("Time Echo", "Create an echo of yourself from a moment ago."),
			iconName: "icon_spell_time_echo",
			school: SpellSchool.Temporal,
			level: 2,
			manaCost: 10,
			requiredLevel: 3,
			targetType: TargetType.Self,
			classRestrictions: [Ids.CharacterClasses.Ascended, Ids.CharacterClasses.TimeWalker]
		));

		db.RegisterProto(new SpellProto(
			id: Ids.Spells.Temporal.FateSight,
			text: Proto.CreateText("Fate Sight", "Glimpse possible futures, granting advantage on your next action."),
			iconName: "icon_spell_fate",
			school: SpellSchool.Temporal,
			level: 1,
			manaCost: 5,
			targetType: TargetType.Self,
			classRestrictions: [Ids.CharacterClasses.Ascended, Ids.CharacterClasses.TimeWalker]
		));

		db.RegisterProto(new SpellProto(
			id: Ids.Spells.Temporal.RewindWound,
			text: Proto.CreateText("Rewind Wound", "Turn back time on an injury, undoing the damage."),
			iconName: "icon_spell_rewind",
			school: SpellSchool.Temporal,
			level: 3,
			manaCost: 20,
			requiredLevel: 5,
			targetType: TargetType.SingleAlly,
			range: 30,
			healingDice: new HitDice(4, 8, 10),
			cooldown: 3,
			classRestrictions: [Ids.CharacterClasses.Ascended, Ids.CharacterClasses.TimeWalker]
		));

		db.RegisterProto(new SpellProto(
			id: Ids.Spells.Temporal.TemporalShift,
			text: Proto.CreateText("Temporal Shift", "Phase briefly out of time, avoiding all attacks."),
			iconName: "icon_spell_shift",
			school: SpellSchool.Temporal,
			level: 4,
			manaCost: 25,
			requiredLevel: 7,
			targetType: TargetType.Self,
			appliesCondition: StatusCondition.Invisible,
			conditionDuration: 2.Turns(),
			cooldown: 5,
			classRestrictions: [Ids.CharacterClasses.Ascended, Ids.CharacterClasses.TimeWalker]
		));

		db.RegisterProto(new SpellProto(
			id: Ids.Spells.Temporal.UnravelFate,
			text: Proto.CreateText("Unravel Fate", "Tear at the threads of destiny itself."),
			iconName: "icon_spell_unravel",
			school: SpellSchool.Temporal,
			level: 5,
			manaCost: 35,
			requiredLevel: 9,
			targetType: TargetType.SingleEnemy,
			range: 60,
			damageDice: new HitDice(6, 10),
			damageType: DamageType.Arcane,
			appliesCondition: StatusCondition.Weakened,
			conditionDuration: 3.Turns(),
			cooldown: 5,
			classRestrictions: [Ids.CharacterClasses.Ascended, Ids.CharacterClasses.TimeWalker]
		));

		db.RegisterProto(new SpellProto(
			id: Ids.Spells.Temporal.TimeStop,
			text: Proto.CreateText("Time Stop", "You briefly stop time for everyone but yourself."),
			iconName: "icon_spell_time_stop",
			school: SpellSchool.Temporal,
			level: 9,
			manaCost: 60,
			requiredLevel: 17,
			targetType: TargetType.AllEnemies,
			appliesCondition: StatusCondition.Frozen,
			conditionDuration: 2.Turns(),
			cooldown: 10,
			classRestrictions: [Ids.CharacterClasses.Ascended, Ids.CharacterClasses.TimeWalker]
		));
	}

	#endregion
}
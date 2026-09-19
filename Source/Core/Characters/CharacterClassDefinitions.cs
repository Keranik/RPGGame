using RPGGame.Core.Items;
using RPGGame.Core.Prototypes;
using RPGGame.Core.Prototypes.Characters;
using RPGGame.Core.Stats;

namespace RPGGame.Core.Characters;

/// <summary>
/// Defines all character classes using the Proto system.
/// </summary>
public class CharacterClassDefinitions : ICoreData {
	public void GameData(GameDb gameDatabase) {
		RegisterStarterClasses(gameDatabase);
		RegisterUnlockableClasses(gameDatabase);
		RegisterFogThemedClasses(gameDatabase);
		RegisterSecretClasses(gameDatabase);
	}

	#region Starter Classes

	private void RegisterStarterClasses(GameDb db) {
		// ═══════════════════════════════════════════════════════════════
		// FIGHTER - Starter Class (STR/CON)
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new CharacterClassProto(
			id: Ids.CharacterClasses.Fighter,
			text: Proto.CreateText("Fighter", "A stalwart fighter trained in the ways of combat. Fighters excel in melee combat and can withstand significant punishment."),
			tagline: "Strong. Durable. Dependable.",
			iconName: "icon_class_fighter",
			portraitName: "portrait_fighter",
			hitDice: HitDice.D10,
			baseHealth: 24,
			baseMana: 0,
			baseArmorClass: 10,
			baseAttributes: new ClassAttributes {
				Strength = 14,
				Dexterity = 10,
				Constitution = 14,
				Intelligence = 8,
				Wisdom = 10,
				Charisma = 10
			},
			healthPerLevel: 6,
			manaPerLevel: 0,
			primaryAttribute: Ids.Stats.Attributes.Strength,
			secondaryAttribute: Ids.Stats.Attributes.Constitution,
			spellcastingType: SpellcastingType.None,
			role: ClassRole.Melee,
			difficulty: 1,
			weaponProficiencies: [
				WeaponType.Sword, WeaponType.Axe, WeaponType.Mace,
				WeaponType.Spear, WeaponType.Shield
			],
			armorProficiencies: [ArmorType.Light, ArmorType.Medium, ArmorType.Heavy],
			startingSkillSchools: [
				Ids.Tags.Meta.CombatSkill,
				Ids.Tags.Meta.PhysicalSkill
			],
			startingSpellSchools: [],
			startingSkills: [
				Ids.Skills.Combat.OneHandedWeapons,
				Ids.Skills.Combat.ArmorProficiency
			],
			startingSpells: [],
			bonusCreationPoints: 10,
			startingEquipment: [
				Ids.Weapons.Swords.IronSword,
				Ids.Armor.Light.LeatherArmor,
				Ids.Armor.Shields.WoodenShield
			],
			startingGold: 15,
			startsUnlocked: true
		));

		// ═══════════════════════════════════════════════════════════════
		// ROGUE - Starter Class (DEX/INT)
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new CharacterClassProto(
			id: Ids.CharacterClasses.Rogue,
			text: Proto.CreateText("Rogue", "A cunning fighter who relies on speed and precision. Rogues deal devastating critical hits and can avoid danger."),
			tagline: "Quick. Precise. Deadly.",
			iconName: "icon_class_rogue",
			portraitName: "portrait_rogue",
			hitDice: HitDice.D8,
			baseHealth: 16,
			baseMana: 0,
			baseArmorClass: 10,
			baseAttributes: new ClassAttributes {
				Strength = 10,
				Dexterity = 16,
				Constitution = 10,
				Intelligence = 12,
				Wisdom = 10,
				Charisma = 10
			},
			healthPerLevel: 4,
			manaPerLevel: 0,
			primaryAttribute: Ids.Stats.Attributes.Dexterity,
			secondaryAttribute: Ids.Stats.Attributes.Intelligence,
			spellcastingType: SpellcastingType.None,
			role: ClassRole.Melee,
			difficulty: 3,
			weaponProficiencies: [WeaponType.Dagger, WeaponType.Sword, WeaponType.Bow],
			armorProficiencies: [ArmorType.Light],
			startingSkillSchools: [
				Ids.Tags.Meta.CombatSkill,
				Ids.Tags.Meta.StealthSkill,
				Ids.Tags.Meta.PhysicalSkill
			],
			startingSpellSchools: [],
			startingSkills: [
				Ids.Skills.Combat.CriticalStrike,
				Ids.Skills.Stealth.Sneaking
			],
			startingSpells: [],
			startingEquipment: [
				Ids.Weapons.Daggers.SteelDagger,
				Ids.Armor.Light.LeatherArmor,
				Ids.Items.Consumables.Lockpicks
			],
			startingGold: 20,
			startsUnlocked: true
		));

		// ═══════════════════════════════════════════════════════════════
		// MAGE - Starter Class (INT/WIS)
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new CharacterClassProto(
			id: Ids.CharacterClasses.Mage,
			text: Proto.CreateText("Mage", "A wielder of arcane magic. Mages command powerful spells but are fragile in close combat."),
			tagline: "Knowledge is power.",
			iconName: "icon_class_mage",
			portraitName: "portrait_mage",
			hitDice: HitDice.D6,
			baseHealth: 12,
			baseMana: 20,
			baseArmorClass: 10,
			baseAttributes: new ClassAttributes {
				Strength = 8,
				Dexterity = 10,
				Constitution = 10,
				Intelligence = 16,
				Wisdom = 12,
				Charisma = 10
			},
			healthPerLevel: 3,
			manaPerLevel: 5,
			primaryAttribute: Ids.Stats.Attributes.Intelligence,
			secondaryAttribute: Ids.Stats.Attributes.Wisdom,
			spellcastingType: SpellcastingType.Arcane,
			role: ClassRole.Caster,
			difficulty: 4,
			weaponProficiencies: [WeaponType.Staff, WeaponType.Dagger],
			armorProficiencies: [ArmorType.Cloth],
			startingSkillSchools: [
				Ids.Tags.Meta.MagicSkill,
				Ids.Tags.Meta.KnowledgeSkill
			],
			startingSpellSchools: [
				Ids.Tags.School.Evocation,
				Ids.Tags.School.Abjuration
			],
			startingSkills: [
				Ids.Skills.Knowledge.Arcana,
				Ids.Skills.Magic.SpellFocus
			],
			startingSpells: [
				Ids.Spells.Cantrips.MagicMissile,
				Ids.Spells.Cantrips.FireBolt
			],
			startingEquipment: [
				Ids.Weapons.Staves.ApprenticeStaff,
				Ids.Armor.Cloth.ClothRobe,
				Ids.Items.Accessories.Spellbook
			],
			startingGold: 10,
			startsUnlocked: true
		));

		// ═══════════════════════════════════════════════════════════════
		// CLERIC - Starter Class (WIS/CON)
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new CharacterClassProto(
			id: Ids.CharacterClasses.Cleric,
			text: Proto.CreateText("Cleric", "A divine servant blessed with healing magic. Clerics support allies and smite the unholy."),
			tagline: "Faith is my shield.",
			iconName: "icon_class_cleric",
			portraitName: "portrait_cleric",
			hitDice: HitDice.D8,
			baseHealth: 16,
			baseMana: 15,
			baseArmorClass: 10,
			baseAttributes: new ClassAttributes {
				Strength = 12,
				Dexterity = 8,
				Constitution = 12,
				Intelligence = 10,
				Wisdom = 16,
				Charisma = 12
			},
			healthPerLevel: 4,
			manaPerLevel: 4,
			primaryAttribute: Ids.Stats.Attributes.Wisdom,
			secondaryAttribute: Ids.Stats.Attributes.Constitution,
			spellcastingType: SpellcastingType.Divine,
			role: ClassRole.Support,
			difficulty: 2,
			weaponProficiencies: [WeaponType.Mace, WeaponType.Staff, WeaponType.Shield],
			armorProficiencies: [ArmorType.Light, ArmorType.Medium],
			startingSkillSchools: [
				Ids.Tags.Meta.CombatSkill,
				Ids.Tags.Meta.MagicSkill,
				Ids.Tags.Meta.KnowledgeSkill
			],
			startingSpellSchools: [
				Ids.Tags.School.Holy
			],
			startingSkills: [
				Ids.Skills.Knowledge.Religion,
				Ids.Skills.Magic.HolyMagic
			],
			startingSpells: [
				Ids.Spells.Cantrips.MinorHeal,
				Ids.Spells.Holy.Heal
			],
			startingEquipment: [
				Ids.Weapons.Maces.IronMace,
				Ids.Armor.Medium.ChainMail,
				Ids.Items.Accessories.HolySymbol
			],
			startingGold: 10,
			startsUnlocked: true
		));
	}

	#endregion

	#region Unlockable Classes

	private void RegisterUnlockableClasses(GameDb db) {
		// ═══════════════════════════════════════════════════════════════
		// RANGER (DEX/WIS)
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new CharacterClassProto(
			id: Ids.CharacterClasses.Ranger,
			text: Proto.CreateText("Ranger", "A master of wilderness survival. Rangers excel at ranged combat and tracking."),
			tagline: "At home in the wild.",
			iconName: "icon_class_ranger",
			portraitName: "portrait_ranger",
			hitDice: HitDice.D10,
			baseHealth: 20,
			baseMana: 5,
			baseArmorClass: 10,
			baseAttributes: new ClassAttributes {
				Strength = 12,
				Dexterity = 14,
				Constitution = 12,
				Intelligence = 10,
				Wisdom = 14,
				Charisma = 8
			},
			healthPerLevel: 5,
			manaPerLevel: 2,
			primaryAttribute: Ids.Stats.Attributes.Dexterity,
			secondaryAttribute: Ids.Stats.Attributes.Wisdom,
			spellcastingType: SpellcastingType.Nature,
			role: ClassRole.Ranged,
			difficulty: 2,
			weaponProficiencies: [WeaponType.Bow, WeaponType.Crossbow, WeaponType.Sword, WeaponType.Dagger],
			armorProficiencies: [ArmorType.Light, ArmorType.Medium],
			startingSkillSchools: [
				Ids.Tags.Meta.CombatSkill,
				Ids.Tags.Meta.SurvivalSkill,
				Ids.Tags.Meta.PhysicalSkill
			],
			startingSpellSchools: [
				Ids.Tags.School.Nature
			],
			startingSkills: [
				Ids.Skills.Combat.Archery,
				Ids.Skills.Survival.General,
				Ids.Skills.Awareness.Tracking
			],
			startingSpells: [],
			startingEquipment: [
				Ids.Weapons.Bows.HuntingBow,
				Ids.Armor.Light.LeatherArmor,
				Ids.Items.Accessories.Quiver
			],
			startingGold: 12,
			startsUnlocked: false,
			unlockHint: "Explore 50 new tiles",
			requiredFogClears: 1
		));

		// ═══════════════════════════════════════════════════════════════
		// PALADIN (STR/CHA)
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new CharacterClassProto(
			id: Ids.CharacterClasses.Paladin,
			text: Proto.CreateText("Paladin", "A holy warrior who combines martial prowess with divine magic. Paladins are champions of justice."),
			tagline: "Righteousness made manifest.",
			iconName: "icon_class_paladin",
			portraitName: "portrait_paladin",
			hitDice: HitDice.D10,
			baseHealth: 20,
			baseMana: 10,
			baseArmorClass: 10,
			baseAttributes: new ClassAttributes {
				Strength = 14,
				Dexterity = 8,
				Constitution = 12,
				Intelligence = 10,
				Wisdom = 12,
				Charisma = 14
			},
			healthPerLevel: 5,
			manaPerLevel: 3,
			primaryAttribute: Ids.Stats.Attributes.Strength,
			secondaryAttribute: Ids.Stats.Attributes.Charisma,
			spellcastingType: SpellcastingType.Divine,
			role: ClassRole.Tank,
			difficulty: 3,
			weaponProficiencies: [WeaponType.Sword, WeaponType.Mace, WeaponType.Shield],
			armorProficiencies: [ArmorType.Light, ArmorType.Medium, ArmorType.Heavy],
			startingSkillSchools: [
				Ids.Tags.Meta.CombatSkill,
				Ids.Tags.Meta.MagicSkill
			],
			startingSpellSchools: [
				Ids.Tags.School.Holy
			],
			startingSkills: [
				Ids.Skills.Combat.OneHandedWeapons,
				Ids.Skills.Combat.Shields,
				Ids.Skills.Magic.HolyMagic
			],
			startingSpells: [
				Ids.Spells.Holy.HolySmite
			],
			startingEquipment: [
				Ids.Weapons.Swords.IronSword,
				Ids.Armor.Medium.ChainMail,
				Ids.Armor.Shields.IronShield
			],
			startingGold: 15,
			startsUnlocked: false,
			unlockHint: "Heal 500 total HP across runs",
			requiredFogClears: 2
		));

		// ═══════════════════════════════════════════════════════════════
		// NECROMANCER (INT/CON)
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new CharacterClassProto(
			id: Ids.CharacterClasses.Necromancer,
			text: Proto.CreateText("Necromancer", "A dark mage who commands the forces of death. Necromancers drain life and raise the dead."),
			tagline: "Death is but a tool.",
			iconName: "icon_class_necromancer",
			portraitName: "portrait_necromancer",
			hitDice: HitDice.D6,
			baseHealth: 12,
			baseMana: 18,
			baseArmorClass: 10,
			baseAttributes: new ClassAttributes {
				Strength = 8,
				Dexterity = 10,
				Constitution = 12,
				Intelligence = 16,
				Wisdom = 12,
				Charisma = 8
			},
			healthPerLevel: 3,
			manaPerLevel: 5,
			primaryAttribute: Ids.Stats.Attributes.Intelligence,
			secondaryAttribute: Ids.Stats.Attributes.Constitution,
			spellcastingType: SpellcastingType.Arcane,
			role: ClassRole.Caster,
			difficulty: 4,
			weaponProficiencies: [WeaponType.Staff, WeaponType.Dagger],
			armorProficiencies: [ArmorType.Cloth],
			startingSkillSchools: [
				Ids.Tags.Meta.MagicSkill,
				Ids.Tags.Meta.KnowledgeSkill
			],
			startingSpellSchools: [
				Ids.Tags.School.Necromancy
			],
			startingSkills: [
				Ids.Skills.Knowledge.Arcana,
				Ids.Skills.Magic.Necromancy
			],
			startingSpells: [
				Ids.Spells.Necromancy.LifeDrain
			],
			startingEquipment: [
				Ids.Weapons.Staves.ApprenticeStaff,
				Ids.Armor.Cloth.ClothRobe,
				Ids.Items.Accessories.Spellbook
			],
			startingGold: 10,
			startsUnlocked: false,
			unlockHint: "Defeat 50 undead enemies",
			requiredFogClears: 2
		));

		// ═══════════════════════════════════════════════════════════════
		// BARBARIAN (STR/CON)
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new CharacterClassProto(
			id: Ids.CharacterClasses.Barbarian,
			text: Proto.CreateText("Barbarian", "A primal warrior who fights with unbridled fury. Barbarians deal massive damage but lack finesse."),
			tagline: "Rage incarnate.",
			iconName: "icon_class_barbarian",
			portraitName: "portrait_barbarian",
			hitDice: HitDice.D12,
			baseHealth: 28,
			baseMana: 0,
			baseArmorClass: 10,
			baseAttributes: new ClassAttributes {
				Strength = 16,
				Dexterity = 12,
				Constitution = 14,
				Intelligence = 8,
				Wisdom = 10,
				Charisma = 8
			},
			healthPerLevel: 7,
			manaPerLevel: 0,
			primaryAttribute: Ids.Stats.Attributes.Strength,
			secondaryAttribute: Ids.Stats.Attributes.Constitution,
			spellcastingType: SpellcastingType.None,
			role: ClassRole.Melee,
			difficulty: 2,
			weaponProficiencies: [WeaponType.Axe, WeaponType.Sword, WeaponType.Mace],
			armorProficiencies: [ArmorType.Light, ArmorType.Medium],
			startingSkillSchools: [
				Ids.Tags.Meta.CombatSkill,
				Ids.Tags.Meta.PhysicalSkill,
				Ids.Tags.Meta.SurvivalSkill
			],
			startingSpellSchools: [],
			startingSkills: [
				Ids.Skills.Combat.TwoHandedWeapons,
				Ids.Skills.Physical.Athletics
			],
			startingSpells: [],
			bonusCreationPoints: 5,
			startingEquipment: [
				Ids.Weapons.Axes.GreatAxe,
				Ids.Armor.Light.LeatherArmor
			],
			startingGold: 8,
			startsUnlocked: false,
			unlockHint: "Deal 1000 damage in a single run",
			requiredFogClears: 1
		));

		// ═══════════════════════════════════════════════════════════════
		// BARD (CHA/DEX)
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new CharacterClassProto(
			id: Ids.CharacterClasses.Bard,
			text: Proto.CreateText("Bard", "A versatile performer who weaves magic through music. Bards inspire allies and confound enemies."),
			tagline: "Every tale needs a hero.",
			iconName: "icon_class_bard",
			portraitName: "portrait_bard",
			hitDice: HitDice.D8,
			baseHealth: 16,
			baseMana: 12,
			baseArmorClass: 10,
			baseAttributes: new ClassAttributes {
				Strength = 10,
				Dexterity = 14,
				Constitution = 10,
				Intelligence = 12,
				Wisdom = 10,
				Charisma = 16
			},
			healthPerLevel: 4,
			manaPerLevel: 4,
			primaryAttribute: Ids.Stats.Attributes.Charisma,
			secondaryAttribute: Ids.Stats.Attributes.Dexterity,
			spellcastingType: SpellcastingType.Arcane,
			role: ClassRole.Support,
			difficulty: 3,
			weaponProficiencies: [WeaponType.Sword, WeaponType.Dagger, WeaponType.Bow],
			armorProficiencies: [ArmorType.Light],
			startingSkillSchools: [
				Ids.Tags.Meta.CombatSkill,
				Ids.Tags.Meta.SocialSkill,
				Ids.Tags.Meta.MagicSkill
			],
			startingSpellSchools: [
				Ids.Tags.School.Enchantment,
				Ids.Tags.School.Illusion
			],
			startingSkills: [
				Ids.Skills.Social.Performance,
				Ids.Skills.Social.Persuasion
			],
			startingSpells: [],
			startingEquipment: [
				Ids.Weapons.Swords.ShortSword,
				Ids.Armor.Light.LeatherArmor
			],
			startingGold: 25,
			startsUnlocked: false,
			unlockHint: "Complete 10 non-combat events",
			requiredFogClears: 2
		));

		// ═══════════════════════════════════════════════════════════════
		// DRUID (WIS/CON)
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new CharacterClassProto(
			id: Ids.CharacterClasses.Druid,
			text: Proto.CreateText("Druid", "A guardian of nature who channels primal magic. Druids can shapeshift and command the elements."),
			tagline: "Nature's wrath and mercy.",
			iconName: "icon_class_druid",
			portraitName: "portrait_druid",
			hitDice: HitDice.D8,
			baseHealth: 16,
			baseMana: 15,
			baseArmorClass: 10,
			baseAttributes: new ClassAttributes {
				Strength = 10,
				Dexterity = 12,
				Constitution = 12,
				Intelligence = 10,
				Wisdom = 16,
				Charisma = 10
			},
			healthPerLevel: 4,
			manaPerLevel: 4,
			primaryAttribute: Ids.Stats.Attributes.Wisdom,
			secondaryAttribute: Ids.Stats.Attributes.Constitution,
			spellcastingType: SpellcastingType.Nature,
			role: ClassRole.Hybrid,
			difficulty: 4,
			weaponProficiencies: [WeaponType.Staff, WeaponType.Dagger, WeaponType.Mace],
			armorProficiencies: [ArmorType.Light, ArmorType.Medium],
			startingSkillSchools: [
				Ids.Tags.Meta.MagicSkill,
				Ids.Tags.Meta.SurvivalSkill,
				Ids.Tags.Meta.KnowledgeSkill
			],
			startingSpellSchools: [
				Ids.Tags.School.Nature
			],
			startingSkills: [
				Ids.Skills.Knowledge.Nature,
				Ids.Skills.Magic.NatureMagic,
				Ids.Skills.Survival.AnimalHandling
			],
			startingSpells: [],
			startingEquipment: [
				Ids.Weapons.Staves.WoodenStaff,
				Ids.Armor.Light.LeatherArmor
			],
			startingGold: 10,
			startsUnlocked: false,
			unlockHint: "Gather 100 herbs",
			requiredFogClears: 2
		));

		// ═══════════════════════════════════════════════════════════════
		// MONK (DEX/WIS)
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new CharacterClassProto(
			id: Ids.CharacterClasses.Monk,
			text: Proto.CreateText("Monk", "Masters of body and mind, Monks channel their inner ki into devastating martial arts and supernatural resilience."),
			tagline: "The body is the weapon.",
			iconName: "icon_class_monk",
			portraitName: "portrait_monk",
			hitDice: HitDice.D8,
			baseHealth: 16,
			baseMana: 10,
			baseArmorClass: 12,
			baseAttributes: new ClassAttributes {
				Strength = 12,
				Dexterity = 16,
				Constitution = 12,
				Intelligence = 10,
				Wisdom = 14,
				Charisma = 8
			},
			healthPerLevel: 4,
			manaPerLevel: 3,
			primaryAttribute: Ids.Stats.Attributes.Dexterity,
			secondaryAttribute: Ids.Stats.Attributes.Wisdom,
			spellcastingType: SpellcastingType.None,
			role: ClassRole.Melee,
			difficulty: 3,
			weaponProficiencies: [WeaponType.Staff],
			armorProficiencies: [ArmorType.Cloth],
			startingSkillSchools: [
				Ids.Tags.Meta.CombatSkill,
				Ids.Tags.Meta.PhysicalSkill
			],
			startingSpellSchools: [],
			startingSkills: [
				Ids.Skills.Combat.Unarmed,
				Ids.Skills.Physical.Acrobatics,
				Ids.Skills.Combat.Dodge
			],
			startingSpells: [],
			startingEquipment: [
				Ids.Armor.Cloth.ClothRobe
			],
			startingGold: 5,
			startsUnlocked: false,
			unlockHint: "Win 10 fights without weapons",
			requiredFogClears: 2
		));

		// ═══════════════════════════════════════════════════════════════
		// WARLORD (CHA/STR)
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new CharacterClassProto(
			id: Ids.CharacterClasses.Warlord,
			text: Proto.CreateText("Warlord", "A born leader who commands respect on the battlefield. Warlords inspire allies to fight beyond their limits."),
			tagline: "Victory through leadership.",
			iconName: "icon_class_warlord",
			portraitName: "portrait_warlord",
			hitDice: HitDice.D10,
			baseHealth: 22,
			baseMana: 0,
			baseArmorClass: 10,
			baseAttributes: new ClassAttributes {
				Strength = 14,
				Dexterity = 10,
				Constitution = 12,
				Intelligence = 10,
				Wisdom = 10,
				Charisma = 16
			},
			healthPerLevel: 5,
			manaPerLevel: 0,
			primaryAttribute: Ids.Stats.Attributes.Charisma,
			secondaryAttribute: Ids.Stats.Attributes.Strength,
			spellcastingType: SpellcastingType.None,
			role: ClassRole.Support,
			difficulty: 3,
			weaponProficiencies: [WeaponType.Sword, WeaponType.Spear, WeaponType.Shield],
			armorProficiencies: [ArmorType.Light, ArmorType.Medium, ArmorType.Heavy],
			startingSkillSchools: [
				Ids.Tags.Meta.CombatSkill,
				Ids.Tags.Meta.SocialSkill
			],
			startingSpellSchools: [],
			startingSkills: [
				Ids.Skills.Combat.OneHandedWeapons,
				Ids.Skills.Combat.Tactics,
				Ids.Skills.Social.Leadership
			],
			startingSpells: [],
			startingEquipment: [
				Ids.Weapons.Swords.IronSword,
				Ids.Armor.Medium.ChainMail
			],
			startingGold: 20,
			startsUnlocked: false,
			unlockHint: "Complete 5 runs with companions",
			requiredFogClears: 2
		));

		// ═══════════════════════════════════════════════════════════════
		// SHADOW DANCER (DEX/CHA)
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new CharacterClassProto(
			id: Ids.CharacterClasses.ShadowDancer,
			text: Proto.CreateText("Shadow Dancer", "Masters of misdirection who move between shadows. Shadow Dancers strike unseen and vanish without a trace."),
			tagline: "You never saw me.",
			iconName: "icon_class_shadowdancer",
			portraitName: "portrait_shadowdancer",
			hitDice: HitDice.D8,
			baseHealth: 14,
			baseMana: 8,
			baseArmorClass: 11,
			baseAttributes: new ClassAttributes {
				Strength = 10,
				Dexterity = 16,
				Constitution = 10,
				Intelligence = 12,
				Wisdom = 10,
				Charisma = 14
			},
			healthPerLevel: 4,
			manaPerLevel: 2,
			primaryAttribute: Ids.Stats.Attributes.Dexterity,
			secondaryAttribute: Ids.Stats.Attributes.Charisma,
			spellcastingType: SpellcastingType.Arcane,
			role: ClassRole.Melee,
			difficulty: 4,
			weaponProficiencies: [WeaponType.Dagger, WeaponType.Sword],
			armorProficiencies: [ArmorType.Light],
			startingSkillSchools: [
				Ids.Tags.Meta.StealthSkill,
				Ids.Tags.Meta.CombatSkill,
				Ids.Tags.Meta.SocialSkill
			],
			startingSpellSchools: [
				Ids.Tags.School.Illusion
			],
			startingSkills: [
				Ids.Skills.Stealth.Sneaking,
				Ids.Skills.Stealth.Hiding,
				Ids.Skills.Combat.CriticalStrike,
				Ids.Skills.Social.Deception
			],
			startingSpells: [],
			startingEquipment: [
				Ids.Weapons.Daggers.SteelDagger,
				Ids.Armor.Light.LeatherArmor
			],
			startingGold: 15,
			startsUnlocked: false,
			unlockHint: "Complete 10 stealth kills",
			requiredFogClears: 2
		));

		// ═══════════════════════════════════════════════════════════════
		// WITCH HUNTER (WIS/STR)
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new CharacterClassProto(
			id: Ids.CharacterClasses.WitchHunter,
			text: Proto.CreateText("Witch Hunter", "Trained from youth to detect and destroy magical threats. Witch Hunters are immune to many magical effects."),
			tagline: "Magic is not power. It is corruption.",
			iconName: "icon_class_witchhunter",
			portraitName: "portrait_witchhunter",
			hitDice: HitDice.D10,
			baseHealth: 20,
			baseMana: 0,
			baseArmorClass: 10,
			baseAttributes: new ClassAttributes {
				Strength = 14,
				Dexterity = 12,
				Constitution = 12,
				Intelligence = 10,
				Wisdom = 14,
				Charisma = 10
			},
			healthPerLevel: 5,
			manaPerLevel: 0,
			primaryAttribute: Ids.Stats.Attributes.Wisdom,
			secondaryAttribute: Ids.Stats.Attributes.Strength,
			spellcastingType: SpellcastingType.None,
			role: ClassRole.Melee,
			difficulty: 3,
			weaponProficiencies: [WeaponType.Sword, WeaponType.Crossbow, WeaponType.Dagger],
			armorProficiencies: [ArmorType.Light, ArmorType.Medium],
			startingSkillSchools: [
				Ids.Tags.Meta.CombatSkill,
				Ids.Tags.Meta.KnowledgeSkill
			],
			startingSpellSchools: [],
			startingSkills: [
				Ids.Skills.Combat.OneHandedWeapons,
				Ids.Skills.Knowledge.Arcana,
				Ids.Skills.Awareness.Perception
			],
			startingSpells: [],
			startingEquipment: [
				Ids.Weapons.Swords.IronSword,
				Ids.Armor.Medium.ChainMail
			],
			startingGold: 15,
			startsUnlocked: false,
			unlockHint: "Defeat 30 spellcasting enemies",
			requiredFogClears: 2
		));

		// ═══════════════════════════════════════════════════════════════
		// SUMMONER (CHA/INT)
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new CharacterClassProto(
			id: Ids.CharacterClasses.Summoner,
			text: Proto.CreateText("Summoner", "A mage who forms pacts with creatures beyond the veil, calling them forth to serve in battle."),
			tagline: "I am never alone.",
			iconName: "icon_class_summoner",
			portraitName: "portrait_summoner",
			hitDice: HitDice.D6,
			baseHealth: 12,
			baseMana: 18,
			baseArmorClass: 10,
			baseAttributes: new ClassAttributes {
				Strength = 8,
				Dexterity = 10,
				Constitution = 12,
				Intelligence = 14,
				Wisdom = 10,
				Charisma = 16
			},
			healthPerLevel: 3,
			manaPerLevel: 5,
			primaryAttribute: Ids.Stats.Attributes.Charisma,
			secondaryAttribute: Ids.Stats.Attributes.Intelligence,
			spellcastingType: SpellcastingType.Arcane,
			role: ClassRole.Caster,
			difficulty: 4,
			weaponProficiencies: [WeaponType.Staff, WeaponType.Dagger],
			armorProficiencies: [ArmorType.Cloth],
			startingSkillSchools: [
				Ids.Tags.Meta.MagicSkill,
				Ids.Tags.Meta.KnowledgeSkill
			],
			startingSpellSchools: [
				Ids.Tags.School.Conjuration
			],
			startingSkills: [
				Ids.Skills.Knowledge.Planes,
				Ids.Skills.Magic.Conjuration
			],
			startingSpells: [],
			startingEquipment: [
				Ids.Weapons.Staves.ApprenticeStaff,
				Ids.Armor.Cloth.ClothRobe,
				Ids.Items.Accessories.Spellbook
			],
			startingGold: 10,
			startsUnlocked: false,
			unlockHint: "Summon 25 creatures across runs",
			requiredFogClears: 2
		));

		// ═══════════════════════════════════════════════════════════════
		// ORACLE (WIS/INT)
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new CharacterClassProto(
			id: Ids.CharacterClasses.Oracle,
			text: Proto.CreateText("Oracle", "Blessed—or cursed—with visions of what may come. Oracles guide fate itself, seeing paths others cannot."),
			tagline: "I have seen what comes.",
			iconName: "icon_class_oracle",
			portraitName: "portrait_oracle",
			hitDice: HitDice.D6,
			baseHealth: 12,
			baseMana: 20,
			baseArmorClass: 10,
			baseAttributes: new ClassAttributes {
				Strength = 8,
				Dexterity = 10,
				Constitution = 10,
				Intelligence = 14,
				Wisdom = 16,
				Charisma = 12
			},
			healthPerLevel: 3,
			manaPerLevel: 5,
			primaryAttribute: Ids.Stats.Attributes.Wisdom,
			secondaryAttribute: Ids.Stats.Attributes.Intelligence,
			spellcastingType: SpellcastingType.Divine,
			role: ClassRole.Support,
			difficulty: 4,
			weaponProficiencies: [WeaponType.Staff],
			armorProficiencies: [ArmorType.Cloth],
			startingSkillSchools: [
				Ids.Tags.Meta.MagicSkill,
				Ids.Tags.Meta.KnowledgeSkill,
				Ids.Tags.Meta.SocialSkill
			],
			startingSpellSchools: [
				Ids.Tags.School.Divination
			],
			startingSkills: [
				Ids.Skills.Magic.Divination,
				Ids.Skills.Awareness.Insight,
				Ids.Skills.Knowledge.Religion
			],
			startingSpells: [],
			startingEquipment: [
				Ids.Weapons.Staves.ApprenticeStaff,
				Ids.Armor.Cloth.ClothRobe
			],
			startingGold: 15,
			startsUnlocked: false,
			unlockHint: "Discover 10 pieces of lore",
			requiredFogClears: 2
		));

		// ═══════════════════════════════════════════════════════════════
		// ALCHEMIST (INT/CON)
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new CharacterClassProto(
			id: Ids.CharacterClasses.Alchemist,
			text: Proto.CreateText("Alchemist", "Part scientist, part mage. Alchemists understand the fundamental nature of matter and reshape it to their will."),
			tagline: "The world is my laboratory.",
			iconName: "icon_class_alchemist",
			portraitName: "portrait_alchemist",
			hitDice: HitDice.D8,
			baseHealth: 14,
			baseMana: 14,
			baseArmorClass: 10,
			baseAttributes: new ClassAttributes {
				Strength = 10,
				Dexterity = 12,
				Constitution = 12,
				Intelligence = 16,
				Wisdom = 12,
				Charisma = 8
			},
			healthPerLevel: 4,
			manaPerLevel: 4,
			primaryAttribute: Ids.Stats.Attributes.Intelligence,
			secondaryAttribute: Ids.Stats.Attributes.Constitution,
			spellcastingType: SpellcastingType.Arcane,
			role: ClassRole.Hybrid,
			difficulty: 3,
			weaponProficiencies: [WeaponType.Dagger, WeaponType.Crossbow],
			armorProficiencies: [ArmorType.Light],
			startingSkillSchools: [
				Ids.Tags.Meta.CraftingSkill,
				Ids.Tags.Meta.MagicSkill,
				Ids.Tags.Meta.KnowledgeSkill
			],
			startingSpellSchools: [
				Ids.Tags.School.Transmutation
			],
			startingSkills: [
				Ids.Skills.Crafting.Alchemy,
				Ids.Skills.Magic.Transmutation,
				Ids.Skills.Gathering.Herbalism
			],
			startingSpells: [],
			startingEquipment: [
				Ids.Weapons.Daggers.SteelDagger,
				Ids.Armor.Light.LeatherArmor
			],
			startingGold: 20,
			startsUnlocked: false,
			unlockHint: "Craft 50 potions",
			requiredFogClears: 1
		));

		// ═══════════════════════════════════════════════════════════════
		// BLOOD MAGE (CON/INT)
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new CharacterClassProto(
			id: Ids.CharacterClasses.BloodMage,
			text: Proto.CreateText("Blood Mage", "Practitioners of forbidden arts, Blood Mages sacrifice their own vitality to fuel devastating magic."),
			tagline: "Power demands sacrifice.",
			iconName: "icon_class_bloodmage",
			portraitName: "portrait_bloodmage",
			hitDice: HitDice.D8,
			baseHealth: 18,
			baseMana: 10,
			baseArmorClass: 10,
			baseAttributes: new ClassAttributes {
				Strength = 10,
				Dexterity = 10,
				Constitution = 16,
				Intelligence = 14,
				Wisdom = 10,
				Charisma = 10
			},
			healthPerLevel: 5,
			manaPerLevel: 3,
			primaryAttribute: Ids.Stats.Attributes.Constitution,
			secondaryAttribute: Ids.Stats.Attributes.Intelligence,
			spellcastingType: SpellcastingType.Arcane,
			role: ClassRole.Caster,
			difficulty: 5,
			weaponProficiencies: [WeaponType.Dagger, WeaponType.Staff],
			armorProficiencies: [ArmorType.Light],
			startingSkillSchools: [
				Ids.Tags.Meta.MagicSkill,
				Ids.Tags.Meta.KnowledgeSkill
			],
			startingSpellSchools: [
				Ids.Tags.School.Blood
			],
			startingSkills: [
				Ids.Skills.Magic.BloodMagic,
				Ids.Skills.Knowledge.Arcana
			],
			startingSpells: [],
			bonusCreationPoints: -5,
			startingEquipment: [
				Ids.Weapons.Daggers.SteelDagger,
				Ids.Armor.Light.LeatherArmor
			],
			startingGold: 8,
			startsUnlocked: false,
			unlockHint: "Spend 500 HP on spell costs",
			requiredFogClears: 3
		));
	}

	#endregion

	#region Fog-Themed Classes

	private void RegisterFogThemedClasses(GameDb db) {
		// ═══════════════════════════════════════════════════════════════
		// FOG WALKER (WIS/DEX) - Fog-Themed
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new CharacterClassProto(
			id: Ids.CharacterClasses.FogWalker,
			text: Proto.CreateText("Fog Walker", "One who has embraced the Fog rather than feared it. Fog Walkers see what others cannot and move unseen through the mist."),
			tagline: "The Fog whispers secrets.",
			iconName: "icon_class_fogwalker",
			portraitName: "portrait_fogwalker",
			hitDice: HitDice.D8,
			baseHealth: 16,
			baseMana: 14,
			baseArmorClass: 11,
			baseAttributes: new ClassAttributes {
				Strength = 10,
				Dexterity = 14,
				Constitution = 12,
				Intelligence = 12,
				Wisdom = 14,
				Charisma = 10
			},
			healthPerLevel: 4,
			manaPerLevel: 4,
			primaryAttribute: Ids.Stats.Attributes.Wisdom,
			secondaryAttribute: Ids.Stats.Attributes.Dexterity,
			spellcastingType: SpellcastingType.Arcane,
			role: ClassRole.Hybrid,
			difficulty: 4,
			weaponProficiencies: [WeaponType.Dagger, WeaponType.Staff, WeaponType.Crossbow],
			armorProficiencies: [ArmorType.Light],
			startingSkillSchools: [
				Ids.Tags.Meta.StealthSkill,
				Ids.Tags.Meta.MagicSkill,
				Ids.Tags.Meta.SurvivalSkill
			],
			startingSpellSchools: [
				Ids.Tags.School.Divination,
				Ids.Tags.School.Illusion
			],
			startingSkills: [
				Ids.Skills.Knowledge.FogLore,
				Ids.Skills.Stealth.Hiding,
				Ids.Skills.Awareness.Perception
			],
			startingSpells: [],
			startingEquipment: [
				Ids.Weapons.Daggers.SteelDagger,
				Ids.Armor.Light.LeatherArmor
			],
			startingGold: 15,
			startsUnlocked: false,
			unlockHint: "Survive 10 days in Deep Fog regions",
			requiredFogClears: 3
		));

		// ═══════════════════════════════════════════════════════════════
		// FOG WARDEN (CON/WIS) - Fog-Themed
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new CharacterClassProto(
			id: Ids.CharacterClasses.FogWarden,
			text: Proto.CreateText("Fog Warden", "Sworn protectors who stand at the boundary between civilization and the encroaching Fog. They wield light against the darkness."),
			tagline: "The last light before the mist.",
			iconName: "icon_class_fogwarden",
			portraitName: "portrait_fogwarden",
			hitDice: HitDice.D10,
			baseHealth: 22,
			baseMana: 8,
			baseArmorClass: 10,
			baseAttributes: new ClassAttributes {
				Strength = 14,
				Dexterity = 10,
				Constitution = 14,
				Intelligence = 10,
				Wisdom = 14,
				Charisma = 10
			},
			healthPerLevel: 6,
			manaPerLevel: 2,
			primaryAttribute: Ids.Stats.Attributes.Constitution,
			secondaryAttribute: Ids.Stats.Attributes.Wisdom,
			spellcastingType: SpellcastingType.Divine,
			role: ClassRole.Tank,
			difficulty: 2,
			weaponProficiencies: [WeaponType.Sword, WeaponType.Spear, WeaponType.Shield, WeaponType.Crossbow],
			armorProficiencies: [ArmorType.Light, ArmorType.Medium, ArmorType.Heavy],
			startingSkillSchools: [
				Ids.Tags.Meta.CombatSkill,
				Ids.Tags.Meta.SurvivalSkill
			],
			startingSpellSchools: [
				Ids.Tags.School.Abjuration
			],
			startingSkills: [
				Ids.Skills.Combat.Shields,
				Ids.Skills.Knowledge.FogLore,
				Ids.Skills.Survival.General
			],
			startingSpells: [
				Ids.Spells.Abjuration.ArcaneShield
			],
			bonusCreationPoints: 5,
			startingEquipment: [
				Ids.Weapons.Swords.IronSword,
				Ids.Armor.Medium.ChainMail,
				Ids.Armor.Shields.IronShield
			],
			startingGold: 12,
			startsUnlocked: false,
			unlockHint: "Clear 20 Fog-corrupted enemies",
			requiredFogClears: 2
		));
	}

	#endregion

	#region Secret Classes

	private void RegisterSecretClasses(GameDb db) {
		// ═══════════════════════════════════════════════════════════════
		// ASCENDED (WIS/INT) - Secret Class
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new CharacterClassProto(
			id: Ids.CharacterClasses.Ascended,
			text: Proto.CreateText("Ascended", "One who has broken free of the time loop and glimpsed the truth. Wields power from beyond."),
			tagline: "Beyond time. Beyond fate.",
			iconName: "icon_class_ascended",
			portraitName: "portrait_ascended",
			hitDice: HitDice.D10,
			baseHealth: 20,
			baseMana: 15,
			baseArmorClass: 12,
			baseAttributes: new ClassAttributes {
				Strength = 12,
				Dexterity = 12,
				Constitution = 12,
				Intelligence = 14,
				Wisdom = 14,
				Charisma = 12
			},
			healthPerLevel: 5,
			manaPerLevel: 4,
			primaryAttribute: Ids.Stats.Attributes.Wisdom,
			secondaryAttribute: Ids.Stats.Attributes.Intelligence,
			spellcastingType: SpellcastingType.Arcane,
			role: ClassRole.Hybrid,
			difficulty: 5,
			weaponProficiencies: [WeaponType.Sword, WeaponType.Staff, WeaponType.Dagger],
			armorProficiencies: [ArmorType.Light, ArmorType.Medium],
			startingSkillSchools: [
				Ids.Tags.Meta.CombatSkill,
				Ids.Tags.Meta.MagicSkill,
				Ids.Tags.Meta.KnowledgeSkill
			],
			startingSpellSchools: [
				Ids.Tags.School.Temporal,
				Ids.Tags.School.Evocation
			],
			startingSkills: [
				Ids.Skills.Knowledge.TimeLore,
				Ids.Skills.Magic.Temporal
			],
			startingSpells: [
				Ids.Spells.Temporal.FateSight
			],
			bonusCreationPoints: -10,
			startingEquipment: [
				Ids.Weapons.Swords.TemporalBlade,
				Ids.Armor.Cloth.TimeWardenRobe
			],
			startingGold: 50,
			startsUnlocked: false,
			unlockHint: "Achieve the true ending",
			requiredFogClears: 5
		));

		// ═══════════════════════════════════════════════════════════════
		// TIME WALKER (INT/WIS) - Secret Class
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new CharacterClassProto(
			id: Ids.CharacterClasses.TimeWalker,
			text: Proto.CreateText("Time Walker", "A being who exists between moments, walking the edges of time itself. Masters temporal magic."),
			tagline: "Time bends to my will.",
			iconName: "icon_class_timewalker",
			portraitName: "portrait_timewalker",
			hitDice: HitDice.D8,
			baseHealth: 16,
			baseMana: 20,
			baseArmorClass: 11,
			baseAttributes: new ClassAttributes {
				Strength = 10,
				Dexterity = 14,
				Constitution = 10,
				Intelligence = 16,
				Wisdom = 14,
				Charisma = 10
			},
			healthPerLevel: 4,
			manaPerLevel: 5,
			primaryAttribute: Ids.Stats.Attributes.Intelligence,
			secondaryAttribute: Ids.Stats.Attributes.Wisdom,
			spellcastingType: SpellcastingType.Arcane,
			role: ClassRole.Caster,
			difficulty: 5,
			weaponProficiencies: [WeaponType.Staff, WeaponType.Dagger],
			armorProficiencies: [ArmorType.Cloth, ArmorType.Light],
			startingSkillSchools: [
				Ids.Tags.Meta.MagicSkill,
				Ids.Tags.Meta.KnowledgeSkill
			],
			startingSpellSchools: [
				Ids.Tags.School.Temporal
			],
			startingSkills: [
				Ids.Skills.Knowledge.TimeLore,
				Ids.Skills.Magic.Temporal,
				Ids.Skills.Knowledge.Arcana
			],
			startingSpells: [
				Ids.Spells.Temporal.TimeEcho,
				Ids.Spells.Temporal.FateSight
			],
			bonusCreationPoints: -15,
			startingEquipment: [
				Ids.Weapons.Staves.ArcaneStaff,
				Ids.Armor.Cloth.TimeWardenRobe
			],
			startingGold: 30,
			startsUnlocked: false,
			unlockHint: "Master all temporal spells",
			requiredFogClears: 7
		));

		// ═══════════════════════════════════════════════════════════════
		// VOID TOUCHED (CHA/CON) - Secret Class (Fog-Themed)
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new CharacterClassProto(
			id: Ids.CharacterClasses.VoidTouched,
			text: Proto.CreateText("Void Touched", "The Fog changed you. Now you channel the void between worlds, wielding entropy itself as a weapon."),
			tagline: "Embrace the nothing.",
			iconName: "icon_class_voidtouched",
			portraitName: "portrait_voidtouched",
			hitDice: HitDice.D6,
			baseHealth: 14,
			baseMana: 22,
			baseArmorClass: 10,
			baseAttributes: new ClassAttributes {
				Strength = 8,
				Dexterity = 12,
				Constitution = 14,
				Intelligence = 14,
				Wisdom = 10,
				Charisma = 14
			},
			healthPerLevel: 3,
			manaPerLevel: 6,
			primaryAttribute: Ids.Stats.Attributes.Charisma,
			secondaryAttribute: Ids.Stats.Attributes.Constitution,
			spellcastingType: SpellcastingType.Arcane,
			role: ClassRole.Caster,
			difficulty: 5,
			weaponProficiencies: [WeaponType.Staff, WeaponType.Dagger],
			armorProficiencies: [ArmorType.Cloth],
			startingSkillSchools: [
				Ids.Tags.Meta.MagicSkill,
				Ids.Tags.Meta.KnowledgeSkill
			],
			startingSpellSchools: [
				Ids.Tags.School.Necromancy,
				Ids.Tags.School.Conjuration
			],
			startingSkills: [
				Ids.Skills.Knowledge.FogLore,
				Ids.Skills.Knowledge.Planes,
				Ids.Skills.Magic.Necromancy
			],
			startingSpells: [
				Ids.Spells.Necromancy.LifeDrain
			],
			bonusCreationPoints: -20,
			startingEquipment: [
				Ids.Weapons.Staves.ApprenticeStaff,
				Ids.Armor.Cloth.ClothRobe
			],
			startingGold: 5,
			startsUnlocked: false,
			unlockHint: "Die to Fog corruption 3 times",
			requiredFogClears: 4
		));
	}

	#endregion
}
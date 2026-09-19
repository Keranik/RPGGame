using RPGGame.Core.Combat;
using RPGGame.Core.Prototypes;
using RPGGame.Core.Prototypes.Item;
using RPGGame.Core.Prototypes.Item.Equipment.Armor;
using RPGGame.Core.Prototypes.Item.Equipment.Weapon;
using RPGGame.Core.Prototypes.Item.Resource;
using RPGGame.Core.Stats;

namespace RPGGame.Core.Items;

/// <summary>
/// Defines all base game items using the Proto system.
/// </summary>
public class ItemDefinitions : ICoreData {
	public void GameData(GameDb gameDatabase) {
		RegisterWeapons(gameDatabase);
		RegisterArmor(gameDatabase);
		RegisterConsumables(gameDatabase);
		RegisterMaterials(gameDatabase);
		RegisterAccessories(gameDatabase);
		RegisterQuestItems(gameDatabase);
	}

	#region Weapons

	private void RegisterWeapons(GameDb db) {
		// ═══════════════════════════════════════════════════════════════
		// SWORDS
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new WeaponProto(
			id: Ids.Weapons.Swords.IronSword,
			text: Proto.CreateText("Iron Sword", "A simple but reliable iron sword."),
			iconName: "icon_sword_iron",
			rarity: RarityType.Common,
			buyPrice: 50,
			itemLevel: 1,
			equipSlot: SlotType.MainHand,
			weaponType: WeaponType.Sword,
			damageDice: HitDice.D8,
			damageType: DamageType.Slashing,
			equipStats: [EquipmentStat.Flat(Ids.Stats.Combat.AttackBonus, 1)]
		));

		db.RegisterProto(new WeaponProto(
			id: Ids.Weapons.Swords.SteelSword,
			text: Proto.CreateText("Steel Sword", "A well-crafted steel sword with a keen edge."),
			iconName: "icon_sword_steel",
			rarity: RarityType.Uncommon,
			buyPrice: 150,
			itemLevel: 5,
			requiredLevel: 3,
			equipSlot: SlotType.MainHand,
			weaponType: WeaponType.Sword,
			damageDice: new HitDice(1, 8, 1),
			damageType: DamageType.Slashing,
			equipStats: [
				EquipmentStat.Flat(Ids.Stats.Combat.AttackBonus, 2),
				EquipmentStat.Flat(Ids.Stats.Combat.DamBonus, 1)
			]
		));

		db.RegisterProto(new WeaponProto(
			id: Ids.Weapons.Swords.TemporalBlade,
			text: Proto.CreateText("Temporal Blade", "A blade that seems to exist in multiple moments at once."),
			iconName: "icon_sword_temporal",
			rarity: RarityType.Legendary,
			buyPrice: 5000,
			itemLevel: 15,
			requiredLevel: 10,
			equipSlot: SlotType.MainHand,
			weaponType: WeaponType.Sword,
			damageDice: new HitDice(2, 6, 3),
			damageType: DamageType.Slashing,
			equipStats: [
				EquipmentStat.Flat(Ids.Stats.Combat.AttackBonus, 4),
				EquipmentStat.Flat(Ids.Stats.Combat.DamBonus, 3),
				EquipmentStat.Flat(Ids.Stats.Combat.Initiative, 5)
			],
			flavorText: "Time flows strangely around its edge.",
			classRestrictions: [Ids.CharacterClasses.Ascended, Ids.CharacterClasses.TimeWalker]
		));

		db.RegisterProto(new WeaponProto(
			id: Ids.Weapons.Swords.ShortSword,
			text: Proto.CreateText("Short Sword", "A light, quick blade favored by agile fighters."),
			iconName: "icon_sword_short",
			rarity: RarityType.Common,
			buyPrice: 35,
			itemLevel: 1,
			equipSlot: SlotType.MainHand,
			weaponType: WeaponType.Sword,
			damageDice: HitDice.D6,
			damageType: DamageType.Slashing,
			equipStats: [
				EquipmentStat.Flat(Ids.Stats.Combat.AttackBonus, 1),
				EquipmentStat.Flat(Ids.Stats.Combat.Initiative, 1)
			]
		));

		// ═══════════════════════════════════════════════════════════════
		// DAGGERS
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new WeaponProto(
			id: Ids.Weapons.Daggers.SteelDagger,
			text: Proto.CreateText("Steel Dagger", "A quick and deadly dagger."),
			iconName: "icon_dagger_steel",
			rarity: RarityType.Common,
			buyPrice: 30,
			itemLevel: 1,
			equipSlot: SlotType.MainHand,
			weaponType: WeaponType.Dagger,
			damageDice: HitDice.D4,
			damageType: DamageType.Piercing,
			equipStats: [
				EquipmentStat.Flat(Ids.Stats.Combat.AttackBonus, 2),
				EquipmentStat.Flat(Ids.Stats.Combat.Initiative, 2)
			]
		));

		db.RegisterProto(new WeaponProto(
			id: Ids.Weapons.Daggers.PoisonedDagger,
			text: Proto.CreateText("Poisoned Dagger", "A dagger coated in deadly venom."),
			iconName: "icon_dagger_poison",
			rarity: RarityType.Rare,
			buyPrice: 200,
			itemLevel: 6,
			requiredLevel: 4,
			equipSlot: SlotType.MainHand,
			weaponType: WeaponType.Dagger,
			damageDice: HitDice.D4,
			damageType: DamageType.Piercing,
			equipStats: [
				EquipmentStat.Flat(Ids.Stats.Combat.AttackBonus, 2),
				EquipmentStat.Flat(Ids.Stats.Combat.Initiative, 2),
				EquipmentStat.Flat(Ids.Stats.Combat.DamBonus, 2)
			],
			flavorText: "One scratch is all it takes."
		));

		// ═══════════════════════════════════════════════════════════════
		// AXES
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new WeaponProto(
			id: Ids.Weapons.Axes.IronAxe,
			text: Proto.CreateText("Iron Axe", "A heavy iron axe."),
			iconName: "icon_axe_iron",
			rarity: RarityType.Common,
			buyPrice: 60,
			itemLevel: 1,
			equipSlot: SlotType.MainHand,
			weaponType: WeaponType.Axe,
			damageDice: HitDice.D10,
			damageType: DamageType.Slashing,
			equipStats: [EquipmentStat.Flat(Ids.Stats.Combat.DamBonus, 1)]
		));

		db.RegisterProto(new WeaponProto(
			id: Ids.Weapons.Axes.GreatAxe,
			text: Proto.CreateText("Great Axe", "A massive two-handed axe."),
			iconName: "icon_axe_great",
			rarity: RarityType.Uncommon,
			buyPrice: 200,
			itemLevel: 5,
			requiredLevel: 4,
			equipSlot: SlotType.MainHand,
			weaponType: WeaponType.Axe,
			damageDice: new HitDice(1, 12, 2),
			damageType: DamageType.Slashing,
			isTwoHanded: true,
			equipStats: [
				EquipmentStat.Flat(Ids.Stats.Combat.DamBonus, 3),
				EquipmentStat.Flat(Ids.Stats.Combat.AttackBonus, -1)
			]
		));

		// ═══════════════════════════════════════════════════════════════
		// STAVES
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new WeaponProto(
			id: Ids.Weapons.Staves.ApprenticeStaff,
			text: Proto.CreateText("Apprentice Staff", "A simple wooden staff for novice mages."),
			iconName: "icon_staff_apprentice",
			rarity: RarityType.Common,
			buyPrice: 40,
			itemLevel: 1,
			equipSlot: SlotType.MainHand,
			weaponType: WeaponType.Staff,
			damageDice: HitDice.D6,
			damageType: DamageType.Bludgeoning,
			isTwoHanded: true,
			equipStats: [
				EquipmentStat.Flat(Ids.Stats.Resource.MaxMana, 5),
				EquipmentStat.Flat(Ids.Stats.Combat.SpellPower, 1)
			]
		));

		db.RegisterProto(new WeaponProto(
			id: Ids.Weapons.Staves.ArcaneStaff,
			text: Proto.CreateText("Arcane Staff", "A staff imbued with magical energy."),
			iconName: "icon_staff_arcane",
			rarity: RarityType.Rare,
			buyPrice: 500,
			itemLevel: 8,
			requiredLevel: 6,
			equipSlot: SlotType.MainHand,
			weaponType: WeaponType.Staff,
			damageDice: HitDice.D8,
			damageType: DamageType.Arcane,
			isTwoHanded: true,
			equipStats: [
				EquipmentStat.Flat(Ids.Stats.Resource.MaxMana, 15),
				EquipmentStat.Flat(Ids.Stats.Combat.SpellPower, 5),
				EquipmentStat.Flat(Ids.Stats.Regeneration.Mana, 1)
			]
		));

		db.RegisterProto(new WeaponProto(
			id: Ids.Weapons.Staves.WoodenStaff,
			text: Proto.CreateText("Wooden Staff", "A sturdy wooden staff connected to nature."),
			iconName: "icon_staff_wooden",
			rarity: RarityType.Common,
			buyPrice: 35,
			itemLevel: 1,
			equipSlot: SlotType.MainHand,
			weaponType: WeaponType.Staff,
			damageDice: HitDice.D6,
			damageType: DamageType.Bludgeoning,
			isTwoHanded: true,
			equipStats: [
				EquipmentStat.Flat(Ids.Stats.Resource.MaxMana, 3),
				EquipmentStat.Flat(Ids.Stats.Attributes.Wisdom, 1)
			]
		));

		// ═══════════════════════════════════════════════════════════════
		// BOWS
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new WeaponProto(
			id: Ids.Weapons.Bows.HuntingBow,
			text: Proto.CreateText("Hunting Bow", "A reliable bow for hunting game."),
			iconName: "icon_bow_hunting",
			rarity: RarityType.Common,
			buyPrice: 45,
			itemLevel: 1,
			equipSlot: SlotType.MainHand,
			weaponType: WeaponType.Bow,
			damageDice: HitDice.D6,
			damageType: DamageType.Piercing,
			isTwoHanded: true,
			range: 30,
			requiredAmmo: AmmoType.Arrows,
			equipStats: [EquipmentStat.Flat(Ids.Stats.Combat.AttackBonus, 1)]
		));

		db.RegisterProto(new WeaponProto(
			id: Ids.Weapons.Bows.Longbow,
			text: Proto.CreateText("Longbow", "A powerful bow with excellent range."),
			iconName: "icon_bow_long",
			rarity: RarityType.Uncommon,
			buyPrice: 150,
			itemLevel: 5,
			requiredLevel: 3,
			equipSlot: SlotType.MainHand,
			weaponType: WeaponType.Bow,
			damageDice: HitDice.D8,
			damageType: DamageType.Piercing,
			isTwoHanded: true,
			range: 50,
			requiredAmmo: AmmoType.Arrows,
			equipStats: [
				EquipmentStat.Flat(Ids.Stats.Combat.AttackBonus, 2),
				EquipmentStat.Flat(Ids.Stats.Combat.DamBonus, 1)
			]
		));

		// ═══════════════════════════════════════════════════════════════
		// MACES
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new WeaponProto(
			id: Ids.Weapons.Maces.IronMace,
			text: Proto.CreateText("Iron Mace", "A sturdy iron mace blessed for holy work."),
			iconName: "icon_mace_iron",
			rarity: RarityType.Common,
			buyPrice: 55,
			itemLevel: 1,
			equipSlot: SlotType.MainHand,
			weaponType: WeaponType.Mace,
			damageDice: HitDice.D6,
			damageType: DamageType.Bludgeoning,
			equipStats: [
				EquipmentStat.Flat(Ids.Stats.Combat.AttackBonus, 1),
				EquipmentStat.Flat(Ids.Stats.Combat.SpellPower, 1)
			]
		));

		db.RegisterProto(new WeaponProto(
			id: Ids.Weapons.Maces.HolyMace,
			text: Proto.CreateText("Holy Mace", "A mace blessed by divine power."),
			iconName: "icon_mace_holy",
			rarity: RarityType.Rare,
			buyPrice: 350,
			itemLevel: 7,
			requiredLevel: 5,
			equipSlot: SlotType.MainHand,
			weaponType: WeaponType.Mace,
			damageDice: HitDice.D8,
			damageType: DamageType.Holy,
			equipStats: [
				EquipmentStat.Flat(Ids.Stats.Combat.AttackBonus, 2),
				EquipmentStat.Flat(Ids.Stats.DamageBonus.Holy, 3),
				EquipmentStat.Flat(Ids.Stats.Combat.SpellPower, 2)
			],
			flavorText: "Light made manifest.",
			classRestrictions: [Ids.CharacterClasses.Cleric, Ids.CharacterClasses.Paladin]
		));
	}

	#endregion

	#region Armor

	private void RegisterArmor(GameDb db) {
		// ═══════════════════════════════════════════════════════════════
		// CLOTH
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new ArmorProto(
			id: Ids.Armor.Cloth.ClothRobe,
			text: Proto.CreateText("Cloth Robe", "Simple cloth robes."),
			iconName: "icon_armor_cloth_robe",
			rarity: RarityType.Common,
			buyPrice: 20,
			itemLevel: 1,
			equipSlot: SlotType.Chest,
			armorType: ArmorType.Cloth,
			armorBonus: 1,
			equipStats: [
				EquipmentStat.Flat(Ids.Stats.Combat.ArmorClass, 1),
				EquipmentStat.Flat(Ids.Stats.Resource.MaxMana, 5)
			]
		));

		db.RegisterProto(new ArmorProto(
			id: Ids.Armor.Cloth.TimeWardenRobe,
			text: Proto.CreateText("Time Warden Robe", "Robes worn by those who guard the timestream."),
			iconName: "icon_armor_time_warden",
			rarity: RarityType.Legendary,
			buyPrice: 3000,
			itemLevel: 15,
			requiredLevel: 10,
			equipSlot: SlotType.Chest,
			armorType: ArmorType.Cloth,
			armorBonus: 4,
			equipStats: [
				EquipmentStat.Flat(Ids.Stats.Combat.ArmorClass, 4),
				EquipmentStat.Flat(Ids.Stats.Resource.MaxMana, 30),
				EquipmentStat.Flat(Ids.Stats.Combat.SpellPower, 8),
				EquipmentStat.Flat(Ids.Stats.Combat.Initiative, 5)
			],
			flavorText: "The fabric shimmers between moments.",
			classRestrictions: [Ids.CharacterClasses.Ascended, Ids.CharacterClasses.TimeWalker]
		));

		db.RegisterProto(new ArmorProto(
			id: Ids.Armor.Cloth.MageRobe,
			text: Proto.CreateText("Mage Robe", "Robes enchanted to enhance magical power."),
			iconName: "icon_armor_mage_robe",
			rarity: RarityType.Uncommon,
			buyPrice: 100,
			itemLevel: 4,
			requiredLevel: 3,
			equipSlot: SlotType.Chest,
			armorType: ArmorType.Cloth,
			armorBonus: 2,
			equipStats: [
				EquipmentStat.Flat(Ids.Stats.Combat.ArmorClass, 2),
				EquipmentStat.Flat(Ids.Stats.Resource.MaxMana, 10),
				EquipmentStat.Flat(Ids.Stats.Combat.SpellPower, 2)
			]
		));

		// ═══════════════════════════════════════════════════════════════
		// LEATHER
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new ArmorProto(
			id: Ids.Armor.Light.LeatherArmor,
			text: Proto.CreateText("Leather Armor", "Flexible leather armor."),
			iconName: "icon_armor_leather",
			rarity: RarityType.Common,
			buyPrice: 40,
			itemLevel: 1,
			equipSlot: SlotType.Chest,
			armorType: ArmorType.Light,
			armorBonus: 2,
			maxDexBonus: 6,
			equipStats: [EquipmentStat.Flat(Ids.Stats.Combat.ArmorClass, 2)]
		));

		db.RegisterProto(new ArmorProto(
			id: Ids.Armor.Light.StuddedLeather,
			text: Proto.CreateText("Studded Leather", "Leather armor reinforced with metal studs."),
			iconName: "icon_armor_studded_leather",
			rarity: RarityType.Uncommon,
			buyPrice: 100,
			itemLevel: 4,
			requiredLevel: 3,
			equipSlot: SlotType.Chest,
			armorType: ArmorType.Light,
			armorBonus: 3,
			maxDexBonus: 5,
			equipStats: [
				EquipmentStat.Flat(Ids.Stats.Combat.ArmorClass, 3),
				EquipmentStat.Flat(Ids.Stats.Resistances.Physical, 3)
			]
		));

		// ═══════════════════════════════════════════════════════════════
		// CHAIN
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new ArmorProto(
			id: Ids.Armor.Medium.ChainMail,
			text: Proto.CreateText("Chain Mail", "Interlocking metal rings provide solid protection."),
			iconName: "icon_armor_chain",
			rarity: RarityType.Common,
			buyPrice: 80,
			itemLevel: 3,
			requiredLevel: 2,
			equipSlot: SlotType.Chest,
			armorType: ArmorType.Medium,
			armorBonus: 4,
			maxDexBonus: 2,
			hasStealthPenalty: true,
			equipStats: [EquipmentStat.Flat(Ids.Stats.Combat.ArmorClass, 4)]
		));

		db.RegisterProto(new ArmorProto(
			id: Ids.Armor.Medium.ScaleMail,
			text: Proto.CreateText("Scale Mail", "Overlapping metal scales offer excellent protection."),
			iconName: "icon_armor_scale",
			rarity: RarityType.Uncommon,
			buyPrice: 150,
			itemLevel: 5,
			requiredLevel: 4,
			equipSlot: SlotType.Chest,
			armorType: ArmorType.Medium,
			armorBonus: 5,
			maxDexBonus: 2,
			hasStealthPenalty: true,
			equipStats: [
				EquipmentStat.Flat(Ids.Stats.Combat.ArmorClass, 5),
				EquipmentStat.Flat(Ids.Stats.Resistances.Physical, 5)
			]
		));

		// ═══════════════════════════════════════════════════════════════
		// PLATE
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new ArmorProto(
			id: Ids.Armor.Heavy.PlateArmor,
			text: Proto.CreateText("Plate Armor", "Heavy plate armor offering maximum protection."),
			iconName: "icon_armor_plate",
			rarity: RarityType.Rare,
			buyPrice: 500,
			itemLevel: 8,
			requiredLevel: 6,
			equipSlot: SlotType.Chest,
			armorType: ArmorType.Heavy,
			armorBonus: 8,
			maxDexBonus: 0,
			hasStealthPenalty: true,
			weight: 30f,
			equipStats: [
				EquipmentStat.Flat(Ids.Stats.Combat.ArmorClass, 8),
				EquipmentStat.Flat(Ids.Stats.Resistances.Physical, 10)
			]
		));

		// ═══════════════════════════════════════════════════════════════
		// HELMETS
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new ArmorProto(
			id: Ids.Armor.Helmets.LeatherCap,
			text: Proto.CreateText("Leather Cap", "A simple leather cap."),
			iconName: "icon_helm_leather",
			rarity: RarityType.Common,
			buyPrice: 15,
			itemLevel: 1,
			equipSlot: SlotType.Head,
			armorType: ArmorType.Light,
			armorBonus: 1,
			equipStats: [EquipmentStat.Flat(Ids.Stats.Combat.ArmorClass, 1)]
		));

		db.RegisterProto(new ArmorProto(
			id: Ids.Armor.Helmets.IronHelm,
			text: Proto.CreateText("Iron Helm", "A sturdy iron helmet."),
			iconName: "icon_helm_iron",
			rarity: RarityType.Common,
			buyPrice: 35,
			itemLevel: 3,
			equipSlot: SlotType.Head,
			armorType: ArmorType.Heavy,
			armorBonus: 2,
			equipStats: [
				EquipmentStat.Flat(Ids.Stats.Combat.ArmorClass, 2),
				EquipmentStat.Flat(Ids.Stats.Resistances.Physical, 2)
			]
		));

		// ═══════════════════════════════════════════════════════════════
		// SHIELDS
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new ArmorProto(
			id: Ids.Armor.Shields.WoodenShield,
			text: Proto.CreateText("Wooden Shield", "A simple wooden shield."),
			iconName: "icon_shield_wooden",
			rarity: RarityType.Common,
			buyPrice: 25,
			itemLevel: 1,
			equipSlot: SlotType.OffHand,
			armorType: ArmorType.Shield,
			armorBonus: 2,
			equipStats: [EquipmentStat.Flat(Ids.Stats.Combat.ArmorClass, 2)]
		));

		db.RegisterProto(new ArmorProto(
			id: Ids.Armor.Shields.IronShield,
			text: Proto.CreateText("Iron Shield", "A sturdy iron shield."),
			iconName: "icon_shield_iron",
			rarity: RarityType.Uncommon,
			buyPrice: 100,
			itemLevel: 4,
			requiredLevel: 3,
			equipSlot: SlotType.OffHand,
			armorType: ArmorType.Shield,
			armorBonus: 3,
			equipStats: [
				EquipmentStat.Flat(Ids.Stats.Combat.ArmorClass, 3),
				EquipmentStat.Flat(Ids.Stats.Resistances.Physical, 5)
			]
		));
	}

	#endregion

	#region Consumables

	private void RegisterConsumables(GameDb db) {
		// ═══════════════════════════════════════════════════════════════
		// POTIONS
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new ItemProto(
			id: Ids.Items.Consumables.HealthPotionSmall,
			text: Proto.CreateText("Small Health Potion", "Restores a small amount of health."),
			iconName: "icon_potion_health_small",
			category: ItemCategory.Consumable,
			type: ItemType.Potion,
			rarity: RarityType.Common,
			buyPrice: 25,
			isStackable: true,
			maxStackSize: 10,
			useEffects: [new ItemEffect { Type = ItemEffectType.Heal, Value = 20 }]
		));

		db.RegisterProto(new ItemProto(
			id: Ids.Items.Consumables.HealthPotionMedium,
			text: Proto.CreateText("Health Potion", "Restores health."),
			iconName: "icon_potion_health",
			category: ItemCategory.Consumable,
			type: ItemType.Potion,
			rarity: RarityType.Uncommon,
			buyPrice: 50,
			itemLevel: 5,
			isStackable: true,
			maxStackSize: 10,
			useEffects: [new ItemEffect { Type = ItemEffectType.Heal, Value = 50 }]
		));

		db.RegisterProto(new ItemProto(
			id: Ids.Items.Consumables.HealthPotionLarge,
			text: Proto.CreateText("Greater Health Potion", "Restores a large amount of health."),
			iconName: "icon_potion_health_large",
			category: ItemCategory.Consumable,
			type: ItemType.Potion,
			rarity: RarityType.Rare,
			buyPrice: 150,
			itemLevel: 10,
			isStackable: true,
			maxStackSize: 5,
			useEffects: [new ItemEffect { Type = ItemEffectType.Heal, Value = 100 }]
		));

		db.RegisterProto(new ItemProto(
			id: Ids.Items.Consumables.ManaPotionSmall,
			text: Proto.CreateText("Small Mana Potion", "Restores a small amount of mana."),
			iconName: "icon_potion_mana_small",
			category: ItemCategory.Consumable,
			type: ItemType.Potion,
			rarity: RarityType.Common,
			buyPrice: 30,
			isStackable: true,
			maxStackSize: 10,
			useEffects: [new ItemEffect { Type = ItemEffectType.RestoreMana, Value = 15 }]
		));

		db.RegisterProto(new ItemProto(
			id: Ids.Items.Consumables.Antidote,
			text: Proto.CreateText("Antidote", "Cures poison."),
			iconName: "icon_potion_antidote",
			category: ItemCategory.Consumable,
			type: ItemType.Potion,
			rarity: RarityType.Common,
			buyPrice: 35,
			isStackable: true,
			maxStackSize: 5,
			useEffects: [new ItemEffect { Type = ItemEffectType.CureCondition, Parameter = "Poisoned" }]
		));

		// ═══════════════════════════════════════════════════════════════
		// FOOD
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new ItemProto(
			id: Ids.Items.Consumables.Rations,
			text: Proto.CreateText("Rations", "One day's worth of preserved food."),
			iconName: "icon_food_rations",
			category: ItemCategory.Consumable,
			type: ItemType.Food,
			rarity: RarityType.Common,
			buyPrice: 5,
			isStackable: true,
			maxStackSize: 20,
			useEffects: [new ItemEffect { Type = ItemEffectType.AddFood, Value = 1 }]
		));

		db.RegisterProto(new ItemProto(
			id: Ids.Items.Consumables.CookedMeat,
			text: Proto.CreateText("Cooked Meat", "A hearty cooked meal."),
			iconName: "icon_food_meat",
			category: ItemCategory.Consumable,
			type: ItemType.Food,
			rarity: RarityType.Common,
			buyPrice: 10,
			isStackable: true,
			maxStackSize: 10,
			useEffects: [
				new ItemEffect { Type = ItemEffectType.AddFood, Value = 2 },
				new ItemEffect { Type = ItemEffectType.AddMorale, Value = 5 }
			]
		));

		// ═══════════════════════════════════════════════════════════════
		// UTILITY
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new ItemProto(
			id: Ids.Items.Consumables.Bandage,
			text: Proto.CreateText("Bandage", "Stops bleeding and provides minor healing."),
			iconName: "icon_bandage",
			category: ItemCategory.Consumable,
			type: ItemType.Medical,
			rarity: RarityType.Common,
			buyPrice: 15,
			isStackable: true,
			maxStackSize: 10,
			useEffects: [
				new ItemEffect { Type = ItemEffectType.CureCondition, Parameter = "Bleeding" },
				new ItemEffect { Type = ItemEffectType.Heal, Value = 5 }
			]
		));

		db.RegisterProto(new ItemProto(
			id: Ids.Items.Consumables.Torch,
			text: Proto.CreateText("Torch", "Provides light in dark areas."),
			iconName: "icon_torch",
			category: ItemCategory.Consumable,
			type: ItemType.Tool,
			rarity: RarityType.Common,
			buyPrice: 3,
			isStackable: true,
			maxStackSize: 10,
			maxUses: 8
		));

		db.RegisterProto(new ItemProto(
			id: Ids.Items.Consumables.CampingKit,
			text: Proto.CreateText("Camping Kit", "Essential supplies for setting up camp."),
			iconName: "icon_camping_kit",
			category: ItemCategory.Consumable,
			type: ItemType.Tool,
			rarity: RarityType.Common,
			buyPrice: 25,
			isStackable: true,
			maxStackSize: 3,
			useEffects: [new ItemEffect { Type = ItemEffectType.AddCampingSupply, Value = 1 }]
		));

		db.RegisterProto(new ItemProto(
			id: Ids.Items.Consumables.Lockpicks,
			text: Proto.CreateText("Lockpicks", "Tools for opening locks."),
			iconName: "icon_lockpicks",
			category: ItemCategory.Consumable,
			type: ItemType.Tool,
			rarity: RarityType.Uncommon,
			buyPrice: 50,
			isStackable: true,
			maxStackSize: 5,
			maxUses: 3,
			classRestrictions: [Ids.CharacterClasses.Rogue]
		));

		db.RegisterProto(new ItemProto(
				id: Ids.Items.Consumables.ManaPotionMedium,
				text: Proto.CreateText("Mana Potion", "Restores mana."),
				iconName: "icon_potion_mana",
				category: ItemCategory.Consumable,
				type: ItemType.Potion,
				rarity: RarityType.Uncommon,
				buyPrice: 60,
				itemLevel: 5,
				isStackable: true,
				maxStackSize: 10,
				useEffects: [new ItemEffect { Type = ItemEffectType.RestoreMana, Value = 40 }]
			));

		db.RegisterProto(new ItemProto(
				id: Ids.Items.Consumables.ManaPotionLarge,
				text: Proto.CreateText("Greater Mana Potion", "Restores a large amount of mana."),
				iconName: "icon_potion_mana_large",
				category: ItemCategory.Consumable,
				type: ItemType.Potion,
				rarity: RarityType.Rare,
				buyPrice: 180,
				itemLevel: 10,
				isStackable: true,
				maxStackSize: 5,
				useEffects: [new ItemEffect { Type = ItemEffectType.RestoreMana, Value = 80 }]
			));
	}

	#endregion

	#region Materials

private void RegisterMaterials(GameDb db) {
	// ═══════════════════════════════════════════════════════════════════
	// ORES & METALS
	// ═══════════════════════════════════════════════════════════════════
	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.IronOre,
		text: Proto.CreateText("Iron Ore", "Raw iron ore for smelting."),
		iconName: "icon_ore_iron",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 5,
		isStackable: true,
		maxStackSize: 50
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.CopperOre,
		text: Proto.CreateText("Copper Ore", "Raw copper ore, easy to smelt."),
		iconName: "icon_ore_copper",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 3,
		isStackable: true,
		maxStackSize: 50
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.SilverOre,
		text: Proto.CreateText("Silver Ore", "Precious silver ore with magical properties."),
		iconName: "icon_ore_silver",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Uncommon,
		buyPrice: 15,
		isStackable: true,
		maxStackSize: 30
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.GoldOre,
		text: Proto.CreateText("Gold Ore", "Valuable gold ore."),
		iconName: "icon_ore_gold",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Rare,
		buyPrice: 50,
		isStackable: true,
		maxStackSize: 20
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.MithrilOre,
		text: Proto.CreateText("Mithril Ore", "Legendary lightweight metal ore."),
		iconName: "icon_ore_mithril",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Epic,
		buyPrice: 200,
		isStackable: true,
		maxStackSize: 10
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.StarMetal,
		text: Proto.CreateText("Star Metal", "Metal from a fallen star, imbued with cosmic power."),
		iconName: "icon_ore_starmetal",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Legendary,
		buyPrice: 500,
		isStackable: true,
		maxStackSize: 5,
		flavorText: "Still warm from its journey through the heavens."
	));

	// ═══════════════════════════════════════════════════════════════════
	// STONE & MINERALS
	// ═══════════════════════════════════════════════════════════════════
	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.Stone,
		text: Proto.CreateText("Stone", "Common stone for building."),
		iconName: "icon_stone",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 1,
		isStackable: true,
		maxStackSize: 99
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.Sand,
		text: Proto.CreateText("Sand", "Fine sand useful for glassmaking."),
		iconName: "icon_sand",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 1,
		isStackable: true,
		maxStackSize: 99
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.Clay,
		text: Proto.CreateText("Clay", "Malleable clay for pottery and bricks."),
		iconName: "icon_clay",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 2,
		isStackable: true,
		maxStackSize: 50
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.Coal,
		text: Proto.CreateText("Coal", "Fuel for forges and furnaces."),
		iconName: "icon_coal",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 3,
		isStackable: true,
		maxStackSize: 50
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.Sulfite,
		text: Proto.CreateText("Sulfite", "A pungent mineral used in alchemy."),
		iconName: "icon_sulfite",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Uncommon,
		buyPrice: 10,
		isStackable: true,
		maxStackSize: 30
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.Obsidian,
		text: Proto.CreateText("Obsidian", "Volcanic glass, razor sharp."),
		iconName: "icon_obsidian",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Uncommon,
		buyPrice: 20,
		isStackable: true,
		maxStackSize: 30
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.Basalt,
		text: Proto.CreateText("Basalt", "Dark volcanic rock."),
		iconName: "icon_basalt",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 4,
		isStackable: true,
		maxStackSize: 50
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.MagicCrystal,
		text: Proto.CreateText("Magic Crystal", "A crystal pulsing with arcane energy."),
		iconName: "icon_crystal_magic",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Rare,
		buyPrice: 75,
		isStackable: true,
		maxStackSize: 20
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.CrystalShard,
		text: Proto.CreateText("Crystal Shard", "A fragment of a larger crystal."),
		iconName: "icon_crystal_shard",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Uncommon,
		buyPrice: 15,
		isStackable: true,
		maxStackSize: 30
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.FireCrystal,
		text: Proto.CreateText("Fire Crystal", "A crystal containing elemental fire."),
		iconName: "icon_crystal_fire",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Rare,
		buyPrice: 100,
		isStackable: true,
		maxStackSize: 15
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.CosmicDust,
		text: Proto.CreateText("Cosmic Dust", "Dust from beyond the stars."),
		iconName: "icon_cosmic_dust",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Legendary,
		buyPrice: 300,
		isStackable: true,
		maxStackSize: 10,
		flavorText: "Glitters with the light of distant galaxies."
	));

	// ═══════════════════════════════════════════════════════════════════
	// GEMS
	// ═══════════════════════════════════════════════════════════════════
	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.GemRuby,
		text: Proto.CreateText("Ruby", "A precious red gem."),
		iconName: "icon_gem_ruby",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Rare,
		buyPrice: 200,
		isStackable: true,
		maxStackSize: 10
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.GemSapphire,
		text: Proto.CreateText("Sapphire", "A precious blue gem."),
		iconName: "icon_gem_sapphire",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Rare,
		buyPrice: 200,
		isStackable: true,
		maxStackSize: 10
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.GemEmerald,
		text: Proto.CreateText("Emerald", "A precious green gem."),
		iconName: "icon_gem_emerald",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Rare,
		buyPrice: 200,
		isStackable: true,
		maxStackSize: 10
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.GemDiamond,
		text: Proto.CreateText("Diamond", "The hardest and most valuable gem."),
		iconName: "icon_gem_diamond",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Epic,
		buyPrice: 500,
		isStackable: true,
		maxStackSize: 5
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.GemMoonstone,
		text: Proto.CreateText("Moonstone", "A gem that glows with lunar light."),
		iconName: "icon_gem_moonstone",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Rare,
		buyPrice: 150,
		isStackable: true,
		maxStackSize: 10
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.Pearl,
		text: Proto.CreateText("Pearl", "A lustrous pearl from the sea."),
		iconName: "icon_pearl",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Uncommon,
		buyPrice: 50,
		isStackable: true,
		maxStackSize: 20
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.BlackPearl,
		text: Proto.CreateText("Black Pearl", "A rare pearl of deepest black."),
		iconName: "icon_pearl_black",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Epic,
		buyPrice: 250,
		isStackable: true,
		maxStackSize: 10
	));

	// ═══════════════════════════════════════════════════════════════════
	// WOOD & BARK
	// ═══════════════════════════════════════════════════════════════════
	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.Wood,
		text: Proto.CreateText("Wood", "Timber for building and crafting."),
		iconName: "icon_wood",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 2,
		isStackable: true,
		maxStackSize: 50
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.OakWood,
		text: Proto.CreateText("Oak Wood", "Strong and durable oak timber."),
		iconName: "icon_wood_oak",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 4,
		isStackable: true,
		maxStackSize: 50
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.PineWood,
		text: Proto.CreateText("Pine Wood", "Light and fragrant pine timber."),
		iconName: "icon_wood_pine",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 3,
		isStackable: true,
		maxStackSize: 50
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.BirchWood,
		text: Proto.CreateText("Birch Wood", "Pale birch timber."),
		iconName: "icon_wood_birch",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 3,
		isStackable: true,
		maxStackSize: 50
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.WillowWood,
		text: Proto.CreateText("Willow Wood", "Flexible willow timber."),
		iconName: "icon_wood_willow",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Uncommon,
		buyPrice: 8,
		isStackable: true,
		maxStackSize: 40
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.ElderWood,
		text: Proto.CreateText("Elder Wood", "Ancient wood with mystical properties."),
		iconName: "icon_wood_elder",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Rare,
		buyPrice: 40,
		isStackable: true,
		maxStackSize: 20
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.IronwoodLog,
		text: Proto.CreateText("Ironwood Log", "Wood as hard as iron."),
		iconName: "icon_wood_ironwood",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Rare,
		buyPrice: 60,
		isStackable: true,
		maxStackSize: 20
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.CorruptedWood,
		text: Proto.CreateText("Corrupted Wood", "Wood twisted by dark magic."),
		iconName: "icon_wood_corrupted",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Uncommon,
		buyPrice: 15,
		isStackable: true,
		maxStackSize: 30
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.FrozenWood,
		text: Proto.CreateText("Frozen Wood", "Wood frozen solid by eternal cold."),
		iconName: "icon_wood_frozen",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Uncommon,
		buyPrice: 12,
		isStackable: true,
		maxStackSize: 30
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.Firewood,
		text: Proto.CreateText("Firewood", "Wood prepared for burning."),
		iconName: "icon_firewood",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 1,
		isStackable: true,
		maxStackSize: 50
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.RottenWood,
		text: Proto.CreateText("Rotten Wood", "Decayed wood, useful for some purposes."),
		iconName: "icon_wood_rotten",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 1,
		isStackable: true,
		maxStackSize: 50
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.Driftwood,
		text: Proto.CreateText("Driftwood", "Wood weathered by the sea."),
		iconName: "icon_driftwood",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 2,
		isStackable: true,
		maxStackSize: 50
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.Bark,
		text: Proto.CreateText("Bark", "Tree bark for tanning and crafting."),
		iconName: "icon_bark",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 1,
		isStackable: true,
		maxStackSize: 50
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.BirchBark,
		text: Proto.CreateText("Birch Bark", "Thin papery birch bark."),
		iconName: "icon_bark_birch",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 2,
		isStackable: true,
		maxStackSize: 50
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.WillowBark,
		text: Proto.CreateText("Willow Bark", "Medicinal willow bark."),
		iconName: "icon_bark_willow",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Uncommon,
		buyPrice: 8,
		isStackable: true,
		maxStackSize: 30
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.DriedBark,
		text: Proto.CreateText("Dried Bark", "Bark dried for preservation."),
		iconName: "icon_bark_dried",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 2,
		isStackable: true,
		maxStackSize: 50
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.PineResin,
		text: Proto.CreateText("Pine Resin", "Sticky resin from pine trees."),
		iconName: "icon_resin",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 5,
		isStackable: true,
		maxStackSize: 30
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.Acorn,
		text: Proto.CreateText("Acorn", "An oak acorn."),
		iconName: "icon_acorn",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 1,
		isStackable: true,
		maxStackSize: 50
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.Pinecone,
		text: Proto.CreateText("Pinecone", "A pinecone with seeds."),
		iconName: "icon_pinecone",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 1,
		isStackable: true,
		maxStackSize: 50
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.Charcoal,
		text: Proto.CreateText("Charcoal", "Charred wood for fuel and drawing."),
		iconName: "icon_charcoal",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 3,
		isStackable: true,
		maxStackSize: 50
	));

	// ═══════════════════════════════════════════════════════════════════
	// HERBS & PLANTS
	// ═══════════════════════════════════════════════════════════════════
	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.Herbs,
		text: Proto.CreateText("Herbs", "Medicinal herbs."),
		iconName: "icon_herbs",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 3,
		isStackable: true,
		maxStackSize: 50
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.RareHerbs,
		text: Proto.CreateText("Rare Herbs", "Rare medicinal herbs with potent properties."),
		iconName: "icon_herbs_rare",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Uncommon,
		buyPrice: 15,
		isStackable: true,
		maxStackSize: 20
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.Wildflowers,
		text: Proto.CreateText("Wildflowers", "A bundle of colorful wildflowers."),
		iconName: "icon_wildflowers",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 2,
		isStackable: true,
		maxStackSize: 50
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.Moonbloom,
		text: Proto.CreateText("Moonbloom", "A flower that blooms only under moonlight."),
		iconName: "icon_moonbloom",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Rare,
		buyPrice: 50,
		isStackable: true,
		maxStackSize: 20
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.Nightshade,
		text: Proto.CreateText("Nightshade", "A deadly poisonous plant."),
		iconName: "icon_nightshade",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Uncommon,
		buyPrice: 20,
		isStackable: true,
		maxStackSize: 20
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.GlowMoss,
		text: Proto.CreateText("Glow Moss", "Moss that emits a soft glow."),
		iconName: "icon_moss_glow",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Uncommon,
		buyPrice: 15,
		isStackable: true,
		maxStackSize: 30
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.MandrakeRoot,
		text: Proto.CreateText("Mandrake Root", "A root said to scream when pulled."),
		iconName: "icon_mandrake",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Rare,
		buyPrice: 75,
		isStackable: true,
		maxStackSize: 10
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.SwampMoss,
		text: Proto.CreateText("Swamp Moss", "Damp moss from the swamp."),
		iconName: "icon_moss_swamp",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 3,
		isStackable: true,
		maxStackSize: 50
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.FrostLichen,
		text: Proto.CreateText("Frost Lichen", "Lichen that thrives in freezing cold."),
		iconName: "icon_lichen_frost",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Uncommon,
		buyPrice: 12,
		isStackable: true,
		maxStackSize: 30
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.FireBloom,
		text: Proto.CreateText("Fire Bloom", "A flower that burns with inner fire."),
		iconName: "icon_firebloom",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Rare,
		buyPrice: 60,
		isStackable: true,
		maxStackSize: 15
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.Elderberries,
		text: Proto.CreateText("Elderberries", "Dark berries from the elder tree."),
		iconName: "icon_elderberries",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Uncommon,
		buyPrice: 8,
		isStackable: true,
		maxStackSize: 30
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.ElderFlowers,
		text: Proto.CreateText("Elder Flowers", "Fragrant flowers from the elder tree."),
		iconName: "icon_elderflowers",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Uncommon,
		buyPrice: 10,
		isStackable: true,
		maxStackSize: 30
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.DesertFlower,
		text: Proto.CreateText("Desert Flower", "A resilient flower from the desert."),
		iconName: "icon_flower_desert",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Uncommon,
		buyPrice: 15,
		isStackable: true,
		maxStackSize: 30
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.CactusFlesh,
		text: Proto.CreateText("Cactus Flesh", "Moist flesh from a cactus."),
		iconName: "icon_cactus_flesh",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 5,
		isStackable: true,
		maxStackSize: 30
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.CactusNeedle,
		text: Proto.CreateText("Cactus Needle", "Sharp needles from a cactus."),
		iconName: "icon_cactus_needle",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 2,
		isStackable: true,
		maxStackSize: 50
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.CactusFlower,
		text: Proto.CreateText("Cactus Flower", "A beautiful bloom from a cactus."),
		iconName: "icon_cactus_flower",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Uncommon,
		buyPrice: 20,
		isStackable: true,
		maxStackSize: 20
	));

	// ═══════════════════════════════════════════════════════════════════
	// FOOD & FORAGING
	// ═══════════════════════════════════════════════════════════════════
	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.Berries,
		text: Proto.CreateText("Berries", "Fresh wild berries."),
		iconName: "icon_berries",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 2,
		isStackable: true,
		maxStackSize: 50
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.RareBerries,
		text: Proto.CreateText("Rare Berries", "Uncommon berries with special properties."),
		iconName: "icon_berries_rare",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Uncommon,
		buyPrice: 10,
		isStackable: true,
		maxStackSize: 30
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.FrozenBerries,
		text: Proto.CreateText("Frozen Berries", "Berries preserved by ice."),
		iconName: "icon_berries_frozen",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 4,
		isStackable: true,
		maxStackSize: 50
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.Mushrooms,
		text: Proto.CreateText("Mushrooms", "Edible forest mushrooms."),
		iconName: "icon_mushrooms",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 3,
		isStackable: true,
		maxStackSize: 50
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.GlowingMushroom,
		text: Proto.CreateText("Glowing Mushroom", "A mushroom that glows in the dark."),
		iconName: "icon_mushroom_glow",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Uncommon,
		buyPrice: 15,
		isStackable: true,
		maxStackSize: 30
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.PoisonousMushroom,
		text: Proto.CreateText("Poisonous Mushroom", "A deadly mushroom. Handle with care."),
		iconName: "icon_mushroom_poison",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Uncommon,
		buyPrice: 12,
		isStackable: true,
		maxStackSize: 20
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.Apples,
		text: Proto.CreateText("Apples", "Fresh apples from the orchard."),
		iconName: "icon_apples",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 2,
		isStackable: true,
		maxStackSize: 50
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.Nuts,
		text: Proto.CreateText("Nuts", "Assorted nuts from the forest."),
		iconName: "icon_nuts",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 2,
		isStackable: true,
		maxStackSize: 50
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.Grapes,
		text: Proto.CreateText("Grapes", "Sweet grapes for wine or eating."),
		iconName: "icon_grapes",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 3,
		isStackable: true,
		maxStackSize: 50
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.WildVegetables,
		text: Proto.CreateText("Wild Vegetables", "Foraged vegetables from the wild."),
		iconName: "icon_vegetables",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 2,
		isStackable: true,
		maxStackSize: 50
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.WildOnions,
		text: Proto.CreateText("Wild Onions", "Pungent wild onions."),
		iconName: "icon_onions",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 2,
		isStackable: true,
		maxStackSize: 50
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.Truffle,
		text: Proto.CreateText("Truffle", "A rare and delicious fungus."),
		iconName: "icon_truffle",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Rare,
		buyPrice: 100,
		isStackable: true,
		maxStackSize: 10
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.DateFruit,
		text: Proto.CreateText("Date Fruit", "Sweet dates from desert palms."),
		iconName: "icon_dates",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 4,
		isStackable: true,
		maxStackSize: 50
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.Honey,
		text: Proto.CreateText("Honey", "Golden honey from wild bees."),
		iconName: "icon_honey",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Uncommon,
		buyPrice: 15,
		isStackable: true,
		maxStackSize: 30
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.Beeswax,
		text: Proto.CreateText("Beeswax", "Wax from beehives."),
		iconName: "icon_beeswax",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 5,
		isStackable: true,
		maxStackSize: 30
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.Eggs,
		text: Proto.CreateText("Eggs", "Fresh eggs from wild birds."),
		iconName: "icon_eggs",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 3,
		isStackable: true,
		maxStackSize: 30
	));

	// ═══════════════════════════════════════════════════════════════════
	// FISH & AQUATIC
	// ═══════════════════════════════════════════════════════════════════
	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.FreshFish,
		text: Proto.CreateText("Fresh Fish", "A freshly caught fish."),
		iconName: "icon_fish",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 5,
		isStackable: true,
		maxStackSize: 30
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.RareFish,
		text: Proto.CreateText("Rare Fish", "An uncommon species of fish."),
		iconName: "icon_fish_rare",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Uncommon,
		buyPrice: 25,
		isStackable: true,
		maxStackSize: 20
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.LakeFish,
		text: Proto.CreateText("Lake Fish", "A fish from calm lake waters."),
		iconName: "icon_fish_lake",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 6,
		isStackable: true,
		maxStackSize: 30
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.SwampFish,
		text: Proto.CreateText("Swamp Fish", "A fish adapted to murky waters."),
		iconName: "icon_fish_swamp",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 4,
		isStackable: true,
		maxStackSize: 30
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.GiantFish,
		text: Proto.CreateText("Giant Fish", "A massive fish of legendary size."),
		iconName: "icon_fish_giant",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Rare,
		buyPrice: 75,
		isStackable: true,
		maxStackSize: 10
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.MagicFish,
		text: Proto.CreateText("Magic Fish", "A fish infused with magical energy."),
		iconName: "icon_fish_magic",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Rare,
		buyPrice: 100,
		isStackable: true,
		maxStackSize: 10
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.IceFish,
		text: Proto.CreateText("Ice Fish", "A fish that lives in frozen waters."),
		iconName: "icon_fish_ice",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Uncommon,
		buyPrice: 20,
		isStackable: true,
		maxStackSize: 20
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.FrozenFish,
		text: Proto.CreateText("Frozen Fish", "A fish preserved in ice."),
		iconName: "icon_fish_frozen",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 8,
		isStackable: true,
		maxStackSize: 30
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.MagmaFish,
		text: Proto.CreateText("Magma Fish", "A fish that thrives in volcanic waters."),
		iconName: "icon_fish_magma",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Rare,
		buyPrice: 80,
		isStackable: true,
		maxStackSize: 10,
		flavorText: "Hot to the touch."
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.SwampEel,
		text: Proto.CreateText("Swamp Eel", "A slippery eel from the swamp."),
		iconName: "icon_eel",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Uncommon,
		buyPrice: 15,
		isStackable: true,
		maxStackSize: 20
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.Tadpole,
		text: Proto.CreateText("Tadpole", "A small amphibian larva."),
		iconName: "icon_tadpole",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 1,
		isStackable: true,
		maxStackSize: 50
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.FrogLegs,
		text: Proto.CreateText("Frog Legs", "Legs from a large frog. A delicacy."),
		iconName: "icon_frog_legs",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 5,
		isStackable: true,
		maxStackSize: 30
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.Leech,
		text: Proto.CreateText("Leech", "A blood-sucking leech."),
		iconName: "icon_leech",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 3,
		isStackable: true,
		maxStackSize: 30
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.Seashells,
		text: Proto.CreateText("Seashells", "Decorative shells from the sea."),
		iconName: "icon_seashells",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 2,
		isStackable: true,
		maxStackSize: 50
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.Clams,
		text: Proto.CreateText("Clams", "Fresh clams from the shore."),
		iconName: "icon_clams",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 4,
		isStackable: true,
		maxStackSize: 30
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.Oysters,
		text: Proto.CreateText("Oysters", "Oysters that may contain pearls."),
		iconName: "icon_oysters",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Uncommon,
		buyPrice: 10,
		isStackable: true,
		maxStackSize: 20
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.Barnacles,
		text: Proto.CreateText("Barnacles", "Crusty barnacles from rocks."),
		iconName: "icon_barnacles",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 1,
		isStackable: true,
		maxStackSize: 50
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.OldBoot,
		text: Proto.CreateText("Old Boot", "A waterlogged old boot. Why did you keep this?"),
		iconName: "icon_old_boot",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 0,
		sellPrice: 1,
		isStackable: false,
		flavorText: "Someone's trash is... still trash."
	));

	// ═══════════════════════════════════════════════════════════════════
	// HUNTING - MEAT
	// ═══════════════════════════════════════════════════════════════════
	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.RabbitMeat,
		text: Proto.CreateText("Rabbit Meat", "Lean meat from a rabbit."),
		iconName: "icon_meat_rabbit",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 4,
		isStackable: true,
		maxStackSize: 30
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.Venison,
		text: Proto.CreateText("Venison", "Quality meat from a deer."),
		iconName: "icon_meat_venison",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 8,
		isStackable: true,
		maxStackSize: 30
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.BoarMeat,
		text: Proto.CreateText("Boar Meat", "Tough meat from a wild boar."),
		iconName: "icon_meat_boar",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 6,
		isStackable: true,
		maxStackSize: 30
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.WolfMeat,
		text: Proto.CreateText("Wolf Meat", "Stringy meat from a wolf."),
		iconName: "icon_meat_wolf",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 5,
		isStackable: true,
		maxStackSize: 30
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.BearMeat,
		text: Proto.CreateText("Bear Meat", "Rich meat from a bear."),
		iconName: "icon_meat_bear",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Uncommon,
		buyPrice: 15,
		isStackable: true,
		maxStackSize: 20
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.FoxMeat,
		text: Proto.CreateText("Fox Meat", "Gamey meat from a fox."),
		iconName: "icon_meat_fox",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 4,
		isStackable: true,
		maxStackSize: 30
	));

	// ═══════════════════════════════════════════════════════════════════
	// HUNTING - PELTS & HIDES
	// ═══════════════════════════════════════════════════════════════════
	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.RabbitPelt,
		text: Proto.CreateText("Rabbit Pelt", "Soft fur from a rabbit."),
		iconName: "icon_pelt_rabbit",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 5,
		isStackable: true,
		maxStackSize: 30
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.RabbitFoot,
		text: Proto.CreateText("Rabbit Foot", "Said to bring good luck."),
		iconName: "icon_rabbit_foot",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Uncommon,
		buyPrice: 15,
		isStackable: true,
		maxStackSize: 10
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.DeerHide,
		text: Proto.CreateText("Deer Hide", "A large hide from a deer."),
		iconName: "icon_hide_deer",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 10,
		isStackable: true,
		maxStackSize: 20
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.Antlers,
		text: Proto.CreateText("Antlers", "Antlers from a deer."),
		iconName: "icon_antlers",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Uncommon,
		buyPrice: 20,
		isStackable: true,
		maxStackSize: 10
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.BoarHide,
		text: Proto.CreateText("Boar Hide", "Tough hide from a boar."),
		iconName: "icon_hide_boar",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 8,
		isStackable: true,
		maxStackSize: 20
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.BoarTusk,
		text: Proto.CreateText("Boar Tusk", "Sharp tusks from a boar."),
		iconName: "icon_tusk_boar",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 6,
		isStackable: true,
		maxStackSize: 20
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.WolfPelt,
		text: Proto.CreateText("Wolf Pelt", "A grey wolf pelt."),
		iconName: "icon_pelt_wolf",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 12,
		isStackable: true,
		maxStackSize: 20
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.DireWolfPelt,
		text: Proto.CreateText("Dire Wolf Pelt", "A massive pelt from a dire wolf."),
		iconName: "icon_pelt_direwolf",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Rare,
		buyPrice: 50,
		isStackable: true,
		maxStackSize: 10
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.WolfFang,
		text: Proto.CreateText("Wolf Fang", "A sharp fang from a wolf."),
		iconName: "icon_fang_wolf",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 5,
		isStackable: true,
		maxStackSize: 30
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.BearPelt,
		text: Proto.CreateText("Bear Pelt", "A thick bear pelt."),
		iconName: "icon_pelt_bear",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Uncommon,
		buyPrice: 30,
		isStackable: true,
		maxStackSize: 10
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.BearClaw,
		text: Proto.CreateText("Bear Claw", "A large claw from a bear."),
		iconName: "icon_claw_bear",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Uncommon,
		buyPrice: 15,
		isStackable: true,
		maxStackSize: 20
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.FoxPelt,
		text: Proto.CreateText("Fox Pelt", "A beautiful red fox pelt."),
		iconName: "icon_pelt_fox",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Uncommon,
		buyPrice: 18,
		isStackable: true,
		maxStackSize: 20
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.RatTail,
		text: Proto.CreateText("Rat Tail", "A disgusting rat tail."),
		iconName: "icon_rat_tail",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 1,
		isStackable: true,
		maxStackSize: 50
	));

	// ═══════════════════════════════════════════════════════════════════
	// HUNTING - FEATHERS & MISC
	// ═══════════════════════════════════════════════════════════════════
	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.Feathers,
		text: Proto.CreateText("Feathers", "Assorted bird feathers."),
		iconName: "icon_feathers",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 2,
		isStackable: true,
		maxStackSize: 50
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.OwlFeather,
		text: Proto.CreateText("Owl Feather", "A silent feather from an owl."),
		iconName: "icon_feather_owl",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Uncommon,
		buyPrice: 10,
		isStackable: true,
		maxStackSize: 30
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.OwlPellet,
		text: Proto.CreateText("Owl Pellet", "Regurgitated owl pellet. Contains bones."),
		iconName: "icon_owl_pellet",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 3,
		isStackable: true,
		maxStackSize: 30
	));

	// ═══════════════════════════════════════════════════════════════════
	// MONSTER PARTS
	// ═══════════════════════════════════════════════════════════════════
	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.SpiderSilk,
		text: Proto.CreateText("Spider Silk", "Strong silk from giant spiders."),
		iconName: "icon_spider_silk",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Uncommon,
		buyPrice: 20,
		isStackable: true,
		maxStackSize: 30
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.SpiderEgg,
		text: Proto.CreateText("Spider Egg", "An egg sac from a giant spider."),
		iconName: "icon_spider_egg",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Uncommon,
		buyPrice: 15,
		isStackable: true,
		maxStackSize: 20
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.VenomSac,
		text: Proto.CreateText("Venom Sac", "A sac containing potent venom."),
		iconName: "icon_venom_sac",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Uncommon,
		buyPrice: 25,
		isStackable: true,
		maxStackSize: 20
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.WyvernScale,
		text: Proto.CreateText("Wyvern Scale", "A tough scale from a wyvern."),
		iconName: "icon_scale_wyvern",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Rare,
		buyPrice: 75,
		isStackable: true,
		maxStackSize: 15
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.WyvernEgg,
		text: Proto.CreateText("Wyvern Egg", "A large wyvern egg."),
		iconName: "icon_egg_wyvern",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Epic,
		buyPrice: 300,
		isStackable: false
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.WyvernWing,
		text: Proto.CreateText("Wyvern Wing", "Leathery wing membrane from a wyvern."),
		iconName: "icon_wing_wyvern",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Rare,
		buyPrice: 60,
		isStackable: true,
		maxStackSize: 10
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.FireScale,
		text: Proto.CreateText("Fire Scale", "A scale hot to the touch."),
		iconName: "icon_scale_fire",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Rare,
		buyPrice: 100,
		isStackable: true,
		maxStackSize: 15
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.DragonBone,
		text: Proto.CreateText("Dragon Bone", "A bone from a dragon."),
		iconName: "icon_bone_dragon",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Epic,
		buyPrice: 250,
		isStackable: true,
		maxStackSize: 10,
		flavorText: "Still radiates power."
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.DragonScale,
		text: Proto.CreateText("Dragon Scale", "An impenetrable dragon scale."),
		iconName: "icon_scale_dragon",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Legendary,
		buyPrice: 500,
		isStackable: true,
		maxStackSize: 5
	));

			db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.DragonTooth,
		text: Proto.CreateText("Dragon Tooth", "A massive tooth from a dragon."),
		iconName: "icon_tooth_dragon",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Legendary,
		buyPrice: 400,
		isStackable: true,
		maxStackSize: 5,
		flavorText: "Sharp enough to cut steel."
	));

	// ═══════════════════════════════════════════════════════════════════
	// CREATURE PARTS
	// ═══════════════════════════════════════════════════════════════════
	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.Bone,
		text: Proto.CreateText("Bone", "Bones from fallen creatures."),
		iconName: "icon_bone",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 2,
		isStackable: true,
		maxStackSize: 50
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.Skull,
		text: Proto.CreateText("Skull", "A skull from a creature."),
		iconName: "icon_skull",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Uncommon,
		buyPrice: 10,
		isStackable: true,
		maxStackSize: 20
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.Ectoplasm,
		text: Proto.CreateText("Ectoplasm", "Residue left by spectral beings."),
		iconName: "icon_ectoplasm",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Rare,
		buyPrice: 50,
		isStackable: true,
		maxStackSize: 20
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.ShadowEssence,
		text: Proto.CreateText("Shadow Essence", "Condensed darkness from shadow creatures."),
		iconName: "icon_shadow_essence",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Rare,
		buyPrice: 60,
		isStackable: true,
		maxStackSize: 15
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.SoulFragment,
		text: Proto.CreateText("Soul Fragment", "A fragment of a departed soul."),
		iconName: "icon_soul_fragment",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Epic,
		buyPrice: 150,
		isStackable: true,
		maxStackSize: 10
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.DemonHorn,
		text: Proto.CreateText("Demon Horn", "A horn from a demonic creature."),
		iconName: "icon_demon_horn",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Rare,
		buyPrice: 75,
		isStackable: true,
		maxStackSize: 10
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.DemonEssence,
		text: Proto.CreateText("Demon Essence", "Pure demonic energy in crystallized form."),
		iconName: "icon_demon_essence",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Epic,
		buyPrice: 200,
		isStackable: true,
		maxStackSize: 10
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.HellhoundFang,
		text: Proto.CreateText("Hellhound Fang", "A blazing fang from a hellhound."),
		iconName: "icon_hellhound_fang",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Rare,
		buyPrice: 80,
		isStackable: true,
		maxStackSize: 10
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.InfernalAsh,
		text: Proto.CreateText("Infernal Ash", "Ash from infernal flames."),
		iconName: "icon_infernal_ash",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Uncommon,
		buyPrice: 30,
		isStackable: true,
		maxStackSize: 30
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.Grubs,
		text: Proto.CreateText("Grubs", "Wriggling grubs. Good for bait."),
		iconName: "icon_grubs",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 1,
		isStackable: true,
		maxStackSize: 50
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.Beetle,
		text: Proto.CreateText("Beetle", "A large beetle."),
		iconName: "icon_beetle",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 2,
		isStackable: true,
		maxStackSize: 30
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.Ants,
		text: Proto.CreateText("Ants", "A jar of ants."),
		iconName: "icon_ants",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 1,
		isStackable: true,
		maxStackSize: 50
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.AntEggs,
		text: Proto.CreateText("Ant Eggs", "Eggs from a giant ant colony."),
		iconName: "icon_ant_eggs",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Uncommon,
		buyPrice: 8,
		isStackable: true,
		maxStackSize: 30
	));

	// ═══════════════════════════════════════════════════════════════════
	// MAGICAL ESSENCES
	// ═══════════════════════════════════════════════════════════════════
	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.FogEssence,
		text: Proto.CreateText("Fog Essence", "Condensed essence of the Fog itself."),
		iconName: "icon_fog_essence",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Legendary,
		buyPrice: 500,
		isStackable: true,
		maxStackSize: 5,
		flavorText: "It shifts and writhes even when contained."
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.TimeShard,
		text: Proto.CreateText("Time Shard", "A crystallized fragment of time."),
		iconName: "icon_time_shard",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Legendary,
		buyPrice: 750,
		isStackable: true,
		maxStackSize: 5,
		flavorText: "Moments frozen in crystal form."
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.ManaEssence,
		text: Proto.CreateText("Mana Essence", "Pure crystallized magical energy."),
		iconName: "icon_mana_essence",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Rare,
		buyPrice: 100,
		isStackable: true,
		maxStackSize: 15
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.EternalIce,
		text: Proto.CreateText("Eternal Ice", "Ice that never melts."),
		iconName: "icon_eternal_ice",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Epic,
		buyPrice: 175,
		isStackable: true,
		maxStackSize: 10,
		flavorText: "Cold beyond measure."
	));

	// ═══════════════════════════════════════════════════════════════════
	// CLOTH & LEATHER
	// ═══════════════════════════════════════════════════════════════════
	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.LeatherScraps,
		text: Proto.CreateText("Leather Scraps", "Leather suitable for crafting."),
		iconName: "icon_leather_scraps",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 4,
		isStackable: true,
		maxStackSize: 50
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.Leather,
		text: Proto.CreateText("Leather", "Tanned leather ready for use."),
		iconName: "icon_leather",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 8,
		isStackable: true,
		maxStackSize: 50
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.Cloth,
		text: Proto.CreateText("Cloth", "Woven fabric for crafting."),
		iconName: "icon_cloth",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 3,
		isStackable: true,
		maxStackSize: 50
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.Rope,
		text: Proto.CreateText("Rope", "Sturdy rope for various uses."),
		iconName: "icon_rope",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 5,
		isStackable: true,
		maxStackSize: 30
	));

	// ═══════════════════════════════════════════════════════════════════
	// ENVIRONMENTAL
	// ═══════════════════════════════════════════════════════════════════
	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.Snow,
		text: Proto.CreateText("Snow", "Packed snow. Melts quickly."),
		iconName: "icon_snow",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 1,
		isStackable: true,
		maxStackSize: 50
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.Ice,
		text: Proto.CreateText("Ice", "A block of ice."),
		iconName: "icon_ice",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 2,
		isStackable: true,
		maxStackSize: 30
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.FreshWater,
		text: Proto.CreateText("Fresh Water", "Clean drinking water."),
		iconName: "icon_water_fresh",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 1,
		isStackable: true,
		maxStackSize: 30
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.MineralWater,
		text: Proto.CreateText("Mineral Water", "Water rich in minerals."),
		iconName: "icon_water_mineral",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Uncommon,
		buyPrice: 5,
		isStackable: true,
		maxStackSize: 20
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.VolcanicAsh,
		text: Proto.CreateText("Volcanic Ash", "Ash from volcanic activity."),
		iconName: "icon_volcanic_ash",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Uncommon,
		buyPrice: 10,
		isStackable: true,
		maxStackSize: 30
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.SulfiteDeposit,
		text: Proto.CreateText("Sulfite Deposit", "A chunk of sulfite mineral."),
		iconName: "icon_sulfite_deposit",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Uncommon,
		buyPrice: 15,
		isStackable: true,
		maxStackSize: 20
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.MineralSalt,
		text: Proto.CreateText("Mineral Salt", "Salt deposits for preservation and cooking."),
		iconName: "icon_salt",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 3,
		isStackable: true,
		maxStackSize: 50
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.RiverStone,
		text: Proto.CreateText("River Stone", "Smooth stones from a riverbed."),
		iconName: "icon_river_stone",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Common,
		buyPrice: 1,
		isStackable: true,
		maxStackSize: 50
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.GoldNugget,
		text: Proto.CreateText("Gold Nugget", "A nugget of pure gold."),
		iconName: "icon_gold_nugget",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Rare,
		buyPrice: 100,
		isStackable: true,
		maxStackSize: 10
	));

	// ═══════════════════════════════════════════════════════════════════
	// SACRED & ANCIENT
	// ═══════════════════════════════════════════════════════════════════
	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.SacredIncense,
		text: Proto.CreateText("Sacred Incense", "Incense used in holy rituals."),
		iconName: "icon_incense",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Uncommon,
		buyPrice: 20,
		isStackable: true,
		maxStackSize: 20
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.HolyWater,
		text: Proto.CreateText("Holy Water", "Water blessed by a priest."),
		iconName: "icon_holy_water",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Uncommon,
		buyPrice: 25,
		isStackable: true,
		maxStackSize: 20
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.AncientRelic,
		text: Proto.CreateText("Ancient Relic", "A relic from a bygone era."),
		iconName: "icon_ancient_relic",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Epic,
		buyPrice: 200,
		isStackable: true,
		maxStackSize: 5
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.AncientPottery,
		text: Proto.CreateText("Ancient Pottery", "Pottery shards from ancient civilizations."),
		iconName: "icon_pottery",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Uncommon,
		buyPrice: 15,
		isStackable: true,
		maxStackSize: 20
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.AncientScroll,
		text: Proto.CreateText("Ancient Scroll", "A scroll containing ancient knowledge."),
		iconName: "icon_scroll_ancient",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Rare,
		buyPrice: 75,
		isStackable: true,
		maxStackSize: 10
	));

	db.RegisterProto(new ItemProto(
		id: Ids.Items.Materials.GoldCoins,
		text: Proto.CreateText("Gold Coins", "Ancient gold coins of unknown origin."),
		iconName: "icon_coins_gold",
		category: ItemCategory.Material,
		type: ItemType.Material,
		rarity: RarityType.Uncommon,
		buyPrice: 50,
		isStackable: true,
		maxStackSize: 20,
		flavorText: "Collectors would pay extra for these."
	));
}

#endregion

	#region Accessories

	private void RegisterAccessories(GameDb db) {
		db.RegisterProto(new ItemProto(
			id: Ids.Items.Accessories.RingOfProtection,
			text: Proto.CreateText("Ring of Protection", "A ring that provides magical protection."),
			iconName: "icon_ring_protection",
			category: ItemCategory.Accessory,
			type: ItemType.Ring,
			rarity: RarityType.Uncommon,
			buyPrice: 200,
			itemLevel: 5,
			equipSlot: SlotType.Ring1,
			equipStats: [
				EquipmentStat.Flat(Ids.Stats.Combat.ArmorClass, 1),
				EquipmentStat.Flat(Ids.Stats.Resistances.Physical, 5),
				EquipmentStat.Flat(Ids.Stats.Resistances.Magical, 5)
			]
		));

		db.RegisterProto(new ItemProto(
			id: Ids.Items.Accessories.AmuletOfHealth,
			text: Proto.CreateText("Amulet of Health", "An amulet that bolsters vitality."),
			iconName: "icon_amulet_health",
			category: ItemCategory.Accessory,
			type: ItemType.Amulet,
			rarity: RarityType.Uncommon,
			buyPrice: 250,
			itemLevel: 5,
			equipSlot: SlotType.Amulet,
			equipStats: [
				EquipmentStat.Flat(Ids.Stats.Resource.MaxHealth, 20),
				EquipmentStat.Flat(Ids.Stats.Attributes.Constitution, 1)
			]
		));

		db.RegisterProto(new ItemProto(
			id: Ids.Items.Accessories.HolySymbol,
			text: Proto.CreateText("Holy Symbol", "A sacred symbol of divine power."),
			iconName: "icon_holy_symbol",
			category: ItemCategory.Accessory,
			type: ItemType.Trinket,
			rarity: RarityType.Common,
			buyPrice: 25,
			itemLevel: 1,
			equipSlot: SlotType.Trinket,
			equipStats: [
				EquipmentStat.Flat(Ids.Stats.Combat.SpellPower, 2),
				EquipmentStat.Flat(Ids.Stats.Attributes.Wisdom, 1)
			],
			classRestrictions: [Ids.CharacterClasses.Cleric, Ids.CharacterClasses.Paladin]
		));

		db.RegisterProto(new ItemProto(
			id: Ids.Items.Accessories.Quiver,
			text: Proto.CreateText("Quiver", "Holds arrows and provides a slight accuracy bonus."),
			iconName: "icon_quiver",
			category: ItemCategory.Accessory,
			type: ItemType.Trinket,
			rarity: RarityType.Common,
			buyPrice: 20,
			itemLevel: 1,
			equipSlot: SlotType.Trinket,
			equipStats: [EquipmentStat.Flat(Ids.Stats.Combat.AttackBonus, 1)],
			classRestrictions: [Ids.CharacterClasses.Ranger]
		));

		db.RegisterProto(new ItemProto(
			id: Ids.Items.Accessories.Spellbook,
			text: Proto.CreateText("Spellbook", "A tome containing arcane knowledge."),
			iconName: "icon_spellbook",
			category: ItemCategory.Accessory,
			type: ItemType.Trinket,
			rarity: RarityType.Common,
			buyPrice: 30,
			itemLevel: 1,
			equipSlot: SlotType.Trinket,
			equipStats: [
				EquipmentStat.Flat(Ids.Stats.Combat.SpellPower, 2),
				EquipmentStat.Flat(Ids.Stats.Attributes.Intelligence, 1)
			],
			classRestrictions: [Ids.CharacterClasses.Mage, Ids.CharacterClasses.Necromancer]
		));

		db.RegisterProto(new ItemProto(
				id: Ids.Items.Accessories.RingOfStrength,
				text: Proto.CreateText("Ring of Strength", "A ring that enhances physical power."),
				iconName: "icon_ring_strength",
				category: ItemCategory.Accessory,
				type: ItemType.Ring,
				rarity: RarityType.Uncommon,
				buyPrice: 200,
				itemLevel: 5,
				equipSlot: SlotType.Ring1,
				equipStats: [
					EquipmentStat.Flat(Ids.Stats.Attributes.Strength, 2),
					EquipmentStat.Flat(Ids.Stats.Combat.DamBonus, 1)
				]
			));

		db.RegisterProto(new ItemProto(
				id: Ids.Items.Accessories.AmuletOfWisdom,
				text: Proto.CreateText("Amulet of Wisdom", "An amulet that sharpens the mind."),
				iconName: "icon_amulet_wisdom",
				category: ItemCategory.Accessory,
				type: ItemType.Amulet,
				rarity: RarityType.Uncommon,
				buyPrice: 250,
				itemLevel: 5,
				equipSlot: SlotType.Amulet,
				equipStats: [
					EquipmentStat.Flat(Ids.Stats.Attributes.Wisdom, 2),
					EquipmentStat.Flat(Ids.Stats.Combat.SpellPower, 2)
				]
			));
	}

	#endregion

	#region Quest Items

	private void RegisterQuestItems(GameDb db) {
		db.RegisterProto(new ItemProto(
			id: Ids.Items.Quest.BanditKingKey,
			text: Proto.CreateText("Bandit King's Key", "A key taken from the Bandit King. Opens something important."),
			iconName: "icon_key_bandit",
			category: ItemCategory.Quest,
			type: ItemType.Quest,
			rarity: RarityType.Rare,
			buyPrice: 0,
			sellPrice: 0,
			isStackable: false
		));

		db.RegisterProto(new ItemProto(
			id: Ids.Items.Quest.BanditKingCrown,
			text: Proto.CreateText("Bandit King's Crown", "The crown of the fallen Bandit King."),
			iconName: "icon_crown_bandit",
			category: ItemCategory.Quest,
			type: ItemType.Quest,
			rarity: RarityType.Epic,
			buyPrice: 0,
			sellPrice: 100,
			isStackable: false
		));

		db.RegisterProto(new ItemProto(
			id: Ids.Items.Quest.BanditLetters,
			text: Proto.CreateText("Bandit Letters", "Letters revealing a larger bandit organization."),
			iconName: "icon_letters",
			category: ItemCategory.Quest,
			type: ItemType.Quest,
			rarity: RarityType.Uncommon,
			buyPrice: 0,
			sellPrice: 0,
			isStackable: false
		));

		db.RegisterProto(new ItemProto(
			id: Ids.Items.Quest.LichPhylacteryShard,
			text: Proto.CreateText("Phylactery Shard", "A shard from the Lich's destroyed phylactery."),
			iconName: "icon_phylactery_shard",
			category: ItemCategory.Quest,
			type: ItemType.Quest,
			rarity: RarityType.Legendary,
			buyPrice: 0,
			sellPrice: 0,
			isStackable: false,
			flavorText: "It still pulses with dark energy."
		));

		db.RegisterProto(new ItemProto(
			id: Ids.Items.Quest.NecronomiconPage,
			text: Proto.CreateText("Necronomicon Page", "A page torn from the forbidden Necronomicon."),
			iconName: "icon_page_dark",
			category: ItemCategory.Quest,
			type: ItemType.Quest,
			rarity: RarityType.Epic,
			buyPrice: 0,
			sellPrice: 0,
			isStackable: true,
			maxStackSize: 10
		));

		db.RegisterProto(new ItemProto(
			id: Ids.Items.Quest.DarkTomePage,
			text: Proto.CreateText("Dark Tome Page", "A page from a dark ritual book."),
			iconName: "icon_page_ritual",
			category: ItemCategory.Quest,
			type: ItemType.Quest,
			rarity: RarityType.Uncommon,
			buyPrice: 0,
			sellPrice: 10,
			isStackable: true,
			maxStackSize: 20
		));

		db.RegisterProto(new ItemProto(
				id: Ids.Items.Quest.AncientRelic,
				text: Proto.CreateText("Ancient Relic", "A mysterious artifact from a lost civilization."),
				iconName: "icon_quest_relic",
				category: ItemCategory.Quest,
				type: ItemType.Quest,
				rarity: RarityType.Epic,
				buyPrice: 0,
				sellPrice: 0,
				isStackable: false,
				flavorText: "Its purpose remains unknown."
			));

		db.RegisterProto(new ItemProto(
				id: Ids.Items.Quest.FogClue,
				text: Proto.CreateText("Fog Clue", "A cryptic clue about the nature of the Fog."),
				iconName: "icon_quest_fog_clue",
				category: ItemCategory.Quest,
				type: ItemType.Quest,
				rarity: RarityType.Legendary,
				buyPrice: 0,
				sellPrice: 0,
				isStackable: true,
				maxStackSize: 5,
				flavorText: "Piece together the truth about what happened."
			));
	}

	#endregion
}
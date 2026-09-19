using RPGGame.Core.Prototypes;
using RPGGame.Core.Prototypes.Skills;
using RPGGame.Core.Stats;

namespace RPGGame.Core.Skills;

/// <summary>
/// Defines all skills using the Proto system.
/// Skills are trainable abilities that provide passive bonuses or active effects.
/// </summary>
public class SkillDefinitions : ICoreData {
	public void GameData(GameDb gameDatabase) {
		RegisterCombatSkills(gameDatabase);
		RegisterPhysicalSkills(gameDatabase);
		RegisterStealthSkills(gameDatabase);
		RegisterAwarenessSkills(gameDatabase);
		RegisterSurvivalSkills(gameDatabase);
		RegisterGatheringSkills(gameDatabase);
		RegisterCraftingSkills(gameDatabase);
		RegisterSocialSkills(gameDatabase);
		RegisterKnowledgeSkills(gameDatabase);
		RegisterMagicSkills(gameDatabase);
	}

	#region Combat Skills

	private void RegisterCombatSkills(GameDb db) {
		// Melee Combat
		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Combat.OneHandedWeapons,
			text: Proto.CreateText("One-Handed Weapons", "Proficiency with swords, axes, maces, and other one-handed weapons."),
			iconName: "icon_skill_onehanded",
			maxRank: 10,
			baseCost: 8
		) {
			Tags = [Ids.Tags.Meta.CombatSkill, Ids.Tags.Combat.Melee, Ids.Tags.Attribute.Strength],
			ModifiersPerRank = new() {
				{ Ids.Stats.Combat.AttackBonus, 1.Flat() }
			}
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Combat.TwoHandedWeapons,
			text: Proto.CreateText("Two-Handed Weapons", "Proficiency with greatswords, greataxes, and other two-handed weapons."),
			iconName: "icon_skill_twohanded",
			maxRank: 10,
			baseCost: 8
		) {
			Tags = [Ids.Tags.Meta.CombatSkill, Ids.Tags.Combat.Melee, Ids.Tags.Weapon.TwoHanded, Ids.Tags.Attribute.Strength],
			ModifiersPerRank = new() {
				{ Ids.Stats.Combat.AttackBonus, 1.Flat() },
				{ Ids.Stats.Combat.DamBonus, 1.Flat() }
			}
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Combat.DualWield,
			text: Proto.CreateText("Dual Wield", "The art of fighting with a weapon in each hand."),
			iconName: "icon_skill_dualwield",
			maxRank: 10,
			baseCost: 10,
			requiredLevel: 3
		) {
			Tags = [Ids.Tags.Meta.CombatSkill, Ids.Tags.Combat.Melee, Ids.Tags.Combat.DualWield, Ids.Tags.Attribute.Dexterity],
			ModifiersPerRank = new() {
				{ Ids.Stats.Combat.AttackBonus, 1.Flat() }
			}
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Combat.Shields,
			text: Proto.CreateText("Shields", "Proficiency with shields for defense and shield bashing."),
			iconName: "icon_skill_shield",
			maxRank: 10,
			baseCost: 6
		) {
			Tags = [Ids.Tags.Meta.CombatSkill, Ids.Tags.Combat.Defense, Ids.Tags.Weapon.Shield, Ids.Tags.Attribute.Strength],
			ModifiersPerRank = new() {
				{ Ids.Stats.Combat.BlockChance, 2.Flat() },
				{ Ids.Stats.Combat.ArmorClass, 1.Flat() }
			}
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Combat.Unarmed,
			text: Proto.CreateText("Unarmed Combat", "Fighting with fists, kicks, and martial arts."),
			iconName: "icon_skill_unarmed",
			maxRank: 10,
			baseCost: 8
		) {
			Tags = [Ids.Tags.Meta.CombatSkill, Ids.Tags.Combat.Melee, Ids.Tags.Combat.Unarmed, Ids.Tags.Attribute.Dexterity],
			ModifiersPerRank = new() {
				{ Ids.Stats.Combat.AttackBonus, 1.Flat() }
			}
		});

		// Ranged Combat
		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Combat.Archery,
			text: Proto.CreateText("Archery", "Proficiency with bows and crossbows."),
			iconName: "icon_skill_archery",
			maxRank: 10,
			baseCost: 8
		) {
			Tags = [Ids.Tags.Meta.CombatSkill, Ids.Tags.Combat.Ranged, Ids.Tags.Weapon.Bow, Ids.Tags.Attribute.Dexterity],
			ModifiersPerRank = new() {
				{ Ids.Stats.Combat.AttackBonus, 1.Flat() }
			}
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Combat.Throwing,
			text: Proto.CreateText("Throwing", "Proficiency with thrown weapons like daggers, axes, and javelins."),
			iconName: "icon_skill_throwing",
			maxRank: 10,
			baseCost: 6
		) {
			Tags = [Ids.Tags.Meta.CombatSkill, Ids.Tags.Combat.Ranged, Ids.Tags.Weapon.Thrown, Ids.Tags.Attribute.Dexterity],
			ModifiersPerRank = new() {
				{ Ids.Stats.Combat.AttackBonus, 1.Flat() }
			}
		});

		// Defensive
		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Combat.ArmorProficiency,
			text: Proto.CreateText("Armor Proficiency", "Reduces armor penalties and improves protection."),
			iconName: "icon_skill_armor",
			maxRank: 10,
			baseCost: 6
		) {
			Tags = [Ids.Tags.Meta.CombatSkill, Ids.Tags.Combat.Defense, Ids.Tags.Attribute.Constitution],
			ModifiersPerRank = new() {
				{ Ids.Stats.Combat.ArmorClass, 1.Flat() }
			}
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Combat.Dodge,
			text: Proto.CreateText("Dodge", "The ability to avoid attacks through agility."),
			iconName: "icon_skill_dodge",
			maxRank: 10,
			baseCost: 8
		) {
			Tags = [Ids.Tags.Meta.CombatSkill, Ids.Tags.Combat.Defense, Ids.Tags.Attribute.Dexterity],
			ModifiersPerRank = new() {
				{ Ids.Stats.Combat.DodgeChance, 2.Flat() }
			}
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Combat.Parry,
			text: Proto.CreateText("Parry", "Deflecting attacks with your weapon."),
			iconName: "icon_skill_parry",
			maxRank: 10,
			baseCost: 8,
			requiredLevel: 2
		) {
			Tags = [Ids.Tags.Meta.CombatSkill, Ids.Tags.Combat.Defense, Ids.Tags.Attribute.Dexterity],
			ModifiersPerRank = new() {
				{ Ids.Stats.Combat.ParryChance, 2.Flat() }
			}
		});

		// Tactics
		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Combat.Tactics,
			text: Proto.CreateText("Tactics", "Combat awareness and strategic positioning."),
			iconName: "icon_skill_tactics",
			maxRank: 10,
			baseCost: 10,
			requiredLevel: 3
		) {
			Tags = [Ids.Tags.Meta.CombatSkill, Ids.Tags.Combat.Tactics, Ids.Tags.Attribute.Intelligence],
			ModifiersPerRank = new() {
				{ Ids.Stats.Combat.Initiative, 1.Flat() }
			}
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Combat.CriticalStrike,
			text: Proto.CreateText("Critical Strike", "Finding and exploiting weak points in enemies."),
			iconName: "icon_skill_critical",
			maxRank: 10,
			baseCost: 10,
			requiredLevel: 4
		) {
			Tags = [Ids.Tags.Meta.CombatSkill, Ids.Tags.Combat.Offense, Ids.Tags.Attribute.Dexterity],
			ModifiersPerRank = new() {
				{ Ids.Stats.Combat.CriticalChance, 2.Flat() },
				{ Ids.Stats.Combat.CriticalDamage, 5.PercentIncrease() }
			}
		});
	}

	#endregion

	#region Physical Skills

	private void RegisterPhysicalSkills(GameDb db) {
		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Physical.Athletics,
			text: Proto.CreateText("Athletics", "Running, jumping, climbing, and feats of strength."),
			iconName: "icon_skill_athletics",
			maxRank: 10,
			baseCost: 5
		) {
			Tags = [Ids.Tags.Meta.PhysicalSkill, Ids.Tags.Physical.Athletics, Ids.Tags.Attribute.Strength],
			ModifiersPerRank = new() {
				{ Ids.Stats.Movement.MovementSpeed, 2.PercentIncrease() }
			}
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Physical.Acrobatics,
			text: Proto.CreateText("Acrobatics", "Balance, tumbling, and agile movement."),
			iconName: "icon_skill_acrobatics",
			maxRank: 10,
			baseCost: 5
		) {
			Tags = [Ids.Tags.Meta.PhysicalSkill, Ids.Tags.Physical.Acrobatics, Ids.Tags.Attribute.Dexterity],
			ModifiersPerRank = new() {
				{ Ids.Stats.Combat.DodgeChance, 1.Flat() }
			}
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Physical.Endurance,
			text: Proto.CreateText("Endurance", "Physical stamina and resistance to fatigue."),
			iconName: "icon_skill_endurance",
			maxRank: 10,
			baseCost: 5
		) {
			Tags = [Ids.Tags.Meta.PhysicalSkill, Ids.Tags.Physical.Endurance, Ids.Tags.Attribute.Constitution],
			ModifiersPerRank = new() {
				{ Ids.Stats.Resource.MaxStamina, 5.Flat() },
				{ Ids.Stats.Regeneration.Stamina, 1.Flat() }
			}
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Physical.Swimming,
			text: Proto.CreateText("Swimming", "Moving through water efficiently."),
			iconName: "icon_skill_swimming",
			maxRank: 5,
			baseCost: 3
		) {
			Tags = [Ids.Tags.Meta.PhysicalSkill, Ids.Tags.Physical.Swimming, Ids.Tags.Attribute.Constitution]
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Physical.Climbing,
			text: Proto.CreateText("Climbing", "Scaling walls, cliffs, and other surfaces."),
			iconName: "icon_skill_climbing",
			maxRank: 5,
			baseCost: 3
		) {
			Tags = [Ids.Tags.Meta.PhysicalSkill, Ids.Tags.Physical.Climbing, Ids.Tags.Attribute.Strength]
		});
	}

	#endregion

	#region Stealth Skills

	private void RegisterStealthSkills(GameDb db) {
		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Stealth.Sneaking,
			text: Proto.CreateText("Sneaking", "Moving silently and avoiding detection."),
			iconName: "icon_skill_sneaking",
			maxRank: 10,
			baseCost: 6
		) {
			Tags = [Ids.Tags.Meta.StealthSkill, Ids.Tags.Stealth.Sneaking, Ids.Tags.Attribute.Dexterity]
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Stealth.Hiding,
			text: Proto.CreateText("Hiding", "Concealing yourself in shadows and cover."),
			iconName: "icon_skill_hiding",
			maxRank: 10,
			baseCost: 6
		) {
			Tags = [Ids.Tags.Meta.StealthSkill, Ids.Tags.Stealth.Hiding, Ids.Tags.Attribute.Dexterity]
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Stealth.Lockpicking,
			text: Proto.CreateText("Lockpicking", "Opening locks without keys."),
			iconName: "icon_skill_lockpicking",
			maxRank: 10,
			baseCost: 6
		) {
			Tags = [Ids.Tags.Meta.StealthSkill, Ids.Tags.Stealth.Lockpicking, Ids.Tags.Attribute.Dexterity]
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Stealth.Pickpocket,
			text: Proto.CreateText("Pickpocket", "Stealing items from pockets and pouches."),
			iconName: "icon_skill_pickpocket",
			maxRank: 10,
			baseCost: 6,
			requiredLevel: 2
		) {
			Tags = [Ids.Tags.Meta.StealthSkill, Ids.Tags.Stealth.Pickpocket, Ids.Tags.Attribute.Dexterity]
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Stealth.TrapHandling,
			text: Proto.CreateText("Trap Handling", "Detecting, disarming, and setting traps."),
			iconName: "icon_skill_traps",
			maxRank: 10,
			baseCost: 6
		) {
			Tags = [Ids.Tags.Meta.StealthSkill, Ids.Tags.Stealth.TrapHandling, Ids.Tags.Attribute.Intelligence]
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Stealth.SleightOfHand,
			text: Proto.CreateText("Sleight of Hand", "Manual dexterity for tricks and deception."),
			iconName: "icon_skill_sleightofhand",
			maxRank: 10,
			baseCost: 5
		) {
			Tags = [Ids.Tags.Meta.StealthSkill, Ids.Tags.Stealth.SleightOfHand, Ids.Tags.Attribute.Dexterity]
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Stealth.Disguise,
			text: Proto.CreateText("Disguise", "Creating and maintaining false appearances."),
			iconName: "icon_skill_disguise",
			maxRank: 5,
			baseCost: 4,
			requiredLevel: 3
		) {
			Tags = [Ids.Tags.Meta.StealthSkill, Ids.Tags.Stealth.Disguise, Ids.Tags.Attribute.Charisma]
		});
	}

	#endregion

	#region Awareness Skills

	private void RegisterAwarenessSkills(GameDb db) {
		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Awareness.Perception,
			text: Proto.CreateText("Perception", "Noticing details, spotting hidden things, and awareness."),
			iconName: "icon_skill_perception",
			maxRank: 10,
			baseCost: 5
		) {
			Tags = [Ids.Tags.Meta.PhysicalSkill, Ids.Tags.Awareness.Perception, Ids.Tags.Attribute.Wisdom],
			ModifiersPerRank = new() {
				{ Ids.Stats.Expedition.VisionRange, 1.Flat() }
			}
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Awareness.Insight,
			text: Proto.CreateText("Insight", "Reading people and detecting lies."),
			iconName: "icon_skill_insight",
			maxRank: 10,
			baseCost: 5
		) {
			Tags = [Ids.Tags.Meta.SocialSkill, Ids.Tags.Awareness.Insight, Ids.Tags.Attribute.Wisdom]
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Awareness.Investigation,
			text: Proto.CreateText("Investigation", "Finding clues and deducing information."),
			iconName: "icon_skill_investigation",
			maxRank: 10,
			baseCost: 5
		) {
			Tags = [Ids.Tags.Meta.KnowledgeSkill, Ids.Tags.Awareness.Investigation, Ids.Tags.Attribute.Intelligence]
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Awareness.Tracking,
			text: Proto.CreateText("Tracking", "Following trails and tracking creatures."),
			iconName: "icon_skill_tracking",
			maxRank: 10,
			baseCost: 5
		) {
			Tags = [Ids.Tags.Meta.SurvivalSkill, Ids.Tags.Awareness.Tracking, Ids.Tags.Attribute.Wisdom]
		});
	}

	#endregion

	#region Survival Skills

	private void RegisterSurvivalSkills(GameDb db) {
		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Survival.General,
			text: Proto.CreateText("Survival", "General wilderness survival and outdoor skills."),
			iconName: "icon_skill_survival",
			maxRank: 10,
			baseCost: 5
		) {
			Tags = [Ids.Tags.Meta.SurvivalSkill, Ids.Tags.Survival.General, Ids.Tags.Attribute.Wisdom],
			ModifiersPerRank = new() {
				{ Ids.Stats.Expedition.FoodConsumption, 2.PercentReduce() }
			}
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Survival.AnimalHandling,
			text: Proto.CreateText("Animal Handling", "Calming, training, and working with animals."),
			iconName: "icon_skill_animalhandling",
			maxRank: 10,
			baseCost: 5
		) {
			Tags = [Ids.Tags.Meta.SurvivalSkill, Ids.Tags.Survival.AnimalHandling, Ids.Tags.Attribute.Wisdom]
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Survival.Navigation,
			text: Proto.CreateText("Navigation", "Finding your way and avoiding getting lost."),
			iconName: "icon_skill_navigation",
			maxRank: 10,
			baseCost: 5
		) {
			Tags = [Ids.Tags.Meta.SurvivalSkill, Ids.Tags.Survival.Navigation, Ids.Tags.Attribute.Intelligence],
			ModifiersPerRank = new() {
				{ Ids.Stats.Expedition.TravelSpeed, 2.PercentIncrease() }
			}
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Survival.Camping,
			text: Proto.CreateText("Camping", "Setting up camps and resting efficiently."),
			iconName: "icon_skill_camping",
			maxRank: 5,
			baseCost: 4
		) {
			Tags = [Ids.Tags.Meta.SurvivalSkill, Ids.Tags.Survival.Camping, Ids.Tags.Attribute.Wisdom]
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Survival.WeatherSense,
			text: Proto.CreateText("Weather Sense", "Predicting and preparing for weather changes."),
			iconName: "icon_skill_weather",
			maxRank: 5,
			baseCost: 3
		) {
			Tags = [Ids.Tags.Meta.SurvivalSkill, Ids.Tags.Survival.WeatherSense, Ids.Tags.Attribute.Wisdom]
		});
	}

	#endregion

	#region Gathering Skills

	private void RegisterGatheringSkills(GameDb db) {
		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Gathering.Mining,
			text: Proto.CreateText("Mining", "Extracting ores, gems, and minerals from stone."),
			iconName: "icon_skill_mining",
			maxRank: 10,
			baseCost: 5
		) {
			Tags = [Ids.Tags.Meta.CraftingSkill, Ids.Tags.Crafting.Mining, Ids.Tags.Attribute.Strength],
			ModifiersPerRank = new() {
				{ Ids.Stats.Economy.GatheringSpeed, 3.PercentIncrease() }
			}
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Gathering.Herbalism,
			text: Proto.CreateText("Herbalism", "Gathering and identifying herbs and plants."),
			iconName: "icon_skill_herbalism",
			maxRank: 10,
			baseCost: 5
		) {
			Tags = [Ids.Tags.Meta.CraftingSkill, Ids.Tags.Survival.Herbalism, Ids.Tags.Attribute.Wisdom],
			ModifiersPerRank = new() {
				{ Ids.Stats.Economy.GatheringSpeed, 3.PercentIncrease() }
			}
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Gathering.Woodcutting,
			text: Proto.CreateText("Woodcutting", "Felling trees and collecting wood."),
			iconName: "icon_skill_woodcutting",
			maxRank: 10,
			baseCost: 5
		) {
			Tags = [Ids.Tags.Meta.CraftingSkill, Ids.Tags.Crafting.Woodworking, Ids.Tags.Attribute.Strength],
			ModifiersPerRank = new() {
				{ Ids.Stats.Economy.GatheringSpeed, 3.PercentIncrease() }
			}
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Gathering.Fishing,
			text: Proto.CreateText("Fishing", "Catching fish from rivers, lakes, and seas."),
			iconName: "icon_skill_fishing",
			maxRank: 10,
			baseCost: 5
		) {
			Tags = [Ids.Tags.Meta.SurvivalSkill, Ids.Tags.Survival.Fishing, Ids.Tags.Attribute.Wisdom],
			ModifiersPerRank = new() {
				{ Ids.Stats.Economy.GatheringSpeed, 3.PercentIncrease() }
			}
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Gathering.Foraging,
			text: Proto.CreateText("Foraging", "Finding edible plants, berries, and mushrooms."),
			iconName: "icon_skill_foraging",
			maxRank: 10,
			baseCost: 5
		) {
			Tags = [Ids.Tags.Meta.SurvivalSkill, Ids.Tags.Survival.Foraging, Ids.Tags.Attribute.Wisdom],
			ModifiersPerRank = new() {
				{ Ids.Stats.Economy.GatheringSpeed, 3.PercentIncrease() }
			}
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Gathering.Skinning,
			text: Proto.CreateText("Skinning", "Harvesting hides, pelts, and parts from animals."),
			iconName: "icon_skill_skinning",
			maxRank: 10,
			baseCost: 5
		) {
			Tags = [Ids.Tags.Meta.SurvivalSkill, Ids.Tags.Survival.Skinning, Ids.Tags.Attribute.Dexterity],
			ModifiersPerRank = new() {
				{ Ids.Stats.Economy.GatheringSpeed, 3.PercentIncrease() }
			}
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Gathering.Hunting,
			text: Proto.CreateText("Hunting", "Tracking and killing game animals."),
			iconName: "icon_skill_hunting",
			maxRank: 10,
			baseCost: 6
		) {
			Tags = [Ids.Tags.Meta.SurvivalSkill, Ids.Tags.Survival.Hunting, Ids.Tags.Attribute.Dexterity]
		});
	}

	#endregion

	#region Crafting Skills

	private void RegisterCraftingSkills(GameDb db) {
		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Crafting.Smithing,
			text: Proto.CreateText("Smithing", "Forging weapons, armor, and metal items."),
			iconName: "icon_skill_smithing",
			maxRank: 10,
			baseCost: 8
		) {
			Tags = [Ids.Tags.Meta.CraftingSkill, Ids.Tags.Crafting.Smithing, Ids.Tags.Attribute.Strength],
			ModifiersPerRank = new() {
				{ Ids.Stats.Economy.CraftingSpeed, 3.PercentIncrease() }
			}
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Crafting.Alchemy,
			text: Proto.CreateText("Alchemy", "Brewing potions, elixirs, and alchemical substances."),
			iconName: "icon_skill_alchemy",
			maxRank: 10,
			baseCost: 8
		) {
			Tags = [Ids.Tags.Meta.CraftingSkill, Ids.Tags.Crafting.Alchemy, Ids.Tags.Attribute.Intelligence],
			ModifiersPerRank = new() {
				{ Ids.Stats.Economy.CraftingSpeed, 3.PercentIncrease() }
			}
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Crafting.Enchanting,
			text: Proto.CreateText("Enchanting", "Imbuing items with magical properties."),
			iconName: "icon_skill_enchanting",
			maxRank: 10,
			baseCost: 10,
			requiredLevel: 5
		) {
			Tags = [Ids.Tags.Meta.CraftingSkill, Ids.Tags.Crafting.Enchanting, Ids.Tags.Attribute.Intelligence],
			ModifiersPerRank = new() {
				{ Ids.Stats.Economy.CraftingSpeed, 3.PercentIncrease() }
			}
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Crafting.Cooking,
			text: Proto.CreateText("Cooking", "Preparing food and beneficial meals."),
			iconName: "icon_skill_cooking",
			maxRank: 10,
			baseCost: 5
		) {
			Tags = [Ids.Tags.Meta.CraftingSkill, Ids.Tags.Crafting.Cooking, Ids.Tags.Attribute.Wisdom],
			ModifiersPerRank = new() {
				{ Ids.Stats.Economy.CraftingSpeed, 3.PercentIncrease() }
			}
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Crafting.Leatherworking,
			text: Proto.CreateText("Leatherworking", "Crafting leather armor and items."),
			iconName: "icon_skill_leatherworking",
			maxRank: 10,
			baseCost: 6
		) {
			Tags = [Ids.Tags.Meta.CraftingSkill, Ids.Tags.Crafting.Leatherworking, Ids.Tags.Attribute.Dexterity],
			ModifiersPerRank = new() {
				{ Ids.Stats.Economy.CraftingSpeed, 3.PercentIncrease() }
			}
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Crafting.Tailoring,
			text: Proto.CreateText("Tailoring", "Making cloth armor and garments."),
			iconName: "icon_skill_tailoring",
			maxRank: 10,
			baseCost: 5
		) {
			Tags = [Ids.Tags.Meta.CraftingSkill, Ids.Tags.Crafting.Tailoring, Ids.Tags.Attribute.Dexterity],
			ModifiersPerRank = new() {
				{ Ids.Stats.Economy.CraftingSpeed, 3.PercentIncrease() }
			}
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Crafting.Woodworking,
			text: Proto.CreateText("Woodworking", "Crafting bows, staves, and wooden items."),
			iconName: "icon_skill_woodworking",
			maxRank: 10,
			baseCost: 5
		) {
			Tags = [Ids.Tags.Meta.CraftingSkill, Ids.Tags.Crafting.Woodworking, Ids.Tags.Attribute.Dexterity],
			ModifiersPerRank = new() {
				{ Ids.Stats.Economy.CraftingSpeed, 3.PercentIncrease() }
			}
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Crafting.Jewelcrafting,
			text: Proto.CreateText("Jewelcrafting", "Creating rings, amulets, and gem-encrusted items."),
			iconName: "icon_skill_jewelcrafting",
			maxRank: 10,
			baseCost: 8,
			requiredLevel: 3
		) {
			Tags = [Ids.Tags.Meta.CraftingSkill, Ids.Tags.Crafting.Jewelcrafting, Ids.Tags.Attribute.Dexterity],
			ModifiersPerRank = new() {
				{ Ids.Stats.Economy.CraftingSpeed, 3.PercentIncrease() }
			}
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Crafting.Scribing,
			text: Proto.CreateText("Scribing", "Writing scrolls and copying spell books."),
			iconName: "icon_skill_scribing",
			maxRank: 10,
			baseCost: 6,
			requiredLevel: 2
		) {
			Tags = [Ids.Tags.Meta.CraftingSkill, Ids.Tags.Crafting.Scribing, Ids.Tags.Attribute.Intelligence],
			ModifiersPerRank = new() {
				{ Ids.Stats.Economy.CraftingSpeed, 3.PercentIncrease() }
			}
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Crafting.Runecraft,
			text: Proto.CreateText("Runecraft", "Inscribing magical runes on items."),
			iconName: "icon_skill_runecraft",
			maxRank: 10,
			baseCost: 10,
			requiredLevel: 5
		) {
			Tags = [Ids.Tags.Meta.CraftingSkill, Ids.Tags.Crafting.Runecraft, Ids.Tags.Attribute.Intelligence],
			ModifiersPerRank = new() {
				{ Ids.Stats.Economy.CraftingSpeed, 3.PercentIncrease() }
			}
		});
	}

	#endregion

	#region Social Skills

	private void RegisterSocialSkills(GameDb db) {
		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Social.Persuasion,
			text: Proto.CreateText("Persuasion", "Convincing others through charm and logic."),
			iconName: "icon_skill_persuasion",
			maxRank: 10,
			baseCost: 5
		) {
			Tags = [Ids.Tags.Meta.SocialSkill, Ids.Tags.Social.Persuasion, Ids.Tags.Attribute.Charisma]
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Social.Intimidation,
			text: Proto.CreateText("Intimidation", "Coercing others through threats and presence."),
			iconName: "icon_skill_intimidation",
			maxRank: 10,
			baseCost: 5
		) {
			Tags = [Ids.Tags.Meta.SocialSkill, Ids.Tags.Social.Intimidation, Ids.Tags.Attribute.Charisma]
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Social.Deception,
			text: Proto.CreateText("Deception", "Lying, bluffing, and misleading others."),
			iconName: "icon_skill_deception",
			maxRank: 10,
			baseCost: 5
		) {
			Tags = [Ids.Tags.Meta.SocialSkill, Ids.Tags.Social.Deception, Ids.Tags.Attribute.Charisma]
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Social.Performance,
			text: Proto.CreateText("Performance", "Entertaining through music, dance, or acting."),
			iconName: "icon_skill_performance",
			maxRank: 10,
			baseCost: 4
		) {
			Tags = [Ids.Tags.Meta.SocialSkill, Ids.Tags.Social.Performance, Ids.Tags.Attribute.Charisma]
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Social.Mercantile,
			text: Proto.CreateText("Mercantile", "Haggling, trading, and appraising goods."),
			iconName: "icon_skill_mercantile",
			maxRank: 10,
			baseCost: 5
		) {
			Tags = [Ids.Tags.Meta.SocialSkill, Ids.Tags.Social.Mercantile, Ids.Tags.Attribute.Charisma],
			ModifiersPerRank = new() {
				{ Ids.Stats.Economy.GoldFind, 3.PercentIncrease() }
			}
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Social.Leadership,
			text: Proto.CreateText("Leadership", "Inspiring and commanding others."),
			iconName: "icon_skill_leadership",
			maxRank: 10,
			baseCost: 8,
			requiredLevel: 3
		) {
			Tags = [Ids.Tags.Meta.SocialSkill, Ids.Tags.Social.Leadership, Ids.Tags.Attribute.Charisma]
		});
	}

	#endregion

	#region Knowledge Skills

	private void RegisterKnowledgeSkills(GameDb db) {
		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Knowledge.Arcana,
			text: Proto.CreateText("Arcana", "Knowledge of magic, spells, and the arcane."),
			iconName: "icon_skill_arcana",
			maxRank: 10,
			baseCost: 6
		) {
			Tags = [Ids.Tags.Meta.KnowledgeSkill, Ids.Tags.Knowledge.Arcana, Ids.Tags.Attribute.Intelligence],
			ModifiersPerRank = new() {
				{ Ids.Stats.Combat.SpellPower, 1.Flat() }
			}
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Knowledge.History,
			text: Proto.CreateText("History", "Knowledge of past events and ancient lore."),
			iconName: "icon_skill_history",
			maxRank: 10,
			baseCost: 4
		) {
			Tags = [Ids.Tags.Meta.KnowledgeSkill, Ids.Tags.Knowledge.History, Ids.Tags.Attribute.Intelligence]
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Knowledge.Religion,
			text: Proto.CreateText("Religion", "Knowledge of gods, rituals, and holy texts."),
			iconName: "icon_skill_religion",
			maxRank: 10,
			baseCost: 5
		) {
			Tags = [Ids.Tags.Meta.KnowledgeSkill, Ids.Tags.Knowledge.Religion, Ids.Tags.Attribute.Wisdom]
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Knowledge.Nature,
			text: Proto.CreateText("Nature", "Knowledge of plants, animals, and the natural world."),
			iconName: "icon_skill_nature",
			maxRank: 10,
			baseCost: 5
		) {
			Tags = [Ids.Tags.Meta.KnowledgeSkill, Ids.Tags.Knowledge.Nature, Ids.Tags.Attribute.Intelligence]
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Knowledge.Medicine,
			text: Proto.CreateText("Medicine", "Knowledge of healing, anatomy, and diseases."),
			iconName: "icon_skill_medicine",
			maxRank: 10,
			baseCost: 6
		) {
			Tags = [Ids.Tags.Meta.KnowledgeSkill, Ids.Tags.Knowledge.Medicine, Ids.Tags.Attribute.Wisdom],
			ModifiersPerRank = new() {
				{ Ids.Stats.Regeneration.Health, 1.Flat() }
			}
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Knowledge.Dungeoneering,
			text: Proto.CreateText("Dungeoneering", "Knowledge of dungeons, caves, and underground."),
			iconName: "icon_skill_dungeoneering",
			maxRank: 10,
			baseCost: 5
		) {
			Tags = [Ids.Tags.Meta.KnowledgeSkill, Ids.Tags.Knowledge.Dungeoneering, Ids.Tags.Attribute.Intelligence]
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Knowledge.Planes,
			text: Proto.CreateText("Planes", "Knowledge of other planes of existence."),
			iconName: "icon_skill_planes",
			maxRank: 10,
			baseCost: 6,
			requiredLevel: 5
		) {
			Tags = [Ids.Tags.Meta.KnowledgeSkill, Ids.Tags.Knowledge.Planes, Ids.Tags.Attribute.Intelligence]
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Knowledge.FogLore,
			text: Proto.CreateText("Fog Lore", "Understanding the mysteries of the Fog."),
			iconName: "icon_skill_foglore",
			maxRank: 10,
			baseCost: 8,
			requiredLevel: 3
		) {
			Tags = [Ids.Tags.Meta.KnowledgeSkill, Ids.Tags.Knowledge.Fog, Ids.Tags.Attribute.Wisdom]
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Knowledge.TimeLore,
			text: Proto.CreateText("Time Lore", "Understanding temporal magic and anomalies."),
			iconName: "icon_skill_timelore",
			maxRank: 10,
			baseCost: 10,
			requiredLevel: 5
		) {
			Tags = [Ids.Tags.Meta.KnowledgeSkill, Ids.Tags.Knowledge.Time, Ids.Tags.Attribute.Intelligence],
			RequiredTags = [Ids.Tags.Class.TimeWalker]
		});
	}

	#endregion

	#region Magic Skills

	private void RegisterMagicSkills(GameDb db) {
		// Spellcasting Schools
		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Magic.Evocation,
			text: Proto.CreateText("Evocation", "Mastery of destructive elemental magic."),
			iconName: "icon_skill_evocation",
			maxRank: 10,
			baseCost: 10,
			requiredLevel: 2
		) {
			Tags = [Ids.Tags.Meta.MagicSkill, Ids.Tags.School.Evocation, Ids.Tags.Attribute.Intelligence],
			ModifiersPerRank = new() {
				{ Ids.Stats.Combat.SpellPower, 2.Flat() }
			}
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Magic.Abjuration,
			text: Proto.CreateText("Abjuration", "Mastery of protective and warding magic."),
			iconName: "icon_skill_abjuration",
			maxRank: 10,
			baseCost: 10,
			requiredLevel: 2
		) {
			Tags = [Ids.Tags.Meta.MagicSkill, Ids.Tags.School.Abjuration, Ids.Tags.Attribute.Intelligence],
			ModifiersPerRank = new() {
				{ Ids.Stats.Resistances.Magical, 2.Flat() }
			}
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Magic.Conjuration,
			text: Proto.CreateText("Conjuration", "Mastery of summoning and teleportation."),
			iconName: "icon_skill_conjuration",
			maxRank: 10,
			baseCost: 10,
			requiredLevel: 3
		) {
			Tags = [Ids.Tags.Meta.MagicSkill, Ids.Tags.School.Conjuration, Ids.Tags.Attribute.Intelligence]
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Magic.Divination,
			text: Proto.CreateText("Divination", "Mastery of scrying and foresight."),
			iconName: "icon_skill_divination",
			maxRank: 10,
			baseCost: 8,
			requiredLevel: 2
		) {
			Tags = [Ids.Tags.Meta.MagicSkill, Ids.Tags.School.Divination, Ids.Tags.Attribute.Wisdom]
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Magic.Enchantment,
			text: Proto.CreateText("Enchantment", "Mastery of charm and mind control."),
			iconName: "icon_skill_enchantment",
			maxRank: 10,
			baseCost: 10,
			requiredLevel: 3
		) {
			Tags = [Ids.Tags.Meta.MagicSkill, Ids.Tags.School.Enchantment, Ids.Tags.Attribute.Charisma]
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Magic.Illusion,
			text: Proto.CreateText("Illusion", "Mastery of deception and trickery magic."),
			iconName: "icon_skill_illusion",
			maxRank: 10,
			baseCost: 8,
			requiredLevel: 2
		) {
			Tags = [Ids.Tags.Meta.MagicSkill, Ids.Tags.School.Illusion, Ids.Tags.Attribute.Intelligence]
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Magic.Necromancy,
			text: Proto.CreateText("Necromancy", "Mastery of death and undeath magic."),
			iconName: "icon_skill_necromancy",
			maxRank: 10,
			baseCost: 10,
			requiredLevel: 4
		) {
			Tags = [Ids.Tags.Meta.MagicSkill, Ids.Tags.School.Necromancy, Ids.Tags.Attribute.Intelligence],
			ModifiersPerRank = new() {
				{ Ids.Stats.Resistances.Necrotic, 3.Flat() }
			}
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Magic.Transmutation,
			text: Proto.CreateText("Transmutation", "Mastery of transformation magic."),
			iconName: "icon_skill_transmutation",
			maxRank: 10,
			baseCost: 8,
			requiredLevel: 2
		) {
			Tags = [Ids.Tags.Meta.MagicSkill, Ids.Tags.School.Transmutation, Ids.Tags.Attribute.Intelligence]
		});

		// Divine Magic
		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Magic.HolyMagic,
			text: Proto.CreateText("Holy Magic", "Channeling divine power for healing and smiting."),
			iconName: "icon_skill_holy",
			maxRank: 10,
			baseCost: 10,
			requiredLevel: 2
		) {
			Tags = [Ids.Tags.Meta.MagicSkill, Ids.Tags.School.Holy, Ids.Tags.Attribute.Wisdom],
			ModifiersPerRank = new() {
				{ Ids.Stats.Resistances.Holy, 3.Flat() }
			}
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Magic.NatureMagic,
			text: Proto.CreateText("Nature Magic", "Drawing power from the natural world."),
			iconName: "icon_skill_naturemagic",
			maxRank: 10,
			baseCost: 10,
			requiredLevel: 2
		) {
			Tags = [Ids.Tags.Meta.MagicSkill, Ids.Tags.School.Nature, Ids.Tags.Attribute.Wisdom]
		});

		// Special Magic
		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Magic.Temporal,
			text: Proto.CreateText("Temporal Magic", "Manipulating the flow of time."),
			iconName: "icon_skill_temporal",
			maxRank: 10,
			baseCost: 15,
			requiredLevel: 7
		) {
			Tags = [Ids.Tags.Meta.MagicSkill, Ids.Tags.School.Temporal, Ids.Tags.Attribute.Intelligence],
			RequiredTags = [Ids.Tags.Class.TimeWalker]
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Magic.BloodMagic,
			text: Proto.CreateText("Blood Magic", "Harnessing life force for power."),
			iconName: "icon_skill_blood",
			maxRank: 10,
			baseCost: 12,
			requiredLevel: 5
		) {
			Tags = [Ids.Tags.Meta.MagicSkill, Ids.Tags.School.Blood, Ids.Tags.Attribute.Constitution]
		});

		// General Magic Skills
		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Magic.SpellFocus,
			text: Proto.CreateText("Spell Focus", "Concentration and mana efficiency."),
			iconName: "icon_skill_spellfocus",
			maxRank: 10,
			baseCost: 6
		) {
			Tags = [Ids.Tags.Meta.MagicSkill, Ids.Tags.Attribute.Intelligence],
			ModifiersPerRank = new() {
				{ Ids.Stats.CostMod.ManaCost, 2.Flat() }
			}
		});

		db.RegisterProto(new SkillProto(
			id: Ids.Skills.Magic.ManaPool,
			text: Proto.CreateText("Mana Pool", "Expanding your magical reserves."),
			iconName: "icon_skill_manapool",
			maxRank: 10,
			baseCost: 6
		) {
			Tags = [Ids.Tags.Meta.MagicSkill, Ids.Tags.Attribute.Intelligence],
			ModifiersPerRank = new() {
				{ Ids.Stats.Resource.MaxMana, 5.Flat() },
				{ Ids.Stats.Regeneration.Mana, 1.Flat() }
			}
		});
	}

	#endregion
}
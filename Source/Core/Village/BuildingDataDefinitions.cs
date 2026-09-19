using RPGGame.Core.Prototypes;
using RPGGame.Core.Prototypes.Village;
using RPGGame.Core.Stats;

namespace RPGGame.Core.Village;

/// <summary>
/// Defines all building data using the Proto system.
/// </summary>
public class BuildingDataDefinitions : ICoreData {
	public void GameData(GameDb gameDatabase) {
		RegisterCoreBuildings(gameDatabase);
		RegisterStarterBuildings(gameDatabase);
		RegisterUnlockableBuildings(gameDatabase);
		RegisterLateGameBuildings(gameDatabase);
		RegisterSpecialBuildings(gameDatabase);
	}

	#region Core Buildings

	private void RegisterCoreBuildings(GameDb db) {
		// ═══════════════════════════════════════════════════════════════
		// THE ANCHOR - Central artifact, always present
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new BuildingProto(
			id: Ids.Buildings.Anchor,
			text: Proto.CreateText("The Anchor", "A mysterious artifact that holds back the fog and binds you to this place. When caught in time, you return here. Its golden light pulses with ancient power."),
			iconName: "icon_anchor",
			category: BuildingCategory.Core,
			size: BuildingSize.Large,
			width: 1,
			height: 1,
			canMove: false,
			canDemolish: false,
			unlockCost: 0,
			baseUpgradeCost: 0,
			maxLevel: 1,
			startsUnlocked: true,
			providesRest: true
		));

		// ═══════════════════════════════════════════════════════════════
		// VILLAGE GATE - Exit to expeditions
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new BuildingProto(
			id: Ids.Buildings.Gate,
			text: Proto.CreateText("Village Gate", "The only safe passage out of the village. Beyond lies the fog. Upgrading improves your initial travel speed."),
			iconName: "icon_gate",
			category: BuildingCategory.Core,
			size: BuildingSize.Medium,
			width: 2,
			height: 1,
			canMove: false,
			canDemolish: false,
			unlockCost: 0,
			baseUpgradeCost: 10,
			maxLevel: 3,
			startsUnlocked: true,
			bonusesPerLevel: [
				new BuildingBonusProto(Ids.Stats.Movement.MovementSpeed, 5, ModifierOperation.PercentIncrease)
			]
		));
	}

	#endregion

	#region Starter Buildings

	private void RegisterStarterBuildings(GameDb db) {
		// ═══════════════════════════════════════════════════════════════
		// BLACKSMITH - Weapons and armor
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new BuildingProto(
			id: Ids.Buildings.Blacksmith,
			text: Proto.CreateText("Blacksmith", "Forge and repair weapons and armor. A well-equipped adventurer survives longer. Higher levels unlock better equipment and crafting options."),
			iconName: "icon_blacksmith",
			category: BuildingCategory.Production,
			size: BuildingSize.Medium,
			width: 1,
			height: 1,
			unlockCost: 0,
			baseUpgradeCost: 8,
			maxLevel: 5,
			startsUnlocked: true,
			bonusesPerLevel: [
				new BuildingBonusProto(Ids.Stats.Combat.AttackBonus, 1)
			],
			hasShop: true,
			hasNPC: true,
			npcId: "npc_blacksmith"
		));

		// ═══════════════════════════════════════════════════════════════
		// GENERAL STORE - Basic supplies
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new BuildingProto(
			id: Ids.Buildings.GeneralStore,
			text: Proto.CreateText("General Store", "Stock up on supplies for your journey. Food, medicine, and camping gear. Higher levels provide better stock and discounts."),
			iconName: "icon_store",
			category: BuildingCategory.Commerce,
			size: BuildingSize.Medium,
			width: 1,
			height: 1,
			unlockCost: 0,
			baseUpgradeCost: 6,
			maxLevel: 5,
			startsUnlocked: true,
			bonusesPerLevel: [
				new BuildingBonusProto(Ids.Stats.BuildingUpgrades.StartingGold, 5)
			],
			hasShop: true,
			hasNPC: true,
			npcId: "npc_merchant"
		));

		// ═══════════════════════════════════════════════════════════════
		// TAVERN - Rest and rumors
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new BuildingProto(
			id: Ids.Buildings.Tavern,
			text: Proto.CreateText("Tavern", "Rest, eat, and gather rumors. A warm bed does wonders for morale. Higher levels improve rest effectiveness and rumor quality."),
			iconName: "icon_tavern",
			category: BuildingCategory.Utility,
			size: BuildingSize.Medium,
			width: 1,
			height: 1,
			unlockCost: 0,
			baseUpgradeCost: 7,
			maxLevel: 5,
			startsUnlocked: true,
			bonusesPerLevel: [
				new BuildingBonusProto(Ids.Stats.BuildingUpgrades.MaxMorale, 5)
			],
			providesRest: true,
			hasNPC: true,
			npcId: "npc_innkeeper"
		));

		// ═══════════════════════════════════════════════════════════════
		// CARPENTER - Building construction
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new BuildingProto(
			id: Ids.Buildings.Carpenter,
			text: Proto.CreateText("Carpenter", "Build and upgrade village structures. Essential for village growth. Higher levels reduce construction costs."),
			iconName: "icon_carpenter",
			category: BuildingCategory.Production,
			size: BuildingSize.Medium,
			width: 1,
			height: 1,
			unlockCost: 0,
			baseUpgradeCost: 8,
			maxLevel: 5,
			startsUnlocked: true,
			bonusesPerLevel: [
				new BuildingBonusProto(Ids.Stats.BuildingUpgrades.BuildingCostReduction, 5, ModifierOperation.PercentIncrease)
			],
			hasNPC: true,
			npcId: "npc_carpenter"
		));
	}

	#endregion

	#region Unlockable Buildings

	private void RegisterUnlockableBuildings(GameDb db) {
		// ═══════════════════════════════════════════════════════════════
		// HERBALIST - Potions and medicine
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new BuildingProto(
			id: Ids.Buildings.Herbalist,
			text: Proto.CreateText("Herbalist", "Brew potions and remedies. Medicine can mean the difference between life and death. Higher levels unlock more powerful concoctions."),
			iconName: "icon_herbalist",
			category: BuildingCategory.Production,
			size: BuildingSize.Small,
			width: 1,
			height: 1,
			unlockCost: 15,
			baseUpgradeCost: 6,
			maxLevel: 5,
			startsUnlocked: false,
			bonusesPerLevel: [
				new BuildingBonusProto(Ids.Stats.Resource.MaxHealth, 5),
				new BuildingBonusProto(Ids.Stats.BuildingUpgrades.PotionEffectiveness, 5, ModifierOperation.PercentIncrease)
			],
			hasShop: true,
			hasNPC: true,
			npcId: "npc_herbalist"
		));

		// ═══════════════════════════════════════════════════════════════
		// CARTOGRAPHER - Maps and exploration
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new BuildingProto(
			id: Ids.Buildings.Cartographer,
			text: Proto.CreateText("Cartographer", "Study maps and plan routes. Knowledge of the land is power. Higher levels reveal more of the fog-shrouded world."),
			iconName: "icon_cartographer",
			category: BuildingCategory.Utility,
			size: BuildingSize.Small,
			width: 1,
			height: 1,
			unlockCost: 20,
			baseUpgradeCost: 8,
			maxLevel: 5,
			startsUnlocked: false,
			bonusesPerLevel: [
				new BuildingBonusProto(Ids.Stats.BuildingUpgrades.FogRevealRadius, 1)
			],
			hasNPC: true,
			npcId: "npc_cartographer"
		));

		// ═══════════════════════════════════════════════════════════════
		// TRAINING GROUNDS - Combat practice
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new BuildingProto(
			id: Ids.Buildings.TrainingGrounds,
			text: Proto.CreateText("Training Grounds", "Hone your combat skills. Practice makes perfect. Higher levels grant bonus experience and unlock class changes."),
			iconName: "icon_training",
			category: BuildingCategory.Training,
			size: BuildingSize.Large,
			width: 1,
			height: 1,
			unlockCost: 25,
			baseUpgradeCost: 10,
			maxLevel: 5,
			startsUnlocked: false,
			bonusesPerLevel: [
				new BuildingBonusProto(Ids.Stats.Economy.ExperienceGain, 5, ModifierOperation.PercentIncrease),
				new BuildingBonusProto(Ids.Stats.Combat.CriticalChance, 1)
			],
			hasNPC: true,
			npcId: "npc_trainer"
		));

		// ═══════════════════════════════════════════════════════════════
		// LIBRARY - Knowledge and magic
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new BuildingProto(
			id: Ids.Buildings.Library,
			text: Proto.CreateText("Library", "A repository of knowledge salvaged from the fog. Learn spells, study lore, and uncover the mysteries of the world."),
			iconName: "icon_library",
			category: BuildingCategory.Training,
			size: BuildingSize.Medium,
			width: 1,
			height: 1,
			unlockCost: 25,
			baseUpgradeCost: 10,
			maxLevel: 5,
			startsUnlocked: false,
			bonusesPerLevel: [
				new BuildingBonusProto(Ids.Stats.Resource.MaxMana, 10),
				new BuildingBonusProto(Ids.Stats.Combat.SpellPower, 1)
			],
			hasNPC: true,
			npcId: "npc_scholar"
		));

		// ═══════════════════════════════════════════════════════════════
		// STABLES - Mounts and travel
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new BuildingProto(
			id: Ids.Buildings.Stables,
			text: Proto.CreateText("Stables", "Raise and care for mounts. A good horse can mean the difference between escape and death in the fog."),
			iconName: "icon_stables",
			category: BuildingCategory.Utility,
			size: BuildingSize.Large,
			width: 1,
			height: 1,
			unlockCost: 30,
			baseUpgradeCost: 12,
			maxLevel: 5,
			startsUnlocked: false,
			bonusesPerLevel: [
				new BuildingBonusProto(Ids.Stats.Movement.MovementSpeed, 8, ModifierOperation.PercentIncrease),
				new BuildingBonusProto(Ids.Stats.Expedition.CarryCapacity, 5)
			],
			hasNPC: true,
			npcId: "npc_stablemaster"
		));

		// ═══════════════════════════════════════════════════════════════
		// HUNTER'S LODGE - Hunting and tracking
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new BuildingProto(
			id: Ids.Buildings.HuntersLodge,
			text: Proto.CreateText("Hunter's Lodge", "Learn the ways of the hunt. Track beasts, gather pelts, and earn food from your kills."),
			iconName: "icon_hunters_lodge",
			category: BuildingCategory.Production,
			size: BuildingSize.Medium,
			width: 1,
			height: 1,
			unlockCost: 20,
			baseUpgradeCost: 8,
			maxLevel: 5,
			startsUnlocked: false,
			bonusesPerLevel: [
				new BuildingBonusProto(Ids.Stats.BuildingUpgrades.HuntingBonus, 10, ModifierOperation.PercentIncrease),
				new BuildingBonusProto(Ids.Stats.BuildingUpgrades.FoodFromCombat, 1)
			],
			hasNPC: true,
			npcId: "npc_hunter"
		));

		// ═══════════════════════════════════════════════════════════════
		// SHRINE - Blessings and divine magic
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new BuildingProto(
			id: Ids.Buildings.Shrine,
			text: Proto.CreateText("Shrine", "A place of worship dedicated to the old gods. Receive blessings before your journey and seek divine guidance."),
			iconName: "icon_shrine",
			category: BuildingCategory.Utility,
			size: BuildingSize.Small,
			width: 1,
			height: 1,
			unlockCost: 20,
			baseUpgradeCost: 8,
			maxLevel: 5,
			startsUnlocked: false,
			bonusesPerLevel: [
				new BuildingBonusProto(Ids.Stats.BuildingUpgrades.BlessingDuration, 10, ModifierOperation.PercentIncrease),
				new BuildingBonusProto(Ids.Stats.Resistances.Holy, 3)
			],
			providesRest: true,
			hasNPC: true,
			npcId: "npc_priest"
		));

		// ═══════════════════════════════════════════════════════════════
		// WORKSHOP - Crafting and tinkering
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new BuildingProto(
			id: Ids.Buildings.Workshop,
			text: Proto.CreateText("Workshop", "Craft tools, gadgets, and useful items. A skilled craftsman can turn raw materials into valuable equipment."),
			iconName: "icon_workshop",
			category: BuildingCategory.Production,
			size: BuildingSize.Medium,
			width: 1,
			height: 1,
			unlockCost: 25,
			baseUpgradeCost: 10,
			maxLevel: 5,
			startsUnlocked: false,
			bonusesPerLevel: [
				new BuildingBonusProto(Ids.Stats.BuildingUpgrades.CraftingQuality, 5, ModifierOperation.PercentIncrease),
				new BuildingBonusProto(Ids.Stats.BuildingUpgrades.SalvageBonus, 10, ModifierOperation.PercentIncrease)
			],
			hasNPC: true,
			npcId: "npc_craftsman"
		));

		// ═══════════════════════════════════════════════════════════════
		// WATCHTOWER - Early warning and vision
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new BuildingProto(
			id: Ids.Buildings.Watchtower,
			text: Proto.CreateText("Watchtower", "Keep watch for threats approaching the village. Provides advance warning of fog movements and enemy patrols."),
			iconName: "icon_watchtower",
			category: BuildingCategory.Defense,
			size: BuildingSize.Small,
			width: 1,
			height: 1,
			unlockCost: 20,
			baseUpgradeCost: 8,
			maxLevel: 5,
			startsUnlocked: false,
			bonusesPerLevel: [
				new BuildingBonusProto(Ids.Stats.Expedition.VisionRange, 1),
				new BuildingBonusProto(Ids.Stats.BuildingUpgrades.AmbushResistance, 5, ModifierOperation.PercentIncrease)
			]
		));

		// ═══════════════════════════════════════════════════════════════
		// FARM - Food production
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new BuildingProto(
			id: Ids.Buildings.Farm,
			text: Proto.CreateText("Farm", "Grow food to sustain the village. A full stomach is a happy stomach. Provides starting food for each expedition."),
			iconName: "icon_farm",
			category: BuildingCategory.Production,
			size: BuildingSize.Large,
			width: 1,
			height: 1,
			unlockCost: 15,
			baseUpgradeCost: 5,
			maxLevel: 5,
			startsUnlocked: false,
			bonusesPerLevel: [
				new BuildingBonusProto(Ids.Stats.BuildingUpgrades.StartingFood, 1),
				new BuildingBonusProto(Ids.Stats.Expedition.FoodConsumption, -5, ModifierOperation.PercentIncrease)
			],
			hasNPC: true,
			npcId: "npc_farmer"
		));

		// ═══════════════════════════════════════════════════════════════
		// WELL - Water and minor healing
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new BuildingProto(
			id: Ids.Buildings.Well,
			text: Proto.CreateText("Well", "Fresh water for the village. Drink from the well to restore a small amount of health for free."),
			iconName: "icon_well",
			category: BuildingCategory.Utility,
			size: BuildingSize.Small,
			width: 1,
			height: 1,
			unlockCost: 10,
			baseUpgradeCost: 5,
			maxLevel: 3,
			startsUnlocked: false,
			bonusesPerLevel: [
				new BuildingBonusProto(Ids.Stats.BuildingUpgrades.FreeHealing, 5)
			]
		));

		// ═══════════════════════════════════════════════════════════════
		// MONUMENT - Achievements and bonuses
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new BuildingProto(
			id: Ids.Buildings.Monument,
			text: Proto.CreateText("Monument", "A stone pillar to commemorate your achievements. Each victory against the fog is recorded here, granting permanent bonuses."),
			iconName: "icon_monument",
			category: BuildingCategory.Utility,
			size: BuildingSize.Small,
			width: 1,
			height: 1,
			unlockCost: 30,
			baseUpgradeCost: 15,
			maxLevel: 5,
			startsUnlocked: false,
			requiredFogClears: 1,
			bonusesPerLevel: [
				new BuildingBonusProto(Ids.Stats.BuildingUpgrades.AchievementBonus, 10, ModifierOperation.PercentIncrease)
			]
		));

		// ═══════════════════════════════════════════════════════════════
		// GRAVEYARD - Honor the fallen
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new BuildingProto(
			id: Ids.Buildings.Graveyard,
			text: Proto.CreateText("Graveyard", "Honor those who fell to the fog. Each death teaches something. Gain bonus resources based on how far you traveled before falling."),
			iconName: "icon_graveyard",
			category: BuildingCategory.Utility,
			size: BuildingSize.Medium,
			width: 1,
			height: 1,
			unlockCost: 25,
			baseUpgradeCost: 10,
			maxLevel: 5,
			startsUnlocked: false,
			bonusesPerLevel: [
				new BuildingBonusProto(Ids.Stats.BuildingUpgrades.DeathBonus, 10, ModifierOperation.PercentIncrease),
				new BuildingBonusProto(Ids.Stats.BuildingUpgrades.LegacyExperience, 5, ModifierOperation.PercentIncrease)
			]
		));
	}

	#endregion

	#region Late Game Buildings

	private void RegisterLateGameBuildings(GameDb db) {
		// ═══════════════════════════════════════════════════════════════
		// ARCANE TOWER - Advanced magic
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new BuildingProto(
			id: Ids.Buildings.ArcaneTower,
			text: Proto.CreateText("Arcane Tower", "A tower devoted to the highest arcane arts. Learn rare spells, enchant equipment, and peer into the mysteries of time."),
			iconName: "icon_arcane_tower",
			category: BuildingCategory.Training,
			size: BuildingSize.Medium,
			width: 1,
			height: 1,
			unlockCost: 75,
			baseUpgradeCost: 25,
			maxLevel: 5,
			prerequisites: [Ids.Buildings.Library],
			requiredFogClears: 2,
			startsUnlocked: false,
			bonusesPerLevel: [
				new BuildingBonusProto(Ids.Stats.Combat.SpellPower, 3),
				new BuildingBonusProto(Ids.Stats.Regeneration.Mana, 1),
				new BuildingBonusProto(Ids.Stats.Resistances.Magical, 5)
			],
			hasNPC: true,
			npcId: "npc_archmage"
		));

		// ═══════════════════════════════════════════════════════════════
		// ARENA - Combat challenges
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new BuildingProto(
			id: Ids.Buildings.Arena,
			text: Proto.CreateText("Arena", "Test your combat prowess against waves of foes. Earn glory, rewards, and unlock special combat techniques."),
			iconName: "icon_arena",
			category: BuildingCategory.Training,
			size: BuildingSize.Large,
			width: 1,
			height: 1,
			unlockCost: 60,
			baseUpgradeCost: 20,
			maxLevel: 5,
			prerequisites: [Ids.Buildings.TrainingGrounds],
			requiredFogClears: 1,
			startsUnlocked: false,
			bonusesPerLevel: [
				new BuildingBonusProto(Ids.Stats.BuildingUpgrades.CombatGold, 10, ModifierOperation.PercentIncrease),
				new BuildingBonusProto(Ids.Stats.Combat.CriticalDamage, 5, ModifierOperation.PercentIncrease)
			],
			hasNPC: true,
			npcId: "npc_arenamaster"
		));

		// ═══════════════════════════════════════════════════════════════
		// OBSERVATORY - Star reading and prophecies
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new BuildingProto(
			id: Ids.Buildings.Observatory,
			text: Proto.CreateText("Observatory", "Study the stars and divine the future. Gain hints about upcoming events and the true nature of the fog."),
			iconName: "icon_observatory",
			category: BuildingCategory.Utility,
			size: BuildingSize.Medium,
			width: 1,
			height: 1,
			unlockCost: 50,
			baseUpgradeCost: 18,
			maxLevel: 5,
			prerequisites: [Ids.Buildings.Cartographer],
			requiredFogClears: 1,
			startsUnlocked: false,
			bonusesPerLevel: [
				new BuildingBonusProto(Ids.Stats.BuildingUpgrades.EventPreview, 1),
				new BuildingBonusProto(Ids.Stats.BuildingUpgrades.LoreDiscovery, 10, ModifierOperation.PercentIncrease)
			],
			hasNPC: true,
			npcId: "npc_astronomer"
		));

		// ═══════════════════════════════════════════════════════════════
		// PORTAL CHAMBER - Fast travel
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new BuildingProto(
			id: Ids.Buildings.PortalChamber,
			text: Proto.CreateText("Portal Chamber", "Harness the same forces that power the Anchor to create shortcuts through the fog. Dangerous, but invaluable."),
			iconName: "icon_portal_chamber",
			category: BuildingCategory.Utility,
			size: BuildingSize.Medium,
			width: 1,
			height: 1,
			unlockCost: 100,
			baseUpgradeCost: 30,
			maxLevel: 3,
			requiredFogClears: 3,
			startsUnlocked: false,
			bonusesPerLevel: [
				new BuildingBonusProto(Ids.Stats.BuildingUpgrades.PortalCharges, 1),
				new BuildingBonusProto(Ids.Stats.BuildingUpgrades.PortalRange, 10)
			],
			hasNPC: true,
			npcId: "npc_portalkeeper"
		));
	}

	#endregion

	#region Special Buildings

	private void RegisterSpecialBuildings(GameDb db) {
		// ═══════════════════════════════════════════════════════════════
		// HOME - Personal space
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new BuildingProto(
			id: Ids.Buildings.Home,
			text: Proto.CreateText("Your Home", "A place to call your own. Store personal items, display trophies, and rest in comfort. Persists across time loops."),
			iconName: "icon_home",
			category: BuildingCategory.Utility,
			size: BuildingSize.Medium,
			width: 1,
			height: 1,
			unlockCost: 50,
			baseUpgradeCost: 15,
			maxLevel: 5,
			requiredFogClears: 1,
			startsUnlocked: false,
			bonusesPerLevel: [
				new BuildingBonusProto(Ids.Stats.BuildingUpgrades.PersonalStorage, 10),
				new BuildingBonusProto(Ids.Stats.BuildingUpgrades.RestEffectiveness, 10, ModifierOperation.PercentIncrease)
			],
			providesRest: true
		));

		// ═══════════════════════════════════════════════════════════════
		// GUILD HALL - Party management
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new BuildingProto(
			id: Ids.Buildings.GuildHall,
			text: Proto.CreateText("Guild Hall", "A gathering place for adventurers. Recruit companions, manage your party, and take on guild contracts for special rewards."),
			iconName: "icon_guild_hall",
			category: BuildingCategory.Utility,
			size: BuildingSize.Large,
			width: 1,
			height: 1,
			unlockCost: 75,
			baseUpgradeCost: 20,
			maxLevel: 5,
			requiredFogClears: 2,
			startsUnlocked: false,
			bonusesPerLevel: [
				new BuildingBonusProto(Ids.Stats.BuildingUpgrades.PartySize, 1),
				new BuildingBonusProto(Ids.Stats.BuildingUpgrades.CompanionExperience, 10, ModifierOperation.PercentIncrease)
			],
			hasNPC: true,
			npcId: "npc_guildmaster"
		));
	}

	#endregion
}
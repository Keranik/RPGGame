using RPGGame.Core.Expedition;
using RPGGame.Core.Prototypes;
using RPGGame.Core.Prototypes.Item;
using RPGGame.Core.Prototypes.Item.Resource;
using RPGGame.Core.Stats;

namespace RPGGame.Core.Resources;

/// <summary>
/// Defines all gatherable resource nodes in the world.
/// </summary>
public class ResourceDefinitions : ICoreData {
	public void GameData(GameDb gameDatabase) {
		RegisterMiningNodes(gameDatabase);
		RegisterHerbalismNodes(gameDatabase);
		RegisterWoodcuttingNodes(gameDatabase);
		RegisterFishingSpots(gameDatabase);
		RegisterHuntingGrounds(gameDatabase);
		RegisterForagingSpots(gameDatabase);
		RegisterSpecialResources(gameDatabase);
		RegisterEnvironmentalResources(gameDatabase);
	}

	#region Mining Nodes

	private void RegisterMiningNodes(GameDb db) {
		// ═══════════════════════════════════════════════════════════════
		// COMMON ORES
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Mining.IronVein,
			text: Proto.CreateText("Iron Vein", "A rocky outcrop rich with iron ore. The dull metallic gleam is unmistakable."),
			iconName: "icon_node_iron",
			spriteName: "sprite_iron_vein",
			category: ResourceCategory.Mining,
			gatheringSkill: Ids.Skills.Gathering.Mining,
			gatheringDC: 10,
			gatherTime: 8f,
			requiredTool: ToolType.Pickaxe,
			primaryYield: Ids.Items.Materials.IronOre,
			minYield: 1,
			maxYield: 3,
			totalGathers: 5,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.Stone, 0.3f, 1, 2),
				new ResourceDrop(Ids.Items.Materials.GemRuby, 0.02f, 1, 1)
			],
			validTerrains: [Ids.Terrains.Mountains.Mountain, Ids.Terrains.Plains.Hills, Ids.Terrains.Underground.Cave],
			spawnWeight: 15f,
			respawnTime: 48f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Mining.CopperDeposit,
			text: Proto.CreateText("Copper Deposit", "Green-tinged rock containing copper ore."),
			iconName: "icon_node_copper",
			spriteName: "sprite_copper_deposit",
			category: ResourceCategory.Mining,
			gatheringSkill: Ids.Skills.Gathering.Mining,
			gatheringDC: 8,
			gatherTime: 6f,
			requiredTool: ToolType.Pickaxe,
			primaryYield: Ids.Items.Materials.CopperOre,
			minYield: 1,
			maxYield: 3,
			totalGathers: 6,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.Stone, 0.4f, 1, 2)
			],
			validTerrains: [Ids.Terrains.Mountains.Mountain, Ids.Terrains.Plains.Hills, Ids.Terrains.Underground.Cave],
			spawnWeight: 20f,
			respawnTime: 36f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Mining.SilverVein,
			text: Proto.CreateText("Silver Vein", "A shimmering vein of precious silver ore."),
			iconName: "icon_node_silver",
			spriteName: "sprite_silver_vein",
			category: ResourceCategory.Mining,
			gatheringSkill: Ids.Skills.Gathering.Mining,
			gatheringDC: 14,
			gatherTime: 10f,
			requiredTool: ToolType.Pickaxe,
			primaryYield: Ids.Items.Materials.SilverOre,
			minYield: 1,
			maxYield: 2,
			totalGathers: 3,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.GemMoonstone, 0.05f, 1, 1)
			],
			validTerrains: [Ids.Terrains.Mountains.Mountain, Ids.Terrains.Underground.Cave],
			spawnWeight: 5f,
			minSpawnDistance: 10f,
			respawnTime: 72f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Mining.GoldVein,
			text: Proto.CreateText("Gold Vein", "A rare vein of gold glittering in the rock."),
			iconName: "icon_node_gold",
			spriteName: "sprite_gold_vein",
			category: ResourceCategory.Mining,
			gatheringSkill: Ids.Skills.Gathering.Mining,
			gatheringDC: 16,
			gatherTime: 12f,
			requiredTool: ToolType.Pickaxe,
			primaryYield: Ids.Items.Materials.GoldOre,
			minYield: 1,
			maxYield: 2,
			totalGathers: 2,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.GemRuby, 0.08f, 1, 1),
				new ResourceDrop(Ids.Items.Materials.GemEmerald, 0.05f, 1, 1)
			],
			validTerrains: [Ids.Terrains.Mountains.Mountain, Ids.Terrains.Underground.Cave],
			spawnWeight: 2f,
			minSpawnDistance: 20f,
			respawnTime: 120f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Mining.MithrilVein,
			text: Proto.CreateText("Mithril Vein", "An extraordinarily rare vein of legendary mithril."),
			iconName: "icon_node_mithril",
			spriteName: "sprite_mithril_vein",
			category: ResourceCategory.Mining,
			gatheringSkill: Ids.Skills.Gathering.Mining,
			gatheringDC: 20,
			gatherTime: 15f,
			requiredTool: ToolType.Pickaxe,
			primaryYield: Ids.Items.Materials.MithrilOre,
			minYield: 1,
			maxYield: 1,
			totalGathers: 2,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.GemDiamond, 0.1f, 1, 1)
			],
			onGatherEvent: Ids.Events.Random.MithrilGuardian,
			gatherEventChance: 0.15f,
			validTerrains: [Ids.Terrains.Mountains.Mountain, Ids.Terrains.Underground.Cave],
			spawnWeight: 0.5f,
			minSpawnDistance: 30f,
			respawnTime: -1f // Never respawns
		));

		// ═══════════════════════════════════════════════════════════════
		// STONE & MINERALS
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Mining.StoneOutcrop,
			text: Proto.CreateText("Stone Outcrop", "A solid formation of workable stone."),
			iconName: "icon_node_stone",
			spriteName: "sprite_stone_outcrop",
			category: ResourceCategory.Mining,
			gatheringSkill: Ids.Skills.Gathering.Mining,
			gatheringDC: 6,
			gatherTime: 5f,
			requiredTool: ToolType.Pickaxe,
			primaryYield: Ids.Items.Materials.Stone,
			minYield: 2,
			maxYield: 4,
			totalGathers: 8,
			validTerrains: [Ids.Terrains.Mountains.Mountain, Ids.Terrains.Plains.Hills, Ids.Terrains.Ruins.OpenRuins],
			spawnWeight: 25f,
			respawnTime: 24f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Mining.CrystalFormation,
			text: Proto.CreateText("Crystal Formation", "A cluster of naturally formed crystals humming with faint energy."),
			iconName: "icon_node_crystal",
			spriteName: "sprite_crystal_formation",
			category: ResourceCategory.Mining,
			gatheringSkill: Ids.Skills.Gathering.Mining,
			gatheringDC: 14,
			gatherTime: 10f,
			requiredTool: ToolType.Pickaxe,
			primaryYield: Ids.Items.Materials.MagicCrystal,
			minYield: 1,
			maxYield: 2,
			totalGathers: 3,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.CrystalShard, 0.5f, 1, 3)
			],
			validTerrains: [Ids.Terrains.Underground.Cave, Ids.Terrains.Ruins.OpenRuins],
			spawnWeight: 3f,
			minSpawnDistance: 15f,
			respawnTime: 96f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Mining.SandPocket,
			text: Proto.CreateText("Sand Pocket", "A deposit of fine sand useful for glassmaking."),
			iconName: "icon_node_sand",
			spriteName: "sprite_sand_pocket",
			category: ResourceCategory.Mining,
			gatheringSkill: Ids.Skills.Survival.General,
			gatheringDC: 5,
			gatherTime: 3f,
			requiredTool: ToolType.Shovel,
			primaryYield: Ids.Items.Materials.Sand,
			minYield: 3,
			maxYield: 6,
			totalGathers: 10,
			validTerrains: [Ids.Terrains.Desert.OpenDesert, Ids.Terrains.Coastal.Beach, Ids.Terrains.Water.River],
			spawnWeight: 20f,
			respawnTime: 12f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Mining.ClayDeposit,
			text: Proto.CreateText("Clay Deposit", "Moist clay suitable for pottery and construction."),
			iconName: "icon_node_clay",
			spriteName: "sprite_clay_deposit",
			category: ResourceCategory.Mining,
			gatheringSkill: Ids.Skills.Survival.General,
			gatheringDC: 6,
			gatherTime: 4f,
			requiredTool: ToolType.Shovel,
			primaryYield: Ids.Items.Materials.Clay,
			minYield: 2,
			maxYield: 5,
			totalGathers: 8,
			validTerrains: [Ids.Terrains.Water.River, Ids.Terrains.Wetlands.Swamp, Ids.Terrains.Water.Lake],
			spawnWeight: 15f,
			respawnTime: 24f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Mining.SulfurDeposit,
			text: Proto.CreateText("Sulfur Deposit", "Yellow crystalline sulfur with a pungent smell."),
			iconName: "icon_node_sulfur",
			spriteName: "sprite_sulfur_deposit",
			category: ResourceCategory.Mining,
			gatheringSkill: Ids.Skills.Gathering.Mining,
			gatheringDC: 12,
			gatherTime: 7f,
			requiredTool: ToolType.Pickaxe,
			primaryYield: Ids.Items.Materials.Sulfite,
			minYield: 1,
			maxYield: 3,
			totalGathers: 4,
			validTerrains: [Ids.Terrains.Volcanic.VolcanicPlain, Ids.Terrains.Underground.Cave],
			spawnWeight: 8f,
			minSpawnDistance: 15f,
			respawnTime: 48f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Mining.CoalSeam,
			text: Proto.CreateText("Coal Seam", "A dark seam of combustible coal."),
			iconName: "icon_node_coal",
			spriteName: "sprite_coal_seam",
			category: ResourceCategory.Mining,
			gatheringSkill: Ids.Skills.Gathering.Mining,
			gatheringDC: 8,
			gatherTime: 6f,
			requiredTool: ToolType.Pickaxe,
			primaryYield: Ids.Items.Materials.Coal,
			minYield: 2,
			maxYield: 4,
			totalGathers: 6,
			validTerrains: [Ids.Terrains.Mountains.Mountain, Ids.Terrains.Underground.Cave, Ids.Terrains.Plains.Hills],
			spawnWeight: 18f,
			respawnTime: 36f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Mining.ObsidianFormation,
			text: Proto.CreateText("Obsidian Formation", "Volcanic glass, sharp as any blade."),
			iconName: "icon_node_obsidian",
			spriteName: "sprite_obsidian_formation",
			category: ResourceCategory.Mining,
			gatheringSkill: Ids.Skills.Gathering.Mining,
			gatheringDC: 15,
			gatherTime: 12f,
			requiredTool: ToolType.Pickaxe,
			primaryYield: Ids.Items.Materials.Obsidian,
			minYield: 1,
			maxYield: 2,
			totalGathers: 3,
			validTerrains: [Ids.Terrains.Volcanic.VolcanicPlain, Ids.Terrains.Mountains.Mountain],
			spawnWeight: 4f,
			minSpawnDistance: 20f,
			respawnTime: 72f
		));
	}

	#endregion

	#region Herbalism Nodes

	private void RegisterHerbalismNodes(GameDb db) {
		// ═══════════════════════════════════════════════════════════════
		// COMMON HERBS
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Herbalism.HerbPatch,
			text: Proto.CreateText("Herb Patch", "A cluster of common medicinal herbs."),
			iconName: "icon_node_herbs",
			spriteName: "sprite_herb_patch",
			category: ResourceCategory.Herbalism,
			gatheringSkill: Ids.Skills.Gathering.Foraging,
			gatheringDC: 8,
			gatherTime: 4f,
			primaryYield: Ids.Items.Materials.Herbs,
			minYield: 1,
			maxYield: 3,
			totalGathers: 4,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.RareHerbs, 0.1f, 1, 1)
			],
			validTerrains: [Ids.Terrains.Forests.Forest, Ids.Terrains.Plains.Grass, Ids.Terrains.Plains.Hills],
			spawnWeight: 20f,
			respawnTime: 24f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Herbalism.WildflowerMeadow,
			text: Proto.CreateText("Wildflower Meadow", "A colorful meadow of wild flowers with medicinal properties."),
			iconName: "icon_node_wildflowers",
			spriteName: "sprite_wildflower_meadow",
			category: ResourceCategory.Herbalism,
			gatheringSkill: Ids.Skills.Gathering.Foraging,
			gatheringDC: 6,
			gatherTime: 3f,
			primaryYield: Ids.Items.Materials.Wildflowers,
			minYield: 2,
			maxYield: 5,
			totalGathers: 6,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.Herbs, 0.3f, 1, 2),
				new ResourceDrop(Ids.Items.Materials.Honey, 0.1f, 1, 1)
			],
			validTerrains: [Ids.Terrains.Plains.Grass, Ids.Terrains.Plains.Hills],
			spawnWeight: 18f,
			respawnTime: 18f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Herbalism.MoonbloomPatch,
			text: Proto.CreateText("Moonbloom Patch", "Pale flowers that only bloom under moonlight. Prized by alchemists."),
			iconName: "icon_node_moonbloom",
			spriteName: "sprite_moonbloom_patch",
			category: ResourceCategory.Herbalism,
			gatheringSkill: Ids.Skills.Gathering.Foraging,
			gatheringDC: 14,
			gatherTime: 6f,
			primaryYield: Ids.Items.Materials.Moonbloom,
			minYield: 1,
			maxYield: 2,
			totalGathers: 2,
			validTerrains: [Ids.Terrains.Forests.Forest, Ids.Terrains.Ruins.OpenRuins],
			spawnWeight: 3f,
			minSpawnDistance: 15f,
			respawnTime: 72f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Herbalism.Nightshade,
			text: Proto.CreateText("Nightshade", "Deadly poisonous berries. Handle with care."),
			iconName: "icon_node_nightshade",
			spriteName: "sprite_nightshade",
			category: ResourceCategory.Herbalism,
			gatheringSkill: Ids.Skills.Gathering.Foraging,
			gatheringDC: 12,
			gatherTime: 5f,
			primaryYield: Ids.Items.Materials.Nightshade,
			minYield: 1,
			maxYield: 2,
			totalGathers: 3,
			onGatherEvent: Ids.Events.Random.PoisonPrick,
			gatherEventChance: 0.15f,
			validTerrains: [Ids.Terrains.Forests.Forest, Ids.Terrains.Wetlands.Swamp],
			spawnWeight: 6f,
			minSpawnDistance: 10f,
			respawnTime: 48f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Herbalism.GlowMoss,
			text: Proto.CreateText("Glow Moss", "Luminescent moss found in dark places."),
			iconName: "icon_node_glowmoss",
			spriteName: "sprite_glow_moss",
			category: ResourceCategory.Herbalism,
			gatheringSkill: Ids.Skills.Gathering.Foraging,
			gatheringDC: 10,
			gatherTime: 4f,
			primaryYield: Ids.Items.Materials.GlowMoss,
			minYield: 1,
			maxYield: 3,
			totalGathers: 4,
			validTerrains: [Ids.Terrains.Underground.Cave, Ids.Terrains.Ruins.OpenRuins],
			spawnWeight: 10f,
			respawnTime: 36f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Herbalism.MandrakeRoot,
			text: Proto.CreateText("Mandrake Root", "A magical root said to scream when pulled from the earth."),
			iconName: "icon_node_mandrake",
			spriteName: "sprite_mandrake",
			category: ResourceCategory.Herbalism,
			gatheringSkill: Ids.Skills.Gathering.Foraging,
			gatheringDC: 16,
			gatherTime: 8f,
			primaryYield: Ids.Items.Materials.MandrakeRoot,
			minYield: 1,
			maxYield: 1,
			totalGathers: 1,
			onGatherEvent: Ids.Events.Random.MandrakeScream,
			gatherEventChance: 0.5f,
			validTerrains: [Ids.Terrains.Forests.Forest, Ids.Terrains.Wetlands.Swamp, Ids.Terrains.Ruins.Graveyard],
			spawnWeight: 2f,
			minSpawnDistance: 20f,
			respawnTime: 168f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Herbalism.SwampMoss,
			text: Proto.CreateText("Swamp Moss", "Thick, slimy moss from the swamps."),
			iconName: "icon_node_swampmoss",
			spriteName: "sprite_swamp_moss",
			category: ResourceCategory.Herbalism,
			gatheringSkill: Ids.Skills.Gathering.Foraging,
			gatheringDC: 8,
			gatherTime: 3f,
			primaryYield: Ids.Items.Materials.SwampMoss,
			minYield: 2,
			maxYield: 4,
			totalGathers: 5,
			validTerrains: [Ids.Terrains.Wetlands.Swamp],
			spawnWeight: 15f,
			respawnTime: 18f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Herbalism.FrostLichen,
			text: Proto.CreateText("Frost Lichen", "A hardy lichen that thrives in freezing conditions."),
			iconName: "icon_node_frostlichen",
			spriteName: "sprite_frost_lichen",
			category: ResourceCategory.Herbalism,
			gatheringSkill: Ids.Skills.Gathering.Foraging,
			gatheringDC: 12,
			gatherTime: 5f,
			primaryYield: Ids.Items.Materials.FrostLichen,
			minYield: 1,
			maxYield: 2,
			totalGathers: 3,
			validTerrains: [Ids.Terrains.Frozen.Snow, Ids.Terrains.Mountains.Mountain],
			spawnWeight: 8f,
			respawnTime: 48f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Herbalism.FireBloom,
			text: Proto.CreateText("Fire Bloom", "A rare flower that grows near volcanic heat."),
			iconName: "icon_node_firebloom",
			spriteName: "sprite_fire_bloom",
			category: ResourceCategory.Herbalism,
			gatheringSkill: Ids.Skills.Gathering.Foraging,
			gatheringDC: 15,
			gatherTime: 6f,
			primaryYield: Ids.Items.Materials.FireBloom,
			minYield: 1,
			maxYield: 2,
			totalGathers: 2,
			validTerrains: [Ids.Terrains.Volcanic.VolcanicPlain],
			spawnWeight: 4f,
			minSpawnDistance: 20f,
			respawnTime: 72f
		));
	}

	#endregion

	#region Woodcutting Nodes

	private void RegisterWoodcuttingNodes(GameDb db) {
		// ═══════════════════════════════════════════════════════════════
		// COMMON TREES
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Woodcutting.OakTree,
			text: Proto.CreateText("Oak Tree", "A sturdy oak tree with strong, useful wood."),
			iconName: "icon_node_oak",
			spriteName: "sprite_oak_tree",
			category: ResourceCategory.Woodcutting,
			gatheringSkill: Ids.Skills.Gathering.Woodcutting,
			gatheringDC: 8,
			gatherTime: 10f,
			requiredTool: ToolType.Axe,
			primaryYield: Ids.Items.Materials.OakWood,
			minYield: 2,
			maxYield: 4,
			totalGathers: 3,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.Bark, 0.4f, 1, 2),
				new ResourceDrop(Ids.Items.Materials.Acorn, 0.2f, 1, 3)
			],
			validTerrains: [Ids.Terrains.Forests.Forest, Ids.Terrains.Plains.Grass],
			spawnWeight: 20f,
			respawnTime: 168f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Woodcutting.PineTree,
			text: Proto.CreateText("Pine Tree", "A tall pine with fragrant wood and useful resin."),
			iconName: "icon_node_pine",
			spriteName: "sprite_pine_tree",
			category: ResourceCategory.Woodcutting,
			Ids.Skills.Gathering.Woodcutting,
			gatheringDC: 8,
			gatherTime: 9f,
			requiredTool: ToolType.Axe,
			primaryYield: Ids.Items.Materials.PineWood,
			minYield: 2,
			maxYield: 4,
			totalGathers: 3,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.PineResin, 0.3f, 1, 2),
				new ResourceDrop(Ids.Items.Materials.Pinecone, 0.25f, 1, 2)
			],
			validTerrains: [Ids.Terrains.Forests.Forest, Ids.Terrains.Mountains.Mountain],
			spawnWeight: 18f,
			respawnTime: 144f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Woodcutting.BirchTree,
			text: Proto.CreateText("Birch Tree", "A slender birch with pale, flexible wood."),
			iconName: "icon_node_birch",
			spriteName: "sprite_birch_tree",
			category: ResourceCategory.Woodcutting,
			Ids.Skills.Gathering.Woodcutting,
			gatheringDC: 6,
			gatherTime: 7f,
			requiredTool: ToolType.Axe,
			primaryYield: Ids.Items.Materials.BirchWood,
			minYield: 2,
			maxYield: 3,
			totalGathers: 2,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.BirchBark, 0.5f, 1, 2)
			],
			validTerrains: [Ids.Terrains.Forests.Forest, Ids.Terrains.Frozen.Snow],
			spawnWeight: 15f,
			respawnTime: 120f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Woodcutting.WillowTree,
			text: Proto.CreateText("Willow Tree", "A graceful willow with flexible branches."),
			iconName: "icon_node_willow",
			spriteName: "sprite_willow_tree",
			category: ResourceCategory.Woodcutting,
			Ids.Skills.Gathering.Woodcutting,
			gatheringDC: 7,
			gatherTime: 8f,
			requiredTool: ToolType.Axe,
			primaryYield: Ids.Items.Materials.WillowWood,
			minYield: 2,
			maxYield: 4,
			totalGathers: 3,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.WillowBark, 0.4f, 1, 2)
			],
			validTerrains: [Ids.Terrains.Water.River, Ids.Terrains.Wetlands.Swamp, Ids.Terrains.Water.Lake],
			spawnWeight: 12f,
			respawnTime: 144f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Woodcutting.DeadTree,
			text: Proto.CreateText("Dead Tree", "A lifeless husk, but still useful for firewood."),
			iconName: "icon_node_deadtree",
			spriteName: "sprite_dead_tree",
			category: ResourceCategory.Woodcutting,
			Ids.Skills.Gathering.Woodcutting,
			gatheringDC: 5,
			gatherTime: 5f,
			requiredTool: ToolType.Axe,
			primaryYield: Ids.Items.Materials.Firewood,
			minYield: 2,
			maxYield: 5,
			totalGathers: 2,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.DriedBark, 0.3f, 1, 2)
			],
			validTerrains: [Ids.Terrains.Forests.Forest, Ids.Terrains.Wetlands.Swamp, Ids.Terrains.Supernatural.Wasteland],
			spawnWeight: 10f,
			respawnTime: -1f
		));

		// ═══════════════════════════════════════════════════════════════
		// SPECIAL TREES
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Woodcutting.IronwoodTree,
			text: Proto.CreateText("Ironwood Tree", "An extremely hard wood, almost as strong as metal."),
			iconName: "icon_node_ironwood",
			spriteName: "sprite_ironwood_tree",
			category: ResourceCategory.Woodcutting,
			Ids.Skills.Gathering.Woodcutting,
			gatheringDC: 16,
			gatherTime: 15f,
			requiredTool: ToolType.Axe,
			primaryYield: Ids.Items.Materials.IronwoodLog,
			minYield: 1,
			maxYield: 2,
			totalGathers: 2,
			validTerrains: [Ids.Terrains.Forests.Forest],
			spawnWeight: 3f,
			minSpawnDistance: 15f,
			respawnTime: 336f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Woodcutting.ElderTree,
			text: Proto.CreateText("Elder Tree", "An ancient tree with magical properties."),
			iconName: "icon_node_elder",
			spriteName: "sprite_elder_tree",
			category: ResourceCategory.Woodcutting,
			Ids.Skills.Gathering.Woodcutting,
			gatheringDC: 14,
			gatherTime: 12f,
			requiredTool: ToolType.Axe,
			primaryYield: Ids.Items.Materials.ElderWood,
			minYield: 1,
			maxYield: 2,
			totalGathers: 2,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.Elderberries, 0.4f, 2, 4),
				new ResourceDrop(Ids.Items.Materials.ElderFlowers, 0.3f, 1, 2)
			],
			onDiscoverEvent: Ids.Events.Random.ElderTreeSpirit,
			validTerrains: [Ids.Terrains.Forests.Forest, Ids.Terrains.Ruins.OpenRuins],
			spawnWeight: 2f,
			minSpawnDistance: 20f,
			respawnTime: 504f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Woodcutting.CorruptedTree,
			text: Proto.CreateText("Corrupted Tree", "A twisted tree tainted by dark magic."),
			iconName: "icon_node_corrupted",
			spriteName: "sprite_corrupted_tree",
			category: ResourceCategory.Woodcutting,
			Ids.Skills.Gathering.Woodcutting,
			gatheringDC: 12,
			gatherTime: 10f,
			requiredTool: ToolType.Axe,
			primaryYield: Ids.Items.Materials.CorruptedWood,
			minYield: 1,
			maxYield: 3,
			totalGathers: 3,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.ShadowEssence, 0.2f, 1, 1)
			],
			onGatherEvent: Ids.Events.Random.CorruptionSpread,
			gatherEventChance: 0.2f,
			validTerrains: [Ids.Terrains.Supernatural.Corrupted, Ids.Terrains.Wetlands.Swamp],
			spawnWeight: 5f,
			minSpawnDistance: 15f,
			respawnTime: 72f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Woodcutting.FrozenPine,
			text: Proto.CreateText("Frozen Pine", "A pine tree encased in eternal ice."),
			iconName: "icon_node_frozenpine",
			spriteName: "sprite_frozen_pine",
			category: ResourceCategory.Woodcutting,
			Ids.Skills.Gathering.Woodcutting,
			gatheringDC: 13,
			gatherTime: 12f,
			requiredTool: ToolType.Axe,
			primaryYield: Ids.Items.Materials.FrozenWood,
			minYield: 1,
			maxYield: 3,
			totalGathers: 2,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.EternalIce, 0.2f, 1, 1)
			],
			validTerrains: [Ids.Terrains.Frozen.Snow, Ids.Terrains.Mountains.Mountain],
			spawnWeight: 6f,
			minSpawnDistance: 20f,
			respawnTime: 168f
		));
	}

	#endregion

	#region Fishing Spots

	private void RegisterFishingSpots(GameDb db) {
		// ═══════════════════════════════════════════════════════════════
		// FRESHWATER
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Fishing.RiverFishingSpot,
			text: Proto.CreateText("River Fishing Spot", "A calm section of river teeming with fish."),
			iconName: "icon_node_river_fish",
			spriteName: "sprite_fishing_spot_river",
			category: ResourceCategory.Fishing,
			gatheringSkill: Ids.Skills.Gathering.Fishing,
			gatheringDC: 10,
			gatherTime: 15f,
			requiredTool: ToolType.FishingRod,
			primaryYield: Ids.Items.Materials.FreshFish,
			minYield: 1,
			maxYield: 2,
			totalGathers: 8,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.RareFish, 0.1f, 1, 1),
				new ResourceDrop(Ids.Items.Materials.OldBoot, 0.05f, 1, 1)
			],
			validTerrains: [Ids.Terrains.Water.River],
			spawnWeight: 15f,
			respawnTime: 12f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Fishing.PondFishingSpot,
			text: Proto.CreateText("Pond", "A still pond with various fish."),
			iconName: "icon_node_pond",
			spriteName: "sprite_fishing_spot_pond",
			category: ResourceCategory.Fishing,
			gatheringSkill: Ids.Skills.Gathering.Fishing,
			gatheringDC: 8,
			gatherTime: 12f,
			requiredTool: ToolType.FishingRod,
			primaryYield: Ids.Items.Materials.FreshFish,
			minYield: 1,
			maxYield: 2,
			totalGathers: 5,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.Tadpole, 0.2f, 1, 3),
				new ResourceDrop(Ids.Items.Materials.FrogLegs, 0.15f, 1, 2)
			],
			validTerrains: [Ids.Terrains.Water.Lake, Ids.Terrains.Plains.Grass],
			spawnWeight: 12f,
			respawnTime: 18f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Fishing.LakeFishingSpot,
			text: Proto.CreateText("Deep Lake", "Deep waters hiding large fish."),
			iconName: "icon_node_lake_fish",
			spriteName: "sprite_fishing_spot_lake",
			category: ResourceCategory.Fishing,
			gatheringSkill: Ids.Skills.Gathering.Fishing,
			gatheringDC: 12,
			gatherTime: 20f,
			requiredTool: ToolType.FishingRod,
			primaryYield: Ids.Items.Materials.LakeFish,
			minYield: 1,
			maxYield: 2,
			totalGathers: 6,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.GiantFish, 0.08f, 1, 1),
				new ResourceDrop(Ids.Items.Materials.Pearl, 0.03f, 1, 1)
			],
			validTerrains: [Ids.Terrains.Water.Lake],
			spawnWeight: 8f,
			respawnTime: 24f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Fishing.SwampFishingSpot,
			text: Proto.CreateText("Murky Waters", "Dark, murky waters with strange creatures."),
			iconName: "icon_node_swamp_fish",
			spriteName: "sprite_fishing_spot_swamp",
			category: ResourceCategory.Fishing,
			gatheringSkill: Ids.Skills.Gathering.Fishing,
			gatheringDC: 11,
			gatherTime: 15f,
			requiredTool: ToolType.FishingRod,
			primaryYield: Ids.Items.Materials.SwampFish,
			minYield: 1,
			maxYield: 2,
			totalGathers: 5,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.Leech, 0.25f, 1, 3),
				new ResourceDrop(Ids.Items.Materials.SwampEel, 0.15f, 1, 1)
			],
			onGatherEvent: Ids.Events.Random.SwampThing,
			gatherEventChance: 0.1f,
			validTerrains: [Ids.Terrains.Wetlands.Swamp],
			spawnWeight: 10f,
			respawnTime: 18f
		));

		// ═══════════════════════════════════════════════════════════════
		// SPECIAL FISHING
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Fishing.MagicSpring,
			text: Proto.CreateText("Magic Spring", "A spring infused with arcane energy. Strange fish swim within."),
			iconName: "icon_node_magic_spring",
			spriteName: "sprite_fishing_magic_spring",
			category: ResourceCategory.Fishing,
			gatheringSkill: Ids.Skills.Gathering.Fishing,
			gatheringDC: 16,
			gatherTime: 25f,
			requiredTool: ToolType.FishingRod,
			primaryYield: Ids.Items.Materials.MagicFish,
			minYield: 1,
			maxYield: 1,
			totalGathers: 3,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.ManaEssence, 0.2f, 1, 1)
			],
			validTerrains: [Ids.Terrains.Ruins.OpenRuins, Ids.Terrains.Forests.Forest],
			spawnWeight: 2f,
			minSpawnDistance: 20f,
			respawnTime: 72f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Fishing.IceFishingHole,
			text: Proto.CreateText("Ice Fishing Hole", "A hole cut in the ice revealing frigid waters below."),
			iconName: "icon_node_ice_fishing",
			spriteName: "sprite_ice_fishing_hole",
			category: ResourceCategory.Fishing,
			gatheringSkill: Ids.Skills.Gathering.Fishing,
			gatheringDC: 14,
			gatherTime: 20f,
			requiredTool: ToolType.FishingRod,
			primaryYield: Ids.Items.Materials.IceFish,
			minYield: 1,
			maxYield: 2,
			totalGathers: 4,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.FrozenFish, 0.15f, 1, 1)
			],
			validTerrains: [Ids.Terrains.Frozen.Snow, Ids.Terrains.Water.Lake],
			spawnWeight: 6f,
			respawnTime: 24f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Fishing.LavaFishingSpot,
			text: Proto.CreateText("Lava Pool", "Somehow, creatures live in this molten rock."),
			iconName: "icon_node_lava_fish",
			spriteName: "sprite_lava_pool",
			category: ResourceCategory.Fishing,
			gatheringSkill: Ids.Skills.Gathering.Fishing,
			gatheringDC: 18,
			gatherTime: 25f,
			requiredTool: ToolType.FishingRod,
			primaryYield: Ids.Items.Materials.MagmaFish,
			minYield: 1,
			maxYield: 1,
			totalGathers: 2,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.FireScale, 0.3f, 1, 2)
			],
			validTerrains: [Ids.Terrains.Volcanic.VolcanicPlain],
			spawnWeight: 2f,
			minSpawnDistance: 25f,
			respawnTime: 96f
		));
	}

	#endregion

	#region Hunting Grounds

	private void RegisterHuntingGrounds(GameDb db) {
		// ═══════════════════════════════════════════════════════════════
		// COMMON GAME
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Hunting.RabbitWarren,
			text: Proto.CreateText("Rabbit Warren", "A network of rabbit burrows. Easy prey for a patient hunter."),
			iconName: "icon_node_rabbit",
			spriteName: "sprite_rabbit_warren",
			category: ResourceCategory.Hunting,
			gatheringSkill: Ids.Skills.Gathering.Hunting,
			gatheringDC: 8,
			gatherTime: 10f,
			primaryYield: Ids.Items.Materials.RabbitMeat,
			minYield: 1,
			maxYield: 2,
			totalGathers: 4,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.RabbitPelt, 0.6f, 1, 1),
				new ResourceDrop(Ids.Items.Materials.RabbitFoot, 0.05f, 1, 1)
			],
			validTerrains: [Ids.Terrains.Plains.Grass, Ids.Terrains.Forests.Forest, Ids.Terrains.Plains.Hills],
			spawnWeight: 20f,
			respawnTime: 24f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Hunting.DeerTrail,
			text: Proto.CreateText("Deer Trail", "Fresh tracks indicate deer pass through here regularly."),
			iconName: "icon_node_deer",
			spriteName: "sprite_deer_trail",
			category: ResourceCategory.Hunting,
			gatheringSkill: Ids.Skills.Gathering.Hunting,
			gatheringDC: 12,
			gatherTime: 20f,
			primaryYield: Ids.Items.Materials.Venison,
			minYield: 2,
			maxYield: 4,
			totalGathers: 2,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.DeerHide, 0.7f, 1, 1),
				new ResourceDrop(Ids.Items.Materials.Antlers, 0.3f, 1, 2)
			],
			validTerrains: [Ids.Terrains.Forests.Forest, Ids.Terrains.Plains.Grass, Ids.Terrains.Plains.Hills],
			spawnWeight: 10f,
			respawnTime: 48f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Hunting.BoarDen,
			text: Proto.CreateText("Boar Den", "A den of wild boars. Dangerous but rewarding."),
			iconName: "icon_node_boar",
			spriteName: "sprite_boar_den",
			category: ResourceCategory.Hunting,
			gatheringSkill: Ids.Skills.Gathering.Hunting,
			gatheringDC: 14,
			gatherTime: 25f,
			primaryYield: Ids.Items.Materials.BoarMeat,
			minYield: 3,
			maxYield: 5,
			totalGathers: 2,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.BoarHide, 0.6f, 1, 1),
				new ResourceDrop(Ids.Items.Materials.BoarTusk, 0.4f, 1, 2)
			],
			onGatherEvent: Ids.Events.Random.AngryBoar,
			gatherEventChance: 0.25f,
			validTerrains: [Ids.Terrains.Forests.Forest],
			spawnWeight: 8f,
			respawnTime: 72f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Hunting.WolfPack,
			text: Proto.CreateText("Wolf Territory", "Wolves hunt in this area. Hunting them is risky."),
			iconName: "icon_node_wolf",
			spriteName: "sprite_wolf_territory",
			category: ResourceCategory.Hunting,
			gatheringSkill: Ids.Skills.Gathering.Hunting,
			gatheringDC: 16,
			gatherTime: 30f,
			primaryYield: Ids.Items.Materials.WolfMeat,
			minYield: 1,
			maxYield: 2,
			totalGathers: 3,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.WolfPelt, 0.7f, 1, 1),
				new ResourceDrop(Ids.Items.Materials.WolfFang, 0.5f, 1, 2)
			],
			onGatherEvent: Ids.Events.Random.WolfAmbush,
			gatherEventChance: 0.35f,
			validTerrains: [Ids.Terrains.Forests.Forest, Ids.Terrains.Frozen.Snow, Ids.Terrains.Mountains.Mountain],
			spawnWeight: 5f,
			minSpawnDistance: 10f,
			respawnTime: 96f
		));

		// ═══════════════════════════════════════════════════════════════
		// EXOTIC GAME
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Hunting.BearCave,
			text: Proto.CreateText("Bear Cave", "A cave where a bear makes its den. Extremely dangerous."),
			iconName: "icon_node_bear",
			spriteName: "sprite_bear_cave",
			category: ResourceCategory.Hunting,
			gatheringSkill: Ids.Skills.Gathering.Hunting,
			gatheringDC: 18,
			gatherTime: 35f,
			primaryYield: Ids.Items.Materials.BearMeat,
			minYield: 4,
			maxYield: 6,
			totalGathers: 1,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.BearPelt, 0.8f, 1, 1),
				new ResourceDrop(Ids.Items.Materials.BearClaw, 0.6f, 2, 4)
			],
			onGatherEvent: Ids.Events.Random.BearAttack,
			gatherEventChance: 0.5f,
			validTerrains: [Ids.Terrains.Forests.Forest, Ids.Terrains.Mountains.Mountain, Ids.Terrains.Underground.Cave],
			spawnWeight: 3f,
			minSpawnDistance: 15f,
			respawnTime: 168f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Hunting.GiantSpiderNest,
			text: Proto.CreateText("Giant Spider Nest", "Webs everywhere. Giant spiders lurk within."),
			iconName: "icon_node_spider",
			spriteName: "sprite_spider_nest",
			category: ResourceCategory.Hunting,
			gatheringSkill: Ids.Skills.Gathering.Hunting,
			gatheringDC: 14,
			gatherTime: 20f,
			primaryYield: Ids.Items.Materials.SpiderSilk,
			minYield: 2,
			maxYield: 4,
			totalGathers: 3,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.VenomSac, 0.4f, 1, 2),
				new ResourceDrop(Ids.Items.Materials.SpiderEgg, 0.2f, 1, 3)
			],
			onGatherEvent: Ids.Events.Random.SpiderAmbush,
			gatherEventChance: 0.4f,
			validTerrains: [Ids.Terrains.Underground.Cave, Ids.Terrains.Forests.Forest, Ids.Terrains.Ruins.OpenRuins],
			spawnWeight: 6f,
			minSpawnDistance: 10f,
			respawnTime: 72f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Hunting.WyvernNest,
			text: Proto.CreateText("Wyvern Nest", "A nest of the fearsome wyvern. Only the brave approach."),
			iconName: "icon_node_wyvern",
			spriteName: "sprite_wyvern_nest",
			category: ResourceCategory.Hunting,
			gatheringSkill: Ids.Skills.Gathering.Hunting,
			gatheringDC: 20,
			gatherTime: 40f,
			primaryYield: Ids.Items.Materials.WyvernScale,
			minYield: 2,
			maxYield: 4,
			totalGathers: 1,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.WyvernEgg, 0.15f, 1, 1),
				new ResourceDrop(Ids.Items.Materials.WyvernWing, 0.3f, 1, 2)
			],
			onGatherEvent: Ids.Events.Random.WyvernAttack,
			gatherEventChance: 0.6f,
			validTerrains: [Ids.Terrains.Mountains.Mountain],
			spawnWeight: 1f,
			minSpawnDistance: 30f,
			respawnTime: -1f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Hunting.FoxDen,
			text: Proto.CreateText("Fox Den", "A clever fox makes its home here."),
			iconName: "icon_node_fox",
			spriteName: "sprite_fox_den",
			category: ResourceCategory.Hunting,
			gatheringSkill: Ids.Skills.Gathering.Hunting,
			gatheringDC: 13,
			gatherTime: 15f,
			primaryYield: Ids.Items.Materials.FoxMeat,
			minYield: 1,
			maxYield: 2,
			totalGathers: 2,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.FoxPelt, 0.7f, 1, 1)
			],
			validTerrains: [Ids.Terrains.Forests.Forest, Ids.Terrains.Plains.Grass, Ids.Terrains.Plains.Hills],
			spawnWeight: 8f,
			respawnTime: 48f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Hunting.OwlNest,
			text: Proto.CreateText("Owl Nest", "A great owl nests in these trees."),
			iconName: "icon_node_owl",
			spriteName: "sprite_owl_nest",
			category: ResourceCategory.Hunting,
			gatheringSkill: Ids.Skills.Gathering.Hunting,
			gatheringDC: 14,
			gatherTime: 15f,
			primaryYield: Ids.Items.Materials.OwlFeather,
			minYield: 2,
			maxYield: 4,
			totalGathers: 3,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.OwlPellet, 0.5f, 1, 2)
			],
			validTerrains: [Ids.Terrains.Forests.Forest],
			spawnWeight: 6f,
			respawnTime: 48f
		));
	}

	#endregion

		#region Foraging Spots

	private void RegisterForagingSpots(GameDb db) {
		// ═══════════════════════════════════════════════════════════════
		// FOOD FORAGING
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Foraging.BerryBush,
			text: Proto.CreateText("Berry Bush", "A bush laden with ripe berries."),
			iconName: "icon_node_berries",
			spriteName: "sprite_berry_bush",
			category: ResourceCategory.Foraging,
			gatheringSkill: Ids.Skills.Gathering.Foraging,
			gatheringDC: 5,
			gatherTime: 3f,
			primaryYield: Ids.Items.Materials.Berries,
			minYield: 2,
			maxYield: 5,
			totalGathers: 4,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.RareBerries, 0.1f, 1, 2)
			],
			validTerrains: [Ids.Terrains.Forests.Forest, Ids.Terrains.Plains.Grass, Ids.Terrains.Plains.Hills],
			spawnWeight: 25f,
			respawnTime: 24f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Foraging.MushroomCluster,
			text: Proto.CreateText("Mushroom Cluster", "A cluster of edible mushrooms."),
			iconName: "icon_node_mushrooms",
			spriteName: "sprite_mushroom_cluster",
			category: ResourceCategory.Foraging,
			gatheringSkill: Ids.Skills.Gathering.Foraging,
			gatheringDC: 8,
			gatherTime: 4f,
			primaryYield: Ids.Items.Materials.Mushrooms,
			minYield: 1,
			maxYield: 4,
			totalGathers: 3,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.GlowingMushroom, 0.1f, 1, 1),
				new ResourceDrop(Ids.Items.Materials.PoisonousMushroom, 0.15f, 1, 2)
			],
			validTerrains: [Ids.Terrains.Forests.Forest, Ids.Terrains.Underground.Cave, Ids.Terrains.Wetlands.Swamp],
			spawnWeight: 18f,
			respawnTime: 18f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Foraging.AppleTree,
			text: Proto.CreateText("Apple Tree", "A tree heavy with ripe apples."),
			iconName: "icon_node_apples",
			spriteName: "sprite_apple_tree",
			category: ResourceCategory.Foraging,
			gatheringSkill: Ids.Skills.Gathering.Foraging,
			gatheringDC: 6,
			gatherTime: 5f,
			primaryYield: Ids.Items.Materials.Apples,
			minYield: 2,
			maxYield: 4,
			totalGathers: 5,
			validTerrains: [Ids.Terrains.Forests.Forest, Ids.Terrains.Plains.Grass, Ids.Terrains.Settlements.Village],
			spawnWeight: 12f,
			respawnTime: 48f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Foraging.NutTree,
			text: Proto.CreateText("Nut Tree", "A tree with edible nuts."),
			iconName: "icon_node_nuts",
			spriteName: "sprite_nut_tree",
			category: ResourceCategory.Foraging,
			gatheringSkill: Ids.Skills.Gathering.Foraging,
			gatheringDC: 7,
			gatherTime: 5f,
			primaryYield: Ids.Items.Materials.Nuts,
			minYield: 2,
			maxYield: 5,
			totalGathers: 4,
			validTerrains: [Ids.Terrains.Forests.Forest],
			spawnWeight: 12f,
			respawnTime: 48f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Foraging.WildVegetables,
			text: Proto.CreateText("Wild Vegetables", "Edible roots and vegetables growing wild."),
			iconName: "icon_node_vegetables",
			spriteName: "sprite_wild_vegetables",
			category: ResourceCategory.Foraging,
			gatheringSkill: Ids.Skills.Gathering.Foraging,
			gatheringDC: 7,
			gatherTime: 5f,
			primaryYield: Ids.Items.Materials.WildVegetables,
			minYield: 1,
			maxYield: 3,
			totalGathers: 3,
			validTerrains: [Ids.Terrains.Plains.Grass, Ids.Terrains.Forests.Forest, Ids.Terrains.Water.River],
			spawnWeight: 15f,
			respawnTime: 24f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Foraging.HoneyBeehive,
			text: Proto.CreateText("Beehive", "A wild beehive dripping with honey."),
			iconName: "icon_node_beehive",
			spriteName: "sprite_beehive",
			category: ResourceCategory.Foraging,
			gatheringSkill: Ids.Skills.Gathering.Foraging,
			gatheringDC: 12,
			gatherTime: 8f,
			primaryYield: Ids.Items.Materials.Honey,
			minYield: 1,
			maxYield: 3,
			totalGathers: 2,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.Beeswax, 0.5f, 1, 2)
			],
			onGatherEvent: Ids.Events.Random.AngryBees,
			gatherEventChance: 0.3f,
			validTerrains: [Ids.Terrains.Forests.Forest, Ids.Terrains.Plains.Grass],
			spawnWeight: 6f,
			respawnTime: 72f
		));

		// ═══════════════════════════════════════════════════════════════
		// OTHER FORAGING
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Foraging.BirdNest,
			text: Proto.CreateText("Bird Nest", "A nest with eggs and feathers."),
			iconName: "icon_node_birdnest",
			spriteName: "sprite_bird_nest",
			category: ResourceCategory.Foraging,
			gatheringSkill: Ids.Skills.Gathering.Foraging,
			gatheringDC: 10,
			gatherTime: 5f,
			primaryYield: Ids.Items.Materials.Eggs,
			minYield: 1,
			maxYield: 3,
			totalGathers: 1,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.Feathers, 0.6f, 2, 4)
			],
			validTerrains: [Ids.Terrains.Forests.Forest, Ids.Terrains.Plains.Grass, Ids.Terrains.Mountains.Mountain],
			spawnWeight: 10f,
			respawnTime: 48f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Foraging.SeashellBeach,
			text: Proto.CreateText("Shell Beach", "A stretch of beach covered in shells."),
			iconName: "icon_node_shells",
			spriteName: "sprite_shell_beach",
			category: ResourceCategory.Foraging,
			gatheringSkill: Ids.Skills.Gathering.Foraging,
			gatheringDC: 5,
			gatherTime: 4f,
			primaryYield: Ids.Items.Materials.Seashells,
			minYield: 3,
			maxYield: 6,
			totalGathers: 6,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.Pearl, 0.05f, 1, 1),
				new ResourceDrop(Ids.Items.Materials.Driftwood, 0.3f, 1, 2)
			],
			validTerrains: [Ids.Terrains.Coastal.Beach],
			spawnWeight: 15f,
			respawnTime: 12f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Foraging.ClamBed,
			text: Proto.CreateText("Clam Bed", "A bed of clams in the shallow water."),
			iconName: "icon_node_clams",
			spriteName: "sprite_clam_bed",
			category: ResourceCategory.Foraging,
			gatheringSkill: Ids.Skills.Gathering.Foraging,
			gatheringDC: 8,
			gatherTime: 6f,
			primaryYield: Ids.Items.Materials.Clams,
			minYield: 2,
			maxYield: 4,
			totalGathers: 4,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.Pearl, 0.1f, 1, 1)
			],
			validTerrains: [Ids.Terrains.Coastal.Beach, Ids.Terrains.Water.River],
			spawnWeight: 10f,
			respawnTime: 24f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Foraging.OysterRocks,
			text: Proto.CreateText("Oyster Rocks", "Rocks covered with oysters."),
			iconName: "icon_node_oysters",
			spriteName: "sprite_oyster_rocks",
			category: ResourceCategory.Foraging,
			gatheringSkill: Ids.Skills.Gathering.Foraging,
			gatheringDC: 10,
			gatherTime: 8f,
			requiredTool: ToolType.Knife,
			primaryYield: Ids.Items.Materials.Oysters,
			minYield: 2,
			maxYield: 4,
			totalGathers: 3,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.Pearl, 0.15f, 1, 1),
				new ResourceDrop(Ids.Items.Materials.BlackPearl, 0.02f, 1, 1)
			],
			validTerrains: [Ids.Terrains.Coastal.Beach],
			spawnWeight: 6f,
			respawnTime: 48f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Foraging.GrapeVine,
			text: Proto.CreateText("Wild Grape Vine", "A vine heavy with wild grapes."),
			iconName: "icon_node_grapes",
			spriteName: "sprite_grape_vine",
			category: ResourceCategory.Foraging,
			gatheringSkill: Ids.Skills.Gathering.Foraging,
			gatheringDC: 6,
			gatherTime: 4f,
			primaryYield: Ids.Items.Materials.Grapes,
			minYield: 2,
			maxYield: 5,
			totalGathers: 4,
			validTerrains: [Ids.Terrains.Forests.Forest, Ids.Terrains.Plains.Hills],
			spawnWeight: 10f,
			respawnTime: 36f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Foraging.WildOnions,
			text: Proto.CreateText("Wild Onions", "A patch of pungent wild onions."),
			iconName: "icon_node_onions",
			spriteName: "sprite_wild_onions",
			category: ResourceCategory.Foraging,
			gatheringSkill: Ids.Skills.Gathering.Foraging,
			gatheringDC: 6,
			gatherTime: 4f,
			primaryYield: Ids.Items.Materials.WildOnions,
			minYield: 2,
			maxYield: 4,
			totalGathers: 3,
			validTerrains: [Ids.Terrains.Plains.Grass, Ids.Terrains.Forests.Forest],
			spawnWeight: 12f,
			respawnTime: 24f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Foraging.TruffleGround,
			text: Proto.CreateText("Truffle Ground", "This soil may hide valuable truffles beneath."),
			iconName: "icon_node_truffle",
			spriteName: "sprite_truffle_ground",
			category: ResourceCategory.Foraging,
			gatheringSkill: Ids.Skills.Gathering.Foraging,
			gatheringDC: 16,
			gatherTime: 10f,
			primaryYield: Ids.Items.Materials.Truffle,
			minYield: 1,
			maxYield: 2,
			totalGathers: 2,
			validTerrains: [Ids.Terrains.Forests.Forest],
			spawnWeight: 3f,
			minSpawnDistance: 15f,
			respawnTime: 96f
		));
	}

	#endregion

	#region Special Resources

	private void RegisterSpecialResources(GameDb db) {
		// ═══════════════════════════════════════════════════════════════
		// MAGICAL RESOURCES
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Special.ManaWell,
			text: Proto.CreateText("Mana Well", "A natural spring of pure magical energy."),
			iconName: "icon_node_mana_well",
			spriteName: "sprite_mana_well",
			category: ResourceCategory.Gathering,
			gatheringSkill: Ids.Skills.Magic.ManaPool,
			gatheringDC: 14,
			gatherTime: 10f,
			primaryYield: Ids.Items.Materials.ManaEssence,
			minYield: 1,
			maxYield: 2,
			totalGathers: 3,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.MagicCrystal, 0.2f, 1, 1)
			],
			onDiscoverEvent: Ids.Events.Random.ManaWellBlessing,
			validTerrains: [Ids.Terrains.Ruins.OpenRuins, Ids.Terrains.Forests.Forest],
			spawnWeight: 2f,
			minSpawnDistance: 20f,
			respawnTime: 120f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Special.FogFragment,
			text: Proto.CreateText("Fog Fragment", "A swirling concentration of the Fog itself, somehow stable."),
			iconName: "icon_node_fog",
			spriteName: "sprite_fog_fragment",
			category: ResourceCategory.Gathering,
			gatheringSkill: Ids.Skills.Knowledge.FogLore,
			gatheringDC: 18,
			gatherTime: 15f,
			primaryYield: Ids.Items.Materials.FogEssence,
			minYield: 1,
			maxYield: 1,
			totalGathers: 1,
			onGatherEvent: Ids.Events.Random.FogWhispers,
			gatherEventChance: 0.5f,
			validTerrains: [Ids.Terrains.Supernatural.Corrupted, Ids.Terrains.Supernatural.Wasteland],
			spawnWeight: 0.5f,
			minSpawnDistance: 30f,
			respawnTime: -1f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Special.TimeCrack,
			text: Proto.CreateText("Time Crack", "A fissure in reality where time itself bleeds through."),
			iconName: "icon_node_time_crack",
			spriteName: "sprite_time_crack",
			category: ResourceCategory.Gathering,
			gatheringSkill: Ids.Skills.Knowledge.TimeLore,
			gatheringDC: 20,
			gatherTime: 20f,
			primaryYield: Ids.Items.Materials.TimeShard,
			minYield: 1,
			maxYield: 1,
			totalGathers: 1,
			onGatherEvent: Ids.Events.Random.TimeAnomaly,
			gatherEventChance: 0.4f,
			onDepletedEvent: Ids.Events.Random.TimeCrackSealed,
			validTerrains: [Ids.Terrains.Ruins.OpenRuins, Ids.Terrains.Supernatural.Corrupted],
			spawnWeight: 0.3f,
			minSpawnDistance: 35f,
			respawnTime: -1f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Special.DragonBones,
			text: Proto.CreateText("Dragon Bones", "The ancient remains of a fallen dragon."),
			iconName: "icon_node_dragon_bones",
			spriteName: "sprite_dragon_bones",
			category: ResourceCategory.Gathering,
			gatheringSkill: Ids.Skills.Survival.General,
			gatheringDC: 14,
			gatherTime: 12f,
			primaryYield: Ids.Items.Materials.DragonBone,
			minYield: 2,
			maxYield: 4,
			totalGathers: 3,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.DragonScale, 0.3f, 1, 2),
				new ResourceDrop(Ids.Items.Materials.DragonTooth, 0.2f, 1, 1)
			],
			onDiscoverEvent: Ids.Events.Random.DragonGhost,
			validTerrains: [Ids.Terrains.Mountains.Mountain, Ids.Terrains.Supernatural.Wasteland, Ids.Terrains.Underground.Cave],
			spawnWeight: 1f,
			minSpawnDistance: 25f,
			respawnTime: -1f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Special.FallenStar,
			text: Proto.CreateText("Fallen Star", "A meteorite still warm from its descent."),
			iconName: "icon_node_fallen_star",
			spriteName: "sprite_fallen_star",
			category: ResourceCategory.Mining,
			gatheringSkill: Ids.Skills.Gathering.Mining,
			gatheringDC: 16,
			gatherTime: 15f,
			requiredTool: ToolType.Pickaxe,
			primaryYield: Ids.Items.Materials.StarMetal,
			minYield: 1,
			maxYield: 2,
			totalGathers: 2,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.CosmicDust, 0.4f, 1, 3)
			],
			onDiscoverEvent: Ids.Events.Random.StarfallWish,
			validTerrains: [Ids.Terrains.Plains.Grass, Ids.Terrains.Desert.OpenDesert, Ids.Terrains.Mountains.Mountain],
			spawnWeight: 0.5f,
			minSpawnDistance: 20f,
			respawnTime: -1f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Special.AncientShrine,
			text: Proto.CreateText("Ancient Shrine", "A forgotten shrine to an unknown deity."),
			iconName: "icon_node_shrine",
			spriteName: "sprite_ancient_shrine",
			category: ResourceCategory.Gathering,
			gatheringSkill: Ids.Skills.Knowledge.Religion,
			gatheringDC: 12,
			gatherTime: 10f,
			primaryYield: Ids.Items.Materials.SacredIncense,
			minYield: 1,
			maxYield: 2,
			totalGathers: 2,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.HolyWater, 0.3f, 1, 1),
				new ResourceDrop(Ids.Items.Materials.AncientRelic, 0.1f, 1, 1)
			],
			onDiscoverEvent: Ids.Events.Random.ShrinePrayer,
			validTerrains: [Ids.Terrains.Ruins.OpenRuins, Ids.Terrains.Forests.Forest, Ids.Terrains.Mountains.Mountain],
			spawnWeight: 2f,
			minSpawnDistance: 15f,
			respawnTime: 168f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Special.UndeadRemains,
			text: Proto.CreateText("Undead Remains", "The remains of defeated undead. Dark energy lingers."),
			iconName: "icon_node_undead",
			spriteName: "sprite_undead_remains",
			category: ResourceCategory.Gathering,
			gatheringSkill: Ids.Skills.Survival.General,
			gatheringDC: 10,
			gatherTime: 6f,
			primaryYield: Ids.Items.Materials.Bone,
			minYield: 2,
			maxYield: 4,
			totalGathers: 2,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.Ectoplasm, 0.3f, 1, 1),
				new ResourceDrop(Ids.Items.Materials.SoulFragment, 0.1f, 1, 1)
			],
			validTerrains: [Ids.Terrains.Ruins.Graveyard, Ids.Terrains.Ruins.OpenRuins, Ids.Terrains.Supernatural.Corrupted],
			spawnWeight: 8f,
			respawnTime: 24f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Special.DemonPortal,
			text: Proto.CreateText("Demon Portal Residue", "The remains of a closed demon portal. Still radiates dark energy."),
			iconName: "icon_node_demon_portal",
			spriteName: "sprite_demon_portal",
			category: ResourceCategory.Gathering,
			gatheringSkill: Ids.Skills.Knowledge.Planes,
			gatheringDC: 16,
			gatherTime: 12f,
			primaryYield: Ids.Items.Materials.DemonEssence,
			minYield: 1,
			maxYield: 2,
			totalGathers: 2,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.DemonHorn, 0.2f, 1, 1),
				new ResourceDrop(Ids.Items.Materials.InfernalAsh, 0.4f, 1, 2)
			],
			onGatherEvent: Ids.Events.Random.DemonWhisper,
			gatherEventChance: 0.3f,
			validTerrains: [Ids.Terrains.Supernatural.Corrupted, Ids.Terrains.Volcanic.VolcanicPlain],
			spawnWeight: 2f,
			minSpawnDistance: 25f,
			respawnTime: 96f
		));

		// ═══════════════════════════════════════════════════════════════
		// TREASURE
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Special.BuriedTreasure,
			text: Proto.CreateText("Buried Treasure", "Something is buried here. An X marks the spot."),
			iconName: "icon_node_treasure",
			spriteName: "sprite_buried_treasure",
			category: ResourceCategory.Gathering,
			gatheringSkill: Ids.Skills.Awareness.Perception,
			gatheringDC: 12,
			gatherTime: 15f,
			requiredTool: ToolType.Shovel,
			primaryYield: Ids.Items.Materials.GoldCoins,
			minYield: 10,
			maxYield: 50,
			totalGathers: 1,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.GemRuby, 0.2f, 1, 2),
				new ResourceDrop(Ids.Items.Materials.GemEmerald, 0.15f, 1, 1),
				new ResourceDrop(Ids.Items.Materials.AncientRelic, 0.1f, 1, 1)
			],
			onGatherEvent: Ids.Events.Random.TreasureCurse,
			gatherEventChance: 0.15f,
			validTerrains: [Ids.Terrains.Coastal.Beach, Ids.Terrains.Desert.OpenDesert, Ids.Terrains.Ruins.OpenRuins],
			spawnWeight: 1f,
			minSpawnDistance: 15f,
			respawnTime: -1f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Special.AbandonedCart,
			text: Proto.CreateText("Abandoned Cart", "A merchant's cart left behind. Who knows what's inside?"),
			iconName: "icon_node_cart",
			spriteName: "sprite_abandoned_cart",
			category: ResourceCategory.Gathering,
			gatheringSkill: Ids.Skills.Awareness.Perception,
			gatheringDC: 8,
			gatherTime: 10f,
			primaryYield: Ids.Items.Materials.Cloth,
			minYield: 2,
			maxYield: 4,
			totalGathers: 2,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.Rope, 0.4f, 1, 2),
				new ResourceDrop(Ids.Items.Materials.Leather, 0.3f, 1, 2),
				new ResourceDrop(Ids.Items.Materials.GoldCoins, 0.2f, 5, 15)
			],
			onDiscoverEvent: Ids.Events.Random.CartOwner,
			validTerrains: [Ids.Terrains.Roads.Road, Ids.Terrains.Plains.Grass, Ids.Terrains.Forests.Forest],
			spawnWeight: 3f,
			respawnTime: -1f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Special.ShipwreckDebris,
			text: Proto.CreateText("Shipwreck Debris", "Flotsam from a wrecked ship."),
			iconName: "icon_node_shipwreck",
			spriteName: "sprite_shipwreck",
			category: ResourceCategory.Gathering,
			gatheringSkill: Ids.Skills.Awareness.Perception,
			gatheringDC: 10,
			gatherTime: 12f,
			primaryYield: Ids.Items.Materials.Driftwood,
			minYield: 2,
			maxYield: 5,
			totalGathers: 4,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.Rope, 0.5f, 1, 3),
				new ResourceDrop(Ids.Items.Materials.Cloth, 0.4f, 1, 2),
				new ResourceDrop(Ids.Items.Materials.GoldCoins, 0.15f, 5, 20),
				new ResourceDrop(Ids.Items.Materials.Barnacles, 0.3f, 2, 4)
			],
			validTerrains: [Ids.Terrains.Coastal.Beach],
			spawnWeight: 4f,
			respawnTime: -1f
		));
	}

	#endregion

	#region Environmental Resources

	private void RegisterEnvironmentalResources(GameDb db) {
		// ═══════════════════════════════════════════════════════════════
		// SNOW & ICE
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Environment.SnowPile,
			text: Proto.CreateText("Snow Pile", "A pile of fresh snow. Could be useful."),
			iconName: "icon_node_snow",
			spriteName: "sprite_snow_pile",
			category: ResourceCategory.Gathering,
			gatheringSkill: Ids.Skills.Survival.General,
			gatheringDC: 4,
			gatherTime: 3f,
			requiredTool: ToolType.Shovel,
			primaryYield: Ids.Items.Materials.Snow,
			minYield: 3,
			maxYield: 6,
			totalGathers: 5,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.FrozenBerries, 0.1f, 1, 2)
			],
			validTerrains: [Ids.Terrains.Frozen.Snow],
			spawnWeight: 30f,
			respawnTime: 6f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Environment.IceBlock,
			text: Proto.CreateText("Ice Block", "A solid block of clear ice."),
			iconName: "icon_node_ice",
			spriteName: "sprite_ice_block",
			category: ResourceCategory.Mining,
			gatheringSkill: Ids.Skills.Gathering.Mining,
			gatheringDC: 8,
			gatherTime: 6f,
			requiredTool: ToolType.Pickaxe,
			primaryYield: Ids.Items.Materials.Ice,
			minYield: 2,
			maxYield: 4,
			totalGathers: 3,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.EternalIce, 0.05f, 1, 1)
			],
			validTerrains: [Ids.Terrains.Frozen.Snow, Ids.Terrains.Mountains.Mountain],
			spawnWeight: 15f,
			respawnTime: 24f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Environment.Icicles,
			text: Proto.CreateText("Icicles", "Sharp icicles hanging from an overhang."),
			iconName: "icon_node_icicles",
			spriteName: "sprite_icicles",
			category: ResourceCategory.Gathering,
			gatheringSkill: Ids.Skills.Survival.General,
			gatheringDC: 10,
			gatherTime: 4f,
			primaryYield: Ids.Items.Materials.Ice,
			minYield: 1,
			maxYield: 3,
			totalGathers: 2,
			validTerrains: [Ids.Terrains.Frozen.Snow, Ids.Terrains.Mountains.Mountain, Ids.Terrains.Underground.Cave],
			spawnWeight: 12f,
			respawnTime: 12f
		));

		// ═══════════════════════════════════════════════════════════════
		// DESERT & SAND
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Environment.Oasis,
			text: Proto.CreateText("Oasis", "A small pool of fresh water in the desert."),
			iconName: "icon_node_oasis",
			spriteName: "sprite_oasis",
			category: ResourceCategory.Gathering,
			gatheringSkill: Ids.Skills.Survival.General,
			gatheringDC: 6,
			gatherTime: 5f,
			primaryYield: Ids.Items.Materials.FreshWater,
			minYield: 2,
			maxYield: 4,
			totalGathers: 5,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.DateFruit, 0.3f, 2, 4),
				new ResourceDrop(Ids.Items.Materials.DesertFlower, 0.2f, 1, 2)
			],
			validTerrains: [Ids.Terrains.Desert.OpenDesert],
			spawnWeight: 3f,
			respawnTime: 48f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Environment.CactusPlant,
			text: Proto.CreateText("Cactus", "A thorny cactus. Contains water and useful materials."),
			iconName: "icon_node_cactus",
			spriteName: "sprite_cactus",
			category: ResourceCategory.Herbalism,
			gatheringSkill: Ids.Skills.Gathering.Herbalism,
			gatheringDC: 10,
			gatherTime: 5f,
			requiredTool: ToolType.Knife,
			primaryYield: Ids.Items.Materials.CactusFlesh,
			minYield: 1,
			maxYield: 3,
			totalGathers: 2,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.CactusNeedle, 0.5f, 2, 5),
				new ResourceDrop(Ids.Items.Materials.CactusFlower, 0.2f, 1, 1)
			],
			validTerrains: [Ids.Terrains.Desert.OpenDesert],
			spawnWeight: 15f,
			respawnTime: 72f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Environment.DesertRuins,
			text: Proto.CreateText("Desert Ruins", "Half-buried ruins poking through the sand."),
			iconName: "icon_node_desert_ruins",
			spriteName: "sprite_desert_ruins",
			category: ResourceCategory.Gathering,
			gatheringSkill: Ids.Skills.Awareness.Perception,
			gatheringDC: 12,
			gatherTime: 15f,
			requiredTool: ToolType.Shovel,
			primaryYield: Ids.Items.Materials.AncientPottery,
			minYield: 1,
			maxYield: 2,
			totalGathers: 3,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.AncientRelic, 0.15f, 1, 1),
				new ResourceDrop(Ids.Items.Materials.GoldCoins, 0.2f, 5, 15),
				new ResourceDrop(Ids.Items.Materials.AncientScroll, 0.1f, 1, 1)
			],
			onDiscoverEvent: Ids.Events.Random.AncientTrap,
			validTerrains: [Ids.Terrains.Desert.OpenDesert],
			spawnWeight: 3f,
			minSpawnDistance: 15f,
			respawnTime: -1f
		));

		// ═══════════════════════════════════════════════════════════════
		// VOLCANIC & FIRE
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Environment.LavaRock,
			text: Proto.CreateText("Lava Rock", "Cooled lava with useful minerals."),
			iconName: "icon_node_lava_rock",
			spriteName: "sprite_lava_rock",
			category: ResourceCategory.Mining,
			gatheringSkill: Ids.Skills.Gathering.Mining,
			gatheringDC: 12,
			gatherTime: 10f,
			requiredTool: ToolType.Pickaxe,
			primaryYield: Ids.Items.Materials.Basalt,
			minYield: 2,
			maxYield: 4,
			totalGathers: 4,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.Obsidian, 0.2f, 1, 1),
				new ResourceDrop(Ids.Items.Materials.FireCrystal, 0.1f, 1, 1)
			],
			validTerrains: [Ids.Terrains.Volcanic.VolcanicPlain],
			spawnWeight: 15f,
			respawnTime: 48f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Environment.GeyserVent,
			text: Proto.CreateText("Geyser Vent", "A vent releasing hot steam and minerals."),
			iconName: "icon_node_geyser",
			spriteName: "sprite_geyser",
			category: ResourceCategory.Gathering,
			gatheringSkill: Ids.Skills.Gathering.Mining,
			gatheringDC: 14,
			gatherTime: 8f,
			primaryYield: Ids.Items.Materials.SulfiteDeposit,
			minYield: 1,
			maxYield: 3,
			totalGathers: 3,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.MineralSalt, 0.4f, 1, 3)
			],
			onGatherEvent: Ids.Events.Random.GeyserEruption,
			gatherEventChance: 0.2f,
			validTerrains: [Ids.Terrains.Volcanic.VolcanicPlain, Ids.Terrains.Mountains.Mountain],
			spawnWeight: 6f,
			respawnTime: 24f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Environment.AshPile,
			text: Proto.CreateText("Volcanic Ash Pile", "A pile of volcanic ash."),
			iconName: "icon_node_ash",
			spriteName: "sprite_ash_pile",
			category: ResourceCategory.Gathering,
			gatheringSkill: Ids.Skills.Survival.General,
			gatheringDC: 5,
			gatherTime: 3f,
			requiredTool: ToolType.Shovel,
			primaryYield: Ids.Items.Materials.VolcanicAsh,
			minYield: 3,
			maxYield: 6,
			totalGathers: 6,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.InfernalAsh, 0.1f, 1, 1)
			],
			validTerrains: [Ids.Terrains.Volcanic.VolcanicPlain],
			spawnWeight: 20f,
			respawnTime: 12f
		));

		// ═══════════════════════════════════════════════════════════════
		// WATER SOURCES
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Environment.FreshwaterSpring,
			text: Proto.CreateText("Freshwater Spring", "A natural spring of clean water."),
			iconName: "icon_node_spring",
			spriteName: "sprite_freshwater_spring",
			category: ResourceCategory.Gathering,
			gatheringSkill: Ids.Skills.Survival.General,
			gatheringDC: 5,
			gatherTime: 4f,
			primaryYield: Ids.Items.Materials.FreshWater,
			minYield: 2,
			maxYield: 4,
			totalGathers: 8,
			validTerrains: [Ids.Terrains.Forests.Forest, Ids.Terrains.Plains.Hills, Ids.Terrains.Mountains.Mountain],
			spawnWeight: 10f,
			respawnTime: 6f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Environment.Waterfall,
			text: Proto.CreateText("Waterfall", "A cascading waterfall. Treasures sometimes wash down."),
			iconName: "icon_node_waterfall",
			spriteName: "sprite_waterfall",
			category: ResourceCategory.Gathering,
			gatheringSkill: Ids.Skills.Awareness.Perception,
			gatheringDC: 12,
			gatherTime: 10f,
			primaryYield: Ids.Items.Materials.FreshWater,
			minYield: 3,
			maxYield: 5,
			totalGathers: 5,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.GoldNugget, 0.1f, 1, 1),
				new ResourceDrop(Ids.Items.Materials.RiverStone, 0.4f, 2, 4),
				new ResourceDrop(Ids.Items.Materials.Pearl, 0.05f, 1, 1)
			],
			validTerrains: [Ids.Terrains.Water.River, Ids.Terrains.Mountains.Mountain],
			spawnWeight: 4f,
			respawnTime: 24f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Environment.MineralHotSpring,
			text: Proto.CreateText("Mineral Hot Spring", "A hot spring rich with healing minerals."),
			iconName: "icon_node_hotspring",
			spriteName: "sprite_hot_spring",
			category: ResourceCategory.Gathering,
			gatheringSkill: Ids.Skills.Survival.General,
			gatheringDC: 8,
			gatherTime: 6f,
			primaryYield: Ids.Items.Materials.MineralWater,
			minYield: 2,
			maxYield: 3,
			totalGathers: 4,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.MineralSalt, 0.3f, 1, 2)
			],
			onDiscoverEvent: Ids.Events.Random.HotSpringRest,
			validTerrains: [Ids.Terrains.Mountains.Mountain, Ids.Terrains.Volcanic.VolcanicPlain],
			spawnWeight: 4f,
			respawnTime: 48f
		));

		// ═══════════════════════════════════════════════════════════════
		// MISC ENVIRONMENTAL
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Environment.AbandonedCampfire,
			text: Proto.CreateText("Abandoned Campfire", "Someone camped here recently."),
			iconName: "icon_node_campfire",
			spriteName: "sprite_abandoned_campfire",
			category: ResourceCategory.Gathering,
			gatheringSkill: Ids.Skills.Awareness.Perception,
			gatheringDC: 6,
			gatherTime: 5f,
			primaryYield: Ids.Items.Materials.Charcoal,
			minYield: 2,
			maxYield: 4,
			totalGathers: 1,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.Firewood, 0.4f, 1, 2),
				new ResourceDrop(Ids.Items.Consumables.Rations, 0.2f, 1, 2)
			],
			onDiscoverEvent: Ids.Events.Random.AbandonedCamp,
			validTerrains: [Ids.Terrains.Forests.Forest, Ids.Terrains.Plains.Grass, Ids.Terrains.Plains.Hills, Ids.Terrains.Roads.Road],
			spawnWeight: 5f,
			respawnTime: -1f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Environment.FallenLog,
			text: Proto.CreateText("Fallen Log", "A rotting log full of insects and grubs."),
			iconName: "icon_node_fallen_log",
			spriteName: "sprite_fallen_log",
			category: ResourceCategory.Gathering,
			gatheringSkill: Ids.Skills.Survival.General,
			gatheringDC: 5,
			gatherTime: 4f,
			primaryYield: Ids.Items.Materials.RottenWood,
			minYield: 1,
			maxYield: 3,
			totalGathers: 2,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.Grubs, 0.5f, 2, 5),
				new ResourceDrop(Ids.Items.Materials.Mushrooms, 0.3f, 1, 2),
				new ResourceDrop(Ids.Items.Materials.Beetle, 0.2f, 1, 2)
			],
			validTerrains: [Ids.Terrains.Forests.Forest, Ids.Terrains.Wetlands.Swamp],
			spawnWeight: 12f,
			respawnTime: 48f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Environment.GraveSite,
			text: Proto.CreateText("Grave Site", "An unmarked grave. Disturbing it may have consequences."),
			iconName: "icon_node_grave",
			spriteName: "sprite_grave_site",
			category: ResourceCategory.Gathering,
			gatheringSkill: Ids.Skills.Survival.General,
			gatheringDC: 10,
			gatherTime: 10f,
			requiredTool: ToolType.Shovel,
			primaryYield: Ids.Items.Materials.Bone,
			minYield: 2,
			maxYield: 4,
			totalGathers: 1,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.GoldCoins, 0.3f, 5, 20),
				new ResourceDrop(Ids.Items.Materials.AncientRelic, 0.1f, 1, 1),
				new ResourceDrop(Ids.Items.Materials.Skull, 0.5f, 1, 1)
			],
			onGatherEvent: Ids.Events.Random.GraveDisturbance,
			gatherEventChance: 0.4f,
			validTerrains: [Ids.Terrains.Ruins.Graveyard, Ids.Terrains.Ruins.OpenRuins, Ids.Terrains.Forests.Forest],
			spawnWeight: 4f,
			minSpawnDistance: 10f,
			respawnTime: -1f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Environment.CobwebCorner,
			text: Proto.CreateText("Cobweb Corner", "Thick cobwebs in a corner. Spiders may still be near."),
			iconName: "icon_node_cobwebs",
			spriteName: "sprite_cobwebs",
			category: ResourceCategory.Gathering,
			gatheringSkill: Ids.Skills.Survival.General,
			gatheringDC: 8,
			gatherTime: 3f,
			primaryYield: Ids.Items.Materials.SpiderSilk,
			minYield: 1,
			maxYield: 2,
			totalGathers: 2,
			onGatherEvent: Ids.Events.Random.HiddenSpider,
			gatherEventChance: 0.25f,
			validTerrains: [Ids.Terrains.Underground.Cave, Ids.Terrains.Ruins.OpenRuins],
			spawnWeight: 10f,
			respawnTime: 24f
		));

		db.RegisterProto(new ResourceProto(
			id: Ids.Resources.Environment.Anthill,
			text: Proto.CreateText("Anthill", "A large anthill. The ants are industrious."),
			iconName: "icon_node_anthill",
			spriteName: "sprite_anthill",
			category: ResourceCategory.Gathering,
			gatheringSkill: Ids.Skills.Survival.General,
			gatheringDC: 8,
			gatherTime: 5f,
			primaryYield: Ids.Items.Materials.Ants,
			minYield: 3,
			maxYield: 8,
			totalGathers: 3,
			bonusDrops: [
				new ResourceDrop(Ids.Items.Materials.AntEggs, 0.3f, 1, 3)
			],
			onGatherEvent: Ids.Events.Random.AntSwarm,
			gatherEventChance: 0.2f,
			validTerrains: [Ids.Terrains.Forests.Forest, Ids.Terrains.Plains.Grass],
			spawnWeight: 8f,
			respawnTime: 48f
		));
	}

	#endregion
}
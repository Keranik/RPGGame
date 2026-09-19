using RPGGame.Core.Effects;
using RPGGame.Core.Generation;
using RPGGame.Core.Prototypes;
using RPGGame.Core.Prototypes.Activities;
using RPGGame.Core.Prototypes.Characters;
using RPGGame.Core.Prototypes.Combat;
using RPGGame.Core.Prototypes.Events;
using RPGGame.Core.Prototypes.Item;
using RPGGame.Core.Prototypes.Item.Equipment;
using RPGGame.Core.Prototypes.Item.Equipment.Armor;
using RPGGame.Core.Prototypes.Item.Equipment.Weapon;
using RPGGame.Core.Prototypes.Item.Resource;
using RPGGame.Core.Prototypes.Locations;
using RPGGame.Core.Prototypes.Lore;
using RPGGame.Core.Prototypes.Skills;
using RPGGame.Core.Prototypes.Spells;
using RPGGame.Core.Prototypes.Stats;
using RPGGame.Core.Prototypes.Village;

namespace RPGGame.Core;

/// <summary>
/// Partial class containing  R e s o u r c e s.Trim() IDs.
/// Fill in the corresponding nested static classes here.
/// </summary>
public static partial class Ids
{
    

	// ═══════════════════════════════════════════════════════════════════════
	// RESOURCES (Gatherable World Nodes)
	// ═══════════════════════════════════════════════════════════════════════

	public static class Resources {
		public static class Mining {
			// Ores
			public static readonly ResourceProto.ID IronVein = newId("IronVein");
			public static readonly ResourceProto.ID CopperDeposit = newId("CopperDeposit");
			public static readonly ResourceProto.ID SilverVein = newId("SilverVein");
			public static readonly ResourceProto.ID GoldVein = newId("GoldVein");
			public static readonly ResourceProto.ID MithrilVein = newId("MithrilVein");

			// Stone & Minerals
			public static readonly ResourceProto.ID StoneOutcrop = newId("StoneOutcrop");
			public static readonly ResourceProto.ID CrystalFormation = newId("CrystalFormation");
			public static readonly ResourceProto.ID SandPocket = newId("SandPocket");
			public static readonly ResourceProto.ID ClayDeposit = newId("ClayDeposit");
			public static readonly ResourceProto.ID SulfurDeposit = newId("SulfurDeposit");
			public static readonly ResourceProto.ID CoalSeam = newId("CoalSeam");
			public static readonly ResourceProto.ID ObsidianFormation = newId("ObsidianFormation");
		}

		public static class Herbalism {
			public static readonly ResourceProto.ID HerbPatch = newId("HerbPatch");
			public static readonly ResourceProto.ID WildflowerMeadow = newId("WildflowerMeadow");
			public static readonly ResourceProto.ID MoonbloomPatch = newId("MoonbloomPatch");
			public static readonly ResourceProto.ID Nightshade = newId("Nightshade");
			public static readonly ResourceProto.ID GlowMoss = newId("GlowMoss");
			public static readonly ResourceProto.ID MandrakeRoot = newId("MandrakeRoot");
			public static readonly ResourceProto.ID SwampMoss = newId("SwampMoss");
			public static readonly ResourceProto.ID FrostLichen = newId("FrostLichen");
			public static readonly ResourceProto.ID FireBloom = newId("FireBloom");
		}

		public static class Woodcutting {
			// Common Trees
			public static readonly ResourceProto.ID OakTree = newId("OakTree");
			public static readonly ResourceProto.ID PineTree = newId("PineTree");
			public static readonly ResourceProto.ID BirchTree = newId("BirchTree");
			public static readonly ResourceProto.ID WillowTree = newId("WillowTree");
			public static readonly ResourceProto.ID DeadTree = newId("DeadTree");

			// Special Trees
			public static readonly ResourceProto.ID IronwoodTree = newId("IronwoodTree");
			public static readonly ResourceProto.ID ElderTree = newId("ElderTree");
			public static readonly ResourceProto.ID CorruptedTree = newId("CorruptedTree");
			public static readonly ResourceProto.ID FrozenPine = newId("FrozenPine");
		}

		public static class Fishing {
			// Freshwater
			public static readonly ResourceProto.ID RiverFishingSpot = newId("RiverFishingSpot");
			public static readonly ResourceProto.ID PondFishingSpot = newId("PondFishingSpot");
			public static readonly ResourceProto.ID LakeFishingSpot = newId("LakeFishingSpot");
			public static readonly ResourceProto.ID SwampFishingSpot = newId("SwampFishingSpot");

			// Special
			public static readonly ResourceProto.ID MagicSpring = newId("MagicSpring");
			public static readonly ResourceProto.ID IceFishingHole = newId("IceFishingHole");
			public static readonly ResourceProto.ID LavaFishingSpot = newId("LavaFishingSpot");
		}

		public static class Hunting {
			// Common Game
			public static readonly ResourceProto.ID RabbitWarren = newId("RabbitWarren");
			public static readonly ResourceProto.ID DeerTrail = newId("DeerTrail");
			public static readonly ResourceProto.ID BoarDen = newId("BoarDen");
			public static readonly ResourceProto.ID WolfPack = newId("WolfPack");
			public static readonly ResourceProto.ID FoxDen = newId("FoxDen");
			public static readonly ResourceProto.ID OwlNest = newId("OwlNest");

			// Exotic Game
			public static readonly ResourceProto.ID BearCave = newId("BearCave");
			public static readonly ResourceProto.ID GiantSpiderNest = newId("GiantSpiderNest");
			public static readonly ResourceProto.ID WyvernNest = newId("WyvernNest");
		}

		public static class Foraging {
			// Food
			public static readonly ResourceProto.ID BerryBush = newId("BerryBush");
			public static readonly ResourceProto.ID MushroomCluster = newId("MushroomCluster");
			public static readonly ResourceProto.ID AppleTree = newId("AppleTree");
			public static readonly ResourceProto.ID NutTree = newId("NutTree");
			public static readonly ResourceProto.ID WildVegetables = newId("WildVegetables");
			public static readonly ResourceProto.ID HoneyBeehive = newId("HoneyBeehive");
			public static readonly ResourceProto.ID GrapeVine = newId("GrapeVine");
			public static readonly ResourceProto.ID WildOnions = newId("WildOnions");
			public static readonly ResourceProto.ID TruffleGround = newId("TruffleGround");

			// Other
			public static readonly ResourceProto.ID BirdNest = newId("BirdNest");
			public static readonly ResourceProto.ID SeashellBeach = newId("SeashellBeach");
			public static readonly ResourceProto.ID ClamBed = newId("ClamBed");
			public static readonly ResourceProto.ID OysterRocks = newId("OysterRocks");
		}

		public static class Special {
			// Magical
			public static readonly ResourceProto.ID ManaWell = newId("ManaWell");
			public static readonly ResourceProto.ID FogFragment = newId("FogFragment");
			public static readonly ResourceProto.ID TimeCrack = newId("TimeCrack");
			public static readonly ResourceProto.ID DragonBones = newId("DragonBones");
			public static readonly ResourceProto.ID FallenStar = newId("FallenStar");
			public static readonly ResourceProto.ID AncientShrine = newId("AncientShrine");
			public static readonly ResourceProto.ID UndeadRemains = newId("UndeadRemains");
			public static readonly ResourceProto.ID DemonPortal = newId("DemonPortal");

			// Treasure
			public static readonly ResourceProto.ID BuriedTreasure = newId("BuriedTreasure");
			public static readonly ResourceProto.ID AbandonedCart = newId("AbandonedCart");
			public static readonly ResourceProto.ID ShipwreckDebris = newId("ShipwreckDebris");
		}

		public static class Environment {
			// Snow & Ice
			public static readonly ResourceProto.ID SnowPile = newId("SnowPile");
			public static readonly ResourceProto.ID IceBlock = newId("IceBlock");
			public static readonly ResourceProto.ID Icicles = newId("Icicles");

			// Desert
			public static readonly ResourceProto.ID Oasis = newId("Oasis");
			public static readonly ResourceProto.ID CactusPlant = newId("CactusPlant");
			public static readonly ResourceProto.ID DesertRuins = newId("DesertRuins");

			// Volcanic
			public static readonly ResourceProto.ID LavaRock = newId("LavaRock");
			public static readonly ResourceProto.ID GeyserVent = newId("GeyserVent");
			public static readonly ResourceProto.ID AshPile = newId("AshPile");

			// Water
			public static readonly ResourceProto.ID FreshwaterSpring = newId("FreshwaterSpring");
			public static readonly ResourceProto.ID Waterfall = newId("Waterfall");
			public static readonly ResourceProto.ID MineralHotSpring = newId("MineralHotSpring");

			// Misc
			public static readonly ResourceProto.ID AbandonedCampfire = newId("AbandonedCampfire");
			public static readonly ResourceProto.ID FallenLog = newId("FallenLog");
			public static readonly ResourceProto.ID GraveSite = newId("GraveSite");
			public static readonly ResourceProto.ID CobwebCorner = newId("CobwebCorner");
			public static readonly ResourceProto.ID Anthill = newId("Anthill");
		}

		private static ResourceProto.ID newId(string name) {
			return new ResourceProto.ID($"Resource_{name}");
		}
	}
}

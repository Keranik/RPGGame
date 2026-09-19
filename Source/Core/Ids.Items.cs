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
/// Partial class containing  I t e m s.Trim() IDs.
/// Fill in the corresponding nested static classes here.
/// </summary>
public static partial class Ids
{
    // ═══════════════════════════════════════════════════════════════════════
	// ITEMS (Consumables, Materials, Accessories, Quest)
	// ═══════════════════════════════════════════════════════════════════════

	public static class Items {
		public static class Consumables {
			public static readonly ItemProto.ID HealthPotionSmall = newId("HealthPotionSmall");
			public static readonly ItemProto.ID HealthPotionMedium = newId("HealthPotionMedium");
			public static readonly ItemProto.ID HealthPotionLarge = newId("HealthPotionLarge");
			public static readonly ItemProto.ID ManaPotionSmall = newId("ManaPotionSmall");
			public static readonly ItemProto.ID ManaPotionMedium = newId("ManaPotionMedium");
			public static readonly ItemProto.ID ManaPotionLarge = newId("ManaPotionLarge");
			public static readonly ItemProto.ID Antidote = newId("Antidote");
			public static readonly ItemProto.ID Rations = newId("Rations");
			public static readonly ItemProto.ID CookedMeat = newId("CookedMeat");
			public static readonly ItemProto.ID Bandage = newId("Bandage");
			public static readonly ItemProto.ID Torch = newId("Torch");
			public static readonly ItemProto.ID CampingKit = newId("CampingKit");
			public static readonly ItemProto.ID Lockpicks = newId("Lockpicks");
		}

		public static class Materials {
			// ═══════════════════════════════════════════════════════════════
			// ORES & METALS
			// ═══════════════════════════════════════════════════════════════
			public static readonly ItemProto.ID IronOre = newId("IronOre");
			public static readonly ItemProto.ID CopperOre = newId("CopperOre");
			public static readonly ItemProto.ID SilverOre = newId("SilverOre");
			public static readonly ItemProto.ID GoldOre = newId("GoldOre");
			public static readonly ItemProto.ID MithrilOre = newId("MithrilOre");
			public static readonly ItemProto.ID StarMetal = newId("StarMetal");

			// ═══════════════════════════════════════════════════════════════
			// STONE & MINERALS
			// ═══════════════════════════════════════════════════════════════
			public static readonly ItemProto.ID Stone = newId("Stone");
			public static readonly ItemProto.ID Sand = newId("Sand");
			public static readonly ItemProto.ID Clay = newId("Clay");
			public static readonly ItemProto.ID Coal = newId("Coal");
			public static readonly ItemProto.ID Sulfite = newId("Sulfite");
			public static readonly ItemProto.ID Obsidian = newId("Obsidian");
			public static readonly ItemProto.ID Basalt = newId("Basalt");
			public static readonly ItemProto.ID MagicCrystal = newId("MagicCrystal");
			public static readonly ItemProto.ID CrystalShard = newId("CrystalShard");
			public static readonly ItemProto.ID FireCrystal = newId("FireCrystal");
			public static readonly ItemProto.ID CosmicDust = newId("CosmicDust");

			// ═══════════════════════════════════════════════════════════════
			// GEMS
			// ═══════════════════════════════════════════════════════════════
			public static readonly ItemProto.ID GemRuby = newId("GemRuby");
			public static readonly ItemProto.ID GemSapphire = newId("GemSapphire");
			public static readonly ItemProto.ID GemEmerald = newId("GemEmerald");
			public static readonly ItemProto.ID GemDiamond = newId("GemDiamond");
			public static readonly ItemProto.ID GemMoonstone = newId("GemMoonstone");
			public static readonly ItemProto.ID Pearl = newId("Pearl");
			public static readonly ItemProto.ID BlackPearl = newId("BlackPearl");

			// ═══════════════════════════════════════════════════════════════
			// WOOD & BARK
			// ═══════════════════════════════════════════════════════════════
			public static readonly ItemProto.ID Wood = newId("Wood");
			public static readonly ItemProto.ID OakWood = newId("OakWood");
			public static readonly ItemProto.ID PineWood = newId("PineWood");
			public static readonly ItemProto.ID BirchWood = newId("BirchWood");
			public static readonly ItemProto.ID WillowWood = newId("WillowWood");
			public static readonly ItemProto.ID ElderWood = newId("ElderWood");
			public static readonly ItemProto.ID IronwoodLog = newId("IronwoodLog");
			public static readonly ItemProto.ID CorruptedWood = newId("CorruptedWood");
			public static readonly ItemProto.ID FrozenWood = newId("FrozenWood");
			public static readonly ItemProto.ID Firewood = newId("Firewood");
			public static readonly ItemProto.ID RottenWood = newId("RottenWood");
			public static readonly ItemProto.ID Driftwood = newId("Driftwood");
			public static readonly ItemProto.ID Bark = newId("Bark");
			public static readonly ItemProto.ID BirchBark = newId("BirchBark");
			public static readonly ItemProto.ID WillowBark = newId("WillowBark");
			public static readonly ItemProto.ID DriedBark = newId("DriedBark");
			public static readonly ItemProto.ID PineResin = newId("PineResin");
			public static readonly ItemProto.ID Acorn = newId("Acorn");
			public static readonly ItemProto.ID Pinecone = newId("Pinecone");
			public static readonly ItemProto.ID Charcoal = newId("Charcoal");

			// ═══════════════════════════════════════════════════════════════
			// HERBS & PLANTS
			// ═══════════════════════════════════════════════════════════════
			public static readonly ItemProto.ID Herbs = newId("Herbs");
			public static readonly ItemProto.ID RareHerbs = newId("RareHerbs");
			public static readonly ItemProto.ID Wildflowers = newId("Wildflowers");
			public static readonly ItemProto.ID Moonbloom = newId("Moonbloom");
			public static readonly ItemProto.ID Nightshade = newId("Nightshade");
			public static readonly ItemProto.ID GlowMoss = newId("GlowMoss");
			public static readonly ItemProto.ID MandrakeRoot = newId("MandrakeRoot");
			public static readonly ItemProto.ID SwampMoss = newId("SwampMoss");
			public static readonly ItemProto.ID FrostLichen = newId("FrostLichen");
			public static readonly ItemProto.ID FireBloom = newId("FireBloom");
			public static readonly ItemProto.ID Elderberries = newId("Elderberries");
			public static readonly ItemProto.ID ElderFlowers = newId("ElderFlowers");
			public static readonly ItemProto.ID DesertFlower = newId("DesertFlower");
			public static readonly ItemProto.ID CactusFlesh = newId("CactusFlesh");
			public static readonly ItemProto.ID CactusNeedle = newId("CactusNeedle");
			public static readonly ItemProto.ID CactusFlower = newId("CactusFlower");

			// ═══════════════════════════════════════════════════════════════
			// FOOD & FORAGING
			// ═══════════════════════════════════════════════════════════════
			public static readonly ItemProto.ID Berries = newId("Berries");
			public static readonly ItemProto.ID RareBerries = newId("RareBerries");
			public static readonly ItemProto.ID FrozenBerries = newId("FrozenBerries");
			public static readonly ItemProto.ID Mushrooms = newId("Mushrooms");
			public static readonly ItemProto.ID GlowingMushroom = newId("GlowingMushroom");
			public static readonly ItemProto.ID PoisonousMushroom = newId("PoisonousMushroom");
			public static readonly ItemProto.ID Apples = newId("Apples");
			public static readonly ItemProto.ID Nuts = newId("Nuts");
			public static readonly ItemProto.ID Grapes = newId("Grapes");
			public static readonly ItemProto.ID WildVegetables = newId("WildVegetables");
			public static readonly ItemProto.ID WildOnions = newId("WildOnions");
			public static readonly ItemProto.ID Truffle = newId("Truffle");
			public static readonly ItemProto.ID DateFruit = newId("DateFruit");
			public static readonly ItemProto.ID Honey = newId("Honey");
			public static readonly ItemProto.ID Beeswax = newId("Beeswax");
			public static readonly ItemProto.ID Eggs = newId("Eggs");

			// ═══════════════════════════════════════════════════════════════
			// FISH & AQUATIC
			// ═══════════════════════════════════════════════════════════════
			public static readonly ItemProto.ID FreshFish = newId("FreshFish");
			public static readonly ItemProto.ID RareFish = newId("RareFish");
			public static readonly ItemProto.ID LakeFish = newId("LakeFish");
			public static readonly ItemProto.ID SwampFish = newId("SwampFish");
			public static readonly ItemProto.ID GiantFish = newId("GiantFish");
			public static readonly ItemProto.ID MagicFish = newId("MagicFish");
			public static readonly ItemProto.ID IceFish = newId("IceFish");
			public static readonly ItemProto.ID FrozenFish = newId("FrozenFish");
			public static readonly ItemProto.ID MagmaFish = newId("MagmaFish");
			public static readonly ItemProto.ID SwampEel = newId("SwampEel");
			public static readonly ItemProto.ID Tadpole = newId("Tadpole");
			public static readonly ItemProto.ID FrogLegs = newId("FrogLegs");
			public static readonly ItemProto.ID Leech = newId("Leech");
			public static readonly ItemProto.ID Seashells = newId("Seashells");
			public static readonly ItemProto.ID Clams = newId("Clams");
			public static readonly ItemProto.ID Oysters = newId("Oysters");
			public static readonly ItemProto.ID Barnacles = newId("Barnacles");
			public static readonly ItemProto.ID OldBoot = newId("OldBoot");

			// ═══════════════════════════════════════════════════════════════
			// HUNTING - MEAT
			// ═══════════════════════════════════════════════════════════════
			public static readonly ItemProto.ID RabbitMeat = newId("RabbitMeat");
			public static readonly ItemProto.ID Venison = newId("Venison");
			public static readonly ItemProto.ID BoarMeat = newId("BoarMeat");
			public static readonly ItemProto.ID WolfMeat = newId("WolfMeat");
			public static readonly ItemProto.ID BearMeat = newId("BearMeat");
			public static readonly ItemProto.ID FoxMeat = newId("FoxMeat");

			// ═══════════════════════════════════════════════════════════════
			// HUNTING - PELTS & HIDES
			// ═══════════════════════════════════════════════════════════════
			public static readonly ItemProto.ID RabbitPelt = newId("RabbitPelt");
			public static readonly ItemProto.ID RabbitFoot = newId("RabbitFoot");
			public static readonly ItemProto.ID DeerHide = newId("DeerHide");
			public static readonly ItemProto.ID Antlers = newId("Antlers");
			public static readonly ItemProto.ID BoarHide = newId("BoarHide");
			public static readonly ItemProto.ID BoarTusk = newId("BoarTusk");
			public static readonly ItemProto.ID WolfPelt = newId("WolfPelt");
			public static readonly ItemProto.ID DireWolfPelt = newId("DireWolfPelt");
			public static readonly ItemProto.ID WolfFang = newId("WolfFang");
			public static readonly ItemProto.ID BearPelt = newId("BearPelt");
			public static readonly ItemProto.ID BearClaw = newId("BearClaw");
			public static readonly ItemProto.ID FoxPelt = newId("FoxPelt");
			public static readonly ItemProto.ID RatTail = newId("RatTail");

			// ═══════════════════════════════════════════════════════════════
			// HUNTING - FEATHERS & MISC
			// ═══════════════════════════════════════════════════════════════
			public static readonly ItemProto.ID Feathers = newId("Feathers");
			public static readonly ItemProto.ID OwlFeather = newId("OwlFeather");
			public static readonly ItemProto.ID OwlPellet = newId("OwlPellet");

			// ═══════════════════════════════════════════════════════════════
			// MONSTER PARTS
			// ═══════════════════════════════════════════════════════════════
			public static readonly ItemProto.ID SpiderSilk = newId("SpiderSilk");
			public static readonly ItemProto.ID SpiderEgg = newId("SpiderEgg");
			public static readonly ItemProto.ID VenomSac = newId("VenomSac");
			public static readonly ItemProto.ID WyvernScale = newId("WyvernScale");
			public static readonly ItemProto.ID WyvernEgg = newId("WyvernEgg");
			public static readonly ItemProto.ID WyvernWing = newId("WyvernWing");
			public static readonly ItemProto.ID FireScale = newId("FireScale");
			public static readonly ItemProto.ID DragonBone = newId("DragonBone");
			public static readonly ItemProto.ID DragonScale = newId("DragonScale");
			public static readonly ItemProto.ID DragonTooth = newId("DragonTooth");

			// ═══════════════════════════════════════════════════════════════
			// CREATURE PARTS
			// ═══════════════════════════════════════════════════════════════
			public static readonly ItemProto.ID Bone = newId("Bone");
			public static readonly ItemProto.ID Skull = newId("Skull");
			public static readonly ItemProto.ID Ectoplasm = newId("Ectoplasm");
			public static readonly ItemProto.ID ShadowEssence = newId("ShadowEssence");
			public static readonly ItemProto.ID SoulFragment = newId("SoulFragment");
			public static readonly ItemProto.ID DemonHorn = newId("DemonHorn");
			public static readonly ItemProto.ID DemonEssence = newId("DemonEssence");
			public static readonly ItemProto.ID HellhoundFang = newId("HellhoundFang");
			public static readonly ItemProto.ID InfernalAsh = newId("InfernalAsh");
			public static readonly ItemProto.ID Grubs = newId("Grubs");
			public static readonly ItemProto.ID Beetle = newId("Beetle");
			public static readonly ItemProto.ID Ants = newId("Ants");
			public static readonly ItemProto.ID AntEggs = newId("AntEggs");

			// ═══════════════════════════════════════════════════════════════
			// MAGICAL ESSENCES
			// ═══════════════════════════════════════════════════════════════
			public static readonly ItemProto.ID FogEssence = newId("FogEssence");
			public static readonly ItemProto.ID TimeShard = newId("TimeShard");
			public static readonly ItemProto.ID ManaEssence = newId("ManaEssence");
			public static readonly ItemProto.ID EternalIce = newId("EternalIce");

			// ═══════════════════════════════════════════════════════════════
			// CLOTH & LEATHER
			// ═══════════════════════════════════════════════════════════════
			public static readonly ItemProto.ID LeatherScraps = newId("LeatherScraps");
			public static readonly ItemProto.ID Leather = newId("Leather");
			public static readonly ItemProto.ID Cloth = newId("Cloth");
			public static readonly ItemProto.ID Rope = newId("Rope");

			// ═══════════════════════════════════════════════════════════════
			// ENVIRONMENTAL
			// ═══════════════════════════════════════════════════════════════
			public static readonly ItemProto.ID Snow = newId("Snow");
			public static readonly ItemProto.ID Ice = newId("Ice");
			public static readonly ItemProto.ID FreshWater = newId("FreshWater");
			public static readonly ItemProto.ID MineralWater = newId("MineralWater");
			public static readonly ItemProto.ID VolcanicAsh = newId("VolcanicAsh");
			public static readonly ItemProto.ID SulfiteDeposit = newId("SulfiteDeposit");
			public static readonly ItemProto.ID MineralSalt = newId("MineralSalt");
			public static readonly ItemProto.ID RiverStone = newId("RiverStone");
			public static readonly ItemProto.ID GoldNugget = newId("GoldNugget");

			// ═══════════════════════════════════════════════════════════════
			// SACRED & ANCIENT
			// ═══════════════════════════════════════════════════════════════
			public static readonly ItemProto.ID SacredIncense = newId("SacredIncense");
			public static readonly ItemProto.ID HolyWater = newId("HolyWater");
			public static readonly ItemProto.ID AncientRelic = newId("AncientRelic");
			public static readonly ItemProto.ID AncientPottery = newId("AncientPottery");
			public static readonly ItemProto.ID AncientScroll = newId("AncientScroll");
			public static readonly ItemProto.ID GoldCoins = newId("GoldCoins");
		}

		public static class Accessories {
			public static readonly ItemProto.ID RingOfProtection = newId("RingOfProtection");
			public static readonly ItemProto.ID RingOfStrength = newId("RingOfStrength");
			public static readonly ItemProto.ID AmuletOfHealth = newId("AmuletOfHealth");
			public static readonly ItemProto.ID AmuletOfWisdom = newId("AmuletOfWisdom");
			public static readonly ItemProto.ID HolySymbol = newId("HolySymbol");
			public static readonly ItemProto.ID Quiver = newId("Quiver");
			public static readonly ItemProto.ID Spellbook = newId("Spellbook");
		}

		public static class Quest {
			public static readonly ItemProto.ID DarkTomePage = newId("DarkTomePage");
			public static readonly ItemProto.ID BanditKingCrown = newId("BanditKingCrown");
			public static readonly ItemProto.ID BanditKingKey = newId("BanditKingKey");
			public static readonly ItemProto.ID BanditLetters = newId("BanditLetters");
			public static readonly ItemProto.ID LichPhylacteryShard = newId("LichPhylacteryShard");
			public static readonly ItemProto.ID NecronomiconPage = newId("NecronomiconPage");
			public static readonly ItemProto.ID AncientRelic = newId("AncientRelic");
			public static readonly ItemProto.ID FogClue = newId("FogClue");
		}

		private static ItemProto.ID newId(string name) {
			return new ItemProto.ID($"Item_{name}");
		}
	}
}

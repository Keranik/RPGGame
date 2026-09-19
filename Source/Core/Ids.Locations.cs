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
/// Partial class containing  L o c a t i o n s.Trim() IDs.
/// Fill in the corresponding nested static classes here.
/// </summary>
public static partial class Ids
{
    // ═══════════════════════════════════════════════════════════════════════
	// LOCATIONS
	// ═══════════════════════════════════════════════════════════════════════

	public static class Locations {
		// ═══════════════════════════════════════════════════════════════
		// SETTLEMENTS
		// ═══════════════════════════════════════════════════════════════

		public static class Settlements {
			public static readonly SettlementProto.ID HavenVillage = newSettlement("HavenVillage");
			public static readonly SettlementProto.ID WaystationInn = newSettlement("WaystationInn");
			public static readonly SettlementProto.ID FrontierOutpost = newSettlement("FrontierOutpost");
			public static readonly SettlementProto.ID FogwatchTower = newSettlement("FogwatchTower");

			private static SettlementProto.ID newSettlement(string name) =>
				new SettlementProto.ID($"Location_Settlement_{name}");
		}

		// ═══════════════════════════════════════════════════════════════
		// LANDMARKS
		// ═══════════════════════════════════════════════════════════════

		public static class Landmarks {
			// Natural
			public static readonly LandmarkProto.ID AncientOak = newLandmark("AncientOak");
			public static readonly LandmarkProto.ID StoneCircle = newLandmark("StoneCircle");
			public static readonly LandmarkProto.ID CrystalCave = newLandmark("CrystalCave");
			public static readonly LandmarkProto.ID TwistedTree = newLandmark("TwistedTree");
			public static readonly LandmarkProto.ID MistyWaterfall = newLandmark("MistyWaterfall");
			public static readonly LandmarkProto.ID HotSpringPool = newLandmark("HotSpringPool");
			public static readonly LandmarkProto.ID GiantMushroom = newLandmark("GiantMushroom");

			// Ruins
			public static readonly LandmarkProto.ID RuinedShrine = newLandmark("RuinedShrine");
			public static readonly LandmarkProto.ID FallenStatue = newLandmark("FallenStatue");
			public static readonly LandmarkProto.ID OldBattlefield = newLandmark("OldBattlefield");
			public static readonly LandmarkProto.ID AncientGrave = newLandmark("AncientGrave");
			public static readonly LandmarkProto.ID BrokenArch = newLandmark("BrokenArch");

			// Man-made
			public static readonly LandmarkProto.ID Crossroads = newLandmark("Crossroads");
			public static readonly LandmarkProto.ID WayShrine = newLandmark("WayShrine");
			public static readonly LandmarkProto.ID AbandonedWell = newLandmark("AbandonedWell");
			public static readonly LandmarkProto.ID HermitHut = newLandmark("HermitHut");
			public static readonly LandmarkProto.ID WatchTowerRuins = newLandmark("WatchTowerRuins");

			// Magical
			public static readonly LandmarkProto.ID ManaWell = newLandmark("ManaWell");
			public static readonly LandmarkProto.ID LeyLineNode = newLandmark("LeyLineNode");
			public static readonly LandmarkProto.ID TemporalRift = newLandmark("TemporalRift");
			public static readonly LandmarkProto.ID FogMonument = newLandmark("FogMonument");
			public static readonly LandmarkProto.ID ShadowTear = newLandmark("ShadowTear");

			// Special
			public static readonly LandmarkProto.ID FogBoundary = newLandmark("FogBoundary");
			public static readonly LandmarkProto.ID FogHeart = newLandmark("FogHeart");

			private static LandmarkProto.ID newLandmark(string name) =>
				new LandmarkProto.ID($"Location_Landmark_{name}");
		}

		// ═══════════════════════════════════════════════════════════════
		// DUNGEONS
		// ═══════════════════════════════════════════════════════════════

		public static class Dungeons {
			// Intro
			public static readonly DungeonProto.ID OvergrownRuins = newDungeon("OvergrownRuins");

			// Caves
			public static readonly DungeonProto.ID SpiderDen = newDungeon("SpiderDen");
			public static readonly DungeonProto.ID WolfCave = newDungeon("WolfCave");
			public static readonly DungeonProto.ID BearCave = newDungeon("BearCave");
			public static readonly DungeonProto.ID GoblinCave = newDungeon("GoblinCave");
			public static readonly DungeonProto.ID CrystalCavern = newDungeon("CrystalCavern");

			// Mines
			public static readonly DungeonProto.ID AbandonedMine = newDungeon("AbandonedMine");
			public static readonly DungeonProto.ID CollapsingMine = newDungeon("CollapsingMine");
			public static readonly DungeonProto.ID DeepMine = newDungeon("DeepMine");

			// Crypts
			public static readonly DungeonProto.ID ForgottenCrypt = newDungeon("ForgottenCrypt");
			public static readonly DungeonProto.ID AncientCatacombs = newDungeon("AncientCatacombs");
			public static readonly DungeonProto.ID CursedMausoleum = newDungeon("CursedMausoleum");

			// Lairs
			public static readonly DungeonProto.ID BanditHideout = newDungeon("BanditHideout");
			public static readonly DungeonProto.ID CultistLair = newDungeon("CultistLair");
			public static readonly DungeonProto.ID OrcStronghold = newDungeon("OrcStronghold");

			// Ruins
			public static readonly DungeonProto.ID AncientRuins = newDungeon("AncientRuins");
			public static readonly DungeonProto.ID CursedTemple = newDungeon("CursedTemple");
			public static readonly DungeonProto.ID AbandonedTower = newDungeon("AbandonedTower");
			public static readonly DungeonProto.ID SunkenLibrary = newDungeon("SunkenLibrary");

			// Story
			public static readonly DungeonProto.ID BanditFortress = newDungeon("BanditFortress");
			public static readonly DungeonProto.ID LichSanctum = newDungeon("LichSanctum");
			public static readonly DungeonProto.ID FogNexus = newDungeon("FogNexus");

			private static DungeonProto.ID newDungeon(string name) =>
				new DungeonProto.ID($"Location_Dungeon_{name}");
		}

		// ═══════════════════════════════════════════════════════════════
		// CAMPS
		// ═══════════════════════════════════════════════════════════════

		public static class Camps {
			public static readonly CampProto.ID ForestClearing = newCamp("ForestClearing");
			public static readonly CampProto.ID RockOverhang = newCamp("RockOverhang");
			public static readonly CampProto.ID AbandonedCamp = newCamp("AbandonedCamp");
			public static readonly CampProto.ID HuntersCamp = newCamp("HuntersCamp");
			public static readonly CampProto.ID RuinedCottage = newCamp("RuinedCottage");
			public static readonly CampProto.ID CaveShelter = newCamp("CaveShelter");
			public static readonly CampProto.ID Waystation = newCamp("Waystation");

			private static CampProto.ID newCamp(string name) =>
				new CampProto.ID($"Location_Camp_{name}");
		}

		// ═══════════════════════════════════════════════════════════════
		// BOSS ARENAS
		// ═══════════════════════════════════════════════════════════════

		public static class BossArenas {
			public static readonly BossArenaProto.ID BanditKingThrone = newArena("BanditKingThrone");
			public static readonly BossArenaProto.ID LichChamber = newArena("LichChamber");
			public static readonly BossArenaProto.ID FogHeraldDomain = newArena("FogHeraldDomain");
			public static readonly BossArenaProto.ID DemonGate = newArena("DemonGate");
			public static readonly BossArenaProto.ID DragonLair = newArena("DragonLair");

			private static BossArenaProto.ID newArena(string name) =>
				new BossArenaProto.ID($"Location_BossArena_{name}");
		}
	}
}

using RPGGame.Core.Prototypes.Expedition;

namespace RPGGame.Core;

public static partial class Ids {
	// ═══════════════════════════════════════════════════════════════════════
	// TERRAINS
	// ═══════════════════════════════════════════════════════════════════════

	public static class Terrains {
		// ═══════════════════════════════════════════════════════════════
		// ROADS & PATHS
		// ═══════════════════════════════════════════════════════════════

		public static class Roads {
			public static readonly TerrainProto.ID Road = n("Road");
			public static readonly TerrainProto.ID Path = n("Path");
			public static readonly TerrainProto.ID Trail = n("Trail");
			public static readonly TerrainProto.ID Bridge = n("Bridge");

			private static TerrainProto.ID n(string name) => new($"Terrain_Road_{name}");
		}

		// ═══════════════════════════════════════════════════════════════
		// GRASSLANDS & PLAINS
		// ═══════════════════════════════════════════════════════════════

		public static class Plains {
			public static readonly TerrainProto.ID Grass = n("Grass");
			public static readonly TerrainProto.ID Hills = n("Hills");
			public static readonly TerrainProto.ID OpenPlains = n("OpenPlains");
			public static readonly TerrainProto.ID Meadow = n("Meadow");
			public static readonly TerrainProto.ID Farmland = n("Farmland");

			private static TerrainProto.ID n(string name) => new($"Terrain_Plains_{name}");
		}

		// ═══════════════════════════════════════════════════════════════
		// FORESTS & WOODLANDS
		// ═══════════════════════════════════════════════════════════════

		public static class Forests {
			public static readonly TerrainProto.ID Forest = n("Forest");
			public static readonly TerrainProto.ID DeepForest = n("DeepForest");
			public static readonly TerrainProto.ID AncientForest = n("AncientForest");
			public static readonly TerrainProto.ID DeadForest = n("DeadForest");
			public static readonly TerrainProto.ID Jungle = n("Jungle");
			public static readonly TerrainProto.ID Orchard = n("Orchard");

			private static TerrainProto.ID n(string name) => new($"Terrain_Forest_{name}");
		}

		// ═══════════════════════════════════════════════════════════════
		// MOUNTAINS & HIGHLANDS
		// ═══════════════════════════════════════════════════════════════

		public static class Mountains {
			public static readonly TerrainProto.ID Mountain = n("Mountain");
			public static readonly TerrainProto.ID Peak = n("Peak");
			public static readonly TerrainProto.ID Cliff = n("Cliff");
			public static readonly TerrainProto.ID Pass = n("Pass");
			public static readonly TerrainProto.ID Highland = n("Highland");

			private static TerrainProto.ID n(string name) => new($"Terrain_Mountain_{name}");
		}

		// ═══════════════════════════════════════════════════════════════
		// WATER FEATURES
		// ═══════════════════════════════════════════════════════════════

		public static class Water {
			public static readonly TerrainProto.ID River = n("River");
			public static readonly TerrainProto.ID Lake = n("Lake");
			public static readonly TerrainProto.ID Pond = n("Pond");
			public static readonly TerrainProto.ID Ocean = n("Ocean");
			public static readonly TerrainProto.ID Shallows = n("Shallows");
			public static readonly TerrainProto.ID Waterfall = n("Waterfall");
			public static readonly TerrainProto.ID HotSpring = n("HotSpring");

			private static TerrainProto.ID n(string name) => new($"Terrain_Water_{name}");
		}

		// ═══════════════════════════════════════════════════════════════
		// COASTAL
		// ═══════════════════════════════════════════════════════════════

		public static class Coastal {
			public static readonly TerrainProto.ID Beach = n("Beach");
			public static readonly TerrainProto.ID RockyCoast = n("RockyCoast");
			public static readonly TerrainProto.ID TidalFlats = n("TidalFlats");
			public static readonly TerrainProto.ID Reef = n("Reef");

			private static TerrainProto.ID n(string name) => new($"Terrain_Coastal_{name}");
		}

		// ═══════════════════════════════════════════════════════════════
		// WETLANDS
		// ═══════════════════════════════════════════════════════════════

		public static class Wetlands {
			public static readonly TerrainProto.ID Swamp = n("Swamp");
			public static readonly TerrainProto.ID Marsh = n("Marsh");
			public static readonly TerrainProto.ID Bog = n("Bog");
			public static readonly TerrainProto.ID Mangrove = n("Mangrove");

			private static TerrainProto.ID n(string name) => new($"Terrain_Wetland_{name}");
		}

		// ═══════════════════════════════════════════════════════════════
		// ARID & DESERT
		// ═══════════════════════════════════════════════════════════════

		public static class Desert {
			public static readonly TerrainProto.ID OpenDesert = n("OpenDesert");
			public static readonly TerrainProto.ID Dunes = n("Dunes");
			public static readonly TerrainProto.ID Oasis = n("Oasis");
			public static readonly TerrainProto.ID Badlands = n("Badlands");
			public static readonly TerrainProto.ID SaltFlats = n("SaltFlats");
			public static readonly TerrainProto.ID Canyon = n("Canyon");
			public static readonly TerrainProto.ID Mesa = n("Mesa");

			private static TerrainProto.ID n(string name) => new($"Terrain_Desert_{name}");
		}

		// ═══════════════════════════════════════════════════════════════
		// COLD & FROZEN
		// ═══════════════════════════════════════════════════════════════

		public static class Frozen {
			public static readonly TerrainProto.ID Snow = n("Snow");
			public static readonly TerrainProto.ID Ice = n("Ice");
			public static readonly TerrainProto.ID Glacier = n("Glacier");
			public static readonly TerrainProto.ID FrozenLake = n("FrozenLake");
			public static readonly TerrainProto.ID Permafrost = n("Permafrost");
			public static readonly TerrainProto.ID Blizzard = n("Blizzard");
			public static readonly TerrainProto.ID Tundra = n("Tundra");

			private static TerrainProto.ID n(string name) => new($"Terrain_Frozen_{name}");
		}

		// ═══════════════════════════════════════════════════════════════
		// VOLCANIC & FIRE
		// ═══════════════════════════════════════════════════════════════

		public static class Volcanic {
			public static readonly TerrainProto.ID VolcanicPlain = n("VolcanicPlain");
			public static readonly TerrainProto.ID LavaField = n("LavaField");
			public static readonly TerrainProto.ID AshWastes = n("AshWastes");
			public static readonly TerrainProto.ID Geothermal = n("Geothermal");
			public static readonly TerrainProto.ID ObsidianField = n("ObsidianField");

			private static TerrainProto.ID n(string name) => new($"Terrain_Volcanic_{name}");
		}

		// ═══════════════════════════════════════════════════════════════
		// UNDERGROUND
		// ═══════════════════════════════════════════════════════════════

		public static class Underground {
			public static readonly TerrainProto.ID Cave = n("Cave");
			public static readonly TerrainProto.ID Cavern = n("Cavern");
			public static readonly TerrainProto.ID Tunnel = n("Tunnel");
			public static readonly TerrainProto.ID UndergroundLake = n("UndergroundLake");
			public static readonly TerrainProto.ID CrystalCavern = n("CrystalCavern");
			public static readonly TerrainProto.ID MushroomForest = n("MushroomForest");
			public static readonly TerrainProto.ID LavaTube = n("LavaTube");
			public static readonly TerrainProto.ID Mine = n("Mine");
			public static readonly TerrainProto.ID Catacombs = n("Catacombs");

			private static TerrainProto.ID n(string name) => new($"Terrain_Underground_{name}");
		}

		// ═══════════════════════════════════════════════════════════════
		// SETTLEMENTS & STRUCTURES
		// ═══════════════════════════════════════════════════════════════

		public static class Settlements {
			public static readonly TerrainProto.ID Village = n("Village");
			public static readonly TerrainProto.ID Town = n("Town");
			public static readonly TerrainProto.ID City = n("City");
			public static readonly TerrainProto.ID Outpost = n("Outpost");
			public static readonly TerrainProto.ID Fort = n("Fort");
			public static readonly TerrainProto.ID Castle = n("Castle");
			public static readonly TerrainProto.ID Temple = n("Temple");
			public static readonly TerrainProto.ID Tower = n("Tower");

			private static TerrainProto.ID n(string name) => new($"Terrain_Settlement_{name}");
		}

		// ═══════════════════════════════════════════════════════════════
		// RUINS & ABANDONED
		// ═══════════════════════════════════════════════════════════════

		public static class Ruins {
			public static readonly TerrainProto.ID OpenRuins = n("OpenRuins");
			public static readonly TerrainProto.ID AbandonedVillage = n("AbandonedVillage");
			public static readonly TerrainProto.ID Graveyard = n("Graveyard");
			public static readonly TerrainProto.ID Battlefield = n("Battlefield");
			public static readonly TerrainProto.ID Shipwreck = n("Shipwreck");
			public static readonly TerrainProto.ID Dungeon = n("Dungeon");
			public static readonly TerrainProto.ID Crypt = n("Crypt");
			public static readonly TerrainProto.ID Monument = n("Monument");

			private static TerrainProto.ID n(string name) => new($"Terrain_Ruins_{name}");
		}

		// ═══════════════════════════════════════════════════════════════
		// CORRUPTED & MAGICAL
		// ═══════════════════════════════════════════════════════════════

		public static class Supernatural {
			public static readonly TerrainProto.ID Corrupted = n("Corrupted");
			public static readonly TerrainProto.ID Blighted = n("Blighted");
			public static readonly TerrainProto.ID ShadowRealm = n("ShadowRealm");
			public static readonly TerrainProto.ID Wasteland = n("Wasteland");
			public static readonly TerrainProto.ID CursedGround = n("CursedGround");
			public static readonly TerrainProto.ID Magical = n("Magical");
			public static readonly TerrainProto.ID FeyCrossing = n("FeyCrossing");
			public static readonly TerrainProto.ID LeyLine = n("LeyLine");
			public static readonly TerrainProto.ID VoidTouched = n("VoidTouched");

			private static TerrainProto.ID n(string name) => new($"Terrain_Supernatural_{name}");
		}

		// ═══════════════════════════════════════════════════════════════
		// SPECIAL
		// ═══════════════════════════════════════════════════════════════

		public static class Special {
			public static readonly TerrainProto.ID Fog = n("Fog");
			public static readonly TerrainProto.ID Blocked = n("Blocked");
			public static readonly TerrainProto.ID Portal = n("Portal");
			public static readonly TerrainProto.ID Sanctuary = n("Sanctuary");
			public static readonly TerrainProto.ID Arena = n("Arena");
			public static readonly TerrainProto.ID Crossroads = n("Crossroads");
			public static readonly TerrainProto.ID Campsite = n("Campsite");
			public static readonly TerrainProto.ID Wayshrine = n("Wayshrine");

			private static TerrainProto.ID n(string name) => new($"Terrain_Special_{name}");
		}
	}
}
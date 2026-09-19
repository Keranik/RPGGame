using RPGGame.Core.Prototypes.Expedition;

namespace RPGGame.Core;

public static partial class Ids {
	// ═══════════════════════════════════════════════════════════════════════
	// BIOMES
	// ═══════════════════════════════════════════════════════════════════════

	public static class Biomes {
		// ═══════════════════════════════════════════════════════════════
		// EARLY GAME (Progression 0-2)
		// Safe, familiar territory near the village
		// ═══════════════════════════════════════════════════════════════

		/// <summary>The immediate area around Haven Village - safe roads and farms.</summary>
		public static readonly BiomeProto.ID Outskirts = n("Outskirts");

		/// <summary>Cultivated farmlands with occasional bandits.</summary>
		public static readonly BiomeProto.ID Farmlands = n("Farmlands");

		/// <summary>Open grasslands with gentle rolling terrain.</summary>
		public static readonly BiomeProto.ID Grasslands = n("Grasslands");

		/// <summary>Light deciduous forest - first taste of wilderness.</summary>
		public static readonly BiomeProto.ID LightWoods = n("LightWoods");

		// ═══════════════════════════════════════════════════════════════
		// MID GAME (Progression 3-5)
		// Increasing danger, diverse environments
		// ═══════════════════════════════════════════════════════════════

		/// <summary>Dense, ancient forest where sunlight barely penetrates.</summary>
		public static readonly BiomeProto.ID DeepForest = n("DeepForest");

		/// <summary>Treacherous swamps and marshes - disease and undead.</summary>
		public static readonly BiomeProto.ID Wetlands = n("Wetlands");

		/// <summary>Hilly terrain with caves and orc patrols.</summary>
		public static readonly BiomeProto.ID RollingHills = n("RollingHills");

		/// <summary>River valleys with bridges and ambush points.</summary>
		public static readonly BiomeProto.ID RiverValley = n("RiverValley");

		/// <summary>Primeval forest touched by ancient magic - fey creatures.</summary>
		public static readonly BiomeProto.ID AncientWoods = n("AncientWoods");

		// ═══════════════════════════════════════════════════════════════
		// LATE GAME (Progression 6-8)
		// Harsh conditions, powerful enemies
		// ═══════════════════════════════════════════════════════════════

		/// <summary>Rugged mountain terrain with harsh conditions.</summary>
		public static readonly BiomeProto.ID Mountains = n("Mountains");

		/// <summary>Elevated plateau with strong winds and exposure.</summary>
		public static readonly BiomeProto.ID Highlands = n("Highlands");

		/// <summary>Cursed lands filled with undead and dark magic.</summary>
		public static readonly BiomeProto.ID HauntedLands = n("HauntedLands");

		/// <summary>Barren wasteland with scorched earth and desperate creatures.</summary>
		public static readonly BiomeProto.ID Badlands = n("Badlands");

		/// <summary>Corrupted swampland near the fog's edge.</summary>
		public static readonly BiomeProto.ID DarkMarsh = n("DarkMarsh");

		// ═══════════════════════════════════════════════════════════════
		// END GAME (Progression 9+)
		// The fog's domain - maximum danger
		// ═══════════════════════════════════════════════════════════════

		/// <summary>Lands twisted by the fog's corruption.</summary>
		public static readonly BiomeProto.ID CorruptedLands = n("CorruptedLands");

		/// <summary>The boundary where normal reality meets the fog.</summary>
		public static readonly BiomeProto.ID FogBorder = n("FogBorder");

		/// <summary>Reality-warped wastes within the fog itself.</summary>
		public static readonly BiomeProto.ID VoidWastes = n("VoidWastes");

		/// <summary>The source of the fog - final destination.</summary>
		public static readonly BiomeProto.ID FogHeart = n("FogHeart");

		// ═══════════════════════════════════════════════════════════════
		// BRANCH-ONLY BIOMES (Progression = -1)
		// Special biomes that only appear in side paths
		// ═══════════════════════════════════════════════════════════════

		/// <summary>Magical underground caverns filled with crystals.</summary>
		public static readonly BiomeProto.ID CrystalCaverns = n("CrystalCaverns");

		/// <summary>Volcanic fissure with extreme heat and rare ores.</summary>
		public static readonly BiomeProto.ID VolcanicRift = n("VolcanicRift");

		/// <summary>Frozen tundra with blizzards and ice creatures.</summary>
		public static readonly BiomeProto.ID FrozenWastes = n("FrozenWastes");

		/// <summary>Fey-touched woodland with strange magic.</summary>
		public static readonly BiomeProto.ID FeyWilds = n("FeyWilds");

		/// <summary>Abandoned mine complex with ore and dangers.</summary>
		public static readonly BiomeProto.ID DeepMines = n("DeepMines");

		/// <summary>Ancient catacombs filled with undead.</summary>
		public static readonly BiomeProto.ID Catacombs = n("Catacombs");

		private static BiomeProto.ID n(string name) => new($"Biome_{name}");
	}
}
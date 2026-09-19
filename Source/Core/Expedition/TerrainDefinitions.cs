using RPGGame.Core.Prototypes;
using RPGGame.Core.Prototypes.Expedition;

namespace RPGGame.Core.Expedition;

/// <summary>
/// Registers all terrain prototypes with gameplay-relevant data.
/// Replaces hardcoded TerrainType enum behavior with data-driven definitions.
/// </summary>
public class TerrainDefinitions : ICoreData {
	public void GameData(GameDb db) {
		RegisterRoads(db);
		RegisterPlains(db);
		RegisterForests(db);
		RegisterMountains(db);
		RegisterWater(db);
		RegisterCoastal(db);
		RegisterWetlands(db);
		RegisterDesert(db);
		RegisterFrozen(db);
		RegisterVolcanic(db);
		RegisterUnderground(db);
		RegisterSettlements(db);
		RegisterRuins(db);
		RegisterSupernatural(db);
		RegisterSpecial(db);
	}

	#region Roads & Paths

	private void RegisterRoads(GameDb db) {
		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Roads.Road,
			name: "Road",
			description: "A well-maintained road allowing swift travel.",
			movementSpeed: 1.4f,
			staminaDrain: 0.6f,
			fatigueRate: 0.7f
		) {
			Category = TerrainCategory.Constructed,
			BaseDifficulty = 0.1f,
			EncounterChanceMultiplier = 0.7f,
			AmbushChanceMultiplier = 0.3f,
			VisionRangeMultiplier = 1.3f,
			MapColor = "#8B7355",
			DisplayOrder = 10
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Roads.Path,
			name: "Path",
			description: "A dirt path through the wilderness.",
			movementSpeed: 1.2f,
			staminaDrain: 0.8f,
			fatigueRate: 0.85f
		) {
			Category = TerrainCategory.Constructed,
			BaseDifficulty = 0.15f,
			EncounterChanceMultiplier = 0.85f,
			AmbushChanceMultiplier = 0.5f,
			VisionRangeMultiplier = 1.1f,
			MapColor = "#A0826D",
			DisplayOrder = 11
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Roads.Trail,
			name: "Trail",
			description: "A narrow trail winding through dense terrain.",
			movementSpeed: 1.0f,
			staminaDrain: 0.9f,
			fatigueRate: 0.95f
		) {
			Category = TerrainCategory.Constructed,
			BaseDifficulty = 0.2f,
			EncounterChanceMultiplier = 1.0f,
			AmbushChanceMultiplier = 0.8f,
			VisionRangeMultiplier = 0.9f,
			MapColor = "#B8A090",
			DisplayOrder = 12
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Roads.Bridge,
			name: "Bridge",
			description: "A bridge crossing over water or a chasm.",
			movementSpeed: 1.3f,
			staminaDrain: 0.7f,
			fatigueRate: 0.8f
		) {
			Category = TerrainCategory.Constructed,
			BaseDifficulty = 0.1f,
			EncounterChanceMultiplier = 0.6f,
			AmbushChanceMultiplier = 1.2f, // Choke point - dangerous!
			VisionRangeMultiplier = 1.2f,
			MapColor = "#6B4423",
			DisplayOrder = 13
		});
	}

	#endregion

	#region Plains & Grasslands

	private void RegisterPlains(GameDb db) {
		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Plains.Grass,
			name: "Grassland",
			description: "Open grassland with gentle terrain.",
			movementSpeed: 1.1f,
			staminaDrain: 0.85f,
			fatigueRate: 0.9f
		) {
			Category = TerrainCategory.Natural,
			BaseDifficulty = 0.2f,
			EncounterChanceMultiplier = 0.9f,
			AmbushChanceMultiplier = 0.4f,
			VisionRangeMultiplier = 1.4f,
			ForagingChanceMultiplier = 1.1f,
			HuntingSuccessMultiplier = 1.0f,
			MoraleModifierPerHour = 0.5f,
			WaterAvailability = 0.4f,
			ResourceCategories = [TerrainResourceCategory.Herbs, TerrainResourceCategory.Forage, TerrainResourceCategory.Game],
			Tags = [Ids.Tags.Region.Grassland],
			MapColor = "#7CBA5F",
			DisplayOrder = 20
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Plains.Hills,
			name: "Hills",
			description: "Rolling hills that slow travel but offer good visibility.",
			movementSpeed: 0.85f,
			staminaDrain: 1.2f,
			fatigueRate: 1.15f
		) {
			Category = TerrainCategory.Natural,
			BaseDifficulty = 0.35f,
			EncounterChanceMultiplier = 1.0f,
			AmbushChanceMultiplier = 0.6f,
			VisionRangeMultiplier = 1.5f,
			ForagingChanceMultiplier = 0.9f,
			HuntingSuccessMultiplier = 1.1f,
			WaterAvailability = 0.3f,
			ResourceCategories = [TerrainResourceCategory.Herbs, TerrainResourceCategory.Ore, TerrainResourceCategory.Game],
			Tags = [Ids.Tags.Region.Hills],
			MapColor = "#8B9C4E",
			DisplayOrder = 21
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Plains.OpenPlains,
			name: "Plains",
			description: "Wide open plains with fast travel but little cover.",
			movementSpeed: 1.2f,
			staminaDrain: 0.8f,
			fatigueRate: 0.85f
		) {
			Category = TerrainCategory.Natural,
			BaseDifficulty = 0.15f,
			EncounterChanceMultiplier = 0.8f,
			AmbushChanceMultiplier = 0.2f,
			VisionRangeMultiplier = 1.6f,
			ForagingChanceMultiplier = 0.8f,
			HuntingSuccessMultiplier = 0.9f,
			WaterAvailability = 0.3f,
			ResourceCategories = [TerrainResourceCategory.Game, TerrainResourceCategory.Herbs],
			Tags = [Ids.Tags.Region.Plains],
			MapColor = "#C4B896",
			DisplayOrder = 22
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Plains.Meadow,
			name: "Meadow",
			description: "A beautiful meadow filled with wildflowers.",
			movementSpeed: 1.1f,
			staminaDrain: 0.8f,
			fatigueRate: 0.8f
		) {
			Category = TerrainCategory.Natural,
			BaseDifficulty = 0.15f,
			EncounterChanceMultiplier = 0.7f,
			AmbushChanceMultiplier = 0.5f,
			VisionRangeMultiplier = 1.3f,
			ForagingChanceMultiplier = 1.5f,
			MoraleModifierPerHour = 1.0f,
			RestEffectivenessMultiplier = 1.1f,
			WaterAvailability = 0.5f,
			ResourceCategories = [TerrainResourceCategory.Herbs, TerrainResourceCategory.Forage],
			MapColor = "#90EE90",
			DisplayOrder = 23
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Plains.Farmland,
			name: "Farmland",
			description: "Cultivated fields near settlements.",
			movementSpeed: 1.0f,
			staminaDrain: 0.9f,
			fatigueRate: 0.9f
		) {
			Category = TerrainCategory.Natural,
			BaseDifficulty = 0.1f,
			EncounterChanceMultiplier = 0.5f,
			AmbushChanceMultiplier = 0.3f,
			VisionRangeMultiplier = 1.2f,
			ForagingChanceMultiplier = 1.3f,
			WaterAvailability = 0.7f,
			ResourceCategories = [TerrainResourceCategory.Forage],
			MapColor = "#DAA520",
			DisplayOrder = 24
		});
	}

	#endregion

	#region Forests

	private void RegisterForests(GameDb db) {
		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Forests.Forest,
			name: "Forest",
			description: "Light woodland with dappled sunlight.",
			movementSpeed: 0.9f,
			staminaDrain: 1.0f,
			fatigueRate: 1.0f
		) {
			Category = TerrainCategory.Natural,
			BaseDifficulty = 0.3f,
			BlocksLineOfSight = true,
			EncounterChanceMultiplier = 1.1f,
			AmbushChanceMultiplier = 1.3f,
			VisionRangeMultiplier = 0.7f,
			ForagingChanceMultiplier = 1.2f,
			HuntingSuccessMultiplier = 1.2f,
			ProvidesNaturalShelter = true,
			WaterAvailability = 0.5f,
			ResourceCategories = [
				TerrainResourceCategory.Wood,
				TerrainResourceCategory.Herbs,
				TerrainResourceCategory.Game,
				TerrainResourceCategory.Forage
			],
			Tags = [Ids.Tags.Region.Forest],
			MapColor = "#228B22",
			DisplayOrder = 30
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Forests.DeepForest,
			name: "Deep Forest",
			description: "Dense, ancient woodland where sunlight barely penetrates.",
			movementSpeed: 0.7f,
			staminaDrain: 1.3f,
			fatigueRate: 1.2f
		) {
			Category = TerrainCategory.Natural,
			BaseDifficulty = 0.5f,
			BlocksLineOfSight = true,
			EncounterChanceMultiplier = 1.4f,
			AmbushChanceMultiplier = 1.8f,
			VisionRangeMultiplier = 0.4f,
			ForagingChanceMultiplier = 1.4f,
			HuntingSuccessMultiplier = 1.3f,
			ProvidesNaturalShelter = true,
			MoraleModifierPerHour = -0.5f,
			WaterAvailability = 0.6f,
			ResourceCategories = [
				TerrainResourceCategory.Wood,
				TerrainResourceCategory.Herbs,
				TerrainResourceCategory.Game,
				TerrainResourceCategory.Forage,
				TerrainResourceCategory.Magical
			],
			Tags = [Ids.Tags.Region.DeepForest],
			MapColor = "#006400",
			DisplayOrder = 31
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Forests.AncientForest,
			name: "Ancient Forest",
			description: "Primeval woods touched by ancient magic.",
			movementSpeed: 0.75f,
			staminaDrain: 1.2f,
			fatigueRate: 1.1f
		) {
			Category = TerrainCategory.Natural,
			BaseDifficulty = 0.55f,
			BlocksLineOfSight = true,
			EncounterChanceMultiplier = 1.2f,
			AmbushChanceMultiplier = 1.5f,
			VisionRangeMultiplier = 0.5f,
			ForagingChanceMultiplier = 1.6f,
			HuntingSuccessMultiplier = 1.1f,
			ProvidesNaturalShelter = true,
			RestEffectivenessMultiplier = 1.2f,
			WaterAvailability = 0.7f,
			ResourceCategories = [
				TerrainResourceCategory.Wood,
				TerrainResourceCategory.Herbs,
				TerrainResourceCategory.Magical
			],
			Tags = [Ids.Tags.Region.DeepForest, Ids.Tags.Element.Nature],
			MapColor = "#2E8B57",
			DisplayOrder = 32
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Forests.DeadForest,
			name: "Dead Forest",
			description: "A corrupted or burned forest, lifeless and hazardous.",
			movementSpeed: 0.85f,
			staminaDrain: 1.1f,
			fatigueRate: 1.15f
		) {
			Category = TerrainCategory.Natural,
			BaseDifficulty = 0.45f,
			BlocksLineOfSight = true,
			EncounterChanceMultiplier = 1.3f,
			AmbushChanceMultiplier = 1.4f,
			VisionRangeMultiplier = 0.6f,
			ForagingChanceMultiplier = 0.2f,
			HuntingSuccessMultiplier = 0.3f,
			MoraleModifierPerHour = -1.5f,
			WaterAvailability = 0.2f,
			ResourceCategories = [TerrainResourceCategory.Wood, TerrainResourceCategory.Salvage],
			Tags = [Ids.Tags.Region.Forest, Ids.Tags.Element.Necrotic],
			MapColor = "#4A3728",
			DisplayOrder = 33
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Forests.Jungle,
			name: "Jungle",
			description: "Dense tropical forest, extremely slow going.",
			movementSpeed: 0.5f,
			staminaDrain: 1.6f,
			fatigueRate: 1.4f
		) {
			Category = TerrainCategory.Natural,
			BaseDifficulty = 0.65f,
			BlocksLineOfSight = true,
			EncounterChanceMultiplier = 1.6f,
			AmbushChanceMultiplier = 2.0f,
			VisionRangeMultiplier = 0.3f,
			ForagingChanceMultiplier = 1.8f,
			HuntingSuccessMultiplier = 1.4f,
			WaterAvailability = 0.9f,
			EnvironmentalDamagePerHour = 0.5f,
			EnvironmentalDamageType = DamageType.Poison,
			ProvidesNaturalShelter = true,
			ResourceCategories = [
				TerrainResourceCategory.Herbs,
				TerrainResourceCategory.Game,
				TerrainResourceCategory.Forage
			],
			Tags = [Ids.Tags.Region.Jungle],
			MapColor = "#013220",
			DisplayOrder = 34
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Forests.Orchard,
			name: "Orchard",
			description: "Cultivated fruit trees in neat rows.",
			movementSpeed: 1.0f,
			staminaDrain: 0.9f,
			fatigueRate: 0.9f
		) {
			Category = TerrainCategory.Natural,
			BaseDifficulty = 0.15f,
			EncounterChanceMultiplier = 0.5f,
			AmbushChanceMultiplier = 0.7f,
			VisionRangeMultiplier = 0.8f,
			ForagingChanceMultiplier = 2.0f,
			MoraleModifierPerHour = 0.5f,
			WaterAvailability = 0.6f,
			ResourceCategories = [TerrainResourceCategory.Forage],
			MapColor = "#90B77D",
			DisplayOrder = 35
		});
	}

	#endregion

	#region Mountains

	private void RegisterMountains(GameDb db) {
		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Mountains.Mountain,
			name: "Mountain",
			description: "Rugged mountain terrain requiring careful navigation.",
			movementSpeed: 0.5f,
			staminaDrain: 1.8f,
			fatigueRate: 1.5f
		) {
			Category = TerrainCategory.Natural,
			BaseDifficulty = 0.7f,
			EncounterChanceMultiplier = 0.8f,
			AmbushChanceMultiplier = 1.2f,
			VisionRangeMultiplier = 1.8f,
			ForagingChanceMultiplier = 0.4f,
			HuntingSuccessMultiplier = 0.6f,
			WaterAvailability = 0.4f,
			EnvironmentalDamagePerHour = 0.3f,
			EnvironmentalDamageType = DamageType.Cold,
			ResourceCategories = [TerrainResourceCategory.Ore, TerrainResourceCategory.Herbs],
			Tags = [Ids.Tags.Region.Mountain],
			MapColor = "#696969",
			DisplayOrder = 40
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Mountains.Peak,
			name: "Mountain Peak",
			description: "The treacherous summit with extreme conditions.",
			movementSpeed: 0.3f,
			staminaDrain: 2.5f,
			fatigueRate: 2.0f
		) {
			Category = TerrainCategory.Extreme,
			BaseDifficulty = 0.9f,
			RequiresSpecialEquipment = true,
			RequiredEquipmentType = "climbing_gear",
			EncounterChanceMultiplier = 0.4f,
			AmbushChanceMultiplier = 0.5f,
			VisionRangeMultiplier = 2.5f,
			ForagingChanceMultiplier = 0.1f,
			HuntingSuccessMultiplier = 0.2f,
			WaterAvailability = 0.2f,
			EnvironmentalDamagePerHour = 1.0f,
			EnvironmentalDamageType = DamageType.Cold,
			MoraleModifierPerHour = -1.0f,
			ResourceCategories = [TerrainResourceCategory.Ore, TerrainResourceCategory.Magical],
			Tags = [Ids.Tags.Region.Mountain],
			MapColor = "#DCDCDC",
			DisplayOrder = 41
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Mountains.Cliff,
			name: "Cliff",
			description: "Sheer cliff face, impassable without equipment.",
			movementSpeed: 0.2f,
			staminaDrain: 3.0f,
			fatigueRate: 2.5f
		) {
			Category = TerrainCategory.Extreme,
			BaseDifficulty = 0.95f,
			IsPassable = false,
			RequiresSpecialEquipment = true,
			RequiredEquipmentType = "climbing_gear",
			VisionRangeMultiplier = 2.0f,
			WaterAvailability = 0.1f,
			MapColor = "#808080",
			DisplayOrder = 42
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Mountains.Pass,
			name: "Mountain Pass",
			description: "A navigable route through the mountains.",
			movementSpeed: 0.7f,
			staminaDrain: 1.3f,
			fatigueRate: 1.2f
		) {
			Category = TerrainCategory.Natural,
			BaseDifficulty = 0.45f,
			EncounterChanceMultiplier = 1.2f,
			AmbushChanceMultiplier = 1.5f,
			VisionRangeMultiplier = 1.0f,
			WaterAvailability = 0.3f,
			Tags = [Ids.Tags.Region.Mountain],
			MapColor = "#A0A0A0",
			DisplayOrder = 43
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Mountains.Highland,
			name: "Highlands",
			description: "Elevated plateau with strong winds.",
			movementSpeed: 0.8f,
			staminaDrain: 1.2f,
			fatigueRate: 1.1f
		) {
			Category = TerrainCategory.Natural,
			BaseDifficulty = 0.4f,
			EncounterChanceMultiplier = 0.9f,
			AmbushChanceMultiplier = 0.6f,
			VisionRangeMultiplier = 1.6f,
			ForagingChanceMultiplier = 0.7f,
			HuntingSuccessMultiplier = 0.8f,
			MoraleModifierPerHour = 0.3f,
			WaterAvailability = 0.35f,
			ResourceCategories = [TerrainResourceCategory.Herbs, TerrainResourceCategory.Game],
			Tags = [Ids.Tags.Region.Hills],
			MapColor = "#9ACD32",
			DisplayOrder = 44
		});
	}

	#endregion

	#region Water

	private void RegisterWater(GameDb db) {
		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Water.River,
			name: "River",
			description: "Flowing water that may need a bridge or ford to cross.",
			movementSpeed: 0.4f,
			staminaDrain: 1.5f,
			fatigueRate: 1.3f
		) {
			Category = TerrainCategory.Aquatic,
			BaseDifficulty = 0.5f,
			IsPassable = false,
			RequiresSpecialEquipment = true,
			RequiredEquipmentType = "boat",
			WaterAvailability = 1.0f,
			ResourceCategories = [TerrainResourceCategory.Fish],
			MapColor = "#4169E1",
			DisplayOrder = 50
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Water.Lake,
			name: "Lake",
			description: "A large body of fresh water.",
			movementSpeed: 0.3f,
			staminaDrain: 1.8f,
			fatigueRate: 1.4f
		) {
			Category = TerrainCategory.Aquatic,
			BaseDifficulty = 0.6f,
			IsPassable = false,
			RequiresSpecialEquipment = true,
			RequiredEquipmentType = "boat",
			VisionRangeMultiplier = 1.5f,
			WaterAvailability = 1.0f,
			ResourceCategories = [TerrainResourceCategory.Fish],
			MapColor = "#4682B4",
			DisplayOrder = 51
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Water.Pond,
			name: "Pond",
			description: "A small body of water, easy to navigate around.",
			movementSpeed: 0.9f,
			staminaDrain: 1.0f,
			fatigueRate: 1.0f
		) {
			Category = TerrainCategory.Aquatic,
			BaseDifficulty = 0.2f,
			WaterAvailability = 1.0f,
			RestEffectivenessMultiplier = 1.1f,
			ResourceCategories = [TerrainResourceCategory.Fish, TerrainResourceCategory.Herbs],
			MapColor = "#87CEEB",
			DisplayOrder = 52
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Water.Ocean,
			name: "Ocean",
			description: "Deep water, impassable without a vessel.",
			movementSpeed: 0f,
			staminaDrain: 2.0f,
			fatigueRate: 1.5f
		) {
			Category = TerrainCategory.Aquatic,
			BaseDifficulty = 1.0f,
			IsPassable = false,
			RequiresSpecialEquipment = true,
			RequiredEquipmentType = "ship",
			VisionRangeMultiplier = 2.0f,
			WaterAvailability = 0f, // Salt water - not drinkable
			ResourceCategories = [TerrainResourceCategory.Fish],
			MapColor = "#000080",
			DisplayOrder = 53
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Water.Shallows,
			name: "Shallows",
			description: "Shallow water that can be waded through.",
			movementSpeed: 0.6f,
			staminaDrain: 1.4f,
			fatigueRate: 1.2f
		) {
			Category = TerrainCategory.Aquatic,
			BaseDifficulty = 0.35f,
			WaterAvailability = 1.0f,
			ResourceCategories = [TerrainResourceCategory.Fish],
			MapColor = "#ADD8E6",
			DisplayOrder = 54
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Water.Waterfall,
			name: "Waterfall",
			description: "A scenic waterfall, sometimes hiding caves.",
			movementSpeed: 0.5f,
			staminaDrain: 1.3f,
			fatigueRate: 1.1f
		) {
			Category = TerrainCategory.Aquatic,
			BaseDifficulty = 0.4f,
			VisionRangeMultiplier = 0.8f,
			WaterAvailability = 1.0f,
			MoraleModifierPerHour = 1.5f,
			RestEffectivenessMultiplier = 1.2f,
			ResourceCategories = [TerrainResourceCategory.Fish],
			MapColor = "#E0FFFF",
			DisplayOrder = 55
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Water.HotSpring,
			name: "Hot Spring",
			description: "Natural hot springs with healing properties.",
			movementSpeed: 0.8f,
			staminaDrain: 0.7f,
			fatigueRate: 0.6f
		) {
			Category = TerrainCategory.Aquatic,
			BaseDifficulty = 0.15f,
			WaterAvailability = 1.0f,
			MoraleModifierPerHour = 2.0f,
			RestEffectivenessMultiplier = 1.5f,
			ProvidesNaturalShelter = true,
			MapColor = "#FFA07A",
			DisplayOrder = 56
		});
	}

	#endregion

	#region Coastal

	private void RegisterCoastal(GameDb db) {
		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Coastal.Beach,
			name: "Beach",
			description: "Sandy shoreline with shells and driftwood.",
			movementSpeed: 0.85f,
			staminaDrain: 1.1f,
			fatigueRate: 1.0f
		) {
			Category = TerrainCategory.Aquatic,
			BaseDifficulty = 0.25f,
			VisionRangeMultiplier = 1.4f,
			WaterAvailability = 0.3f,
			ForagingChanceMultiplier = 1.2f,
			MoraleModifierPerHour = 0.5f,
			ResourceCategories = [TerrainResourceCategory.Forage, TerrainResourceCategory.Salvage],
			Tags = [Ids.Tags.Region.Coastal],
			MapColor = "#F0E68C",
			DisplayOrder = 60
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Coastal.RockyCoast,
			name: "Rocky Coast",
			description: "Rugged coastal terrain with tide pools.",
			movementSpeed: 0.7f,
			staminaDrain: 1.2f,
			fatigueRate: 1.1f
		) {
			Category = TerrainCategory.Aquatic,
			BaseDifficulty = 0.4f,
			VisionRangeMultiplier = 1.5f,
			WaterAvailability = 0.4f,
			ForagingChanceMultiplier = 1.4f,
			ResourceCategories = [TerrainResourceCategory.Forage, TerrainResourceCategory.Fish],
			Tags = [Ids.Tags.Region.Coastal],
			MapColor = "#778899",
			DisplayOrder = 61
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Coastal.TidalFlats,
			name: "Tidal Flats",
			description: "Muddy flats exposed at low tide.",
			movementSpeed: 0.6f,
			staminaDrain: 1.4f,
			fatigueRate: 1.2f
		) {
			Category = TerrainCategory.Aquatic,
			BaseDifficulty = 0.45f,
			VisionRangeMultiplier = 1.3f,
			WaterAvailability = 0.5f,
			ForagingChanceMultiplier = 1.6f,
			ResourceCategories = [TerrainResourceCategory.Forage, TerrainResourceCategory.Fish],
			MapColor = "#8B7355",
			DisplayOrder = 62
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Coastal.Reef,
			name: "Coral Reef",
			description: "Underwater terrain requiring diving.",
			movementSpeed: 0.3f,
			staminaDrain: 2.0f,
			fatigueRate: 1.5f
		) {
			Category = TerrainCategory.Aquatic,
			BaseDifficulty = 0.7f,
			IsPassable = false,
			RequiresSpecialEquipment = true,
			RequiredEquipmentType = "diving_gear",
			WaterAvailability = 0f,
			ResourceCategories = [TerrainResourceCategory.Fish, TerrainResourceCategory.Magical],
			MapColor = "#FF7F50",
			DisplayOrder = 63
		});
	}

	#endregion

	#region Wetlands

	private void RegisterWetlands(GameDb db) {
		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Wetlands.Swamp,
			name: "Swamp",
			description: "Treacherous wetland with disease and danger.",
			movementSpeed: 0.5f,
			staminaDrain: 1.5f,
			fatigueRate: 1.4f
		) {
			Category = TerrainCategory.Natural,
			BaseDifficulty = 0.55f,
			BlocksLineOfSight = true,
			EncounterChanceMultiplier = 1.3f,
			AmbushChanceMultiplier = 1.6f,
			VisionRangeMultiplier = 0.5f,
			ForagingChanceMultiplier = 1.3f,
			HuntingSuccessMultiplier = 0.8f,
			WaterAvailability = 0.8f, // Unclean water
			MoraleModifierPerHour = -1.0f,
			EnvironmentalDamagePerHour = 0.3f,
			EnvironmentalDamageType = DamageType.Poison,
			ResourceCategories = [
				TerrainResourceCategory.Herbs,
				TerrainResourceCategory.Game,
				TerrainResourceCategory.Magical
			],
			Tags = [Ids.Tags.Region.Swamp],
			MapColor = "#556B2F",
			DisplayOrder = 70
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Wetlands.Marsh,
			name: "Marsh",
			description: "Wet grassland with hidden pools and mud.",
			movementSpeed: 0.6f,
			staminaDrain: 1.3f,
			fatigueRate: 1.2f
		) {
			Category = TerrainCategory.Natural,
			BaseDifficulty = 0.45f,
			EncounterChanceMultiplier = 1.1f,
			AmbushChanceMultiplier = 1.4f,
			VisionRangeMultiplier = 0.7f,
			ForagingChanceMultiplier = 1.2f,
			WaterAvailability = 0.7f,
			MoraleModifierPerHour = -0.5f,
			ResourceCategories = [TerrainResourceCategory.Herbs, TerrainResourceCategory.Fish],
			Tags = [Ids.Tags.Region.Marsh],
			MapColor = "#6B8E23",
			DisplayOrder = 71
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Wetlands.Bog,
			name: "Bog",
			description: "Treacherous peat bog with risk of sinking.",
			movementSpeed: 0.4f,
			staminaDrain: 1.7f,
			fatigueRate: 1.5f
		) {
			Category = TerrainCategory.Natural,
			BaseDifficulty = 0.65f,
			EncounterChanceMultiplier = 0.9f,
			AmbushChanceMultiplier = 1.2f,
			VisionRangeMultiplier = 0.6f,
			ForagingChanceMultiplier = 0.8f,
			WaterAvailability = 0.6f,
			MoraleModifierPerHour = -1.5f,
			ResourceCategories = [TerrainResourceCategory.Herbs],
			Tags = [Ids.Tags.Region.Swamp],
			MapColor = "#5C4033",
			DisplayOrder = 72
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Wetlands.Mangrove,
			name: "Mangrove",
			description: "Coastal swamp with tangled roots.",
			movementSpeed: 0.45f,
			staminaDrain: 1.6f,
			fatigueRate: 1.4f
		) {
			Category = TerrainCategory.Aquatic,
			BaseDifficulty = 0.6f,
			BlocksLineOfSight = true,
			EncounterChanceMultiplier = 1.2f,
			AmbushChanceMultiplier = 1.7f,
			VisionRangeMultiplier = 0.4f,
			ForagingChanceMultiplier = 1.1f,
			WaterAvailability = 0.5f,
			ResourceCategories = [
				TerrainResourceCategory.Fish,
				TerrainResourceCategory.Wood,
				TerrainResourceCategory.Herbs
			],
			Tags = [Ids.Tags.Region.Swamp, Ids.Tags.Region.Coastal],
			MapColor = "#4A7023",
			DisplayOrder = 73
		});
	}

	#endregion

	#region Desert

	private void RegisterDesert(GameDb db) {
		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Desert.OpenDesert,
			name: "Desert",
			description: "Arid wasteland requiring careful water management.",
			movementSpeed: 0.8f,
			staminaDrain: 1.4f,
			fatigueRate: 1.3f
		) {
			Category = TerrainCategory.Extreme,
			BaseDifficulty = 0.55f,
			EncounterChanceMultiplier = 0.7f,
			AmbushChanceMultiplier = 0.8f,
			VisionRangeMultiplier = 1.8f,
			ForagingChanceMultiplier = 0.2f,
			HuntingSuccessMultiplier = 0.4f,
			WaterAvailability = 0.05f,
			EnvironmentalDamagePerHour = 0.5f,
			EnvironmentalDamageType = DamageType.Fire,
			MoraleModifierPerHour = -0.5f,
			ResourceCategories = [TerrainResourceCategory.Ore],
			Tags = [Ids.Tags.Region.Desert],
			MapColor = "#EDC9AF",
			DisplayOrder = 80
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Desert.Dunes,
			name: "Sand Dunes",
			description: "Shifting sand dunes, extremely slow travel.",
			movementSpeed: 0.4f,
			staminaDrain: 2.0f,
			fatigueRate: 1.8f
		) {
			Category = TerrainCategory.Extreme,
			BaseDifficulty = 0.7f,
			EncounterChanceMultiplier = 0.5f,
			AmbushChanceMultiplier = 1.3f,
			VisionRangeMultiplier = 1.5f,
			ForagingChanceMultiplier = 0.1f,
			WaterAvailability = 0.02f,
			EnvironmentalDamagePerHour = 0.7f,
			EnvironmentalDamageType = DamageType.Fire,
			MoraleModifierPerHour = -1.0f,
			Tags = [Ids.Tags.Region.Desert],
			MapColor = "#F4A460",
			DisplayOrder = 81
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Desert.Oasis,
			name: "Oasis",
			description: "Life-giving water source in the desert.",
			movementSpeed: 1.0f,
			staminaDrain: 0.8f,
			fatigueRate: 0.7f
		) {
			Category = TerrainCategory.Natural,
			BaseDifficulty = 0.2f,
			EncounterChanceMultiplier = 1.2f,
			VisionRangeMultiplier = 1.0f,
			ForagingChanceMultiplier = 1.5f,
			WaterAvailability = 1.0f,
			MoraleModifierPerHour = 2.0f,
			RestEffectivenessMultiplier = 1.4f,
			ProvidesNaturalShelter = true,
			ResourceCategories = [TerrainResourceCategory.Herbs, TerrainResourceCategory.Forage],
			Tags = [Ids.Tags.Region.Desert],
			MapColor = "#00CED1",
			DisplayOrder = 82
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Desert.Badlands,
			name: "Badlands",
			description: "Eroded terrain with difficult navigation.",
			movementSpeed: 0.6f,
			staminaDrain: 1.5f,
			fatigueRate: 1.4f
		) {
			Category = TerrainCategory.Extreme,
			BaseDifficulty = 0.6f,
			EncounterChanceMultiplier = 0.8f,
			AmbushChanceMultiplier = 1.4f,
			VisionRangeMultiplier = 1.2f,
			ForagingChanceMultiplier = 0.3f,
			WaterAvailability = 0.1f,
			MoraleModifierPerHour = -0.8f,
			ResourceCategories = [TerrainResourceCategory.Ore, TerrainResourceCategory.Salvage],
			MapColor = "#CD853F",
			DisplayOrder = 83
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Desert.SaltFlats,
			name: "Salt Flats",
			description: "Blinding white expanse with no resources.",
			movementSpeed: 1.1f,
			staminaDrain: 1.2f,
			fatigueRate: 1.1f
		) {
			Category = TerrainCategory.Extreme,
			BaseDifficulty = 0.5f,
			EncounterChanceMultiplier = 0.3f,
			VisionRangeMultiplier = 2.0f,
			ForagingChanceMultiplier = 0f,
			WaterAvailability = 0f,
			EnvironmentalDamagePerHour = 0.3f,
			EnvironmentalDamageType = DamageType.Fire,
			MoraleModifierPerHour = -1.2f,
			Tags = [Ids.Tags.Region.Desert],
			MapColor = "#FFFAFA",
			DisplayOrder = 84
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Desert.Canyon,
			name: "Canyon",
			description: "Deep ravine with limited paths.",
			movementSpeed: 0.7f,
			staminaDrain: 1.3f,
			fatigueRate: 1.2f
		) {
			Category = TerrainCategory.Natural,
			BaseDifficulty = 0.5f,
			BlocksLineOfSight = true,
			EncounterChanceMultiplier = 1.1f,
			AmbushChanceMultiplier = 1.8f,
			VisionRangeMultiplier = 0.6f,
			WaterAvailability = 0.2f,
			ProvidesNaturalShelter = true,
			ResourceCategories = [TerrainResourceCategory.Ore],
			MapColor = "#B8860B",
			DisplayOrder = 85
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Desert.Mesa,
			name: "Mesa",
			description: "Flat-topped elevation in desert terrain.",
			movementSpeed: 0.75f,
			staminaDrain: 1.4f,
			fatigueRate: 1.3f
		) {
			Category = TerrainCategory.Natural,
			BaseDifficulty = 0.55f,
			EncounterChanceMultiplier = 0.9f,
			VisionRangeMultiplier = 2.0f,
			ForagingChanceMultiplier = 0.4f,
			WaterAvailability = 0.15f,
			ResourceCategories = [TerrainResourceCategory.Ore],
			Tags = [Ids.Tags.Region.Desert],
			MapColor = "#D2691E",
			DisplayOrder = 86
		});
	}

	#endregion

	#region Frozen

	private void RegisterFrozen(GameDb db) {
		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Frozen.Snow,
			name: "Snow",
			description: "Snow-covered terrain requiring cold protection.",
			movementSpeed: 0.6f,
			staminaDrain: 1.5f,
			fatigueRate: 1.4f
		) {
			Category = TerrainCategory.Extreme,
			BaseDifficulty = 0.55f,
			EncounterChanceMultiplier = 0.7f,
			AmbushChanceMultiplier = 0.9f,
			VisionRangeMultiplier = 1.3f,
			ForagingChanceMultiplier = 0.3f,
			HuntingSuccessMultiplier = 0.7f,
			WaterAvailability = 0.8f, // Snow can be melted
			EnvironmentalDamagePerHour = 0.5f,
			EnvironmentalDamageType = DamageType.Cold,
			MoraleModifierPerHour = -0.5f,
			ResourceCategories = [TerrainResourceCategory.Game],
			Tags = [Ids.Tags.Region.Tundra],
			MapColor = "#FFFAFA",
			DisplayOrder = 90
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Frozen.Ice,
			name: "Ice Field",
			description: "Slippery ice with risk of falling.",
			movementSpeed: 0.5f,
			staminaDrain: 1.6f,
			fatigueRate: 1.5f
		) {
			Category = TerrainCategory.Extreme,
			BaseDifficulty = 0.65f,
			EncounterChanceMultiplier = 0.5f,
			VisionRangeMultiplier = 1.5f,
			ForagingChanceMultiplier = 0.1f,
			WaterAvailability = 0.9f,
			EnvironmentalDamagePerHour = 0.7f,
			EnvironmentalDamageType = DamageType.Cold,
			MoraleModifierPerHour = -0.8f,
			Tags = [Ids.Tags.Region.Arctic],
			MapColor = "#B0E0E6",
			DisplayOrder = 91
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Frozen.Glacier,
			name: "Glacier",
			description: "Slow movement with crevasse danger.",
			movementSpeed: 0.35f,
			staminaDrain: 2.2f,
			fatigueRate: 1.8f
		) {
			Category = TerrainCategory.Extreme,
			BaseDifficulty = 0.8f,
			RequiresSpecialEquipment = true,
			RequiredEquipmentType = "climbing_gear",
			EncounterChanceMultiplier = 0.3f,
			VisionRangeMultiplier = 1.6f,
			ForagingChanceMultiplier = 0f,
			WaterAvailability = 0.9f,
			EnvironmentalDamagePerHour = 1.0f,
			EnvironmentalDamageType = DamageType.Cold,
			MoraleModifierPerHour = -1.0f,
			ResourceCategories = [TerrainResourceCategory.Magical],
			Tags = [Ids.Tags.Region.Arctic],
			MapColor = "#E0FFFF",
			DisplayOrder = 92
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Frozen.FrozenLake,
			name: "Frozen Lake",
			description: "Ice-covered lake that may crack under weight.",
			movementSpeed: 0.8f,
			staminaDrain: 1.2f,
			fatigueRate: 1.1f
		) {
			Category = TerrainCategory.Extreme,
			BaseDifficulty = 0.5f,
			EncounterChanceMultiplier = 0.6f,
			VisionRangeMultiplier = 1.4f,
			WaterAvailability = 0.9f,
			EnvironmentalDamagePerHour = 0.4f,
			EnvironmentalDamageType = DamageType.Cold,
			ResourceCategories = [TerrainResourceCategory.Fish],
			Tags = [Ids.Tags.Region.Arctic],
			MapColor = "#87CEFA",
			DisplayOrder = 93
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Frozen.Permafrost,
			name: "Permafrost",
			description: "Frozen ground, hard to dig or camp.",
			movementSpeed: 0.7f,
			staminaDrain: 1.4f,
			fatigueRate: 1.3f
		) {
			Category = TerrainCategory.Extreme,
			BaseDifficulty = 0.6f,
			EncounterChanceMultiplier = 0.6f,
			VisionRangeMultiplier = 1.2f,
			ForagingChanceMultiplier = 0.2f,
			WaterAvailability = 0.7f,
			EnvironmentalDamagePerHour = 0.4f,
			EnvironmentalDamageType = DamageType.Cold,
			RestEffectivenessMultiplier = 0.7f,
			Tags = [Ids.Tags.Region.Tundra],
			MapColor = "#D3D3D3",
			DisplayOrder = 94
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Frozen.Blizzard,
			name: "Blizzard Zone",
			description: "Extreme cold with near-zero visibility.",
			movementSpeed: 0.3f,
			staminaDrain: 2.5f,
			fatigueRate: 2.0f
		) {
			Category = TerrainCategory.Extreme,
			BaseDifficulty = 0.9f,
			EncounterChanceMultiplier = 0.4f,
			AmbushChanceMultiplier = 1.5f,
			VisionRangeMultiplier = 0.2f,
			ForagingChanceMultiplier = 0f,
			WaterAvailability = 0.8f,
			EnvironmentalDamagePerHour = 1.5f,
			EnvironmentalDamageType = DamageType.Cold,
			MoraleModifierPerHour = -2.0f,
			Tags = [Ids.Tags.Region.Arctic],
			MapColor = "#F0F8FF",
			DisplayOrder = 95
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Frozen.Tundra,
			name: "Tundra",
			description: "Cold, treeless plain with sparse life.",
			movementSpeed: 0.85f,
			staminaDrain: 1.2f,
			fatigueRate: 1.15f
		) {
			Category = TerrainCategory.Natural,
			BaseDifficulty = 0.45f,
			EncounterChanceMultiplier = 0.7f,
			VisionRangeMultiplier = 1.5f,
			ForagingChanceMultiplier = 0.4f,
			HuntingSuccessMultiplier = 0.8f,
			WaterAvailability = 0.5f,
			EnvironmentalDamagePerHour = 0.3f,
			EnvironmentalDamageType = DamageType.Cold,
			ResourceCategories = [TerrainResourceCategory.Game, TerrainResourceCategory.Herbs],
			Tags = [Ids.Tags.Region.Tundra],
			MapColor = "#C0C0C0",
			DisplayOrder = 96
		});
	}

	#endregion

	#region Volcanic

	private void RegisterVolcanic(GameDb db) {
		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Volcanic.VolcanicPlain,
			name: "Volcanic Terrain",
			description: "Heat damage but rare minerals abound.",
			movementSpeed: 0.6f,
			staminaDrain: 1.6f,
			fatigueRate: 1.5f
		) {
			Category = TerrainCategory.Extreme,
			BaseDifficulty = 0.65f,
			EncounterChanceMultiplier = 0.8f,
			VisionRangeMultiplier = 1.0f,
			ForagingChanceMultiplier = 0.2f,
			WaterAvailability = 0.1f,
			EnvironmentalDamagePerHour = 0.8f,
			EnvironmentalDamageType = DamageType.Fire,
			MoraleModifierPerHour = -1.0f,
			ResourceCategories = [TerrainResourceCategory.Ore, TerrainResourceCategory.Magical],
			Tags = [Ids.Tags.Region.Volcanic],
			MapColor = "#8B0000",
			DisplayOrder = 100
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Volcanic.LavaField,
			name: "Lava Field",
			description: "Extremely dangerous, fire damage guaranteed.",
			movementSpeed: 0.3f,
			staminaDrain: 2.5f,
			fatigueRate: 2.0f
		) {
			Category = TerrainCategory.Extreme,
			BaseDifficulty = 0.95f,
			RequiresSpecialEquipment = true,
			RequiredEquipmentType = "fire_protection",
			EncounterChanceMultiplier = 0.5f,
			VisionRangeMultiplier = 0.8f,
			ForagingChanceMultiplier = 0f,
			WaterAvailability = 0f,
			EnvironmentalDamagePerHour = 2.0f,
			EnvironmentalDamageType = DamageType.Fire,
			MoraleModifierPerHour = -2.0f,
			ResourceCategories = [TerrainResourceCategory.Ore, TerrainResourceCategory.Magical],
			Tags = [Ids.Tags.Region.Volcanic, Ids.Tags.Element.Fire],
			MapColor = "#FF4500",
			DisplayOrder = 101
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Volcanic.AshWastes,
			name: "Ash Wastes",
			description: "Volcanic aftermath with poor visibility.",
			movementSpeed: 0.7f,
			staminaDrain: 1.4f,
			fatigueRate: 1.3f
		) {
			Category = TerrainCategory.Extreme,
			BaseDifficulty = 0.55f,
			EncounterChanceMultiplier = 0.9f,
			AmbushChanceMultiplier = 1.3f,
			VisionRangeMultiplier = 0.5f,
			ForagingChanceMultiplier = 0.1f,
			WaterAvailability = 0.1f,
			EnvironmentalDamagePerHour = 0.3f,
			EnvironmentalDamageType = DamageType.Poison,
			MoraleModifierPerHour = -1.5f,
			ResourceCategories = [TerrainResourceCategory.Ore, TerrainResourceCategory.Salvage],
			Tags = [Ids.Tags.Region.Volcanic],
			MapColor = "#696969",
			DisplayOrder = 102
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Volcanic.Geothermal,
			name: "Geothermal",
			description: "Hot springs and geysers dot the landscape.",
			movementSpeed: 0.8f,
			staminaDrain: 1.2f,
			fatigueRate: 1.1f
		) {
			Category = TerrainCategory.Extreme,
			BaseDifficulty = 0.45f,
			EncounterChanceMultiplier = 0.9f,
			VisionRangeMultiplier = 0.9f,
			ForagingChanceMultiplier = 0.6f,
			WaterAvailability = 0.8f,
			EnvironmentalDamagePerHour = 0.2f,
			EnvironmentalDamageType = DamageType.Fire,
			RestEffectivenessMultiplier = 1.2f,
			ResourceCategories = [TerrainResourceCategory.Ore, TerrainResourceCategory.Magical],
			Tags = [Ids.Tags.Region.Volcanic],
			MapColor = "#FF6347",
			DisplayOrder = 103
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Volcanic.ObsidianField,
			name: "Obsidian Field",
			description: "Sharp volcanic glass terrain, dangerous to traverse.",
			movementSpeed: 0.5f,
			staminaDrain: 1.8f,
			fatigueRate: 1.6f
		) {
			Category = TerrainCategory.Extreme,
			BaseDifficulty = 0.7f,
			EncounterChanceMultiplier = 0.6f,
			VisionRangeMultiplier = 1.1f,
			ForagingChanceMultiplier = 0f,
			WaterAvailability = 0.05f,
			EnvironmentalDamagePerHour = 0.4f,
			EnvironmentalDamageType = DamageType.Physical,
			ResourceCategories = [TerrainResourceCategory.Ore, TerrainResourceCategory.Magical],
			Tags = [Ids.Tags.Region.Volcanic],
			MapColor = "#1C1C1C",
			DisplayOrder = 104
		});
	}

	#endregion

#region Underground

	private void RegisterUnderground(GameDb db) {
		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Underground.Cave,
			name: "Cave",
			description: "Natural cave entrance leading underground.",
			movementSpeed: 0.8f,
			staminaDrain: 1.1f,
			fatigueRate: 1.0f
		) {
			Category = TerrainCategory.Underground,
			BaseDifficulty = 0.4f,
			BlocksLineOfSight = true,
			EncounterChanceMultiplier = 1.2f,
			AmbushChanceMultiplier = 1.4f,
			VisionRangeMultiplier = 0.4f,
			ForagingChanceMultiplier = 0.6f,
			ProvidesNaturalShelter = true,
			WaterAvailability = 0.4f,
			ResourceCategories = [TerrainResourceCategory.Ore, TerrainResourceCategory.Herbs],
			Tags = [Ids.Tags.Region.Cave],
			MapColor = "#4A4A4A",
			DisplayOrder = 110
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Underground.Cavern,
			name: "Cavern",
			description: "Large underground chamber.",
			movementSpeed: 0.85f,
			staminaDrain: 1.0f,
			fatigueRate: 0.95f
		) {
			Category = TerrainCategory.Underground,
			BaseDifficulty = 0.35f,
			BlocksLineOfSight = true,
			EncounterChanceMultiplier = 1.3f,
			AmbushChanceMultiplier = 1.2f,
			VisionRangeMultiplier = 0.5f,
			ProvidesNaturalShelter = true,
			WaterAvailability = 0.5f,
			ResourceCategories = [TerrainResourceCategory.Ore, TerrainResourceCategory.Magical],
			Tags = [Ids.Tags.Region.Cave],
			MapColor = "#363636",
			DisplayOrder = 111
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Underground.Tunnel,
			name: "Tunnel",
			description: "Narrow underground passage.",
			movementSpeed: 0.7f,
			staminaDrain: 1.2f,
			fatigueRate: 1.1f
		) {
			Category = TerrainCategory.Underground,
			BaseDifficulty = 0.45f,
			BlocksLineOfSight = true,
			EncounterChanceMultiplier = 1.1f,
			AmbushChanceMultiplier = 1.6f,
			VisionRangeMultiplier = 0.3f,
			ProvidesNaturalShelter = true,
			WaterAvailability = 0.3f,
			Tags = [Ids.Tags.Region.Cave],
			MapColor = "#2F2F2F",
			DisplayOrder = 112
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Underground.UndergroundLake,
			name: "Underground Lake",
			description: "Subterranean body of water.",
			movementSpeed: 0.4f,
			staminaDrain: 1.5f,
			fatigueRate: 1.3f
		) {
			Category = TerrainCategory.Underground,
			BaseDifficulty = 0.55f,
			IsPassable = false,
			RequiresSpecialEquipment = true,
			RequiredEquipmentType = "boat",
			VisionRangeMultiplier = 0.4f,
			WaterAvailability = 1.0f,
			ResourceCategories = [TerrainResourceCategory.Fish],
			Tags = [Ids.Tags.Region.Cave],
			MapColor = "#1E3A5F",
			DisplayOrder = 113
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Underground.CrystalCavern,
			name: "Crystal Cavern",
			description: "Magical cavern filled with luminescent crystals.",
			movementSpeed: 0.75f,
			staminaDrain: 1.1f,
			fatigueRate: 1.0f
		) {
			Category = TerrainCategory.Underground,
			BaseDifficulty = 0.5f,
			BlocksLineOfSight = true,
			EncounterChanceMultiplier = 1.0f,
			AmbushChanceMultiplier = 1.0f,
			VisionRangeMultiplier = 0.7f, // Crystals provide light
			ForagingChanceMultiplier = 0.8f,
			ProvidesNaturalShelter = true,
			MoraleModifierPerHour = 0.5f,
			WaterAvailability = 0.4f,
			ResourceCategories = [TerrainResourceCategory.Ore, TerrainResourceCategory.Magical],
			Tags = [Ids.Tags.Region.Cave, Ids.Tags.Element.Arcane],
			MapColor = "#9370DB",
			DisplayOrder = 114
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Underground.MushroomForest,
			name: "Mushroom Forest",
			description: "Giant fungi providing bioluminescent light.",
			movementSpeed: 0.7f,
			staminaDrain: 1.15f,
			fatigueRate: 1.05f
		) {
			Category = TerrainCategory.Underground,
			BaseDifficulty = 0.45f,
			BlocksLineOfSight = true,
			EncounterChanceMultiplier = 1.2f,
			AmbushChanceMultiplier = 1.4f,
			VisionRangeMultiplier = 0.6f,
			ForagingChanceMultiplier = 1.8f,
			ProvidesNaturalShelter = true,
			WaterAvailability = 0.6f,
			ResourceCategories = [TerrainResourceCategory.Herbs, TerrainResourceCategory.Forage, TerrainResourceCategory.Magical],
			Tags = [Ids.Tags.Region.Cave, Ids.Tags.Element.Nature],
			MapColor = "#8A2BE2",
			DisplayOrder = 115
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Underground.LavaTube,
			name: "Lava Tube",
			description: "Volcanic tunnel with extreme heat.",
			movementSpeed: 0.6f,
			staminaDrain: 1.8f,
			fatigueRate: 1.6f
		) {
			Category = TerrainCategory.Underground,
			BaseDifficulty = 0.75f,
			BlocksLineOfSight = true,
			EncounterChanceMultiplier = 0.8f,
			VisionRangeMultiplier = 0.5f,
			WaterAvailability = 0f,
			EnvironmentalDamagePerHour = 1.2f,
			EnvironmentalDamageType = DamageType.Fire,
			MoraleModifierPerHour = -1.0f,
			ResourceCategories = [TerrainResourceCategory.Ore, TerrainResourceCategory.Magical],
			Tags = [Ids.Tags.Region.Cave, Ids.Tags.Region.Volcanic, Ids.Tags.Element.Fire],
			MapColor = "#8B0000",
			DisplayOrder = 116
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Underground.Mine,
			name: "Mine",
			description: "Excavated tunnels with ore deposits.",
			movementSpeed: 0.8f,
			staminaDrain: 1.2f,
			fatigueRate: 1.1f
		) {
			Category = TerrainCategory.Underground,
			BaseDifficulty = 0.4f,
			BlocksLineOfSight = true,
			EncounterChanceMultiplier = 1.0f,
			AmbushChanceMultiplier = 1.3f,
			VisionRangeMultiplier = 0.4f,
			ProvidesNaturalShelter = true,
			WaterAvailability = 0.3f,
			ResourceCategories = [TerrainResourceCategory.Ore, TerrainResourceCategory.Salvage],
			Tags = [Ids.Tags.Region.Dungeon],
			MapColor = "#5C4033",
			DisplayOrder = 117
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Underground.Catacombs,
			name: "Catacombs",
			description: "Underground burial chambers, home to the undead.",
			movementSpeed: 0.75f,
			staminaDrain: 1.15f,
			fatigueRate: 1.1f
		) {
			Category = TerrainCategory.Underground,
			BaseDifficulty = 0.55f,
			BlocksLineOfSight = true,
			EncounterChanceMultiplier = 1.5f,
			AmbushChanceMultiplier = 1.5f,
			VisionRangeMultiplier = 0.35f,
			ForagingChanceMultiplier = 0.2f,
			ProvidesNaturalShelter = true,
			MoraleModifierPerHour = -1.5f,
			WaterAvailability = 0.2f,
			ResourceCategories = [TerrainResourceCategory.Salvage, TerrainResourceCategory.Magical],
			Tags = [Ids.Tags.Region.Crypt, Ids.Tags.Creature.Undead, Ids.Tags.Element.Necrotic],
			MapColor = "#2F4F4F",
			DisplayOrder = 118
		});
	}

	#endregion

	#region Settlements

	private void RegisterSettlements(GameDb db) {
		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Settlements.Village,
			name: "Village",
			description: "A small settlement with basic services.",
			movementSpeed: 1.2f,
			staminaDrain: 0.7f,
			fatigueRate: 0.6f
		) {
			Category = TerrainCategory.Settlement,
			BaseDifficulty = 0.05f,
			EncounterChanceMultiplier = 0.2f,
			AmbushChanceMultiplier = 0.1f,
			VisionRangeMultiplier = 1.2f,
			WaterAvailability = 1.0f,
			MoraleModifierPerHour = 1.5f,
			RestEffectivenessMultiplier = 1.5f,
			ProvidesNaturalShelter = true,
			MapColor = "#DEB887",
			DisplayOrder = 120
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Settlements.Town,
			name: "Town",
			description: "A larger settlement with more services.",
			movementSpeed: 1.3f,
			staminaDrain: 0.6f,
			fatigueRate: 0.5f
		) {
			Category = TerrainCategory.Settlement,
			BaseDifficulty = 0.03f,
			EncounterChanceMultiplier = 0.1f,
			AmbushChanceMultiplier = 0.05f,
			VisionRangeMultiplier = 1.3f,
			WaterAvailability = 1.0f,
			MoraleModifierPerHour = 2.0f,
			RestEffectivenessMultiplier = 1.6f,
			ProvidesNaturalShelter = true,
			MapColor = "#D2B48C",
			DisplayOrder = 121
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Settlements.City,
			name: "City",
			description: "A major settlement with full services.",
			movementSpeed: 1.4f,
			staminaDrain: 0.5f,
			fatigueRate: 0.4f
		) {
			Category = TerrainCategory.Settlement,
			BaseDifficulty = 0.02f,
			EncounterChanceMultiplier = 0.05f,
			AmbushChanceMultiplier = 0.02f,
			VisionRangeMultiplier = 1.4f,
			WaterAvailability = 1.0f,
			MoraleModifierPerHour = 2.5f,
			RestEffectivenessMultiplier = 1.8f,
			ProvidesNaturalShelter = true,
			MapColor = "#C4A484",
			DisplayOrder = 122
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Settlements.Outpost,
			name: "Outpost",
			description: "A small defensive structure.",
			movementSpeed: 1.1f,
			staminaDrain: 0.8f,
			fatigueRate: 0.75f
		) {
			Category = TerrainCategory.Settlement,
			BaseDifficulty = 0.1f,
			EncounterChanceMultiplier = 0.3f,
			AmbushChanceMultiplier = 0.2f,
			VisionRangeMultiplier = 1.5f,
			WaterAvailability = 0.8f,
			MoraleModifierPerHour = 1.0f,
			RestEffectivenessMultiplier = 1.2f,
			ProvidesNaturalShelter = true,
			MapColor = "#8B4513",
			DisplayOrder = 123
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Settlements.Fort,
			name: "Fort",
			description: "A military installation.",
			movementSpeed: 1.15f,
			staminaDrain: 0.75f,
			fatigueRate: 0.7f
		) {
			Category = TerrainCategory.Settlement,
			BaseDifficulty = 0.08f,
			EncounterChanceMultiplier = 0.25f,
			AmbushChanceMultiplier = 0.15f,
			VisionRangeMultiplier = 1.6f,
			WaterAvailability = 0.9f,
			MoraleModifierPerHour = 1.2f,
			RestEffectivenessMultiplier = 1.3f,
			ProvidesNaturalShelter = true,
			MapColor = "#A0522D",
			DisplayOrder = 124
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Settlements.Castle,
			name: "Castle",
			description: "A fortified noble residence.",
			movementSpeed: 1.2f,
			staminaDrain: 0.7f,
			fatigueRate: 0.65f
		) {
			Category = TerrainCategory.Settlement,
			BaseDifficulty = 0.05f,
			EncounterChanceMultiplier = 0.15f,
			AmbushChanceMultiplier = 0.1f,
			VisionRangeMultiplier = 1.7f,
			WaterAvailability = 1.0f,
			MoraleModifierPerHour = 1.5f,
			RestEffectivenessMultiplier = 1.5f,
			ProvidesNaturalShelter = true,
			MapColor = "#696969",
			DisplayOrder = 125
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Settlements.Temple,
			name: "Temple",
			description: "A religious structure offering healing.",
			movementSpeed: 1.1f,
			staminaDrain: 0.7f,
			fatigueRate: 0.6f
		) {
			Category = TerrainCategory.Settlement,
			BaseDifficulty = 0.05f,
			EncounterChanceMultiplier = 0.1f,
			AmbushChanceMultiplier = 0.05f,
			VisionRangeMultiplier = 1.2f,
			WaterAvailability = 0.9f,
			MoraleModifierPerHour = 2.0f,
			RestEffectivenessMultiplier = 1.8f,
			ProvidesNaturalShelter = true,
			Tags = [Ids.Tags.Element.Holy],
			MapColor = "#FFD700",
			DisplayOrder = 126
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Settlements.Tower,
			name: "Tower",
			description: "A wizard tower or watchtower.",
			movementSpeed: 0.9f,
			staminaDrain: 1.1f,
			fatigueRate: 1.0f
		) {
			Category = TerrainCategory.Settlement,
			BaseDifficulty = 0.2f,
			EncounterChanceMultiplier = 0.5f,
			AmbushChanceMultiplier = 0.3f,
			VisionRangeMultiplier = 2.0f,
			WaterAvailability = 0.7f,
			MoraleModifierPerHour = 0.5f,
			RestEffectivenessMultiplier = 1.1f,
			ProvidesNaturalShelter = true,
			Tags = [Ids.Tags.Element.Arcane],
			MapColor = "#4169E1",
			DisplayOrder = 127
		});
	}

	#endregion

	#region Ruins

	private void RegisterRuins(GameDb db) {
		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Ruins.OpenRuins,
			name: "Ruins",
			description: "Crumbling ancient structures to explore.",
			movementSpeed: 0.75f,
			staminaDrain: 1.2f,
			fatigueRate: 1.1f
		) {
			Category = TerrainCategory.Settlement,
			BaseDifficulty = 0.45f,
			BlocksLineOfSight = true,
			EncounterChanceMultiplier = 1.3f,
			AmbushChanceMultiplier = 1.5f,
			VisionRangeMultiplier = 0.7f,
			ForagingChanceMultiplier = 0.5f,
			ProvidesNaturalShelter = true,
			WaterAvailability = 0.3f,
			ResourceCategories = [TerrainResourceCategory.Salvage, TerrainResourceCategory.Magical],
			Tags = [Ids.Tags.Region.Ruins],
			MapColor = "#808080",
			DisplayOrder = 130
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Ruins.AbandonedVillage,
			name: "Abandoned Village",
			description: "A deserted settlement, eerily quiet.",
			movementSpeed: 0.9f,
			staminaDrain: 1.0f,
			fatigueRate: 0.95f
		) {
			Category = TerrainCategory.Settlement,
			BaseDifficulty = 0.35f,
			EncounterChanceMultiplier = 1.2f,
			AmbushChanceMultiplier = 1.3f,
			VisionRangeMultiplier = 0.9f,
			ForagingChanceMultiplier = 0.8f,
			ProvidesNaturalShelter = true,
			MoraleModifierPerHour = -1.0f,
			WaterAvailability = 0.5f,
			ResourceCategories = [TerrainResourceCategory.Salvage, TerrainResourceCategory.Forage],
			Tags = [Ids.Tags.Region.Ruins],
			MapColor = "#A9A9A9",
			DisplayOrder = 131
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Ruins.Graveyard,
			name: "Graveyard",
			description: "Burial ground with restless spirits.",
			movementSpeed: 0.85f,
			staminaDrain: 1.1f,
			fatigueRate: 1.05f
		) {
			Category = TerrainCategory.Settlement,
			BaseDifficulty = 0.5f,
			EncounterChanceMultiplier = 1.5f,
			AmbushChanceMultiplier = 1.4f,
			VisionRangeMultiplier = 0.8f,
			ForagingChanceMultiplier = 0.3f,
			MoraleModifierPerHour = -2.0f,
			WaterAvailability = 0.3f,
			ResourceCategories = [TerrainResourceCategory.Magical],
			Tags = [Ids.Tags.Region.Graveyard, Ids.Tags.Creature.Undead, Ids.Tags.Element.Necrotic],
			MapColor = "#2F4F4F",
			DisplayOrder = 132
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Ruins.Battlefield,
			name: "Battlefield",
			description: "Old war site with ghosts and relics.",
			movementSpeed: 0.9f,
			staminaDrain: 1.05f,
			fatigueRate: 1.0f
		) {
			Category = TerrainCategory.Natural,
			BaseDifficulty = 0.45f,
			EncounterChanceMultiplier = 1.3f,
			AmbushChanceMultiplier = 1.1f,
			VisionRangeMultiplier = 1.1f,
			ForagingChanceMultiplier = 0.4f,
			MoraleModifierPerHour = -1.5f,
			WaterAvailability = 0.3f,
			ResourceCategories = [TerrainResourceCategory.Salvage, TerrainResourceCategory.Magical],
			Tags = [Ids.Tags.Region.Ruins, Ids.Tags.Creature.Undead],
			MapColor = "#8B4513",
			DisplayOrder = 133
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Ruins.Shipwreck,
			name: "Shipwreck",
			description: "Wrecked vessel with salvage opportunities.",
			movementSpeed: 0.6f,
			staminaDrain: 1.3f,
			fatigueRate: 1.2f
		) {
			Category = TerrainCategory.Aquatic,
			BaseDifficulty = 0.5f,
			EncounterChanceMultiplier = 1.1f,
			AmbushChanceMultiplier = 1.2f,
			VisionRangeMultiplier = 0.8f,
			ForagingChanceMultiplier = 0.6f,
			WaterAvailability = 0.4f,
			ResourceCategories = [TerrainResourceCategory.Salvage, TerrainResourceCategory.Fish],
			Tags = [Ids.Tags.Region.Coastal],
			MapColor = "#8B7355",
			DisplayOrder = 134
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Ruins.Dungeon,
			name: "Dungeon",
			description: "Dangerous underground complex.",
			movementSpeed: 0.7f,
			staminaDrain: 1.3f,
			fatigueRate: 1.2f
		) {
			Category = TerrainCategory.Underground,
			BaseDifficulty = 0.65f,
			BlocksLineOfSight = true,
			EncounterChanceMultiplier = 1.6f,
			AmbushChanceMultiplier = 1.7f,
			VisionRangeMultiplier = 0.35f,
			ForagingChanceMultiplier = 0.2f,
			ProvidesNaturalShelter = true,
			MoraleModifierPerHour = -1.0f,
			WaterAvailability = 0.2f,
			ResourceCategories = [TerrainResourceCategory.Salvage, TerrainResourceCategory.Magical],
			Tags = [Ids.Tags.Region.Dungeon],
			MapColor = "#1C1C1C",
			DisplayOrder = 135
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Ruins.Crypt,
			name: "Crypt",
			description: "Underground tomb filled with the dead.",
			movementSpeed: 0.75f,
			staminaDrain: 1.2f,
			fatigueRate: 1.15f
		) {
			Category = TerrainCategory.Underground,
			BaseDifficulty = 0.6f,
			BlocksLineOfSight = true,
			EncounterChanceMultiplier = 1.5f,
			AmbushChanceMultiplier = 1.5f,
			VisionRangeMultiplier = 0.3f,
			ForagingChanceMultiplier = 0.1f,
			ProvidesNaturalShelter = true,
			MoraleModifierPerHour = -2.0f,
			WaterAvailability = 0.1f,
			ResourceCategories = [TerrainResourceCategory.Salvage, TerrainResourceCategory.Magical],
			Tags = [Ids.Tags.Region.Crypt, Ids.Tags.Creature.Undead, Ids.Tags.Element.Necrotic],
			MapColor = "#2F2F2F",
			DisplayOrder = 136
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Ruins.Monument,
			name: "Ancient Monument",
			description: "Standing stones with magical properties.",
			movementSpeed: 1.0f,
			staminaDrain: 0.9f,
			fatigueRate: 0.85f
		) {
			Category = TerrainCategory.Settlement,
			BaseDifficulty = 0.3f,
			EncounterChanceMultiplier = 0.8f,
			AmbushChanceMultiplier = 0.6f,
			VisionRangeMultiplier = 1.2f,
			MoraleModifierPerHour = 0.5f,
			RestEffectivenessMultiplier = 1.2f,
			WaterAvailability = 0.3f,
			ResourceCategories = [TerrainResourceCategory.Magical],
			Tags = [Ids.Tags.Element.Arcane],
			MapColor = "#778899",
			DisplayOrder = 137
		});
	}

	#endregion

	#region Supernatural

	private void RegisterSupernatural(GameDb db) {
		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Supernatural.Corrupted,
			name: "Corrupted Land",
			description: "Tainted by dark magic, warping reality.",
			movementSpeed: 0.7f,
			staminaDrain: 1.4f,
			fatigueRate: 1.3f
		) {
			Category = TerrainCategory.Supernatural,
			BaseDifficulty = 0.65f,
			EncounterChanceMultiplier = 1.5f,
			AmbushChanceMultiplier = 1.4f,
			VisionRangeMultiplier = 0.6f,
			ForagingChanceMultiplier = 0.2f,
			HuntingSuccessMultiplier = 0.3f,
			MoraleModifierPerHour = -2.0f,
			EnvironmentalDamagePerHour = 0.5f,
			EnvironmentalDamageType = DamageType.Necrotic,
			WaterAvailability = 0.2f,
			ResourceCategories = [TerrainResourceCategory.Magical],
			Tags = [Ids.Tags.Fog.Corrupted, Ids.Tags.Element.Necrotic],
			MapColor = "#4B0082",
			DisplayOrder = 140
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Supernatural.Blighted,
			name: "Blighted Land",
			description: "Diseased and dying, spreading corruption.",
			movementSpeed: 0.75f,
			staminaDrain: 1.3f,
			fatigueRate: 1.25f
		) {
			Category = TerrainCategory.Supernatural,
			BaseDifficulty = 0.6f,
			EncounterChanceMultiplier = 1.4f,
			AmbushChanceMultiplier = 1.3f,
			VisionRangeMultiplier = 0.7f,
			ForagingChanceMultiplier = 0.1f,
			HuntingSuccessMultiplier = 0.2f,
			MoraleModifierPerHour = -1.8f,
			EnvironmentalDamagePerHour = 0.4f,
			EnvironmentalDamageType = DamageType.Poison,
			WaterAvailability = 0.15f,
			Tags = [Ids.Tags.Fog.Corrupted],
			MapColor = "#556B2F",
			DisplayOrder = 141
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Supernatural.ShadowRealm,
			name: "Shadow Realm",
			description: "Partial overlay with the shadow plane.",
			movementSpeed: 0.65f,
			staminaDrain: 1.5f,
			fatigueRate: 1.4f
		) {
			Category = TerrainCategory.Supernatural,
			BaseDifficulty = 0.75f,
			BlocksLineOfSight = true,
			EncounterChanceMultiplier = 1.6f,
			AmbushChanceMultiplier = 2.0f,
			VisionRangeMultiplier = 0.3f,
			ForagingChanceMultiplier = 0f,
			MoraleModifierPerHour = -2.5f,
			EnvironmentalDamagePerHour = 0.3f,
			EnvironmentalDamageType = DamageType.Necrotic,
			WaterAvailability = 0f,
			ResourceCategories = [TerrainResourceCategory.Magical],
			Tags = [Ids.Tags.Element.Shadow, Ids.Tags.Element.Necrotic],
			MapColor = "#1A1A2E",
			DisplayOrder = 142
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Supernatural.Wasteland,
			name: "Wasteland",
			description: "Destroyed, lifeless terrain.",
			movementSpeed: 0.8f,
			staminaDrain: 1.3f,
			fatigueRate: 1.2f
		) {
			Category = TerrainCategory.Supernatural,
			BaseDifficulty = 0.55f,
			EncounterChanceMultiplier = 1.2f,
			AmbushChanceMultiplier = 1.0f,
			VisionRangeMultiplier = 1.3f,
			ForagingChanceMultiplier = 0.05f,
			HuntingSuccessMultiplier = 0.1f,
			MoraleModifierPerHour = -1.5f,
			WaterAvailability = 0.05f,
			ResourceCategories = [TerrainResourceCategory.Salvage],
			MapColor = "#3D3D3D",
			DisplayOrder = 143
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Supernatural.CursedGround,
			name: "Cursed Ground",
			description: "Negative magical effects plague travelers.",
			movementSpeed: 0.7f,
			staminaDrain: 1.4f,
			fatigueRate: 1.35f
		) {
			Category = TerrainCategory.Supernatural,
			BaseDifficulty = 0.6f,
			EncounterChanceMultiplier = 1.4f,
			AmbushChanceMultiplier = 1.3f,
			VisionRangeMultiplier = 0.8f,
			ForagingChanceMultiplier = 0.3f,
			MoraleModifierPerHour = -2.0f,
			RestEffectivenessMultiplier = 0.5f,
			EnvironmentalDamagePerHour = 0.3f,
			EnvironmentalDamageType = DamageType.Necrotic,
			WaterAvailability = 0.2f,
			Tags = [Ids.Tags.Element.Necrotic],
			MapColor = "#2F2F4F",
			DisplayOrder = 144
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Supernatural.Magical,
			name: "Magical Terrain",
			description: "Infused with arcane energy.",
			movementSpeed: 0.9f,
			staminaDrain: 1.0f,
			fatigueRate: 0.9f
		) {
			Category = TerrainCategory.Supernatural,
			BaseDifficulty = 0.4f,
			EncounterChanceMultiplier = 1.1f,
			AmbushChanceMultiplier = 0.9f,
			VisionRangeMultiplier = 1.0f,
			ForagingChanceMultiplier = 1.2f,
			MoraleModifierPerHour = 0.5f,
			RestEffectivenessMultiplier = 1.2f,
			WaterAvailability = 0.5f,
			ResourceCategories = [TerrainResourceCategory.Magical, TerrainResourceCategory.Herbs],
			Tags = [Ids.Tags.Element.Arcane],
			MapColor = "#9370DB",
			DisplayOrder = 145
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Supernatural.FeyCrossing,
			name: "Fey Crossing",
			description: "Thin barrier to the fairy realm.",
			movementSpeed: 0.85f,
			staminaDrain: 1.1f,
			fatigueRate: 1.0f
		) {
			Category = TerrainCategory.Supernatural,
			BaseDifficulty = 0.5f,
			EncounterChanceMultiplier = 1.3f,
			AmbushChanceMultiplier = 1.2f,
			VisionRangeMultiplier = 0.8f,
			ForagingChanceMultiplier = 1.5f,
			MoraleModifierPerHour = 0.8f,
			RestEffectivenessMultiplier = 1.3f,
			WaterAvailability = 0.7f,
			ResourceCategories = [TerrainResourceCategory.Magical, TerrainResourceCategory.Herbs, TerrainResourceCategory.Forage],
			Tags = [Ids.Tags.Creature.Fey, Ids.Tags.Element.Nature],
			MapColor = "#00FA9A",
			DisplayOrder = 146
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Supernatural.LeyLine,
			name: "Ley Line",
			description: "Magical power concentration point.",
			movementSpeed: 1.0f,
			staminaDrain: 0.8f,
			fatigueRate: 0.7f
		) {
			Category = TerrainCategory.Supernatural,
			BaseDifficulty = 0.35f,
			EncounterChanceMultiplier = 1.0f,
			AmbushChanceMultiplier = 0.8f,
			VisionRangeMultiplier = 1.1f,
			MoraleModifierPerHour = 1.0f,
			RestEffectivenessMultiplier = 1.4f,
			WaterAvailability = 0.5f,
			ResourceCategories = [TerrainResourceCategory.Magical],
			Tags = [Ids.Tags.Element.Arcane],
			MapColor = "#7B68EE",
			DisplayOrder = 147
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Supernatural.VoidTouched,
			name: "Void Touched",
			description: "Reality is unstable and dangerous.",
			movementSpeed: 0.5f,
			staminaDrain: 2.0f,
			fatigueRate: 1.8f
		) {
			Category = TerrainCategory.Supernatural,
			BaseDifficulty = 0.9f,
			BlocksLineOfSight = true,
			EncounterChanceMultiplier = 1.8f,
			AmbushChanceMultiplier = 1.6f,
			VisionRangeMultiplier = 0.4f,
			ForagingChanceMultiplier = 0f,
			MoraleModifierPerHour = -3.0f,
			EnvironmentalDamagePerHour = 1.0f,
			EnvironmentalDamageType = DamageType.Force,
			WaterAvailability = 0f,
			ResourceCategories = [TerrainResourceCategory.Magical],
			Tags = [Ids.Tags.Element.Void, Ids.Tags.Fog.Corrupted],
			MapColor = "#0D0D0D",
			DisplayOrder = 148
		});
	}

	#endregion

	#region Special

	private void RegisterSpecial(GameDb db) {
		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Special.Fog,
			name: "Fog",
			description: "The edge of explored territory.",
			movementSpeed: 0.6f,
			staminaDrain: 1.5f,
			fatigueRate: 1.4f
		) {
			Category = TerrainCategory.Supernatural,
			BaseDifficulty = 0.7f,
			BlocksLineOfSight = true,
			EncounterChanceMultiplier = 1.5f,
			AmbushChanceMultiplier = 1.8f,
			VisionRangeMultiplier = 0.2f,
			ForagingChanceMultiplier = 0.3f,
			MoraleModifierPerHour = -2.0f,
			EnvironmentalDamagePerHour = 0.5f,
			EnvironmentalDamageType = DamageType.Necrotic,
			WaterAvailability = 0.3f,
			Tags = [Ids.Tags.Fog.Dense],
			MapColor = "#696969",
			DisplayOrder = 150
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Special.Blocked,
			name: "Blocked",
			description: "Impassable terrain.",
			movementSpeed: 0f,
			staminaDrain: 0f,
			fatigueRate: 0f
		) {
			Category = TerrainCategory.Natural,
			BaseDifficulty = 1.0f,
			IsPassable = false,
			MapColor = "#000000",
			DisplayOrder = 151
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Special.Portal,
			name: "Portal",
			description: "Magical gateway to another location.",
			movementSpeed: 1.5f,
			staminaDrain: 0.5f,
			fatigueRate: 0.5f
		) {
			Category = TerrainCategory.Supernatural,
			BaseDifficulty = 0.2f,
			EncounterChanceMultiplier = 0.5f,
			VisionRangeMultiplier = 1.0f,
			MoraleModifierPerHour = 0.5f,
			Tags = [Ids.Tags.Element.Arcane],
			MapColor = "#8A2BE2",
			DisplayOrder = 152
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Special.Sanctuary,
			name: "Sanctuary",
			description: "Protected safe zone.",
			movementSpeed: 1.2f,
			staminaDrain: 0.5f,
			fatigueRate: 0.4f
		) {
			Category = TerrainCategory.Settlement,
			BaseDifficulty = 0.01f,
			EncounterChanceMultiplier = 0f,
			AmbushChanceMultiplier = 0f,
			VisionRangeMultiplier = 1.3f,
			WaterAvailability = 1.0f,
			MoraleModifierPerHour = 3.0f,
			RestEffectivenessMultiplier = 2.0f,
			ProvidesNaturalShelter = true,
			Tags = [Ids.Tags.Element.Holy],
			MapColor = "#FFFACD",
			DisplayOrder = 153
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Special.Arena,
			name: "Arena",
			description: "Combat arena for battles.",
			movementSpeed: 1.1f,
			staminaDrain: 0.9f,
			fatigueRate: 0.85f
		) {
			Category = TerrainCategory.Settlement,
			BaseDifficulty = 0.3f,
			EncounterChanceMultiplier = 2.0f,
			AmbushChanceMultiplier = 0f,
			VisionRangeMultiplier = 1.5f,
			ProvidesNaturalShelter = true,
			MapColor = "#B22222",
			DisplayOrder = 154
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Special.Crossroads,
			name: "Crossroads",
			description: "Intersection of paths, often magical.",
			movementSpeed: 1.2f,
			staminaDrain: 0.8f,
			fatigueRate: 0.75f
		) {
			Category = TerrainCategory.Constructed,
			BaseDifficulty = 0.2f,
			EncounterChanceMultiplier = 1.2f,
			AmbushChanceMultiplier = 0.8f,
			VisionRangeMultiplier = 1.3f,
			WaterAvailability = 0.4f,
			Tags = [Ids.Tags.Element.Arcane],
			MapColor = "#DAA520",
			DisplayOrder = 155
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Special.Campsite,
			name: "Campsite",
			description: "Established rest area.",
			movementSpeed: 1.1f,
			staminaDrain: 0.7f,
			fatigueRate: 0.6f
		) {
			Category = TerrainCategory.Constructed,
			BaseDifficulty = 0.1f,
			EncounterChanceMultiplier = 0.5f,
			AmbushChanceMultiplier = 0.3f,
			VisionRangeMultiplier = 1.1f,
			WaterAvailability = 0.7f,
			MoraleModifierPerHour = 1.5f,
			RestEffectivenessMultiplier = 1.5f,
			ProvidesNaturalShelter = true,
			MapColor = "#CD853F",
			DisplayOrder = 156
		});

		db.RegisterProto(new TerrainProto(
			id: Ids.Terrains.Special.Wayshrine,
			name: "Wayshrine",
			description: "Fast travel point with blessings.",
			movementSpeed: 1.3f,
			staminaDrain: 0.6f,
			fatigueRate: 0.5f
		) {
			Category = TerrainCategory.Settlement,
			BaseDifficulty = 0.05f,
			EncounterChanceMultiplier = 0.3f,
			AmbushChanceMultiplier = 0.1f,
			VisionRangeMultiplier = 1.2f,
			WaterAvailability = 0.8f,
			MoraleModifierPerHour = 2.0f,
			RestEffectivenessMultiplier = 1.6f,
			ProvidesNaturalShelter = true,
			Tags = [Ids.Tags.Element.Holy],
			MapColor = "#FFD700",
			DisplayOrder = 157
		});
	}

	#endregion
}
	
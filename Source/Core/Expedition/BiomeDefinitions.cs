using RPGGame.Core.Prototypes;
using RPGGame.Core.Prototypes.Expedition;

namespace RPGGame.Core.Expedition;

/// <summary>
/// Registers all biome prototypes for procedural path generation.
/// Biomes define terrain distribution, danger levels, and content spawning rules.
/// </summary>
public class BiomeDefinitions : ICoreData {
	public void GameData(GameDb db) {
		RegisterEarlyGameBiomes(db);
		RegisterMidGameBiomes(db);
		RegisterLateGameBiomes(db);
		RegisterEndGameBiomes(db);
		RegisterBranchOnlyBiomes(db);
	}

	#region Early Game Biomes (Progression 0-2)

	private void RegisterEarlyGameBiomes(GameDb db) {
		// ═══════════════════════════════════════════════════════════════
		// OUTSKIRTS - Starting area around Haven Village
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new BiomeProto(
			id: Ids.Biomes.Outskirts,
			name: "Village Outskirts",
			description: "The familiar roads and farms surrounding Haven Village.",
			primaryTerrain: Ids.Terrains.Roads.Path,
			progressionOrder: 0,
			dangerLevel: 0.1f
		) {
			MinLength = 3,
			MaxLength = 5,
			VariantTerrains = [
				new WeightedTerrain(Ids.Terrains.Roads.Road, 2.0f),
				new WeightedTerrain(Ids.Terrains.Plains.Farmland, 1.5f),
				new WeightedTerrain(Ids.Terrains.Plains.Grass, 1.0f)
			],
			VariantTerrainChance = 0.4f,
			BranchChanceMultiplier = 0.5f,
			DungeonChanceBonus = 0f,
			LandmarkChanceMultiplier = 0.5f,
			EventDensityMultiplier = 0.7f,
			ResourceNodeMultiplier = 0.8f,
			CampsiteSpacingMultiplier = 0.5f,
			ValidNextBiomes = [Ids.Biomes.Farmlands, Ids.Biomes.Grasslands],
			CanAppearInBranches = false,
			TransitionText = "You set out from the safety of Haven Village...",
			AmbientDescription = "The familiar sights and sounds of home surround you.",
			MusicId = "music_village_outskirts",
			AmbientSoundId = "ambient_countryside",
			Tags = [Ids.Tags.Region.Plains],
			PreferredEncounters = [
				Ids.Encounters.Random.Rats,
				Ids.Encounters.Random.WildBeast
			],
			PreferredEvents = [
				Ids.Events.Random.MerchantRoad,
				Ids.Events.Random.WoundedTraveler
			]
		});

		// ═══════════════════════════════════════════════════════════════
		// FARMLANDS - Cultivated fields with occasional threats
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new BiomeProto(
			id: Ids.Biomes.Farmlands,
			name: "Farmlands",
			description: "Cultivated fields stretching toward the horizon.",
			primaryTerrain: Ids.Terrains.Plains.Farmland,
			progressionOrder: 1,
			dangerLevel: 0.15f
		) {
			MinLength = 4,
			MaxLength = 7,
			VariantTerrains = [
				new WeightedTerrain(Ids.Terrains.Roads.Path, 1.5f),
				new WeightedTerrain(Ids.Terrains.Plains.Grass, 1.2f),
				new WeightedTerrain(Ids.Terrains.Plains.Meadow, 0.8f),
				new WeightedTerrain(Ids.Terrains.Forests.Orchard, 0.5f)
			],
			VariantTerrainChance = 0.35f,
			BranchChanceMultiplier = 0.7f,
			DungeonChanceBonus = 0f,
			LandmarkChanceMultiplier = 0.8f,
			EventDensityMultiplier = 0.9f,
			ResourceNodeMultiplier = 1.2f,
			CampsiteSpacingMultiplier = 0.7f,
			ValidNextBiomes = [Ids.Biomes.Grasslands, Ids.Biomes.LightWoods],
			TransitionText = "The village fades behind you as farmlands spread before you.",
			AmbientDescription = "Golden fields sway in the breeze. Farmers wave from distant homesteads.",
			MusicId = "music_farmlands",
			AmbientSoundId = "ambient_farm",
			Tags = [Ids.Tags.Region.Plains],
			PreferredEncounters = [
				Ids.Encounters.Random.Rats,
				Ids.Encounters.Random.WildBeast,
				Ids.Encounters.Random.BanditsSmall
			],
			PreferredEvents = [
				Ids.Events.Random.MerchantRoad,
				Ids.Events.Random.WoundedTraveler,
				Ids.Events.Random.AbandonedCamp,
				Ids.Events.Random.HerbPatch
			]
		});

		// ═══════════════════════════════════════════════════════════════
		// GRASSLANDS - Open terrain with good visibility
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new BiomeProto(
			id: Ids.Biomes.Grasslands,
			name: "Grasslands",
			description: "Open grasslands with gentle rolling terrain.",
			primaryTerrain: Ids.Terrains.Plains.Grass,
			progressionOrder: 1,
			dangerLevel: 0.2f
		) {
			MinLength = 5,
			MaxLength = 9,
			VariantTerrains = [
				new WeightedTerrain(Ids.Terrains.Plains.OpenPlains, 1.5f),
				new WeightedTerrain(Ids.Terrains.Plains.Meadow, 1.2f),
				new WeightedTerrain(Ids.Terrains.Plains.Hills, 0.8f),
				new WeightedTerrain(Ids.Terrains.Roads.Trail, 0.5f)
			],
			VariantTerrainChance = 0.4f,
			BranchChanceMultiplier = 0.8f,
			DungeonChanceBonus = 0.05f,
			LandmarkChanceMultiplier = 1.0f,
			EventDensityMultiplier = 1.0f,
			ResourceNodeMultiplier = 1.0f,
			CampsiteSpacingMultiplier = 0.8f,
			ValidNextBiomes = [Ids.Biomes.LightWoods, Ids.Biomes.RollingHills, Ids.Biomes.RiverValley],
			TransitionText = "The land opens up into vast grasslands.",
			AmbientDescription = "Tall grass waves like an ocean. The sky stretches endlessly above.",
			MusicId = "music_grasslands",
			AmbientSoundId = "ambient_plains",
			Tags = [Ids.Tags.Region.Plains, Ids.Tags.Region.Grassland],
			PreferredEncounters = [
				Ids.Encounters.Random.Wolves,
				Ids.Encounters.Random.WildBeast,
				Ids.Encounters.Random.WildBoar,
				Ids.Encounters.Random.BanditsSmall
			],
			PreferredEvents = [
				Ids.Events.Random.BeautifulSunset,
				Ids.Events.Random.WildAnimal,
				Ids.Events.Random.StandingStones,
				Ids.Events.Random.HerbPatch
			]
		});

		// ═══════════════════════════════════════════════════════════════
		// LIGHT WOODS - First taste of the wilderness
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new BiomeProto(
			id: Ids.Biomes.LightWoods,
			name: "Light Woods",
			description: "Light deciduous forest with dappled sunlight.",
			primaryTerrain: Ids.Terrains.Forests.Forest,
			progressionOrder: 2,
			dangerLevel: 0.25f
		) {
			MinLength = 5,
			MaxLength = 10,
			VariantTerrains = [
				new WeightedTerrain(Ids.Terrains.Roads.Trail, 1.5f),
				new WeightedTerrain(Ids.Terrains.Plains.Grass, 1.0f),
				new WeightedTerrain(Ids.Terrains.Plains.Meadow, 0.8f),
				new WeightedTerrain(Ids.Terrains.Water.Pond, 0.3f)
			],
			VariantTerrainChance = 0.35f,
			BranchChanceMultiplier = 1.0f,
			DungeonChanceBonus = 0.1f,
			LandmarkChanceMultiplier = 1.1f,
			EventDensityMultiplier = 1.1f,
			ResourceNodeMultiplier = 1.3f,
			CampsiteSpacingMultiplier = 0.9f,
			ValidNextBiomes = [Ids.Biomes.DeepForest, Ids.Biomes.RollingHills, Ids.Biomes.RiverValley],
			TransitionText = "Trees begin to cluster, and the path winds into the woods.",
			AmbientDescription = "Birdsong fills the air. Sunlight filters through the canopy.",
			MusicId = "music_light_forest",
			AmbientSoundId = "ambient_forest_light",
			Tags = [Ids.Tags.Region.Forest],
			AllowedDungeons = [
				Ids.Locations.Dungeons.SpiderDen,
				Ids.Locations.Dungeons.WolfCave,
				Ids.Locations.Dungeons.BanditHideout
			],
			PreferredEncounters = [
				Ids.Encounters.Random.Wolves,
				Ids.Encounters.Random.WolfPack,
				Ids.Encounters.Random.GiantSpiders,
				Ids.Encounters.Random.BanditsSmall,
				Ids.Encounters.Random.Bear
			],
			PreferredEvents = [
				Ids.Events.Random.HerbPatch,
				Ids.Events.Random.WildAnimal,
				Ids.Events.Random.WolfAmbush,
				Ids.Events.Random.HiddenCave,
				Ids.Events.Random.MushroomRing
			]
		});
	}

	#endregion

	#region Mid Game Biomes (Progression 3-5)

	private void RegisterMidGameBiomes(GameDb db) {
		// ═══════════════════════════════════════════════════════════════
		// DEEP FOREST - Dense, dangerous woodland
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new BiomeProto(
			id: Ids.Biomes.DeepForest,
			name: "Deep Forest",
			description: "Dense, ancient woodland where sunlight barely penetrates.",
			primaryTerrain: Ids.Terrains.Forests.DeepForest,
			progressionOrder: 3,
			dangerLevel: 0.4f
		) {
			MinLength = 6,
			MaxLength = 12,
			VariantTerrains = [
				new WeightedTerrain(Ids.Terrains.Forests.Forest, 1.5f),
				new WeightedTerrain(Ids.Terrains.Roads.Trail, 1.0f),
				new WeightedTerrain(Ids.Terrains.Wetlands.Marsh, 0.6f),
				new WeightedTerrain(Ids.Terrains.Water.Pond, 0.4f)
			],
			VariantTerrainChance = 0.3f,
			BranchChanceMultiplier = 1.2f,
			DungeonChanceBonus = 0.15f,
			LandmarkChanceMultiplier = 1.2f,
			EventDensityMultiplier = 1.3f,
			ResourceNodeMultiplier = 1.4f,
			CampsiteSpacingMultiplier = 1.0f,
			ValidNextBiomes = [Ids.Biomes.AncientWoods, Ids.Biomes.Wetlands, Ids.Biomes.Mountains],
			TransitionText = "The trees grow denser, blocking out the sky.",
			AmbientDescription = "Ancient trees tower overhead. Strange sounds echo in the darkness.",
			MusicId = "music_deep_forest",
			AmbientSoundId = "ambient_forest_deep",
			Tags = [Ids.Tags.Region.DeepForest],
			AllowedDungeons = [
				Ids.Locations.Dungeons.SpiderDen,
				Ids.Locations.Dungeons.GoblinCave,
				Ids.Locations.Dungeons.BanditHideout,
				Ids.Locations.Dungeons.AncientRuins
			],
			PreferredEncounters = [
				Ids.Encounters.Random.WolfPack,
				Ids.Encounters.Random.GiantSpiders,
				Ids.Encounters.Random.SpiderNest,
				Ids.Encounters.Random.Bear,
				Ids.Encounters.Random.Goblins
			],
			PreferredEvents = [
				Ids.Events.Random.SpiderAmbush,
				Ids.Events.Random.MushroomRing,
				Ids.Events.Random.HiddenCave,
				Ids.Events.Random.GhostlyApparition,
				Ids.Events.Random.HermitWise
			]
		});

		// ═══════════════════════════════════════════════════════════════
		// WETLANDS - Treacherous swamps and marshes
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new BiomeProto(
			id: Ids.Biomes.Wetlands,
			name: "Wetlands",
			description: "Treacherous swamps and marshes filled with danger.",
			primaryTerrain: Ids.Terrains.Wetlands.Swamp,
			progressionOrder: 4,
			dangerLevel: 0.45f
		) {
			MinLength = 5,
			MaxLength = 10,
			VariantTerrains = [
				new WeightedTerrain(Ids.Terrains.Wetlands.Marsh, 1.5f),
				new WeightedTerrain(Ids.Terrains.Wetlands.Bog, 0.8f),
				new WeightedTerrain(Ids.Terrains.Forests.DeadForest, 0.6f),
				new WeightedTerrain(Ids.Terrains.Water.Shallows, 0.5f)
			],
			VariantTerrainChance = 0.4f,
			BranchChanceMultiplier = 0.8f,
			DungeonChanceBonus = 0.1f,
			LandmarkChanceMultiplier = 0.9f,
			EventDensityMultiplier = 1.4f,
			ResourceNodeMultiplier = 1.2f,
			CampsiteSpacingMultiplier = 1.3f,
			ValidNextBiomes = [Ids.Biomes.DarkMarsh, Ids.Biomes.DeepForest, Ids.Biomes.HauntedLands],
			TransitionText = "The ground grows soft and treacherous. Mist rises from stagnant water.",
			AmbientDescription = "Murky water and twisted trees surround you. The air is thick and foul.",
			MusicId = "music_swamp",
			AmbientSoundId = "ambient_swamp",
			Tags = [Ids.Tags.Region.Swamp],
			AllowedDungeons = [
				Ids.Locations.Dungeons.ForgottenCrypt,
				Ids.Locations.Dungeons.CultistLair
			],
			PreferredEncounters = [
				Ids.Encounters.Random.UndeadRising,
				Ids.Encounters.Random.SwampCreature,
				Ids.Encounters.Random.GiantSpiders
			],
			PreferredEvents = [
				Ids.Events.Random.SwampThing,
				Ids.Events.Random.GhostlyApparition,
				Ids.Events.Random.FogThickens,
				Ids.Events.Random.GraveDisturbance
			]
		});

		// ═══════════════════════════════════════════════════════════════
		// ROLLING HILLS - Elevated terrain with caves
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new BiomeProto(
			id: Ids.Biomes.RollingHills,
			name: "Rolling Hills",
			description: "Hilly terrain with caves and orc patrols.",
			primaryTerrain: Ids.Terrains.Plains.Hills,
			progressionOrder: 3,
			dangerLevel: 0.35f
		) {
			MinLength = 5,
			MaxLength = 9,
			VariantTerrains = [
				new WeightedTerrain(Ids.Terrains.Plains.Grass, 1.2f),
				new WeightedTerrain(Ids.Terrains.Mountains.Highland, 0.8f),
				new WeightedTerrain(Ids.Terrains.Roads.Trail, 0.6f),
				new WeightedTerrain(Ids.Terrains.Underground.Cave, 0.3f)
			],
			VariantTerrainChance = 0.35f,
			BranchChanceMultiplier = 1.1f,
			DungeonChanceBonus = 0.2f,
			LandmarkChanceMultiplier = 1.0f,
			EventDensityMultiplier = 1.0f,
			ResourceNodeMultiplier = 1.1f,
			CampsiteSpacingMultiplier = 0.9f,
			ValidNextBiomes = [Ids.Biomes.Mountains, Ids.Biomes.Highlands, Ids.Biomes.DeepForest],
			TransitionText = "The land rises into rolling hills dotted with rocky outcrops.",
			AmbientDescription = "Wind sweeps across the hillsides. Distant caves promise danger and treasure.",
			MusicId = "music_hills",
			AmbientSoundId = "ambient_hills",
			Tags = [Ids.Tags.Region.Hills],
			AllowedDungeons = [
				Ids.Locations.Dungeons.GoblinCave,
				Ids.Locations.Dungeons.OrcStronghold,
				Ids.Locations.Dungeons.AbandonedMine
			],
			PreferredEncounters = [
				Ids.Encounters.Random.OrcPatrol,
				Ids.Encounters.Random.Goblins,
				Ids.Encounters.Random.Wolves,
				Ids.Encounters.Random.Bear
			],
			PreferredEvents = [
				Ids.Events.Random.HiddenCave,
				Ids.Events.Random.OldBattlefield,
				Ids.Events.Random.StandingStones
			]
		});

		// ═══════════════════════════════════════════════════════════════
		// RIVER VALLEY - Water crossings and ambush points
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new BiomeProto(
			id: Ids.Biomes.RiverValley,
			name: "River Valley",
			description: "River valleys with bridges and ambush points.",
			primaryTerrain: Ids.Terrains.Plains.Grass,
			progressionOrder: 3,
			dangerLevel: 0.35f
		) {
			MinLength = 4,
			MaxLength = 8,
			VariantTerrains = [
				new WeightedTerrain(Ids.Terrains.Roads.Bridge, 1.0f),
				new WeightedTerrain(Ids.Terrains.Water.Shallows, 0.8f),
				new WeightedTerrain(Ids.Terrains.Forests.Forest, 0.7f),
				new WeightedTerrain(Ids.Terrains.Water.Waterfall, 0.2f)
			],
			VariantTerrainChance = 0.45f,
			BranchChanceMultiplier = 0.9f,
			DungeonChanceBonus = 0.1f,
			LandmarkChanceMultiplier = 1.2f,
			EventDensityMultiplier = 1.2f,
			ResourceNodeMultiplier = 1.3f,
			CampsiteSpacingMultiplier = 0.8f,
			ValidNextBiomes = [Ids.Biomes.Wetlands, Ids.Biomes.DeepForest, Ids.Biomes.RollingHills],
			TransitionText = "A river carves through the landscape, its waters glinting in the light.",
			AmbientDescription = "The sound of rushing water fills the air. Bridges span treacherous crossings.",
			MusicId = "music_river",
			AmbientSoundId = "ambient_river",
			Tags = [Ids.Tags.Region.Plains],
			PreferredEncounters = [
				Ids.Encounters.Random.BanditsMixed,
				Ids.Encounters.Random.Wolves
			],
			PreferredEvents = [
				Ids.Events.Random.FreshwaterSpring,
				Ids.Events.Random.BanditDemand,
				Ids.Events.Random.MerchantRoad
			]
		});

		// ═══════════════════════════════════════════════════════════════
		// ANCIENT WOODS - Fey-touched primeval forest
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new BiomeProto(
			id: Ids.Biomes.AncientWoods,
			name: "Ancient Woods",
			description: "Primeval forest touched by ancient magic.",
			primaryTerrain: Ids.Terrains.Forests.AncientForest,
			progressionOrder: 5,
			dangerLevel: 0.5f
		) {
			MinLength = 6,
			MaxLength = 11,
			VariantTerrains = [
				new WeightedTerrain(Ids.Terrains.Forests.DeepForest, 1.2f),
				new WeightedTerrain(Ids.Terrains.Supernatural.FeyCrossing, 0.5f),
				new WeightedTerrain(Ids.Terrains.Supernatural.Magical, 0.4f),
				new WeightedTerrain(Ids.Terrains.Water.Pond, 0.3f)
			],
			VariantTerrainChance = 0.35f,
			BranchChanceMultiplier = 1.3f,
			DungeonChanceBonus = 0.15f,
			LandmarkChanceMultiplier = 1.4f,
			EventDensityMultiplier = 1.3f,
			ResourceNodeMultiplier = 1.5f,
			CampsiteSpacingMultiplier = 1.1f,
			ValidNextBiomes = [Ids.Biomes.Mountains, Ids.Biomes.HauntedLands, Ids.Biomes.CorruptedLands],
			TransitionText = "The forest grows ancient. Magic thrums in the very air.",
			AmbientDescription = "Massive trees older than memory tower above. Spirits whisper in the wind.",
			MusicId = "music_ancient_forest",
			AmbientSoundId = "ambient_forest_ancient",
			Tags = [Ids.Tags.Region.DeepForest, Ids.Tags.Element.Nature, Ids.Tags.Creature.Fey],
			AllowedDungeons = [
				Ids.Locations.Dungeons.AncientRuins,
				Ids.Locations.Dungeons.CursedTemple
			],
			PreferredEncounters = [
				Ids.Encounters.Random.WolfPack,
				Ids.Encounters.Random.SpiderNest
			],
			PreferredEvents = [
				Ids.Events.Random.ManaWellBlessing,
				Ids.Events.Random.ElderTreeSpirit,
				Ids.Events.Random.GhostlyApparition,
				Ids.Events.Random.AncientShrine
			]
		});
	}

	#endregion

	#region Late Game Biomes (Progression 6-8)

	private void RegisterLateGameBiomes(GameDb db) {
		// ═══════════════════════════════════════════════════════════════
		// MOUNTAINS - Harsh alpine terrain
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new BiomeProto(
			id: Ids.Biomes.Mountains,
			name: "Mountains",
			description: "Rugged mountain terrain with harsh conditions.",
			primaryTerrain: Ids.Terrains.Mountains.Mountain,
			progressionOrder: 6,
			dangerLevel: 0.55f
		) {
			MinLength = 6,
			MaxLength = 10,
			VariantTerrains = [
				new WeightedTerrain(Ids.Terrains.Mountains.Pass, 1.5f),
				new WeightedTerrain(Ids.Terrains.Mountains.Highland, 1.0f),
				new WeightedTerrain(Ids.Terrains.Underground.Cave, 0.6f),
				new WeightedTerrain(Ids.Terrains.Frozen.Snow, 0.4f)
			],
			VariantTerrainChance = 0.4f,
			BranchChanceMultiplier = 1.0f,
			DungeonChanceBonus = 0.25f,
			LandmarkChanceMultiplier = 1.1f,
			EventDensityMultiplier = 0.9f,
			ResourceNodeMultiplier = 1.2f,
			CampsiteSpacingMultiplier = 1.2f,
			ValidNextBiomes = [Ids.Biomes.Highlands, Ids.Biomes.HauntedLands, Ids.Biomes.CorruptedLands],
			TransitionText = "The path climbs into the mountains. The air grows thin and cold.",
			AmbientDescription = "Jagged peaks pierce the clouds. The wind howls through narrow passes.",
			MusicId = "music_mountains",
			AmbientSoundId = "ambient_mountain",
			Tags = [Ids.Tags.Region.Mountain],
			AllowedDungeons = [
				Ids.Locations.Dungeons.DeepMine,
				Ids.Locations.Dungeons.OrcStronghold,
				Ids.Locations.Dungeons.CrystalCavern
			],
			PreferredEncounters = [
				Ids.Encounters.Random.OrcPatrol,
				Ids.Encounters.Random.Bear,
				Ids.Encounters.Random.Wyvern
			],
			PreferredEvents = [
				Ids.Events.Random.Earthquake,
				Ids.Events.Random.HiddenCave,
				Ids.Events.Random.MithrilGuardian
			]
		});

		// ═══════════════════════════════════════════════════════════════
		// HIGHLANDS - Elevated plateau with exposure
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new BiomeProto(
			id: Ids.Biomes.Highlands,
			name: "Highlands",
			description: "Elevated plateau with strong winds and exposure.",
			primaryTerrain: Ids.Terrains.Mountains.Highland,
			progressionOrder: 6,
			dangerLevel: 0.5f
		) {
			MinLength = 5,
			MaxLength = 9,
			VariantTerrains = [
				new WeightedTerrain(Ids.Terrains.Plains.Hills, 1.2f),
				new WeightedTerrain(Ids.Terrains.Mountains.Mountain, 0.8f),
				new WeightedTerrain(Ids.Terrains.Frozen.Tundra, 0.5f),
				new WeightedTerrain(Ids.Terrains.Ruins.Monument, 0.3f)
			],
			VariantTerrainChance = 0.35f,
			BranchChanceMultiplier = 0.9f,
			DungeonChanceBonus = 0.15f,
			LandmarkChanceMultiplier = 1.3f,
			EventDensityMultiplier = 1.0f,
			ResourceNodeMultiplier = 0.9f,
			CampsiteSpacingMultiplier = 1.1f,
			ValidNextBiomes = [Ids.Biomes.Mountains, Ids.Biomes.Badlands, Ids.Biomes.HauntedLands],
			TransitionText = "You emerge onto a windswept plateau high above the world.",
			AmbientDescription = "The wind never stops here. Ancient stones mark forgotten paths.",
			MusicId = "music_highlands",
			AmbientSoundId = "ambient_wind",
			Tags = [Ids.Tags.Region.Hills, Ids.Tags.Region.Mountain],
			PreferredEncounters = [
				Ids.Encounters.Random.Wyvern,
				Ids.Encounters.Random.OrcPatrol
			],
			PreferredEvents = [
				Ids.Events.Random.StandingStones,
				Ids.Events.Random.StormApproaching,
				Ids.Events.Random.AncientShrine
			]
		});

		// ═══════════════════════════════════════════════════════════════
		// HAUNTED LANDS - Cursed undead territory
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new BiomeProto(
			id: Ids.Biomes.HauntedLands,
			name: "Haunted Lands",
			description: "Cursed lands filled with undead and dark magic.",
			primaryTerrain: Ids.Terrains.Ruins.Graveyard,
			progressionOrder: 7,
			dangerLevel: 0.65f
		) {
			MinLength = 6,
			MaxLength = 11,
			VariantTerrains = [
				new WeightedTerrain(Ids.Terrains.Forests.DeadForest, 1.5f),
				new WeightedTerrain(Ids.Terrains.Ruins.OpenRuins, 1.2f),
				new WeightedTerrain(Ids.Terrains.Supernatural.CursedGround, 0.8f),
				new WeightedTerrain(Ids.Terrains.Ruins.Battlefield, 0.5f)
			],
			VariantTerrainChance = 0.45f,
			BranchChanceMultiplier = 1.1f,
			DungeonChanceBonus = 0.3f,
			LandmarkChanceMultiplier = 1.2f,
			EventDensityMultiplier = 1.4f,
			ResourceNodeMultiplier = 0.7f,
			CampsiteSpacingMultiplier = 1.3f,
			ValidNextBiomes = [Ids.Biomes.CorruptedLands, Ids.Biomes.DarkMarsh, Ids.Biomes.FogBorder],
			TransitionText = "A chill settles in your bones. The land itself feels dead.",
			AmbientDescription = "Tombstones and dead trees stretch in all directions. Whispers haunt the wind.",
			MusicId = "music_haunted",
			AmbientSoundId = "ambient_haunted",
			Tags = [Ids.Tags.Region.Graveyard, Ids.Tags.Creature.Undead, Ids.Tags.Element.Necrotic],
			AllowedDungeons = [
				Ids.Locations.Dungeons.ForgottenCrypt,
				Ids.Locations.Dungeons.AncientCatacombs,
				Ids.Locations.Dungeons.CursedMausoleum,
				Ids.Locations.Dungeons.LichSanctum
			],
			PreferredEncounters = [
				Ids.Encounters.Random.UndeadRising,
				Ids.Encounters.Random.Haunted,
				Ids.Encounters.Random.Undead
			],
			PreferredEvents = [
				Ids.Events.Random.GraveDisturbance,
				Ids.Events.Random.GhostlyApparition,
				Ids.Events.Random.DemonWhisper,
				Ids.Events.Random.TreasureCurse
			]
		});

		// ═══════════════════════════════════════════════════════════════
		// BADLANDS - Scorched wasteland
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new BiomeProto(
			id: Ids.Biomes.Badlands,
			name: "Badlands",
			description: "Barren wasteland with scorched earth and desperate creatures.",
			primaryTerrain: Ids.Terrains.Desert.Badlands,
			progressionOrder: 7,
			dangerLevel: 0.6f
		) {
			MinLength = 5,
			MaxLength = 9,
			VariantTerrains = [
				new WeightedTerrain(Ids.Terrains.Desert.Canyon, 1.0f),
				new WeightedTerrain(Ids.Terrains.Volcanic.AshWastes, 0.8f),
				new WeightedTerrain(Ids.Terrains.Supernatural.Wasteland, 0.6f),
				new WeightedTerrain(Ids.Terrains.Desert.Mesa, 0.4f)
			],
			VariantTerrainChance = 0.4f,
			BranchChanceMultiplier = 0.8f,
			DungeonChanceBonus = 0.2f,
			LandmarkChanceMultiplier = 0.8f,
			EventDensityMultiplier = 1.1f,
			ResourceNodeMultiplier = 0.6f,
			CampsiteSpacingMultiplier = 1.4f,
			ValidNextBiomes = [Ids.Biomes.CorruptedLands, Ids.Biomes.FogBorder],
			TransitionText = "Life fades from the land. Only the desperate survive here.",
			AmbientDescription = "Cracked earth stretches to the horizon. Nothing grows. Nothing lives.",
			MusicId = "music_badlands",
			AmbientSoundId = "ambient_desert_wind",
			Tags = [Ids.Tags.Region.Desert],
			AllowedDungeons = [
				Ids.Locations.Dungeons.CollapsingMine,
				Ids.Locations.Dungeons.CultistLair
			],
			PreferredEncounters = [
				Ids.Encounters.Random.CultistRitual,
				Ids.Encounters.Random.Hellhounds
			],
			PreferredEvents = [
				Ids.Events.Random.Exhaustion,
				Ids.Events.Random.MysteriousStranger,
				Ids.Events.Random.DemonWhisper
			]
		});

		// ═══════════════════════════════════════════════════════════════
		// DARK MARSH - Corrupted swampland
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new BiomeProto(
			id: Ids.Biomes.DarkMarsh,
			name: "Dark Marsh",
			description: "Corrupted swampland near the fog's edge.",
			primaryTerrain: Ids.Terrains.Wetlands.Swamp,
			progressionOrder: 8,
			dangerLevel: 0.7f
		) {
			MinLength = 5,
			MaxLength = 9,
			VariantTerrains = [
				new WeightedTerrain(Ids.Terrains.Wetlands.Bog, 1.2f),
				new WeightedTerrain(Ids.Terrains.Forests.DeadForest, 1.0f),
				new WeightedTerrain(Ids.Terrains.Supernatural.Corrupted, 0.8f),
				new WeightedTerrain(Ids.Terrains.Supernatural.CursedGround, 0.5f)
			],
			VariantTerrainChance = 0.45f,
			BranchChanceMultiplier = 0.7f,
			DungeonChanceBonus = 0.25f,
			LandmarkChanceMultiplier = 0.9f,
			EventDensityMultiplier = 1.5f,
			ResourceNodeMultiplier = 0.5f,
			CampsiteSpacingMultiplier = 1.4f,
			ValidNextBiomes = [Ids.Biomes.FogBorder, Ids.Biomes.CorruptedLands],
			TransitionText = "The swamp grows darker. Corruption seeps from the very water.",
			AmbientDescription = "Twisted shapes move beneath the surface. The fog creeps ever closer.",
			MusicId = "music_dark_marsh",
			AmbientSoundId = "ambient_swamp_dark",
			Tags = [Ids.Tags.Region.Swamp, Ids.Tags.Fog.Corrupted],
			AllowedDungeons = [
				Ids.Locations.Dungeons.CultistLair,
				Ids.Locations.Dungeons.ForgottenCrypt
			],
			PreferredEncounters = [
				Ids.Encounters.Random.UndeadRising,
				Ids.Encounters.Random.SwampCreature,
				Ids.Encounters.Random.CultistRitual
			],
			PreferredEvents = [
				Ids.Events.Random.FogWhispers,
				Ids.Events.Random.SwampThing,
				Ids.Events.Random.GhostlyApparition,
				Ids.Events.Random.CorruptionSpread
			]
		});
	}

	#endregion

	#region End Game Biomes (Progression 9+)

	private void RegisterEndGameBiomes(GameDb db) {
		// ═══════════════════════════════════════════════════════════════
		// CORRUPTED LANDS - Fog-twisted terrain
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new BiomeProto(
			id: Ids.Biomes.CorruptedLands,
			name: "Corrupted Lands",
			description: "Lands twisted by the fog's corruption.",
			primaryTerrain: Ids.Terrains.Supernatural.Corrupted,
			progressionOrder: 9,
			dangerLevel: 0.8f
		) {
			MinLength = 6,
			MaxLength = 10,
			VariantTerrains = [
				new WeightedTerrain(Ids.Terrains.Supernatural.Blighted, 1.2f),
				new WeightedTerrain(Ids.Terrains.Supernatural.CursedGround, 1.0f),
				new WeightedTerrain(Ids.Terrains.Forests.DeadForest, 0.8f),
				new WeightedTerrain(Ids.Terrains.Supernatural.ShadowRealm, 0.4f)
			],
			VariantTerrainChance = 0.5f,
			BranchChanceMultiplier = 0.6f,
			DungeonChanceBonus = 0.3f,
			LandmarkChanceMultiplier = 1.0f,
			EventDensityMultiplier = 1.5f,
			ResourceNodeMultiplier = 0.4f,
			CampsiteSpacingMultiplier = 1.5f,
			ValidNextBiomes = [Ids.Biomes.FogBorder, Ids.Biomes.VoidWastes],
			TransitionText = "Reality itself seems wrong here. The fog's touch has changed everything.",
			AmbientDescription = "Colors are wrong. Shapes twist. The land itself rejects you.",
			MusicId = "music_corrupted",
			AmbientSoundId = "ambient_corruption",
			Tags = [Ids.Tags.Fog.Corrupted, Ids.Tags.Element.Necrotic],
			AllowedDungeons = [
				Ids.Locations.Dungeons.CursedTemple,
				Ids.Locations.Dungeons.FogNexus
			],
			PreferredEncounters = [
				Ids.Encounters.Story.FogCreatures,
				Ids.Encounters.Random.Haunted,
				Ids.Encounters.Random.CultistRitual
			],
			PreferredEvents = [
				Ids.Events.Random.FogWhispers,
				Ids.Events.Random.TimeAnomaly,
				Ids.Events.Random.CorruptionSpread,
				Ids.Events.Story.FogClueDiscovered
			]
		});

		// ═══════════════════════════════════════════════════════════════
		// FOG BORDER - Edge of the fog
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new BiomeProto(
			id: Ids.Biomes.FogBorder,
			name: "Fog Border",
			description: "The boundary where normal reality meets the fog.",
			primaryTerrain: Ids.Terrains.Special.Fog,
			progressionOrder: 10,
			dangerLevel: 0.85f
		) {
			MinLength = 4,
			MaxLength = 8,
			VariantTerrains = [
				new WeightedTerrain(Ids.Terrains.Supernatural.Corrupted, 1.5f),
				new WeightedTerrain(Ids.Terrains.Supernatural.VoidTouched, 0.6f),
				new WeightedTerrain(Ids.Terrains.Supernatural.ShadowRealm, 0.5f)
			],
			VariantTerrainChance = 0.4f,
			BranchChanceMultiplier = 0.4f,
			DungeonChanceBonus = 0.2f,
			LandmarkChanceMultiplier = 0.8f,
			EventDensityMultiplier = 1.6f,
			ResourceNodeMultiplier = 0.3f,
			CampsiteSpacingMultiplier = 1.6f,
			ValidNextBiomes = [Ids.Biomes.VoidWastes, Ids.Biomes.FogHeart],
			CanAppearInBranches = false,
			TransitionText = "The fog rises before you like a wall. Beyond lies the unknown.",
			AmbientDescription = "The boundary shimmers and pulses. Time itself feels uncertain.",
			MusicId = "music_fog_border",
			AmbientSoundId = "ambient_fog",
			Tags = [Ids.Tags.Fog.Dense, Ids.Tags.Fog.Corrupted],
			PreferredEncounters = [
				Ids.Encounters.Story.FogCreatures
			],
			PreferredEvents = [
				Ids.Events.Random.FogWhispers,
				Ids.Events.Random.TimeAnomaly,
				Ids.Events.Story.FogClueDiscovered
			]
		});

		// ═══════════════════════════════════════════════════════════════
		// VOID WASTES - Inside the fog
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new BiomeProto(
			id: Ids.Biomes.VoidWastes,
			name: "Void Wastes",
			description: "Reality-warped wastes within the fog itself.",
			primaryTerrain: Ids.Terrains.Supernatural.VoidTouched,
			progressionOrder: 11,
			dangerLevel: 0.95f
		) {
			MinLength = 5,
			MaxLength = 8,
			VariantTerrains = [
				new WeightedTerrain(Ids.Terrains.Supernatural.ShadowRealm, 1.0f),
				new WeightedTerrain(Ids.Terrains.Supernatural.Corrupted, 0.8f),
				new WeightedTerrain(Ids.Terrains.Special.Fog, 0.6f)
			],
			VariantTerrainChance = 0.5f,
			BranchChanceMultiplier = 0.3f,
			DungeonChanceBonus = 0.1f,
			LandmarkChanceMultiplier = 0.5f,
			EventDensityMultiplier = 1.8f,
			ResourceNodeMultiplier = 0.1f,
			CampsiteSpacingMultiplier = 2.0f,
			ValidNextBiomes = [Ids.Biomes.FogHeart],
			CanAppearInBranches = false,
			TransitionText = "You step into the fog. Reality dissolves around you.",
			AmbientDescription = "There is no sky. No ground. Only the fog and what lurks within.",
			MusicId = "music_void",
			AmbientSoundId = "ambient_void",
			Tags = [Ids.Tags.Fog.Suffocating, Ids.Tags.Element.Void],
			PreferredEncounters = [
				Ids.Encounters.Story.FogCreatures
			],
			PreferredEvents = [
				Ids.Events.Random.TimeAnomaly,
				Ids.Events.Random.FogWhispers,
				Ids.Events.Story.FogClueDiscovered
			]
		});

		// ═══════════════════════════════════════════════════════════════
		// FOG HEART - Final destination
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new BiomeProto(
			id: Ids.Biomes.FogHeart,
			name: "Heart of the Fog",
			description: "The source of the fog - your final destination.",
			primaryTerrain: Ids.Terrains.Supernatural.VoidTouched,
			progressionOrder: 12,
			dangerLevel: 1.0f
		) {
			MinLength = 3,
			MaxLength = 5,
			VariantTerrains = [
				new WeightedTerrain(Ids.Terrains.Supernatural.ShadowRealm, 1.0f),
				new WeightedTerrain(Ids.Terrains.Special.Fog, 0.5f)
			],
			VariantTerrainChance = 0.3f,
			BranchChanceMultiplier = 0f,
			DungeonChanceBonus = 0f,
			LandmarkChanceMultiplier = 0.2f,
			EventDensityMultiplier = 0.5f,
			ResourceNodeMultiplier = 0f,
			CampsiteSpacingMultiplier = 3.0f,
			ValidNextBiomes = [],
			CanAppearInBranches = false,
			TransitionText = "You have reached the source. The Herald awaits.",
			AmbientDescription = "This is where it all began. And where it must end.",
			MusicId = "music_final",
			AmbientSoundId = "ambient_heartbeat",
			Tags = [Ids.Tags.Fog.Suffocating, Ids.Tags.Element.Void]
		});
	}

	#endregion

	#region Branch-Only Biomes (Progression = -1)

	private void RegisterBranchOnlyBiomes(GameDb db) {
		// ═══════════════════════════════════════════════════════════════
		// CRYSTAL CAVERNS - Magical underground
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new BiomeProto(
			id: Ids.Biomes.CrystalCaverns,
			name: "Crystal Caverns",
			description: "Magical underground caverns filled with crystals.",
			primaryTerrain: Ids.Terrains.Underground.CrystalCavern,
			progressionOrder: -1,
			dangerLevel: 0.5f
		) {
			MinLength = 4,
			MaxLength = 8,
			VariantTerrains = [
				new WeightedTerrain(Ids.Terrains.Underground.Cavern, 1.2f),
				new WeightedTerrain(Ids.Terrains.Underground.UndergroundLake, 0.5f),
				new WeightedTerrain(Ids.Terrains.Supernatural.Magical, 0.4f)
			],
			VariantTerrainChance = 0.35f,
			BranchChanceMultiplier = 0.5f,
			DungeonChanceBonus = 0.3f,
			LandmarkChanceMultiplier = 1.5f,
			EventDensityMultiplier = 1.0f,
			ResourceNodeMultiplier = 2.0f,
			CampsiteSpacingMultiplier = 1.0f,
			CanAppearInBranches = true,
			TransitionText = "A hidden entrance leads deep underground...",
			AmbientDescription = "Crystals illuminate vast chambers with ethereal light.",
			MusicId = "music_crystal_cave",
			AmbientSoundId = "ambient_cave_crystal",
			Tags = [Ids.Tags.Region.Cave, Ids.Tags.Element.Arcane],
			AllowedDungeons = [
				Ids.Locations.Dungeons.CrystalCavern
			],
			PreferredEvents = [
				Ids.Events.Random.ManaWellBlessing,
				Ids.Events.Random.HiddenCache
			]
		});

		// ═══════════════════════════════════════════════════════════════
		// VOLCANIC RIFT - Extreme heat zone
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new BiomeProto(
			id: Ids.Biomes.VolcanicRift,
			name: "Volcanic Rift",
			description: "Volcanic fissure with extreme heat and rare ores.",
			primaryTerrain: Ids.Terrains.Volcanic.VolcanicPlain,
			progressionOrder: -1,
			dangerLevel: 0.7f
		) {
			MinLength = 3,
			MaxLength = 6,
			VariantTerrains = [
				new WeightedTerrain(Ids.Terrains.Volcanic.Geothermal, 1.0f),
				new WeightedTerrain(Ids.Terrains.Volcanic.ObsidianField, 0.7f),
				new WeightedTerrain(Ids.Terrains.Volcanic.LavaField, 0.3f)
			],
			VariantTerrainChance = 0.4f,
			BranchChanceMultiplier = 0.3f,
			DungeonChanceBonus = 0.2f,
			LandmarkChanceMultiplier = 0.8f,
			EventDensityMultiplier = 1.2f,
			ResourceNodeMultiplier = 1.8f,
			CampsiteSpacingMultiplier = 1.5f,
			CanAppearInBranches = true,
			TransitionText = "Heat rises from a volcanic fissure ahead...",
			AmbientDescription = "Lava glows in the depths. The air shimmers with heat.",
			MusicId = "music_volcanic",
			AmbientSoundId = "ambient_lava",
			Tags = [Ids.Tags.Region.Volcanic, Ids.Tags.Element.Fire],
			PreferredEncounters = [
				Ids.Encounters.Random.Hellhounds
			],
			PreferredEvents = [
				Ids.Events.Random.GeyserEruption,
				Ids.Events.Random.HotSpringRest
			]
		});

		// ═══════════════════════════════════════════════════════════════
		// FROZEN WASTES - Extreme cold zone
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new BiomeProto(
			id: Ids.Biomes.FrozenWastes,
			name: "Frozen Wastes",
			description: "Frozen tundra with blizzards and ice creatures.",
			primaryTerrain: Ids.Terrains.Frozen.Snow,
			progressionOrder: -1,
			dangerLevel: 0.6f
		) {
			MinLength = 4,
			MaxLength = 7,
			VariantTerrains = [
				new WeightedTerrain(Ids.Terrains.Frozen.Tundra, 1.2f),
				new WeightedTerrain(Ids.Terrains.Frozen.Ice, 0.8f),
				new WeightedTerrain(Ids.Terrains.Frozen.FrozenLake, 0.5f),
				new WeightedTerrain(Ids.Terrains.Frozen.Glacier, 0.3f)
			],
			VariantTerrainChance = 0.4f,
			BranchChanceMultiplier = 0.4f,
			DungeonChanceBonus = 0.15f,
			LandmarkChanceMultiplier = 0.7f,
			EventDensityMultiplier = 0.9f,
			ResourceNodeMultiplier = 0.6f,
			CampsiteSpacingMultiplier = 1.3f,
			CanAppearInBranches = true,
			TransitionText = "A bitter wind cuts through you as snow begins to fall...",
			AmbientDescription = "White stretches endlessly. The cold seeps into your bones.",
			MusicId = "music_frozen",
			AmbientSoundId = "ambient_blizzard",
			Tags = [Ids.Tags.Region.Arctic, Ids.Tags.Element.Cold],
			PreferredEvents = [
				Ids.Events.Random.StormApproaching,
				Ids.Events.Random.Exhaustion
			]
		});

		// ═══════════════════════════════════════════════════════════════
		// FEY WILDS - Magical forest pocket
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new BiomeProto(
			id: Ids.Biomes.FeyWilds,
			name: "Fey Wilds",
			description: "Fey-touched woodland with strange magic.",
			primaryTerrain: Ids.Terrains.Supernatural.FeyCrossing,
			progressionOrder: -1,
			dangerLevel: 0.45f
		) {
			MinLength = 3,
			MaxLength = 6,
			VariantTerrains = [
				new WeightedTerrain(Ids.Terrains.Forests.AncientForest, 1.2f),
				new WeightedTerrain(Ids.Terrains.Supernatural.Magical, 1.0f),
				new WeightedTerrain(Ids.Terrains.Plains.Meadow, 0.6f)
			],
			VariantTerrainChance = 0.45f,
			BranchChanceMultiplier = 0.6f,
			DungeonChanceBonus = 0.1f,
			LandmarkChanceMultiplier = 1.8f,
			EventDensityMultiplier = 1.5f,
			ResourceNodeMultiplier = 1.6f,
			CampsiteSpacingMultiplier = 0.8f,
			CanAppearInBranches = true,
			TransitionText = "The forest shimmers, and you step into another world...",
			AmbientDescription = "Colors are too bright. Music drifts from nowhere. Reality bends.",
			MusicId = "music_fey",
			AmbientSoundId = "ambient_fey",
			Tags = [Ids.Tags.Creature.Fey, Ids.Tags.Element.Nature, Ids.Tags.Element.Arcane],
			PreferredEvents = [
				Ids.Events.Random.MysteriousStranger,
				Ids.Events.Random.MushroomRing,
				Ids.Events.Random.ManaWellBlessing
			]
		});

		// ═══════════════════════════════════════════════════════════════
		// DEEP MINES - Abandoned mine complex
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new BiomeProto(
			id: Ids.Biomes.DeepMines,
			name: "Deep Mines",
			description: "Abandoned mine complex with ore and dangers.",
			primaryTerrain: Ids.Terrains.Underground.Mine,
			progressionOrder: -1,
			dangerLevel: 0.55f
		) {
			MinLength = 4,
			MaxLength = 8,
			VariantTerrains = [
				new WeightedTerrain(Ids.Terrains.Underground.Tunnel, 1.5f),
				new WeightedTerrain(Ids.Terrains.Underground.Cavern, 1.0f),
				new WeightedTerrain(Ids.Terrains.Underground.Cave, 0.8f)
			],
			VariantTerrainChance = 0.4f,
			BranchChanceMultiplier = 0.7f,
			DungeonChanceBonus = 0.25f,
			LandmarkChanceMultiplier = 0.9f,
			EventDensityMultiplier = 1.1f,
			ResourceNodeMultiplier = 2.5f,
			CampsiteSpacingMultiplier = 1.2f,
			CanAppearInBranches = true,
			TransitionText = "Old mine shafts lead into the darkness below...",
			AmbientDescription = "Abandoned equipment rusts in the tunnels. Something moved in the dark.",
			MusicId = "music_mine",
			AmbientSoundId = "ambient_mine",
			Tags = [Ids.Tags.Region.Dungeon],
			AllowedDungeons = [
				Ids.Locations.Dungeons.AbandonedMine,
				Ids.Locations.Dungeons.CollapsingMine,
				Ids.Locations.Dungeons.DeepMine
			],
			PreferredEncounters = [
				Ids.Encounters.Random.Goblins,
				Ids.Encounters.Random.GiantSpiders,
				Ids.Encounters.Random.OreGuardian
			],
			PreferredEvents = [
				Ids.Events.Random.HiddenCache,
				Ids.Events.Random.AncientTrap
			]
		});

		// ═══════════════════════════════════════════════════════════════
		// CATACOMBS - Undead-filled tunnels
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new BiomeProto(
				id: Ids.Biomes.Catacombs,
				name: "Catacombs",
				description: "Ancient catacombs filled with undead.",
				primaryTerrain: Ids.Terrains.Underground.Catacombs,
				progressionOrder: -1,
				dangerLevel: 0.65f
			) {
				MinLength = 4,
				MaxLength = 7,
				VariantTerrains = [
					new WeightedTerrain(Ids.Terrains.Ruins.Crypt, 1.2f),
					new WeightedTerrain(Ids.Terrains.Underground.Tunnel, 1.0f),
					new WeightedTerrain(Ids.Terrains.Supernatural.CursedGround, 0.6f),
					new WeightedTerrain(Ids.Terrains.Underground.Cavern, 0.4f)
				],
				VariantTerrainChance = 0.4f,
				BranchChanceMultiplier = 0.5f,
				DungeonChanceBonus = 0.35f,
				LandmarkChanceMultiplier = 1.0f,
				EventDensityMultiplier = 1.3f,
				ResourceNodeMultiplier = 0.4f,
				CampsiteSpacingMultiplier = 1.4f,
				CanAppearInBranches = true,
				TransitionText = "Ancient stairs descend into darkness. The smell of death rises...",
				AmbientDescription = "Bones line the walls. Whispers echo from empty tombs.",
				MusicId = "music_catacombs",
				AmbientSoundId = "ambient_crypt",
				Tags = [Ids.Tags.Region.Crypt, Ids.Tags.Creature.Undead, Ids.Tags.Element.Necrotic],
				AllowedDungeons = [
					Ids.Locations.Dungeons.ForgottenCrypt,
					Ids.Locations.Dungeons.AncientCatacombs,
					Ids.Locations.Dungeons.CursedMausoleum
				],
				PreferredEncounters = [
					Ids.Encounters.Random.UndeadRising,
					Ids.Encounters.Random.Undead,
					Ids.Encounters.Random.Haunted
				],
				PreferredEvents = [
					Ids.Events.Random.GraveDisturbance,
					Ids.Events.Random.GhostlyApparition,
					Ids.Events.Random.TreasureCurse,
					Ids.Events.Random.AncientTrap
				]
			});
	}

	#endregion
}
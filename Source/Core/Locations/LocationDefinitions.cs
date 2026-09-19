using RPGGame.Core.Effects;
using RPGGame.Core.Expedition;
using RPGGame.Core.Prototypes;
using RPGGame.Core.Prototypes.Combat;
using RPGGame.Core.Prototypes.Locations;

namespace RPGGame.Core.Locations;

/// <summary>
/// Defines all map locations using the Proto system.
/// </summary>
public class LocationDefinitions : ICoreData {
	public void GameData(GameDb gameDatabase) {
		RegisterSettlements(gameDatabase);
		RegisterLandmarks(gameDatabase);
		RegisterCamps(gameDatabase);
		RegisterDungeons(gameDatabase);
		RegisterBossArenas(gameDatabase);
	}

	#region Settlements

	private void RegisterSettlements(GameDb db) {
		db.RegisterProto(new SettlementProto(
			Ids.Locations.Settlements.HavenVillage,
			"Haven Village",
			"Your home village, protected by the Anchor's power. The last bastion of safety before the fog-touched lands.",
			SettlementSize.Village
		) {
			IconName = "icon_location_village",
			SceneName = "Village",
			MusicId = "music_village",
			IsHomeVillage = true,
			CanFastTravel = true,
			HasQuestBoard = true,
			Population = 150,
			DefaultTerrain = Ids.Terrains.Settlements.Village,
			FirstVisitEventId = Ids.Events.Intro.VillageIntroduction
		});

		db.RegisterProto(new SettlementProto(
			Ids.Locations.Settlements.WaystationInn,
			"The Weary Wanderer Inn",
			"A roadside inn that somehow persists despite the encroaching fog. The innkeeper claims the building predates the fog itself.",
			SettlementSize.Hamlet
		) {
			IconName = "icon_location_inn",
			SceneName = "WaystationInn",
			MusicId = "music_inn",
			PriceModifier = 1.2f,
			MinSpawnDistance = 15,
			MaxSpawnDistance = 30,
			SpawnWeight = 0.3f,
			DefaultTerrain = Ids.Terrains.Roads.Road,
			ValidTerrains = [Ids.Terrains.Roads.Road, Ids.Terrains.Roads.Path, Ids.Terrains.Forests.Forest]
		});

		db.RegisterProto(new SettlementProto(
			Ids.Locations.Settlements.FrontierOutpost,
			"Frontier Outpost",
			"A military outpost established to monitor the fog's advance. Soldiers here seem perpetually on edge.",
			SettlementSize.Hamlet
		) {
			IconName = "icon_location_outpost",
			SceneName = "Outpost",
			MusicId = "music_outpost",
			HasShop = true,
			MinSpawnDistance = 25,
			MaxSpawnDistance = 40,
			SpawnWeight = 0.2f,
			ValidTerrains = [Ids.Terrains.Plains.Hills, Ids.Terrains.Mountains.Mountain, Ids.Terrains.Plains.Grass]
		});

		db.RegisterProto(new SettlementProto(
			Ids.Locations.Settlements.FogwatchTower,
			"Fogwatch Tower",
			"A lonely tower manned by those who study the fog. They trade knowledge for supplies.",
			SettlementSize.Hamlet
		) {
			IconName = "icon_location_tower",
			SceneName = "FogwatchTower",
			MusicId = "music_mystery",
			HasShop = true,
			MinSpawnDistance = 35,
			SpawnWeight = 0.15f,
			ValidTerrains = [Ids.Terrains.Plains.Hills, Ids.Terrains.Supernatural.Corrupted]
		});
	}

	#endregion

	#region Landmarks

	private void RegisterLandmarks(GameDb db) {
		// ???????????????????????????????????????????????????????????????
		// NATURAL LANDMARKS
		// ???????????????????????????????????????????????????????????????

		db.RegisterProto(new LandmarkProto(
			Ids.Locations.Landmarks.AncientOak,
			"The Ancient Oak",
			"A massive oak tree that has stood for centuries. Its bark is carved with symbols of a forgotten language.",
			LandmarkType.AncientTree
		) {
			IconName = "icon_landmark_tree",
			ProvidesBuff = true,
			VisitEffect = Ids.Effects.Buffs.Blessed,
			VisitEffectDuration = 12,
			VisionBonus = 2,
			SpawnWeight = 0.5f,
			ValidTerrains = [Ids.Terrains.Forests.Forest, Ids.Terrains.Forests.DeepForest, Ids.Terrains.Forests.AncientForest]
		});

		db.RegisterProto(new LandmarkProto(
			Ids.Locations.Landmarks.StoneCircle,
			"Stone Circle",
			"Ancient standing stones arranged in a perfect circle. The air hums with dormant power.",
			LandmarkType.StoneCircle
		) {
			IconName = "icon_landmark_stones",
			ProvidesBuff = true,
			VisitEffect = Ids.Effects.Buffs.Resistance,
			VisitEffectDuration = 8,
			UnlocksLore = Ids.Lore.History.BeforeFog,
			SpawnWeight = 0.3f,
			ValidTerrains = [Ids.Terrains.Plains.Grass, Ids.Terrains.Plains.Hills, Ids.Terrains.Mountains.Highland]
		});

		db.RegisterProto(new LandmarkProto(
			Ids.Locations.Landmarks.CrystalCave,
			"Crystal Cave Entrance",
			"The entrance to a cave filled with glowing crystals. Their light pulses rhythmically.",
			LandmarkType.CrystalFormation
		) {
			IconName = "icon_landmark_crystal",
			SpawnWeight = 0.25f,
			ValidTerrains = [Ids.Terrains.Mountains.Mountain, Ids.Terrains.Underground.Cave, Ids.Terrains.Underground.CrystalCavern]
		});

		db.RegisterProto(new LandmarkProto(
			Ids.Locations.Landmarks.TwistedTree,
			"The Twisted Tree",
			"A tree warped by fog exposure into an unnatural spiral. Its leaves glow faintly at night.",
			LandmarkType.TwistedTree
		) {
			IconName = "icon_landmark_twisted",
			UnlocksLore = Ids.Lore.Fog.Nature01,
			MinSpawnDistance = 20,
			SpawnWeight = 0.6f,
			ValidTerrains = [Ids.Terrains.Supernatural.Corrupted, Ids.Terrains.Forests.DeadForest, Ids.Terrains.Supernatural.Blighted]
		});

		db.RegisterProto(new LandmarkProto(
			Ids.Locations.Landmarks.MistyWaterfall,
			"Misty Falls",
			"A beautiful waterfall shrouded in perpetual mist. The water is said to have healing properties.",
			LandmarkType.Waterfall
		) {
			IconName = "icon_landmark_waterfall",
			ProvidesBuff = true,
			VisitEffect = Ids.Effects.Buffs.Regeneration,
			VisitEffectDuration = 6,
			SpawnWeight = 0.4f,
			ValidTerrains = [Ids.Terrains.Water.River, Ids.Terrains.Forests.Forest, Ids.Terrains.Mountains.Mountain]
		});

		db.RegisterProto(new LandmarkProto(
			Ids.Locations.Landmarks.HotSpringPool,
			"Hot Spring",
			"A natural hot spring. Bathing here soothes weary muscles and lifts the spirit.",
			LandmarkType.HotSpring
		) {
			IconName = "icon_landmark_hotspring",
			CanRest = true,
			ProvidesBuff = true,
			VisitEffect = Ids.Effects.Buffs.Empowerment,
			VisitEffectDuration = 12,
			SpawnWeight = 0.25f,
			ValidTerrains = [Ids.Terrains.Volcanic.Geothermal, Ids.Terrains.Mountains.Mountain, Ids.Terrains.Water.HotSpring]
		});

		db.RegisterProto(new LandmarkProto(
			Ids.Locations.Landmarks.GiantMushroom,
			"Giant Mushroom Grove",
			"Massive mushrooms tower overhead, their caps providing shelter and their glow lighting the way.",
			LandmarkType.GiantMushroom
		) {
			IconName = "icon_landmark_mushroom",
			CanRest = true,
			SpawnWeight = 0.3f,
			ValidTerrains = [Ids.Terrains.Underground.MushroomForest, Ids.Terrains.Underground.Cave, Ids.Terrains.Wetlands.Swamp]
		});

		// ???????????????????????????????????????????????????????????????
		// RUINS
		// ???????????????????????????????????????????????????????????????

		db.RegisterProto(new LandmarkProto(
			Ids.Locations.Landmarks.RuinedShrine,
			"Ruined Shrine",
			"The remains of a shrine to a forgotten deity. Offerings still appear mysteriously.",
			LandmarkType.RuinedShrine
		) {
			IconName = "icon_landmark_shrine",
			ProvidesBuff = true,
			VisitEffect = Ids.Effects.Buffs.Blessed,
			VisitEffectDuration = 24,
			EffectCooldown = 72,
			SpawnWeight = 0.4f,
			ValidTerrains = [Ids.Terrains.Ruins.OpenRuins, Ids.Terrains.Forests.Forest, Ids.Terrains.Plains.Hills]
		});

		db.RegisterProto(new LandmarkProto(
			Ids.Locations.Landmarks.FallenStatue,
			"Fallen Statue",
			"A massive statue lies broken on the ground. Who it depicted is lost to time.",
			LandmarkType.AncientStatue
		) {
			IconName = "icon_landmark_statue",
			UnlocksLore = Ids.Lore.History.BeforeFog,
			SpawnWeight = 0.35f,
			ValidTerrains = [Ids.Terrains.Ruins.OpenRuins, Ids.Terrains.Plains.Grass, Ids.Terrains.Plains.Hills]
		});

		db.RegisterProto(new LandmarkProto(
			Ids.Locations.Landmarks.OldBattlefield,
			"Old Battlefield",
			"The site of a battle fought long ago. Rusted weapons and bones still litter the ground.",
			LandmarkType.OldBattlefield
		) {
			IconName = "icon_landmark_battlefield",
			UnlocksLore = Ids.Lore.History.Unmaking,
			OnEnterEventId = Ids.Events.Random.GraveDisturbance,
			SpawnWeight = 0.35f,
			ValidTerrains = [Ids.Terrains.Plains.Grass, Ids.Terrains.Ruins.Battlefield, Ids.Terrains.Plains.Hills]
		});

		db.RegisterProto(new LandmarkProto(
			Ids.Locations.Landmarks.AncientGrave,
			"Ancient Grave",
			"A weathered gravestone marks this spot. The inscription is worn but legible.",
			LandmarkType.GraveSite
		) {
			IconName = "icon_landmark_grave",
			OnEnterEventId = Ids.Events.Random.GraveDisturbance,
			SpawnWeight = 0.4f,
			ValidTerrains = [Ids.Terrains.Ruins.Graveyard, Ids.Terrains.Forests.Forest, Ids.Terrains.Plains.Hills]
		});

		db.RegisterProto(new LandmarkProto(
			Ids.Locations.Landmarks.BrokenArch,
			"Broken Arch",
			"Part of what was once a grand archway. The stonework is exquisite despite its age.",
			LandmarkType.BrokenArch
		) {
			IconName = "icon_landmark_arch",
			SpawnWeight = 0.4f,
			ValidTerrains = [Ids.Terrains.Ruins.OpenRuins, Ids.Terrains.Roads.Road]
		});

		// ???????????????????????????????????????????????????????????????
		// MAN-MADE
		// ???????????????????????????????????????????????????????????????

		db.RegisterProto(new LandmarkProto(
			Ids.Locations.Landmarks.Crossroads,
			"Crossroads",
			"A junction where multiple paths meet. Travelers often leave offerings here for safe passage.",
			LandmarkType.Crossroads
		) {
			IconName = "icon_landmark_crossroads",
			SpawnWeight = 1f,
			VisionBonus = 1,
			ValidTerrains = [Ids.Terrains.Roads.Road, Ids.Terrains.Roads.Path, Ids.Terrains.Special.Crossroads]
		});

		db.RegisterProto(new LandmarkProto(
			Ids.Locations.Landmarks.WayShrine,
			"Way Shrine",
			"A small roadside shrine maintained by traveling monks. Provides blessings to weary wanderers.",
			LandmarkType.WayShrine
		) {
			IconName = "icon_landmark_wayshrine",
			ProvidesBuff = true,
			VisitEffect = Ids.Effects.Buffs.Blessed,
			VisitEffectDuration = 6,
			CanRest = true,
			IsSafe = true,
			SpawnWeight = 0.5f,
			ValidTerrains = [Ids.Terrains.Roads.Road, Ids.Terrains.Roads.Path]
		});

		db.RegisterProto(new LandmarkProto(
			Ids.Locations.Landmarks.AbandonedWell,
			"Abandoned Well",
			"An old stone well. The water below is dark and still.",
			LandmarkType.OldWell
		) {
			IconName = "icon_landmark_well",
			SpawnWeight = 0.5f,
			ValidTerrains = [Ids.Terrains.Ruins.OpenRuins, Ids.Terrains.Plains.Grass, Ids.Terrains.Ruins.AbandonedVillage]
		});

		db.RegisterProto(new LandmarkProto(
			Ids.Locations.Landmarks.HermitHut,
			"Hermit's Hut",
			"A small, weathered hut. Someone lives here, though they rarely show themselves.",
			LandmarkType.HermitHut
		) {
			IconName = "icon_landmark_hut",
			OnEnterEventId = Ids.Events.Random.MysteriousStranger,
			SpawnWeight = 0.3f,
			ValidTerrains = [Ids.Terrains.Forests.Forest, Ids.Terrains.Forests.DeepForest, Ids.Terrains.Wetlands.Swamp]
		});

		db.RegisterProto(new LandmarkProto(
			Ids.Locations.Landmarks.WatchTowerRuins,
			"Watchtower Ruins",
			"The remains of an old watchtower. It still offers a good vantage point.",
			LandmarkType.WatchTower
		) {
			IconName = "icon_landmark_tower",
			VisionBonus = 3,
			SpawnWeight = 0.35f,
			ValidTerrains = [Ids.Terrains.Plains.Hills, Ids.Terrains.Mountains.Mountain, Ids.Terrains.Ruins.OpenRuins]
		});

		// ???????????????????????????????????????????????????????????????
		// MAGICAL
		// ???????????????????????????????????????????????????????????????

		db.RegisterProto(new LandmarkProto(
			Ids.Locations.Landmarks.ManaWell,
			"Mana Well",
			"A natural concentration of magical energy. Spellcasters are drawn to its power.",
			LandmarkType.ManaWell
		) {
			IconName = "icon_landmark_mana",
			OnEnterEventId = Ids.Events.Random.ManaWellBlessing,
			MinSpawnDistance = 15,
			SpawnWeight = 0.2f,
			ValidTerrains = [Ids.Terrains.Supernatural.Magical, Ids.Terrains.Supernatural.LeyLine, Ids.Terrains.Forests.AncientForest]
		});

		db.RegisterProto(new LandmarkProto(
			Ids.Locations.Landmarks.LeyLineNode,
			"Ley Line Nexus",
			"Where multiple ley lines cross. The magical energy here is palpable.",
			LandmarkType.LeyLineNode
		) {
			IconName = "icon_landmark_leyline",
			ProvidesBuff = true,
			VisitEffect = Ids.Effects.Buffs.Empowerment,
			VisitEffectDuration = 8,
			MinSpawnDistance = 20,
			SpawnWeight = 0.15f,
			ValidTerrains = [Ids.Terrains.Supernatural.LeyLine, Ids.Terrains.Supernatural.Magical]
		});

		db.RegisterProto(new LandmarkProto(
			Ids.Locations.Landmarks.TemporalRift,
			"Temporal Rift",
			"A tear in time itself. The air shimmers and moments seem to repeat.",
			LandmarkType.TemporalRift
		) {
			IconName = "icon_landmark_rift",
			OnEnterEventId = Ids.Events.Random.TimeAnomaly,
			UnlocksLore = Ids.Lore.Magic.Temporal,
			MinSpawnDistance = 30,
			SpawnWeight = 0.1f,
			ValidTerrains = [Ids.Terrains.Supernatural.VoidTouched, Ids.Terrains.Supernatural.Corrupted]
		});

		db.RegisterProto(new LandmarkProto(
			Ids.Locations.Landmarks.FogMonument,
			"Fog Monument",
			"A structure that appeared with the fog. Its purpose is unknown, but it pulses with strange energy.",
			LandmarkType.FogMonument
		) {
			IconName = "icon_landmark_fog",
			OnEnterEventId = Ids.Events.Random.FogWhispers,
			UnlocksLore = Ids.Lore.Fog.Clue01,
			MinSpawnDistance = 25,
			SpawnWeight = 0.15f,
			ValidTerrains = [Ids.Terrains.Supernatural.Corrupted, Ids.Terrains.Supernatural.VoidTouched, Ids.Terrains.Supernatural.ShadowRealm]
		});

		db.RegisterProto(new LandmarkProto(
			Ids.Locations.Landmarks.ShadowTear,
			"Shadow Tear",
			"A rift to the shadow realm. Darkness seeps through, even in daylight.",
			LandmarkType.ShadowTear
		) {
			IconName = "icon_landmark_shadow",
			MinSpawnDistance = 30,
			SpawnWeight = 0.1f,
			ValidTerrains = [Ids.Terrains.Supernatural.ShadowRealm, Ids.Terrains.Supernatural.Corrupted]
		});

		// ???????????????????????????????????????????????????????????????
		// SPECIAL (Non-spawning)
		// ???????????????????????????????????????????????????????????????

		db.RegisterProto(new LandmarkProto(
			Ids.Locations.Landmarks.FogBoundary,
			"The Fog's Edge",
			"The boundary where the fog begins. Beyond lies the unknown.",
			LandmarkType.FogBoundary
		) {
			IconName = "icon_landmark_fogboundary",
			CanSpawnProcedurally = false
		});

		db.RegisterProto(new LandmarkProto(
			Ids.Locations.Landmarks.FogHeart,
			"The Heart of the Fog",
			"The source of the creeping fog. Time and space bend around this place.",
			LandmarkType.FogHeart
		) {
			IconName = "icon_landmark_fogheart",
			CanSpawnProcedurally = false,
			FirstVisitEventId = Ids.Events.Story.CaughtInTime
		});
	}

	#endregion

	#region Camps

	private void RegisterCamps(GameDb db) {
		db.RegisterProto(new CampProto(
			Ids.Locations.Camps.ForestClearing,
			"Forest Clearing",
			"A small clearing in the forest, suitable for making camp.",
			CampType.Clearing
		) {
			IconName = "icon_camp_clearing",
			RestEffectiveness = 1f,
			SafetyLevel = 0.5f,
			SpawnWeight = 1f,
			ValidTerrains = [Ids.Terrains.Forests.Forest, Ids.Terrains.Forests.DeepForest, Ids.Terrains.Plains.Grass]
		});

		db.RegisterProto(new CampProto(
			Ids.Locations.Camps.RockOverhang,
			"Rock Overhang",
			"A natural rock formation providing shelter from the elements.",
			CampType.Cave
		) {
			IconName = "icon_camp_overhang",
			RestEffectiveness = 1.1f,
			SafetyLevel = 0.7f,
			HasShelter = true,
			SpawnWeight = 0.6f,
			ValidTerrains = [Ids.Terrains.Mountains.Mountain, Ids.Terrains.Plains.Hills, Ids.Terrains.Mountains.Cliff]
		});

		db.RegisterProto(new CampProto(
			Ids.Locations.Camps.AbandonedCamp,
			"Abandoned Camp",
			"Someone camped here before. Their fire pit is cold but usable.",
			CampType.Clearing
		) {
			IconName = "icon_camp_abandoned",
			RestEffectiveness = 1f,
			SafetyLevel = 0.4f,
			FirstVisitEventId = Ids.Events.Random.AbandonedCamp,
			SpawnWeight = 0.8f,
			ValidTerrains = [Ids.Terrains.Forests.Forest, Ids.Terrains.Roads.Path, Ids.Terrains.Plains.Hills]
		});

		db.RegisterProto(new CampProto(
			Ids.Locations.Camps.HuntersCamp,
			"Hunter's Camp",
			"A well-maintained camp used by hunters. Supplies are sometimes left behind.",
			CampType.HuntersCamp
		) {
			IconName = "icon_camp_hunter",
			RestEffectiveness = 1.2f,
			SafetyLevel = 0.8f,
			HasShelter = true,
			AvailableResources = [Ids.Items.Consumables.Rations, Ids.Items.Materials.LeatherScraps],
			SpawnWeight = 0.4f,
			ValidTerrains = [Ids.Terrains.Forests.Forest, Ids.Terrains.Forests.DeepForest]
		});

		db.RegisterProto(new CampProto(
			Ids.Locations.Camps.RuinedCottage,
			"Ruined Cottage",
			"The remains of a small home. The walls still provide some protection.",
			CampType.Ruins
		) {
			IconName = "icon_camp_ruins",
			RestEffectiveness = 1.1f,
			SafetyLevel = 0.6f,
			HasShelter = true,
			SpawnWeight = 0.5f,
			ValidTerrains = [Ids.Terrains.Ruins.OpenRuins, Ids.Terrains.Ruins.AbandonedVillage, Ids.Terrains.Forests.Forest]
		});

		db.RegisterProto(new CampProto(
			Ids.Locations.Camps.CaveShelter,
			"Cave Shelter",
			"A shallow cave that provides excellent shelter, though it may have other occupants.",
			CampType.Cave
		) {
			IconName = "icon_camp_cave",
			RestEffectiveness = 1.15f,
			SafetyLevel = 0.6f,
			HasShelter = true,
			SpawnWeight = 0.5f,
			ValidTerrains = [Ids.Terrains.Mountains.Mountain, Ids.Terrains.Underground.Cave, Ids.Terrains.Plains.Hills]
		});

		db.RegisterProto(new CampProto(
			Ids.Locations.Camps.Waystation,
			"Waystation",
			"A maintained rest stop for travelers. Safe and comfortable.",
			CampType.Waystation
		) {
			IconName = "icon_camp_waystation",
			RestEffectiveness = 1.25f,
			SafetyLevel = 1f,
			HasShelter = true,
			IsSafe = true,
			IsPermanent = true,
			MinSpawnDistance = 10,
			SpawnWeight = 0.3f,
			ValidTerrains = [Ids.Terrains.Roads.Road, Ids.Terrains.Roads.Path]
		});
	}

	#endregion

	#region Dungeons

	private void RegisterDungeons(GameDb db) {
		// ═══════════════════════════════════════════════════════════════
		// TUTORIAL DUNGEONS
		// ═══════════════════════════════════════════════════════════════

		db.RegisterProto(new DungeonProto(
				Ids.Locations.Dungeons.OvergrownRuins,
				"Overgrown Ruins",
				"Ancient stones covered in vines and moss. Nature has reclaimed this place, but something still stirs within.",
				DungeonType.AncientRuins
			) {
				IconName = "icon_dungeon_ruins",
				DifficultyTier = 1,
				RecommendedLevel = 1,
				FloorCount = 2,
				PossibleEncounters = [Ids.Encounters.Random.Wolves, Ids.Encounters.Random.SpiderNest],
				ClearExperience = 50,
				ClearGold = 30,
				Hazards = [DungeonHazard.Traps],
				CanSpawnProcedurally = false  // Tutorial-only dungeon
			});

		// ???????????????????????????????????????????????????????????????
		// CAVES
		// ???????????????????????????????????????????????????????????????

		db.RegisterProto(new DungeonProto(
			Ids.Locations.Dungeons.SpiderDen,
			"Spider's Den",
			"A cave system infested with giant spiders. Webs cover every surface.",
			DungeonType.SpiderNest
		) {
			IconName = "icon_dungeon_spider",
			DifficultyTier = 2,
			RecommendedLevel = 3,
			FloorCount = 2,
			PossibleEncounters = [Ids.Encounters.Random.SpiderNest],
			ClearExperience = 100,
			ClearGold = 50,
			Hazards = [DungeonHazard.Traps],
			SpawnWeight = 0.6f,
			ValidTerrains = [Ids.Terrains.Forests.Forest, Ids.Terrains.Underground.Cave]
		});

		db.RegisterProto(new DungeonProto(
			Ids.Locations.Dungeons.WolfCave,
			"Wolf Den",
			"A pack of wolves has made this cave their home. Bones litter the entrance.",
			DungeonType.WolfDen
		) {
			IconName = "icon_dungeon_wolf",
			DifficultyTier = 1,
			RecommendedLevel = 2,
			FloorCount = 1,
			PossibleEncounters = [Ids.Encounters.Random.Wolves],
			ClearExperience = 60,
			ClearGold = 25,
			SpawnWeight = 0.7f,
			ValidTerrains = [Ids.Terrains.Forests.Forest, Ids.Terrains.Plains.Hills, Ids.Terrains.Underground.Cave]
		});

		db.RegisterProto(new DungeonProto(
			Ids.Locations.Dungeons.BearCave,
			"Bear Cave",
			"A large cave that reeks of bear. Something valuable glints in the darkness.",
			DungeonType.BearCave
		) {
			IconName = "icon_dungeon_bear",
			DifficultyTier = 3,
			RecommendedLevel = 4,
			FloorCount = 1,
			ClearExperience = 120,
			ClearGold = 75,
			SpawnWeight = 0.4f,
			ValidTerrains = [Ids.Terrains.Forests.Forest, Ids.Terrains.Mountains.Mountain, Ids.Terrains.Underground.Cave]
		});

		db.RegisterProto(new DungeonProto(
			Ids.Locations.Dungeons.GoblinCave,
			"Goblin Warren",
			"A sprawling cave system claimed by goblins. The smell is terrible.",
			DungeonType.Cave
		) {
			IconName = "icon_dungeon_goblin",
			DifficultyTier = 2,
			RecommendedLevel = 2,
			FloorCount = 3,
			PossibleEncounters = [Ids.Encounters.Random.Goblins],
			ClearExperience = 80,
			ClearGold = 75,
			SpawnWeight = 0.7f,
			ValidTerrains = [Ids.Terrains.Plains.Hills, Ids.Terrains.Underground.Cave, Ids.Terrains.Mountains.Mountain]
		});

		db.RegisterProto(new DungeonProto(
			Ids.Locations.Dungeons.CrystalCavern,
			"Crystal Cavern",
			"A cavern filled with luminescent crystals. Beautiful, but something guards them.",
			DungeonType.CavernSystem
		) {
			IconName = "icon_dungeon_crystal",
			DifficultyTier = 4,
			RecommendedLevel = 6,
			FloorCount = 3,
			ClearExperience = 200,
			ClearGold = 150,
			FirstClearRewards = [
				new LootEntry(Ids.Items.Materials.MagicCrystal, 1f, 3, 5)
			],
			MinSpawnDistance = 20,
			SpawnWeight = 0.25f,
			ValidTerrains = [Ids.Terrains.Underground.Cave, Ids.Terrains.Underground.CrystalCavern, Ids.Terrains.Mountains.Mountain]
		});

		// ???????????????????????????????????????????????????????????????
		// MINES
		// ???????????????????????????????????????????????????????????????

		db.RegisterProto(new DungeonProto(
			Ids.Locations.Dungeons.AbandonedMine,
			"Abandoned Mine",
			"An old mine, abandoned for unknown reasons. Tools and ore carts remain.",
			DungeonType.Mine
		) {
			IconName = "icon_dungeon_mine",
			DifficultyTier = 2,
			RecommendedLevel = 3,
			FloorCount = 2,
			PossibleEncounters = [Ids.Encounters.Random.Rats],
			ClearExperience = 75,
			ClearGold = 50,
			FirstClearRewards = [
				new LootEntry(Ids.Items.Materials.IronOre, 0.8f, 5, 10)
			],
			Hazards = [DungeonHazard.Darkness],
			SpawnWeight = 0.5f,
			ValidTerrains = [Ids.Terrains.Mountains.Mountain, Ids.Terrains.Plains.Hills, Ids.Terrains.Underground.Mine]
		});

		db.RegisterProto(new DungeonProto(
			Ids.Locations.Dungeons.CollapsingMine,
			"Collapsing Mine",
			"This mine is unstable. Valuable ore remains, but so does danger.",
			DungeonType.Mine
		) {
			IconName = "icon_dungeon_mine_danger",
			DifficultyTier = 4,
			RecommendedLevel = 5,
			FloorCount = 3,
			ClearExperience = 150,
			ClearGold = 100,
			FirstClearRewards = [
				new LootEntry(Ids.Items.Materials.SilverOre, 0.6f, 3, 6)
			],
			Hazards = [DungeonHazard.Collapse, DungeonHazard.Darkness],
			Mechanics = DungeonMechanics.TimedRun,
			MinSpawnDistance = 15,
			SpawnWeight = 0.3f,
			ValidTerrains = [Ids.Terrains.Mountains.Mountain, Ids.Terrains.Underground.Mine]
		});

		db.RegisterProto(new DungeonProto(
			Ids.Locations.Dungeons.DeepMine,
			"Deep Mine",
			"A mine that delves deep into the earth. Strange sounds echo from below.",
			DungeonType.Mine
		) {
			IconName = "icon_dungeon_mine_deep",
			DifficultyTier = 6,
			RecommendedLevel = 8,
			FloorCount = 5,
			ClearExperience = 300,
			ClearGold = 200,
			FirstClearRewards = [
				new LootEntry(Ids.Items.Materials.MithrilOre, 0.5f, 2, 4)
			],
			Hazards = [DungeonHazard.Darkness, DungeonHazard.Traps],
			Mechanics = DungeonMechanics.MultiPath,
			MinSpawnDistance = 25,
			SpawnWeight = 0.15f,
			ValidTerrains = [Ids.Terrains.Mountains.Mountain, Ids.Terrains.Underground.Mine]
		});

		// ???????????????????????????????????????????????????????????????
		// CRYPTS & CATACOMBS
		// ???????????????????????????????????????????????????????????????

		db.RegisterProto(new DungeonProto(
			Ids.Locations.Dungeons.ForgottenCrypt,
			"Forgotten Crypt",
			"An ancient burial site. The dead here do not rest peacefully.",
			DungeonType.Crypt
		) {
			IconName = "icon_dungeon_crypt",
			DifficultyTier = 3,
			RecommendedLevel = 5,
			FloorCount = 3,
			PossibleEncounters = [Ids.Encounters.Random.UndeadRising],
			ClearExperience = 150,
			ClearGold = 100,
			Hazards = [DungeonHazard.Darkness, DungeonHazard.Cursed],
			MinSpawnDistance = 15,
			SpawnWeight = 0.4f,
			ValidTerrains = [Ids.Terrains.Ruins.Graveyard, Ids.Terrains.Ruins.OpenRuins]
		});

		db.RegisterProto(new DungeonProto(
			Ids.Locations.Dungeons.AncientCatacombs,
			"Ancient Catacombs",
			"Miles of underground tunnels filled with the remains of an ancient civilization.",
			DungeonType.Catacomb
		) {
			IconName = "icon_dungeon_catacomb",
			DifficultyTier = 5,
			RecommendedLevel = 8,
			FloorCount = 5,
			PossibleEncounters = [Ids.Encounters.Random.UndeadRising, Ids.Encounters.Random.Haunted],
			ClearExperience = 300,
			ClearGold = 200,
			Hazards = [DungeonHazard.Darkness, DungeonHazard.Traps, DungeonHazard.Cursed],
			Mechanics = DungeonMechanics.MultiPath | DungeonMechanics.Puzzle,
			MinSpawnDistance = 25,
			SpawnWeight = 0.2f,
			ValidTerrains = [Ids.Terrains.Ruins.OpenRuins, Ids.Terrains.Underground.Catacombs]
		});

		db.RegisterProto(new DungeonProto(
			Ids.Locations.Dungeons.CursedMausoleum,
			"Cursed Mausoleum",
			"A grand tomb tainted by dark magic. Powerful undead guard something within.",
			DungeonType.Crypt
		) {
			IconName = "icon_dungeon_mausoleum",
			DifficultyTier = 6,
			RecommendedLevel = 10,
			FloorCount = 4,
			PossibleEncounters = [Ids.Encounters.Random.UndeadRising, Ids.Encounters.Random.Haunted],
			ClearExperience = 350,
			ClearGold = 250,
			Hazards = [DungeonHazard.Darkness, DungeonHazard.Cursed],
			Mechanics = DungeonMechanics.SecretBoss,
			MinSpawnDistance = 30,
			SpawnWeight = 0.15f,
			ValidTerrains = [Ids.Terrains.Ruins.Graveyard, Ids.Terrains.Ruins.OpenRuins]
		});

		// ???????????????????????????????????????????????????????????????
		// LAIRS
		// ???????????????????????????????????????????????????????????????

		db.RegisterProto(new DungeonProto(
			Ids.Locations.Dungeons.BanditHideout,
			"Bandit Hideout",
			"A camp where bandits plan their raids. They won't welcome visitors.",
			DungeonType.BanditCamp
		) {
			IconName = "icon_dungeon_bandit",
			DifficultyTier = 2,
			RecommendedLevel = 3,
			FloorCount = 2,
			PossibleEncounters = [Ids.Encounters.Random.BanditsSmall, Ids.Encounters.Random.BanditsMixed],
			ClearExperience = 100,
			ClearGold = 150,
			Mechanics = DungeonMechanics.Stealth,
			SpawnWeight = 0.5f,
			ValidTerrains = [Ids.Terrains.Forests.Forest, Ids.Terrains.Underground.Cave, Ids.Terrains.Ruins.OpenRuins]
		});

		db.RegisterProto(new DungeonProto(
			Ids.Locations.Dungeons.CultistLair,
			"Cultist Lair",
			"A hidden sanctuary where dark rituals are performed.",
			DungeonType.CultistLair
		) {
			IconName = "icon_dungeon_cult",
			DifficultyTier = 4,
			RecommendedLevel = 6,
			FloorCount = 3,
			PossibleEncounters = [Ids.Encounters.Random.CultistRitual],
			ClearExperience = 200,
			ClearGold = 125,
			Hazards = [DungeonHazard.Cursed],
			MinSpawnDistance = 20,
			SpawnWeight = 0.3f,
			ValidTerrains = [Ids.Terrains.Ruins.OpenRuins, Ids.Terrains.Underground.Cave, Ids.Terrains.Settlements.Temple]
		});

		db.RegisterProto(new DungeonProto(
			Ids.Locations.Dungeons.OrcStronghold,
			"Orc Stronghold",
			"A fortified position held by orcs. Only the strong survive here.",
			DungeonType.Fortress
		) {
			IconName = "icon_dungeon_orc",
			DifficultyTier = 5,
			RecommendedLevel = 7,
			FloorCount = 3,
			PossibleEncounters = [Ids.Encounters.Random.OrcPatrol],
			ClearExperience = 250,
			ClearGold = 175,
			MinSpawnDistance = 25,
			SpawnWeight = 0.25f,
			ValidTerrains = [Ids.Terrains.Mountains.Mountain, Ids.Terrains.Plains.Hills, Ids.Terrains.Desert.Badlands]
		});

		// ???????????????????????????????????????????????????????????????
		// RUINS
		// ???????????????????????????????????????????????????????????????

		db.RegisterProto(new DungeonProto(
			Ids.Locations.Dungeons.AncientRuins,
			"Ancient Ruins",
			"The remains of a once-great structure. Secrets lie buried within.",
			DungeonType.AncientRuins
		) {
			IconName = "icon_dungeon_ruins",
			DifficultyTier = 3,
			RecommendedLevel = 4,
			FloorCount = 3,
			ClearExperience = 125,
			ClearGold = 100,
			FirstClearRewards = [
				new LootEntry(Ids.Items.Materials.AncientRelic, 0.3f)
			],
			Hazards = [DungeonHazard.Traps],
			Mechanics = DungeonMechanics.Puzzle,
			SpawnWeight = 0.4f,
			ValidTerrains = [Ids.Terrains.Ruins.OpenRuins, Ids.Terrains.Forests.Forest]
		});

		db.RegisterProto(new DungeonProto(
			Ids.Locations.Dungeons.CursedTemple,
			"Cursed Temple",
			"A temple corrupted by dark forces. Foul things now worship here.",
			DungeonType.Temple
		) {
			IconName = "icon_dungeon_temple",
			DifficultyTier = 5,
			RecommendedLevel = 7,
			FloorCount = 4,
			PossibleEncounters = [Ids.Encounters.Random.CultistRitual],
			ClearExperience = 275,
			ClearGold = 175,
			Hazards = [DungeonHazard.Cursed],
			MinSpawnDistance = 20,
			SpawnWeight = 0.25f,
			ValidTerrains = [Ids.Terrains.Ruins.OpenRuins, Ids.Terrains.Settlements.Temple]
		});

		db.RegisterProto(new DungeonProto(
			Ids.Locations.Dungeons.AbandonedTower,
			"Abandoned Tower",
			"A wizard's tower, long abandoned. Magical experiments may still roam within.",
			DungeonType.Tower
		) {
			IconName = "icon_dungeon_tower",
			DifficultyTier = 4,
			RecommendedLevel = 6,
			FloorCount = 4,
			ClearExperience = 200,
			ClearGold = 125,
			FirstClearRewards = [
				new LootEntry(Ids.Items.Materials.ManaEssence, 0.5f, 1, 3)
			],
			Hazards = [DungeonHazard.Traps],
			SpawnWeight = 0.3f,
			ValidTerrains = [Ids.Terrains.Plains.Hills, Ids.Terrains.Forests.Forest, Ids.Terrains.Settlements.Tower]
		});

		db.RegisterProto(new DungeonProto(
			Ids.Locations.Dungeons.SunkenLibrary,
			"Sunken Library",
			"A library that sank into the earth. Knowledge and danger await in equal measure.",
			DungeonType.ForgottenLibrary
		) {
			IconName = "icon_dungeon_library",
			DifficultyTier = 5,
			RecommendedLevel = 8,
			FloorCount = 4,
			ClearExperience = 300,
			ClearGold = 100,
			FirstClearRewards = [
				new LootEntry(Ids.Items.Materials.AncientScroll, 0.6f, 2, 4)
			],
			Hazards = [DungeonHazard.Darkness, DungeonHazard.Flooding],
			Mechanics = DungeonMechanics.Puzzle,
			MinSpawnDistance = 25,
			SpawnWeight = 0.2f,
			ValidTerrains = [Ids.Terrains.Ruins.OpenRuins, Ids.Terrains.Wetlands.Swamp]
		});

		// ???????????????????????????????????????????????????????????????
		// STORY DUNGEONS (Non-procedural)
		// ???????????????????????????????????????????????????????????????

		db.RegisterProto(new DungeonProto(
			Ids.Locations.Dungeons.BanditFortress,
			"Bandit King's Fortress",
			"The stronghold of the infamous Bandit King. Only the brave or foolish dare approach.",
			DungeonType.Fortress
		) {
			IconName = "icon_dungeon_fortress",
			DifficultyTier = 5,
			RecommendedLevel = 8,
			FloorCount = 4,
			IsProceduralLayout = false,
			LayoutPrefab = "Dungeon_BanditFortress",
			BossEncounter = Ids.Encounters.Bosses.BanditKing,
			ClearExperience = 500,
			ClearGold = 500,
			ClearedFlag = "bandit_king_defeated",
			Repeatable = false,
			Mechanics = DungeonMechanics.Phases | DungeonMechanics.Adds,
			CanSpawnProcedurally = false
		});

		db.RegisterProto(new DungeonProto(
			Ids.Locations.Dungeons.LichSanctum,
			"The Lich's Sanctum",
			"An ancient tower where a lich has made its lair. Death magic permeates every stone.",
			DungeonType.LichSanctum
		) {
			IconName = "icon_dungeon_lich",
			DifficultyTier = 7,
			RecommendedLevel = 12,
			FloorCount = 5,
			IsProceduralLayout = false,
			LayoutPrefab = "Dungeon_LichSanctum",
			BossEncounter = Ids.Encounters.Bosses.Lich,
			ClearExperience = 1000,
			ClearGold = 750,
			ClearedFlag = "lich_defeated",
			Repeatable = false,
			Hazards = [DungeonHazard.Darkness, DungeonHazard.Cursed],
			Mechanics = DungeonMechanics.Phases | DungeonMechanics.Adds | DungeonMechanics.Puzzle,
			CanSpawnProcedurally = false
		});

		db.RegisterProto(new DungeonProto(
			Ids.Locations.Dungeons.FogNexus,
			"The Fog Nexus",
			"The heart of the fog itself. Reality bends and time loops endlessly.",
			DungeonType.FogHeart
		) {
			IconName = "icon_dungeon_fog",
			DifficultyTier = 10,
			RecommendedLevel = 20,
			FloorCount = 7,
			IsProceduralLayout = false,
			LayoutPrefab = "Dungeon_FogNexus",
			BossEncounter = Ids.Encounters.Bosses.FogHerald,
			ClearExperience = 5000,
			ClearGold = 2000,
			ClearedFlag = "fog_herald_defeated",
			Repeatable = false,
			Hazards = [DungeonHazard.Fog, DungeonHazard.Temporal],
			Mechanics = DungeonMechanics.Phases | DungeonMechanics.Scripted | DungeonMechanics.NoEscape,
			CanSpawnProcedurally = false
		});
	}

	#endregion

	#region Boss Arenas

	private void RegisterBossArenas(GameDb db) {
		db.RegisterProto(new BossArenaProto(
			Ids.Locations.BossArenas.BanditKingThrone,
			"The Bandit King's Throne",
			"The throne room of the Bandit King. He sits surrounded by his most loyal warriors.",
			Ids.Encounters.Bosses.BanditKing
		) {
			IconName = "icon_boss_bandit",
			SceneName = "BossArena_BanditKing",
			MusicId = "music_boss_bandit",
			RequiredLevel = 8,
			PreFightEvent = Ids.Events.Story.BanditCampCleared,
			PostFightEvent = Ids.Events.Story.BanditCampCleared,
			DefeatedFlag = "bandit_king_defeated",
			CinematicId = "cinematic_bandit_king",
			Mechanics = ArenaMechanics.Phases | ArenaMechanics.Adds,
			CanSpawnProcedurally = false
		});

		db.RegisterProto(new BossArenaProto(
			Ids.Locations.BossArenas.LichChamber,
			"The Lich's Chamber",
			"The inner sanctum of the Lich. Phylactery shards pulse with dark energy.",
			Ids.Encounters.Bosses.Lich
		) {
			IconName = "icon_boss_lich",
			SceneName = "BossArena_Lich",
			MusicId = "music_boss_lich",
			RequiredLevel = 12,
			RequiredFlag = "bandit_king_defeated",
			PostFightEvent = Ids.Events.Story.LichDefeated,
			DefeatedFlag = "lich_defeated",
			CinematicId = "cinematic_lich",
			Mechanics = ArenaMechanics.Phases | ArenaMechanics.Adds | ArenaMechanics.Environment,
			CanSpawnProcedurally = false
		});

		db.RegisterProto(new BossArenaProto(
			Ids.Locations.BossArenas.FogHeraldDomain,
			"Herald's Domain",
			"The space where time loops endlessly. The Fog Herald awaits at its center.",
			Ids.Encounters.Bosses.FogHerald
		) {
			IconName = "icon_boss_herald",
			SceneName = "BossArena_FogHerald",
			MusicId = "music_boss_final",
			RequiredLevel = 20,
			RequiredFlag = "lich_defeated",
			CanRetreat = false,
			PostFightEvent = Ids.Events.Story.FogHeraldDefeated,
			DefeatedFlag = "fog_herald_defeated",
			CinematicId = "cinematic_fog_herald",
			Mechanics = ArenaMechanics.Phases | ArenaMechanics.Scripted | ArenaMechanics.Environment,
			CanSpawnProcedurally = false
		});

		db.RegisterProto(new BossArenaProto(
			Ids.Locations.BossArenas.DemonGate,
			"The Demon Gate",
			"A portal to the infernal planes. A demon lord guards the crossing.",
			Ids.Encounters.Bosses.BanditKing // TODO: Add proper demon lord encounter
		) {
			IconName = "icon_boss_demon",
			SceneName = "BossArena_Demon",
			MusicId = "music_boss_demon",
			RequiredLevel = 15,
			BossRespawns = true,
			RespawnHours = 168,
			Mechanics = ArenaMechanics.Phases | ArenaMechanics.TimedDPS,
			CanSpawnProcedurally = false
		});

		db.RegisterProto(new BossArenaProto(
			Ids.Locations.BossArenas.DragonLair,
			"Valdros's Lair",
			"The ancient dragon Valdros slumbers upon a mountain of treasure.",
			Ids.Encounters.Bosses.BanditKing // TODO: Add dragon encounter
		) {
			IconName = "icon_boss_dragon",
			SceneName = "BossArena_Dragon",
			MusicId = "music_boss_dragon",
			RequiredLevel = 25,
			BossRespawns = true,
			RespawnHours = 336, // 2 weeks
			Mechanics = ArenaMechanics.Phases | ArenaMechanics.Environment | ArenaMechanics.Positioning,
			CanSpawnProcedurally = false
		});
	}

	#endregion
}
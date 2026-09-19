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
/// Partial class containing  E v e n t s.Trim() IDs.
/// Fill in the corresponding nested static classes here.
/// </summary>
public static partial class Ids
{
    
	// ═══════════════════════════════════════════════════════════════════════
	// EVENTS
	// ═══════════════════════════════════════════════════════════════════════

		public static class Events {
		public static class Intro {
			public static readonly EventProto.ID TheRoadAhead = newId("Intro_TheRoadAhead");
			public static readonly EventProto.ID FirstCombatComplete = newId("Intro_FirstCombatComplete");
			public static readonly EventProto.ID VillageIntroduction = newId("Intro_VillageIntroduction");
			public static readonly EventProto.ID GateGuardBoon = newId("Intro_GateGuardBoon");
			public static readonly EventProto.ID FirstCombatEncounter = newId("Intro_FirstCombatEncounter");
			public static readonly EventProto.ID DefenseBoonChoice = newId("Intro_DefenseBoonChoice");
		}

		public static class Random {
			// Resource Events
			public static readonly EventProto.ID AbandonedCamp = newId("AbandonedCamp");
			public static readonly EventProto.ID HerbPatch = newId("HerbPatch");
			public static readonly EventProto.ID HiddenCache = newId("HiddenCache");
			public static readonly EventProto.ID AbandonedWagon = newId("AbandonedWagon");
			public static readonly EventProto.ID FreshwaterSpring = newId("FreshwaterSpring");
			public static readonly EventProto.ID MushroomRing = newId("MushroomRing");
			public static readonly EventProto.ID FallenAdventurer = newId("FallenAdventurer");

			// Encounter Events
			public static readonly EventProto.ID WoundedTraveler = newId("WoundedTraveler");
			public static readonly EventProto.ID MerchantRoad = newId("MerchantRoad");
			public static readonly EventProto.ID BanditDemand = newId("BanditDemand");
			public static readonly EventProto.ID LostChild = newId("LostChild");
			public static readonly EventProto.ID RefugeeCamp = newId("RefugeeCamp");
			public static readonly EventProto.ID HermitWise = newId("HermitWise");
			public static readonly EventProto.ID GhostlyApparition = newId("GhostlyApparition");

			// Environmental Events
			public static readonly EventProto.ID StormApproaching = newId("StormApproaching");
			public static readonly EventProto.ID FogThickens = newId("FogThickens");
			public static readonly EventProto.ID BeautifulSunset = newId("BeautifulSunset");
			public static readonly EventProto.ID Earthquake = newId("Earthquake");
			public static readonly EventProto.ID NightfallDanger = newId("NightfallDanger");
			public static readonly EventProto.ID FloraOvergrowth = newId("FloraOvergrowth");

			// Discovery Events
			public static readonly EventProto.ID AncientShrine = newId("AncientShrine");
			public static readonly EventProto.ID MysteriousStranger = newId("MysteriousStranger");
			public static readonly EventProto.ID RuinedLibrary = newId("RuinedLibrary");
			public static readonly EventProto.ID OldBattlefield = newId("OldBattlefield");
			public static readonly EventProto.ID StandingStones = newId("StandingStones");
			public static readonly EventProto.ID HiddenCave = newId("HiddenCave");

			// Creature Events
			public static readonly EventProto.ID WildAnimal = newId("WildAnimal");
			public static readonly EventProto.ID WolfAmbush = newId("WolfAmbush");
			public static readonly EventProto.ID BearAttack = newId("BearAttack");
			public static readonly EventProto.ID SpiderAmbush = newId("SpiderAmbush");
			public static readonly EventProto.ID AngryBoar = newId("AngryBoar");
			public static readonly EventProto.ID AngryBees = newId("AngryBees");
			public static readonly EventProto.ID SwampThing = newId("SwampThing");
			public static readonly EventProto.ID WyvernAttack = newId("WyvernAttack");

			// Mystical Events
			public static readonly EventProto.ID ManaWellBlessing = newId("ManaWellBlessing");
			public static readonly EventProto.ID FogWhispers = newId("FogWhispers");
			public static readonly EventProto.ID TimeAnomaly = newId("TimeAnomaly");
			public static readonly EventProto.ID TimeCrackSealed = newId("TimeCrackSealed");
			public static readonly EventProto.ID DragonGhost = newId("DragonGhost");
			public static readonly EventProto.ID StarfallWish = newId("StarfallWish");
			public static readonly EventProto.ID ShrinePrayer = newId("ShrinePrayer");
			public static readonly EventProto.ID DemonWhisper = newId("DemonWhisper");
			public static readonly EventProto.ID TreasureCurse = newId("TreasureCurse");

			// Hazard Events
			public static readonly EventProto.ID MithrilGuardian = newId("MithrilGuardian");
			public static readonly EventProto.ID PoisonPrick = newId("PoisonPrick");
			public static readonly EventProto.ID MandrakeScream = newId("MandrakeScream");
			public static readonly EventProto.ID ElderTreeSpirit = newId("ElderTreeSpirit");
			public static readonly EventProto.ID CorruptionSpread = newId("CorruptionSpread");
			public static readonly EventProto.ID CartOwner = newId("CartOwner");
			public static readonly EventProto.ID AncientTrap = newId("AncientTrap");
			public static readonly EventProto.ID GeyserEruption = newId("GeyserEruption");
			public static readonly EventProto.ID HotSpringRest = newId("HotSpringRest");
			public static readonly EventProto.ID GraveDisturbance = newId("GraveDisturbance");
			public static readonly EventProto.ID HiddenSpider = newId("HiddenSpider");
			public static readonly EventProto.ID AntSwarm = newId("AntSwarm");

			// Status Events
			public static readonly EventProto.ID Exhaustion = newId("Exhaustion");
			public static readonly EventProto.ID Collapse = newId("Collapse");
		}

		public static class Story {
			public static readonly EventProto.ID VillageReturn = newId("Story_VillageReturn");
			public static readonly EventProto.ID CampsiteFound = newId("Story_CampsiteFound");
			public static readonly EventProto.ID BanditCampCleared = newId("Story_BanditCampCleared");
			public static readonly EventProto.ID LichDefeated = newId("Story_LichDefeated");
			public static readonly EventProto.ID FogHeraldDefeated = newId("Story_FogHeraldDefeated");
			public static readonly EventProto.ID CaughtInTime = newId("Story_CaughtInTime");
			public static readonly EventProto.ID FogClueDiscovered = newId("Story_FogClueDiscovered");
		}

		private static EventProto.ID newId(string name) {
			return new EventProto.ID($"Event_{name}");
		}
	}
}

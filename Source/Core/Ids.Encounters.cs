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
/// Partial class containing  E n c o u n t e r s.Trim() IDs.
/// Fill in the corresponding nested static classes here.
/// </summary>
public static partial class Ids
{
    	// ═══════════════════════════════════════════════════════════════════════
	// ENCOUNTERS
	// ═══════════════════════════════════════════════════════════════════════

	public static class Encounters {
		public static class Random {
			public static readonly EncounterProto.ID Wolves = newId("Wolves");
			public static readonly EncounterProto.ID BanditsSmall = newId("BanditsSmall");
			public static readonly EncounterProto.ID BanditsMixed = newId("BanditsMixed");
			public static readonly EncounterProto.ID Rats = newId("Rats");
			public static readonly EncounterProto.ID Goblins = newId("Goblins");
			public static readonly EncounterProto.ID SpiderNest = newId("SpiderNest");
			public static readonly EncounterProto.ID OrcPatrol = newId("OrcPatrol");
			public static readonly EncounterProto.ID UndeadRising = newId("UndeadRising");
			public static readonly EncounterProto.ID CultistRitual = newId("CultistRitual");
			public static readonly EncounterProto.ID Haunted = newId("Haunted");
			public static readonly EncounterProto.ID Hellhounds = newId("Hellhounds");
			public static readonly EncounterProto.ID Bear = newId("Bear");
			public static readonly EncounterProto.ID WildBeast = newId("WildBeast");
			public static readonly EncounterProto.ID WolfPack = newId("WolfPack");
			public static readonly EncounterProto.ID GiantSpiders = newId("GiantSpiders");
			public static readonly EncounterProto.ID WildBoar = newId("WildBoar");
			public static readonly EncounterProto.ID SwampCreature = newId("SwampCreature");
			public static readonly EncounterProto.ID Wyvern = newId("Wyvern");
			public static readonly EncounterProto.ID OreGuardian = newId("OreGuardian");
			public static readonly EncounterProto.ID Undead = newId("Undead");
			public static readonly EncounterProto.ID TrapdoorSpider = newId("TrapdoorSpider");


		}

		public static class Story {
			public static readonly EncounterProto.ID IntroWolves = newId("Story_IntroWolves");
			public static readonly EncounterProto.ID BanditCamp = newId("Story_BanditCamp");
			public static readonly EncounterProto.ID FogCreatures = newId("Story_FogCreatures");
			public static readonly EncounterProto.ID CultistLair = newId("Story_CultistLair");
		}

		public static class Bosses {
			public static readonly EncounterProto.ID BanditKing = newId("Boss_BanditKing");
			public static readonly EncounterProto.ID Lich = newId("Boss_Lich");
			public static readonly EncounterProto.ID FogHerald = newId("Boss_FogHerald");
		}

		private static EncounterProto.ID newId(string name) {
			return new EncounterProto.ID($"Encounter_{name}");
		}
	}
}

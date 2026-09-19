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
/// Partial class containing  C h a r a c t e r C l a s s e s.Trim() IDs.
/// Fill in the corresponding nested static classes here.
/// </summary>
public static partial class Ids
{
    	// ═══════════════════════════════════════════════════════════════════════
	// CHARACTER CLASSES
	// ═══════════════════════════════════════════════════════════════════════

	public static class CharacterClasses {
			// Starter classes
			public static readonly CharacterClassProto.ID Fighter = newId("Fighter");
			public static readonly CharacterClassProto.ID Rogue = newId("Rogue");
			public static readonly CharacterClassProto.ID Mage = newId("Mage");
			public static readonly CharacterClassProto.ID Cleric = newId("Cleric");

			// Unlockable classes
			public static readonly CharacterClassProto.ID Ranger = newId("Ranger");
			public static readonly CharacterClassProto.ID Paladin = newId("Paladin");
			public static readonly CharacterClassProto.ID Necromancer = newId("Necromancer");
			public static readonly CharacterClassProto.ID Barbarian = newId("Barbarian");
			public static readonly CharacterClassProto.ID Bard = newId("Bard");
			public static readonly CharacterClassProto.ID Druid = newId("Druid");
			public static readonly CharacterClassProto.ID Monk = newId("Monk");
			public static readonly CharacterClassProto.ID Warlord = newId("Warlord");
			public static readonly CharacterClassProto.ID ShadowDancer = newId("ShadowDancer");
			public static readonly CharacterClassProto.ID WitchHunter = newId("WitchHunter");
			public static readonly CharacterClassProto.ID Summoner = newId("Summoner");
			public static readonly CharacterClassProto.ID Oracle = newId("Oracle");
			public static readonly CharacterClassProto.ID Alchemist = newId("Alchemist");
			public static readonly CharacterClassProto.ID BloodMage = newId("BloodMage");

			// Fog-Themed classes
			public static readonly CharacterClassProto.ID FogWalker = newId("FogWalker");
			public static readonly CharacterClassProto.ID FogWarden = newId("FogWarden");

			// Secret classes
			public static readonly CharacterClassProto.ID Ascended = newId("Ascended");
			public static readonly CharacterClassProto.ID TimeWalker = newId("TimeWalker");
			public static readonly CharacterClassProto.ID VoidTouched = newId("VoidTouched");

		private static CharacterClassProto.ID newId(string name) {
			return new CharacterClassProto.ID($"Class_{name}");
		}
	}
}

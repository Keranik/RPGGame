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
/// Partial class containing  W e a p o n s.Trim() IDs.
/// Fill in the corresponding nested static classes here.
/// </summary>
public static partial class Ids
{
    	// ═══════════════════════════════════════════════════════════════════════
	// WEAPONS
	// ═══════════════════════════════════════════════════════════════════════

	public static class Weapons {
		public static class Swords {
			public static readonly WeaponProto.ID ShortSword = newId("ShortSword");
			public static readonly WeaponProto.ID LongSword = newId("LongSword");
			public static readonly WeaponProto.ID GreatSword = newId("GreatSword");
			public static readonly WeaponProto.ID IronSword = newId("IronSword");
			public static readonly WeaponProto.ID SteelSword = newId("SteelSword");
			public static readonly WeaponProto.ID TemporalBlade = newId("TemporalBlade");
		}

		public static class Daggers {
			public static readonly WeaponProto.ID Dagger = newId("Dagger");
			public static readonly WeaponProto.ID Stiletto = newId("Stiletto");
			public static readonly WeaponProto.ID Shiv = newId("Shiv");
			public static readonly WeaponProto.ID SteelDagger = newId("SteelDagger");
			public static readonly WeaponProto.ID PoisonedDagger = newId("PoisonedDagger");
		}

		public static class Axes {
			public static readonly WeaponProto.ID Hatchet = newId("Hatchet");
			public static readonly WeaponProto.ID BattleAxe = newId("BattleAxe");
			public static readonly WeaponProto.ID WarAxe = newId("WarAxe");
			public static readonly WeaponProto.ID IronAxe = newId("IronAxe");
			public static readonly WeaponProto.ID GreatAxe = newId("GreatAxe");
		}

		public static class Bows {
			public static readonly WeaponProto.ID ShortBow = newId("ShortBow");
			public static readonly WeaponProto.ID LongBow = newId("LongBow");
			public static readonly WeaponProto.ID CompositeBow = newId("CompositeBow");
			public static readonly WeaponProto.ID HuntingBow = newId("HuntingBow");
			public static readonly WeaponProto.ID Longbow = newId("Longbow");
		}

		public static class Staves {
			public static readonly WeaponProto.ID WoodenStaff = newId("WoodenStaff");
			public static readonly WeaponProto.ID ApprenticeStaff = newId("ApprenticeStaff");
			public static readonly WeaponProto.ID ArcaneStaff = newId("ArcaneStaff");
		}

		public static class Maces {
			public static readonly WeaponProto.ID Club = newId("Club");
			public static readonly WeaponProto.ID IronMace = newId("IronMace");
			public static readonly WeaponProto.ID SteelMace = newId("SteelMace");
			public static readonly WeaponProto.ID HolyMace = newId("HolyMace");
		}

		private static WeaponProto.ID newId(string name) {
			return new WeaponProto.ID($"Weapon_{name}");
		}
	}
}

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
/// Partial class containing  A r m o r.Trim() IDs.
/// Fill in the corresponding nested static classes here.
/// </summary>
public static partial class Ids
{
	// ═══════════════════════════════════════════════════════════════════════
	// ARMOR
	// ═══════════════════════════════════════════════════════════════════════

	public static class Armor {
		public static class Cloth {
			public static readonly ArmorProto.ID ClothRobe = newId("ClothRobe");
			public static readonly ArmorProto.ID ApprenticeRobe = newId("ApprenticeRobe");
			public static readonly ArmorProto.ID TimeWardenRobe = newId("TimeWardenRobe");
			public static readonly ArmorProto.ID MageRobe = newId("MageRobe");
		}

		public static class Light {
			public static readonly ArmorProto.ID LeatherArmor = newId("LeatherArmor");
			public static readonly ArmorProto.ID StuddedLeather = newId("StuddedLeather");
		}

		public static class Medium {
			public static readonly ArmorProto.ID ChainMail = newId("ChainMail");
			public static readonly ArmorProto.ID ScaleMail = newId("ScaleMail");
		}

		public static class Heavy {
			public static readonly ArmorProto.ID PlateArmor = newId("PlateArmor");
		}

		public static class Helmets {
			public static readonly ArmorProto.ID LeatherCap = newId("LeatherCap");
			public static readonly ArmorProto.ID IronHelm = newId("IronHelm");
		}

		public static class Shields {
			public static readonly ArmorProto.ID WoodenShield = newId("WoodenShield");
			public static readonly ArmorProto.ID IronShield = newId("IronShield");
			public static readonly ArmorProto.ID TowerShield = newId("TowerShield");
		}

		private static ArmorProto.ID newId(string name) {
			return new ArmorProto.ID($"Armor_{name}");
		}
	}
}

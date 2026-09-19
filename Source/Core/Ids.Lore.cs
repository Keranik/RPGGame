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
/// Partial class containing  L o r e.Trim() IDs.
/// Fill in the corresponding nested static classes here.
/// </summary>
public static partial class Ids
{
    
	// ═══════════════════════════════════════════════════════════════════════
	// LORE
	// ═══════════════════════════════════════════════════════════════════════

	public static class Lore {
		public static class World {
			public static readonly LoreProto.ID Introduction = newId("World_Introduction");
			public static readonly LoreProto.ID Anchors = newId("World_Anchors");
			public static readonly LoreProto.ID Wanderers = newId("World_Wanderers");
		}

		public static class Fog {
			public static readonly LoreProto.ID Nature01 = newId("Fog_Nature01");
			public static readonly LoreProto.ID Origin01 = newId("Fog_Origin01");
			public static readonly LoreProto.ID TimeLoop01 = newId("Fog_TimeLoop01");
			public static readonly LoreProto.ID Clue01 = newId("Fog_Clue01");
			public static readonly LoreProto.ID Clue02 = newId("Fog_Clue02");
			public static readonly LoreProto.ID Clue03 = newId("Fog_Clue03");
			public static readonly LoreProto.ID Clue04 = newId("Fog_Clue04");
			public static readonly LoreProto.ID Clue05 = newId("Fog_Clue05");
			public static readonly LoreProto.ID Voices01 = newId("Fog_Voices01");
			public static readonly LoreProto.ID TimeLoop02 = newId("Fog_TimeLoop02");
			public static readonly LoreProto.ID TimeLoop03 = newId("Fog_TimeLoop03");
			public static readonly LoreProto.ID LeyLines01 = newId("Fog_LeyLines01");
		}

		public static class History {
			public static readonly LoreProto.ID BeforeFog = newId("History_BeforeFog");
			public static readonly LoreProto.ID Unmaking = newId("History_Unmaking");
			public static readonly LoreProto.ID FirstWanderers = newId("History_FirstWanderers");
		}

		public static class Village {
			public static readonly LoreProto.ID Overview = newId("Village_Overview");
			public static readonly LoreProto.ID TheAnchor = newId("Village_TheAnchor");
			public static readonly LoreProto.ID Elder = newId("Village_Elder");
			public static readonly LoreProto.ID Blacksmith = newId("Village_Blacksmith");
		}

		public static class Bestiary {
			public static readonly LoreProto.ID FogTouched = newId("Bestiary_FogTouched");
			public static readonly LoreProto.ID Undead = newId("Bestiary_Undead");
			public static readonly LoreProto.ID Demons = newId("Bestiary_Demons");
		}

		public static class Magic {
			public static readonly LoreProto.ID Overview = newId("Magic_Overview");
			public static readonly LoreProto.ID Temporal = newId("Magic_Temporal");
			public static readonly LoreProto.ID Holy = newId("Magic_Holy");
			public static readonly LoreProto.ID Arcane = newId("Magic_Arcane");
		}

		private static LoreProto.ID newId(string name) {
			return new LoreProto.ID($"Lore_{name}");
		}
	}
}

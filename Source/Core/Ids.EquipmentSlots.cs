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
/// Partial class containing  E q u i p m e n t S l o t s.Trim() IDs.
/// Fill in the corresponding nested static classes here.
/// </summary>
public static partial class Ids
{
    	// ═══════════════════════════════════════════════════════════════════════
	// EQUIPMENT SLOTS
	// ═══════════════════════════════════════════════════════════════════════

	public static class EquipmentSlots {
		private static EquipmentSlotProto.ID newSlot(string name) => new($"Slot_{name}");

		// Core Armor Slots
		public static readonly EquipmentSlotProto.ID Head = newSlot("Head");
		public static readonly EquipmentSlotProto.ID Chest = newSlot("Chest");
		public static readonly EquipmentSlotProto.ID Legs = newSlot("Legs");
		public static readonly EquipmentSlotProto.ID Feet = newSlot("Feet");
		public static readonly EquipmentSlotProto.ID Hands = newSlot("Hands");
		public static readonly EquipmentSlotProto.ID Back = newSlot("Back");
		public static readonly EquipmentSlotProto.ID Waist = newSlot("Waist");

		// Weapon Slots
		public static readonly EquipmentSlotProto.ID MainHand = newSlot("MainHand");
		public static readonly EquipmentSlotProto.ID OffHand = newSlot("OffHand");
		public static readonly EquipmentSlotProto.ID BothHands = newSlot("BothHands");
		public static readonly EquipmentSlotProto.ID Quiver = newSlot("Quiver");

		// Accessory Slots
		public static readonly EquipmentSlotProto.ID Ring1 = newSlot("Ring1");
		public static readonly EquipmentSlotProto.ID Ring2 = newSlot("Ring2");
		public static readonly EquipmentSlotProto.ID Amulet = newSlot("Amulet");
		public static readonly EquipmentSlotProto.ID Trinket = newSlot("Trinket");

		// Class-Specific Slots (examples for future expansion)
		public static readonly EquipmentSlotProto.ID DruidTotem = newSlot("DruidTotem");
		public static readonly EquipmentSlotProto.ID WarlockPact = newSlot("WarlockPact");
		public static readonly EquipmentSlotProto.ID MonkFocus = newSlot("MonkFocus");
	}
}

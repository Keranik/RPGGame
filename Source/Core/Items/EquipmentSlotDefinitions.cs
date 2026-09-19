using RPGGame.Core.Prototypes;
using RPGGame.Core.Prototypes.Item.Equipment;
using UnityEngine;

namespace RPGGame.Core.Items;

/// <summary>
/// Defines all equipment slots using the Proto system.
/// Slot positions are normalized (0-1) for the paper doll display.
/// </summary>
public class EquipmentSlotDefinitions : ICoreData {
	public void GameData(GameDb gameDatabase) {
		RegisterCoreArmorSlots(gameDatabase);
		RegisterWeaponSlots(gameDatabase);
		RegisterAccessorySlots(gameDatabase);
		RegisterClassSpecificSlots(gameDatabase);
	}

	#region Core Armor Slots

	private void RegisterCoreArmorSlots(GameDb db) {
		db.RegisterProto(new EquipmentSlotProto(
			Ids.EquipmentSlots.Head,
			Proto.CreateText("Head", "Helmets, hoods, and headgear.")
		) {
			EmptySlotIcon = "icon_slot_head",
			Category = EquipmentSlotCategory.Armor,
			PaperDollPosition = new Vector2(0.5f, 0.05f),
			DisplayOrder = 1,
			LegacySlotType = SlotType.Head
		});

		db.RegisterProto(new EquipmentSlotProto(
			Ids.EquipmentSlots.Chest,
			Proto.CreateText("Chest", "Armor, robes, and clothing for the torso.")
		) {
			EmptySlotIcon = "icon_slot_chest",
			Category = EquipmentSlotCategory.Armor,
			PaperDollPosition = new Vector2(0.5f, 0.25f),
			SlotSize = new Vector2(1.5f, 1.5f),
			DisplayOrder = 2,
			LegacySlotType = SlotType.Chest
		});

		db.RegisterProto(new EquipmentSlotProto(
			Ids.EquipmentSlots.Hands,
			Proto.CreateText("Hands", "Gloves and gauntlets.")
		) {
			EmptySlotIcon = "icon_slot_hands",
			Category = EquipmentSlotCategory.Armor,
			PaperDollPosition = new Vector2(0.15f, 0.35f),
			DisplayOrder = 5,
			LegacySlotType = SlotType.Hands
		});

		db.RegisterProto(new EquipmentSlotProto(
			Ids.EquipmentSlots.Legs,
			Proto.CreateText("Legs", "Leggings, pants, and leg armor.")
		) {
			EmptySlotIcon = "icon_slot_legs",
			Category = EquipmentSlotCategory.Armor,
			PaperDollPosition = new Vector2(0.5f, 0.55f),
			DisplayOrder = 3,
			LegacySlotType = SlotType.Legs
		});

		db.RegisterProto(new EquipmentSlotProto(
			Ids.EquipmentSlots.Feet,
			Proto.CreateText("Feet", "Boots and footwear.")
		) {
			EmptySlotIcon = "icon_slot_feet",
			Category = EquipmentSlotCategory.Armor,
			PaperDollPosition = new Vector2(0.5f, 0.85f),
			DisplayOrder = 4,
			LegacySlotType = SlotType.Feet
		});

		db.RegisterProto(new EquipmentSlotProto(
			Ids.EquipmentSlots.Back,
			Proto.CreateText("Back", "Cloaks and capes.")
		) {
			EmptySlotIcon = "icon_slot_back",
			Category = EquipmentSlotCategory.Armor,
			PaperDollPosition = new Vector2(0.85f, 0.2f),
			DisplayOrder = 6,
			LegacySlotType = SlotType.Back
		});

		db.RegisterProto(new EquipmentSlotProto(
			Ids.EquipmentSlots.Waist,
			Proto.CreateText("Waist", "Belts and sashes.")
		) {
			EmptySlotIcon = "icon_slot_waist",
			Category = EquipmentSlotCategory.Armor,
			PaperDollPosition = new Vector2(0.5f, 0.45f),
			DisplayOrder = 7,
			LegacySlotType = SlotType.Waist
		});
	}

	#endregion

	#region Weapon Slots

	private void RegisterWeaponSlots(GameDb db) {
		db.RegisterProto(new EquipmentSlotProto(
			Ids.EquipmentSlots.MainHand,
			Proto.CreateText("Main Hand", "Primary weapon.")
		) {
			EmptySlotIcon = "icon_slot_mainhand",
			Category = EquipmentSlotCategory.Weapon,
			PaperDollPosition = new Vector2(0.1f, 0.5f),
			DisplayOrder = 10,
			MutuallyExclusiveSlots = [Ids.EquipmentSlots.BothHands],
			LegacySlotType = SlotType.MainHand
		});

		db.RegisterProto(new EquipmentSlotProto(
			Ids.EquipmentSlots.OffHand,
			Proto.CreateText("Off Hand", "Shield or secondary weapon.")
		) {
			EmptySlotIcon = "icon_slot_offhand",
			Category = EquipmentSlotCategory.Weapon,
			PaperDollPosition = new Vector2(0.9f, 0.5f),
			DisplayOrder = 11,
			MutuallyExclusiveSlots = [Ids.EquipmentSlots.BothHands],
			LegacySlotType = SlotType.OffHand
		});

		db.RegisterProto(new EquipmentSlotProto(
			Ids.EquipmentSlots.BothHands,
			Proto.CreateText("Both Hands", "Two-handed weapons.")
		) {
			EmptySlotIcon = "icon_slot_twohanded",
			Category = EquipmentSlotCategory.Weapon,
			PaperDollPosition = new Vector2(0.1f, 0.5f),
			SlotSize = new Vector2(1f, 1.5f),
			DisplayOrder = 12,
			MutuallyExclusiveSlots = [Ids.EquipmentSlots.MainHand, Ids.EquipmentSlots.OffHand],
			RequiresEmptySlots = [Ids.EquipmentSlots.MainHand, Ids.EquipmentSlots.OffHand],
			IsCoreSlot = false, // Virtual slot - shown only when two-handed equipped
			LegacySlotType = SlotType.BothHands
		});

		db.RegisterProto(new EquipmentSlotProto(
			Ids.EquipmentSlots.Quiver,
			Proto.CreateText("Quiver", "Ammunition for ranged weapons.")
		) {
			EmptySlotIcon = "icon_slot_quiver",
			Category = EquipmentSlotCategory.Weapon,
			PaperDollPosition = new Vector2(0.85f, 0.35f),
			DisplayOrder = 13,
			LegacySlotType = SlotType.Quiver
		});
	}

	#endregion

	#region Accessory Slots

	private void RegisterAccessorySlots(GameDb db) {
		db.RegisterProto(new EquipmentSlotProto(
			Ids.EquipmentSlots.Ring1,
			Proto.CreateText("Ring", "A magical ring.")
		) {
			EmptySlotIcon = "icon_slot_ring",
			Category = EquipmentSlotCategory.Accessory,
			PaperDollPosition = new Vector2(0.2f, 0.7f),
			DisplayOrder = 20,
			LegacySlotType = SlotType.Ring1
		});

		db.RegisterProto(new EquipmentSlotProto(
			Ids.EquipmentSlots.Ring2,
			Proto.CreateText("Ring", "A magical ring.")
		) {
			EmptySlotIcon = "icon_slot_ring",
			Category = EquipmentSlotCategory.Accessory,
			PaperDollPosition = new Vector2(0.8f, 0.7f),
			DisplayOrder = 21,
			LegacySlotType = SlotType.Ring2
		});

		db.RegisterProto(new EquipmentSlotProto(
			Ids.EquipmentSlots.Amulet,
			Proto.CreateText("Amulet", "A neck amulet or pendant.")
		) {
			EmptySlotIcon = "icon_slot_amulet",
			Category = EquipmentSlotCategory.Accessory,
			PaperDollPosition = new Vector2(0.5f, 0.15f),
			DisplayOrder = 22,
			LegacySlotType = SlotType.Amulet
		});

		db.RegisterProto(new EquipmentSlotProto(
			Ids.EquipmentSlots.Trinket,
			Proto.CreateText("Trinket", "A magical trinket or charm.")
		) {
			EmptySlotIcon = "icon_slot_trinket",
			Category = EquipmentSlotCategory.Accessory,
			PaperDollPosition = new Vector2(0.15f, 0.15f),
			DisplayOrder = 23,
			LegacySlotType = SlotType.Trinket
		});
	}

	#endregion

	#region Class-Specific Slots

	private void RegisterClassSpecificSlots(GameDb db) {
		// Druid Totem - Only available to Druids
		db.RegisterProto(new EquipmentSlotProto(
			Ids.EquipmentSlots.DruidTotem,
			Proto.CreateText("Nature Totem", "A totem that channels nature's power.")
		) {
			EmptySlotIcon = "icon_slot_totem",
			Category = EquipmentSlotCategory.Special,
			PaperDollPosition = new Vector2(0.85f, 0.6f),
			DisplayOrder = 30,
			ClassRestrictions = [Ids.CharacterClasses.Druid],
			IsCoreSlot = false
		});

		//// Warlock Pact - Only available to Warlocks
		//db.RegisterProto(new EquipmentSlotProto(
		//	Ids.EquipmentSlots.WarlockPact,
		//	Proto.CreateText("Pact Token", "A token of your otherworldly pact.")
		//) {
		//	EmptySlotIcon = "icon_slot_pact",
		//	Category = EquipmentSlotCategory.Special,
		//	PaperDollPosition = new Vector2(0.85f, 0.6f),
		//	DisplayOrder = 31,
		//	ClassRestrictions = [Ids.CharacterClasses.Warlock],
		//	IsCoreSlot = false
		//});

		//// Monk Focus - Only available to Monks
		//db.RegisterProto(new EquipmentSlotProto(
		//	Ids.EquipmentSlots.MonkFocus,
		//	Proto.CreateText("Ki Focus", "An item that focuses your inner energy.")
		//) {
		//	EmptySlotIcon = "icon_slot_ki",
		//	Category = EquipmentSlotCategory.Special,
		//	PaperDollPosition = new Vector2(0.85f, 0.6f),
		//	DisplayOrder = 32,
		//	ClassRestrictions = [Ids.CharacterClasses.Monk],
		//	IsCoreSlot = false
		//});
	}

	#endregion
}
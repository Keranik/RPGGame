using RPGGame.Core.Items;
using RPGGame.Core.Prototypes.Characters;
using UnityEngine;

namespace RPGGame.Core.Prototypes.Item.Equipment;

/// <summary>
/// Prototype defining an equipment slot.
/// Supports dynamic slot definitions with class restrictions and UI positioning.
/// </summary>
public class EquipmentSlotProto : Proto {
	new public readonly struct ID(string value) : IEquatable<ID>, IComparable<ID> {
		public readonly string Value = value;

		public bool Equals(ID other) => Value == other.Value;
		public int CompareTo(ID other) => string.Compare(Value, other.Value, StringComparison.Ordinal);
		public override string ToString() => Value;
		public override bool Equals(object? obj) => obj is ID other && Equals(other);
		public override int GetHashCode() => Value?.GetHashCode() ?? 0;

		public static bool operator ==(Proto.ID lhs, ID rhs) => string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static bool operator ==(ID lhs, Proto.ID rhs) => string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static bool operator !=(Proto.ID lhs, ID rhs) => !string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static bool operator !=(ID lhs, Proto.ID rhs) => !string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static bool operator ==(ID lhs, ID rhs) => string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static bool operator !=(ID lhs, ID rhs) => !string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static implicit operator Proto.ID(ID id) => new(id.Value);
	}

	new public ID Id => new(base.Id.Value);

	/// <summary>
	/// Icon to display for an empty slot.
	/// </summary>
	public string EmptySlotIcon { get; init; } = "icon_slot_empty";

	/// <summary>
	/// Slot category for grouping (e.g., Armor, Weapon, Accessory).
	/// </summary>
	public EquipmentSlotCategory Category { get; init; } = EquipmentSlotCategory.Armor;

	/// <summary>
	/// Position on the paper doll (normalized 0-1 coordinates).
	/// </summary>
	public Vector2 PaperDollPosition { get; init; } = Vector2.zero;

	/// <summary>
	/// Size of the slot on the paper doll (1 = standard, 2 = double width/height).
	/// </summary>
	public Vector2 SlotSize { get; init; } = Vector2.one;

	/// <summary>
	/// Z-order for layering on the paper doll.
	/// </summary>
	public int ZOrder { get; init; } = 0;

	/// <summary>
	/// Classes that have access to this slot. Empty = all classes.
	/// </summary>
	public List<CharacterClassProto.ID> ClassRestrictions { get; init; } = [];

	/// <summary>
	/// Races that have access to this slot. Empty = all races.
	/// </summary>
	public List<string> RaceRestrictions { get; init; } = [];

	/// <summary>
	/// Other slots that are mutually exclusive (e.g., BothHands blocks MainHand/OffHand).
	/// </summary>
	public List<ID> MutuallyExclusiveSlots { get; init; } = [];

	/// <summary>
	/// Required slots that must be empty for this to be used.
	/// </summary>
	public List<ID> RequiresEmptySlots { get; init; } = [];

	/// <summary>
	/// Minimum player level to unlock this slot.
	/// </summary>
	public int UnlocksAtLevel { get; init; } = 1;

	/// <summary>
	/// Whether this is a core slot that's always visible (vs. unlockable).
	/// </summary>
	public bool IsCoreSlot { get; init; } = true;

	/// <summary>
	/// Display order in equipment lists.
	/// </summary>
	public int DisplayOrder { get; init; } = 0;

	/// <summary>
	/// Legacy SlotType mapping for compatibility.
	/// </summary>
	public SlotType? LegacySlotType { get; init; }

	public EquipmentSlotProto(ID id, Loc text) : base(id, text) {
	}

	/// <summary>
	/// Checks if a class has access to this slot.
	/// </summary>
	public bool IsAvailableToClass(CharacterClassProto.ID classId) {
		if (ClassRestrictions.Count == 0) {
			return true;
		}
		return ClassRestrictions.Contains(classId);
	}

	/// <summary>
	/// Checks if a character meets the level requirement for this slot.
	/// </summary>
	public bool IsUnlockedAtLevel(int level) {
		return level >= UnlocksAtLevel;
	}
}

/// <summary>
/// Categories for equipment slots.
/// </summary>
public enum EquipmentSlotCategory {
	Weapon,
	Armor,
	Accessory,
	Special
}
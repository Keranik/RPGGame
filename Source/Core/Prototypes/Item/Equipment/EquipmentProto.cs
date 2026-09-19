using RPGGame.Core.Characters;
using RPGGame.Core.Items;
using RPGGame.Core.Prototypes.Characters;
using RPGGame.Core.Prototypes.Item;

namespace RPGGame.Core.Prototypes.Item.Equipment;

/// <summary>
/// Base prototype for all equippable items.
/// Inherits from ItemProto and adds equipment-specific properties.
/// </summary>
public abstract class EquipmentProto : ItemProto {
	// Define the prefix specific to equipment
	public const string EQUIPMENT_PREFIX = "Equipment_";

	// Override the ID with equipment-specific logic
	new public ID Id { get; }

	// EquipmentProto-specific ID struct
	new public readonly struct ID : IEquatable<ID>, IComparable<ID> {
		public readonly string Value;

		public ID(string value) {
			Value = value;
		}

		public bool Equals(ID other) => Value == other.Value;
		public int CompareTo(ID other) => string.Compare(Value, other.Value, StringComparison.Ordinal);
		public override string ToString() => Value;
		public override bool Equals(object obj) => obj is ID other && Equals(other);
		public override int GetHashCode() => Value?.GetHashCode() ?? 0;

		public static bool operator ==(Proto.ID lhs, ID rhs) => string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static bool operator ==(ID lhs, Proto.ID rhs) => string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static bool operator !=(Proto.ID lhs, ID rhs) => !string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static bool operator !=(ID lhs, Proto.ID rhs) => !string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static bool operator ==(ID lhs, ID rhs) => string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static bool operator !=(ID lhs, ID rhs) => !string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);

		public static implicit operator Proto.ID(ID equipId) => new Proto.ID(equipId.Value);
		public static implicit operator ItemProto.ID(ID equipId) => new ItemProto.ID(equipId.Value);
	}

	// ═══════════════════════════════════════════════════════════════
	// Equipment-Specific Properties (not in base ItemProto)
	// ═══════════════════════════════════════════════════════════════

	/// <summary>
	/// Physical weight of the equipment.
	/// </summary>
	public float Weight { get; }

	/// <summary>
	/// Maximum durability of the equipment.
	/// </summary>
	public int Durability { get; }

	/// <summary>
	/// Whether this is a two-handed item (weapons/shields).
	/// </summary>
	public bool IsTwoHanded { get; }

	/// <summary>
	/// Creates a new equipment prototype.
	/// </summary>
	protected EquipmentProto(
		ID id,
		Loc text,
		string iconName,
		RarityType rarity,
		int buyPrice,
		SlotType equipSlot,
		int itemLevel = 1,
		int requiredLevel = 0,
		int? sellPrice = null,
		IReadOnlyList<EquipmentStat>? equipStats = null,
		float weight = 1f,
		int durability = 100,
		bool isTwoHanded = false,
		IReadOnlyList<CharacterClassProto.ID>? classRestrictions = null,
		string? flavorText = null
	) : base(
		id: new ItemProto.ID(id.Value),
		text: text,
		iconName: iconName,
		category: ItemCategory.Weapon, // Will be overridden by subclass determination
		type: ItemType.Weapon, // Will be overridden by subclass determination
		rarity: rarity,
		buyPrice: buyPrice,
		sellPrice: sellPrice,
		itemLevel: itemLevel,
		requiredLevel: requiredLevel,
		isStackable: false, // Equipment never stacks
		maxStackSize: 1,
		equipSlot: equipSlot, // Non-nullable for equipment
		equipStats: equipStats,
		maxUses: 0,
		useEffects: null,
		classRestrictions: classRestrictions,
		flavorText: flavorText
	) {
		Id = id;
		Weight = weight;
		Durability = durability;
		IsTwoHanded = isTwoHanded;
	}

	/// <summary>
	/// Creates a new equipment prototype with explicit category and type.
	/// </summary>
	protected EquipmentProto(
		ID id,
		Loc text,
		string iconName,
		ItemCategory category,
		ItemType type,
		RarityType rarity,
		int buyPrice,
		SlotType equipSlot,
		int itemLevel = 1,
		int requiredLevel = 0,
		int? sellPrice = null,
		IReadOnlyList<EquipmentStat>? equipStats = null,
		float weight = 1f,
		int durability = 100,
		bool isTwoHanded = false,
		IReadOnlyList<CharacterClassProto.ID>? classRestrictions = null,
		string? flavorText = null
	) : base(
		id: new ItemProto.ID(id.Value),
		text: text,
		iconName: iconName,
		category: category,
		type: type,
		rarity: rarity,
		buyPrice: buyPrice,
		sellPrice: sellPrice,
		itemLevel: itemLevel,
		requiredLevel: requiredLevel,
		isStackable: false, // Equipment never stacks
		maxStackSize: 1,
		equipSlot: equipSlot,
		equipStats: equipStats,
		maxUses: 0,
		useEffects: null,
		classRestrictions: classRestrictions,
		flavorText: flavorText
	) {
		Id = id;
		Weight = weight;
		Durability = durability;
		IsTwoHanded = isTwoHanded;
	}
}
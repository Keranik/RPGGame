using RPGGame.Core.Characters;
using RPGGame.Core.Items;
using RPGGame.Core.Prototypes.Characters;

namespace RPGGame.Core.Prototypes.Item.Equipment.Armor;

/// <summary>
/// Prototype for armor items.
/// </summary>
public class ArmorProto : EquipmentProto {
	// Define the prefix specific to armor
	private const string ARMOR_PREFIX = "Armor_";

	// Override the ID with armor-specific logic
	new public ID Id { get; }

	// ArmorProto-specific ID struct
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

		public static implicit operator EquipmentProto.ID(ID armorId) => new EquipmentProto.ID(armorId.Value);
		public static implicit operator ItemProto.ID(ID armorId) => new ItemProto.ID(armorId.Value);
		public static implicit operator Proto.ID(ID armorId) => new Proto.ID(armorId.Value);
	}

	// ═══════════════════════════════════════════════════════════════
	// Armor Properties
	// ═══════════════════════════════════════════════════════════════
	public ArmorType ArmorType { get; }
	public int ArmorBonus { get; }
	public int MaxDexBonus { get; }
	public bool HasStealthPenalty { get; }

	/// <summary>
	/// Creates a new armor prototype.
	/// </summary>
	public ArmorProto(
		ID id,
		Loc text,
		string iconName,
		RarityType rarity,
		int buyPrice,
		SlotType equipSlot,
		ArmorType armorType,
		int armorBonus,
		int itemLevel = 1,
		int requiredLevel = 0,
		int? sellPrice = null,
		IReadOnlyList<EquipmentStat>? equipStats = null,
		float weight = 1f,
		int durability = 100,
		int maxDexBonus = 99,
		bool hasStealthPenalty = false,
		IReadOnlyList<CharacterClassProto.ID>? classRestrictions = null,
		string? flavorText = null
	) : base(
		id: new EquipmentProto.ID(id.Value),
		text: text,
		iconName: iconName,
		category: ItemCategory.Armor,
		type: ItemType.Armor,
		rarity: rarity,
		buyPrice: buyPrice,
		equipSlot: equipSlot,
		itemLevel: itemLevel,
		requiredLevel: requiredLevel,
		sellPrice: sellPrice,
		equipStats: equipStats,
		weight: weight,
		durability: durability,
		isTwoHanded: false,
		classRestrictions: classRestrictions,
		flavorText: flavorText
	) {
		Id = id;
		ArmorType = armorType;
		ArmorBonus = armorBonus;
		MaxDexBonus = maxDexBonus;
		HasStealthPenalty = hasStealthPenalty;
	}
}
using RPGGame.Core.Characters;
using RPGGame.Core.Items;
using RPGGame.Core.Prototypes.Characters;

namespace RPGGame.Core.Prototypes.Item;

/// <summary>
/// Base prototype for all items in the game.
/// </summary>
public class ItemProto : Proto {
	// Define the prefix specific to items
	public const string ITEM_PREFIX = "Item_";

	// Override the ID with item-specific logic
	new public ID Id { get; }

	// ItemProto-specific ID struct
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

		public static implicit operator Proto.ID(ID itemId) => new Proto.ID(itemId.Value);
	}

	// ═══════════════════════════════════════════════════════════════
	// Core Properties
	// ═══════════════════════════════════════════════════════════════
	public string IconName { get; }
	public ItemCategory Category { get; }
	public ItemType Type { get; }
	public RarityType Rarity { get; }

	// ═══════════════════════════════════════════════════════════════
	// Value Properties
	// ═══════════════════════════════════════════════════════════════
	public int BuyPrice { get; }
	public int SellPrice { get; }
	public int ItemLevel { get; }
	public int RequiredLevel { get; }

	// ═══════════════════════════════════════════════════════════════
	// Stack Properties
	// ═══════════════════════════════════════════════════════════════
	public bool IsStackable { get; }
	public int MaxStackSize { get; }

	// ═══════════════════════════════════════════════════════════════
	// Equipment Properties (for equippable items)
	// ═══════════════════════════════════════════════════════════════
	public SlotType? EquipSlot { get; }
	public IReadOnlyList<EquipmentStat> EquipStats { get; }

	// ═══════════════════════════════════════════════════════════════
	// Use Properties (for consumables)
	// ═══════════════════════════════════════════════════════════════
	public int MaxUses { get; }
	public IReadOnlyList<ItemEffect> UseEffects { get; }

	// ═══════════════════════════════════════════════════════════════
	// Restrictions
	// ═══════════════════════════════════════════════════════════════
	public IReadOnlyList<CharacterClassProto.ID> ClassRestrictions { get; }

	// ═══════════════════════════════════════════════════════════════
	// Flavor
	// ═══════════════════════════════════════════════════════════════
	public string FlavorText { get; }

	/// <summary>
	/// Creates a new item prototype.
	/// </summary>
	public ItemProto(
		ID id,
		Loc text,
		string iconName,
		ItemCategory category,
		ItemType type,
		RarityType rarity = RarityType.Common,
		int buyPrice = 0,
		int? sellPrice = null,
		int itemLevel = 1,
		int requiredLevel = 0,
		bool isStackable = false,
		int maxStackSize = 1,
		SlotType? equipSlot = null,
		IReadOnlyList<EquipmentStat>? equipStats = null,
		int maxUses = 0,
		IReadOnlyList<ItemEffect>? useEffects = null,
		IReadOnlyList<CharacterClassProto.ID>? classRestrictions = null,
		string? flavorText = null
	) : base(id, text) {
		Id = id;
		IconName = iconName;
		Category = category;
		Type = type;
		Rarity = rarity;
		BuyPrice = buyPrice;
		SellPrice = sellPrice ?? (buyPrice / 2);
		ItemLevel = itemLevel;
		RequiredLevel = requiredLevel;
		IsStackable = isStackable;
		MaxStackSize = isStackable ? maxStackSize : 1;
		EquipSlot = equipSlot;
		EquipStats = equipStats ?? [];
		MaxUses = maxUses;
		UseEffects = useEffects ?? [];
		ClassRestrictions = classRestrictions ?? [];
		FlavorText = flavorText ?? string.Empty;
	}

	/// <summary>
	/// Whether this item can be equipped.
	/// </summary>
	public bool IsEquippable => EquipSlot.HasValue;

	/// <summary>
	/// Whether this item can be used (consumable with effects).
	/// </summary>
	public bool IsUsable => UseEffects.Count > 0 || MaxUses > 0;
}
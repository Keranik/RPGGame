using RPGGame.Core.Items;
using RPGGame.Core.Prototypes.Item;

namespace RPGGame.Core.Prototypes.Combat;

/// <summary>
/// Represents a potential loot drop from enemies, dungeons, chests, or other sources.
/// Supports item references, drop chances, quantity ranges, and conditions.
/// </summary>
public class LootEntry {
	#region Properties

	/// <summary>The item to drop (using ItemProto.ID for strong typing).</summary>
	public ItemProto.ID ItemId { get; init; }

	/// <summary>Chance to drop (0-1, where 1 = 100%).</summary>
	public float DropChance { get; init; } = 1f;

	/// <summary>Minimum quantity to drop.</summary>
	public int MinQuantity { get; init; } = 1;

	/// <summary>Maximum quantity to drop.</summary>
	public int MaxQuantity { get; init; } = 1;

	/// <summary>Minimum rarity for procedurally generated items.</summary>
	public RarityType MinRarity { get; init; } = RarityType.Common;

	/// <summary>Maximum rarity for procedurally generated items.</summary>
	public RarityType MaxRarity { get; init; } = RarityType.Legendary;

	/// <summary>Whether this is a guaranteed drop on first kill/clear.</summary>
	public bool FirstTimeOnly { get; init; }

	/// <summary>Required flag for this drop to be available.</summary>
	public string? RequiredFlag { get; init; }

	/// <summary>Flag that disables this drop after being set.</summary>
	public string? DisabledByFlag { get; init; }

	/// <summary>Minimum player level for this drop.</summary>
	public int MinPlayerLevel { get; init; }

	/// <summary>Maximum player level for this drop (-1 = no max).</summary>
	public int MaxPlayerLevel { get; init; } = -1;

	/// <summary>Weight for weighted random selection (higher = more likely).</summary>
	public float Weight { get; init; } = 1f;

	/// <summary>Whether to generate a procedural item instead of using ItemId directly.</summary>
	public bool IsProcedural { get; init; }

	/// <summary>Item category for procedural generation.</summary>
	public ItemCategory? ProceduralCategory { get; init; }

	#endregion

	#region Constructors

	/// <summary>
	/// Creates an empty loot entry (for object initializer syntax).
	/// </summary>
	public LootEntry() { }

	/// <summary>
	/// Creates a simple loot entry with guaranteed drop.
	/// </summary>
	public LootEntry(ItemProto.ID itemId, int quantity = 1) {
		ItemId = itemId;
		DropChance = 1f;
		MinQuantity = quantity;
		MaxQuantity = quantity;
	}

	/// <summary>
	/// Creates a loot entry with drop chance.
	/// </summary>
	public LootEntry(ItemProto.ID itemId, float dropChance, int quantity = 1) {
		ItemId = itemId;
		DropChance = dropChance;
		MinQuantity = quantity;
		MaxQuantity = quantity;
	}

	/// <summary>
	/// Creates a loot entry with drop chance and quantity range.
	/// </summary>
	public LootEntry(ItemProto.ID itemId, float dropChance, int minQuantity, int maxQuantity) {
		ItemId = itemId;
		DropChance = dropChance;
		MinQuantity = minQuantity;
		MaxQuantity = maxQuantity;
	}

	#endregion

	#region Factory Methods

	/// <summary>
	/// Creates a guaranteed single-item drop.
	/// </summary>
	public static LootEntry Guaranteed(ItemProto.ID itemId, int quantity = 1) {
		return new LootEntry(itemId, 1f, quantity, quantity);
	}

	/// <summary>
	/// Creates a drop with a specific chance.
	/// </summary>
	public static LootEntry WithChance(ItemProto.ID itemId, float chance, int quantity = 1) {
		return new LootEntry(itemId, chance, quantity, quantity);
	}

	/// <summary>
	/// Creates a drop with quantity range.
	/// </summary>
	public static LootEntry WithRange(ItemProto.ID itemId, float chance, int min, int max) {
		return new LootEntry(itemId, chance, min, max);
	}

	/// <summary>
	/// Creates a first-time-only drop (quest rewards, first clears).
	/// </summary>
	public static LootEntry FirstClear(ItemProto.ID itemId, int quantity = 1) {
		return new LootEntry {
			ItemId = itemId,
			DropChance = 1f,
			MinQuantity = quantity,
			MaxQuantity = quantity,
			FirstTimeOnly = true
		};
	}

	/// <summary>
	/// Creates a procedural item drop.
	/// </summary>
	public static LootEntry Procedural(ItemCategory category, float chance, RarityType minRarity = RarityType.Common, RarityType maxRarity = RarityType.Legendary) {
		return new LootEntry {
			IsProcedural = true,
			ProceduralCategory = category,
			DropChance = chance,
			MinRarity = minRarity,
			MaxRarity = maxRarity
		};
	}

	/// <summary>
	/// Creates a level-restricted drop.
	/// </summary>
	public static LootEntry LevelRestricted(ItemProto.ID itemId, float chance, int minLevel, int maxLevel = -1) {
		return new LootEntry {
			ItemId = itemId,
			DropChance = chance,
			MinPlayerLevel = minLevel,
			MaxPlayerLevel = maxLevel
		};
	}

	#endregion

	#region Evaluation

	/// <summary>
	/// Checks if this loot entry can drop given the current context.
	/// </summary>
	public bool CanDrop(int playerLevel, HashSet<string>? flags = null, bool isFirstTime = false) {
		// Check first-time restriction
		if (FirstTimeOnly && !isFirstTime) {
			return false;
		}

		// Check level restrictions
		if (playerLevel < MinPlayerLevel) {
			return false;
		}
		if (MaxPlayerLevel >= 0 && playerLevel > MaxPlayerLevel) {
			return false;
		}

		// Check flag requirements
		if (!string.IsNullOrEmpty(RequiredFlag) && (flags == null || !flags.Contains(RequiredFlag))) {
			return false;
		}

		// Check disabled flag
		if (!string.IsNullOrEmpty(DisabledByFlag) && flags != null && flags.Contains(DisabledByFlag)) {
			return false;
		}

		return true;
	}

	/// <summary>
	/// Rolls for quantity within the range.
	/// </summary>
	public int RollQuantity(Random random) {
		if (MinQuantity == MaxQuantity) {
			return MinQuantity;
		}
		return random.Next(MinQuantity, MaxQuantity + 1);
	}

	/// <summary>
	/// Rolls for rarity within the range.
	/// </summary>
	public RarityType RollRarity(Random random) {
		int min = (int)MinRarity;
		int max = (int)MaxRarity;
		return (RarityType)random.Next(min, max + 1);
	}

	/// <summary>
	/// Rolls to see if this item drops.
	/// </summary>
	public bool RollDrop(Random random) {
		if (DropChance >= 1f) return true;
		if (DropChance <= 0f) return false;
		return random.NextDouble() < DropChance;
	}

	#endregion

	#region Display

	public override string ToString() {
		if (IsProcedural) {
			return $"Procedural {ProceduralCategory} ({DropChance:P0})";
		}
		string qty = MinQuantity == MaxQuantity 
			? MinQuantity.ToString() 
			: $"{MinQuantity}-{MaxQuantity}";
		return $"{ItemId} x{qty} ({DropChance:P0})";
	}

	#endregion
}

/// <summary>
/// Categories for procedural item generation.
/// </summary>
public enum ItemCategory {
	Weapon,
	Armor,
	Accessory,
	Consumable,
	Material,
	Quest,
	Currency
}
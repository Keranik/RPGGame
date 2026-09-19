using RPGGame.Core.Combat;
using RPGGame.Core.Metrics;
using RPGGame.Core.Prototypes;
using RPGGame.Core.Prototypes.Item;
using RPGGame.Core.Prototypes.Item.Equipment;
using RPGGame.Core.Prototypes.Item.Equipment.Armor;
using RPGGame.Core.Prototypes.Item.Equipment.Weapon;
using RPGGame.Core.Stats;

namespace RPGGame.Core.Items;

/// <summary>
/// Manages the player's inventory and equipment.
/// </summary>
[Dependency(RegistrationType.Singleton)]
public class InventoryManager {
	#region Fields

	private readonly GameDb k_gameDb;
	private readonly MetricsManager k_metrics;

	private readonly List<ItemInstance> k_items = [];
	private readonly Dictionary<SlotType, ItemInstance?> k_equipment = [];

	private int k_maxSlots = 20;
	private int k_gold = 0;

	#endregion

	#region Properties

	/// <summary>
	/// All items in inventory.
	/// </summary>
	public IReadOnlyList<ItemInstance> Items => k_items;

	/// <summary>
	/// All equipped items.
	/// </summary>
	public IReadOnlyDictionary<SlotType, ItemInstance?> Equipment => k_equipment;

	/// <summary>
	/// Current gold.
	/// </summary>
	public int Gold {
		get => k_gold;
		set {
			int oldGold = k_gold;
			k_gold = Math.Max(0, value);
			if (k_gold != oldGold) {
				OnGoldChanged?.Invoke(k_gold, k_gold - oldGold);
			}
		}
	}

	/// <summary>
	/// Current number of inventory slots used.
	/// </summary>
	public int UsedSlots => k_items.Count;

	/// <summary>
	/// Maximum inventory slots.
	/// </summary>
	public int MaxSlots {
		get => k_maxSlots;
		set => k_maxSlots = Math.Max(1, value);
	}

	/// <summary>
	/// Available slots.
	/// </summary>
	public int AvailableSlots => MaxSlots - UsedSlots;

	/// <summary>
	/// Whether inventory is full.
	/// </summary>
	public bool IsFull => UsedSlots >= MaxSlots;

	/// <summary>
	/// Total weight of inventory.
	/// </summary>
	public float TotalWeight => k_items.Sum(i => GetItemWeight(i.Prototype) * i.Count);

	/// <summary>
	/// Total value of inventory (sell prices).
	/// </summary>
	public int TotalValue => k_items.Sum(i => i.TotalSellValue);

	#endregion

	#region Events

	public event Action<ItemInstance>? OnItemAdded;
	public event Action<ItemInstance>? OnItemRemoved;
	public event Action<ItemInstance, SlotType>? OnItemEquipped;
	public event Action<ItemInstance, SlotType>? OnItemUnequipped;
	public event Action<int, int>? OnGoldChanged;
	public event Action? OnInventoryChanged;

	#endregion

	#region Constructor

	public InventoryManager(GameDb gameDb, MetricsManager metrics) {
		k_gameDb = gameDb;
		k_metrics = metrics;

		// Initialize equipment slots
		foreach (SlotType slot in Enum.GetValues(typeof(SlotType))) {
			k_equipment[slot] = null;
		}

		UnityEngine.Debug.Log("InventoryManager initialized");
	}

	#endregion

	#region Helper Methods for Proto Access

	/// <summary>
	/// Gets the weight of an item from its proto hierarchy.
	/// </summary>
	private static float GetItemWeight(ItemProto proto) {
		return proto switch {
			EquipmentProto equipment => equipment.Weight,
			_ => 0.1f // Default weight for non-equipment
		};
	}

	/// <summary>
	/// Gets the armor bonus from an item if it's armor.
	/// </summary>
	private static int GetArmorBonus(ItemProto proto) {
		return proto switch {
			ArmorProto armor => armor.ArmorBonus,
			_ => 0
		};
	}

	/// <summary>
	/// Checks if an item is two-handed.
	/// </summary>
	private static bool IsTwoHanded(ItemProto proto) {
		return proto switch {
			WeaponProto weapon => weapon.IsTwoHanded,
			_ => false
		};
	}

	/// <summary>
	/// Gets damage dice from a weapon.
	/// </summary>
	private static string GetDamageDice(ItemProto proto) {
		return proto switch {
			WeaponProto weapon => weapon.DamageDice.ToString(),
			_ => "1d4" // Unarmed
		};
	}

	/// <summary>
	/// Gets damage type from a weapon.
	/// </summary>
	private static DamageType GetDamageType(ItemProto proto) {
		return proto switch {
			WeaponProto weapon => weapon.DamageType,
			_ => DamageType.Bludgeoning // Unarmed
		};
	}

	/// <summary>
	/// Checks if an item is equippable.
	/// </summary>
	private static bool IsEquippable(ItemProto proto) {
		return proto.EquipSlot.HasValue || proto is EquipmentProto;
	}

	/// <summary>
	/// Checks if an item is usable.
	/// </summary>
	private static bool IsUsable(ItemProto proto) {
		return proto.UseEffects.Count > 0;
	}

	/// <summary>
	/// Checks if an item can be sold.
	/// </summary>
	private static bool CanSell(ItemProto proto) {
		return proto.Category != ItemCategory.Quest && proto.SellPrice > 0;
	}

	/// <summary>
	/// Checks if an item is a quest item.
	/// </summary>
	private static bool IsQuestItem(ItemProto proto) {
		return proto.Category == ItemCategory.Quest;
	}

	/// <summary>
	/// Gets the item ID as a string.
	/// </summary>
	private static string GetItemIdString(ItemProto proto) {
		return proto.Id.Value;
	}

	#endregion

	#region Add/Remove Items

	/// <summary>
	/// Adds an item to inventory by ID.
	/// </summary>
	public ItemInstance? AddItem(Proto.ID itemId, int count = 1) {
		if (!k_gameDb.TryGetProto<ItemProto>(itemId, out var proto)) {
			UnityEngine.Debug.LogError($"Item not found: {itemId}");
			return null;
		}

		return AddItem(proto, count);
	}

	/// <summary>
	/// Adds an item to inventory by string ID.
	/// </summary>
	public ItemInstance? AddItem(string itemId, int count = 1) {
		return AddItem(new Proto.ID(itemId), count);
	}

	/// <summary>
	/// Adds an item to inventory.
	/// </summary>
	public ItemInstance? AddItem(ItemProto proto, int count = 1) {
		if (count <= 0) {
			return null;
		}

		// Try to stack with existing
		if (proto.IsStackable) {
			var existing = k_items.FirstOrDefault(i =>
				i.Prototype.Id == proto.Id &&
				i.Count < proto.MaxStackSize);

			if (existing != null) {
				int remaining = existing.TryAddToStack(count);
				if (remaining < count) {
					k_metrics.Add(MetricType.ItemsCollected, count - remaining);
					OnInventoryChanged?.Invoke();
				}

				if (remaining > 0) {
					// Create new stack for remainder
					return AddItemInternal(proto, remaining);
				}

				return existing;
			}
		}

		return AddItemInternal(proto, count);
	}

	private ItemInstance? AddItemInternal(ItemProto proto, int count) {
		if (IsFull) {
			UnityEngine.Debug.LogWarning("Inventory is full");
			return null;
		}

		var instance = new ItemInstance(proto, count);
		k_items.Add(instance);

		k_metrics.Add(MetricType.ItemsCollected, count);
		k_metrics.IncrementBreakdown(MetricType.ItemsCollected, GetItemIdString(proto));

		OnItemAdded?.Invoke(instance);
		OnInventoryChanged?.Invoke();

		UnityEngine.Debug.Log($"Added {count}x {proto.DisplayText.Name} to inventory");
		return instance;
	}

	/// <summary>
	/// Adds an existing item instance.
	/// </summary>
	public bool AddItem(ItemInstance item) {
		if (IsFull) {
			return false;
		}

		k_items.Add(item);
		OnItemAdded?.Invoke(item);
		OnInventoryChanged?.Invoke();
		return true;
	}

	/// <summary>
	/// Removes an item instance from inventory.
	/// </summary>
	public bool RemoveItem(ItemInstance item) {
		if (!k_items.Contains(item)) {
			return false;
		}

		// Unequip if equipped
		if (item.IsEquipped) {
			UnequipItem(item);
		}

		k_items.Remove(item);
		OnItemRemoved?.Invoke(item);
		OnInventoryChanged?.Invoke();

		UnityEngine.Debug.Log($"Removed {item.DisplayName} from inventory");
		return true;
	}

	/// <summary>
	/// Removes a count of an item by ID.
	/// </summary>
	public int RemoveItem(Proto.ID itemId, int count) {
		int remaining = count;
		var toRemove = new List<ItemInstance>();

		foreach (var item in k_items.Where(i => i.Prototype.Id == itemId)) {
			int removed = item.RemoveFromStack(remaining);
			remaining -= removed;

			if (item.Count <= 0) {
				toRemove.Add(item);
			}

			if (remaining <= 0) {
				break;
			}
		}

		foreach (var item in toRemove) {
			if (item.IsEquipped) {
				UnequipItem(item);
			}
			k_items.Remove(item);
			OnItemRemoved?.Invoke(item);
		}

		if (toRemove.Count > 0 || remaining < count) {
			OnInventoryChanged?.Invoke();
		}

		return count - remaining;
	}

	/// <summary>
	/// Removes a count of an item by string ID.
	/// </summary>
	public int RemoveItem(string itemId, int count) {
		return RemoveItem(new Proto.ID(itemId), count);
	}

	/// <summary>
	/// Checks if inventory has a count of an item.
	/// </summary>
	public bool HasItem(Proto.ID itemId, int count = 1) {
		return GetItemCount(itemId) >= count;
	}

	/// <summary>
	/// Checks if inventory has a count of an item by string ID.
	/// </summary>
	public bool HasItem(string itemId, int count = 1) {
		return HasItem(new Proto.ID(itemId), count);
	}

	/// <summary>
	/// Gets total count of an item type.
	/// </summary>
	public int GetItemCount(Proto.ID itemId) {
		return k_items.Where(i => i.Prototype.Id == itemId).Sum(i => i.Count);
	}

	/// <summary>
	/// Gets total count of an item type by string ID.
	/// </summary>
	public int GetItemCount(string itemId) {
		return GetItemCount(new Proto.ID(itemId));
	}

	/// <summary>
	/// Gets first instance of an item type.
	/// </summary>
	public ItemInstance? GetItem(Proto.ID itemId) {
		return k_items.FirstOrDefault(i => i.Prototype.Id == itemId);
	}

	/// <summary>
	/// Gets first instance of an item type by string ID.
	/// </summary>
	public ItemInstance? GetItem(string itemId) {
		return GetItem(new Proto.ID(itemId));
	}

	/// <summary>
	/// Gets item by instance ID.
	/// </summary>
	public ItemInstance? GetItemByInstanceId(string instanceId) {
		return k_items.FirstOrDefault(i => i.InstanceId == instanceId);
	}

	#endregion

	#region Equipment

	/// <summary>
	/// Equips an item.
	/// </summary>
	public bool EquipItem(ItemInstance item) {
		if (!item.Prototype.EquipSlot.HasValue) {
			return false;
		}
		if (!k_items.Contains(item)) {
			return false;
		}
		if (item.IsBroken) {
			return false;
		}

		var slot = item.Prototype.EquipSlot.Value;

		// Unequip current item in slot
		if (k_equipment[slot] != null) {
			UnequipItem(k_equipment[slot]!);
		}

		// Handle two-handed weapons
		if (IsTwoHanded(item.Prototype) && slot == SlotType.MainHand) {
			if (k_equipment[SlotType.OffHand] != null) {
				UnequipItem(k_equipment[SlotType.OffHand]!);
			}
		}

		// Handle equipping off-hand when two-handed is equipped
		if (slot == SlotType.OffHand) {
			var mainHand = k_equipment[SlotType.MainHand];
			if (mainHand != null && IsTwoHanded(mainHand.Prototype)) {
				UnequipItem(mainHand);
			}
		}

		// Equip the item
		k_equipment[slot] = item;
		item.IsEquipped = true;

		k_metrics.Increment(MetricType.ItemsEquipped);
		OnItemEquipped?.Invoke(item, slot);
		OnInventoryChanged?.Invoke();

		UnityEngine.Debug.Log($"Equipped {item.DisplayName} in {slot}");
		return true;
	}

	/// <summary>
	/// Unequips an item.
	/// </summary>
	public bool UnequipItem(ItemInstance item) {
		if (!item.IsEquipped) {
			return false;
		}
		if (!item.Prototype.EquipSlot.HasValue) {
			return false;
		}

		var slot = item.Prototype.EquipSlot.Value;

		if (k_equipment[slot] != item) {
			return false;
		}

		k_equipment[slot] = null;
		item.IsEquipped = false;

		OnItemUnequipped?.Invoke(item, slot);
		OnInventoryChanged?.Invoke();

		UnityEngine.Debug.Log($"Unequipped {item.DisplayName} from {slot}");
		return true;
	}

	/// <summary>
	/// Unequips item in a specific slot.
	/// </summary>
	public bool UnequipSlot(SlotType slot) {
		var item = k_equipment[slot];
		if (item == null) {
			return false;
		}
		return UnequipItem(item);
	}

	/// <summary>
	/// Gets equipped item in a slot.
	/// </summary>
	public ItemInstance? GetEquippedItem(SlotType slot) {
		return k_equipment.GetValueOrDefault(slot);
	}

	/// <summary>
	/// Gets all equipped items.
	/// </summary>
	public IEnumerable<ItemInstance> GetAllEquippedItems() {
		return k_equipment.Values.Where(i => i != null)!;
	}

	/// <summary>
	/// Gets all stat modifiers from equipment.
	/// </summary>
	public IEnumerable<StatModifier> GetEquipmentModifiers() {
		foreach (var item in GetAllEquippedItems()) {
			foreach (var mod in item.GetStatModifiers()) {
				yield return mod;
			}
		}
	}

	/// <summary>
	/// Gets equipped weapon damage dice.
	/// </summary>
	public string GetWeaponDamage() {
		var weapon = GetEquippedItem(SlotType.MainHand);
		return weapon != null ? GetDamageDice(weapon.Prototype) : "1d4"; // Unarmed
	}

	/// <summary>
	/// Gets equipped weapon damage type.
	/// </summary>
	public DamageType GetWeaponDamageType() {
		var weapon = GetEquippedItem(SlotType.MainHand);
		return weapon != null ? GetDamageType(weapon.Prototype) : DamageType.Bludgeoning; // Unarmed
	}

	/// <summary>
	/// Gets total armor class bonus from equipment.
	/// </summary>
	public int GetArmorBonus() {
		int total = 0;
		foreach (var item in GetAllEquippedItems()) {
			total += GetArmorBonus(item.Prototype);
		}
		return total;
	}

	/// <summary>
	/// Checks if any equipment needs repair.
	/// </summary>
	public bool HasDamagedEquipment() {
		return GetAllEquippedItems().Any(i => i.IsDurabilityLow);
	}

	/// <summary>
	/// Gets all equipment that needs repair.
	/// </summary>
	public IEnumerable<ItemInstance> GetDamagedEquipment() {
		return GetAllEquippedItems().Where(i => i.IsDurabilityLow);
	}

	#endregion

	#region Item Usage

	/// <summary>
	/// Uses a consumable item.
	/// </summary>
	public ItemUseResult UseItem(ItemInstance item, ItemUseContext context = ItemUseContext.Anytime) {
		if (!IsUsable(item.Prototype)) {
			return new ItemUseResult { Success = false, Message = "This item cannot be used." };
		}

		if (item.RemainingUses.HasValue && item.RemainingUses.Value <= 0) {
			return new ItemUseResult { Success = false, Message = "This item has no uses remaining." };
		}

		// Use the item
		item.Use();

		var result = new ItemUseResult {
			Success = true,
			Message = $"Used {item.DisplayName}.",
			Effects = [.. item.Prototype.UseEffects],
			Item = item
		};

		// Remove if consumable with no remaining uses
		if (item.Prototype.Category == ItemCategory.Consumable) {
			if (!item.RemainingUses.HasValue || item.RemainingUses.Value <= 0) {
				item.RemoveFromStack(1);
				if (item.Count <= 0) {
					RemoveItem(item);
					result.WasConsumed = true;
				}
			}
		}

		k_metrics.Increment(MetricType.ItemsUsed);
		k_metrics.IncrementBreakdown(MetricType.ItemsUsed, GetItemIdString(item.Prototype));
		OnInventoryChanged?.Invoke();

		return result;
	}

	#endregion

	#region Shop/Trade

	/// <summary>
	/// Attempts to buy an item.
	/// </summary>
	public BuyResult BuyItem(Proto.ID itemId, int count = 1, float priceMultiplier = 1f) {
		if (!k_gameDb.TryGetProto<ItemProto>(itemId, out var proto)) {
			return new BuyResult { Success = false, Message = "Item not found." };
		}

		int totalCost = (int)(proto.BuyPrice * count * priceMultiplier);

		if (Gold < totalCost) {
			return new BuyResult {
				Success = false,
				Message = $"Not enough gold. Need {totalCost}, have {Gold}.",
				Cost = totalCost
			};
		}

		if (IsFull && !proto.IsStackable) {
			return new BuyResult { Success = false, Message = "Inventory is full." };
		}

		Gold -= totalCost;
		var item = AddItem(proto, count);

		k_metrics.RecordGoldTransaction(-totalCost, "Purchase");
		k_metrics.Increment(MetricType.ItemsPurchased);

		return new BuyResult {
			Success = true,
			Message = $"Bought {count}x {proto.DisplayText.Name} for {totalCost} gold.",
			Item = item,
			Cost = totalCost
		};
	}

	/// <summary>
	/// Attempts to buy an item by string ID.
	/// </summary>
	public BuyResult BuyItem(string itemId, int count = 1, float priceMultiplier = 1f) {
		return BuyItem(new Proto.ID(itemId), count, priceMultiplier);
	}

	/// <summary>
	/// Attempts to sell an item.
	/// </summary>
	public SellResult SellItem(ItemInstance item, int count = -1, float priceMultiplier = 1f) {
		if (!k_items.Contains(item)) {
			return new SellResult { Success = false, Message = "Item not in inventory." };
		}

		if (!CanSell(item.Prototype)) {
			return new SellResult { Success = false, Message = "This item cannot be sold." };
		}

		if (item.IsLocked) {
			return new SellResult { Success = false, Message = "This item is locked." };
		}

		if (item.IsEquipped) {
			return new SellResult { Success = false, Message = "Unequip item before selling." };
		}

		// Sell all if count is -1
		int toSell = count < 0 ? item.Count : Math.Min(count, item.Count);
		int totalValue = (int)(item.Prototype.SellPrice * toSell * priceMultiplier);

		item.RemoveFromStack(toSell);
		Gold += totalValue;

		if (item.Count <= 0) {
			RemoveItem(item);
		}

		k_metrics.RecordGoldTransaction(totalValue, "Sale");
		k_metrics.Increment(MetricType.ItemsSold);

		OnInventoryChanged?.Invoke();

		return new SellResult {
			Success = true,
			Message = $"Sold {toSell}x {item.Prototype.DisplayText.Name} for {totalValue} gold.",
			GoldReceived = totalValue,
			CountSold = toSell
		};
	}

	/// <summary>
	/// Gets estimated sell value for items.
	/// </summary>
	public int GetSellValue(ItemInstance item, int count = -1, float priceMultiplier = 1f) {
		int toSell = count < 0 ? item.Count : Math.Min(count, item.Count);
		return (int)(item.Prototype.SellPrice * toSell * priceMultiplier);
	}

	#endregion

	#region Queries

	/// <summary>
	/// Gets items by category.
	/// </summary>
	public IEnumerable<ItemInstance> GetItemsByCategory(ItemCategory category) {
		return k_items.Where(i => i.Prototype.Category == category);
	}

	/// <summary>
	/// Gets items by type.
	/// </summary>
	public IEnumerable<ItemInstance> GetItemsByType(ItemType type) {
		return k_items.Where(i => i.Prototype.Type == type);
	}

	/// <summary>
	/// Gets items that can be equipped in a slot.
	/// </summary>
	public IEnumerable<ItemInstance> GetEquippableItems(SlotType slot) {
		return k_items.Where(i =>
			i.Prototype.EquipSlot == slot &&
			!i.IsEquipped &&
			!i.IsBroken);
	}

	/// <summary>
	/// Gets usable items for a context.
	/// </summary>
	public IEnumerable<ItemInstance> GetUsableItems(ItemUseContext context = ItemUseContext.Anytime) {
		return k_items.Where(i =>
			IsUsable(i.Prototype) &&
			(!i.RemainingUses.HasValue || i.RemainingUses.Value > 0));
	}

	/// <summary>
	/// Gets items by rarity.
	/// </summary>
	public IEnumerable<ItemInstance> GetItemsByRarity(RarityType rarity) {
		return k_items.Where(i => i.Prototype.Rarity == rarity);
	}

	/// <summary>
	/// Gets all quest items.
	/// </summary>
	public IEnumerable<ItemInstance> GetQuestItems() {
		return k_items.Where(i => IsQuestItem(i.Prototype));
	}

	/// <summary>
	/// Sorts inventory.
	/// </summary>
	public void SortInventory(InventorySortMode mode = InventorySortMode.CategoryThenName) {
		IOrderedEnumerable<ItemInstance> sorted = mode switch {
			InventorySortMode.Name => k_items.OrderBy(i => i.Prototype.DisplayText.Name),
			InventorySortMode.Rarity => k_items
				.OrderByDescending(i => i.Prototype.Rarity)
				.ThenBy(i => i.Prototype.DisplayText.Name),
			InventorySortMode.Value => k_items
				.OrderByDescending(i => i.TotalSellValue)
				.ThenBy(i => i.Prototype.DisplayText.Name),
			InventorySortMode.Type => k_items
				.OrderBy(i => i.Prototype.Type)
				.ThenBy(i => i.Prototype.DisplayText.Name),
			_ => k_items
				.OrderBy(i => i.Prototype.Category)
				.ThenByDescending(i => i.Prototype.Rarity)
				.ThenBy(i => i.Prototype.DisplayText.Name)
		};

		var sortedList = sorted.ToList();
		k_items.Clear();
		k_items.AddRange(sortedList);

		OnInventoryChanged?.Invoke();
	}

	/// <summary>
	/// Gets inventory summary for UI.
	/// </summary>
	public InventorySummary GetSummary() {
		return new InventorySummary {
			UsedSlots = UsedSlots,
			MaxSlots = MaxSlots,
			TotalItems = k_items.Sum(i => i.Count),
			TotalWeight = TotalWeight,
			TotalValue = TotalValue,
			Gold = Gold,
			EquippedCount = GetAllEquippedItems().Count(),
			HasDamagedEquipment = HasDamagedEquipment()
		};
	}

	#endregion

	#region Durability

	/// <summary>
	/// Reduces durability on all equipped items.
	/// </summary>
	public void TickEquipmentDurability(int amount = 1) {
		foreach (var item in GetAllEquippedItems()) {
			item.ReduceDurability(amount);

			if (item.IsBroken) {
				UnityEngine.Debug.LogWarning($"{item.DisplayName} has broken!");
				// Could fire an event here
			}
		}
	}

	/// <summary>
	/// Repairs an item.
	/// </summary>
	public bool RepairItem(ItemInstance item, int repairAmount = -1) {
		if (!item.Durability.HasValue) {
			return false;
		}

		if (repairAmount < 0) {
			item.FullRepair();
		} else {
			item.Repair(repairAmount);
		}

		OnInventoryChanged?.Invoke();
		return true;
	}

	/// <summary>
	/// Gets repair cost for an item.
	/// </summary>
	public int GetRepairCost(ItemInstance item) {
		if (!item.Durability.HasValue || !item.MaxDurability.HasValue) {
			return 0;
		}

		int missing = item.MaxDurability.Value - item.Durability.Value;
		int baseCost = item.Prototype.BuyPrice / 10;

		return (int)(baseCost * ((float)missing / item.MaxDurability.Value));
	}

	#endregion

	#region Serialization

	/// <summary>
	/// Clears inventory and equipment.
	/// </summary>
	public void Clear() {
		// Unequip all
		foreach (var slot in k_equipment.Keys.ToList()) {
			k_equipment[slot] = null;
		}

		k_items.Clear();
		k_gold = 0;

		OnInventoryChanged?.Invoke();
	}

	/// <summary>
	/// Gets save data.
	/// </summary>
	public InventorySaveData ToData() {
		var equippedSlots = new Dictionary<SlotType, string>();
		foreach (var (slot, item) in k_equipment) {
			if (item != null) {
				equippedSlots[slot] = item.InstanceId;
			}
		}

		return new InventorySaveData {
			Items = k_items.Select(i => i.ToData()).ToList(),
			MaxSlots = k_maxSlots,
			Gold = k_gold,
			EquippedSlots = equippedSlots
		};
	}

	/// <summary>
	/// Loads from save data.
	/// </summary>
	public void FromData(InventorySaveData data) {
		Clear();

		k_maxSlots = data.MaxSlots;
		k_gold = data.Gold;

		// Load items
		foreach (var itemData in data.Items) {
			var protoId = new Proto.ID(itemData.ItemId);
			if (!k_gameDb.TryGetProto<ItemProto>(protoId, out var proto)) {
				UnityEngine.Debug.LogWarning($"Item proto not found during load: {itemData.ItemId}");
				continue;
			}

			var instance = new ItemInstance(proto, itemData);
			k_items.Add(instance);
		}

		// Restore equipment
		foreach (var (slot, instanceId) in data.EquippedSlots) {
			var item = GetItemByInstanceId(instanceId);
			if (item != null) {
				k_equipment[slot] = item;
				item.IsEquipped = true;
			}
		}

		OnInventoryChanged?.Invoke();
	}

	#endregion
}

#region Supporting Types

/// <summary>
/// Result of using an item.
/// </summary>
public class ItemUseResult {
	public bool Success { get; set; }
	public string Message { get; set; } = "";
	public List<ItemEffect> Effects { get; set; } = [];
	public ItemInstance? Item { get; set; }
	public bool WasConsumed { get; set; }
}

/// <summary>
/// Result of buying an item.
/// </summary>
public class BuyResult {
	public bool Success { get; set; }
	public string Message { get; set; } = "";
	public ItemInstance? Item { get; set; }
	public int Cost { get; set; }
}

/// <summary>
/// Result of selling an item.
/// </summary>
public class SellResult {
	public bool Success { get; set; }
	public string Message { get; set; } = "";
	public int GoldReceived { get; set; }
	public int CountSold { get; set; }
}

/// <summary>
/// Inventory sort modes.
/// </summary>
public enum InventorySortMode {
	CategoryThenName,
	Name,
	Rarity,
	Value,
	Type
}

/// <summary>
/// Inventory summary for UI.
/// </summary>
public class InventorySummary {
	public int UsedSlots { get; set; }
	public int MaxSlots { get; set; }
	public int TotalItems { get; set; }
	public float TotalWeight { get; set; }
	public int TotalValue { get; set; }
	public int Gold { get; set; }
	public int EquippedCount { get; set; }
	public bool HasDamagedEquipment { get; set; }
}

/// <summary>
/// Inventory save data.
/// </summary>
public class InventorySaveData {
	public List<ItemInstanceData> Items { get; set; } = [];
	public int MaxSlots { get; set; }
	public int Gold { get; set; }
	public Dictionary<SlotType, string> EquippedSlots { get; set; } = [];
}

#endregion
namespace RPGGame.Core.Items;

/// <summary>
/// Simple inventory data container for RunState.
/// This is a data class for serialization; actual inventory management
/// is handled by InventoryManager.
/// </summary>
/// <remarks>
/// In our architecture:
/// - Inventory is a data container stored in RunState
/// - InventoryManager is a singleton service that operates on inventory data
/// - This separation allows RunState to be serialized easily
/// </remarks>
public class Inventory {
	/// <summary>
	/// Item instance IDs in inventory.
	/// The actual item data is managed by InventoryManager.
	/// </summary>
	public List<string> ItemInstanceIds { get; set; } = [];

	/// <summary>
	/// Equipped item instance IDs by slot.
	/// </summary>
	public Dictionary<SlotType, string?> EquippedItems { get; set; } = [];

	/// <summary>
	/// Maximum inventory slots.
	/// </summary>
	public int MaxSlots { get; set; } = 20;

	/// <summary>
	/// Creates a new empty inventory.
	/// </summary>
	public Inventory() {
		// Initialize equipment slots
		foreach (SlotType slot in Enum.GetValues(typeof(SlotType))) {
			EquippedItems[slot] = null;
		}
	}

	/// <summary>
	/// Creates from save data.
	/// </summary>
	public static Inventory FromData(InventoryData data) {
		var inventory = new Inventory {
			ItemInstanceIds = [.. data.ItemInstanceIds],
			MaxSlots = data.MaxSlots
		};

		foreach (var (slot, instanceId) in data.EquippedItems) {
			inventory.EquippedItems[slot] = instanceId;
		}

		return inventory;
	}

	/// <summary>
	/// Converts to save data.
	/// </summary>
	public InventoryData ToData() {
		return new InventoryData {
			ItemInstanceIds = [.. ItemInstanceIds],
			EquippedItems = new Dictionary<SlotType, string?>(EquippedItems),
			MaxSlots = MaxSlots
		};
	}
}

#region Serialization Data

/// <summary>
/// Serializable inventory data.
/// </summary>
public class InventoryData {
	public List<string> ItemInstanceIds { get; set; } = [];
	public Dictionary<SlotType, string?> EquippedItems { get; set; } = [];
	public int MaxSlots { get; set; } = 20;
}

#endregion
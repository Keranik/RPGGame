using RPGGame.Core;
using RPGGame.Core.Items;
using RPGGame.Core.Prototypes.Characters;
using RPGGame.Core.Prototypes.Item.Equipment;
using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Components;

/// <summary>
/// A paper doll equipment display panel with fixed slot positions.
/// Slots are positioned around a character silhouette in anatomically correct locations.
/// 
/// Layout (approximate positions on a 320x480 panel):
/// <code>
///          [Head]
///    [Back]      [Amulet]
/// [MainHand] [Chest] [OffHand]
///    [Hands]  [Waist]
/// [Ring1]    [Legs]    [Ring2]
///  [Trinket] [Feet] [Quiver]
/// </code>
/// </summary>
public class GameEquipmentPanel : VisualElement {
	#region Constants - Slot Positions (normalized 0-1)

	// These define where each slot appears on the paper doll
	// Positions are relative to the portrait area (not including header)
	private static readonly Dictionary<string, Vector2> SlotPositions = new() {
		// Head area (top center)
		["Slot_Head"] = new Vector2(0.50f, 0.08f),

		// Shoulder/neck area
		["Slot_Back"] = new Vector2(0.18f, 0.18f),
		["Slot_Amulet"] = new Vector2(0.82f, 0.18f),

		// Torso area
		["Slot_MainHand"] = new Vector2(0.12f, 0.38f),
		["Slot_Chest"] = new Vector2(0.50f, 0.32f),
		["Slot_OffHand"] = new Vector2(0.88f, 0.38f),

		// Waist/hands area
		["Slot_Hands"] = new Vector2(0.22f, 0.48f),
		["Slot_Waist"] = new Vector2(0.50f, 0.48f),

		// Lower body
		["Slot_Ring1"] = new Vector2(0.18f, 0.62f),
		["Slot_Legs"] = new Vector2(0.50f, 0.62f),
		["Slot_Ring2"] = new Vector2(0.82f, 0.62f),

		// Feet area
		["Slot_Trinket"] = new Vector2(0.22f, 0.78f),
		["Slot_Feet"] = new Vector2(0.50f, 0.82f),
		["Slot_Quiver"] = new Vector2(0.78f, 0.78f),

		// Class-specific (positioned in accessory areas)
		["Slot_DruidTotem"] = new Vector2(0.15f, 0.92f),
		["Slot_WarlockPact"] = new Vector2(0.50f, 0.92f),
		["Slot_MonkFocus"] = new Vector2(0.85f, 0.92f),
	};

	#endregion

	#region Private Fields

	private readonly GameContainer k_container;
	private readonly GameContainer k_headerSection;
	private readonly GameLabel k_characterNameLabel;
	private readonly GameLabel k_characterInfoLabel;
	private readonly GameContainer k_portraitArea;
	private readonly GameContainer k_silhouette;
	private readonly GameContainer k_slotsContainer;

	private CharacterClassProto.ID? k_characterClass;
	private int k_characterLevel = 1;
	private string k_characterName = "Character";
	private string k_characterInfo = "";
	private bool k_isSubscribed;

	private readonly Dictionary<EquipmentSlotProto.ID, GameItemSlotDisplay> k_slots = new();

	// Configurable sizes
	private float k_panelWidth = 320f;
	private float k_panelHeight = 480f;
	private float k_slotSize = 52f;
	private float k_headerHeight = 56f;

	// Callbacks
	private Action<EquipmentSlotProto.ID, ItemInstance?>? k_onSlotClicked;
	private Action<EquipmentSlotProto.ID, ItemInstance?>? k_onSlotRightClicked;
	private Action<EquipmentSlotProto.ID, ItemInstance>? k_onItemEquipped;
	private Action<EquipmentSlotProto.ID, ItemInstance>? k_onItemUnequipped;

	#endregion

	#region Service Accessors

	private static GameDb GameDb => GameServices.Db;
	private static InventoryManager Inventory => GameServices.Inventory;

	#endregion

	#region Constructor

	public GameEquipmentPanel() {
		// Main container
		k_container = new GameContainer("equipment-panel")
			.SetColumn()
			.SetAlignItems(Align.Center);

		// Header section with character info
		k_headerSection = new GameContainer("equipment-header")
			.SetColumn()
			.SetAlignItems(Align.Center)
			.SetJustifyContent(Justify.Center)
			.SetFullWidth();

		k_characterNameLabel = new GameLabel("Character")
			.SetStyle(LabelStyle.TitleMedium)
			.SetColor(LabelColor.Primary)
			.SetTextAlign(TextAnchor.MiddleCenter)
			.Build();

		k_characterInfoLabel = new GameLabel("Level 1")
			.SetStyle(LabelStyle.Caption)
			.SetColor(LabelColor.Secondary)
			.SetTextAlign(TextAnchor.MiddleCenter)
			.Build();

		k_headerSection
			.AddChild(k_characterNameLabel)
			.AddChild(k_characterInfoLabel);

		// Portrait area - contains silhouette and slots
		k_portraitArea = new GameContainer("portrait-area")
			.SetRelative()
			.SetFlexGrow(1);

		// Character silhouette/placeholder (background image would go here)
		k_silhouette = new GameContainer("character-silhouette")
			.SetAbsolute()
			.SetLeft(Length.Percent(20))
			.SetRight(Length.Percent(20))
			.SetTop(Length.Percent(5))
			.SetBottom(Length.Percent(5));

		// Slots container overlays everything
		k_slotsContainer = new GameContainer("slots-container")
			.SetAbsoluteFill();

		k_portraitArea
			.AddChild(k_silhouette)
			.AddChild(k_slotsContainer);

		k_container
			.AddChild(k_headerSection)
			.AddChild(k_portraitArea);

		Add(k_container);

		GameTheme.OnThemeChanged += OnThemeChanged;
	}

	#endregion

	#region Fluent API - Data

	/// <summary>
	/// Sets the character class (determines which slots are available).
	/// </summary>
	public GameEquipmentPanel SetCharacterClass(CharacterClassProto.ID classId) {
		k_characterClass = classId;
		return this;
	}

	/// <summary>
	/// Sets the character level (determines which slots are unlocked).
	/// </summary>
	public GameEquipmentPanel SetCharacterLevel(int level) {
		k_characterLevel = level;
		return this;
	}

	/// <summary>
	/// Sets the character name for display.
	/// </summary>
	public GameEquipmentPanel SetCharacterName(string name) {
		k_characterName = name;
		k_characterNameLabel.SetText(name);
		return this;
	}

	/// <summary>
	/// Sets the character info line (e.g., "Level 5 Warrior").
	/// </summary>
	public GameEquipmentPanel SetCharacterInfo(string info) {
		k_characterInfo = info;
		k_characterInfoLabel.SetText(info);
		return this;
	}

	#endregion

	#region Fluent API - Appearance

	/// <summary>
	/// Sets the panel size.
	/// </summary>
	public GameEquipmentPanel SetSize(float width, float height) {
		k_panelWidth = width;
		k_panelHeight = height;
		return this;
	}

	/// <summary>
	/// Sets the slot size.
	/// </summary>
	public GameEquipmentPanel SetSlotSize(float size) {
		k_slotSize = size;
		return this;
	}

	/// <summary>
	/// Sets the header height.
	/// </summary>
	public GameEquipmentPanel SetHeaderHeight(float height) {
		k_headerHeight = height;
		return this;
	}

	#endregion

	#region Fluent API - Events

	/// <summary>
	/// Called when an equipment slot is clicked.
	/// </summary>
	public GameEquipmentPanel OnSlotClicked(Action<EquipmentSlotProto.ID, ItemInstance?> callback) {
		k_onSlotClicked = callback;
		return this;
	}

	/// <summary>
	/// Called when an equipment slot is right-clicked.
	/// </summary>
	public GameEquipmentPanel OnSlotRightClicked(Action<EquipmentSlotProto.ID, ItemInstance?> callback) {
		k_onSlotRightClicked = callback;
		return this;
	}

	/// <summary>
	/// Called when an item is equipped.
	/// </summary>
	public GameEquipmentPanel OnItemEquipped(Action<EquipmentSlotProto.ID, ItemInstance> callback) {
		k_onItemEquipped = callback;
		return this;
	}

	/// <summary>
	/// Called when an item is unequipped.
	/// </summary>
	public GameEquipmentPanel OnItemUnequipped(Action<EquipmentSlotProto.ID, ItemInstance> callback) {
		k_onItemUnequipped = callback;
		return this;
	}

	#endregion

	#region Build

	/// <summary>
	/// Builds the equipment panel and subscribes to inventory events.
	/// </summary>
	public GameEquipmentPanel Build() {
		ApplyTheme();
		ApplyLayout();
		BuildSlots();
		SubscribeToInventory();
		RefreshAllSlots();
		return this;
	}

	#endregion

	#region Layout

	private void ApplyLayout() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		// Apply sizes
		k_container
			.SetWidth(k_panelWidth)
			.SetHeight(k_panelHeight);

		// Header sizing
		k_headerSection
			.SetHeight(k_headerHeight)
			.SetPadding(spacing.SM, spacing.SM);

		// Portrait area fills remaining space
		float portraitHeight = k_panelHeight - k_headerHeight;
		k_portraitArea
			.SetWidth(k_panelWidth)
			.SetHeight(portraitHeight);
	}

	#endregion

	#region Event Subscription

	private void SubscribeToInventory() {
		if (k_isSubscribed) return;

		Inventory.OnItemEquipped += OnItemEquippedInternal;
		Inventory.OnItemUnequipped += OnItemUnequippedInternal;
		Inventory.OnInventoryChanged += RefreshAllSlots;
		k_isSubscribed = true;
	}

	private void UnsubscribeFromInventory() {
		if (!k_isSubscribed) return;

		Inventory.OnItemEquipped -= OnItemEquippedInternal;
		Inventory.OnItemUnequipped -= OnItemUnequippedInternal;
		Inventory.OnInventoryChanged -= RefreshAllSlots;
		k_isSubscribed = false;
	}

	#endregion

	#region Slot Building

	private void BuildSlots() {
		k_slotsContainer.Clear();
		k_slots.Clear();

		// Get all equipment slot protos
		var slotProtos = GameDb.GetAll<EquipmentSlotProto>()
			.Where(IsSlotAvailable)
			.OrderBy(s => s.DisplayOrder)
			.ToList();

		foreach (var slotProto in slotProtos) {
			CreateSlot(slotProto);
		}
	}

	private bool IsSlotAvailable(EquipmentSlotProto slotProto) {
		// Don't show BothHands slot - it's virtual
		if (slotProto.Id == Ids.EquipmentSlots.BothHands) {
			return false;
		}

		// Check if core slot or meets requirements
		if (!slotProto.IsCoreSlot) {
			// Check level requirement
			if (!slotProto.IsUnlockedAtLevel(k_characterLevel)) {
				return false;
			}

			// Check class restriction
			if (k_characterClass.HasValue && !slotProto.IsAvailableToClass(k_characterClass.Value)) {
				return false;
			}
		}

		return true;
	}

	private void CreateSlot(EquipmentSlotProto slotProto) {
		// Calculate slot size (can be larger for two-handed weapons, etc.)
		float slotWidth = k_slotSize * slotProto.SlotSize.x;
		float slotHeight = k_slotSize * slotProto.SlotSize.y;

		var slot = new GameItemSlotDisplay()
			.SetSlotId(slotProto.Id)
			.SetSize(slotWidth)
			.SetEmptyIcon(slotProto.EmptySlotIcon ?? GetDefaultEmptyIcon(slotProto))
			.SetDraggable(true)
			.SetDroppable(true)
			.SetDropFilter(item => CanEquipInSlot(item, slotProto))
			.OnClick(() => HandleSlotClicked(slotProto.Id))
			.OnRightClick(() => HandleSlotRightClicked(slotProto.Id))
			.OnItemDropped((item, source) => HandleItemDropped(slotProto, item, source as GameItemSlotDisplay))
			.OnDragStarted(() => HandleDragStarted(slotProto))
			.Build();

		var typedSlot = (GameItemSlotDisplay)slot;

		// Position slot using our fixed positions or proto positions
		Vector2 position = GetSlotPosition(slotProto);

		typedSlot.style.position = Position.Absolute;

		// Calculate pixel position from normalized coordinates
		// Center the slot on its position
		float portraitWidth = k_panelWidth;
		float portraitHeight = k_panelHeight - k_headerHeight;

		float leftPx = (position.x * portraitWidth) - (slotWidth / 2f);
		float topPx = (position.y * portraitHeight) - (slotHeight / 2f);

		// Clamp to keep slots within bounds
		leftPx = Mathf.Clamp(leftPx, 2, portraitWidth - slotWidth - 2);
		topPx = Mathf.Clamp(topPx, 2, portraitHeight - slotHeight - 2);

		typedSlot.style.left = leftPx;
		typedSlot.style.top = topPx;

		k_slotsContainer.Add(typedSlot);
		k_slots[slotProto.Id] = typedSlot;
	}

	private Vector2 GetSlotPosition(EquipmentSlotProto slotProto) {
		// First check our fixed positions
		if (SlotPositions.TryGetValue(slotProto.Id.Value, out var fixedPos)) {
			return fixedPos;
		}

		// Fall back to proto-defined position
		if (slotProto.PaperDollPosition != Vector2.zero) {
			return slotProto.PaperDollPosition;
		}

		// Default to center if nothing defined
		return new Vector2(0.5f, 0.5f);
	}

	private string GetDefaultEmptyIcon(EquipmentSlotProto slotProto) {
		// Return category-specific default icons
		return slotProto.Category switch {
			EquipmentSlotCategory.Weapon => "⚔️",
			EquipmentSlotCategory.Armor => "🛡️",
			EquipmentSlotCategory.Accessory => "💍",
			EquipmentSlotCategory.Special => "✨",
			_ => "○"
		};
	}

	private bool CanEquipInSlot(ItemInstance? item, EquipmentSlotProto slotProto) {
		if (item == null) return false;

		// Check if item has an equip slot
		if (!item.Prototype.EquipSlot.HasValue) return false;

		// Get the item's target slot
		var itemSlotType = item.Prototype.EquipSlot.Value;

		// Map to new slot IDs
		var itemSlotId = MapLegacySlotToId(itemSlotType);
		if (itemSlotId == null) return false;

		// Direct match
		if (itemSlotId == slotProto.Id) return true;

		// Ring can go in Ring1 or Ring2
		if (itemSlotId == Ids.EquipmentSlots.Ring1 || itemSlotId == Ids.EquipmentSlots.Ring2) {
			if (slotProto.Id == Ids.EquipmentSlots.Ring1 || slotProto.Id == Ids.EquipmentSlots.Ring2) {
				return true;
			}
		}

		// Two-handed weapons block both MainHand and OffHand
		if (itemSlotId == Ids.EquipmentSlots.BothHands) {
			if (slotProto.Id == Ids.EquipmentSlots.MainHand) {
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Maps legacy SlotType enum to new EquipmentSlotProto.ID.
	/// TODO: Remove once SlotType is fully deprecated from ItemProto.
	/// </summary>
	private static EquipmentSlotProto.ID? MapLegacySlotToId(SlotType legacySlot) {
		return legacySlot switch {
			SlotType.Head => Ids.EquipmentSlots.Head,
			SlotType.Chest => Ids.EquipmentSlots.Chest,
			SlotType.Legs => Ids.EquipmentSlots.Legs,
			SlotType.Feet => Ids.EquipmentSlots.Feet,
			SlotType.Hands => Ids.EquipmentSlots.Hands,
			SlotType.MainHand => Ids.EquipmentSlots.MainHand,
			SlotType.OffHand => Ids.EquipmentSlots.OffHand,
			SlotType.BothHands => Ids.EquipmentSlots.BothHands,
			SlotType.Ring1 => Ids.EquipmentSlots.Ring1,
			SlotType.Ring2 => Ids.EquipmentSlots.Ring2,
			SlotType.Amulet => Ids.EquipmentSlots.Amulet,
			SlotType.Trinket => Ids.EquipmentSlots.Trinket,
			SlotType.Back => Ids.EquipmentSlots.Back,
			SlotType.Waist => Ids.EquipmentSlots.Waist,
			SlotType.Quiver => Ids.EquipmentSlots.Quiver,
			_ => null
		};
	}

	#endregion

	#region Event Handlers

	private void HandleSlotClicked(EquipmentSlotProto.ID slotId) {
		var item = GetEquippedItem(slotId);
		k_onSlotClicked?.Invoke(slotId, item);
	}

	private void HandleSlotRightClicked(EquipmentSlotProto.ID slotId) {
		var item = GetEquippedItem(slotId);
		k_onSlotRightClicked?.Invoke(slotId, item);
	}

	private void HandleItemDropped(EquipmentSlotProto slotProto, ItemInstance? item, GameItemSlotDisplay? sourceSlot) {
		if (item == null) return;

		// If dropped from another equipment slot, handle swap
		if (sourceSlot?.SlotId is EquipmentSlotProto.ID sourceSlotId) {
			var currentItem = GetEquippedItem(slotProto.Id);

			// Unequip from source
			Inventory.UnequipItem(item);

			// If there's an item in target slot, unequip it
			if (currentItem != null) {
				Inventory.UnequipItem(currentItem);
			}

			// Equip dropped item
			Inventory.EquipItem(item);
		} else {
			// Dropped from inventory
			Inventory.EquipItem(item);
		}
	}

	private void HandleDragStarted(EquipmentSlotProto slotProto) {
		// Could highlight valid drop targets
	}

	private void OnItemEquippedInternal(ItemInstance item, SlotType legacySlot) {
		var slotId = MapLegacySlotToId(legacySlot);
		if (slotId.HasValue) {
			RefreshSlotById(slotId.Value);
			k_onItemEquipped?.Invoke(slotId.Value, item);
		}
	}

	private void OnItemUnequippedInternal(ItemInstance item, SlotType legacySlot) {
		var slotId = MapLegacySlotToId(legacySlot);
		if (slotId.HasValue) {
			RefreshSlotById(slotId.Value);
			k_onItemUnequipped?.Invoke(slotId.Value, item);
		}
	}

	private void RefreshSlotById(EquipmentSlotProto.ID slotId) {
		if (k_slots.TryGetValue(slotId, out var slot)) {
			RefreshSlot(slotId, slot);
		}
	}

	#endregion

	#region Refresh

	private void RefreshAllSlots() {
		foreach (var (slotId, slot) in k_slots) {
			RefreshSlot(slotId, slot);
		}
	}

	private void RefreshSlot(EquipmentSlotProto.ID slotId, GameItemSlotDisplay slot) {
		var item = GetEquippedItem(slotId);
		slot.SetItem(item);
	}

	private ItemInstance? GetEquippedItem(EquipmentSlotProto.ID slotId) {
		// Find items equipped in this slot
		// For now, use legacy lookup via SlotType until InventoryManager is updated
		var slotProto = GameDb.Get<EquipmentSlotProto>(slotId);
		if (slotProto?.LegacySlotType == null) return null;

		return Inventory.GetEquippedItem(slotProto.LegacySlotType.Value);
	}

	#endregion

	#region Public Methods

	/// <summary>
	/// Unequips the item in the specified slot.
	/// </summary>
	public void UnequipItem(EquipmentSlotProto.ID slotId) {
		var item = GetEquippedItem(slotId);
		if (item != null) {
			Inventory.UnequipItem(item);
		}
	}

	/// <summary>
	/// Gets the slot component for a specific slot ID.
	/// </summary>
	public GameItemSlotDisplay? GetSlot(EquipmentSlotProto.ID slotId) {
		return k_slots.GetValueOrDefault(slotId);
	}

	/// <summary>
	/// Rebuilds slots (call after class or level change).
	/// </summary>
	public void RebuildSlots() {
		BuildSlots();
		RefreshAllSlots();
	}

	#endregion

	#region Theme

	private void ApplyTheme() {
		var theme = GameTheme.Current;
		var colors = theme.Colors;
		var borders = theme.Borders;
		var spacing = theme.Spacing;

		// Container styling
		k_container
			.SetBackgroundColor(colors.BackgroundSecondary)
			.SetBorderRadius(borders.RadiusMD)
			.SetPadding(spacing.PanelPadding);

		// Header styling
		k_headerSection
			.SetBackgroundColor(colors.Surface)
			.SetBorderBottomWidth(borders.WidthThin)
			.SetBorderBottomColor(colors.SurfaceBorder)
			.SetBorderRadius(borders.RadiusMD, borders.RadiusMD, 0, 0);

		// Portrait area styling
		k_portraitArea
			.SetBackgroundColor(colors.BackgroundTertiary)
			.SetBorderRadius(0, 0, borders.RadiusMD, borders.RadiusMD);

		// Silhouette styling (placeholder for character image)
		k_silhouette
			.SetBackgroundColor(colors.Surface)
			.SetOpacity(0.3f)
			.SetBorderRadius(borders.RadiusLG);

		// Update labels with current values
		k_characterNameLabel.SetText(k_characterName);
		k_characterInfoLabel.SetText(k_characterInfo);
	}

	private void OnThemeChanged(GameTheme theme) {
		ApplyTheme();
		ApplyLayout();
	}

	#endregion

	#region Cleanup

	public new void RemoveFromHierarchy() {
		UnsubscribeFromInventory();
		GameTheme.OnThemeChanged -= OnThemeChanged;
		base.RemoveFromHierarchy();
	}

	#endregion
}
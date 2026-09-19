using RPGGame.Core;
using RPGGame.Core.Items;
using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Components;

/// <summary>
/// An inventory grid panel with sorting, filtering, and item management.
/// Automatically subscribes to InventoryManager events via GameServices.
/// 
/// <para>Usage:</para>
/// <code>
/// var inventory = new GameInventoryPanel()
///     .SetColumns(6)
///     .SetMaxSlots(24)
///     .SetShowSortControls(true)
///     .OnItemClicked(item => ShowItemDetails(item))
///     .OnItemRightClicked(item => ShowContextMenu(item))
///     .Build();
/// </code>
/// </summary>
public class GameInventoryPanel : VisualElement {
    #region Private Fields

    private readonly VisualElement k_container;
    private readonly VisualElement k_headerRow;
    private readonly Label k_titleLabel;
    private readonly Label k_countLabel;
    private readonly VisualElement k_sortControls;
    private readonly VisualElement k_gridContainer;
    private readonly VisualElement k_footerRow;
    private readonly Label k_goldLabel;
    private readonly Label k_weightLabel;

    private int k_columns = 6;
    private int k_maxSlots = 24;
    private float k_slotSize = 56f;
    private float k_slotGap = 4f;
    private bool k_showSortControls = true;
    private bool k_showFooter = true;
    private InventorySortMode k_sortMode = InventorySortMode.CategoryThenName;
    private ItemCategory? k_filterCategory;
    private bool k_isSubscribed;

    private readonly List<GameItemSlotDisplay> k_slots = [];

    // Callbacks
    private Action<ItemInstance?>? k_onItemClicked;
    private Action<ItemInstance?>? k_onItemRightClicked;
    private Action<ItemInstance?>? k_onItemDoubleClicked;
    private Action<ItemInstance?, int>? k_onItemDropped;

    #endregion

    #region Service Accessors

    private static InventoryManager Inventory => GameServices.Inventory;

    #endregion

    #region Constructor

    public GameInventoryPanel() {
        k_container = new VisualElement {
            name = "inventory-panel",
            style = {
                flexDirection = FlexDirection.Column
            }
        };

        // Header row
        k_headerRow = new VisualElement {
            style = {
                flexDirection = FlexDirection.Row,
                justifyContent = Justify.SpaceBetween,
                alignItems = Align.Center,
                marginBottom = 8
            }
        };

        k_titleLabel = new Label("Inventory") {
            style = {
                fontSize = 14,
                unityFontStyleAndWeight = FontStyle.Bold
            }
        };

        k_countLabel = new Label("0/24") {
            style = {
                fontSize = 11
            }
        };

        var headerLeft = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center } };
        headerLeft.Add(k_titleLabel);
        headerLeft.Add(k_countLabel);
        k_countLabel.style.marginLeft = 8;

        k_sortControls = new VisualElement {
            style = {
                flexDirection = FlexDirection.Row
            }
        };

        k_headerRow.Add(headerLeft);
        k_headerRow.Add(k_sortControls);

        // Grid container
        k_gridContainer = new VisualElement {
            name = "inventory-grid",
            style = {
                flexDirection = FlexDirection.Row,
                flexWrap = Wrap.Wrap
            }
        };

        // Footer row
        k_footerRow = new VisualElement {
            style = {
                flexDirection = FlexDirection.Row,
                justifyContent = Justify.SpaceBetween,
                alignItems = Align.Center,
                marginTop = 8
            }
        };

        k_goldLabel = new Label("💰 0") {
            style = { fontSize = 12 }
        };

        k_weightLabel = new Label("⚖️ 0.0") {
            style = { fontSize = 12 }
        };

        k_footerRow.Add(k_goldLabel);
        k_footerRow.Add(k_weightLabel);

        k_container.Add(k_headerRow);
        k_container.Add(k_gridContainer);
        k_container.Add(k_footerRow);
        Add(k_container);

        GameTheme.OnThemeChanged += OnThemeChanged;
    }

    #endregion

    #region Fluent API - Appearance

    /// <summary>
    /// Sets the number of columns.
    /// </summary>
    public GameInventoryPanel SetColumns(int columns) {
        k_columns = Math.Max(1, columns);
        return this;
    }

    /// <summary>
    /// Sets the maximum number of slots.
    /// </summary>
    public GameInventoryPanel SetMaxSlots(int maxSlots) {
        k_maxSlots = Math.Max(1, maxSlots);
        return this;
    }

    /// <summary>
    /// Sets the slot size.
    /// </summary>
    public GameInventoryPanel SetSlotSize(float size) {
        k_slotSize = size;
        return this;
    }

    /// <summary>
    /// Sets the gap between slots.
    /// </summary>
    public GameInventoryPanel SetSlotGap(float gap) {
        k_slotGap = gap;
        return this;
    }

    /// <summary>
    /// Sets whether to show sort controls.
    /// </summary>
    public GameInventoryPanel SetShowSortControls(bool show) {
        k_showSortControls = show;
        k_sortControls.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
        return this;
    }

    /// <summary>
    /// Sets whether to show footer (gold, weight).
    /// </summary>
    public GameInventoryPanel SetShowFooter(bool show) {
        k_showFooter = show;
        k_footerRow.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
        return this;
    }

    /// <summary>
    /// Sets the title.
    /// </summary>
    public GameInventoryPanel SetTitle(string title) {
        k_titleLabel.text = title;
        return this;
    }

    #endregion

    #region Fluent API - Behavior

    /// <summary>
    /// Sets the sort mode.
    /// </summary>
    public GameInventoryPanel SetSortMode(InventorySortMode mode) {
        k_sortMode = mode;
        RefreshInventory();
        return this;
    }

    /// <summary>
    /// Sets the category filter.
    /// </summary>
    public GameInventoryPanel SetFilter(ItemCategory? category) {
        k_filterCategory = category;
        RefreshInventory();
        return this;
    }

    #endregion

    #region Fluent API - Events

    /// <summary>
    /// Called when an item is clicked.
    /// </summary>
    public GameInventoryPanel OnItemClicked(Action<ItemInstance?> callback) {
        k_onItemClicked = callback;
        return this;
    }

    /// <summary>
    /// Called when an item is right-clicked.
    /// </summary>
    public GameInventoryPanel OnItemRightClicked(Action<ItemInstance?> callback) {
        k_onItemRightClicked = callback;
        return this;
    }

    /// <summary>
    /// Called when an item is double-clicked.
    /// </summary>
    public GameInventoryPanel OnItemDoubleClicked(Action<ItemInstance?> callback) {
        k_onItemDoubleClicked = callback;
        return this;
    }

    /// <summary>
    /// Called when an item is dropped into a slot.
    /// </summary>
    public GameInventoryPanel OnItemDropped(Action<ItemInstance?, int> callback) {
        k_onItemDropped = callback;
        return this;
    }

    #endregion

    #region Build

    /// <summary>
    /// Builds the inventory panel and subscribes to inventory events.
    /// </summary>
    public GameInventoryPanel Build() {
        ApplyTheme();
        BuildSortControls();
        BuildSlots();
        SubscribeToInventory();
        RefreshInventory();
        return this;
    }

    #endregion

    #region Event Subscription

    private void SubscribeToInventory() {
        if (k_isSubscribed) return;

        Inventory.OnInventoryChanged += RefreshInventory;
        Inventory.OnGoldChanged += RefreshGold;
        k_isSubscribed = true;
    }

    private void UnsubscribeFromInventory() {
        if (!k_isSubscribed) return;

        Inventory.OnInventoryChanged -= RefreshInventory;
        Inventory.OnGoldChanged -= RefreshGold;
        k_isSubscribed = false;
    }

    #endregion

    #region Slot Building

    private void BuildSortControls() {
        k_sortControls.Clear();

        if (!k_showSortControls) return;

        var sortButton = new GameButton("⇅")
            .SetVariant(ButtonVariant.Ghost)
            .SetSize(ButtonSize.Small)
            .OnClick(CycleSortMode)
            .Build();
        sortButton.tooltip = $"Sort: {k_sortMode}";

        k_sortControls.Add(sortButton);
    }

    private void BuildSlots() {
        k_gridContainer.Clear();
        k_slots.Clear();

        float gridWidth = k_columns * (k_slotSize + k_slotGap) - k_slotGap;
        k_gridContainer.style.width = gridWidth;

        for (int i = 0; i < k_maxSlots; i++) {
            int slotIndex = i;

            var slot = new GameItemSlotDisplay()
                .SetSlotId(slotIndex)
                .SetSize(k_slotSize)
                .SetDraggable(true)
                .SetDroppable(true)
                .OnClick(() => HandleItemClicked(slotIndex))
                .OnRightClick(() => HandleItemRightClicked(slotIndex))
                .OnDoubleClick(() => HandleItemDoubleClicked(slotIndex))
                .OnItemDropped((item, source) => HandleItemDropped(item, source as GameItemSlotDisplay, slotIndex))
                .Build();

            var typedSlot = (GameItemSlotDisplay)slot;

            typedSlot.style.marginRight = k_slotGap;
            typedSlot.style.marginBottom = k_slotGap;

            k_gridContainer.Add(typedSlot);
            k_slots.Add(typedSlot);
        }
    }

    #endregion

    #region Event Handlers

    private void HandleItemClicked(int slotIndex) {
        var item = slotIndex < k_slots.Count ? k_slots[slotIndex].Item : null;
        k_onItemClicked?.Invoke(item);
    }

	private void HandleItemRightClicked(int slotIndex) {
		var item = slotIndex < k_slots.Count ? k_slots[slotIndex].Item : null;
		k_onItemRightClicked?.Invoke(item);

		// The context menu is now handled by GameItemSlotDisplay.OnRightClickAction
		// which is called by BaseGameSlotDisplay when right-clicked
	}

    private void HandleItemDoubleClicked(int slotIndex) {
        var item = slotIndex < k_slots.Count ? k_slots[slotIndex].Item : null;
        k_onItemDoubleClicked?.Invoke(item);

        // Default double-click: equip if equippable
        if (item != null && item.Prototype.IsEquippable) {
            Inventory.EquipItem(item);
        }
    }

    private void HandleItemDropped(ItemInstance? item, GameItemSlotDisplay? source, int targetSlotIndex) {
        k_onItemDropped?.Invoke(item, targetSlotIndex);
    }

	private void ShowItemContextMenu(ItemInstance item, int slotIndex, Vector2 mousePosition) {
		if (slotIndex < 0 || slotIndex >= k_slots.Count) return;

		var slot = k_slots[slotIndex];
    
		// Use the slot's ShowContextMenu with cursor position
		slot.ShowContextMenu(mousePosition);
	}

    private void CycleSortMode() {
        var modes = Enum.GetValues(typeof(InventorySortMode)).Cast<InventorySortMode>().ToArray();
        int currentIndex = Array.IndexOf(modes, k_sortMode);
        k_sortMode = modes[(currentIndex + 1) % modes.Length];

        Inventory.SortInventory(k_sortMode);
        RefreshInventory();

        // Update tooltip
        var sortButton = k_sortControls.Q<VisualElement>();
        if (sortButton != null) {
            sortButton.tooltip = $"Sort: {k_sortMode}";
        }
    }

    #endregion

    #region Refresh

    private void RefreshInventory() {
        // Get items with optional filtering
        var items = Inventory.Items
            .Where(i => !i.IsEquipped) // Don't show equipped items in inventory grid
            .Where(i => !k_filterCategory.HasValue || i.Prototype.Category == k_filterCategory.Value)
            .ToList();

        // Update slots
        for (int i = 0; i < k_slots.Count; i++) {
            if (i < items.Count) {
                k_slots[i].SetItem(items[i]);
            } else {
                k_slots[i].ClearItem();
            }
        }

        // Update count label
        k_countLabel.text = $"{Inventory.UsedSlots}/{Inventory.MaxSlots}";

        // Update footer
        RefreshGold(Inventory.Gold, 0);
        k_weightLabel.text = $"⚖️ {Inventory.TotalWeight:F1}";
    }

    private void RefreshGold(int newGold, int delta) {
        k_goldLabel.text = $"💰 {newGold:N0}";

        if (delta != 0) {
            var theme = GameTheme.Current;
            k_goldLabel.style.color = delta > 0 ? theme.Colors.Success : theme.Colors.Error;

            // Reset color after delay
            schedule.Execute(() => {
                k_goldLabel.style.color = theme.Colors.TextPrimary;
            }).ExecuteLater(1000);
        }
    }

    private void ClearAllSlots() {
        foreach (var slot in k_slots) {
            slot.ClearItem();
        }
        k_countLabel.text = "0/0";
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Gets the item at a specific slot index.
    /// </summary>
    public ItemInstance? GetItemAtSlot(int index) {
        if (index < 0 || index >= k_slots.Count) return null;
        return k_slots[index].Item;
    }

    /// <summary>
    /// Selects a slot.
    /// </summary>
    public void SelectSlot(int index) {
        for (int i = 0; i < k_slots.Count; i++) {
            k_slots[i].SetSelected(i == index);
        }
    }

    /// <summary>
    /// Clears selection.
    /// </summary>
    public void ClearSelection() {
        foreach (var slot in k_slots) {
            slot.SetSelected(false);
        }
    }

    #endregion

    #region Theme

	private void ApplyTheme() {
		var theme = GameTheme.Current;
		var colors = theme.Colors;
		var borders = theme.Borders;
		var typography = theme.Typography;

		k_container.style.backgroundColor = colors.BackgroundSecondary;
		borders.ApplyRadius(k_container.style, borders.RadiusMD);
		theme.Spacing.PanelPadding.ApplyTo(k_container.style);

		// Apply typography to labels
		typography.TitleSmall.ApplyTo(k_titleLabel.style);
		k_titleLabel.style.color = colors.TextPrimary;

		typography.Caption.ApplyTo(k_countLabel.style);
		k_countLabel.style.color = colors.TextSecondary;

		typography.LabelSmall.ApplyTo(k_goldLabel.style);
		k_goldLabel.style.color = colors.TextPrimary;

		typography.Caption.ApplyTo(k_weightLabel.style);
		k_weightLabel.style.color = colors.TextSecondary;
	}

    private void OnThemeChanged(GameTheme theme) {
        ApplyTheme();
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
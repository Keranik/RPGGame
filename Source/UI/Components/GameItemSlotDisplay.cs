using RPGGame.Core;
using RPGGame.Core.Items;
using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Components;

/// <summary>
/// Slot display for items (inventory, equipment, loot).
/// Inherits from BaseGameSlotDisplay with full drag/drop support.
/// 
/// Supports three view tiers:
/// - Compact: Grid slot with icon, rarity frame, stack count (for inventories, spinners)
/// - Summary: Tooltip with name, stats, effects (hover or basic tooltip)
/// - Detailed: Full inspection panel with all item data (via Inspect action)
/// </summary>
public class GameItemSlotDisplay : BaseGameSlotDisplay<GameItemSlotDisplay> {
    #region Fields

    private ItemInstance? k_item;
    private string? k_emptyIconName;

    #endregion

    #region Properties

    public ItemInstance? Item => k_item;
    public override bool IsEmpty => k_item == null;

    #endregion

    #region Service Accessors

    private static InventoryManager Inventory => GameServices.Inventory;

    #endregion

    #region Constructor

    public GameItemSlotDisplay() : base() { }

    #endregion

    #region Fluent API - Item Data

    public GameItemSlotDisplay SetItem(ItemInstance? item) {
        k_item = item;

        if (item != null) {
            SetRarity(item.Prototype.Rarity);
            SetStackCount(item.Count);

            if (item.Durability.HasValue && item.MaxDurability.HasValue) {
                SetDurability(item.DurabilityPercent);
            } else {
                SetDurability(1f);
            }
        } else {
            SetRarity(RarityType.Common);
            SetStackCount(1);
            SetDurability(1f);
        }

        RefreshDisplay();
        return this;
    }

    public GameItemSlotDisplay ClearItem() {
        return SetItem(null);
    }

    public GameItemSlotDisplay SetEmptyIcon(string iconName) {
        k_emptyIconName = iconName;
        RefreshDisplay();
        return this;
    }

    #endregion

    #region Abstract Implementation

    protected override Texture2D? GetIcon() {
        return null;
    }

    protected override string GetDisplayName() {
        return k_item?.DisplayName ?? "Empty";
    }

    protected override string GetPlaceholderInitials() {
        return GetInitials(k_item?.Prototype.DisplayText.Name);
    }

    protected override GameTooltip CreateBasicTooltip() {
        // Tier 2: Summary tooltip - shows key item info at a glance
        var tooltip = new GameItemTooltip();
        
        tooltip.SetItem(k_item);
        tooltip.SetPosition(TooltipPosition.Right);
        tooltip.SetDelay(0);
        
        return tooltip;
    }

    protected override GameTooltip CreateExpandedTooltip() {
        // Tier 2+: Summary with comparison (Alt/Shift modifiers)
        var tooltip = new GameItemTooltip();
        
        tooltip.SetItem(k_item);
        tooltip.SetShowDetailed(true);
        tooltip.SetShowComparison(true);
		tooltip.SetShowCloseButton(true);
        tooltip.SetPosition(TooltipPosition.Right);
        
        return tooltip;
    }

    protected override bool CanDrag() => k_item != null && k_isDraggable;

    protected override bool CanAcceptDrop(ItemInstance? item) {
        return k_isDroppable;
    }

    public override ItemInstance? GetDraggedItem() => k_item;

	protected override void OnRightClickAction(Vector2 mousePosition) {
		if (k_item == null) return;
		ShowContextMenu(mousePosition);
	}

    #endregion

    #region Context Menu

	/// <summary>
	/// Shows a context menu for this item at the specified screen position.
	/// </summary>
	public void ShowContextMenu(Vector2 screenPosition) {
		if (k_item == null) return;

		var menu = GameContextMenu.Show(this, screenPosition)
			.AddHeader(k_item.DisplayName);

        // Tier 3: Inspect option
        menu.AddItem("🔍 Inspect", () => {
            ShowDetailedInspector();
            k_onInspect?.Invoke();
        });

        menu.AddDivider();

		// Item-specific actions
		if (k_item.Prototype.IsEquippable && !k_item.IsEquipped) {
			menu.AddItem("⚔️ Equip", EquipItem);
		}

		if (k_item.IsEquipped) {
			menu.AddItem("📤 Unequip", UnequipItem);
		}

		if (k_item.Prototype.IsUsable) {
			menu.AddItem("✨ Use", UseItem);
		}

		menu.AddDivider();

		// Lock/Favorite
		if (k_item.IsLocked) {
			menu.AddItem("🔓 Unlock", () => { k_item.IsLocked = false; RefreshDisplay(); });
		} else {
			menu.AddItem("🔒 Lock", () => { k_item.IsLocked = true; RefreshDisplay(); });
		}

		if (k_item.IsFavorite) {
			menu.AddItem("☆ Unfavorite", () => { k_item.IsFavorite = false; RefreshDisplay(); });
		} else {
			menu.AddItem("★ Favorite", () => { k_item.IsFavorite = true; RefreshDisplay(); });
		}

		menu.AddDivider();
		menu.AddItem("🗑️ Drop", DropItem, enabled: !k_item.IsLocked && !k_item.IsEquipped);

		menu.AddDivider();
		menu.AddItem("Cancel", null);
	}

	/// <inheritdoc/>
	protected override void AddTypeSpecificContextMenuItems(GameContextMenu menu) {
		// Already handled in ShowContextMenu - this slot has its own full implementation
	}

    /// <summary>
    /// Shows context menu at the current mouse cursor position.
    /// </summary>
    public void ShowContextMenuAtCursor(MouseDownEvent evt) {
        ShowContextMenu(evt.mousePosition);
    }

    #endregion

    #region Item Actions
	
    private void UseItem() {
        if (k_item == null) return;
        
        var result = Inventory.UseItem(k_item);
        if (result.Success) {
            GameToast.Show(result.Message, ToastType.Info);
        } else {
            GameToast.Show(result.Message, ToastType.Error);
        }
    }

    private void EquipItem() {
        if (k_item == null) return;
        
        if (Inventory.EquipItem(k_item)) {
            GameToast.Show($"Equipped {k_item.DisplayName}", ToastType.Success);
        } else {
            GameToast.Show($"Cannot equip {k_item.DisplayName}", ToastType.Error);
        }
    }

    private void UnequipItem() {
        if (k_item == null) return;
        
        if (Inventory.UnequipItem(k_item)) {
            GameToast.Show($"Unequipped {k_item.DisplayName}", ToastType.Info);
        } else {
            GameToast.Show($"Cannot unequip {k_item.DisplayName}", ToastType.Error);
        }
    }

    private void DropItem() {
        if (k_item == null) return;
        
        if (Inventory.RemoveItem(k_item)) {
            GameToast.Show($"Dropped {k_item.DisplayName}", ToastType.Warning);
        }
    }

    #endregion
}
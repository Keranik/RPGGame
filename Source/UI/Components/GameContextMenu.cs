using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Components;

/// <summary>
/// A reusable context menu component for right-click or click actions.
/// Supports nested menus, icons, dividers, and keyboard navigation.
/// 
/// <para>Usage:</para>
/// <code>
/// // Simple context menu
/// GameContextMenu.Show(targetElement, mousePosition)
///     .AddItem("Equip", () => EquipItem())
///     .AddItem("Drop", () => DropItem(), enabled: !item.IsLocked)
///     .AddDivider()
///     .AddItem("Cancel", null);
/// 
/// // With icons and submenus
/// GameContextMenu.Show(targetElement, position)
///     .AddItem("🗡️ Attack", OnAttack)
///     .AddSubMenu("🎒 Use Item", submenu => submenu
///         .AddItem("Health Potion", () => UsePotion("health"))
///         .AddItem("Mana Potion", () => UsePotion("mana")));
/// </code>
/// </summary>
public class GameContextMenu : VisualElement {
	#region Private Fields

	private readonly GameContainer k_menuContainer;
	private readonly List<ContextMenuItem> k_items = [];
	private int k_selectedIndex = -1;
	private GameContextMenu? k_parentMenu;
	private GameContextMenu? k_activeSubMenu;

	private static GameContextMenu? s_activeMenu;

	#endregion

	#region Constructors

	private GameContextMenu() {
		// Full screen overlay to catch outside clicks
		style.position = Position.Absolute;
		style.left = 0;
		style.top = 0;
		style.right = 0;
		style.bottom = 0;
		pickingMode = PickingMode.Position;

		k_menuContainer = new GameContainer("context-menu-container")
			.SetAbsolute()
			.SetColumn()
			.SetMinWidth(150)
			.Build();

		Add(k_menuContainer);

		// Close on background click
		RegisterCallback<ClickEvent>(OnBackgroundClick);
		RegisterCallback<KeyDownEvent>(OnKeyDown);

		ApplyTheme();
		GameTheme.OnThemeChanged += OnThemeChanged;
	}

	#endregion

	#region Static API

	/// <summary>
	/// Shows a context menu at the specified position.
	/// </summary>
	/// <summary>
	/// Shows a context menu at the specified position.
	/// The position should be in screen/panel coordinates (e.g., from MouseDownEvent.mousePosition).
	/// </summary>
	public static GameContextMenu Show(VisualElement anchor, Vector2 screenPosition) {
		// Close any existing menu
		CloseAll();

		var menu = new GameContextMenu();
		s_activeMenu = menu;

		// Add to the root of the visual tree so positioning is consistent
		var root = anchor.panel?.visualTree;
		if (root == null) {
			// Fallback to anchor's parent
			anchor.parent?.Add(menu);
		} else {
			root.Add(menu);
		}

		// Schedule positioning after layout is calculated
		menu.schedule.Execute(() => {
			menu.PositionMenu(screenPosition, root);
		});

		menu.Focus();

		return menu;
	}

	/// <summary>
	/// Positions the menu at the specified screen position, clamping to screen bounds.
	/// </summary>
	private void PositionMenu(Vector2 screenPosition, VisualElement? root) {
		// Get menu size (use estimates if not yet laid out)
		float menuWidth = k_menuContainer.resolvedStyle.width;
		float menuHeight = k_menuContainer.resolvedStyle.height;

		if (menuWidth <= 0) menuWidth = 200f;
		if (menuHeight <= 0) menuHeight = k_items.Count * 32f + 20f;

		float x = screenPosition.x;
		float y = screenPosition.y;

		// Clamp to screen bounds if we have a root
		if (root != null) {
			var rootBounds = root.worldBound;
			float padding = 8f;

			// Clamp right edge
			if (x + menuWidth > rootBounds.xMax - padding) {
				x = rootBounds.xMax - menuWidth - padding;
			}

			// Clamp bottom edge - flip above cursor if needed
			if (y + menuHeight > rootBounds.yMax - padding) {
				y = screenPosition.y - menuHeight;
				// If still off screen, just clamp it
				if (y < rootBounds.y + padding) {
					y = rootBounds.y + padding;
				}
			}

			// Clamp left edge
			if (x < rootBounds.x + padding) {
				x = rootBounds.x + padding;
			}

			// Clamp top edge
			if (y < rootBounds.y + padding) {
				y = rootBounds.y + padding;
			}
		}

		k_menuContainer.style.left = x;
		k_menuContainer.style.top = y;
	}

	/// <summary>
	/// Shows a context menu for a target element (positioned near the element).
	/// </summary>
	public static GameContextMenu ShowFor(VisualElement target, VisualElement parent) {
		var rect = target.worldBound;
		var position = new Vector2(rect.x, rect.yMax);
		return Show(parent, position);
	}

	/// <summary>
	/// Closes all active context menus.
	/// </summary>
	public static void CloseAll() {
		if (s_activeMenu != null) {
			s_activeMenu.Close();
			s_activeMenu = null;
		}
	}

	#endregion

	#region Fluent API - Items

	/// <summary>
	/// Adds a menu item.
	/// </summary>
	public GameContextMenu AddItem(string label, Action? onClick, bool enabled = true, string? icon = null) {
		var item = new ContextMenuItem {
			Label = label,
			OnClick = onClick,
			IsEnabled = enabled,
			Icon = icon,
			Type = ContextMenuItemType.Action
		};

		k_items.Add(item);
		AddItemElement(item);

		return this;
	}

	/// <summary>
	/// Adds a divider line.
	/// </summary>
	public GameContextMenu AddDivider() {
		var item = new ContextMenuItem {
			Type = ContextMenuItemType.Divider
		};

		k_items.Add(item);
		AddDividerElement();

		return this;
	}

	/// <summary>
	/// Adds a submenu.
	/// </summary>
	public GameContextMenu AddSubMenu(string label, Action<GameContextMenu> buildSubmenu, string? icon = null) {
		var item = new ContextMenuItem {
			Label = label,
			Icon = icon,
			Type = ContextMenuItemType.SubMenu,
			SubMenuBuilder = buildSubmenu
		};

		k_items.Add(item);
		AddSubMenuElement(item);

		return this;
	}

	/// <summary>
	/// Adds a header (non-clickable label).
	/// </summary>
	public GameContextMenu AddHeader(string label) {
		var item = new ContextMenuItem {
			Label = label,
			Type = ContextMenuItemType.Header
		};

		k_items.Add(item);
		AddHeaderElement(item);

		return this;
	}

	#endregion

	#region Item Element Creation

	private void AddItemElement(ContextMenuItem item) {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		var element = new GameContainer($"context-menu-item-{item.Label.ToLower().Replace(" ", "-")}")
			.SetRow()
			.SetAlignItems(Align.Center)
			.SetPadding(spacing.XS, spacing.SM, spacing.XS, spacing.SM)
			.Build();

		element.userData = item;

		// Icon
		if (!string.IsNullOrEmpty(item.Icon)) {
			var iconLabel = new GameLabel(item.Icon)
				.SetStyle(LabelStyle.BodyMedium)
				.SetMarginRight(spacing.XS)
				.SetWidth(theme.Icons.SizeMD)
				.Build();
			element.Add(iconLabel);
		}

		// Label
		var label = new GameLabel(item.Label)
			.SetStyle(LabelStyle.BodyMedium)
			.SetFlexGrow(1f)
			.Build();
		element.Add(label);

		// Enable/disable styling
		if (!item.IsEnabled) {
			element.style.opacity = 0.5f;
			element.pickingMode = PickingMode.Ignore;
		} else {
			element.RegisterCallback<ClickEvent>(evt => {
				item.OnClick?.Invoke();
				Close();
				evt.StopPropagation();
			});
			element.RegisterCallback<MouseEnterEvent>(evt => {
				CloseSubMenu();
				k_selectedIndex = k_items.IndexOf(item);
				ApplyItemHoverStyle(element, true);
			});
			element.RegisterCallback<MouseLeaveEvent>(evt => {
				ApplyItemHoverStyle(element, false);
			});
		}

		k_menuContainer.Add(element);
	}

	private void AddDividerElement() {
		var divider = new GameDivider()
			.SetMargin(GameTheme.Current.Spacing.XXS)
			.Build();

		k_menuContainer.Add(divider);
	}

	private void AddSubMenuElement(ContextMenuItem item) {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		var element = new GameContainer($"context-menu-submenu-{item.Label.ToLower().Replace(" ", "-")}")
			.SetRow()
			.SetAlignItems(Align.Center)
			.SetPadding(spacing.XS, spacing.SM, spacing.XS, spacing.SM)
			.Build();

		element.userData = item;

		// Icon
		if (!string.IsNullOrEmpty(item.Icon)) {
			var iconLabel = new GameLabel(item.Icon)
				.SetStyle(LabelStyle.BodyMedium)
				.SetMarginRight(spacing.XS)
				.SetWidth(theme.Icons.SizeMD)
				.Build();
			element.Add(iconLabel);
		}

		// Label
		var label = new GameLabel(item.Label)
			.SetStyle(LabelStyle.BodyMedium)
			.SetFlexGrow(1f)
			.Build();
		element.Add(label);

		// Arrow indicator
		var arrow = new GameLabel("▶")
			.SetStyle(LabelStyle.Caption)
			.SetColor(LabelColor.Secondary)
			.SetMarginLeft(spacing.XS)
			.Build();
		element.Add(arrow);

		element.RegisterCallback<MouseEnterEvent>(evt => {
			k_selectedIndex = k_items.IndexOf(item);
			ApplyItemHoverStyle(element, true);
			ShowSubMenu(item, element);
		});
		element.RegisterCallback<MouseLeaveEvent>(evt => {
			// Don't close immediately - allow mouse to move to submenu
			schedule.Execute(() => {
				if (k_activeSubMenu == null) {
					ApplyItemHoverStyle(element, false);
				}
			}).ExecuteLater(150);
		});

		k_menuContainer.Add(element);
	}

	private void AddHeaderElement(ContextMenuItem item) {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		var header = new GameLabel(item.Label)
			.SetStyle(LabelStyle.LabelSmall)
			.SetColor(LabelColor.Tertiary)
			.SetPadding(spacing.XXS, spacing.XS, spacing.XXS, spacing.XS)
			.Build();

		header.pickingMode = PickingMode.Ignore;

		k_menuContainer.Add(header);
	}

	private void ApplyItemHoverStyle(VisualElement element, bool isHovered) {
		var theme = GameTheme.Current;
		var colors = theme.Colors;

		element.style.backgroundColor = isHovered ? colors.SurfaceHover : colors.Transparent;
	}

	#endregion

	#region SubMenu

	private void ShowSubMenu(ContextMenuItem item, VisualElement anchor) {
		CloseSubMenu();

		if (item.SubMenuBuilder == null) {
			return;
		}

		k_activeSubMenu = new GameContextMenu();
		k_activeSubMenu.k_parentMenu = this;
		k_activeSubMenu.style.position = Position.Absolute;
		k_activeSubMenu.style.left = 0;
		k_activeSubMenu.style.top = 0;
		k_activeSubMenu.style.right = StyleKeyword.Auto;
		k_activeSubMenu.style.bottom = StyleKeyword.Auto;

		// Position next to parent item
		var rect = anchor.worldBound;
		k_activeSubMenu.k_menuContainer.style.left = rect.xMax;
		k_activeSubMenu.k_menuContainer.style.top = rect.y;

		item.SubMenuBuilder(k_activeSubMenu);

		parent?.Add(k_activeSubMenu);
	}

	private void CloseSubMenu() {
		if (k_activeSubMenu != null) {
			k_activeSubMenu.RemoveFromHierarchy();
			k_activeSubMenu = null;
		}
	}

	#endregion

	#region Event Handlers

	private void OnBackgroundClick(ClickEvent evt) {
		if (evt.target == this) {
			Close();
		}
	}

	private void OnKeyDown(KeyDownEvent evt) {
		switch (evt.keyCode) {
			case KeyCode.Escape:
				Close();
				evt.StopPropagation();
				break;

			case KeyCode.UpArrow:
				NavigateSelection(-1);
				evt.StopPropagation();
				break;

			case KeyCode.DownArrow:
				NavigateSelection(1);
				evt.StopPropagation();
				break;

			case KeyCode.Return:
			case KeyCode.KeypadEnter:
				ActivateSelection();
				evt.StopPropagation();
				break;

			case KeyCode.RightArrow:
				if (k_selectedIndex >= 0 && k_items[k_selectedIndex].Type == ContextMenuItemType.SubMenu) {
					var element = k_menuContainer.Children().ElementAt(k_selectedIndex);
					ShowSubMenu(k_items[k_selectedIndex], element);
					k_activeSubMenu?.Focus();
				}
				evt.StopPropagation();
				break;

			case KeyCode.LeftArrow:
				if (k_parentMenu != null) {
					k_parentMenu.CloseSubMenu();
					k_parentMenu.Focus();
				}
				evt.StopPropagation();
				break;
		}
	}

	private void NavigateSelection(int direction) {
		int newIndex = k_selectedIndex;
		int attempts = 0;

		do {
			newIndex += direction;
			if (newIndex < 0) newIndex = k_items.Count - 1;
			if (newIndex >= k_items.Count) newIndex = 0;
			attempts++;
		} while (attempts < k_items.Count &&
		         (k_items[newIndex].Type == ContextMenuItemType.Divider ||
		          k_items[newIndex].Type == ContextMenuItemType.Header ||
		          !k_items[newIndex].IsEnabled));

		if (newIndex != k_selectedIndex) {
			// Update visual selection
			UpdateSelectionVisuals(k_selectedIndex, false);
			k_selectedIndex = newIndex;
			UpdateSelectionVisuals(k_selectedIndex, true);
		}
	}

	private void UpdateSelectionVisuals(int index, bool selected) {
		if (index < 0 || index >= k_menuContainer.childCount) {
			return;
		}

		var element = k_menuContainer.Children().ElementAt(index);
		if (element.name.StartsWith("context-menu-item") || element.name.StartsWith("context-menu-submenu")) {
			ApplyItemHoverStyle(element, selected);
		}
	}

	private void ActivateSelection() {
		if (k_selectedIndex < 0 || k_selectedIndex >= k_items.Count) {
			return;
		}

		var item = k_items[k_selectedIndex];
		if (item.IsEnabled && item.OnClick != null) {
			item.OnClick.Invoke();
			Close();
		}
	}

	#endregion

	#region Close

	/// <summary>
	/// Closes this menu.
	/// </summary>
	public void Close() {
		CloseSubMenu();
		k_parentMenu?.CloseSubMenu();

		GameTheme.OnThemeChanged -= OnThemeChanged;
		RemoveFromHierarchy();

		if (s_activeMenu == this) {
			s_activeMenu = null;
		}
	}

	#endregion

	#region Theme

	private void ApplyTheme() {
		var theme = GameTheme.Current;
		var colors = theme.Colors;
		var borders = theme.Borders;
		var spacing = theme.Spacing;

		k_menuContainer
			.SetBackgroundColor(colors.Surface)
			.SetBorderRadius(borders.RadiusSM)
			.SetBorderWidth(borders.WidthThin)
			.SetBorderColor(colors.SurfaceBorder)
			.SetPadding(spacing.XXS);
	}

	private void OnThemeChanged(GameTheme theme) {
		ApplyTheme();
	}

	#endregion
}

#region Supporting Types

internal class ContextMenuItem {
	public string Label { get; set; } = "";
	public string? Icon { get; set; }
	public Action? OnClick { get; set; }
	public bool IsEnabled { get; set; } = true;
	public ContextMenuItemType Type { get; set; } = ContextMenuItemType.Action;
	public Action<GameContextMenu>? SubMenuBuilder { get; set; }
}

internal enum ContextMenuItemType {
	Action,
	Divider,
	SubMenu,
	Header
}

#endregion
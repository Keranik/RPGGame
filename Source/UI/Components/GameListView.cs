using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Components;

#nullable disable

/// <summary>
/// A themed list view component for displaying scrollable lists of items.
/// Supports selection, virtualization, and custom item templates.
/// 
/// <para>Usage:</para>
/// <code>
/// var inventoryList = new GameListView&lt;ItemData&gt;()
///     .SetItems(inventory.Items)
///     .SetItemTemplate(item => CreateItemRow(item))
///     .SetSelectionType(SelectionType.Single)
///     .OnSelectionChanged(items => SelectItem(items.First()))
///     .Build();
/// </code>
/// </summary>
public class GameListView<T> : VisualElement {
	#region Private Fields

	private readonly VisualElement k_container;
	private readonly ScrollView k_scrollView;
	private readonly VisualElement k_listContainer;
	private readonly Label k_emptyLabel;

	private IList<T> k_items = new List<T>();
	private Func<T, VisualElement> k_itemTemplate;
	private ListSelectionType k_selectionType = ListSelectionType.None;
	private HashSet<int> k_selectedIndices = new();
	private ListVariant k_variant = ListVariant.Default;

	private Action<IEnumerable<T>> k_onSelectionChanged;
	private Action<T, int> k_onItemClicked;

	#endregion

	#region Constructors

	public GameListView() {
		k_container = new VisualElement {
			style = { flexGrow = 1 }
		};

		k_scrollView = new ScrollView(ScrollViewMode.Vertical) {
			style = { flexGrow = 1 }
		};

		k_listContainer = new VisualElement {
			style = { flexDirection = FlexDirection.Column }
		};

		k_emptyLabel = new Label("No items") {
			style = {
				display = DisplayStyle.None,
				unityTextAlign = TextAnchor.MiddleCenter
			}
		};

		k_scrollView.Add(k_listContainer);
		k_container.Add(k_scrollView);
		k_container.Add(k_emptyLabel);

		Add(k_container);

		GameTheme.OnThemeChanged += OnThemeChanged;
	}

	#endregion

	#region Fluent API - Data

	public GameListView<T> SetItems(IList<T> items) {
		k_items = items ?? new List<T>();
		RefreshList();
		return this;
	}

	public GameListView<T> SetItems(IEnumerable<T> items) {
		k_items = items?.ToList() ?? new List<T>();
		RefreshList();
		return this;
	}

	public GameListView<T> SetItemTemplate(Func<T, VisualElement> template) {
		k_itemTemplate = template;
		RefreshList();
		return this;
	}

	public GameListView<T> SetEmptyText(string text) {
		k_emptyLabel.text = text;
		return this;
	}

	public IList<T> Items => k_items;

	public IEnumerable<T> SelectedItems => k_selectedIndices.Select(i => k_items[i]);

	public T SelectedItem => k_selectedIndices.Count > 0 ? k_items[k_selectedIndices.First()] : default;

	#endregion

	#region Fluent API - Appearance

	public GameListView<T> SetVariant(ListVariant variant) {
		k_variant = variant;
		ApplyTheme();
		return this;
	}

	public GameListView<T> SetSelectionType(ListSelectionType selectionType) {
		k_selectionType = selectionType;
		return this;
	}

	public GameListView<T> SetWidth(float width) {
		style.width = width;
		return this;
	}

	public GameListView<T> SetWidth(Length width) {
		style.width = width;
		return this;
	}

	public GameListView<T> SetHeight(float height) {
		style.height = height;
		return this;
	}

	public GameListView<T> SetHeight(Length height) {
		style.height = height;
		return this;
	}

	public GameListView<T> SetMaxHeight(float maxHeight) {
		style.maxHeight = maxHeight;
		return this;
	}

	public GameListView<T> SetMargin(int top = 0, int right = 0, int bottom = 0, int left = 0) {
		style.marginTop = top;
		style.marginRight = right;
		style.marginBottom = bottom;
		style.marginLeft = left;
		return this;
	}

	#endregion

	#region Fluent API - Events

	public GameListView<T> OnSelectionChanged(Action<IEnumerable<T>> callback) {
		k_onSelectionChanged = callback;
		return this;
	}

	public GameListView<T> OnItemClicked(Action<T, int> callback) {
		k_onItemClicked = callback;
		return this;
	}

	#endregion

	#region Selection

	public GameListView<T> SelectIndex(int index) {
		if (index < 0 || index >= k_items.Count) {
			return this;
		}

		if (k_selectionType == ListSelectionType.Single) {
			k_selectedIndices.Clear();
		}

		k_selectedIndices.Add(index);
		UpdateSelectionVisuals();
		k_onSelectionChanged?.Invoke(SelectedItems);

		return this;
	}

	public GameListView<T> DeselectIndex(int index) {
		k_selectedIndices.Remove(index);
		UpdateSelectionVisuals();
		k_onSelectionChanged?.Invoke(SelectedItems);
		return this;
	}

	public GameListView<T> ClearSelection() {
		k_selectedIndices.Clear();
		UpdateSelectionVisuals();
		k_onSelectionChanged?.Invoke(SelectedItems);
		return this;
	}

	public GameListView<T> SelectAll() {
		if (k_selectionType != ListSelectionType.Multiple) {
			return this;
		}

		for (int i = 0; i < k_items.Count; i++) {
			k_selectedIndices.Add(i);
		}
		UpdateSelectionVisuals();
		k_onSelectionChanged?.Invoke(SelectedItems);

		return this;
	}

	#endregion

	#region Build

	public GameListView<T> Build() {
		ApplyTheme();
		RefreshList();
		return this;
	}

	#endregion

	#region List Management

	public void RefreshList() {
		k_listContainer.Clear();

		if (k_items.Count == 0) {
			k_emptyLabel.style.display = DisplayStyle.Flex;
			k_scrollView.style.display = DisplayStyle.None;
			return;
		}

		k_emptyLabel.style.display = DisplayStyle.None;
		k_scrollView.style.display = DisplayStyle.Flex;

		for (int i = 0; i < k_items.Count; i++) {
			var item = k_items[i];
			var itemElement = CreateListItem(item, i);
			k_listContainer.Add(itemElement);
		}

		UpdateSelectionVisuals();
	}

	public void AddItem(T item) {
		k_items.Add(item);
		var itemElement = CreateListItem(item, k_items.Count - 1);
		k_listContainer.Add(itemElement);

		k_emptyLabel.style.display = DisplayStyle.None;
		k_scrollView.style.display = DisplayStyle.Flex;
	}

	public void RemoveItem(T item) {
		int index = k_items.IndexOf(item);
		if (index >= 0) {
			RemoveItemAt(index);
		}
	}

	public void RemoveItemAt(int index) {
		if (index < 0 || index >= k_items.Count) {
			return;
		}

		k_items.RemoveAt(index);
		k_selectedIndices.Remove(index);

		// Adjust selected indices
		var newSelectedIndices = new HashSet<int>();
		foreach (var selectedIndex in k_selectedIndices) {
			if (selectedIndex > index) {
				newSelectedIndices.Add(selectedIndex - 1);
			} else {
				newSelectedIndices.Add(selectedIndex);
			}
		}
		k_selectedIndices = newSelectedIndices;

		RefreshList();
	}

	private VisualElement CreateListItem(T item, int index) {
		var wrapper = new VisualElement {
			name = $"list-item-{index}",
			userData = index
		};

		// Use custom template or default
		VisualElement content;
		if (k_itemTemplate != null) {
			content = k_itemTemplate(item);
		} else {
			content = new Label(item?.ToString() ?? "null");
		}

		wrapper.Add(content);

		// Apply list item styling
		ApplyListItemStyle(wrapper, index, false);

		// Register click handler
		if (k_selectionType != ListSelectionType.None) {
			wrapper.RegisterCallback<ClickEvent>(evt => OnItemClick(index));
		}

		wrapper.RegisterCallback<MouseEnterEvent>(evt => OnItemMouseEnter(wrapper, index));
		wrapper.RegisterCallback<MouseLeaveEvent>(evt => OnItemMouseLeave(wrapper, index));

		return wrapper;
	}

	private void ApplyListItemStyle(VisualElement element, int index, bool isHovered) {
		var theme = GameTheme.Current;
		var colors = theme.Colors;
		var spacing = theme.Spacing;
		var listStyles = theme.Components.List;

		bool isSelected = k_selectedIndices.Contains(index);

		element.style.paddingTop = spacing.XS;
		element.style.paddingBottom = spacing.XS;
		element.style.paddingLeft = spacing.SM;
		element.style.paddingRight = spacing.SM;
		element.style.minHeight = k_variant == ListVariant.Compact 
			? listStyles.ItemHeightCompact 
			: listStyles.ItemHeight;

		if (isSelected) {
			element.style.backgroundColor = colors.SurfaceSelected;
		} else if (isHovered) {
			element.style.backgroundColor = colors.SurfaceHover;
		} else {
			element.style.backgroundColor = index % 2 == 0 && k_variant == ListVariant.Striped
				? colors.BackgroundSecondary
				: colors.Transparent;
		}

		// Border between items
		if (k_variant != ListVariant.Compact) {
			element.style.borderBottomWidth = theme.Borders.WidthThin;
			element.style.borderBottomColor = colors.SurfaceBorder;
		}
	}

	private void UpdateSelectionVisuals() {
		for (int i = 0; i < k_listContainer.childCount; i++) {
			var child = k_listContainer[i];
			ApplyListItemStyle(child, i, false);
		}
	}

	#endregion

	#region Theme Application

	private void ApplyTheme() {
		var theme = GameTheme.Current;
		var colors = theme.Colors;
		var borders = theme.Borders;
		var typography = theme.Typography;

		// Container styling
		if (k_variant != ListVariant.Plain) {
			borders.ApplyColor(k_container.style, colors.SurfaceBorder);
			borders.ApplyWidth(k_container.style, borders.WidthThin);
			borders.ApplyRadius(k_container.style, borders.RadiusMD);
		}

		// Empty label styling
		typography.BodyMedium.ApplyTo(k_emptyLabel.style);
		k_emptyLabel.style.color = colors.TextSecondary;
		k_emptyLabel.style.paddingTop = theme.Spacing.XL;
		k_emptyLabel.style.paddingBottom = theme.Spacing.XL;

		// Scrollbar styling
		var scrollbar = k_scrollView.Q<Scroller>();
		if (scrollbar != null) {
			scrollbar.style.width = theme.Components.ScrollView.ScrollbarWidth;
		}
	}

	private void OnThemeChanged(GameTheme theme) {
		ApplyTheme();
		RefreshList();
	}

	#endregion

	#region Event Handlers

	private void OnItemClick(int index) {
		if (k_selectionType == ListSelectionType.None) {
			k_onItemClicked?.Invoke(k_items[index], index);
			return;
		}

		if (k_selectionType == ListSelectionType.Single) {
			k_selectedIndices.Clear();
			k_selectedIndices.Add(index);
		} else {
			if (k_selectedIndices.Contains(index)) {
				k_selectedIndices.Remove(index);
			} else {
				k_selectedIndices.Add(index);
			}
		}

		UpdateSelectionVisuals();
		k_onSelectionChanged?.Invoke(SelectedItems);
		k_onItemClicked?.Invoke(k_items[index], index);
	}

	private void OnItemMouseEnter(VisualElement element, int index) {
		ApplyListItemStyle(element, index, true);
	}

	private void OnItemMouseLeave(VisualElement element, int index) {
		ApplyListItemStyle(element, index, false);
	}

	#endregion

	#region Cleanup

	new public void RemoveFromHierarchy() {
		GameTheme.OnThemeChanged -= OnThemeChanged;
		base.RemoveFromHierarchy();
	}

	#endregion
}

#region Enums

public enum ListSelectionType {
	None,
	Single,
	Multiple
}

public enum ListVariant {
	Default,
	Compact,
	Striped,
	Plain
}

#endregion
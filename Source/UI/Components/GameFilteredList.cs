using System;
using System.Collections.Generic;
using System.Linq;
using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Components;

/// <summary>
/// A generic filterable, searchable list component.
/// Supports filtering by tags, text search, and custom predicates.
/// </summary>
/// <typeparam name="T">The type of items in the list.</typeparam>
public class GameFilteredList<T> : VisualElement {
	#region Private Fields

	private readonly GameContainer k_headerContainer;
	private readonly GameContainer k_filterContainer;
	private readonly GameContainer k_searchContainer;
	private readonly GameContainer k_listContainer;
	private readonly ScrollView k_scrollView;
	private readonly GameLabel k_countLabel;
	private readonly GameTextField k_searchField;

	private List<T> k_allItems = [];
	private List<T> k_filteredItems = [];
	private Func<T, VisualElement>? k_itemTemplate;
	private Func<T, string>? k_searchTextExtractor;
	private Func<T, bool>? k_customFilter;
	private string k_searchText = "";
	private readonly Dictionary<string, Func<T, bool>> k_activeFilters = new();
	private readonly Dictionary<string, GameButton> k_filterButtons = new();

	private T? k_selectedItem;
	private VisualElement? k_selectedElement;
	private Action<T>? k_onItemSelected;
	private Action<T>? k_onItemDoubleClicked;

	private float k_itemHeight = 48f;
	private bool k_showSearch = true;
	private bool k_showCount = true;
	private string k_emptyMessage = "No items found";

	#endregion

	#region Properties

	public IReadOnlyList<T> AllItems => k_allItems;
	public IReadOnlyList<T> FilteredItems => k_filteredItems;
	public T? SelectedItem => k_selectedItem;
	public int FilteredCount => k_filteredItems.Count;

	#endregion

	#region Constructor

	public GameFilteredList() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		style.flexGrow = 1;
		style.flexDirection = FlexDirection.Column;

		// Header with search and count
		k_headerContainer = new GameContainer("filter-header")
			.SetRow()
			.SetSpaceBetween()
			.SetAlignItems(Align.Center)
			.SetMarginBottom(spacing.SM)
			.Build();

		// Search field
		k_searchContainer = new GameContainer("search-container")
			.SetGrow()
			.SetMarginRight(spacing.SM)
			.Build();

		k_searchField = new GameTextField()
			.SetPlaceholder("Search...")
			.Build();
		k_searchField.OnValueChanged(OnSearchChanged);
		k_searchContainer.AddChild(k_searchField);

		// Count label
		k_countLabel = new GameLabel("0 items")
			.SetStyle(LabelStyle.Caption)
			.SetColor(LabelColor.Secondary)
			.Build();

		k_headerContainer
			.AddChild(k_searchContainer)
			.AddChild(k_countLabel);

		// Filter buttons container
		k_filterContainer = new GameContainer("filter-buttons")
			.SetRow()
			.SetFlexWrap(Wrap.Wrap)
			.SetMarginBottom(spacing.SM)
			.Build();

		// Scrollable list container
		k_scrollView = new ScrollView(ScrollViewMode.Vertical) {
			style = { flexGrow = 1 }
		};

		k_listContainer = new GameContainer("list-items")
			.SetColumn()
			.SetGrow()
			.Build();

		k_scrollView.Add(k_listContainer);

		Add(k_headerContainer);
		Add(k_filterContainer);
		Add(k_scrollView);
	}

	#endregion

	#region Builder Methods

	public GameFilteredList<T> SetItems(IEnumerable<T> items) {
		k_allItems = items.ToList();
		ApplyFilters();
		return this;
	}

	public GameFilteredList<T> SetItemTemplate(Func<T, VisualElement> template) {
		k_itemTemplate = template;
		return this;
	}

	public GameFilteredList<T> SetSearchTextExtractor(Func<T, string> extractor) {
		k_searchTextExtractor = extractor;
		return this;
	}

	public GameFilteredList<T> AddFilter(string name, string label, Func<T, bool> predicate, bool isDefault = false) {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		var button = new GameButton(label)
			.SetVariant(isDefault ? ButtonVariant.Primary : ButtonVariant.Outline)
			.SetSize(ButtonSize.Small)
			.OnClick(() => ToggleFilter(name, predicate))
			.SetMarginRight(spacing.XS)
			.SetMarginBottom(spacing.XS)
			.Build();

		k_filterButtons[name] = button;
		k_filterContainer.AddChild(button);

		if (isDefault) {
			k_activeFilters[name] = predicate;
		}

		return this;
	}

	public GameFilteredList<T> SetCustomFilter(Func<T, bool> filter) {
		k_customFilter = filter;
		return this;
	}

	public GameFilteredList<T> SetItemHeight(float height) {
		k_itemHeight = height;
		return this;
	}

	public GameFilteredList<T> SetShowSearch(bool show) {
		k_showSearch = show;
		k_searchContainer.SetVisible(show);
		return this;
	}

	public GameFilteredList<T> SetShowCount(bool show) {
		k_showCount = show;
		k_countLabel.SetVisible(show);
		return this;
	}

	public GameFilteredList<T> SetEmptyMessage(string message) {
		k_emptyMessage = message;
		return this;
	}

	public GameFilteredList<T> OnItemSelected(Action<T> callback) {
		k_onItemSelected = callback;
		return this;
	}

	public GameFilteredList<T> OnItemDoubleClicked(Action<T> callback) {
		k_onItemDoubleClicked = callback;
		return this;
	}

	public GameFilteredList<T> Build() {
		ApplyFilters();
		return this;
	}

	#endregion

	#region Public Methods

	public void Refresh() {
		ApplyFilters();
	}

	public void ClearFilters() {
		k_activeFilters.Clear();
		k_searchText = "";
		k_searchField.SetValue("");

		foreach (var button in k_filterButtons.Values) {
			button.RemoveFromClassList("selected");
		}

		ApplyFilters();
	}

	public void SelectItem(T? item) {
		if (item == null) {
			ClearSelection();
			return;
		}

		k_selectedItem = item;

		int index = k_filteredItems.IndexOf(item);
		if (index >= 0 && index < k_listContainer.childCount) {
			if (k_selectedElement != null) {
				k_selectedElement.RemoveFromClassList("selected");
			}
			k_selectedElement = k_listContainer[index];
			k_selectedElement.AddToClassList("selected");
		}

		k_onItemSelected?.Invoke(item);
	}

	public void ClearSelection() {
		k_selectedItem = default;
		if (k_selectedElement != null) {
			k_selectedElement.RemoveFromClassList("selected");
			k_selectedElement = null;
		}
	}

	public void ScrollToItem(T item) {
		int index = k_filteredItems.IndexOf(item);
		if (index >= 0) {
			float scrollPosition = index * k_itemHeight;
			k_scrollView.scrollOffset = new Vector2(0, scrollPosition);
		}
	}

	#endregion

	#region Private Methods

	private void OnSearchChanged(string text) {
		k_searchText = text?.ToLowerInvariant() ?? "";
		ApplyFilters();
	}

	private void ToggleFilter(string name, Func<T, bool> predicate) {
		if (k_activeFilters.ContainsKey(name)) {
			k_activeFilters.Remove(name);
			k_filterButtons[name].RemoveFromClassList("selected");
		} else {
			k_activeFilters[name] = predicate;
			k_filterButtons[name].AddToClassList("selected");
		}

		ApplyFilters();
	}

	private void ApplyFilters() {
		IEnumerable<T> filtered = k_allItems;

		if (k_customFilter != null) {
			filtered = filtered.Where(k_customFilter);
		}

		if (k_activeFilters.Count > 0) {
			filtered = filtered.Where(item =>
				k_activeFilters.Values.Any(filter => filter(item))
			);
		}

		if (!string.IsNullOrEmpty(k_searchText) && k_searchTextExtractor != null) {
			filtered = filtered.Where(item => {
				var searchable = k_searchTextExtractor(item)?.ToLowerInvariant() ?? "";
				return searchable.Contains(k_searchText);
			});
		}

		k_filteredItems = filtered.ToList();

		RebuildList();
		UpdateCountLabel();
	}

	private void RebuildList() {
		k_listContainer.Clear();
		k_selectedElement = null;

		var theme = GameTheme.Current;
		var colors = theme.Colors;
		var spacing = theme.Spacing;
		var borders = theme.Borders;

		if (k_filteredItems.Count == 0) {
			var emptyLabel = new GameLabel(k_emptyMessage)
				.SetStyle(LabelStyle.BodyMedium)
				.SetColor(LabelColor.Tertiary)
				.SetAlignSelf(Align.Center)
				.SetMarginTop(spacing.LG)
				.Build();
			k_listContainer.AddChild(emptyLabel);
			return;
		}

		if (k_itemTemplate == null) {
			return;
		}

		foreach (var item in k_filteredItems) {
			var element = k_itemTemplate(item);

			var container = new GameContainer("list-item")
				.SetPadding(spacing.XS, spacing.SM, spacing.XS, spacing.SM)
				.SetBottomSeparator(c => c.SurfaceBorder, borders.WidthThin)
				.Build();

			container.Add(element);

			var capturedItem = item;
			container
				.OnClick(() => SelectItem(capturedItem))
				.OnHoverBackground(c => c.SurfaceHover);

			container.RegisterCallback<ClickEvent>(evt => {
				if (evt.clickCount == 2) {
					k_onItemDoubleClicked?.Invoke(capturedItem);
				}
			});

			container.RegisterCallback<AttachToPanelEvent>(_ => {
				if (capturedItem != null && capturedItem.Equals(k_selectedItem)) {
					container.AddToClassList("selected");
					container.SetBackgroundColor(colors.SurfaceSelected);
					k_selectedElement = container;
				}
			});

			k_listContainer.AddChild(container);
		}
	}

	private void UpdateCountLabel() {
		if (k_showCount) {
			string countText = k_filteredItems.Count == k_allItems.Count
				? $"{k_allItems.Count} items"
				: $"{k_filteredItems.Count} / {k_allItems.Count} items";
			k_countLabel.SetText(countText);
		}
	}

	#endregion
}
using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Components;

#nullable disable

/// <summary>
/// A themed grid layout component for inventory, skill trees, and structured layouts.
/// Supports fixed columns/rows, cell sizing, and selection.
/// 
/// <para>Usage:</para>
/// <code>
/// // Inventory grid
/// var grid = new GameGrid&lt;ItemData&gt;()
///     .SetColumns(6)
///     .SetCellSize(64, 64)
///     .SetItems(inventory.Items)
///     .SetItemTemplate(item => CreateItemSlot(item))
///     .SetSelectionType(GridSelectionType.Single)
///     .OnCellClicked((item, x, y) => ShowItemDetails(item))
///     .Build();
/// 
/// // Slot machine layout (5x3)
/// var slotGrid = new GameGrid&lt;ReelData&gt;()
///     .SetColumns(5)
///     .SetRows(3)
///     .SetCellSize(80, 80)
///     .Build();
/// </code>
/// </summary>
public class GameGrid<T> : VisualElement {
	#region Private Fields

	private readonly VisualElement k_container;
	private readonly ScrollView k_scrollView;
	private readonly VisualElement k_gridContainer;

	private IList<T> k_items = new List<T>();
	private Func<T, VisualElement> k_itemTemplate;
	private Func<int, int, VisualElement> k_emptyCellTemplate;
	private int k_columns = 4;
	private int k_rows = -1; // -1 = auto
	private float k_cellWidth = 64f;
	private float k_cellHeight = 64f;
	private float k_cellGap = 4f;
	private GridSelectionType k_selectionType = GridSelectionType.None;
	private HashSet<(int x, int y)> k_selectedCells = new();
	private GridVariant k_variant = GridVariant.Default;

	private Action<T, int, int> k_onCellClicked;
	private Action<IEnumerable<(T item, int x, int y)>> k_onSelectionChanged;

	#endregion

	#region Constructors

	public GameGrid() {
		k_container = new VisualElement {
			style = { flexGrow = 1 }
		};

		k_scrollView = new ScrollView(ScrollViewMode.Vertical) {
			style = { flexGrow = 1 }
		};

		k_gridContainer = new VisualElement {
			style = {
				flexDirection = FlexDirection.Row,
				flexWrap = Wrap.Wrap
			}
		};

		k_scrollView.Add(k_gridContainer);
		k_container.Add(k_scrollView);
		Add(k_container);

		GameTheme.OnThemeChanged += OnThemeChanged;
	}

	#endregion

	#region Fluent API - Layout

	public GameGrid<T> SetColumns(int columns) {
		k_columns = Mathf.Max(1, columns);
		RebuildGrid();
		return this;
	}

	public GameGrid<T> SetRows(int rows) {
		k_rows = rows; // -1 for auto
		RebuildGrid();
		return this;
	}

	public GameGrid<T> SetCellSize(float width, float height) {
		k_cellWidth = width;
		k_cellHeight = height;
		RebuildGrid();
		return this;
	}

	public GameGrid<T> SetCellSize(float size) {
		return SetCellSize(size, size);
	}

	public GameGrid<T> SetGap(float gap) {
		k_cellGap = gap;
		RebuildGrid();
		return this;
	}

	#endregion

	#region Fluent API - Data

	public GameGrid<T> SetItems(IList<T> items) {
		k_items = items ?? new List<T>();
		RebuildGrid();
		return this;
	}

	public GameGrid<T> SetItems(IEnumerable<T> items) {
		return SetItems(items?.ToList());
	}

	public GameGrid<T> SetItemTemplate(Func<T, VisualElement> template) {
		k_itemTemplate = template;
		RebuildGrid();
		return this;
	}

	public GameGrid<T> SetEmptyCellTemplate(Func<int, int, VisualElement> template) {
		k_emptyCellTemplate = template;
		RebuildGrid();
		return this;
	}

	public T GetItemAt(int x, int y) {
		int index = y * k_columns + x;
		return index < k_items.Count ? k_items[index] : default;
	}

	public void SetItemAt(int x, int y, T item) {
		int index = y * k_columns + x;
		if (index < k_items.Count) {
			k_items[index] = item;
			RefreshCell(x, y);
		}
	}

	#endregion

	#region Fluent API - Appearance

	public GameGrid<T> SetVariant(GridVariant variant) {
		k_variant = variant;
		ApplyTheme();
		return this;
	}

	public GameGrid<T> SetSelectionType(GridSelectionType selectionType) {
		k_selectionType = selectionType;
		return this;
	}

	public GameGrid<T> SetSize(float width, float height) {
		style.width = width;
		style.height = height;
		return this;
	}

	public GameGrid<T> SetScrollable(bool scrollable) {
		if (scrollable) {
			k_container.Clear();
			k_container.Add(k_scrollView);
		} else {
			k_container.Clear();
			k_container.Add(k_gridContainer);
		}
		return this;
	}

	#endregion

	#region Fluent API - Events

	public GameGrid<T> OnCellClicked(Action<T, int, int> callback) {
		k_onCellClicked = callback;
		return this;
	}

	public GameGrid<T> OnSelectionChanged(Action<IEnumerable<(T item, int x, int y)>> callback) {
		k_onSelectionChanged = callback;
		return this;
	}

	#endregion

	#region Selection

	public GameGrid<T> SelectCell(int x, int y) {
		if (k_selectionType == GridSelectionType.None) {
			return this;
		}

		if (k_selectionType == GridSelectionType.Single) {
			k_selectedCells.Clear();
		}

		k_selectedCells.Add((x, y));
		UpdateSelectionVisuals();
		NotifySelectionChanged();

		return this;
	}

	public GameGrid<T> DeselectCell(int x, int y) {
		k_selectedCells.Remove((x, y));
		UpdateSelectionVisuals();
		NotifySelectionChanged();
		return this;
	}

	public GameGrid<T> ClearSelection() {
		k_selectedCells.Clear();
		UpdateSelectionVisuals();
		NotifySelectionChanged();
		return this;
	}

	public bool IsCellSelected(int x, int y) => k_selectedCells.Contains((x, y));

	#endregion

	#region Build

	public GameGrid<T> Build() {
		ApplyTheme();
		RebuildGrid();
		return this;
	}

	#endregion

	#region Grid Management

	private void RebuildGrid() {
		k_gridContainer.Clear();

		int totalCells = k_rows > 0 
			? k_columns * k_rows 
			: Mathf.Max(k_items.Count, k_columns);

		// Calculate rows if auto
		int actualRows = k_rows > 0 
			? k_rows 
			: Mathf.CeilToInt((float)k_items.Count / k_columns);

		for (int y = 0; y < actualRows; y++) {
			for (int x = 0; x < k_columns; x++) {
				int index = y * k_columns + x;
				var cell = CreateCell(x, y, index);
				k_gridContainer.Add(cell);
			}
		}

		// Set container width
		float containerWidth = k_columns * (k_cellWidth + k_cellGap) - k_cellGap;
		k_gridContainer.style.width = containerWidth;
	}

	private VisualElement CreateCell(int x, int y, int index) {
		var cell = new VisualElement {
			name = $"cell-{x}-{y}",
			userData = (x, y)
		};

		ApplyCellStyle(cell, x, y, false);

		// Add content
		if (index < k_items.Count && k_itemTemplate != null) {
			cell.Add(k_itemTemplate(k_items[index]));
		} else if (k_emptyCellTemplate != null) {
			cell.Add(k_emptyCellTemplate(x, y));
		}

		// Register events
		cell.RegisterCallback<ClickEvent>(evt => OnCellClick(x, y));
		cell.RegisterCallback<MouseEnterEvent>(evt => OnCellMouseEnter(cell, x, y));
		cell.RegisterCallback<MouseLeaveEvent>(evt => OnCellMouseLeave(cell, x, y));

		return cell;
	}

	private void ApplyCellStyle(VisualElement cell, int x, int y, bool isHovered) {
		var theme = GameTheme.Current;
		var colors = theme.Colors;
		var borders = theme.Borders;

		cell.style.width = k_cellWidth;
		cell.style.height = k_cellHeight;
		cell.style.marginRight = k_cellGap;
		cell.style.marginBottom = k_cellGap;
		cell.style.alignItems = Align.Center;
		cell.style.justifyContent = Justify.Center;

		bool isSelected = k_selectedCells.Contains((x, y));

		// Variant styling
		if (k_variant == GridVariant.Slots) {
			cell.style.backgroundColor = colors.BackgroundSecondary;
			borders.ApplyRadius(cell.style, borders.RadiusSM);
			borders.ApplyWidth(cell.style, borders.WidthThin);
			borders.ApplyColor(cell.style, isSelected ? colors.Primary : colors.SurfaceBorder);
		} else {
			cell.style.backgroundColor = isSelected 
				? colors.SurfaceSelected 
				: (isHovered ? colors.SurfaceHover : colors.Surface);
			borders.ApplyRadius(cell.style, borders.RadiusSM);
			borders.ApplyWidth(cell.style, borders.WidthThin);
			borders.ApplyColor(cell.style, colors.SurfaceBorder);
		}
	}

	private void RefreshCell(int x, int y) {
		var cell = k_gridContainer.Q<VisualElement>($"cell-{x}-{y}");
		if (cell == null) {
			return;
		}

		cell.Clear();

		int index = y * k_columns + x;
		if (index < k_items.Count && k_itemTemplate != null) {
			cell.Add(k_itemTemplate(k_items[index]));
		} else if (k_emptyCellTemplate != null) {
			cell.Add(k_emptyCellTemplate(x, y));
		}
	}

	private void UpdateSelectionVisuals() {
		foreach (var cell in k_gridContainer.Children()) {
			if (cell.userData is (int x, int y)) {
				ApplyCellStyle(cell, x, y, false);
			}
		}
	}

	private void NotifySelectionChanged() {
		var selected = k_selectedCells.Select(pos => {
			int index = pos.y * k_columns + pos.x;
			T item = index < k_items.Count ? k_items[index] : default;
			return (item, pos.x, pos.y);
		});
		k_onSelectionChanged?.Invoke(selected);
	}

	#endregion

	#region Event Handlers

	private void OnCellClick(int x, int y) {
		int index = y * k_columns + x;
		T item = index < k_items.Count ? k_items[index] : default;

		if (k_selectionType != GridSelectionType.None) {
			if (k_selectedCells.Contains((x, y))) {
				DeselectCell(x, y);
			} else {
				SelectCell(x, y);
			}
		}

		k_onCellClicked?.Invoke(item, x, y);
	}

	private void OnCellMouseEnter(VisualElement cell, int x, int y) {
		ApplyCellStyle(cell, x, y, true);
	}

	private void OnCellMouseLeave(VisualElement cell, int x, int y) {
		ApplyCellStyle(cell, x, y, false);
	}

	#endregion

	#region Theme Application

	private void ApplyTheme() {
		var theme = GameTheme.Current;
		var colors = theme.Colors;
		var borders = theme.Borders;

		if (k_variant != GridVariant.Plain) {
			k_container.style.backgroundColor = colors.Background;
			borders.ApplyRadius(k_container.style, borders.RadiusMD);
			theme.Spacing.PanelPadding.ApplyTo(k_container.style);
		}
	}

	private void OnThemeChanged(GameTheme theme) {
		ApplyTheme();
		RebuildGrid();
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

public enum GridSelectionType {
	None,
	Single,
	Multiple
}

public enum GridVariant {
	Default,
	Slots,
	Plain
}

#endregion
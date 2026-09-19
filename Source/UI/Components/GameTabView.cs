using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Components;

#nullable disable

/// <summary>
/// A themed tab view component for organizing content into switchable sections.
/// 
/// <para>Usage:</para>
/// <code>
/// var tabs = new GameTabView()
///     .AddTab("Inventory", inventoryPanel)
///     .AddTab("Equipment", equipmentPanel)
///     .AddTab("Skills", skillsPanel)
///     .SetActiveTab(0)
///     .SetGrow()
///     .SetPadding(spacing.SM, spacing.SM)
///     .OnTabChanged(index => PlayTabSound())
///     .Build();
/// </code>
/// </summary>
public class GameTabView : VisualElement {
	#region Private Fields

	private readonly VisualElement k_tabBar;
	private readonly VisualElement k_contentContainer;
	private readonly VisualElement k_indicator;

	private readonly List<(string title, VisualElement content, VisualElement tab)> k_tabs = new();
	private int k_activeIndex = 0;
	private TabPosition k_position = TabPosition.Top;
	private TabVariant k_variant = TabVariant.Default;

	private Action<int> k_onTabChanged;

	#endregion

	#region Properties

	/// <summary>
	/// The content container that holds tab content.
	/// </summary>
	public VisualElement Content => k_contentContainer;

	/// <summary>
	/// The tab bar container.
	/// </summary>
	public VisualElement TabBar => k_tabBar;

	#endregion

	#region Constructors

	public GameTabView() {
		style.flexDirection = FlexDirection.Column;
		style.flexGrow = 1;

		k_tabBar = new VisualElement {
			style = {
				flexDirection = FlexDirection.Row,
				position = Position.Relative
			}
		};

		k_indicator = new VisualElement {
			style = {
				position = Position.Absolute,
				bottom = 0
			}
		};
		k_tabBar.Add(k_indicator);

		k_contentContainer = new VisualElement {
			style = { flexGrow = 1 }
		};

		Add(k_tabBar);
		Add(k_contentContainer);

		GameTheme.OnThemeChanged += OnThemeChanged;
	}

	#endregion

	#region Fluent API - Tabs

	public GameTabView AddTab(string title, VisualElement content) {
		int index = k_tabs.Count;

		var tab = CreateTabButton(title, index);
		k_tabBar.Insert(k_tabs.Count, tab); // Insert before indicator

		content.style.display = DisplayStyle.None;
		k_contentContainer.Add(content);

		k_tabs.Add((title, content, tab));

		if (k_tabs.Count == 1) {
			SetActiveTab(0);
		}

		return this;
	}

	public GameTabView AddTab(string title, Func<VisualElement> contentFactory) {
		// Lazy loading - content created when tab is first activated
		var placeholder = new VisualElement { userData = contentFactory };
		return AddTab(title, placeholder);
	}

	public GameTabView RemoveTab(int index) {
		if (index < 0 || index >= k_tabs.Count) {
			return this;
		}

		var (_, content, tab) = k_tabs[index];
		k_tabBar.Remove(tab);
		k_contentContainer.Remove(content);
		k_tabs.RemoveAt(index);

		if (k_activeIndex >= k_tabs.Count) {
			SetActiveTab(k_tabs.Count - 1);
		} else if (k_activeIndex == index) {
			SetActiveTab(k_activeIndex);
		}

		return this;
	}

	public GameTabView SetActiveTab(int index) {
		if (index < 0 || index >= k_tabs.Count) {
			return this;
		}

		// Deactivate current
		if (k_activeIndex >= 0 && k_activeIndex < k_tabs.Count) {
			k_tabs[k_activeIndex].content.style.display = DisplayStyle.None;
			UpdateTabStyle(k_activeIndex, false);
		}

		k_activeIndex = index;

		// Activate new
		var (_, content, _) = k_tabs[k_activeIndex];

		// Handle lazy loading
		if (content.userData is Func<VisualElement> factory) {
			content.Clear();
			content.Add(factory());
			content.userData = null;
		}

		content.style.display = DisplayStyle.Flex;
		UpdateTabStyle(k_activeIndex, true);
		UpdateIndicatorPosition();

		k_onTabChanged?.Invoke(k_activeIndex);

		// Play sound
		if (GameTheme.Current.Audio.Enabled) {
			UnityEngine.Debug.Log($"[Audio] Playing: {GameTheme.Current.Audio.TabSwitch}");
		}

		return this;
	}

	public int ActiveIndex => k_activeIndex;
	public string ActiveTitle => k_tabs.Count > 0 ? k_tabs[k_activeIndex].title : null;

	#endregion

	#region Fluent API - Appearance

	public GameTabView SetPosition(TabPosition position) {
		k_position = position;

		Clear();
		switch (position) {
			case TabPosition.Bottom:
				style.flexDirection = FlexDirection.ColumnReverse;
				k_indicator.style.bottom = StyleKeyword.Auto;
				k_indicator.style.top = 0;
				break;
			case TabPosition.Left:
				style.flexDirection = FlexDirection.Row;
				k_tabBar.style.flexDirection = FlexDirection.Column;
				break;
			case TabPosition.Right:
				style.flexDirection = FlexDirection.RowReverse;
				k_tabBar.style.flexDirection = FlexDirection.Column;
				break;
			default:
				style.flexDirection = FlexDirection.Column;
				k_indicator.style.top = StyleKeyword.Auto;
				k_indicator.style.bottom = 0;
				break;
		}
		Add(k_tabBar);
		Add(k_contentContainer);

		ApplyTheme();
		return this;
	}

	public GameTabView SetVariant(TabVariant variant) {
		k_variant = variant;
		ApplyTheme();
		return this;
	}

	public GameTabView SetSize(float width, float height) {
		style.width = width;
		style.height = height;
		return this;
	}

	/// <summary>
	/// Sets the width.
	/// </summary>
	public GameTabView SetWidth(float width) {
		style.width = width;
		return this;
	}

	/// <summary>
	/// Sets the width.
	/// </summary>
	public GameTabView SetWidth(Length width) {
		style.width = width;
		return this;
	}

	/// <summary>
	/// Sets the height.
	/// </summary>
	public GameTabView SetHeight(float height) {
		style.height = height;
		return this;
	}

	/// <summary>
	/// Sets the height.
	/// </summary>
	public GameTabView SetHeight(Length height) {
		style.height = height;
		return this;
	}

	/// <summary>
	/// Sets the minimum height.
	/// </summary>
	public GameTabView SetMinHeight(float minHeight) {
		style.minHeight = minHeight;
		return this;
	}

	/// <summary>
	/// Sets the maximum height.
	/// </summary>
	public GameTabView SetMaxHeight(float maxHeight) {
		style.maxHeight = maxHeight;
		return this;
	}

	#endregion

	#region Fluent API - Layout

	/// <summary>
	/// Sets flex grow to fill available space.
	/// </summary>
	public GameTabView SetGrow(float grow = 1f) {
		style.flexGrow = grow;
		return this;
	}

	/// <summary>
	/// Sets flex shrink.
	/// </summary>
	public GameTabView SetShrink(float shrink) {
		style.flexShrink = shrink;
		return this;
	}

	/// <summary>
	/// Sets padding on the content container (all sides).
	/// </summary>
	public GameTabView SetContentPadding(float all) {
		k_contentContainer.style.paddingTop = all;
		k_contentContainer.style.paddingRight = all;
		k_contentContainer.style.paddingBottom = all;
		k_contentContainer.style.paddingLeft = all;
		return this;
	}

	/// <summary>
	/// Sets padding on the content container (vertical, horizontal).
	/// </summary>
	public GameTabView SetContentPadding(float vertical, float horizontal) {
		k_contentContainer.style.paddingTop = vertical;
		k_contentContainer.style.paddingRight = horizontal;
		k_contentContainer.style.paddingBottom = vertical;
		k_contentContainer.style.paddingLeft = horizontal;
		return this;
	}

	/// <summary>
	/// Sets padding on the content container (top, right, bottom, left).
	/// </summary>
	public GameTabView SetContentPadding(float top, float right, float bottom, float left) {
		k_contentContainer.style.paddingTop = top;
		k_contentContainer.style.paddingRight = right;
		k_contentContainer.style.paddingBottom = bottom;
		k_contentContainer.style.paddingLeft = left;
		return this;
	}

	/// <summary>
	/// Sets padding on the tab view element itself (all sides).
	/// </summary>
	public GameTabView SetPadding(float all) {
		style.paddingTop = all;
		style.paddingRight = all;
		style.paddingBottom = all;
		style.paddingLeft = all;
		return this;
	}

	/// <summary>
	/// Sets padding on the tab view element itself (vertical, horizontal).
	/// </summary>
	public GameTabView SetPadding(float vertical, float horizontal) {
		style.paddingTop = vertical;
		style.paddingRight = horizontal;
		style.paddingBottom = vertical;
		style.paddingLeft = horizontal;
		return this;
	}

	/// <summary>
	/// Sets padding on the tab view element itself (top, right, bottom, left).
	/// </summary>
	public GameTabView SetPadding(float top, float right, float bottom, float left) {
		style.paddingTop = top;
		style.paddingRight = right;
		style.paddingBottom = bottom;
		style.paddingLeft = left;
		return this;
	}

	/// <summary>
	/// Sets margin on all sides.
	/// </summary>
	public GameTabView SetMargin(float all) {
		style.marginTop = all;
		style.marginRight = all;
		style.marginBottom = all;
		style.marginLeft = all;
		return this;
	}

	/// <summary>
	/// Sets margin (vertical, horizontal).
	/// </summary>
	public GameTabView SetMargin(float vertical, float horizontal) {
		style.marginTop = vertical;
		style.marginRight = horizontal;
		style.marginBottom = vertical;
		style.marginLeft = horizontal;
		return this;
	}

	/// <summary>
	/// Sets margin (top, right, bottom, left).
	/// </summary>
	public GameTabView SetMargin(float top, float right, float bottom, float left) {
		style.marginTop = top;
		style.marginRight = right;
		style.marginBottom = bottom;
		style.marginLeft = left;
		return this;
	}

	#endregion

	#region Fluent API - Events

	public GameTabView OnTabChanged(Action<int> callback) {
		k_onTabChanged = callback;
		return this;
	}

	#endregion

	#region Build

	public GameTabView Build() {
		ApplyTheme();
		if (k_tabs.Count > 0) {
			SetActiveTab(0);
		}
		return this;
	}

	#endregion

	#region Tab Management

	private VisualElement CreateTabButton(string title, int index) {
		var tab = new VisualElement {
			style = {
				flexDirection = FlexDirection.Row,
				alignItems = Align.Center,
				justifyContent = Justify.Center
			}
		};

		var label = new Label(title);
		tab.Add(label);

		tab.RegisterCallback<ClickEvent>(evt => SetActiveTab(index));
		tab.RegisterCallback<MouseEnterEvent>(evt => OnTabMouseEnter(tab, index));
		tab.RegisterCallback<MouseLeaveEvent>(evt => OnTabMouseLeave(tab, index));

		return tab;
	}

	private void UpdateTabStyle(int index, bool isActive) {
		if (index < 0 || index >= k_tabs.Count) {
			return;
		}

		var tab = k_tabs[index].tab;
		var theme = GameTheme.Current;
		var colors = theme.Colors;
		var typography = theme.Typography;
		var tabStyles = theme.Components.Tab;

		tab.style.height = tabStyles.Height;
		tab.style.minWidth = tabStyles.MinWidth;
		tab.style.paddingLeft = theme.Spacing.MD;
		tab.style.paddingRight = theme.Spacing.MD;

		var label = tab.Q<Label>();
		if (label != null) {
			typography.LabelMedium.ApplyTo(label.style);
			label.style.color = isActive ? colors.Primary : colors.TextSecondary;
		}

		if (k_variant == TabVariant.Filled && isActive) {
			tab.style.backgroundColor = colors.Surface;
		} else {
			tab.style.backgroundColor = colors.Transparent;
		}
	}

	private void UpdateIndicatorPosition() {
		if (k_tabs.Count == 0) {
			return;
		}

		var theme = GameTheme.Current;
		var colors = theme.Colors;
		var tabStyles = theme.Components.Tab;

		k_indicator.style.height = tabStyles.IndicatorHeight;
		k_indicator.style.backgroundColor = colors.Primary;

		// Calculate position based on active tab
		float offset = 0;
		float width = 0;

		for (int i = 0; i < k_tabs.Count; i++) {
			var tabWidth = k_tabs[i].tab.resolvedStyle.width;
			if (i < k_activeIndex) {
				offset += tabWidth;
			} else if (i == k_activeIndex) {
				width = tabWidth;
			}
		}

		k_indicator.style.left = offset;
		k_indicator.style.width = width > 0 ? width : tabStyles.MinWidth;
	}

	private void OnTabMouseEnter(VisualElement tab, int index) {
		if (index == k_activeIndex) {
			return;
		}

		var colors = GameTheme.Current.Colors;
		tab.style.backgroundColor = ModifyAlpha(colors.Primary, 0.1f);
	}

	private void OnTabMouseLeave(VisualElement tab, int index) {
		UpdateTabStyle(index, index == k_activeIndex);
	}

	private static Color ModifyAlpha(Color color, float alpha) {
		return new Color(color.r, color.g, color.b, alpha);
	}

	#endregion

	#region Theme Application

	private void ApplyTheme() {
		var theme = GameTheme.Current;
		var colors = theme.Colors;
		var borders = theme.Borders;

		// Tab bar styling
		k_tabBar.style.backgroundColor = k_variant == TabVariant.Filled 
			? colors.BackgroundSecondary 
			: colors.Transparent;

		if (k_position == TabPosition.Top || k_position == TabPosition.Bottom) {
			k_tabBar.style.borderBottomWidth = borders.WidthThin;
			k_tabBar.style.borderBottomColor = colors.SurfaceBorder;
		}

		// Update all tabs
		for (int i = 0; i < k_tabs.Count; i++) {
			UpdateTabStyle(i, i == k_activeIndex);
		}

		UpdateIndicatorPosition();
	}

	private void OnThemeChanged(GameTheme theme) => ApplyTheme();

	#endregion

	#region Cleanup

	new public void RemoveFromHierarchy() {
		GameTheme.OnThemeChanged -= OnThemeChanged;
		base.RemoveFromHierarchy();
	}

	#endregion
}

#region Enums

public enum TabPosition {
	Top,
	Bottom,
	Left,
	Right
}

public enum TabVariant {
	Default,
	Filled,
	Pills
}

#endregion
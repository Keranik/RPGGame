using RPGGame.UI.Styles;
using UnityEngine.UIElements;

namespace RPGGame.UI.Components;

/// <summary>
/// A themed scroll view component with fluent API.
/// Wraps Unity's ScrollView with consistent styling and theme support.
/// 
/// <para>Usage:</para>
/// <code>
/// var scrollView = new GameScrollView()
///     .SetDirection(ScrollDirection.Vertical)
///     .SetShowScrollbars(ScrollerVisibility.Auto)
///     .SetHeight(400)
///     .Build();
/// 
/// scrollView.AddChild(content);
/// </code>
/// </summary>
public class GameScrollView : VisualElement {
	#region Private Fields

	private readonly ScrollView k_scrollView;
	private ScrollDirection k_direction = ScrollDirection.Vertical;
	private ScrollerVisibility k_scrollbarVisibility = ScrollerVisibility.Auto;

	#endregion

	#region Properties

	/// <summary>
	/// The internal ScrollView element.
	/// </summary>
	public ScrollView ScrollView => k_scrollView;

	/// <summary>
	/// The content container inside the scroll view.
	/// </summary>
	public VisualElement Content => k_scrollView.contentContainer;

	/// <summary>
	/// The current scroll direction.
	/// </summary>
	public ScrollDirection Direction => k_direction;

	/// <summary>
	/// The current scrollbar visibility setting.
	/// </summary>
	public ScrollerVisibility ScrollbarVisibility => k_scrollbarVisibility;

	#endregion

	#region Constructor

	public GameScrollView(string? name = null) {
		if (!string.IsNullOrEmpty(name)) {
			this.name = name;
		}

		style.flexGrow = 1;

		k_scrollView = new ScrollView(ScrollViewMode.Vertical) {
			style = { flexGrow = 1 }
		};

		Add(k_scrollView);

		GameTheme.OnThemeChanged += OnThemeChanged;
	}

	#endregion

	#region Fluent API - Direction

	/// <summary>
	/// Sets the scroll direction.
	/// </summary>
	public GameScrollView SetDirection(ScrollDirection direction) {
		k_direction = direction;
		ApplyDirection();
		return this;
	}

	/// <summary>
	/// Sets to vertical scrolling only.
	/// </summary>
	public GameScrollView SetVertical() {
		return SetDirection(ScrollDirection.Vertical);
	}

	/// <summary>
	/// Sets to horizontal scrolling only.
	/// </summary>
	public GameScrollView SetHorizontal() {
		return SetDirection(ScrollDirection.Horizontal);
	}

	/// <summary>
	/// Sets to both vertical and horizontal scrolling.
	/// </summary>
	public GameScrollView SetBoth() {
		return SetDirection(ScrollDirection.Both);
	}

	private void ApplyDirection() {
		k_scrollView.mode = k_direction switch {
			ScrollDirection.Vertical => ScrollViewMode.Vertical,
			ScrollDirection.Horizontal => ScrollViewMode.Horizontal,
			ScrollDirection.Both => ScrollViewMode.VerticalAndHorizontal,
			_ => ScrollViewMode.Vertical
		};
	}

	#endregion

	#region Fluent API - Scrollbar Visibility

	/// <summary>
	/// Sets the scrollbar visibility.
	/// </summary>
	public GameScrollView SetScrollbarVisibility(ScrollerVisibility visibility) {
		k_scrollbarVisibility = visibility;
		ApplyScrollbarVisibility();
		return this;
	}

	/// <summary>
	/// Shows scrollbars only when needed.
	/// </summary>
	public GameScrollView SetScrollbarsAuto() {
		return SetScrollbarVisibility(ScrollerVisibility.Auto);
	}

	/// <summary>
	/// Always shows scrollbars.
	/// </summary>
	public GameScrollView SetScrollbarsAlwaysVisible() {
		return SetScrollbarVisibility(ScrollerVisibility.AlwaysVisible);
	}

	/// <summary>
	/// Hides scrollbars.
	/// </summary>
	public GameScrollView SetScrollbarsHidden() {
		return SetScrollbarVisibility(ScrollerVisibility.Hidden);
	}

	private void ApplyScrollbarVisibility() {
		k_scrollView.verticalScrollerVisibility = k_scrollbarVisibility;
		k_scrollView.horizontalScrollerVisibility = k_scrollbarVisibility;
	}

	#endregion

	#region Fluent API - Size

	/// <summary>
	/// Sets the width.
	/// </summary>
	public GameScrollView SetWidth(float width) {
		style.width = width;
		return this;
	}

	/// <summary>
	/// Sets the width.
	/// </summary>
	public GameScrollView SetWidth(Length width) {
		style.width = width;
		return this;
	}

	/// <summary>
	/// Sets the height.
	/// </summary>
	public GameScrollView SetHeight(float height) {
		style.height = height;
		return this;
	}

	/// <summary>
	/// Sets the height.
	/// </summary>
	public GameScrollView SetHeight(Length height) {
		style.height = height;
		return this;
	}

	/// <summary>
	/// Sets the minimum height.
	/// </summary>
	public GameScrollView SetMinHeight(float minHeight) {
		style.minHeight = minHeight;
		return this;
	}

	/// <summary>
	/// Sets the minimum height.
	/// </summary>
	public GameScrollView SetMinHeight(Length minHeight) {
		style.minHeight = minHeight;
		return this;
	}

	/// <summary>
	/// Sets the maximum height.
	/// </summary>
	public GameScrollView SetMaxHeight(float maxHeight) {
		style.maxHeight = maxHeight;
		return this;
	}

	/// <summary>
	/// Sets the maximum height.
	/// </summary>
	public GameScrollView SetMaxHeight(Length maxHeight) {
		style.maxHeight = maxHeight;
		return this;
	}

	/// <summary>
	/// Sets flex grow to fill available space.
	/// </summary>
	public GameScrollView SetGrow(float grow = 1f) {
		style.flexGrow = grow;
		return this;
	}

	#endregion

	#region Fluent API - Padding

	/// <summary>
	/// Sets padding on all sides.
	/// </summary>
	public GameScrollView SetPadding(float all) {
		k_scrollView.contentContainer.style.paddingTop = all;
		k_scrollView.contentContainer.style.paddingRight = all;
		k_scrollView.contentContainer.style.paddingBottom = all;
		k_scrollView.contentContainer.style.paddingLeft = all;
		return this;
	}

	/// <summary>
	/// Sets padding (top, right, bottom, left).
	/// </summary>
	public GameScrollView SetPadding(float top, float right, float bottom, float left) {
		k_scrollView.contentContainer.style.paddingTop = top;
		k_scrollView.contentContainer.style.paddingRight = right;
		k_scrollView.contentContainer.style.paddingBottom = bottom;
		k_scrollView.contentContainer.style.paddingLeft = left;
		return this;
	}

	/// <summary>
	/// Sets padding (vertical, horizontal).
	/// </summary>
	public GameScrollView SetPadding(float vertical, float horizontal) {
		return SetPadding(vertical, horizontal, vertical, horizontal);
	}

	#endregion

	#region Fluent API - Content

	/// <summary>
	/// Adds a child element to the scroll content.
	/// </summary>
	public GameScrollView AddChild(VisualElement child) {
		k_scrollView.Add(child);
		return this;
	}

	/// <summary>
	/// Clears all content from the scroll view.
	/// </summary>
	public GameScrollView ClearContent() {
		k_scrollView.Clear();
		return this;
	}

	#endregion

	#region Fluent API - Scroll Control

	/// <summary>
	/// Scrolls to the top.
	/// </summary>
	public GameScrollView ScrollToTop() {
		k_scrollView.scrollOffset = new UnityEngine.Vector2(k_scrollView.scrollOffset.x, 0);
		return this;
	}

	/// <summary>
	/// Scrolls to the bottom.
	/// </summary>
	public GameScrollView ScrollToBottom() {
		k_scrollView.scrollOffset = new UnityEngine.Vector2(k_scrollView.scrollOffset.x, float.MaxValue);
		return this;
	}

	/// <summary>
	/// Scrolls to a specific element.
	/// </summary>
	public GameScrollView ScrollTo(VisualElement element) {
		k_scrollView.ScrollTo(element);
		return this;
	}

	#endregion

	#region Build

	/// <summary>
	/// Builds the scroll view with current settings.
	/// </summary>
	public GameScrollView Build() {
		ApplyDirection();
		ApplyScrollbarVisibility();
		ApplyTheme();
		return this;
	}

	#endregion

	#region Theme

	private void ApplyTheme() {
		var theme = GameTheme.Current;
		var scrollViewStyles = theme.Components.ScrollView;

		// Only style scrollbars based on direction
		if (k_direction is ScrollDirection.Vertical or ScrollDirection.Both) {
			ApplyVerticalScrollerTheme(scrollViewStyles, theme);
		}

		if (k_direction is ScrollDirection.Horizontal or ScrollDirection.Both) {
			ApplyHorizontalScrollerTheme(scrollViewStyles, theme);
		}
	}

	private void ApplyVerticalScrollerTheme(ScrollViewStyles scrollViewStyles, GameTheme theme) {
		var verticalScroller = k_scrollView.verticalScroller;
		if (verticalScroller == null) return;

		verticalScroller.style.width = scrollViewStyles.ScrollbarWidth;

		var slider = verticalScroller.slider;
		if (slider != null) {
			var dragger = slider.Q("unity-dragger");
			if (dragger != null) {
				dragger.style.backgroundColor = theme.Colors.SurfaceBorder;
				theme.Borders.ApplyRadius(dragger.style, theme.Borders.RadiusSM);
			}
		}

		var tracker = verticalScroller.Q("unity-tracker");
		if (tracker != null) {
			tracker.style.backgroundColor = theme.Colors.BackgroundSecondary;
		}
	}

	private void ApplyHorizontalScrollerTheme(ScrollViewStyles scrollViewStyles, GameTheme theme) {
		var horizontalScroller = k_scrollView.horizontalScroller;
		if (horizontalScroller == null) return;

		horizontalScroller.style.height = scrollViewStyles.ScrollbarWidth;

		var slider = horizontalScroller.slider;
		if (slider != null) {
			var dragger = slider.Q("unity-dragger");
			if (dragger != null) {
				dragger.style.backgroundColor = theme.Colors.SurfaceBorder;
				theme.Borders.ApplyRadius(dragger.style, theme.Borders.RadiusSM);
			}
		}

		var tracker = horizontalScroller.Q("unity-tracker");
		if (tracker != null) {
			tracker.style.backgroundColor = theme.Colors.BackgroundSecondary;
		}
	}

	private void OnThemeChanged(GameTheme theme) {
		ApplyTheme();
	}

	#endregion

	#region Cleanup

	public new void RemoveFromHierarchy() {
		GameTheme.OnThemeChanged -= OnThemeChanged;
		base.RemoveFromHierarchy();
	}

	#endregion
}

#region Enums

/// <summary>
/// Scroll direction for GameScrollView.
/// </summary>
public enum ScrollDirection {
	Vertical,
	Horizontal,
	Both
}

#endregion
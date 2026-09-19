using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Components;

/// <summary>
/// Panel layout direction.
/// </summary>
public enum LayoutDirection {
	Vertical,
	VerticalReverse,
	Horizontal,
	HorizontalReverse
}

/// <summary>
/// Panel visual variants.
/// </summary>
public enum PanelVariant {
	Default,
	Elevated,
	Outlined,
	Filled,
	Ghost,
	Card
}

/// <summary>
/// A themed panel/container component for grouping UI elements.
/// Supports various styles, layouts, and optional headers.
/// 
/// <para>Usage:</para>
/// <code>
/// var panel = new GamePanel()
///     .SetVariant(PanelVariant.Elevated)
///     .SetLayout(LayoutDirection.Vertical)
///     .SetHeader("Settings")
///     .SetWidth(300)
///     .SetPadding(12)
///     .SetPickingMode(PickingMode.Position)
///     .AddChild(new GameLabel("Option 1"))
///     .AddChild(new GameButton("Save").Build())
///     .Build();
/// </code>
/// </summary>
public class GamePanel : VisualElement {
	#region Private Fields

	private readonly VisualElement k_headerContainer;
	private readonly Label k_headerLabel;
	private readonly VisualElement k_contentContainer;

	private PanelVariant k_variant = PanelVariant.Default;
	private bool k_hasHeader = false;

	#endregion

	#region Constructors

	public GamePanel() {
		// Header container (hidden by default)
		k_headerContainer = new VisualElement {
			style = {
				display = DisplayStyle.None,
				flexDirection = FlexDirection.Row,
				alignItems = Align.Center,
				justifyContent = Justify.SpaceBetween
			}
		};

		k_headerLabel = new Label {
			style = { unityTextAlign = TextAnchor.MiddleLeft }
		};
		k_headerContainer.Add(k_headerLabel);

		// Content container
		k_contentContainer = new VisualElement {
			style = {
				flexDirection = FlexDirection.Column,
				flexGrow = 1
			}
		};

		Add(k_headerContainer);
		Add(k_contentContainer);

		// Subscribe to theme changes
		GameTheme.OnThemeChanged += OnThemeChanged;
	}

	#endregion

	#region Fluent API - Content

	public GamePanel SetHeader(string headerText) {
		k_hasHeader = !string.IsNullOrEmpty(headerText);
		k_headerLabel.text = headerText;
		k_headerContainer.style.display = k_hasHeader ? DisplayStyle.Flex : DisplayStyle.None;
		ApplyTheme();
		return this;
	}

	public GamePanel AddChild(VisualElement child) {
		k_contentContainer.Add(child);
		return this;
	}

	public GamePanel AddChildren(params VisualElement[] children) {
		foreach (var child in children) {
			k_contentContainer.Add(child);
		}
		return this;
	}

	public GamePanel ClearChildren() {
		k_contentContainer.Clear();
		return this;
	}

	/// <summary>
	/// Gets the content container for direct manipulation.
	/// </summary>
	public VisualElement Content => k_contentContainer;

	#endregion

	#region Fluent API - Appearance

	public GamePanel SetVariant(PanelVariant variant) {
		k_variant = variant;
		ApplyTheme();
		return this;
	}

	public GamePanel SetLayout(LayoutDirection direction) {
		k_contentContainer.style.flexDirection = direction switch {
			LayoutDirection.Horizontal => FlexDirection.Row,
			LayoutDirection.HorizontalReverse => FlexDirection.RowReverse,
			LayoutDirection.VerticalReverse => FlexDirection.ColumnReverse,
			_ => FlexDirection.Column
		};
		return this;
	}

	public GamePanel SetAlignment(Align align) {
		k_contentContainer.style.alignItems = align;
		return this;
	}

	public GamePanel SetJustify(Justify justify) {
		k_contentContainer.style.justifyContent = justify;
		return this;
	}

	public GamePanel SetGap(int gap) {
		// Apply gap between children using margins
		k_contentContainer.Query<VisualElement>().ForEach(child => {
			if (k_contentContainer.style.flexDirection == FlexDirection.Row ||
			    k_contentContainer.style.flexDirection == FlexDirection.RowReverse) {
				child.style.marginRight = gap;
			} else {
				child.style.marginBottom = gap;
			}
		});
		return this;
	}

	public GamePanel SetWrap(bool wrap) {
		k_contentContainer.style.flexWrap = wrap ? Wrap.Wrap : Wrap.NoWrap;
		return this;
	}

	public GamePanel SetFlexWrap(Wrap wrap) {
		k_contentContainer.style.flexWrap = wrap;
		return this;
	}

	#endregion

	#region Fluent API - Sizing

	public GamePanel SetSize(float width, float height) {
		style.width = width;
		style.height = height;
		return this;
	}

	public GamePanel SetSize(Length width, Length height) {
		style.width = width;
		style.height = height;
		return this;
	}

	public GamePanel SetWidth(float width) {
		style.width = width;
		return this;
	}

	public GamePanel SetWidth(Length width) {
		style.width = width;
		return this;
	}

	public GamePanel SetHeight(float height) {
		style.height = height;
		return this;
	}

	public GamePanel SetHeight(Length height) {
		style.height = height;
		return this;
	}

	public GamePanel SetMinWidth(float minWidth) {
		style.minWidth = minWidth;
		return this;
	}

	public GamePanel SetMinWidth(Length minWidth) {
		style.minWidth = minWidth;
		return this;
	}

	public GamePanel SetMinHeight(float minHeight) {
		style.minHeight = minHeight;
		return this;
	}

	public GamePanel SetMinHeight(Length minHeight) {
		style.minHeight = minHeight;
		return this;
	}

	public GamePanel SetMaxWidth(float maxWidth) {
		style.maxWidth = maxWidth;
		return this;
	}

	public GamePanel SetMaxWidth(Length maxWidth) {
		style.maxWidth = maxWidth;
		return this;
	}

	public GamePanel SetMaxHeight(float maxHeight) {
		style.maxHeight = maxHeight;
		return this;
	}

	public GamePanel SetMaxHeight(Length maxHeight) {
		style.maxHeight = maxHeight;
		return this;
	}

	public GamePanel SetMinSize(float minWidth, float minHeight) {
		style.minWidth = minWidth;
		style.minHeight = minHeight;
		return this;
	}

	public GamePanel SetMaxSize(float maxWidth, float maxHeight) {
		style.maxWidth = maxWidth;
		style.maxHeight = maxHeight;
		return this;
	}

	public GamePanel SetFlexGrow(float grow) {
		style.flexGrow = grow;
		return this;
	}

	public GamePanel SetFlexShrink(float shrink) {
		style.flexShrink = shrink;
		return this;
	}

	#endregion

	#region Fluent API - Positioning

	public GamePanel SetPosition(Position position) {
		style.position = position;
		return this;
	}

	public GamePanel SetAbsolute(float? top = null, float? right = null, float? bottom = null, float? left = null) {
		style.position = Position.Absolute;
		if (top.HasValue) style.top = top.Value;
		if (right.HasValue) style.right = right.Value;
		if (bottom.HasValue) style.bottom = bottom.Value;
		if (left.HasValue) style.left = left.Value;
		return this;
	}

	public GamePanel SetAbsoluteFill() {
		style.position = Position.Absolute;
		style.top = 0;
		style.right = 0;
		style.bottom = 0;
		style.left = 0;
		return this;
	}

	#endregion

	#region Fluent API - Interaction

	public GamePanel SetPickingMode(PickingMode mode) {
		pickingMode = mode;
		return this;
	}

	public GamePanel SetVisible(bool visible) {
		style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
		return this;
	}

	public GamePanel SetOpacity(float opacity) {
		style.opacity = opacity;
		return this;
	}

	#endregion

	#region Fluent API - Custom Styling

	public GamePanel SetBackgroundColor(Color color) {
		style.backgroundColor = color;
		return this;
	}

	public GamePanel SetBorderColor(Color color) {
		GameTheme.Current.Borders.ApplyColor(style, color);
		return this;
	}

	public GamePanel SetBorderRadius(float radius) {
		GameTheme.Current.Borders.ApplyRadius(style, radius);
		return this;
	}

	public GamePanel SetBorderRadius(float topLeft, float topRight, float bottomRight, float bottomLeft) {
		style.borderTopLeftRadius = topLeft;
		style.borderTopRightRadius = topRight;
		style.borderBottomRightRadius = bottomRight;
		style.borderBottomLeftRadius = bottomLeft;
		return this;
	}

	public GamePanel SetBorderWidth(float width) {
		GameTheme.Current.Borders.ApplyWidth(style, width);
		return this;
	}

	public GamePanel SetPadding(int all) {
		style.paddingTop = all;
		style.paddingBottom = all;
		style.paddingLeft = all;
		style.paddingRight = all;
		return this;
	}

	public GamePanel SetPadding(int vertical, int horizontal) {
		style.paddingTop = vertical;
		style.paddingBottom = vertical;
		style.paddingLeft = horizontal;
		style.paddingRight = horizontal;
		return this;
	}

	public GamePanel SetPadding(int top, int right, int bottom, int left) {
		style.paddingTop = top;
		style.paddingRight = right;
		style.paddingBottom = bottom;
		style.paddingLeft = left;
		return this;
	}

	public GamePanel SetPaddingTop(int top) {
		style.paddingTop = top;
		return this;
	}

	public GamePanel SetPaddingRight(int right) {
		style.paddingRight = right;
		return this;
	}

	public GamePanel SetPaddingBottom(int bottom) {
		style.paddingBottom = bottom;
		return this;
	}

	public GamePanel SetPaddingLeft(int left) {
		style.paddingLeft = left;
		return this;
	}

	public GamePanel SetMargin(int all) {
		style.marginTop = all;
		style.marginBottom = all;
		style.marginLeft = all;
		style.marginRight = all;
		return this;
	}

	public GamePanel SetMargin(int vertical, int horizontal) {
		style.marginTop = vertical;
		style.marginBottom = vertical;
		style.marginLeft = horizontal;
		style.marginRight = horizontal;
		return this;
	}

	public GamePanel SetMargin(int top, int right, int bottom, int left) {
		style.marginTop = top;
		style.marginRight = right;
		style.marginBottom = bottom;
		style.marginLeft = left;
		return this;
	}

	public GamePanel SetMarginTop(int top) {
		style.marginTop = top;
		return this;
	}

	public GamePanel SetMarginRight(int right) {
		style.marginRight = right;
		return this;
	}

	public GamePanel SetMarginBottom(int bottom) {
		style.marginBottom = bottom;
		return this;
	}

	public GamePanel SetMarginLeft(int left) {
		style.marginLeft = left;
		return this;
	}

	public GamePanel SetOverflow(Overflow overflow) {
		style.overflow = overflow;
		return this;
	}

	#endregion

	#region Build

	public GamePanel Build() {
		ApplyTheme();
		return this;
	}

	#endregion

	#region Theme Application

	private void ApplyTheme() {
		var theme = GameTheme.Current;
		var colors = theme.Colors;
		var borders = theme.Borders;
		var spacing = theme.Spacing;

		// Apply variant-specific styling
		var (bgColor, borderColor, borderWidth, borderRadius) = k_variant switch {
			PanelVariant.Elevated => (colors.BackgroundElevated, colors.SurfaceBorder, borders.WidthThin, borders.CardRadius),
			PanelVariant.Outlined => (colors.Transparent, colors.SurfaceBorder, borders.WidthMedium, borders.CardRadius),
			PanelVariant.Filled => (colors.Surface, colors.Transparent, 0f, borders.CardRadius),
			PanelVariant.Ghost => (colors.Transparent, colors.Transparent, 0f, 0f),
			PanelVariant.Card => (colors.Surface, colors.SurfaceBorder, borders.WidthThin, borders.CardRadius),
			_ => (colors.Surface, colors.SurfaceBorder, borders.WidthThin, borders.RadiusMD)
		};

		style.backgroundColor = bgColor;
		borders.ApplyColor(style, borderColor);
		borders.ApplyWidth(style, borderWidth);
		borders.ApplyRadius(style, borderRadius);

		// Apply default padding based on variant
		if (k_variant != PanelVariant.Ghost) {
			spacing.PanelPadding.ApplyTo(style);
		}

		// Apply header styling
		if (k_hasHeader) {
			k_headerContainer.style.paddingBottom = spacing.SM;
			k_headerContainer.style.marginBottom = spacing.SM;
			k_headerContainer.style.borderBottomWidth = borders.WidthThin;
			k_headerContainer.style.borderBottomColor = colors.SurfaceBorder;

			theme.Typography.TitleMedium.ApplyTo(k_headerLabel.style);
			k_headerLabel.style.color = colors.TextPrimary;
		}
	}

	private void OnThemeChanged(GameTheme theme) {
		ApplyTheme();
	}

	#endregion

	#region Cleanup

	new public void RemoveFromHierarchy() {
		GameTheme.OnThemeChanged -= OnThemeChanged;
		base.RemoveFromHierarchy();
	}

	#endregion
}
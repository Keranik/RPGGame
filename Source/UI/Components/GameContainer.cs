using System;
using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.UIElements.Cursor;

namespace RPGGame.UI.Components;

/// <summary>
/// A simple themed container with full fluent API support for layout and styling.
/// Use this instead of raw VisualElement when you need fluent builders.
/// </summary>
public class GameContainer : VisualElement {
	#region Constructors

	public GameContainer() {
		GameTheme.OnThemeChanged += OnThemeChanged;
	}

	public GameContainer(string name) : this() {
		this.name = name;
	}

	#endregion

	#region Fluent API - Layout Direction

	public GameContainer SetFlexDirection(FlexDirection direction) {
		style.flexDirection = direction;
		return this;
	}

	public GameContainer SetRow() {
		style.flexDirection = FlexDirection.Row;
		return this;
	}

	public GameContainer SetColumn() {
		style.flexDirection = FlexDirection.Column;
		return this;
	}

	public GameContainer SetRowReverse() {
		style.flexDirection = FlexDirection.RowReverse;
		return this;
	}

	public GameContainer SetColumnReverse() {
		style.flexDirection = FlexDirection.ColumnReverse;
		return this;
	}

	#endregion

	#region Fluent API - Alignment

	public GameContainer SetAlignItems(Align align) {
		style.alignItems = align;
		return this;
	}

	public GameContainer SetAlignSelf(Align align) {
		style.alignSelf = align;
		return this;
	}

	public GameContainer SetAlignContent(Align align) {
		style.alignContent = align;
		return this;
	}

	public GameContainer SetJustifyContent(Justify justify) {
		style.justifyContent = justify;
		return this;
	}

	public GameContainer SetFlexWrap(Wrap wrap) {
		style.flexWrap = wrap;
		return this;
	}

	/// <summary>Center items both horizontally and vertically.</summary>
	public GameContainer SetCenter() {
		style.alignItems = Align.Center;
		style.justifyContent = Justify.Center;
		return this;
	}

	/// <summary>Space items evenly with space between.</summary>
	public GameContainer SetSpaceBetween() {
		style.justifyContent = Justify.SpaceBetween;
		return this;
	}

	/// <summary>Space items evenly with space around.</summary>
	public GameContainer SetSpaceAround() {
		style.justifyContent = Justify.SpaceAround;
		return this;
	}

	#endregion

	#region Fluent API - Sizing

	public GameContainer SetWidth(float width) {
		style.width = width;
		return this;
	}

	public GameContainer SetWidth(Length width) {
		style.width = width;
		return this;
	}

	public GameContainer SetHeight(float height) {
		style.height = height;
		return this;
	}

	public GameContainer SetHeight(Length height) {
		style.height = height;
		return this;
	}

	public GameContainer SetSize(float width, float height) {
		style.width = width;
		style.height = height;
		return this;
	}

	public GameContainer SetSize(Length width, Length height) {
		style.width = width;
		style.height = height;
		return this;
	}

	public GameContainer SetMinWidth(float minWidth) {
		style.minWidth = minWidth;
		return this;
	}

	public GameContainer SetMinHeight(float minHeight) {
		style.minHeight = minHeight;
		return this;
	}

	public GameContainer SetMaxWidth(float maxWidth) {
		style.maxWidth = maxWidth;
		return this;
	}

	public GameContainer SetMaxHeight(float maxHeight) {
		style.maxHeight = maxHeight;
		return this;
	}

	public GameContainer SetFullWidth() {
		style.width = Length.Percent(100);
		return this;
	}

	public GameContainer SetFullHeight() {
		style.height = Length.Percent(100);
		return this;
	}

	public GameContainer SetFullSize() {
		style.width = Length.Percent(100);
		style.height = Length.Percent(100);
		return this;
	}

	#endregion

	#region Fluent API - Flex

	public GameContainer SetFlexGrow(float grow) {
		style.flexGrow = grow;
		return this;
	}

	public GameContainer SetFlexShrink(float shrink) {
		style.flexShrink = shrink;
		return this;
	}

	public GameContainer SetFlexBasis(Length basis) {
		style.flexBasis = basis;
		return this;
	}

	/// <summary>Sets flex-grow to 1 to fill available space.</summary>
	public GameContainer SetGrow() {
		style.flexGrow = 1;
		return this;
	}

	#endregion

	#region Fluent API - Position

	public GameContainer SetTop(Length top) {
		style.top = top;
		return this;
	}

	public GameContainer SetRight(Length right) {
		style.right = right;
		return this;
	}

	public GameContainer SetBottom(Length bottom) {
		style.bottom = bottom;
		return this;
	}

	public GameContainer SetLeft(Length left) {
		style.left = left;
		return this;
	}

	public GameContainer SetPosition(Position position) {
		style.position = position;
		return this;
	}

	public GameContainer SetAbsolute() {
		style.position = Position.Absolute;
		return this;
	}

	public GameContainer SetRelative() {
		style.position = Position.Relative;
		return this;
	}

	public GameContainer SetTop(float top) {
		style.top = top;
		return this;
	}

	public GameContainer SetRight(float right) {
		style.right = right;
		return this;
	}

	public GameContainer SetBottom(float bottom) {
		style.bottom = bottom;
		return this;
	}

	public GameContainer SetLeft(float left) {
		style.left = left;
		return this;
	}

	/// <summary>Fills the parent container absolutely.</summary>
	public GameContainer SetAbsoluteFill() {
		style.position = Position.Absolute;
		style.top = 0;
		style.right = 0;
		style.bottom = 0;
		style.left = 0;
		return this;
	}

	#endregion

	#region Fluent API - Margin

	public GameContainer SetMargin(int all) {
		var scaled = GameTheme.Current.Spacing.GetScaled(all);
		style.marginTop = scaled;
		style.marginBottom = scaled;
		style.marginLeft = scaled;
		style.marginRight = scaled;
		return this;
	}

	public GameContainer SetMargin(int vertical, int horizontal) {
		var spacing = GameTheme.Current.Spacing;
		style.marginTop = spacing.GetScaled(vertical);
		style.marginBottom = spacing.GetScaled(vertical);
		style.marginLeft = spacing.GetScaled(horizontal);
		style.marginRight = spacing.GetScaled(horizontal);
		return this;
	}

	public GameContainer SetMargin(int top, int right, int bottom, int left) {
		var spacing = GameTheme.Current.Spacing;
		style.marginTop = spacing.GetScaled(top);
		style.marginRight = spacing.GetScaled(right);
		style.marginBottom = spacing.GetScaled(bottom);
		style.marginLeft = spacing.GetScaled(left);
		return this;
	}

	public GameContainer SetMarginTop(int top) {
		style.marginTop = GameTheme.Current.Spacing.GetScaled(top);
		return this;
	}

	public GameContainer SetMarginRight(int right) {
		style.marginRight = GameTheme.Current.Spacing.GetScaled(right);
		return this;
	}

	public GameContainer SetMarginBottom(int bottom) {
		style.marginBottom = GameTheme.Current.Spacing.GetScaled(bottom);
		return this;
	}

	public GameContainer SetMarginLeft(int left) {
		style.marginLeft = GameTheme.Current.Spacing.GetScaled(left);
		return this;
	}

	public GameContainer SetMargin(MarginPreset preset) {
		preset.ApplyTo(style);
		return this;
	}

	#endregion

	#region Fluent API - Padding

	public GameContainer SetPadding(int all) {
		var scaled = GameTheme.Current.Spacing.GetScaled(all);
		style.paddingTop = scaled;
		style.paddingBottom = scaled;
		style.paddingLeft = scaled;
		style.paddingRight = scaled;
		return this;
	}

	public GameContainer SetPadding(int vertical, int horizontal) {
		var spacing = GameTheme.Current.Spacing;
		style.paddingTop = spacing.GetScaled(vertical);
		style.paddingBottom = spacing.GetScaled(vertical);
		style.paddingLeft = spacing.GetScaled(horizontal);
		style.paddingRight = spacing.GetScaled(horizontal);
		return this;
	}

	public GameContainer SetPadding(int top, int right, int bottom, int left) {
		var spacing = GameTheme.Current.Spacing;
		style.paddingTop = spacing.GetScaled(top);
		style.paddingRight = spacing.GetScaled(right);
		style.paddingBottom = spacing.GetScaled(bottom);
		style.paddingLeft = spacing.GetScaled(left);
		return this;
	}

	public GameContainer SetPaddingTop(int top) {
		style.paddingTop = GameTheme.Current.Spacing.GetScaled(top);
		return this;
	}

	public GameContainer SetPaddingRight(int right) {
		style.paddingRight = GameTheme.Current.Spacing.GetScaled(right);
		return this;
	}

	public GameContainer SetPaddingBottom(int bottom) {
		style.paddingBottom = GameTheme.Current.Spacing.GetScaled(bottom);
		return this;
	}

	public GameContainer SetPaddingLeft(int left) {
		style.paddingLeft = GameTheme.Current.Spacing.GetScaled(left);
		return this;
	}

	public GameContainer SetPadding(PaddingPreset preset) {
		preset.ApplyTo(style);
		return this;
	}

	#endregion

	#region Fluent API - Background

	public GameContainer SetBackgroundColor(Color color) {
		style.backgroundColor = color;
		return this;
	}

	public GameContainer SetBackground(Func<ColorPalette, Color> selector) {
		style.backgroundColor = selector(GameTheme.Current.Colors);
		return this;
	}

	public GameContainer SetBackgroundImage(Texture2D image) {
		style.backgroundImage = new StyleBackground(image);
		return this;
	}

	public GameContainer SetBackgroundImage(Sprite sprite) {
		style.backgroundImage = new StyleBackground(sprite);
		return this;
	}

	#endregion

	#region Fluent API - Border Width

	/// <summary>Sets all border widths to the same value.</summary>
	public GameContainer SetBorderWidth(float width) {
		style.borderTopWidth = width;
		style.borderRightWidth = width;
		style.borderBottomWidth = width;
		style.borderLeftWidth = width;
		return this;
	}

	public GameContainer SetBorderTopWidth(float width) {
		style.borderTopWidth = width;
		return this;
	}

	public GameContainer SetBorderRightWidth(float width) {
		style.borderRightWidth = width;
		return this;
	}

	public GameContainer SetBorderBottomWidth(float width) {
		style.borderBottomWidth = width;
		return this;
	}

	public GameContainer SetBorderLeftWidth(float width) {
		style.borderLeftWidth = width;
		return this;
	}

	public GameContainer SetBorderWidth(float top, float right, float bottom, float left) {
		style.borderTopWidth = top;
		style.borderRightWidth = right;
		style.borderBottomWidth = bottom;
		style.borderLeftWidth = left;
		return this;
	}

	/// <summary>Sets only bottom border width (useful for underline effects).</summary>
	public GameContainer SetBorderBottomOnly(float width) {
		style.borderTopWidth = 0;
		style.borderRightWidth = 0;
		style.borderBottomWidth = width;
		style.borderLeftWidth = 0;
		return this;
	}

	/// <summary>Sets only top border width.</summary>
	public GameContainer SetBorderTopOnly(float width) {
		style.borderTopWidth = width;
		style.borderRightWidth = 0;
		style.borderBottomWidth = 0;
		style.borderLeftWidth = 0;
		return this;
	}

	/// <summary>Sets only left border width.</summary>
	public GameContainer SetBorderLeftOnly(float width) {
		style.borderTopWidth = 0;
		style.borderRightWidth = 0;
		style.borderBottomWidth = 0;
		style.borderLeftWidth = width;
		return this;
	}

	/// <summary>Sets only right border width.</summary>
	public GameContainer SetBorderRightOnly(float width) {
		style.borderTopWidth = 0;
		style.borderRightWidth = width;
		style.borderBottomWidth = 0;
		style.borderLeftWidth = 0;
		return this;
	}

	#endregion

	#region Fluent API - Border Color

	/// <summary>Sets all border colors to the same value.</summary>
	public GameContainer SetBorderColor(Color color) {
		style.borderTopColor = color;
		style.borderRightColor = color;
		style.borderBottomColor = color;
		style.borderLeftColor = color;
		return this;
	}

	public GameContainer SetBorderColor(Func<ColorPalette, Color> selector) {
		var color = selector(GameTheme.Current.Colors);
		style.borderTopColor = color;
		style.borderRightColor = color;
		style.borderBottomColor = color;
		style.borderLeftColor = color;
		return this;
	}

	public GameContainer SetBorderTopColor(Color color) {
		style.borderTopColor = color;
		return this;
	}

	public GameContainer SetBorderRightColor(Color color) {
		style.borderRightColor = color;
		return this;
	}

	public GameContainer SetBorderBottomColor(Color color) {
		style.borderBottomColor = color;
		return this;
	}

	public GameContainer SetBorderLeftColor(Color color) {
		style.borderLeftColor = color;
		return this;
	}

	public GameContainer SetBorderColor(Color top, Color right, Color bottom, Color left) {
		style.borderTopColor = top;
		style.borderRightColor = right;
		style.borderBottomColor = bottom;
		style.borderLeftColor = left;
		return this;
	}

	#endregion

	#region Fluent API - Border Radius

	/// <summary>Sets all border radii to the same value.</summary>
	public GameContainer SetBorderRadius(float radius) {
		style.borderTopLeftRadius = radius;
		style.borderTopRightRadius = radius;
		style.borderBottomRightRadius = radius;
		style.borderBottomLeftRadius = radius;
		return this;
	}

	public GameContainer SetBorderTopLeftRadius(float radius) {
		style.borderTopLeftRadius = radius;
		return this;
	}

	public GameContainer SetBorderTopRightRadius(float radius) {
		style.borderTopRightRadius = radius;
		return this;
	}

	public GameContainer SetBorderBottomRightRadius(float radius) {
		style.borderBottomRightRadius = radius;
		return this;
	}

	public GameContainer SetBorderBottomLeftRadius(float radius) {
		style.borderBottomLeftRadius = radius;
		return this;
	}

	public GameContainer SetBorderRadius(float topLeft, float topRight, float bottomRight, float bottomLeft) {
		style.borderTopLeftRadius = topLeft;
		style.borderTopRightRadius = topRight;
		style.borderBottomRightRadius = bottomRight;
		style.borderBottomLeftRadius = bottomLeft;
		return this;
	}

	/// <summary>Sets top corners only (for top-rounded elements).</summary>
	public GameContainer SetBorderRadiusTop(float radius) {
		style.borderTopLeftRadius = radius;
		style.borderTopRightRadius = radius;
		style.borderBottomRightRadius = 0;
		style.borderBottomLeftRadius = 0;
		return this;
	}

	/// <summary>Sets bottom corners only (for bottom-rounded elements).</summary>
	public GameContainer SetBorderRadiusBottom(float radius) {
		style.borderTopLeftRadius = 0;
		style.borderTopRightRadius = 0;
		style.borderBottomRightRadius = radius;
		style.borderBottomLeftRadius = radius;
		return this;
	}

	/// <summary>Sets left corners only.</summary>
	public GameContainer SetBorderRadiusLeft(float radius) {
		style.borderTopLeftRadius = radius;
		style.borderTopRightRadius = 0;
		style.borderBottomRightRadius = 0;
		style.borderBottomLeftRadius = radius;
		return this;
	}

	/// <summary>Sets right corners only.</summary>
	public GameContainer SetBorderRadiusRight(float radius) {
		style.borderTopLeftRadius = 0;
		style.borderTopRightRadius = radius;
		style.borderBottomRightRadius = radius;
		style.borderBottomLeftRadius = 0;
		return this;
	}

	#endregion

	#region Fluent API - Complete Border

	/// <summary>Sets complete border with width, color, and radius from theme.</summary>
	public GameContainer SetBorder(float width, Color color, float radius) {
		SetBorderWidth(width);
		SetBorderColor(color);
		SetBorderRadius(radius);
		return this;
	}

	/// <summary>Sets complete border using theme settings.</summary>
	public GameContainer SetBorderFromTheme(Func<BorderSettings, float> widthSelector, Func<ColorPalette, Color> colorSelector, Func<BorderSettings, float> radiusSelector) {
		var borders = GameTheme.Current.Borders;
		var colors = GameTheme.Current.Colors;
		SetBorderWidth(widthSelector(borders));
		SetBorderColor(colorSelector(colors));
		SetBorderRadius(radiusSelector(borders));
		return this;
	}

	/// <summary>Sets a bottom separator line.</summary>
	public GameContainer SetBottomSeparator(Color color, float width = 1f) {
		style.borderBottomWidth = width;
		style.borderBottomColor = color;
		return this;
	}

	/// <summary>Sets a bottom separator using theme colors.</summary>
	public GameContainer SetBottomSeparator(Func<ColorPalette, Color> colorSelector, float width = 1f) {
		style.borderBottomWidth = width;
		style.borderBottomColor = colorSelector(GameTheme.Current.Colors);
		return this;
	}

	/// <summary>Sets a top separator line.</summary>
	public GameContainer SetTopSeparator(Color color, float width = 1f) {
		style.borderTopWidth = width;
		style.borderTopColor = color;
		return this;
	}

	/// <summary>Sets a left separator line.</summary>
	public GameContainer SetLeftSeparator(Color color, float width = 1f) {
		style.borderLeftWidth = width;
		style.borderLeftColor = color;
		return this;
	}

	/// <summary>Sets a right separator line.</summary>
	public GameContainer SetRightSeparator(Color color, float width = 1f) {
		style.borderRightWidth = width;
		style.borderRightColor = color;
		return this;
	}

	#endregion

	#region Fluent API - Display & Visibility

	public GameContainer SetDisplay(DisplayStyle display) {
		style.display = display;
		return this;
	}

	public GameContainer SetVisible(bool visible) {
		style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
		return this;
	}

	public GameContainer SetOpacity(float opacity) {
		style.opacity = opacity;
		return this;
	}

	public GameContainer SetOverflow(Overflow overflow) {
		style.overflow = overflow;
		return this;
	}

	public GameContainer Hide() {
		style.display = DisplayStyle.None;
		return this;
	}

	public GameContainer Show() {
		style.display = DisplayStyle.Flex;
		return this;
	}

	#endregion

	#region Fluent API - Interaction

	public GameContainer SetPickingMode(PickingMode mode) {
		pickingMode = mode;
		return this;
	}

	public GameContainer SetCursor(Cursor cursor) {
		style.cursor = cursor;
		return this;
	}

	#endregion

	#region Fluent API - Events

	public GameContainer OnClick(Action callback) {
		RegisterCallback<ClickEvent>(_ => callback());
		return this;
	}

	public GameContainer OnMouseEnter(Action callback) {
		RegisterCallback<MouseEnterEvent>(_ => callback());
		return this;
	}

	public GameContainer OnMouseLeave(Action callback) {
		RegisterCallback<MouseLeaveEvent>(_ => callback());
		return this;
	}

	public GameContainer OnHover(Action onEnter, Action onLeave) {
		RegisterCallback<MouseEnterEvent>(_ => onEnter());
		RegisterCallback<MouseLeaveEvent>(_ => onLeave());
		return this;
	}

	public GameContainer OnHoverBackground(Color hoverColor, Color normalColor) {
		RegisterCallback<MouseEnterEvent>(_ => style.backgroundColor = hoverColor);
		RegisterCallback<MouseLeaveEvent>(_ => style.backgroundColor = normalColor);
		return this;
	}

	public GameContainer OnHoverBackground(Func<ColorPalette, Color> hoverSelector, Func<ColorPalette, Color>? normalSelector = null) {
		var colors = GameTheme.Current.Colors;
		var hoverColor = hoverSelector(colors);
		RegisterCallback<MouseEnterEvent>(_ => style.backgroundColor = hoverColor);
		if (normalSelector != null) {
			var normalColor = normalSelector(colors);
			RegisterCallback<MouseLeaveEvent>(_ => style.backgroundColor = normalColor);
		} else {
			RegisterCallback<MouseLeaveEvent>(_ => style.backgroundColor = StyleKeyword.Null);
		}
		return this;
	}

	#endregion

	#region Fluent API - Children

	public GameContainer AddChild(VisualElement child) {
		Add(child);
		return this;
	}

	public GameContainer AddChildren(params VisualElement[] children) {
		foreach (var child in children) {
			Add(child);
		}
		return this;
	}

	public GameContainer ClearChildren() {
		Clear();
		return this;
	}

	public GameContainer InsertChild(int index, VisualElement child) {
		Insert(index, child);
		return this;
	}

	#endregion

	#region Build

	public GameContainer Build() {
		return this;
	}

	#endregion

	#region Theme

	protected virtual void OnThemeChanged(GameTheme theme) {
		// Subclasses can override to respond to theme changes
	}

	#endregion

	#region Cleanup

	public new void RemoveFromHierarchy() {
		GameTheme.OnThemeChanged -= OnThemeChanged;
		base.RemoveFromHierarchy();
	}

	#endregion
}
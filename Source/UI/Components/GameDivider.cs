using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Components;

#nullable disable

/// <summary>
/// A themed divider/separator component for visually separating content.
/// 
/// <para>Usage:</para>
/// <code>
/// // Horizontal divider
/// var divider = new GameDivider().Build();
/// 
/// // Vertical divider with label
/// var divider = new GameDivider()
///     .SetOrientation(DividerOrientation.Vertical)
///     .SetLabel("OR")
///     .Build();
/// </code>
/// </summary>
public class GameDivider : VisualElement {
	#region Private Fields

	private readonly VisualElement k_line1;
	private readonly Label k_label;
	private readonly VisualElement k_line2;

	private DividerOrientation k_orientation = DividerOrientation.Horizontal;
	private DividerVariant k_variant = DividerVariant.Default;
	private bool k_hasLabel = false;

	#endregion

	#region Constructors

	public GameDivider() {
		style.flexDirection = FlexDirection.Row;
		style.alignItems = Align.Center;

		k_line1 = new VisualElement { style = { flexGrow = 1 } };
		k_label = new Label { style = { display = DisplayStyle.None } };
		k_line2 = new VisualElement { style = { flexGrow = 1 } };

		Add(k_line1);
		Add(k_label);
		Add(k_line2);

		GameTheme.OnThemeChanged += OnThemeChanged;
	}

	#endregion

	#region Fluent API

	public GameDivider SetOrientation(DividerOrientation orientation) {
		k_orientation = orientation;
		style.flexDirection = orientation == DividerOrientation.Horizontal 
			? FlexDirection.Row 
			: FlexDirection.Column;
		ApplyTheme();
		return this;
	}

	public GameDivider SetVariant(DividerVariant variant) {
		k_variant = variant;
		ApplyTheme();
		return this;
	}

	public GameDivider SetLabel(string text) {
		k_hasLabel = !string.IsNullOrEmpty(text);
		k_label.text = text;
		k_label.style.display = k_hasLabel ? DisplayStyle.Flex : DisplayStyle.None;
		k_line2.style.display = k_hasLabel ? DisplayStyle.Flex : DisplayStyle.None;
		ApplyTheme();
		return this;
	}

	public GameDivider SetThickness(float thickness) {
		if (k_orientation == DividerOrientation.Horizontal) {
			k_line1.style.height = thickness;
			k_line2.style.height = thickness;
		} else {
			k_line1.style.width = thickness;
			k_line2.style.width = thickness;
		}
		return this;
	}

	public GameDivider SetColor(Color color) {
		k_line1.style.backgroundColor = color;
		k_line2.style.backgroundColor = color;
		return this;
	}

	public GameDivider SetSpacing(int spacing) {
		style.marginTop = spacing;
		style.marginBottom = spacing;
		return this;
	}

	public GameDivider SetMargin(int all) {
		style.marginTop = all;
		style.marginRight = all;
		style.marginBottom = all;
		style.marginLeft = all;
		return this;
	}

	public GameDivider SetMargin(int vertical, int horizontal) {
		style.marginTop = vertical;
		style.marginBottom = vertical;
		style.marginLeft = horizontal;
		style.marginRight = horizontal;
		return this;
	}

	public GameDivider SetMargin(int top, int right, int bottom, int left) {
		style.marginTop = top;
		style.marginRight = right;
		style.marginBottom = bottom;
		style.marginLeft = left;
		return this;
	}

	public GameDivider SetMarginTop(int top) {
		style.marginTop = top;
		return this;
	}

	public GameDivider SetMarginBottom(int bottom) {
		style.marginBottom = bottom;
		return this;
	}

	public GameDivider SetMarginLeft(int left) {
		style.marginLeft = left;
		return this;
	}

	public GameDivider SetMarginRight(int right) {
		style.marginRight = right;
		return this;
	}

	#endregion

	#region Build

	public GameDivider Build() {
		ApplyTheme();
		return this;
	}

	#endregion

	#region Theme Application

	private void ApplyTheme() {
		var theme = GameTheme.Current;
		var colors = theme.Colors;
		var spacing = theme.Spacing;
		var typography = theme.Typography;
		var dividerStyles = theme.Components.Divider;

		// Line color based on variant
		Color lineColor = k_variant switch {
			DividerVariant.Light => ModifyAlpha(colors.SurfaceBorder, 0.5f),
			DividerVariant.Strong => colors.TextSecondary,
			_ => colors.SurfaceBorder
		};

		k_line1.style.backgroundColor = lineColor;
		k_line2.style.backgroundColor = lineColor;

		// Line thickness
		float thickness = dividerStyles.Thickness;
		if (k_orientation == DividerOrientation.Horizontal) {
			k_line1.style.height = thickness;
			k_line2.style.height = thickness;
			style.marginTop = dividerStyles.Spacing;
			style.marginBottom = dividerStyles.Spacing;
		} else {
			k_line1.style.width = thickness;
			k_line2.style.width = thickness;
			style.marginLeft = dividerStyles.Spacing;
			style.marginRight = dividerStyles.Spacing;
		}

		// Label styling
		if (k_hasLabel) {
			typography.Caption.ApplyTo(k_label.style);
			k_label.style.color = colors.TextSecondary;
			k_label.style.paddingLeft = spacing.SM;
			k_label.style.paddingRight = spacing.SM;
		}
	}

	private void OnThemeChanged(GameTheme theme) => ApplyTheme();

	private static Color ModifyAlpha(Color color, float alpha) {
		return new Color(color.r, color.g, color.b, alpha);
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

public enum DividerOrientation {
	Horizontal,
	Vertical
}

public enum DividerVariant {
	Default,
	Light,
	Strong
}

#endregion
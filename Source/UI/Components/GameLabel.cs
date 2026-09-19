using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Components;

/// <summary>
/// Text style presets for labels.
/// </summary>
public enum LabelStyle {
	DisplayLarge,
	DisplayMedium,
	DisplaySmall,
	HeadlineLarge,
	HeadlineMedium,
	HeadlineSmall,
	TitleLarge,
	TitleMedium,
	TitleSmall,
	BodyLarge,
	BodyMedium,
	BodySmall,
	LabelLarge,
	LabelMedium,
	LabelSmall,
	Caption,
	Overline,
	// New styles for the D&D typography system
	Button,
	Monospace,
	MonospaceSmall,
	TableHeader,
	StatValue,
	FlavorText,
	DropCap
}

/// <summary>
/// Color variants for labels.
/// </summary>
public enum LabelColor {
	Primary,
	Secondary,
	Tertiary,
	Disabled,
	Inverse,
	Link,
	Success,
	Warning,
	Error,
	Info,
	Accent
}

/// <summary>
/// A themed label component with predefined text styles and color variants.
/// Supports the D&D-style typography system with automatic text transformation.
/// </summary>
public class GameLabel : VisualElement {
	#region Private Fields

	private readonly Label k_label;
	private LabelStyle k_style = LabelStyle.BodyMedium;
	private LabelColor k_colorVariant = LabelColor.Primary;
	private bool k_useCustomColor;
	private Color k_customColor;
	private string k_rawText = string.Empty;
	private TextStyle? k_currentTextStyle;

	#endregion

	#region Constructors

	public GameLabel() : this(string.Empty) { }

	public GameLabel(string text) {
		k_rawText = text;
		k_label = new Label(text);
		Add(k_label);

		GameTheme.OnThemeChanged += OnThemeChanged;
	}

	#endregion

	#region Fluent API - Content

	/// <summary>
	/// Sets the label text. Text transformation (uppercase/lowercase) is applied
	/// automatically based on the current LabelStyle.
	/// </summary>
	public GameLabel SetText(string text) {
		k_rawText = text;
		ApplyTextWithTransform();
		return this;
	}

	/// <summary>
	/// Sets the label text without applying text transformation.
	/// Use this for pre-formatted text or when you need exact control.
	/// </summary>
	public GameLabel SetTextRaw(string text) {
		k_rawText = text;
		k_label.text = text;
		return this;
	}

	/// <summary>
	/// Gets the raw (untransformed) text.
	/// </summary>
	public string Text => k_rawText;

	/// <summary>
	/// Gets the displayed (potentially transformed) text.
	/// </summary>
	public string DisplayedText => k_label.text;

	#endregion

	#region Fluent API - Typography Style

	public GameLabel SetStyle(LabelStyle labelStyle) {
		k_style = labelStyle;
		ApplyTheme();
		return this;
	}

	public GameLabel SetFontSize(int size) {
		k_label.style.fontSize = size;
		return this;
	}

	public GameLabel SetBold(bool bold = true) {
		k_label.style.unityFontStyleAndWeight = bold ? FontStyle.Bold : FontStyle.Normal;
		return this;
	}

	public GameLabel SetItalic(bool italic = true) {
		k_label.style.unityFontStyleAndWeight = italic ? FontStyle.Italic : FontStyle.Normal;
		return this;
	}

	public GameLabel SetTextAlign(TextAnchor align) {
		k_label.style.unityTextAlign = align;
		return this;
	}

	public GameLabel SetLetterSpacing(float spacing) {
		k_label.style.letterSpacing = spacing;
		return this;
	}

	public GameLabel SetWordSpacing(float spacing) {
		k_label.style.wordSpacing = spacing;
		return this;
	}

	public GameLabel SetWhiteSpace(WhiteSpace whiteSpace) {
		k_label.style.whiteSpace = whiteSpace;
		return this;
	}

	public GameLabel SetTextOverflow(TextOverflow overflow) {
		k_label.style.textOverflow = overflow;
		return this;
	}

	public GameLabel SetMaxLines(int lines) {
		k_label.style.maxWidth = lines > 0 ? lines : StyleKeyword.None;
		return this;
	}

	#endregion

	#region Fluent API - Color

	public GameLabel SetColor(LabelColor colorVariant) {
		k_colorVariant = colorVariant;
		k_useCustomColor = false;
		ApplyTheme();
		return this;
	}

	public GameLabel SetColor(Color color) {
		k_customColor = color;
		k_useCustomColor = true;
		k_label.style.color = color;
		return this;
	}

	#endregion

	#region Fluent API - Layout & Sizing

	public GameLabel SetWidth(float width) {
		style.width = width;
		return this;
	}

	public GameLabel SetWidth(Length width) {
		style.width = width;
		return this;
	}

	public GameLabel SetHeight(float height) {
		style.height = height;
		return this;
	}

	public GameLabel SetHeight(Length height) {
		style.height = height;
		return this;
	}

	public GameLabel SetMinWidth(float minWidth) {
		style.minWidth = minWidth;
		return this;
	}

	public GameLabel SetMaxWidth(float maxWidth) {
		style.maxWidth = maxWidth;
		return this;
	}

	public GameLabel SetMinHeight(float minHeight) {
		style.minHeight = minHeight;
		return this;
	}

	public GameLabel SetMaxHeight(float maxHeight) {
		style.maxHeight = maxHeight;
		return this;
	}

	public GameLabel SetFlexGrow(float grow) {
		style.flexGrow = grow;
		return this;
	}

	public GameLabel SetFlexShrink(float shrink) {
		style.flexShrink = shrink;
		return this;
	}

	public GameLabel SetAlignSelf(Align align) {
		style.alignSelf = align;
		return this;
	}

	#endregion

	#region Fluent API - Position

	public GameLabel SetPosition(Position position) {
		style.position = position;
		return this;
	}

	public GameLabel SetAbsolute() {
		style.position = Position.Absolute;
		return this;
	}

	public GameLabel SetTop(float top) {
		style.top = top;
		return this;
	}

	public GameLabel SetTop(Length top) {
		style.top = top;
		return this;
	}

	public GameLabel SetRight(float right) {
		style.right = right;
		return this;
	}

	public GameLabel SetRight(Length right) {
		style.right = right;
		return this;
	}

	public GameLabel SetBottom(float bottom) {
		style.bottom = bottom;
		return this;
	}

	public GameLabel SetBottom(Length bottom) {
		style.bottom = bottom;
		return this;
	}

	public GameLabel SetLeft(float left) {
		style.left = left;
		return this;
	}

	public GameLabel SetLeft(Length left) {
		style.left = left;
		return this;
	}

	#endregion

	#region Fluent API - Background & Border

	public GameLabel SetBackgroundColor(Color color) {
		style.backgroundColor = color;
		return this;
	}

	public GameLabel SetBorderRadius(float radius) {
		style.borderTopLeftRadius = radius;
		style.borderTopRightRadius = radius;
		style.borderBottomLeftRadius = radius;
		style.borderBottomRightRadius = radius;
		return this;
	}

	public GameLabel SetBorderRadius(float topLeft, float topRight, float bottomRight, float bottomLeft) {
		style.borderTopLeftRadius = topLeft;
		style.borderTopRightRadius = topRight;
		style.borderBottomRightRadius = bottomRight;
		style.borderBottomLeftRadius = bottomLeft;
		return this;
	}

	#endregion

	#region Fluent API - Margin (with theme support)

	public GameLabel SetMargin(int all) {
		style.marginTop = all;
		style.marginBottom = all;
		style.marginLeft = all;
		style.marginRight = all;
		return this;
	}

	public GameLabel SetMargin(int vertical, int horizontal) {
		style.marginTop = vertical;
		style.marginBottom = vertical;
		style.marginLeft = horizontal;
		style.marginRight = horizontal;
		return this;
	}

	public GameLabel SetMargin(int top, int right, int bottom, int left) {
		style.marginTop = top;
		style.marginRight = right;
		style.marginBottom = bottom;
		style.marginLeft = left;
		return this;
	}

	public GameLabel SetMarginTop(int top) {
		style.marginTop = top;
		return this;
	}

	public GameLabel SetMarginRight(int right) {
		style.marginRight = right;
		return this;
	}

	public GameLabel SetMarginBottom(int bottom) {
		style.marginBottom = bottom;
		return this;
	}

	public GameLabel SetMarginLeft(int left) {
		style.marginLeft = left;
		return this;
	}

	/// <summary>
	/// Apply margin using theme spacing preset.
	/// </summary>
	public GameLabel SetMargin(MarginPreset preset) {
		preset.ApplyTo(style);
		return this;
	}

	#endregion

	#region Fluent API - Padding

	public GameLabel SetPadding(int all) {
		k_label.style.paddingTop = all;
		k_label.style.paddingBottom = all;
		k_label.style.paddingLeft = all;
		k_label.style.paddingRight = all;
		return this;
	}

	public GameLabel SetPadding(int vertical, int horizontal) {
		k_label.style.paddingTop = vertical;
		k_label.style.paddingBottom = vertical;
		k_label.style.paddingLeft = horizontal;
		k_label.style.paddingRight = horizontal;
		return this;
	}

	public GameLabel SetPadding(int top, int right, int bottom, int left) {
		k_label.style.paddingTop = top;
		k_label.style.paddingRight = right;
		k_label.style.paddingBottom = bottom;
		k_label.style.paddingLeft = left;
		return this;
	}

	public GameLabel SetPaddingLeft(int left) {
		k_label.style.paddingLeft = left;
		return this;
	}

	public GameLabel SetPaddingRight(int right) {
		k_label.style.paddingRight = right;
		return this;
	}

	public GameLabel SetPaddingTop(int top) {
		k_label.style.paddingTop = top;
		return this;
	}

	public GameLabel SetPaddingBottom(int bottom) {
		k_label.style.paddingBottom = bottom;
		return this;
	}

	/// <summary>
	/// Apply padding using theme spacing preset.
	/// </summary>
	public GameLabel SetPadding(PaddingPreset preset) {
		preset.ApplyTo(k_label.style);
		return this;
	}

	#endregion

	#region Fluent API - Display & Visibility

	public GameLabel SetDisplay(DisplayStyle display) {
		style.display = display;
		return this;
	}

	public GameLabel SetVisible(bool visible) {
		style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
		return this;
	}

	public GameLabel SetOpacity(float opacity) {
		style.opacity = opacity;
		return this;
	}

	public GameLabel Hide() {
		style.display = DisplayStyle.None;
		return this;
	}

	public GameLabel Show() {
		style.display = DisplayStyle.Flex;
		return this;
	}

	#endregion

	#region Fluent API - Binding

	public GameLabel BindText(object dataSource, string propertyPath) {
		k_label.dataSource = dataSource;
		k_label.SetBinding(nameof(k_label.text), new UnityEngine.UIElements.DataBinding {
			bindingMode = BindingMode.ToTarget,
			dataSourcePath = new Unity.Properties.PropertyPath(propertyPath)
		});
		return this;
	}

	#endregion

	#region Build

	public GameLabel Build() {
		ApplyTheme();
		return this;
	}

	#endregion

	#region Theme Application

	private void ApplyTheme() {
		var theme = GameTheme.Current;
		var colors = theme.Colors;
		var typography = theme.Typography;

		// Get the appropriate TextStyle for this LabelStyle
		k_currentTextStyle = k_style switch {
			LabelStyle.DisplayLarge => typography.DisplayLarge,
			LabelStyle.DisplayMedium => typography.DisplayMedium,
			LabelStyle.DisplaySmall => typography.DisplaySmall,
			LabelStyle.HeadlineLarge => typography.HeadlineLarge,
			LabelStyle.HeadlineMedium => typography.HeadlineMedium,
			LabelStyle.HeadlineSmall => typography.HeadlineSmall,
			LabelStyle.TitleLarge => typography.TitleLarge,
			LabelStyle.TitleMedium => typography.TitleMedium,
			LabelStyle.TitleSmall => typography.TitleSmall,
			LabelStyle.BodyLarge => typography.BodyLarge,
			LabelStyle.BodyMedium => typography.BodyMedium,
			LabelStyle.BodySmall => typography.BodySmall,
			LabelStyle.LabelLarge => typography.LabelLarge,
			LabelStyle.LabelMedium => typography.LabelMedium,
			LabelStyle.LabelSmall => typography.LabelSmall,
			LabelStyle.Caption => typography.Caption,
			LabelStyle.Overline => typography.Overline,
			LabelStyle.Button => typography.Button,
			LabelStyle.Monospace => typography.Monospace,
			LabelStyle.MonospaceSmall => typography.MonospaceSmall,
			LabelStyle.TableHeader => typography.TableHeader,
			LabelStyle.StatValue => typography.StatValue,
			LabelStyle.FlavorText => typography.FlavorText,
			LabelStyle.DropCap => typography.DropCap,
			_ => typography.BodyMedium
		};

		// Apply typography styles to the label
		k_currentTextStyle.ApplyTo(k_label.style);

		// Apply text transformation (uppercase/lowercase)
		ApplyTextWithTransform();

		// Apply color (unless custom color is set)
		if (!k_useCustomColor) {
			Color textColor = k_colorVariant switch {
				LabelColor.Primary => colors.TextPrimary,
				LabelColor.Secondary => colors.TextSecondary,
				LabelColor.Tertiary => colors.TextTertiary,
				LabelColor.Disabled => colors.TextDisabled,
				LabelColor.Inverse => colors.TextInverse,
				LabelColor.Link => colors.TextLink,
				LabelColor.Success => colors.Success,
				LabelColor.Warning => colors.Warning,
				LabelColor.Error => colors.Error,
				LabelColor.Info => colors.Info,
				LabelColor.Accent => colors.Accent,
				_ => colors.TextPrimary
			};

			k_label.style.color = textColor;
		}
	}

	/// <summary>
	/// Applies text transformation based on the current TextStyle.
	/// </summary>
	private void ApplyTextWithTransform() {
		if (k_currentTextStyle != null && k_currentTextStyle.RequiresTextTransform) {
			k_label.text = k_currentTextStyle.ApplyTextTransform(k_rawText);
		} else {
			k_label.text = k_rawText;
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
using RPGGame.Core;
using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Components;

/// <summary>
/// Button visual variants.
/// </summary>
public enum ButtonVariant {
	Primary,
	Secondary,
	Success,
	Warning,
	Danger,
	Outline,
	OutlineSecondary,
	Ghost,
	Link
}

/// <summary>
/// Button size presets.
/// </summary>
public enum ButtonSize {
	Small,
	Medium,
	Large
}

/// <summary>
/// Icon position relative to text.
/// </summary>
public enum IconPosition {
	Left,
	Right
}

/// <summary>
/// A themed button component with support for multiple variants, sizes, icons, and states.
/// Uses the fluent builder pattern for easy configuration.
/// 
/// <para>Usage:</para>
/// <code>
/// var button = new GameButton("Click Me")
///     .SetVariant(ButtonVariant.Primary)
///     .SetSize(ButtonSize.Large)
///     .SetIcon(myIcon, IconPosition.Left)
///     .OnClick(() => DoSomething())
///     .Build();
/// </code>
/// </summary>
public class GameButton : VisualElement {
	#region Private Fields

	private readonly Button k_button;
	private readonly VisualElement k_iconContainer;
	private readonly Label k_label;
	private readonly VisualElement k_contentContainer;

	private ButtonVariant k_variant = ButtonVariant.Primary;
	private ButtonSize k_size = ButtonSize.Medium;
	private IconPosition k_iconPosition = IconPosition.Left;
	private bool k_isLoading = false;
	private bool k_isFullWidth = false;
	private string? k_tooltipText;

	#endregion

	#region Constructors

	public GameButton() : this(string.Empty) { }

	public GameButton(string text) {
		// Create the internal button
		k_button = new Button();
		k_button.RemoveFromClassList("unity-button");

		// Create content container for icon + label layout
		k_contentContainer = new VisualElement {
			style = {
				flexDirection = FlexDirection.Row,
				alignItems = Align.Center,
				justifyContent = Justify.Center
			}
		};

		// Create icon container (hidden by default)
		k_iconContainer = new VisualElement {
			style = { display = DisplayStyle.None }
		};

		// Create label
		k_label = new Label(text) {
			style = { marginLeft = 0, marginRight = 0, paddingLeft = 0, paddingRight = 0 }
		};

		k_contentContainer.Add(k_iconContainer);
		k_contentContainer.Add(k_label);
		k_button.Add(k_contentContainer);

		Add(k_button);

		// Subscribe to theme changes
		GameTheme.OnThemeChanged += OnThemeChanged;

		// Register callbacks for hover/press states
		k_button.RegisterCallback<MouseEnterEvent>(OnMouseEnter);
		k_button.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
		k_button.RegisterCallback<MouseDownEvent>(OnMouseDown);
		k_button.RegisterCallback<MouseUpEvent>(OnMouseUp);
	}

	#endregion

	#region Fluent API - Content

	public GameButton SetText(string text) {
		var buttonStyle = GameTheme.Current?.Typography?.Button;
		k_label.text = buttonStyle?.ApplyTextTransform(text) ?? text;
		return this;
	}

	public GameButton SetDisplay(DisplayStyle display) {
		style.display = display;
		return this;
	}

	public GameButton SetVisible(bool isVisible) {
		style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
		return this;
	}

	public GameButton SetIcon(Texture2D? icon, IconPosition position = IconPosition.Left) {
		k_iconPosition = position;

		if (icon == null) {
			k_iconContainer.style.display = DisplayStyle.None;
			return this;
		}

		k_iconContainer.Clear();
		k_iconContainer.style.display = DisplayStyle.Flex;

		var iconElement = new VisualElement {
			style = {
				backgroundImage = new StyleBackground(icon),
				width = GameTheme.Current.Components.Button.IconSize,
				height = GameTheme.Current.Components.Button.IconSize
			}
		};

		k_iconContainer.Add(iconElement);
		UpdateIconPosition();

		return this;
	}

	public GameButton SetIcon(Sprite? icon, IconPosition position = IconPosition.Left) {
		k_iconPosition = position;

		if (icon == null) {
			k_iconContainer.style.display = DisplayStyle.None;
			return this;
		}

		k_iconContainer.Clear();
		k_iconContainer.style.display = DisplayStyle.Flex;

		var iconElement = new VisualElement {
			style = {
				backgroundImage = new StyleBackground(icon),
				width = GameTheme.Current.Components.Button.IconSize,
				height = GameTheme.Current.Components.Button.IconSize
			}
		};

		k_iconContainer.Add(iconElement);
		UpdateIconPosition();

		return this;
	}

	public GameButton SetTooltip(string newTooltip) {
		k_tooltipText = newTooltip;
		k_button.tooltip = newTooltip;
		return this;
	}

	#endregion

	#region Fluent API - Appearance

	public GameButton SetVariant(ButtonVariant variant) {
		k_variant = variant;
		ApplyTheme();
		return this;
	}

	public GameButton SetSize(ButtonSize size) {
		k_size = size;
		ApplyTheme();
		return this;
	}

	public GameButton SetFullWidth(bool fullWidth = true) {
		k_isFullWidth = fullWidth;
		style.width = fullWidth ? Length.Percent(100) : StyleKeyword.Auto;
		return this;
	}

	public new GameButton SetEnabled(bool enabled) {
		k_button.SetEnabled(enabled);
		ApplyTheme();
		return this;
	}

	public GameButton SetLoading(bool loading) {
		k_isLoading = loading;
		k_button.SetEnabled(!loading);
		k_label.text = loading ? "Loading..." : k_label.text;
		ApplyTheme();
		return this;
	}

	#endregion

	#region Fluent API - Custom Styling

	public GameButton SetBackgroundColor(Color color) {
		k_button.style.backgroundColor = color;
		return this;
	}

	public GameButton SetTextColor(Color color) {
		k_label.style.color = color;
		return this;
	}

	public GameButton SetBorderColor(Color color) {
		GameTheme.Current.Borders.ApplyColor(k_button.style, color);
		return this;
	}

	public GameButton SetBorderRadius(float radius) {
		GameTheme.Current.Borders.ApplyRadius(k_button.style, radius);
		return this;
	}

	public GameButton SetBorderWidth(float width) {
		GameTheme.Current.Borders.ApplyWidth(k_button.style, width);
		return this;
	}

	public GameButton SetPadding(int vertical, int horizontal) {
		k_button.style.paddingTop = vertical;
		k_button.style.paddingBottom = vertical;
		k_button.style.paddingLeft = horizontal;
		k_button.style.paddingRight = horizontal;
		return this;
	}

	public GameButton SetMargin(int all) {
		style.marginTop = all;
		style.marginBottom = all;
		style.marginLeft = all;
		style.marginRight = all;
		return this;
	}

	public GameButton SetMargin(int vertical, int horizontal) {
		style.marginTop = vertical;
		style.marginBottom = vertical;
		style.marginLeft = horizontal;
		style.marginRight = horizontal;
		return this;
	}

	public GameButton SetMargin(int top, int right, int bottom, int left) {
		style.marginTop = top;
		style.marginRight = right;
		style.marginBottom = bottom;
		style.marginLeft = left;
		return this;
	}

	public GameButton SetMarginTop(int top) {
		style.marginTop = top;
		return this;
	}

	public GameButton SetMarginRight(int right) {
		style.marginRight = right;
		return this;
	}

	public GameButton SetMarginBottom(int bottom) {
		style.marginBottom = bottom;
		return this;
	}

	public GameButton SetMarginLeft(int left) {
		style.marginLeft = left;
		return this;
	}

	public GameButton SetMinWidth(float width) {
		k_button.style.minWidth = width;
		return this;
	}

	public GameButton SetMinHeight(float height) {
		k_button.style.minHeight = height;
		return this;
	}

	#endregion

	#region Fluent API - Events

	public GameButton OnClick(Action callback) {
		k_button.clicked += callback;
		return this;
	}

	public GameButton OnClickWithSound(Action callback, string? soundId = null) {
		k_button.clicked += () => {
			// Play sound if audio is enabled
			if (GameTheme.Current.Audio.Enabled) {
				string sound = soundId ?? GameTheme.Current.Audio.ButtonClick;
				// AudioManager.Instance?.PlaySound(sound);
				UnityEngine.Debug.Log($"[Audio] Playing: {sound}");
			}
			callback?.Invoke();
		};
		return this;
	}

	#endregion

	#region Build

	/// <summary>
	/// Finalizes the button configuration and applies the theme.
	/// </summary>
	public GameButton Build() {
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
		var typography = theme.Typography;
		var components = theme.Components.Button;

		// Get size-specific settings
		var (height, fontSize, padding) = k_size switch {
			ButtonSize.Small => components.Small,
			ButtonSize.Large => components.Large,
			_ => components.Medium
		};

		// Apply size
		k_button.style.minHeight = height;
		k_button.style.minWidth = components.MinWidth;
		padding.ApplyTo(k_button.style);

		// Apply border radius
		borders.ApplyRadius(k_button.style, borders.ButtonRadius);

		// Apply Button typography style (Scaly Sans Bold, uppercase)
		var buttonTextStyle = typography.Button;
		buttonTextStyle.ApplyTo(k_label.style);
	
		// Override font size based on button size (Button style has default size)
		k_label.style.fontSize = fontSize;

		// Apply variant-specific colors
		ApplyVariantColors(colors, borders);

		// Apply text style
		k_label.style.unityTextAlign = TextAnchor.MiddleCenter;

		// Apply icon spacing
		UpdateIconSpacing();
	}

	private void ApplyVariantColors(ColorPalette colors, BorderSettings borders) {
		bool isDisabled = !k_button.enabledSelf || k_isLoading;

		// Get colors based on variant
		var (bgColor, textColor, borderColor, borderWidth) = k_variant switch {
			ButtonVariant.Primary => (
				isDisabled ? colors.PrimaryDisabled : colors.Primary,
				colors.White,
				colors.Transparent,
				0f
			),
			ButtonVariant.Secondary => (
				isDisabled ? colors.SecondaryDisabled : colors.Secondary,
				colors.White,
				colors.Transparent,
				0f
			),
			ButtonVariant.Success => (
				isDisabled ? ModifyAlpha(colors.Success, 0.4f) : colors.Success,
				colors.White,
				colors.Transparent,
				0f
			),
			ButtonVariant.Warning => (
				isDisabled ? ModifyAlpha(colors.Warning, 0.4f) : colors.Warning,
				colors.TextInverse,
				colors.Transparent,
				0f
			),
			ButtonVariant.Danger => (
				isDisabled ? ModifyAlpha(colors.Error, 0.4f) : colors.Error,
				colors.White,
				colors.Transparent,
				0f
			),
			ButtonVariant.Outline => (
				colors.Transparent,
				isDisabled ? colors.TextDisabled : colors.Primary,
				isDisabled ? colors.TextDisabled : colors.Primary,
				borders.WidthMedium
			),
			ButtonVariant.OutlineSecondary => (
				colors.Transparent,
				isDisabled ? colors.TextDisabled : colors.Secondary,
				isDisabled ? colors.TextDisabled : colors.Secondary,
				borders.WidthMedium
			),
			ButtonVariant.Ghost => (
				colors.Transparent,
				isDisabled ? colors.TextDisabled : colors.TextPrimary,
				colors.Transparent,
				0f
			),
			ButtonVariant.Link => (
				colors.Transparent,
				isDisabled ? colors.TextDisabled : colors.TextLink,
				colors.Transparent,
				0f
			),
			_ => (colors.Primary, colors.White, colors.Transparent, 0f)
		};

		k_button.style.backgroundColor = bgColor;
		k_label.style.color = textColor;
		borders.ApplyColor(k_button.style, borderColor);
		borders.ApplyWidth(k_button.style, borderWidth);
	}

	private void OnThemeChanged(GameTheme theme) {
		ApplyTheme();
	}

	#endregion

	#region Interaction States

	private void OnMouseEnter(MouseEnterEvent evt) {
		if (!k_button.enabledSelf || k_isLoading) {
			return;
		}

		var colors = GameTheme.Current.Colors;

		ColorRPG hoverColor = k_variant switch {
			ButtonVariant.Primary => colors.PrimaryHover,
			ButtonVariant.Secondary => colors.SecondaryHover,
			ButtonVariant.Success => colors.SuccessHover,
			ButtonVariant.Warning => colors.WarningHover,
			ButtonVariant.Danger => colors.ErrorHover,
			ButtonVariant.Outline => ModifyAlpha(colors.Primary, 0.1f),
			ButtonVariant.OutlineSecondary => ModifyAlpha(colors.Secondary, 0.1f),
			ButtonVariant.Ghost => ModifyAlpha(colors.TextPrimary, 0.1f),
			ButtonVariant.Link => colors.Transparent,
			_ => colors.PrimaryHover
		};

		k_button.style.backgroundColor = hoverColor;

		// Underline for link variant
		if (k_variant == ButtonVariant.Link) {
			k_label.style.color = colors.TextLinkHover;
		}
	}

	private void OnMouseLeave(MouseLeaveEvent evt) {
		ApplyTheme();
	}

	private void OnMouseDown(MouseDownEvent evt) {
		if (!k_button.enabledSelf || k_isLoading) {
			return;
		}

		var colors = GameTheme.Current.Colors;

		ColorRPG pressedColor = k_variant switch {
			ButtonVariant.Primary => colors.PrimaryPressed,
			ButtonVariant.Secondary => colors.SecondaryPressed,
			ButtonVariant.Success => ModifyAlpha(colors.Success, 0.8f),
			ButtonVariant.Warning => ModifyAlpha(colors.Warning, 0.8f),
			ButtonVariant.Danger => ModifyAlpha(colors.Error, 0.8f),
			ButtonVariant.Outline => ModifyAlpha(colors.Primary, 0.2f),
			ButtonVariant.OutlineSecondary => ModifyAlpha(colors.Secondary, 0.2f),
			ButtonVariant.Ghost => ModifyAlpha(colors.TextPrimary, 0.2f),
			ButtonVariant.Link => colors.Transparent,
			_ => colors.PrimaryPressed
		};

		k_button.style.backgroundColor = pressedColor;
	}

	private void OnMouseUp(MouseUpEvent evt) {
		OnMouseEnter(null!);
	}

	#endregion

	#region Helper Methods

	private void UpdateIconPosition() {
		k_contentContainer.Clear();

		if (k_iconPosition == IconPosition.Right) {
			k_contentContainer.Add(k_label);
			k_contentContainer.Add(k_iconContainer);
		} else {
			k_contentContainer.Add(k_iconContainer);
			k_contentContainer.Add(k_label);
		}

		UpdateIconSpacing();
	}

	private void UpdateIconSpacing() {
		float spacing = GameTheme.Current.Components.Button.IconSpacing;

		if (k_iconContainer.style.display == DisplayStyle.None || string.IsNullOrEmpty(k_label.text)) {
			k_iconContainer.style.marginLeft = 0;
			k_iconContainer.style.marginRight = 0;
			return;
		}

		if (k_iconPosition == IconPosition.Right) {
			k_iconContainer.style.marginLeft = spacing;
			k_iconContainer.style.marginRight = 0;
		} else {
			k_iconContainer.style.marginLeft = 0;
			k_iconContainer.style.marginRight = spacing;
		}
	}

	private static Color ModifyAlpha(Color color, float alpha) {
		return new Color(color.r, color.g, color.b, alpha);
	}

	#endregion

	#region Cleanup

	public new void RemoveFromHierarchy() {
		GameTheme.OnThemeChanged -= OnThemeChanged;
		base.RemoveFromHierarchy();
	}

	#endregion
}
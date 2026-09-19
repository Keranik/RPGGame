using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Components;

#nullable disable

/// <summary>
/// A themed badge component for labels, counts, and status indicators.
/// Supports multiple shape styles appropriate for game UIs.
/// 
/// <para>Usage:</para>
/// <code>
/// // Notification count
/// var badge = new GameBadge()
///     .SetText("5")
///     .SetVariant(BadgeVariant.Error)
///     .SetShape(BadgeShape.Diamond)
///     .Build();
/// 
/// // Status label with icon
/// var status = new GameBadge()
///     .SetText("Online")
///     .SetVariant(BadgeVariant.Success)
///     .SetShape(BadgeShape.Beveled)
///     .SetShowIndicator(true)
///     .Build();
/// 
/// // Compact count badge
/// var count = new GameBadge()
///     .SetText("99+")
///     .SetVariant(BadgeVariant.Error)
///     .SetShape(BadgeShape.Compact)
///     .Build();
/// </code>
/// </summary>
public class GameBadge : VisualElement {
	#region Private Fields

	private readonly VisualElement k_outerFrame;
	private readonly VisualElement k_innerFrame;
	private readonly VisualElement k_background;
	private readonly VisualElement k_highlight;
	private readonly VisualElement k_iconContainer;
	private readonly Label k_label;
	private readonly VisualElement k_indicator;

	private BadgeVariant k_variant = BadgeVariant.Default;
	private BadgeSize k_size = BadgeSize.Medium;
	private BadgeShape k_shape = BadgeShape.Standard;
	private bool k_showIndicator = false;
	private bool k_pulse = false;
	private IVisualElementScheduledItem k_pulseAnimation;

	#endregion

	#region Constructors

	public GameBadge() {
		// Outer frame for decorative borders
		k_outerFrame = new VisualElement {
			name = "badge-outer",
			style = {
				position = Position.Relative,
				flexDirection = FlexDirection.Row,
				alignItems = Align.Center,
				justifyContent = Justify.Center
			}
		};

		// Inner frame for layered effect
		k_innerFrame = new VisualElement {
			name = "badge-inner",
			style = {
				position = Position.Absolute,
				left = 1, top = 1, right = 1, bottom = 1
			}
		};

		// Background layer
		k_background = new VisualElement {
			name = "badge-bg",
			style = {
				position = Position.Absolute,
				left = 0, top = 0, right = 0, bottom = 0
			}
		};

		// Top highlight for depth
		k_highlight = new VisualElement {
			name = "badge-highlight",
			style = {
				position = Position.Absolute,
				left = 2, top = 1, right = 2,
				height = 1
			}
		};

		// Icon container
		k_iconContainer = new VisualElement {
			name = "badge-icon",
			style = { display = DisplayStyle.None, marginRight = 4 }
		};

		// Label
		k_label = new Label {
			name = "badge-label"
		};

		// Status indicator (pulsing dot)
		k_indicator = new VisualElement {
			name = "badge-indicator",
			style = { display = DisplayStyle.None, marginRight = 6 }
		};

		// Build hierarchy
		k_outerFrame.Add(k_background);
		k_outerFrame.Add(k_innerFrame);
		k_outerFrame.Add(k_highlight);
		k_outerFrame.Add(k_indicator);
		k_outerFrame.Add(k_iconContainer);
		k_outerFrame.Add(k_label);

		Add(k_outerFrame);

		GameTheme.OnThemeChanged += OnThemeChanged;
	}

	public GameBadge(string text) : this() {
		SetText(text);
	}

	#endregion

	#region Fluent API - Content

	public GameBadge SetText(string text) {
		k_label.text = text;
		return this;
	}

	public GameBadge SetIcon(Texture2D icon) {
		k_iconContainer.Clear();
		if (icon != null) {
			float iconSize = GetIconSize();
			var iconElement = new VisualElement {
				style = {
					backgroundImage = new StyleBackground(icon),
					width = iconSize,
					height = iconSize
				}
			};
			k_iconContainer.Add(iconElement);
			k_iconContainer.style.display = DisplayStyle.Flex;
		} else {
			k_iconContainer.style.display = DisplayStyle.None;
		}
		return this;
	}

	public GameBadge SetIcon(Sprite sprite) {
		k_iconContainer.Clear();
		if (sprite != null) {
			float iconSize = GetIconSize();
			var iconElement = new VisualElement {
				style = {
					backgroundImage = new StyleBackground(sprite),
					width = iconSize,
					height = iconSize
				}
			};
			k_iconContainer.Add(iconElement);
			k_iconContainer.style.display = DisplayStyle.Flex;
		} else {
			k_iconContainer.style.display = DisplayStyle.None;
		}
		return this;
	}

	#endregion

	#region Fluent API - Appearance

	public GameBadge SetVariant(BadgeVariant variant) {
		k_variant = variant;
		ApplyTheme();
		return this;
	}

	public GameBadge SetSize(BadgeSize size) {
		k_size = size;
		ApplyTheme();
		return this;
	}

	public GameBadge SetShape(BadgeShape shape) {
		k_shape = shape;
		ApplyTheme();
		return this;
	}

	public GameBadge SetShowIndicator(bool show = true) {
		k_showIndicator = show;
		k_indicator.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
		ApplyTheme();
		return this;
	}

	public GameBadge SetPulse(bool pulse = true) {
		k_pulse = pulse;
		if (pulse) {
			StartPulseAnimation();
		} else {
			StopPulseAnimation();
		}
		return this;
	}

	#endregion

	#region Build

	public GameBadge Build() {
		ApplyTheme();
		return this;
	}

	#endregion

	#region Theme Application

	private void ApplyTheme() {
		var theme = GameTheme.Current;
		var colors = theme.Colors;
		var borders = theme.Borders;
		var typography = theme.Typography;

		// Get variant colors
		var (bgColor, textColor, borderColor, highlightColor) = GetVariantColors(colors);

		// Get size dimensions
		var (height, textStyle, paddingH, paddingV) = GetSizeDimensions();

		// Apply shape-specific styling
		ApplyShapeStyle(bgColor, borderColor, highlightColor, height, paddingH, paddingV);

		// Label styling - apply typography
		textStyle.ApplyTo(k_label.style);
		k_label.style.color = textColor;
		k_label.style.unityTextAlign = TextAnchor.MiddleCenter;

		// Indicator styling
		if (k_showIndicator) {
			float indicatorSize = k_size == BadgeSize.Small ? 6 : 8;
			k_indicator.style.width = indicatorSize;
			k_indicator.style.height = indicatorSize;
			k_indicator.style.backgroundColor = textColor;
			borders.ApplyRadius(k_indicator.style, borders.RadiusFull);
		}

		// Icon container
		float iconSize = GetIconSize();
		var icon = k_iconContainer.Q<VisualElement>();
		if (icon != null) {
			icon.style.width = iconSize;
			icon.style.height = iconSize;
			icon.style.unityBackgroundImageTintColor = textColor;
		}
	}

	private (Color bg, Color text, Color border, Color highlight) GetVariantColors(ColorPalette colors) {
		return k_variant switch {
			BadgeVariant.Primary => (
				colors.Primary,
				colors.White,
				DarkenColor(colors.Primary, 0.3f),
				LightenColor(colors.Primary, 0.3f)
			),
			BadgeVariant.Secondary => (
				colors.Secondary,
				colors.White,
				DarkenColor(colors.Secondary, 0.3f),
				LightenColor(colors.Secondary, 0.3f)
			),
			BadgeVariant.Success => (
				colors.Success,
				colors.White,
				DarkenColor(colors.Success, 0.3f),
				LightenColor(colors.Success, 0.3f)
			),
			BadgeVariant.Warning => (
				colors.Warning,
				colors.TextInverse,
				DarkenColor(colors.Warning, 0.2f),
				LightenColor(colors.Warning, 0.2f)
			),
			BadgeVariant.Error => (
				colors.Error,
				colors.White,
				DarkenColor(colors.Error, 0.3f),
				LightenColor(colors.Error, 0.3f)
			),
			BadgeVariant.Info => (
				colors.Info,
				colors.White,
				DarkenColor(colors.Info, 0.3f),
				LightenColor(colors.Info, 0.3f)
			),
			BadgeVariant.Outline => (
				colors.Transparent,
				colors.TextPrimary,
				colors.SurfaceBorder,
				colors.Transparent
			),
			BadgeVariant.Dark => (
				new Color(0.1f, 0.1f, 0.12f, 0.95f),
				colors.White,
				new Color(0.2f, 0.2f, 0.22f, 1f),
				new Color(0.3f, 0.3f, 0.32f, 0.5f)
			),
			BadgeVariant.Legendary => (
				new Color(0.6f, 0.45f, 0.1f, 1f),
				colors.White,
				new Color(0.4f, 0.3f, 0.05f, 1f),
				new Color(1f, 0.85f, 0.4f, 0.6f)
			),
			BadgeVariant.Epic => (
				new Color(0.5f, 0.2f, 0.7f, 1f),
				colors.White,
				new Color(0.35f, 0.1f, 0.5f, 1f),
				new Color(0.7f, 0.4f, 0.9f, 0.5f)
			),
			_ => (
				colors.BackgroundTertiary,
				colors.TextPrimary,
				colors.SurfaceBorder,
				LightenColor(colors.BackgroundTertiary, 0.1f)
			)
		};
	}

	private (float height, TextStyle textStyle, float paddingH, float paddingV) GetSizeDimensions() {
		var badgeStyles = GameTheme.Current.Components.Badge;
		var spacing = GameTheme.Current.Spacing;
		var typography = GameTheme.Current.Typography;

		return k_size switch {
			BadgeSize.Small => (badgeStyles.HeightSmall, typography.Caption, spacing.XS, spacing.XXS),
			BadgeSize.Large => (badgeStyles.HeightLarge, typography.LabelMedium, spacing.SM, spacing.XXS),
			BadgeSize.ExtraLarge => (badgeStyles.HeightExtraLarge, typography.LabelLarge, spacing.MD, spacing.XS),
			_ => (badgeStyles.HeightMedium, typography.LabelSmall, spacing.XS, spacing.XXS)
		};
	}

	private float GetIconSize() {
		var theme = GameTheme.Current;
		return k_size switch {
			BadgeSize.Small => theme.Icons.SizeSM,
			BadgeSize.Large => theme.Icons.SizeLG,
			BadgeSize.ExtraLarge => theme.Icons.SizeXL,
			_ => theme.Icons.SizeMD
		};
	}

	private void ApplyShapeStyle(Color bgColor, Color borderColor, Color highlightColor, float height, float paddingH, float paddingV) {
		var borders = GameTheme.Current.Borders;

		// Reset all styles
		k_outerFrame.style.rotate = new Rotate(0);
		k_label.style.rotate = new Rotate(0);
		k_iconContainer.style.rotate = new Rotate(0);
		k_indicator.style.rotate = new Rotate(0);

		switch (k_shape) {
			case BadgeShape.Compact:
				// Minimal padding, tight fit
				ApplyCompactStyle(bgColor, borderColor, highlightColor, height, paddingV);
				break;

			case BadgeShape.Beveled:
				// Angular corners - tech/sci-fi feel
				ApplyBeveledStyle(bgColor, borderColor, highlightColor, height, paddingH, paddingV);
				break;

			case BadgeShape.Diamond:
				// Rotated square - good for single characters/numbers
				ApplyDiamondStyle(bgColor, borderColor, highlightColor, height);
				break;

			case BadgeShape.Hexagon:
				// Hex shape approximation
				ApplyHexagonStyle(bgColor, borderColor, highlightColor, height, paddingH, paddingV);
				break;

			case BadgeShape.Shield:
				// Shield/banner shape
				ApplyShieldStyle(bgColor, borderColor, highlightColor, height, paddingH, paddingV);
				break;

			case BadgeShape.Tag:
				// Price tag / label style
				ApplyTagStyle(bgColor, borderColor, highlightColor, height, paddingH, paddingV);
				break;

			case BadgeShape.Bracket:
				// Tech brackets [ TEXT ]
				ApplyBracketStyle(bgColor, borderColor, highlightColor, height, paddingH, paddingV);
				break;

			default: // Standard
				ApplyStandardStyle(bgColor, borderColor, highlightColor, height, paddingH, paddingV);
				break;
		}

		// Highlight for depth (top edge lighter)
		k_highlight.style.backgroundColor = highlightColor;
	}

	private void ApplyCompactStyle(Color bgColor, Color borderColor, Color highlightColor, float height, float paddingV) {
		var borders = GameTheme.Current.Borders;
		var spacing = GameTheme.Current.Spacing;

		k_outerFrame.style.height = height;
		k_outerFrame.style.paddingLeft = spacing.XXS;    
		k_outerFrame.style.paddingRight = spacing.XXS;  
		k_outerFrame.style.paddingTop = paddingV;
		k_outerFrame.style.paddingBottom = paddingV;

		k_background.style.backgroundColor = bgColor;
		borders.ApplyRadius(k_background.style, borders.RadiusXS);   
		borders.ApplyRadius(k_outerFrame.style, borders.RadiusXS);   
		borders.ApplyColor(k_outerFrame.style, borderColor);
		borders.ApplyWidth(k_outerFrame.style, borders.WidthThin);   
	}

	private void ApplyStandardStyle(Color bgColor, Color borderColor, Color highlightColor, float height, float paddingH, float paddingV) {
		var borders = GameTheme.Current.Borders;

		k_outerFrame.style.height = height;
		k_outerFrame.style.paddingLeft = paddingH;
		k_outerFrame.style.paddingRight = paddingH;
		k_outerFrame.style.paddingTop = paddingV;
		k_outerFrame.style.paddingBottom = paddingV;

		k_background.style.backgroundColor = bgColor;
		borders.ApplyRadius(k_background.style, borders.RadiusSM); 
		borders.ApplyRadius(k_outerFrame.style, borders.RadiusSM);  
		borders.ApplyColor(k_outerFrame.style, borderColor);
		borders.ApplyWidth(k_outerFrame.style, borders.WidthThin);   
	}

	private void ApplyBeveledStyle(Color bgColor, Color borderColor, Color highlightColor, float height, float paddingH, float paddingV) {
		var borders = GameTheme.Current.Borders;
		float bevel = height * 0.3f;

		k_outerFrame.style.height = height;
		k_outerFrame.style.paddingLeft = paddingH + bevel * 0.5f;
		k_outerFrame.style.paddingRight = paddingH + bevel * 0.5f;
		k_outerFrame.style.paddingTop = paddingV;
		k_outerFrame.style.paddingBottom = paddingV;

		k_background.style.backgroundColor = bgColor;

		// Beveled corners - different radius on each corner
		k_background.style.borderTopLeftRadius = borders.RadiusXS;       
		k_background.style.borderTopRightRadius = bevel;
		k_background.style.borderBottomRightRadius = borders.RadiusXS;  
		k_background.style.borderBottomLeftRadius = bevel;

		k_outerFrame.style.borderTopLeftRadius = borders.RadiusXS;      
		k_outerFrame.style.borderTopRightRadius = bevel;
		k_outerFrame.style.borderBottomRightRadius = borders.RadiusXS;   
		k_outerFrame.style.borderBottomLeftRadius = bevel;

		borders.ApplyColor(k_outerFrame.style, borderColor);
		borders.ApplyWidth(k_outerFrame.style, borders.WidthThin);      

		// Inner frame for layered tech look
		k_innerFrame.style.display = DisplayStyle.Flex;
		k_innerFrame.style.backgroundColor = DarkenColor(bgColor, 0.15f);
		float innerBevel = bevel - borders.WidthThin;
		k_innerFrame.style.borderTopLeftRadius = borders.WidthThin;      
		k_innerFrame.style.borderTopRightRadius = innerBevel;            
		k_innerFrame.style.borderBottomRightRadius = borders.WidthThin;  
		k_innerFrame.style.borderBottomLeftRadius = innerBevel;          
	}

	private void ApplyDiamondStyle(Color bgColor, Color borderColor, Color highlightColor, float height) {
		var borders = GameTheme.Current.Borders;
		float size = height;

		k_outerFrame.style.width = size;
		k_outerFrame.style.height = size;
		k_outerFrame.style.paddingLeft = 0;
		k_outerFrame.style.paddingRight = 0;
		k_outerFrame.style.paddingTop = 0;
		k_outerFrame.style.paddingBottom = 0;

		// Rotate the container 45 degrees
		k_outerFrame.style.rotate = new Rotate(45);

		// Counter-rotate content
		k_label.style.rotate = new Rotate(-45);
		k_iconContainer.style.rotate = new Rotate(-45);
		k_indicator.style.rotate = new Rotate(-45);

		k_background.style.backgroundColor = bgColor;
		borders.ApplyRadius(k_background.style, 3);
		borders.ApplyRadius(k_outerFrame.style, 3);
		borders.ApplyColor(k_outerFrame.style, borderColor);
		borders.ApplyWidth(k_outerFrame.style, 1);

		k_innerFrame.style.display = DisplayStyle.None;
	}

	private void ApplyHexagonStyle(Color bgColor, Color borderColor, Color highlightColor, float height, float paddingH, float paddingV) {
		var borders = GameTheme.Current.Borders;

		k_outerFrame.style.height = height;
		k_outerFrame.style.paddingLeft = paddingH + 4;
		k_outerFrame.style.paddingRight = paddingH + 4;
		k_outerFrame.style.paddingTop = paddingV;
		k_outerFrame.style.paddingBottom = paddingV;

		k_background.style.backgroundColor = bgColor;

		// Approximate hex with asymmetric corners
		float hexCorner = height * 0.4f;
		k_background.style.borderTopLeftRadius = hexCorner;
		k_background.style.borderTopRightRadius = hexCorner;
		k_background.style.borderBottomRightRadius = hexCorner;
		k_background.style.borderBottomLeftRadius = hexCorner;

		k_outerFrame.style.borderTopLeftRadius = hexCorner;
		k_outerFrame.style.borderTopRightRadius = hexCorner;
		k_outerFrame.style.borderBottomRightRadius = hexCorner;
		k_outerFrame.style.borderBottomLeftRadius = hexCorner;

		borders.ApplyColor(k_outerFrame.style, borderColor);
		borders.ApplyWidth(k_outerFrame.style, 1);
	}

	private void ApplyShieldStyle(Color bgColor, Color borderColor, Color highlightColor, float height, float paddingH, float paddingV) {
		var borders = GameTheme.Current.Borders;

		k_outerFrame.style.height = height;
		k_outerFrame.style.paddingLeft = paddingH;
		k_outerFrame.style.paddingRight = paddingH;
		k_outerFrame.style.paddingTop = paddingV;
		k_outerFrame.style.paddingBottom = paddingV + 2;

		k_background.style.backgroundColor = bgColor;

		// Shield shape - rounded top, pointed bottom (approximated)
		k_background.style.borderTopLeftRadius = 4;
		k_background.style.borderTopRightRadius = 4;
		k_background.style.borderBottomRightRadius = height * 0.5f;
		k_background.style.borderBottomLeftRadius = height * 0.5f;

		k_outerFrame.style.borderTopLeftRadius = 4;
		k_outerFrame.style.borderTopRightRadius = 4;
		k_outerFrame.style.borderBottomRightRadius = height * 0.5f;
		k_outerFrame.style.borderBottomLeftRadius = height * 0.5f;

		borders.ApplyColor(k_outerFrame.style, borderColor);
		borders.ApplyWidth(k_outerFrame.style, 1);
	}

	private void ApplyTagStyle(Color bgColor, Color borderColor, Color highlightColor, float height, float paddingH, float paddingV) {
		var borders = GameTheme.Current.Borders;

		k_outerFrame.style.height = height;
		k_outerFrame.style.paddingLeft = paddingH + 6;
		k_outerFrame.style.paddingRight = paddingH;
		k_outerFrame.style.paddingTop = paddingV;
		k_outerFrame.style.paddingBottom = paddingV;

		k_background.style.backgroundColor = bgColor;

		// Tag shape - angled left edge
		k_background.style.borderTopLeftRadius = height * 0.5f;
		k_background.style.borderTopRightRadius = 3;
		k_background.style.borderBottomRightRadius = 3;
		k_background.style.borderBottomLeftRadius = height * 0.5f;

		k_outerFrame.style.borderTopLeftRadius = height * 0.5f;
		k_outerFrame.style.borderTopRightRadius = 3;
		k_outerFrame.style.borderBottomRightRadius = 3;
		k_outerFrame.style.borderBottomLeftRadius = height * 0.5f;

		borders.ApplyColor(k_outerFrame.style, borderColor);
		borders.ApplyWidth(k_outerFrame.style, 1);
	}

	private void ApplyBracketStyle(Color bgColor, Color borderColor, Color highlightColor, float height, float paddingH, float paddingV) {
		var borders = GameTheme.Current.Borders;

		k_outerFrame.style.height = height;
		k_outerFrame.style.paddingLeft = paddingH + 4;
		k_outerFrame.style.paddingRight = paddingH + 4;
		k_outerFrame.style.paddingTop = paddingV;
		k_outerFrame.style.paddingBottom = paddingV;

		// Transparent/minimal background
		k_background.style.backgroundColor = bgColor;
		borders.ApplyRadius(k_background.style, 0);
		borders.ApplyRadius(k_outerFrame.style, 0);

		// Only left and right borders (bracket style)
		k_outerFrame.style.borderLeftWidth = 2;
		k_outerFrame.style.borderRightWidth = 2;
		k_outerFrame.style.borderTopWidth = 0;
		k_outerFrame.style.borderBottomWidth = 0;
		k_outerFrame.style.borderLeftColor = borderColor;
		k_outerFrame.style.borderRightColor = borderColor;
	}

	#endregion

	#region Animation

	private void StartPulseAnimation() {
		if (k_pulseAnimation != null) {
			return;
		}

		float startTime = Time.time;
		k_pulseAnimation = schedule.Execute(() => {
			float elapsed = Time.time - startTime;
			float pulse = (Mathf.Sin(elapsed * 4f) + 1f) * 0.5f; // 0 to 1
			float scale = 1f + (pulse * 0.1f); // 1.0 to 1.1
			k_outerFrame.style.scale = new Scale(new Vector2(scale, scale));
		}).Every(16);
	}

	private void StopPulseAnimation() {
		k_pulseAnimation?.Pause();
		k_pulseAnimation = null;
		k_outerFrame.style.scale = new Scale(Vector2.one);
	}

	#endregion

	#region Color Helpers

	private static Color LightenColor(Color color, float amount) {
		return new Color(
			Mathf.Min(1f, color.r + amount),
			Mathf.Min(1f, color.g + amount),
			Mathf.Min(1f, color.b + amount),
			color.a
		);
	}

	private static Color DarkenColor(Color color, float amount) {
		return new Color(
			Mathf.Max(0f, color.r - amount),
			Mathf.Max(0f, color.g - amount),
			Mathf.Max(0f, color.b - amount),
			color.a
		);
	}

	#endregion

	#region Theme Changed

	private void OnThemeChanged(GameTheme theme) => ApplyTheme();

	#endregion

	#region Cleanup

	public new void RemoveFromHierarchy() {
		StopPulseAnimation();
		GameTheme.OnThemeChanged -= OnThemeChanged;
		base.RemoveFromHierarchy();
	}

	#endregion
}

#region Enums

public enum BadgeVariant {
	Default,
	Primary,
	Secondary,
	Success,
	Warning,
	Error,
	Info,
	Outline,
	Dark,
	Legendary,
	Epic
}

public enum BadgeSize {
	Small,
	Medium,
	Large,
	ExtraLarge
}

public enum BadgeShape {
	/// <summary>Standard rectangular with small corner radius</summary>
	Standard,
	/// <summary>Minimal padding, tight text fit</summary>
	Compact,
	/// <summary>Angular/chambered corners - tech/sci-fi style</summary>
	Beveled,
	/// <summary>Rotated square - good for single characters</summary>
	Diamond,
	/// <summary>Hexagonal shape approximation</summary>
	Hexagon,
	/// <summary>Shield/banner shape</summary>
	Shield,
	/// <summary>Price tag with angled left edge</summary>
	Tag,
	/// <summary>Tech brackets [ TEXT ]</summary>
	Bracket
}

#endregion
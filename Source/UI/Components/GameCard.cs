using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.UIElements.Cursor;

namespace RPGGame.UI.Components;

#nullable disable

/// <summary>
/// A themed card component for displaying content in a structured, elevated container.
/// Supports header, content, footer, and optional image/media areas.
/// 
/// <para>Usage:</para>
/// <code>
/// var itemCard = new GameCard()
///     .SetHeader("Legendary Sword")
///     .SetSubheader("Weapon • Rare")
///     .SetImage(swordSprite)
///     .SetContent(statsPanel)
///     .SetFooter(actionButtons)
///     .SetClickable(() => SelectItem())
///     .Build();
/// </code>
/// </summary>
public class GameCard : VisualElement {
	#region Private Fields

	private readonly VisualElement k_container;
	private readonly VisualElement k_imageContainer;
	private readonly VisualElement k_headerContainer;
	private readonly Label k_headerLabel;
	private readonly Label k_subheaderLabel;
	private readonly VisualElement k_contentContainer;
	private readonly VisualElement k_footerContainer;

	private CardVariant k_variant = CardVariant.Elevated;
	private bool k_isClickable = false;
	private bool k_isSelected = false;
	private Action k_onClick;

	#endregion

	#region Constructors

	public GameCard() {
		k_container = new VisualElement {
			style = { flexDirection = FlexDirection.Column }
		};

		// Image area (hidden by default)
		k_imageContainer = new VisualElement {
			style = {
				display = DisplayStyle.None,
				alignItems = Align.Center,
				justifyContent = Justify.Center
			}
		};

		// Header area
		k_headerContainer = new VisualElement {
			style = {
				display = DisplayStyle.None,
				flexDirection = FlexDirection.Column
			}
		};

		k_headerLabel = new Label();
		k_subheaderLabel = new Label { style = { display = DisplayStyle.None } };

		k_headerContainer.Add(k_headerLabel);
		k_headerContainer.Add(k_subheaderLabel);

		// Content area
		k_contentContainer = new VisualElement {
			style = { flexGrow = 1 }
		};

		// Footer area
		k_footerContainer = new VisualElement {
			style = {
				display = DisplayStyle.None,
				flexDirection = FlexDirection.Row,
				justifyContent = Justify.FlexEnd
			}
		};

		k_container.Add(k_imageContainer);
		k_container.Add(k_headerContainer);
		k_container.Add(k_contentContainer);
		k_container.Add(k_footerContainer);

		Add(k_container);

		// Register interaction callbacks
		RegisterCallback<MouseEnterEvent>(OnMouseEnter);
		RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
		RegisterCallback<ClickEvent>(OnClick);

		GameTheme.OnThemeChanged += OnThemeChanged;
	}

	#endregion

	#region Fluent API - Content

	public GameCard SetHeader(string header) {
		k_headerLabel.text = header;
		k_headerContainer.style.display = DisplayStyle.Flex;
		ApplyTheme();
		return this;
	}

	public GameCard SetSubheader(string subheader) {
		k_subheaderLabel.text = subheader;
		k_subheaderLabel.style.display = string.IsNullOrEmpty(subheader) ? DisplayStyle.None : DisplayStyle.Flex;
		return this;
	}

	public GameCard SetImage(Texture2D image, float height = 150) {
		k_imageContainer.Clear();
		if (image != null) {
			var imageElement = new VisualElement {
				style = {
					backgroundImage = new StyleBackground(image),
					width = Length.Percent(100),
					height = height,
					backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
					backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
					backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
					backgroundSize = new BackgroundSize(BackgroundSizeType.Contain)
				}
			};
			k_imageContainer.Add(imageElement);
			k_imageContainer.style.display = DisplayStyle.Flex;
		} else {
			k_imageContainer.style.display = DisplayStyle.None;
		}
		return this;
	}

	public GameCard SetImage(Sprite sprite, float height = 150) {
		k_imageContainer.Clear();
		if (sprite != null) {
			var imageElement = new VisualElement {
				style = {
					backgroundImage = new StyleBackground(sprite),
					width = Length.Percent(100),
					height = height,
					backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
					backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
					backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
					backgroundSize = new BackgroundSize(BackgroundSizeType.Contain)
				}
			};
			k_imageContainer.Add(imageElement);
			k_imageContainer.style.display = DisplayStyle.Flex;
		} else {
			k_imageContainer.style.display = DisplayStyle.None;
		}
		return this;
	}

	public GameCard SetContent(VisualElement content) {
		k_contentContainer.Clear();
		if (content != null) {
			k_contentContainer.Add(content);
		}
		return this;
	}

	public GameCard AddContent(VisualElement content) {
		k_contentContainer.Add(content);
		return this;
	}

	public GameCard SetFooter(VisualElement footer) {
		k_footerContainer.Clear();
		if (footer != null) {
			k_footerContainer.Add(footer);
			k_footerContainer.style.display = DisplayStyle.Flex;
		} else {
			k_footerContainer.style.display = DisplayStyle.None;
		}
		return this;
	}

	public GameCard AddFooterAction(GameButton button) {
		k_footerContainer.Add(button);
		k_footerContainer.style.display = DisplayStyle.Flex;
		return this;
	}

	public VisualElement Content => k_contentContainer;
	public VisualElement Footer => k_footerContainer;

	#endregion

	#region Fluent API - Appearance

	public GameCard SetVariant(CardVariant variant) {
		k_variant = variant;
		ApplyTheme();
		return this;
	}

	public GameCard SetWidth(float width) {
		style.width = width;
		return this;
	}

	public GameCard SetWidth(Length width) {
		style.width = width;
		return this;
	}

	public GameCard SetHeight(float height) {
		style.height = height;
		return this;
	}

	public GameCard SetHeight(Length height) {
		style.height = height;
		return this;
	}

	public GameCard SetMinWidth(float minWidth) {
		style.minWidth = minWidth;
		return this;
	}

	public GameCard SetMaxWidth(float maxWidth) {
		style.maxWidth = maxWidth;
		return this;
	}

	public GameCard SetMargin(int all) {
		style.marginTop = all;
		style.marginRight = all;
		style.marginBottom = all;
		style.marginLeft = all;
		return this;
	}

	public GameCard SetMargin(int vertical, int horizontal) {
		style.marginTop = vertical;
		style.marginBottom = vertical;
		style.marginLeft = horizontal;
		style.marginRight = horizontal;
		return this;
	}

	public GameCard SetSelected(bool selected) {
		k_isSelected = selected;
		ApplyTheme();
		return this;
	}

	#endregion

	#region Fluent API - Behavior

	public GameCard SetClickable(Action onClick = null) {
		k_isClickable = true;
		k_onClick = onClick;
		return this;
	}

	#endregion

	#region Build

	public GameCard Build() {
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

		// Variant-specific styling
		Color bgColor, borderColor;
		float borderWidth;

		switch (k_variant) {
			case CardVariant.Outlined:
				bgColor = colors.Transparent;
				borderColor = colors.SurfaceBorder;
				borderWidth = borders.WidthMedium;
				break;
			case CardVariant.Filled:
				bgColor = colors.Surface;
				borderColor = colors.Transparent;
				borderWidth = 0;
				break;
			default: // Elevated
				bgColor = colors.BackgroundElevated;
				borderColor = colors.SurfaceBorder;
				borderWidth = borders.WidthThin;
				break;
		}

		// Selected state
		if (k_isSelected) {
			borderColor = colors.Primary;
			borderWidth = borders.WidthMedium;
		}

		k_container.style.backgroundColor = bgColor;
		borders.ApplyColor(k_container.style, borderColor);
		borders.ApplyWidth(k_container.style, borderWidth);
		borders.ApplyRadius(k_container.style, borders.CardRadius);
		spacing.CardPadding.ApplyTo(k_container.style);

		// Header styling
		typography.TitleMedium.ApplyTo(k_headerLabel.style);
		k_headerLabel.style.color = colors.TextPrimary;

		typography.Caption.ApplyTo(k_subheaderLabel.style);
		k_subheaderLabel.style.color = colors.TextSecondary;
		k_subheaderLabel.style.marginTop = spacing.XXS;

		k_headerContainer.style.marginBottom = spacing.SM;

		// Footer styling
		k_footerContainer.style.marginTop = spacing.SM;
		k_footerContainer.style.paddingTop = spacing.SM;
		k_footerContainer.style.borderTopWidth = borders.WidthThin;
		k_footerContainer.style.borderTopColor = colors.SurfaceBorder;

		// Clickable cursor
		if (k_isClickable) {
			style.cursor = new Cursor();
		}
	}

	private void OnThemeChanged(GameTheme theme) => ApplyTheme();

	#endregion

	#region Interaction

	private void OnMouseEnter(MouseEnterEvent evt) {
		if (!k_isClickable) {
			return;
		}

		var colors = GameTheme.Current.Colors;
		k_container.style.backgroundColor = colors.SurfaceHover;
	}

	private void OnMouseLeave(MouseLeaveEvent evt) {
		ApplyTheme();
	}

	private void OnClick(ClickEvent evt) {
		if (!k_isClickable) {
			return;
		}
		k_onClick?.Invoke();
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

public enum CardVariant {
	Elevated,
	Outlined,
	Filled
}

#endregion
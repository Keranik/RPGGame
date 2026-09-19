using RPGGame.Core.Items;
using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Components;

#nullable disable

/// <summary>
/// A themed frame/border that displays rarity colors for items.
/// Perfect for inventory slots, item cards, and loot displays.
/// 
/// <para>Usage:</para>
/// <code>
/// var itemFrame = new GameRarityFrame()
///     .SetRarity(RarityType.Legendary)
///     .SetContent(itemIcon)
///     .SetShowGlow(true)
///     .Build();
/// </code>
/// </summary>
public class GameRarityFrame : VisualElement {
	#region Private Fields

	private readonly VisualElement k_glowLayer;
	private readonly VisualElement k_borderLayer;
	private readonly VisualElement k_backgroundLayer;
	private readonly VisualElement k_contentContainer;

	private RarityType k_rarity = RarityType.Common;
	private bool k_showGlow = false;
	private float k_size = 64;

	#endregion

	#region Constructors

	public GameRarityFrame() {
		style.position = Position.Relative;

		k_glowLayer = new VisualElement {
			style = {
				position = Position.Absolute,
				left = -4, top = -4, right = -4, bottom = -4,
				display = DisplayStyle.None
			}
		};

		k_borderLayer = new VisualElement {
			style = {
				position = Position.Absolute,
				left = 0, top = 0, right = 0, bottom = 0
			}
		};

		k_backgroundLayer = new VisualElement {
			style = {
				position = Position.Absolute,
				left = 2, top = 2, right = 2, bottom = 2
			}
		};

		k_contentContainer = new VisualElement {
			style = {
				position = Position.Absolute,
				left = 4, top = 4, right = 4, bottom = 4,
				alignItems = Align.Center,
				justifyContent = Justify.Center
			}
		};

		Add(k_glowLayer);
		Add(k_borderLayer);
		Add(k_backgroundLayer);
		Add(k_contentContainer);

		GameTheme.OnThemeChanged += OnThemeChanged;
	}

	#endregion

	#region Fluent API

	public GameRarityFrame SetRarity(RarityType rarity) {
		k_rarity = rarity;
		ApplyTheme();
		return this;
	}

	public GameRarityFrame SetContent(VisualElement content) {
		k_contentContainer.Clear();
		if (content != null) {
			k_contentContainer.Add(content);
		}
		return this;
	}

	public GameRarityFrame SetIcon(Texture2D icon) {
		k_contentContainer.Clear();
		if (icon != null) {
			var iconElement = new VisualElement {
				style = {
					backgroundImage = new StyleBackground(icon),
					width = Length.Percent(100),
					height = Length.Percent(100),
					backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
					backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
					backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
					backgroundSize = new BackgroundSize(BackgroundSizeType.Contain)
				}
			};
			k_contentContainer.Add(iconElement);
		}
		return this;
	}

	public GameRarityFrame SetIcon(Sprite sprite) {
		k_contentContainer.Clear();
		if (sprite != null) {
			var iconElement = new VisualElement {
				style = {
					backgroundImage = new StyleBackground(sprite),
					width = Length.Percent(100),
					height = Length.Percent(100),
					backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
					backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
					backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
					backgroundSize = new BackgroundSize(BackgroundSizeType.Contain)
				}
			};
			k_contentContainer.Add(iconElement);
		}
		return this;
	}

	public GameRarityFrame SetShowGlow(bool show = true) {
		k_showGlow = show;
		k_glowLayer.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
		ApplyTheme();
		return this;
	}

	public GameRarityFrame SetSize(float size) {
		k_size = size;
		style.width = size;
		style.height = size;
		return this;
	}

	public VisualElement Content => k_contentContainer;

	#endregion

	#region Build

	public GameRarityFrame Build() {
		ApplyTheme();
		style.width = k_size;
		style.height = k_size;
		return this;
	}

	#endregion

	#region Theme Application

	private void ApplyTheme() {
		var theme = GameTheme.Current;
		var colors = theme.Colors;
		var borders = theme.Borders;

		Color rarityColor = colors.GetRarityColor(k_rarity);
		Color bgColor = colors.BackgroundSecondary;

		// Glow layer
		if (k_showGlow && k_rarity >= RarityType.Rare) {
			k_glowLayer.style.display = DisplayStyle.Flex;
			k_glowLayer.style.backgroundColor = ModifyAlpha(rarityColor, 0.3f);
			borders.ApplyRadius(k_glowLayer.style, borders.RadiusMD + 4);
		} else {
			k_glowLayer.style.display = DisplayStyle.None;
		}

		// Border layer
		k_borderLayer.style.backgroundColor = rarityColor;
		borders.ApplyRadius(k_borderLayer.style, borders.RadiusMD);

		// Background layer
		k_backgroundLayer.style.backgroundColor = bgColor;
		borders.ApplyRadius(k_backgroundLayer.style, borders.RadiusSM);
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
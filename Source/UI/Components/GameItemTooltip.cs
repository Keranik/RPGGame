using RPGGame.Core;
using RPGGame.Core.Generation;
using RPGGame.Core.Items;
using RPGGame.Core.Prototypes;
using RPGGame.Core.Prototypes.Stats;
using RPGGame.Core.Stats;
using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Components;

/// <summary>
/// A rich item tooltip with stat display and comparison support.
/// Supports modifier keys: Alt for detailed stats, Shift for comparison.
/// </summary>
public class GameItemTooltip : GameTooltip {
	#region Private Fields

	private ItemInstance? k_item;
	private ItemInstance? k_comparisonItem;
	private InventoryManager k_inventoryManager => GameServices.Inventory;
	private GameDb k_gameDb => GameServices.Db;

	private readonly VisualElement k_contentRoot;
	private readonly VisualElement k_headerSection;
	private readonly VisualElement k_statsSection;
	private readonly VisualElement k_comparisonSection;
	private readonly VisualElement k_detailsSection;
	private readonly VisualElement k_flavorSection;

	private bool k_showDetailed = false;
	private bool k_showComparison = false;

	#endregion

	#region Constructor

	public GameItemTooltip() : base() {
		k_contentRoot = new VisualElement {
			style = {
				minWidth = 200,
				maxWidth = 350
			}
		};

		k_headerSection = new VisualElement { name = "header" };
		k_statsSection = new VisualElement { name = "stats" };
		k_comparisonSection = new VisualElement { name = "comparison", style = { display = DisplayStyle.None } };
		k_detailsSection = new VisualElement { name = "details", style = { display = DisplayStyle.None } };
		k_flavorSection = new VisualElement { name = "flavor" };

		k_contentRoot.Add(k_headerSection);
		k_contentRoot.Add(k_statsSection);
		k_contentRoot.Add(k_comparisonSection);
		k_contentRoot.Add(k_detailsSection);
		k_contentRoot.Add(k_flavorSection);

		SetContent(k_contentRoot);

		RegisterCallback<KeyDownEvent>(OnKeyDown);
		RegisterCallback<KeyUpEvent>(OnKeyUp);
	}

	#endregion

	#region Fluent API

	/// <summary>
	/// Sets the item to display.
	/// </summary>
	public GameItemTooltip SetItem(ItemInstance? item) {
		k_item = item;
		RebuildContent();
		return this;
	}

	/// <summary>
	/// Sets the item to compare against (usually the equipped item).
	/// </summary>
	public GameItemTooltip SetComparisonItem(ItemInstance? item) {
		k_comparisonItem = item;
		RebuildContent();
		return this;
	}

	/// <summary>
	/// Forces detailed view.
	/// </summary>
	public GameItemTooltip SetShowDetailed(bool show) {
		k_showDetailed = show;
		UpdateVisibility();
		return this;
	}

	/// <summary>
	/// Forces comparison view.
	/// </summary>
	public GameItemTooltip SetShowComparison(bool show) {
		k_showComparison = show;
		UpdateVisibility();
		return this;
	}

	/// <summary>
	/// Shows the tooltip for a GeneratedItem at the specified screen position.
	/// </summary>
	public void ShowForGeneratedItem(GeneratedItem item, Vector2 screenPosition) {
		if (item == null) {
			Hide();
			return;
		}

		k_item = null;
		k_comparisonItem = null;

		RebuildContentForGeneratedItem(item);

		style.position = Position.Absolute;
		style.left = screenPosition.x;
		style.top = screenPosition.y;

		Show();
	}

	#endregion

	#region Build

	new public GameItemTooltip Build() {
		base.Build();
		RebuildContent();
		return this;
	}

	#endregion

	#region Content Building

	private void RebuildContent() {
		if (k_item == null) {
			k_contentRoot.style.display = DisplayStyle.None;
			return;
		}

		k_contentRoot.style.display = DisplayStyle.Flex;

		BuildHeader();
		BuildStats();
		BuildComparison();
		BuildDetails();
		BuildFlavor();
		UpdateVisibility();
	}

	private void RebuildContentForGeneratedItem(GeneratedItem item) {
		k_contentRoot.style.display = DisplayStyle.Flex;

		k_headerSection.Clear();
		k_statsSection.Clear();
		k_comparisonSection.Clear();
		k_detailsSection.Clear();
		k_flavorSection.Clear();

		var theme = GameTheme.Current;
		var colors = theme.Colors;

		// Header
		var nameLabel = new Label(item.DisplayName) {
			style = {
				fontSize = 14,
				unityFontStyleAndWeight = FontStyle.Bold,
				color = colors.GetRarityColor(item.Rarity),
				marginBottom = 2
			}
		};
		k_headerSection.Add(nameLabel);

		var rarityLabel = new Label(item.Rarity.ToString()) {
			style = {
				fontSize = 11,
				color = colors.GetRarityColor(item.Rarity),
				marginBottom = 4
			}
		};
		k_headerSection.Add(rarityLabel);

		var levelLabel = new Label($"Item Level {item.BaseItemLevel}") {
			style = {
				fontSize = 10,
				color = colors.TextTertiary
			}
		};
		k_headerSection.Add(levelLabel);

		var divider = new VisualElement {
			style = {
				height = 1,
				backgroundColor = colors.SurfaceBorder,
				marginTop = 6,
				marginBottom = 6
			}
		};
		k_headerSection.Add(divider);

		// Stats from the generated item
		var stats = item.GetEquipmentStats();
		foreach (var stat in stats) {
			string displayName = GetStatDisplayName(stat.Target);
			string prefix = stat.Value >= 0 ? "+" : "";
			string suffix = stat.Operation == ModifierOperation.PercentMore ? "%" : "";
			string valueText = $"{prefix}{stat.Value:F0}{suffix}";
			Color valueColor = stat.Value >= 0 ? colors.Success : colors.Error;

			AddStatLine(k_statsSection, displayName, valueText, valueColor);
		}

		// Show stat bonuses from procedural generation
		if (item.StatBonuses.Count > 0) {
			foreach (var (targetId, bonus) in item.StatBonuses) {
				string displayName = GetStatDisplayName(targetId);
				string prefix = bonus >= 0 ? "+" : "";
				AddStatLine(k_statsSection, displayName, $"{prefix}{bonus}", colors.Accent);
			}
		}

		// Enchantments
		if (item.Enchantments.Count > 0) {
			var enchantHeader = new Label("Enchantments:") {
				style = {
					fontSize = 11,
					color = colors.TextSecondary,
					marginTop = 4
				}
			};
			k_statsSection.Add(enchantHeader);

			foreach (var enchantId in item.Enchantments) {
				var enchantLine = new Label($"  ✨ {enchantId.Value}") {
					style = {
						fontSize = 10,
						color = colors.Accent
					}
				};
				k_statsSection.Add(enchantLine);
			}
		}

		// Durability
		if (item.BaseDurability > 0) {
			int totalDur = item.BaseDurability + item.BonusDurability;
			string durText = item.BonusDurability > 0
				? $"{totalDur} (+{item.BonusDurability})"
				: totalDur.ToString();
			AddStatLine(k_statsSection, "Durability", durText, colors.TextSecondary);
		}

		// Value
		AddStatLine(k_flavorSection, "Value", $"{item.FinalBuyPrice} gold", colors.TextTertiary);

		k_comparisonSection.style.display = DisplayStyle.None;
		k_detailsSection.style.display = DisplayStyle.None;
	}

	private void BuildHeader() {
		k_headerSection.Clear();

		if (k_item == null) return;

		var theme = GameTheme.Current;
		var colors = theme.Colors;
		var proto = k_item.Prototype;

		var nameLabel = new Label(k_item.DisplayName) {
			style = {
				fontSize = 14,
				unityFontStyleAndWeight = FontStyle.Bold,
				color = colors.GetRarityColor(proto.Rarity),
				marginBottom = 2
			}
		};
		k_headerSection.Add(nameLabel);

		string typeText = proto.Type.ToString();
		if (proto.EquipSlot.HasValue) {
			typeText += $" ({proto.EquipSlot.Value})";
		}

		var typeLabel = new Label(typeText) {
			style = {
				fontSize = 11,
				color = colors.TextSecondary,
				marginBottom = 4
			}
		};
		k_headerSection.Add(typeLabel);

		var levelLabel = new Label($"Item Level {k_item.EffectiveItemLevel}") {
			style = {
				fontSize = 10,
				color = colors.TextTertiary
			}
		};
		k_headerSection.Add(levelLabel);

		var divider = new VisualElement {
			style = {
				height = 1,
				backgroundColor = colors.SurfaceBorder,
				marginTop = 6,
				marginBottom = 6
			}
		};
		k_headerSection.Add(divider);
	}

	private void BuildStats() {
		k_statsSection.Clear();

		if (k_item == null) return;

		var theme = GameTheme.Current;
		var colors = theme.Colors;
		var proto = k_item.Prototype;

		// Equipment stats
		foreach (var stat in proto.EquipStats) {
			string displayName = GetStatDisplayName(stat.Target);
			AddStatLine(k_statsSection, displayName, FormatStatValue(stat), colors.TextPrimary);
		}

		// Enchantment stats
		foreach (var enchant in k_item.Enchantments) {
			foreach (var stat in enchant.Stats) {
				string displayName = GetStatDisplayName(stat.Target);
				AddStatLine(k_statsSection, $"  {displayName}", FormatStatValue(stat), colors.Accent);
			}
		}

		// Durability
		if (k_item.Durability.HasValue && k_item.MaxDurability.HasValue) {
			string durText = $"{k_item.Durability}/{k_item.MaxDurability}";
			Color durColor = k_item.IsDurabilityLow ? colors.Error : colors.TextSecondary;
			AddStatLine(k_statsSection, "Durability", durText, durColor);
		}
	}

	private void BuildComparison() {
		k_comparisonSection.Clear();

		if (k_item == null) return;

		var compareItem = k_comparisonItem;
		if (compareItem == null && k_inventoryManager != null && k_item.Prototype.EquipSlot.HasValue) {
			compareItem = k_inventoryManager.GetEquippedItem(k_item.Prototype.EquipSlot.Value);
		}

		if (compareItem == null || compareItem == k_item) {
			return;
		}

		var theme = GameTheme.Current;
		var colors = theme.Colors;

		var headerLabel = new Label($"Compared to: {compareItem.DisplayName}") {
			style = {
				fontSize = 11,
				color = colors.TextSecondary,
				marginBottom = 4,
				unityFontStyleAndWeight = FontStyle.Italic
			}
		};
		k_comparisonSection.Add(headerLabel);

		var currentStats = GetItemStats(k_item);
		var equippedStats = GetItemStats(compareItem);

		var allStats = currentStats.Keys.Union(equippedStats.Keys).Distinct();

		foreach (var targetId in allStats) {
			float current = currentStats.GetValueOrDefault(targetId, 0);
			float equipped = equippedStats.GetValueOrDefault(targetId, 0);
			float diff = current - equipped;

			if (Math.Abs(diff) < 0.01f) continue;

			string displayName = GetStatDisplayName(targetId);
			string diffText = diff > 0 ? $"+{diff:F1}" : $"{diff:F1}";
			Color diffColor = diff > 0 ? colors.Success : colors.Error;

			AddStatLine(k_comparisonSection, displayName, diffText, diffColor);
		}
	}

	private void BuildDetails() {
		k_detailsSection.Clear();

		if (k_item == null) return;

		var theme = GameTheme.Current;
		var colors = theme.Colors;
		var proto = k_item.Prototype;

		if (proto.ClassRestrictions.Count > 0) {
			var classes = string.Join(", ", proto.ClassRestrictions.Select(c => c.Value));
			AddStatLine(k_detailsSection, "Classes", classes, colors.Warning);
		}

		if (proto.RequiredLevel > 1) {
			AddStatLine(k_detailsSection, "Required Level", proto.RequiredLevel.ToString(), colors.TextSecondary);
		}

		AddStatLine(k_detailsSection, "Sell Value", $"{proto.SellPrice} gold", colors.TextTertiary);

		if (proto.UseEffects.Count > 0) {
			var effectsLabel = new Label("Use Effects:") {
				style = {
					fontSize = 11,
					color = colors.TextSecondary,
					marginTop = 4
				}
			};
			k_detailsSection.Add(effectsLabel);

			foreach (var effect in proto.UseEffects) {
				var effectLine = new Label($"  • {effect.Type}") {
					style = {
						fontSize = 10,
						color = colors.Accent
					}
				};
				k_detailsSection.Add(effectLine);
			}
		}
	}

	private void BuildFlavor() {
		k_flavorSection.Clear();

		if (k_item == null) return;

		var theme = GameTheme.Current;
		var colors = theme.Colors;
		var proto = k_item.Prototype;

		if (!string.IsNullOrEmpty(proto.DisplayText.Description)) {
			var flavorLabel = new Label(proto.DisplayText.Description) {
				style = {
					fontSize = 10,
					color = colors.TextTertiary,
					unityFontStyleAndWeight = FontStyle.Italic,
					whiteSpace = WhiteSpace.Normal,
					marginTop = 6
				}
			};
			k_flavorSection.Add(flavorLabel);
		}

		var hintLabel = new Label("[Alt] Details  [Shift] Compare") {
			style = {
				fontSize = 9,
				color = colors.TextTertiary.WithAlpha(50.Percent()),
				marginTop = 8,
				unityTextAlign = TextAnchor.MiddleCenter
			}
		};
		k_flavorSection.Add(hintLabel);
	}

	private void AddStatLine(VisualElement container, string label, string value, Color valueColor) {
		var theme = GameTheme.Current;

		var line = new VisualElement {
			style = {
				flexDirection = FlexDirection.Row,
				justifyContent = Justify.SpaceBetween,
				marginBottom = 2
			}
		};

		var labelElement = new Label(label) {
			style = {
				fontSize = 11,
				color = theme.Colors.TextSecondary
			}
		};

		var valueElement = new Label(value) {
			style = {
				fontSize = 11,
				color = valueColor,
				unityFontStyleAndWeight = FontStyle.Bold
			}
		};

		line.Add(labelElement);
		line.Add(valueElement);
		container.Add(line);
	}

	private string GetStatDisplayName(Proto.ID targetId) {
		if (k_gameDb != null && k_gameDb.TryGetProto<StatProto>(new StatProto.ID(targetId.Value), out var statProto)) {
			return statProto.DisplayText.Name;
		}
		// Fallback: extract name from ID
		return targetId.GetName();
	}

	private string FormatStatValue(EquipmentStat stat) {
		string prefix = stat.Value >= 0 ? "+" : "";
		string suffix = stat.Operation == ModifierOperation.PercentMore ? "%" : "";
		return $"{prefix}{stat.Value}{suffix}";
	}

	private Dictionary<Proto.ID, float> GetItemStats(ItemInstance item) {
		var stats = new Dictionary<Proto.ID, float>();

		foreach (var stat in item.Prototype.EquipStats) {
			stats[stat.Target] = stats.GetValueOrDefault(stat.Target, 0) + stat.Value;
		}

		foreach (var enchant in item.Enchantments) {
			foreach (var stat in enchant.Stats) {
				stats[stat.Target] = stats.GetValueOrDefault(stat.Target, 0) + stat.Value;
			}
		}

		return stats;
	}

	private void UpdateVisibility() {
		k_detailsSection.style.display = k_showDetailed ? DisplayStyle.Flex : DisplayStyle.None;
		k_comparisonSection.style.display = k_showComparison ? DisplayStyle.Flex : DisplayStyle.None;
	}

	#endregion

	#region Keyboard Handling

	private void OnKeyDown(KeyDownEvent evt) {
		bool changed = false;

		if (evt.altKey && !k_showDetailed) {
			k_showDetailed = true;
			changed = true;
		}

		if (evt.shiftKey && !k_showComparison) {
			k_showComparison = true;
			changed = true;
		}

		if (changed) {
			UpdateVisibility();
		}
	}

	private void OnKeyUp(KeyUpEvent evt) {
		bool changed = false;

		if (!evt.altKey && k_showDetailed) {
			k_showDetailed = false;
			changed = true;
		}

		if (!evt.shiftKey && k_showComparison) {
			k_showComparison = false;
			changed = true;
		}

		if (changed) {
			UpdateVisibility();
		}
	}

	#endregion
}
using System;
using System.Collections.Generic;
using System.Linq;
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
/// A unified panel for displaying item details.
/// Supports Compact, Summary, and Full view modes with optional expansion.
/// Works with both ItemInstance (inventory items) and GeneratedItem (procedural previews).
/// </summary>
[Obsolete("Use GameItemTooltip instead")]
public class GameItemDetailsPanel : VisualElement {
	#region Private Fields

	private readonly GameDb k_gameDb;

	private ItemInstance? k_itemInstance;
	private GeneratedItem? k_generatedItem;

	private EntityViewMode k_viewMode = EntityViewMode.Compact;
	private ExpandMode k_expandMode = ExpandMode.None;
	private bool k_isExpanded = false;

	// Layout containers
	private VisualElement k_root = null!;
	private VisualElement k_headerRow = null!;
	private VisualElement k_statsSection = null!;
	private VisualElement k_detailsSection = null!;
	private VisualElement k_flavorSection = null!;
	private VisualElement k_expandButtonContainer = null!;

	// Header elements
	private VisualElement k_iconFrame = null!;
	private Label k_iconPlaceholder = null!;
	private GameLabel k_nameLabel = null!;
	private GameBadge k_rarityBadge = null!;
	private GameLabel k_typeLabel = null!;

	// Callbacks
	private Action? k_onExpandClicked;
	private Action? k_onOpenDetailsClicked;

	#endregion

	#region Constructor

	/// <summary>
	/// Creates a new item details panel with dependency injection.
	/// </summary>
	public GameItemDetailsPanel(GameDb gameDb, InventoryManager inventoryManager) {
		k_gameDb = gameDb;

		BuildUI();
		ApplyTheme();

		GameTheme.OnThemeChanged += OnThemeChanged;
	}

	#endregion

	#region Fluent Configuration

	/// <summary>
	/// Sets an ItemInstance to display.
	/// </summary>
	public GameItemDetailsPanel SetItem(ItemInstance? item) {
		k_itemInstance = item;
		k_generatedItem = null;
		RefreshUI();
		return this;
	}

	/// <summary>
	/// Sets a GeneratedItem to display (for previews, spinners, etc.).
	/// </summary>
	public GameItemDetailsPanel SetGeneratedItem(GeneratedItem? item) {
		k_generatedItem = item;
		k_itemInstance = null;
		RefreshUI();
		return this;
	}

	/// <summary>
	/// Sets the view mode (Compact, Summary, Full).
	/// </summary>
	public GameItemDetailsPanel SetViewMode(EntityViewMode mode) {
		k_viewMode = mode;
		UpdateViewMode();
		return this;
	}

	/// <summary>
	/// Sets the expand mode (None, InPlace, NewWindow).
	/// </summary>
	public GameItemDetailsPanel SetExpandMode(ExpandMode mode) {
		k_expandMode = mode;
		UpdateExpandButton();
		return this;
	}

	/// <summary>
	/// Called when the expand button is clicked (InPlace mode).
	/// </summary>
	public GameItemDetailsPanel OnExpandClicked(Action callback) {
		k_onExpandClicked = callback;
		return this;
	}

	/// <summary>
	/// Called when "Open Details" is clicked (NewWindow mode).
	/// </summary>
	public GameItemDetailsPanel OnOpenDetailsClicked(Action callback) {
		k_onOpenDetailsClicked = callback;
		return this;
	}

	/// <summary>
	/// Sets the panel width (overrides style-based width).
	/// </summary>
	public GameItemDetailsPanel SetWidth(float width) {
		style.width = width;
		return this;
	}

	/// <summary>
	/// Builds and returns the panel (fluent terminal).
	/// </summary>
	public GameItemDetailsPanel Build() {
		UpdateViewMode();
		RefreshUI();
		return this;
	}

	#endregion

	#region UI Building

	private void BuildUI() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var colors = theme.Colors;
		var borders = theme.Borders;
		var styles = theme.Components.ItemDetails;

		k_root = new VisualElement {
			name = "item-details-root",
			style = {
				flexDirection = FlexDirection.Column,
				backgroundColor = colors.BackgroundSecondary,
				borderTopLeftRadius = borders.RadiusMD,
				borderTopRightRadius = borders.RadiusMD,
				borderBottomLeftRadius = borders.RadiusMD,
				borderBottomRightRadius = borders.RadiusMD,
				paddingTop = spacing.SM,
				paddingBottom = spacing.SM,
				paddingLeft = spacing.SM,
				paddingRight = spacing.SM,
				minWidth = styles.CompactWidth
			}
		};

		BuildHeaderRow(spacing, colors, styles);
		BuildStatsSection(spacing);
		BuildDetailsSection(spacing);
		BuildFlavorSection(spacing);
		BuildExpandButton(spacing);

		k_root.Add(k_headerRow);
		k_root.Add(k_statsSection);
		k_root.Add(k_detailsSection);
		k_root.Add(k_flavorSection);
		k_root.Add(k_expandButtonContainer);

		Add(k_root);
	}

	private void BuildHeaderRow(SpacingSettings spacing, ColorPalette colors, ItemDetailsPanelStyles styles) {
		k_headerRow = new VisualElement {
			name = "header-row",
			style = {
				flexDirection = FlexDirection.Row,
				alignItems = Align.Center,
				marginBottom = spacing.XS
			}
		};

		// Icon frame - starts with compact size
		k_iconFrame = new VisualElement {
			name = "icon-frame",
			style = {
				width = styles.IconSize,
				height = styles.IconSize,
				backgroundColor = colors.Surface,
				borderTopLeftRadius = 4,
				borderTopRightRadius = 4,
				borderBottomLeftRadius = 4,
				borderBottomRightRadius = 4,
				marginRight = spacing.SM,
				alignItems = Align.Center,
				justifyContent = Justify.Center,
				borderTopWidth = 2,
				borderRightWidth = 2,
				borderBottomWidth = 2,
				borderLeftWidth = 2,
				borderTopColor = colors.SurfaceBorder,
				borderRightColor = colors.SurfaceBorder,
				borderBottomColor = colors.SurfaceBorder,
				borderLeftColor = colors.SurfaceBorder
			}
		};

		k_iconPlaceholder = new Label("?") {
			name = "icon-placeholder",
			style = {
				fontSize = styles.IconSize * 0.4f,
				color = colors.TextTertiary,
				unityTextAlign = TextAnchor.MiddleCenter
			}
		};
		k_iconFrame.Add(k_iconPlaceholder);

		var infoColumn = new VisualElement {
			name = "info-column",
			style = {
				flexDirection = FlexDirection.Column,
				flexGrow = 1
			}
		};

		var nameRow = new VisualElement {
			style = {
				flexDirection = FlexDirection.Row,
				alignItems = Align.Center,
				flexWrap = Wrap.Wrap
			}
		};

		k_nameLabel = new GameLabel("Unknown Item")
			.SetStyle(LabelStyle.TitleSmall)
			.SetColor(LabelColor.Primary)
			.Build();

		k_rarityBadge = new GameBadge()
			.SetText("Common")
			.SetVariant(BadgeVariant.Default)
			.SetSize(BadgeSize.Small)
			.Build();
		k_rarityBadge.style.marginLeft = spacing.XS;

		nameRow.Add(k_nameLabel);
		nameRow.Add(k_rarityBadge);

		k_typeLabel = new GameLabel("Item")
			.SetStyle(LabelStyle.Caption)
			.SetColor(LabelColor.Tertiary)
			.Build();

		infoColumn.Add(nameRow);
		infoColumn.Add(k_typeLabel);

		k_headerRow.Add(k_iconFrame);
		k_headerRow.Add(infoColumn);
	}

	private void BuildStatsSection(SpacingSettings spacing) {
		k_statsSection = new VisualElement {
			name = "stats-section",
			style = {
				flexDirection = FlexDirection.Column,
				marginBottom = spacing.XS
			}
		};
	}

	private void BuildDetailsSection(SpacingSettings spacing) {
		k_detailsSection = new VisualElement {
			name = "details-section",
			style = {
				flexDirection = FlexDirection.Column,
				display = DisplayStyle.None
			}
		};
	}

	private void BuildFlavorSection(SpacingSettings spacing) {
		k_flavorSection = new VisualElement {
			name = "flavor-section",
			style = {
				flexDirection = FlexDirection.Column,
				display = DisplayStyle.None
			}
		};
	}

	private void BuildExpandButton(SpacingSettings spacing) {
		k_expandButtonContainer = new VisualElement {
			name = "expand-button-container",
			style = {
				flexDirection = FlexDirection.Row,
				justifyContent = Justify.Center,
				marginTop = spacing.XS,
				display = DisplayStyle.None
			}
		};
	}

	#endregion

	#region View Mode Management

	private void UpdateViewMode() {
		var effectiveMode = k_isExpanded && k_expandMode == ExpandMode.InPlace
			? GetExpandedMode(k_viewMode)
			: k_viewMode;

		var styles = GameTheme.Current.Components.ItemDetails;

		switch (effectiveMode) {
			case EntityViewMode.Compact:
				ApplyCompactStyles(styles);
				break;
			case EntityViewMode.Summary:
				ApplySummaryStyles(styles);
				break;
			case EntityViewMode.Full:
				ApplyFullStyles(styles);
				break;
		}

		UpdateExpandButton();
	}

	private EntityViewMode GetExpandedMode(EntityViewMode current) {
		return current switch {
			EntityViewMode.Compact => EntityViewMode.Summary,
			EntityViewMode.Summary => EntityViewMode.Full,
			_ => EntityViewMode.Full
		};
	}

	private void ApplyCompactStyles(ItemDetailsPanelStyles styles) {
		// Panel width
		k_root.style.minWidth = styles.CompactWidth;

		// Icon size - compact
		k_iconFrame.style.width = styles.IconSize;
		k_iconFrame.style.height = styles.IconSize;
		k_iconPlaceholder.style.fontSize = styles.IconSize * 0.4f;

		// Visibility
		k_detailsSection.style.display = DisplayStyle.None;
		k_flavorSection.style.display = DisplayStyle.None;

		// Refresh stats with compact limit
		RefreshUI();
	}

	private void ApplySummaryStyles(ItemDetailsPanelStyles styles) {
		// Panel width
		k_root.style.minWidth = styles.SummaryWidth;

		// Icon size - large for summary/full
		k_iconFrame.style.width = styles.IconSizeLarge;
		k_iconFrame.style.height = styles.IconSizeLarge;
		k_iconPlaceholder.style.fontSize = styles.IconSizeLarge * 0.4f;

		// Visibility
		k_detailsSection.style.display = DisplayStyle.Flex;
		k_flavorSection.style.display = DisplayStyle.None;

		// Refresh to show all stats
		RefreshUI();
	}

	private void ApplyFullStyles(ItemDetailsPanelStyles styles) {
		// Panel width
		k_root.style.minWidth = styles.FullWidth;

		// Icon size - large for summary/full
		k_iconFrame.style.width = styles.IconSizeLarge;
		k_iconFrame.style.height = styles.IconSizeLarge;
		k_iconPlaceholder.style.fontSize = styles.IconSizeLarge * 0.4f;

		// Visibility
		k_detailsSection.style.display = DisplayStyle.Flex;
		k_flavorSection.style.display = DisplayStyle.Flex;

		// Refresh to show all stats
		RefreshUI();
	}

	private void UpdateExpandButton() {
		k_expandButtonContainer.Clear();

		switch (k_expandMode) {
			case ExpandMode.None:
				k_expandButtonContainer.style.display = DisplayStyle.None;
				break;

			case ExpandMode.InPlace:
				k_expandButtonContainer.style.display = DisplayStyle.Flex;
				var expandIcon = k_isExpanded ? "▲ Collapse" : "▼ Expand";
				var inPlaceBtn = new GameButton(expandIcon)
					.SetVariant(ButtonVariant.Ghost)
					.SetSize(ButtonSize.Small)
					.OnClick(OnInPlaceExpandClicked)
					.Build();
				k_expandButtonContainer.Add(inPlaceBtn);
				break;

			case ExpandMode.NewWindow:
				k_expandButtonContainer.style.display = DisplayStyle.Flex;
				var openBtn = new GameButton("📋 Open Details")
					.SetVariant(ButtonVariant.Ghost)
					.SetSize(ButtonSize.Small)
					.OnClick(() => k_onOpenDetailsClicked?.Invoke())
					.Build();
				k_expandButtonContainer.Add(openBtn);
				break;
		}
	}

	private void OnInPlaceExpandClicked() {
		k_isExpanded = !k_isExpanded;
		UpdateViewMode();
		k_onExpandClicked?.Invoke();
	}

	#endregion

	#region UI Refresh

	/// <summary>
	/// Refreshes all displayed data.
	/// </summary>
	public void RefreshUI() {
		if (k_itemInstance != null) {
			RefreshFromItemInstance();
		} else if (k_generatedItem != null) {
			RefreshFromGeneratedItem();
		} else {
			ClearDisplay();
		}
	}

	private void RefreshFromItemInstance() {
		if (k_itemInstance == null) return;

		var colors = GameTheme.Current.Colors;
		var proto = k_itemInstance.Prototype;

		k_nameLabel.SetText(k_itemInstance.DisplayName);
		k_nameLabel.style.color = colors.GetRarityColor(proto.Rarity);

		k_rarityBadge.SetText(proto.Rarity.ToString());
		k_rarityBadge.SetVariant(GetRarityBadgeVariant(proto.Rarity));

		k_typeLabel.SetText(FormatItemType(proto));

		SetIconFrameRarity(proto.Rarity);
		k_iconPlaceholder.text = GetItemInitials(k_itemInstance.DisplayName);

		RefreshStats(k_itemInstance.GetAllEquipmentModifiers().ToList());
		RefreshDetails(proto.RequiredLevel, proto.ClassRestrictions.Select(c => c.Value).ToList(),
			k_itemInstance.Durability, k_itemInstance.MaxDurability);
		RefreshFlavor(proto.DisplayText.Description, proto.SellPrice);
	}

	private void RefreshFromGeneratedItem() {
		if (k_generatedItem == null) return;

		var colors = GameTheme.Current.Colors;

		k_nameLabel.SetText(k_generatedItem.DisplayName);
		k_nameLabel.style.color = colors.GetRarityColor(k_generatedItem.Rarity);

		k_rarityBadge.SetText(k_generatedItem.Rarity.ToString());
		k_rarityBadge.SetVariant(GetRarityBadgeVariant(k_generatedItem.Rarity));

		string typeText = k_generatedItem switch {
			GeneratedWeapon w => $"Weapon • {w.BaseProto.WeaponType}",
			GeneratedArmor a => $"Armor • {a.BaseProto.ArmorType}",
			_ => "Item"
		};
		k_typeLabel.SetText(typeText);

		SetIconFrameRarity(k_generatedItem.Rarity);
		k_iconPlaceholder.text = GetItemInitials(k_generatedItem.DisplayName);

		RefreshStats(k_generatedItem.GetEquipmentStats());

		int durability = k_generatedItem.BaseDurability + k_generatedItem.BonusDurability;
		RefreshDetails(0, [], durability, durability);

		RefreshFlavor(null, k_generatedItem.FinalBuyPrice);
	}

	private void RefreshStats(List<EquipmentStat> stats) {
		k_statsSection.Clear();

		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var colors = theme.Colors;
		var styles = theme.Components.ItemDetails;

		// Determine effective mode for stat limiting
		var effectiveMode = k_isExpanded && k_expandMode == ExpandMode.InPlace
			? GetExpandedMode(k_viewMode)
			: k_viewMode;

		// Only limit stats in compact mode
		int maxStats = effectiveMode == EntityViewMode.Compact ? styles.CompactMaxStats : stats.Count;

		foreach (var stat in stats.Take(maxStats)) {
			var row = new VisualElement {
				style = {
					flexDirection = FlexDirection.Row,
					justifyContent = Justify.SpaceBetween,
					marginBottom = spacing.XXS
				}
			};

			string statName = GetStatDisplayName(stat.Target);
			var nameLabel = new GameLabel(statName)
				.SetStyle(LabelStyle.Caption)
				.SetColor(LabelColor.Secondary)
				.Build();

			string valueText = FormatStatValue(stat);
			var valueLabel = new GameLabel(valueText)
				.SetStyle(LabelStyle.LabelSmall)
				.Build();
			valueLabel.style.color = stat.Value >= 0 ? colors.Success : colors.Error;

			row.Add(nameLabel);
			row.Add(valueLabel);
			k_statsSection.Add(row);
		}

		// Show "+X more" indicator in compact mode if there are hidden stats
		if (effectiveMode == EntityViewMode.Compact && stats.Count > maxStats) {
			var moreLabel = new GameLabel($"+{stats.Count - maxStats} more")
				.SetStyle(LabelStyle.Caption)
				.SetColor(LabelColor.Tertiary)
				.Build();
			k_statsSection.Add(moreLabel);
		}
	}

	private void RefreshDetails(int requiredLevel, List<string> classRestrictions, int? durability, int? maxDurability) {
		k_detailsSection.Clear();

		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var colors = theme.Colors;

		if (requiredLevel > 1) {
			AddDetailRow(k_detailsSection, "Required Level", requiredLevel.ToString(), colors.TextSecondary);
		}

		if (classRestrictions.Count > 0) {
			AddDetailRow(k_detailsSection, "Classes", string.Join(", ", classRestrictions), colors.Warning);
		}

		if (durability.HasValue && maxDurability.HasValue) {
			float percent = maxDurability.Value > 0 ? (float)durability.Value / maxDurability.Value : 1f;
			Color durColor = percent < 0.25f ? colors.Error : (percent < 0.5f ? colors.Warning : colors.TextSecondary);
			AddDetailRow(k_detailsSection, "Durability", $"{durability}/{maxDurability}", durColor);
		}
	}

	private void RefreshFlavor(string? description, int sellPrice) {
		k_flavorSection.Clear();

		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var colors = theme.Colors;

		if (!string.IsNullOrEmpty(description)) {
			var descLabel = new GameLabel(description)
				.SetStyle(LabelStyle.Caption)
				.SetColor(LabelColor.Tertiary)
				.Build();
			descLabel.style.whiteSpace = WhiteSpace.Normal;
			descLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
			descLabel.style.marginBottom = spacing.XS;
			k_flavorSection.Add(descLabel);
		}

		AddDetailRow(k_flavorSection, "Sell Value", $"{sellPrice} gold", colors.TextTertiary);
	}

	private void ClearDisplay() {
		k_nameLabel.SetText("No Item");
		k_typeLabel.SetText("");
		k_rarityBadge.SetText("");
		k_statsSection.Clear();
		k_detailsSection.Clear();
		k_flavorSection.Clear();
	}

	private void AddDetailRow(VisualElement container, string label, string value, Color valueColor) {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		var row = new VisualElement {
			style = {
				flexDirection = FlexDirection.Row,
				justifyContent = Justify.SpaceBetween,
				marginBottom = spacing.XXS
			}
		};

		var labelElement = new GameLabel(label)
			.SetStyle(LabelStyle.Caption)
			.SetColor(LabelColor.Tertiary)
			.Build();

		var valueElement = new GameLabel(value)
			.SetStyle(LabelStyle.LabelSmall)
			.Build();
		valueElement.style.color = valueColor;

		row.Add(labelElement);
		row.Add(valueElement);
		container.Add(row);
	}

	#endregion

	#region Helpers

	private void SetIconFrameRarity(RarityType rarity) {
		var colors = GameTheme.Current.Colors;
		var rarityColor = colors.GetRarityColor(rarity);

		k_iconFrame.style.borderTopColor = rarityColor;
		k_iconFrame.style.borderRightColor = rarityColor;
		k_iconFrame.style.borderBottomColor = rarityColor;
		k_iconFrame.style.borderLeftColor = rarityColor;
	}

	private string GetStatDisplayName(Proto.ID targetId) {
		if (k_gameDb.TryGetProto<StatProto>(new StatProto.ID(targetId.Value), out var statProto)) {
			return statProto.DisplayText.Name;
		}
		return targetId.GetName();
	}

	private string FormatStatValue(EquipmentStat stat) {
		string prefix = stat.Value >= 0 ? "+" : "";
		string suffix = stat.Operation == ModifierOperation.PercentMore ? "%" : "";
		return $"{prefix}{stat.Value:F0}{suffix}";
	}

	private string FormatItemType(Core.Prototypes.Item.ItemProto proto) {
		string typeText = proto.Type.ToString();
		if (proto.EquipSlot.HasValue) {
			typeText += $" • {proto.EquipSlot.Value}";
		}
		return typeText;
	}

	private string GetItemInitials(string name) {
		if (string.IsNullOrEmpty(name)) return "?";

		var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
		if (words.Length == 1) {
			return words[0].Length > 1 ? words[0][..2].ToUpper() : words[0].ToUpper();
		}

		return string.Concat(words.Take(2).Select(w => char.ToUpper(w[0])));
	}

	private BadgeVariant GetRarityBadgeVariant(RarityType rarity) {
		return rarity switch {
			RarityType.Common => BadgeVariant.Default,
			RarityType.Uncommon => BadgeVariant.Success,
			RarityType.Rare => BadgeVariant.Info,
			RarityType.Epic => BadgeVariant.Warning,
			RarityType.Legendary => BadgeVariant.Error,
			RarityType.Mythic => BadgeVariant.Error,
			_ => BadgeVariant.Default
		};
	}

	#endregion

	#region Theme

	private void ApplyTheme() {
		var theme = GameTheme.Current;
		var colors = theme.Colors;
		k_root.style.backgroundColor = colors.BackgroundSecondary;
	}

	private void OnThemeChanged(GameTheme theme) {
		ApplyTheme();
		UpdateViewMode(); // Re-apply view mode styles
	}

	#endregion

	#region Cleanup

	public new void RemoveFromHierarchy() {
		GameTheme.OnThemeChanged -= OnThemeChanged;
		base.RemoveFromHierarchy();
	}

	#endregion
}
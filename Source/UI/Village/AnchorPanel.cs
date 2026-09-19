using RPGGame.Core.Simulation;
using RPGGame.Core.Village;
using RPGGame.UI.Components;
using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Village;

/// <summary>
/// UI panel displayed when clicking The Anchor.
/// Shows meta progression, lore, and world upgrades.
/// </summary>
public class AnchorPanel : VisualElement {
	#region Fields

	private readonly VillageManager k_villageManager;
	private readonly GameStateManager k_stateManager;

	private GamePanel k_content = null!;
	private GameTabView k_tabs = null!;

	#endregion

	#region Events

	public event Action? OnClosed;

	#endregion

	#region Constructor

	public AnchorPanel(VillageManager villageManager, GameStateManager stateManager) {
		k_villageManager = villageManager;
		k_stateManager = stateManager;

		BuildUI();
		style.display = DisplayStyle.None;
	}

	#endregion

	#region UI Building

	private void BuildUI() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var colors = theme.Colors;

		// Full overlay
		style.position = Position.Absolute;
		style.left = 0;
		style.top = 0;
		style.right = 0;
		style.bottom = 0;
		style.alignItems = Align.Center;
		style.justifyContent = Justify.Center;
		style.backgroundColor = colors.BackgroundOverlay;

		// Panel container
		k_content = new GamePanel()
			.SetVariant(PanelVariant.Card)
			.SetPadding(spacing.XL)
			.Build();
		k_content.style.width = theme.Components.Dialog.MaxWidth;
		k_content.style.maxHeight = Length.Percent(85);

		// Header
		var header = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetLayout(LayoutDirection.Horizontal)
			.SetJustify(Justify.SpaceBetween)
			.SetAlignment(Align.Center)
			.Build();

		var titlePanel = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetAlignment(Align.Center)
			.Build();

		var titleLabel = new GameLabel("✦ The Anchor ✦")
			.SetStyle(LabelStyle.HeadlineLarge)
			.SetColor(LabelColor.Primary)
			.Build();

		var subtitleLabel = new GameLabel("The light that holds back the fog")
			.SetStyle(LabelStyle.BodySmall)
			.SetColor(LabelColor.Tertiary)
			.Build();

		titlePanel.Content.Add(titleLabel);
		titlePanel.Content.Add(subtitleLabel);

		var closeBtn = new GameButton("✕")
			.SetVariant(ButtonVariant.Ghost)
			.SetSize(ButtonSize.Small)
			.OnClick(Close)
			.Build();

		header.Content.Add(titlePanel);
		header.Content.Add(closeBtn);

		var divider = new GameDivider().Build();
		divider.style.marginTop = spacing.MD;
		divider.style.marginBottom = spacing.MD;

		// Tabs
		k_tabs = new GameTabView()
			.AddTab("Statistics", CreateStatsTab())
			.AddTab("Lore", CreateLoreTab())
			.AddTab("Bonuses", CreateBonusesTab())
			.SetVariant(TabVariant.Default)
			.Build();

		// Close button
		var closeButton = new GameButton("Close")
			.SetVariant(ButtonVariant.Outline)
			.SetFullWidth()
			.OnClick(Close)
			.Build();
		closeButton.style.marginTop = spacing.MD;

		k_content.Content.Add(header);
		k_content.Content.Add(divider);
		k_content.Content.Add(k_tabs);
		k_content.Content.Add(closeButton);

		Add(k_content);

		// Close on background click
		RegisterCallback<ClickEvent>(evt => {
			if (evt.target == this) {
				Close();
			}
		});
	}

	private VisualElement CreateStatsTab() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		var container = new ScrollView(ScrollViewMode.Vertical);
		container.style.maxHeight = theme.Components.Dialog.MaxWidth * 0.6f;

		var meta = k_stateManager.MetaProgression;

		// Stats grid
		var statsGrid = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetLayout(LayoutDirection.Horizontal)
			.SetWrap(true)
			.Build();

		AddStatCard(statsGrid, "🔄", "Total Runs", meta.TotalRuns.ToString());
		AddStatCard(statsGrid, "🌫", "Fog Clears", meta.FogClears.ToString());
		AddStatCard(statsGrid, "⬆", "Upgrade Points", meta.VillageUpgradePoints.ToString());
		AddStatCard(statsGrid, "🗺", "Tiles Revealed", meta.PermanentlyRevealedTiles.Count.ToString());
		AddStatCard(statsGrid, "📜", "Lore Found", meta.DiscoveredLore.Count.ToString());
		AddStatCard(statsGrid, "⚔", "Classes Unlocked", meta.UnlockedClasses.Count.ToString());

		container.Add(statsGrid);

		// True ending badge
		if (meta.TrueEndingAchieved) {
			var trueBadge = new GameBadge("✦ TRUE ENDING ACHIEVED ✦")
				.SetVariant(BadgeVariant.Legendary)
				.SetPulse()
				.Build();
			trueBadge.style.marginTop = spacing.MD;
			trueBadge.style.alignSelf = Align.Center;
			container.Add(trueBadge);
		}

		return container;
	}

	private void AddStatCard(GamePanel container, string icon, string label, string value) {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		var card = new GameCard()
			.SetVariant(CardVariant.Outlined)
			.Build();
		card.style.width = Length.Percent(48);
		card.style.marginRight = Length.Percent(2);
		card.style.marginBottom = spacing.XS;

		var content = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetLayout(LayoutDirection.Horizontal)
			.SetAlignment(Align.Center)
			.Build();

		var iconLabel = new GameLabel(icon)
			.SetStyle(LabelStyle.DisplaySmall)
			.Build();

		var textContainer = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.Build();
		textContainer.style.marginLeft = spacing.SM;

		var valueLabel = new GameLabel(value)
			.SetStyle(LabelStyle.HeadlineMedium)
			.SetColor(LabelColor.Primary)
			.Build();

		var nameLabel = new GameLabel(label)
			.SetStyle(LabelStyle.Caption)
			.SetColor(LabelColor.Tertiary)
			.Build();

		textContainer.Content.Add(valueLabel);
		textContainer.Content.Add(nameLabel);

		content.Content.Add(iconLabel);
		content.Content.Add(textContainer);

		card.Content.Add(content);
		container.Content.Add(card);
	}

	private VisualElement CreateLoreTab() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var colors = theme.Colors;

		var container = new ScrollView(ScrollViewMode.Vertical);
		container.style.maxHeight = theme.Components.Dialog.MaxWidth * 0.6f;

		var anchorResult = k_villageManager.InteractWithAnchor();

		if (!anchorResult.Success || anchorResult.LoreHints.Count == 0) {
			var emptyPanel = new GamePanel()
				.SetVariant(PanelVariant.Ghost)
				.SetAlignment(Align.Center)
				.Build();
			emptyPanel.style.paddingTop = spacing.XXXL;
			emptyPanel.style.paddingBottom = spacing.XXXL;

			var emptyLabel = new GameLabel("The Anchor is silent... for now.")
				.SetStyle(LabelStyle.BodyMedium)
				.SetColor(LabelColor.Tertiary)
				.Build();
			emptyPanel.Content.Add(emptyLabel);

			container.Add(emptyPanel);
		} else {
			foreach (var hint in anchorResult.LoreHints) {
				var hintCard = new GameCard()
					.SetVariant(CardVariant.Outlined)
					.Build();
				hintCard.style.marginBottom = spacing.XS;

				var hintLabel = new GameLabel($"「 {hint} 」")
					.SetStyle(LabelStyle.BodyMedium)
					.SetColor(LabelColor.Secondary)
					.Build();
				hintCard.Content.Add(hintLabel);

				container.Add(hintCard);
			}
		}

		return container;
	}

	private VisualElement CreateBonusesTab() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		var container = new ScrollView(ScrollViewMode.Vertical);
		container.style.maxHeight = theme.Components.Dialog.MaxWidth * 0.6f;

		var meta = k_stateManager.MetaProgression;
		var modifiers = meta.GetAllMetaModifiers();

		if (modifiers.Count == 0) {
			var emptyPanel = new GamePanel()
				.SetVariant(PanelVariant.Ghost)
				.SetAlignment(Align.Center)
				.Build();
			emptyPanel.style.paddingTop = spacing.XXXL;
			emptyPanel.style.paddingBottom = spacing.XXXL;

			var emptyLabel = new GameLabel("No permanent bonuses yet.")
				.SetStyle(LabelStyle.BodyMedium)
				.SetColor(LabelColor.Tertiary)
				.Build();

			var hintLabel = new GameLabel("Build and upgrade your village!")
				.SetStyle(LabelStyle.BodySmall)
				.SetColor(LabelColor.Tertiary)
				.Build();

			emptyPanel.Content.Add(emptyLabel);
			emptyPanel.Content.Add(hintLabel);

			container.Add(emptyPanel);
		} else {
			var bonusPanel = new GamePanel()
				.SetVariant(PanelVariant.Ghost)
				.SetLayout(LayoutDirection.Horizontal)
				.SetWrap(true)
				.Build();

			foreach (var mod in modifiers) {
				// Use StatId instead of Stat
				string statName = ExtractStatDisplayName(mod.StatId.Value);
				var sign = mod.Value >= 0 ? "+" : "";
				var badge = new GameBadge($"{sign}{mod.Value} {statName}")
					.SetVariant(BadgeVariant.Success)
					.SetShape(BadgeShape.Tag)
					.Build();
				badge.AddTooltip(mod.Source, TooltipPosition.Top);
				badge.style.marginRight = spacing.XXS;
				badge.style.marginBottom = spacing.XXS;
				bonusPanel.Content.Add(badge);
			}

			container.Add(bonusPanel);
		}

		return container;
	}

	#endregion

	#region Helper Methods

	private string ExtractStatDisplayName(string idValue) {
		var parts = idValue.Split('_');
		return parts.Length >= 2 ? parts[^1] : idValue;
	}

	#endregion

	#region Public Methods

	public void Show() {
		style.display = DisplayStyle.Flex;
	}

	public void Close() {
		style.display = DisplayStyle.None;
		OnClosed?.Invoke();
	}

	#endregion
}
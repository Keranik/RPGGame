using RPGGame.Core.Simulation;
using RPGGame.Core.Village;
using RPGGame.UI.Components;
using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Village;

/// <summary>
/// UI panel displayed when clicking on a building in the village.
/// Shows building info, services, and upgrade options.
/// </summary>
public class BuildingPanel : VisualElement {
	#region Fields

	private readonly VillageManager k_villageManager;
	private readonly GameStateManager k_stateManager;
	private readonly UiManager k_uiManager;

	private Building? k_currentBuilding;

	private GamePanel k_content = null!;
	private GamePanel k_headerSection = null!;
	private GamePanel k_servicesSection = null!;
	private GamePanel k_upgradeSection = null!;
	private GamePanel k_bonusesSection = null!;

	#endregion

	#region Events

	public event Action? OnClosed;
	public event Action<Building, string>? OnServiceUsed;

	#endregion

	#region Constructor

	public BuildingPanel(
		VillageManager villageManager,
		GameStateManager stateManager,
		UiManager uiManager
	) {
		k_villageManager = villageManager;
		k_stateManager = stateManager;
		k_uiManager = uiManager;

		BuildUI();
		style.display = DisplayStyle.None;
	}

	#endregion

	#region UI Building

	private void BuildUI() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var colors = theme.Colors;
		var dialogStyles = theme.Components.Dialog;

		// Full overlay
		style.position = Position.Absolute;
		style.left = 0;
		style.top = 0;
		style.right = 0;
		style.bottom = 0;
		style.alignItems = Align.Center;
		style.justifyContent = Justify.Center;
		style.backgroundColor = colors.BackgroundOverlay;

		// Main panel container
		k_content = new GamePanel()
			.SetVariant(PanelVariant.Card)
			.SetPadding(spacing.XL)
			.Build();
		k_content.style.width = dialogStyles.MinWidth;
		k_content.style.maxHeight = Length.Percent(80);

		// Header section
		k_headerSection = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.Build();
		k_headerSection.style.marginBottom = spacing.MD;

		// Services section
		k_servicesSection = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetHeader("Services")
			.Build();
		k_servicesSection.style.marginBottom = spacing.MD;

		// Upgrade section
		k_upgradeSection = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetHeader("Upgrade")
			.Build();
		k_upgradeSection.style.marginBottom = spacing.MD;

		// Bonuses section
		k_bonusesSection = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetHeader("Current Bonuses")
			.Build();
		k_bonusesSection.style.marginBottom = spacing.MD;

		// Close button
		var closeButton = new GameButton("Close")
			.SetVariant(ButtonVariant.Outline)
			.SetFullWidth()
			.OnClick(Close)
			.Build();

		k_content.Content.Add(k_headerSection);
		k_content.Content.Add(new GameDivider().Build());
		k_content.Content.Add(k_servicesSection);
		k_content.Content.Add(k_upgradeSection);
		k_content.Content.Add(k_bonusesSection);
		k_content.Content.Add(closeButton);

		Add(k_content);

		// Close on background click
		RegisterCallback<ClickEvent>(evt => {
			if (evt.target == this) {
				Close();
			}
		});
	}

	#endregion

	#region Public Methods

	public void Show(Building building) {
		k_currentBuilding = building;
		Refresh();
		style.display = DisplayStyle.Flex;
	}

	public void Close() {
		style.display = DisplayStyle.None;
		k_currentBuilding = null;
		OnClosed?.Invoke();
	}

	public void Refresh() {
		if (k_currentBuilding == null) return;

		RefreshHeader();
		RefreshServices();
		RefreshUpgrade();
		RefreshBonuses();
	}

	#endregion

	#region Content Refresh

	private void RefreshHeader() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		k_headerSection.ClearChildren();
		if (k_currentBuilding == null) return;

		// Header row with name and level badge
		var headerRow = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetLayout(LayoutDirection.Horizontal)
			.SetJustify(Justify.SpaceBetween)
			.SetAlignment(Align.Center)
			.Build();

		var nameLabel = new GameLabel(k_currentBuilding.DisplayName)
			.SetStyle(LabelStyle.HeadlineMedium)
			.SetColor(LabelColor.Primary)
			.Build();

		var levelBadge = new GameBadge($"Lv {k_currentBuilding.Level}/{k_currentBuilding.Proto.MaxLevel}")
			.SetVariant(BadgeVariant.Primary)
			.SetShape(BadgeShape.Standard)
			.Build();

		headerRow.Content.Add(nameLabel);
		headerRow.Content.Add(levelBadge);

		// Description
		var descLabel = new GameLabel(k_currentBuilding.Description)
			.SetStyle(LabelStyle.BodyMedium)
			.SetColor(LabelColor.Secondary)
			.SetWhiteSpace(WhiteSpace.Normal)
			.Build();
		descLabel.style.marginTop = spacing.XS;

		k_headerSection.AddChild(headerRow);
		k_headerSection.AddChild(descLabel);
	}

	private void RefreshServices() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		k_servicesSection.ClearChildren();
		if (k_currentBuilding == null) return;

		var services = k_currentBuilding.GetAvailableServices().ToList();
		
		if (services.Count == 0) {
			k_servicesSection.style.display = DisplayStyle.None;
			return;
		}
		
		k_servicesSection.style.display = DisplayStyle.Flex;

		foreach (var service in services) {
			var serviceCard = new GameCard()
				.SetHeader(service.Name)
				.SetSubheader(service.Description)
				.SetVariant(CardVariant.Outlined)
				.SetClickable(() => UseService(service.Id))
				.Build();
			serviceCard.style.marginBottom = spacing.XS;
			k_servicesSection.AddChild(serviceCard);
		}
	}

	private void RefreshUpgrade() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		k_upgradeSection.ClearChildren();
		if (k_currentBuilding == null) return;

		if (!k_currentBuilding.CanUpgrade) {
			var maxBadge = new GameBadge("Maximum Level Reached")
				.SetVariant(BadgeVariant.Success)
				.Build();
			k_upgradeSection.AddChild(maxBadge);
			return;
		}

		var cost = k_currentBuilding.UpgradeCost;
		var points = k_villageManager.UpgradePoints;
		var canAfford = points >= cost;

		// Cost panel
		var costPanel = new GamePanel()
			.SetVariant(PanelVariant.Outlined)
			.SetLayout(LayoutDirection.Horizontal)
			.SetJustify(Justify.SpaceBetween)
			.SetPadding(spacing.SM)
			.Build();

		var costLabel = new GameLabel($"Cost: {cost} points")
			.SetStyle(LabelStyle.BodyMedium)
			.SetColor(canAfford ? LabelColor.Primary : LabelColor.Error)
			.Build();

		var availableLabel = new GameLabel($"Available: {points}")
			.SetStyle(LabelStyle.BodySmall)
			.SetColor(LabelColor.Secondary)
			.Build();

		costPanel.Content.Add(costLabel);
		costPanel.Content.Add(availableLabel);

		// Upgrade button
		var upgradeButton = new GameButton($"Upgrade to Level {k_currentBuilding.Level + 1}")
			.SetVariant(canAfford ? ButtonVariant.Success : ButtonVariant.Ghost)
			.SetFullWidth()
			.SetEnabled(canAfford)
			.OnClick(UpgradeBuilding)
			.Build();
		upgradeButton.style.marginTop = spacing.XS;

		k_upgradeSection.AddChild(costPanel);
		k_upgradeSection.AddChild(upgradeButton);
	}

	private void RefreshBonuses() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		k_bonusesSection.ClearChildren();
		if (k_currentBuilding == null) return;

		var bonuses = k_currentBuilding.GetCurrentBonuses().ToList();
		
		if (bonuses.Count == 0) {
			k_bonusesSection.style.display = DisplayStyle.None;
			return;
		}
		
		k_bonusesSection.style.display = DisplayStyle.Flex;

		var bonusPanel = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetLayout(LayoutDirection.Horizontal)
			.SetWrap(true)
			.Build();

		foreach (var bonus in bonuses) {
			// Use StatId instead of Stat, and extract display name
			string statName = ExtractStatDisplayName(bonus.StatId.Value);
			var sign = bonus.Value >= 0 ? "+" : "";
			var bonusBadge = new GameBadge($"{sign}{bonus.Value} {statName}")
				.SetVariant(BadgeVariant.Success)
				.SetShape(BadgeShape.Tag)
				.Build();
			bonusBadge.style.marginRight = spacing.XXS;
			bonusBadge.style.marginBottom = spacing.XXS;
			bonusPanel.Content.Add(bonusBadge);
		}

		k_bonusesSection.AddChild(bonusPanel);
	}

	private string ExtractStatDisplayName(string idValue) {
		var parts = idValue.Split('_');
		return parts.Length >= 2 ? parts[^1] : idValue;
	}

	#endregion

	#region Actions

	private void UseService(string serviceId) {
		if (k_currentBuilding == null) return;

		k_villageManager.UseService(k_currentBuilding, serviceId);
		OnServiceUsed?.Invoke(k_currentBuilding, serviceId);

		GameToast.Show($"Used {serviceId} at {k_currentBuilding.DisplayName}", ToastType.Info);
	}

	private void UpgradeBuilding() {
		if (k_currentBuilding == null) return;

		var result = k_villageManager.UpgradeBuilding(k_currentBuilding.InstanceId);

		if (result.Success) {
			GameToast.Show(result.Message, ToastType.Success);
			Refresh();
		} else {
			GameToast.Show(result.Message, ToastType.Error);
		}
	}

	#endregion
}
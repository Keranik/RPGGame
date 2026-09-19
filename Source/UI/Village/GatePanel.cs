using System.Diagnostics;
using RPGGame.Core;
using RPGGame.Core.Prototypes.Stats;
using RPGGame.Core.Simulation;
using RPGGame.Core.Stats;
using RPGGame.Core.Village;
using RPGGame.UI.Components;
using RPGGame.UI.Styles;
using UnityEngine.UIElements;

namespace RPGGame.UI.Village;

/// <summary>
/// UI panel displayed when clicking the village gate.
/// Shows departure confirmation and pre-expedition info.
/// </summary>
public class GatePanel : VisualElement {
	#region Fields

	private readonly VillageManager k_villageManager;
	private readonly GameStateManager k_stateManager;
	private readonly GameSession k_session;

	private GamePanel k_content = null!;
	private GamePanel k_warningsSection = null!;
	private GamePanel k_suppliesSection = null!;

	#endregion

	#region Events

	public event Action? OnClosed;
	public event Action? OnDepartureConfirmed;

	#endregion

	#region Constructor

	public GatePanel(
		VillageManager villageManager,
		GameStateManager stateManager,
		GameSession session
	) {
		k_villageManager = villageManager;
		k_stateManager = stateManager;
		k_session = session;

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

		// Panel container
		k_content = new GamePanel()
			.SetVariant(PanelVariant.Card)
			.SetPadding(spacing.XL)
			.Build();
		k_content.style.width = dialogStyles.MinWidth;

		// Header
		var header = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetAlignment(Align.Center)
			.Build();

		var titleLabel = new GameLabel("⛩ Leave Village")
			.SetStyle(LabelStyle.HeadlineMedium)
			.SetColor(LabelColor.Primary)
			.Build();

		var subtitleLabel = new GameLabel("Are you ready to venture into the fog?")
			.SetStyle(LabelStyle.BodyMedium)
			.SetColor(LabelColor.Secondary)
			.Build();
		subtitleLabel.style.marginTop = spacing.XXS;

		header.Content.Add(titleLabel);
		header.Content.Add(subtitleLabel);

		var divider = new GameDivider().Build();
		divider.style.marginTop = spacing.MD;
		divider.style.marginBottom = spacing.MD;

		// Warnings section
		k_warningsSection = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.Build();
		k_warningsSection.style.marginBottom = spacing.MD;

		// Supplies section
		k_suppliesSection = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetHeader("Current Supplies")
			.Build();
		k_suppliesSection.style.marginBottom = spacing.XL;

		// Action buttons
		var buttonPanel = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetLayout(LayoutDirection.Horizontal)
			.SetJustify(Justify.SpaceBetween)
			.Build();

		var cancelButton = new GameButton("Not Yet")
			.SetVariant(ButtonVariant.Outline)
			.OnClick(Close)
			.Build();
		cancelButton.style.flexGrow = 1;
		cancelButton.style.marginRight = spacing.XS;

		var departButton = new GameButton("⚔ Depart")
			.SetVariant(ButtonVariant.Primary)
			.OnClick(ConfirmDeparture)
			.Build();
		departButton.style.flexGrow = 1;

		buttonPanel.Content.Add(cancelButton);
		buttonPanel.Content.Add(departButton);

		k_content.Content.Add(header);
		k_content.Content.Add(divider);
		k_content.Content.Add(k_warningsSection);
		k_content.Content.Add(k_suppliesSection);
		k_content.Content.Add(buttonPanel);

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

	public void Show() {
		Refresh();
		style.display = DisplayStyle.Flex;
	}

	public void Close() {
		style.display = DisplayStyle.None;
		OnClosed?.Invoke();
	}

	public void Refresh() {
		RefreshWarnings();
		RefreshSupplies();
	}

	#endregion

	#region Content

	private void RefreshWarnings() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var borders = theme.Borders;
		var colors = theme.Colors;

		k_warningsSection.ClearChildren();

		var runState = k_session.CurrentRun;
		if (runState == null) return;

		var departureInfo = k_villageManager.GetDepartureInfo(runState);

		// Warnings
		if (departureInfo.Warnings.Count > 0) {
			var warningsPanel = new GamePanel()
				.SetVariant(PanelVariant.Outlined)
				.SetPadding(spacing.SM)
				.Build();
			warningsPanel.style.borderLeftColor = colors.Warning;
			warningsPanel.style.borderLeftWidth = borders.WidthThick;

			var warningTitle = new GameLabel("⚠ Warnings")
				.SetStyle(LabelStyle.TitleSmall)
				.SetColor(LabelColor.Warning)
				.Build();
			warningTitle.style.marginBottom = spacing.XS;
			warningsPanel.Content.Add(warningTitle);

			foreach (var warning in departureInfo.Warnings) {
				var warningLabel = new GameLabel($"• {warning}")
					.SetStyle(LabelStyle.BodySmall)
					.SetColor(LabelColor.Warning)
					.Build();
				warningsPanel.Content.Add(warningLabel);
			}

			k_warningsSection.AddChild(warningsPanel);
		}

		// Suggestions
		if (departureInfo.Suggestions.Count > 0) {
			var suggestPanel = new GamePanel()
				.SetVariant(PanelVariant.Ghost)
				.Build();
			suggestPanel.style.marginTop = spacing.SM;

			var suggestTitle = new GameLabel("💡 Suggestions")
				.SetStyle(LabelStyle.TitleSmall)
				.SetColor(LabelColor.Info)
				.Build();
			suggestTitle.style.marginBottom = spacing.XXS;
			suggestPanel.Content.Add(suggestTitle);

			foreach (var suggestion in departureInfo.Suggestions) {
				var suggestionLabel = new GameLabel($"• {suggestion}")
					.SetStyle(LabelStyle.BodySmall)
					.SetColor(LabelColor.Secondary)
					.Build();
				suggestPanel.Content.Add(suggestionLabel);
			}

			k_warningsSection.AddChild(suggestPanel);
		}
	}

	private void RefreshSupplies() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		k_suppliesSection.ClearChildren();

		bool runExists = k_stateManager.TryGetRunState(out RunState? runState);

		if (!runExists || runState == null) {
			UnityEngine.Debug.LogError("Tried to refresh supply state but no run exists.");
			return;
		}

		var playerCharacter = runState.Character;

		// Supplies grid
		var suppliesGrid = new GamePanel()
			.SetVariant(PanelVariant.Outlined)
			.SetLayout(LayoutDirection.Horizontal)
			.SetWrap(true)
			.SetJustify(Justify.SpaceAround)
			.SetPadding(spacing.SM)
			.Build();

		AddSupplyDisplay(suppliesGrid, "🪙", "Gold", playerCharacter.BaseStats.GetInt(Ids.Stats.Expedition.GoldOnHand));
		AddSupplyDisplay(suppliesGrid, "🍖", "Food", playerCharacter.BaseStats.GetInt(Ids.Stats.Expedition.FoodOnHand));
		AddSupplyDisplay(suppliesGrid, "💊", "Medical", playerCharacter.BaseStats.GetInt(Ids.Stats.Expedition.MedicalSupplies));
		AddSupplyDisplay(suppliesGrid, "🏕", "Camping", playerCharacter.BaseStats.GetInt(Ids.Stats.Expedition.CampingSupplies));

		k_suppliesSection.AddChild(suppliesGrid);

		// Health/Mana bars
		var statsPanel = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.Build();
		statsPanel.style.marginTop = spacing.SM;

		var healthBar = new GameStatBar()
			.SetLabel("Health")
			.SetRange(0, playerCharacter.BaseStats.Get(Ids.Stats.Resource.MaxHealth))
			.SetValue(playerCharacter.BaseStats.Get(Ids.Stats.Resource.CurrentHealth))
			.SetVariant(StatBarVariant.Health)
			.SetShowValue()
			.Build();

		var manaBar = new GameStatBar()
			.SetLabel("Mana")
			.SetRange(0, playerCharacter.BaseStats.Get(Ids.Stats.Resource.MaxMana))
			.SetValue(playerCharacter.BaseStats.Get(Ids.Stats.Resource.CurrentMana))
			.SetVariant(StatBarVariant.Mana)
			.SetShowValue()
			.Build();

		statsPanel.Content.Add(healthBar);
		statsPanel.Content.Add(manaBar);

		k_suppliesSection.AddChild(statsPanel);
	}

	private void AddSupplyDisplay(GamePanel container, string icon, string label, int value) {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		var item = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetAlignment(Align.Center)
			.Build();
		item.style.width = Length.Percent(45);
		item.style.marginBottom = spacing.XS;

		var iconLabel = new GameLabel(icon)
			.SetStyle(LabelStyle.DisplaySmall)
			.Build();

		var valueLabel = new GameLabel(value.ToString())
			.SetStyle(LabelStyle.TitleMedium)
			.SetColor(LabelColor.Primary)
			.Build();

		var nameLabel = new GameLabel(label)
			.SetStyle(LabelStyle.Caption)
			.SetColor(LabelColor.Tertiary)
			.Build();

		item.Content.Add(iconLabel);
		item.Content.Add(valueLabel);
		item.Content.Add(nameLabel);

		container.Content.Add(item);
	}

	#endregion

	#region Actions

	private void ConfirmDeparture() {
		Close();
		OnDepartureConfirmed?.Invoke();

		if (k_session.StartExpedition()) {
			GameToast.Show("You venture into the fog...", ToastType.Info);
		} else {
			GameToast.Show("Cannot depart right now.", ToastType.Error);
		}
	}

	#endregion
}
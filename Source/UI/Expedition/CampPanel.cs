using RPGGame.Core;
using RPGGame.Core.Expedition;
using RPGGame.Core.Simulation;
using RPGGame.Core.Stats;
using RPGGame.UI.Components;
using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Expedition;

/// <summary>
/// Panel displayed when the player sets up camp.
/// Allows resting, level-ups, inventory management, and other camp activities.
/// </summary>
public class CampPanel : VisualElement {
	#region Fields

	private readonly ExpeditionManager k_expeditionManager;
	private readonly GameSession k_session;
	private readonly GameStateManager k_stateManager;

	private GamePanel k_content = null!;
	private GamePanel k_statusSection = null!;
	private GamePanel k_actionsSection = null!;
	private GamePanel k_restSection = null!;

	// Status displays
	private GameStatBar k_healthBar = null!;
	private GameStatBar k_manaBar = null!;
	private GameStatBar k_staminaBar = null!;
	private GameStatBar k_fatigueBar = null!;
	private GameLabel k_timeLabel = null!;
	private GameLabel k_suppliesLabel = null!;

	// Rest controls
	private GameSlider k_restHoursSlider = null!;
	private GameLabel k_restHoursLabel = null!;
	private GameButton k_restButton = null!;

	// Action buttons
	private GameButton k_levelUpButton = null!;
	private GameButton k_inventoryButton = null!;
	private GameButton k_craftButton = null!;
	private GameButton k_breakCampButton = null!;

	// Level up notification
	private GamePanel k_levelUpNotice = null!;

	#endregion

	#region Events

	public event Action? OnCampBroken;
	public event Action? OnLevelUpRequested;

	#endregion

	#region Constructor

	public CampPanel(
		ExpeditionManager expeditionManager,
		GameSession session,
		GameStateManager stateManager
	) {
		k_expeditionManager = expeditionManager;
		k_session = session;
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
		var dialogStyles = theme.Components.Dialog;

		// Full screen overlay
		style.position = Position.Absolute;
		style.left = 0;
		style.top = 0;
		style.right = 0;
		style.bottom = 0;
		style.alignItems = Align.Center;
		style.justifyContent = Justify.Center;
		style.backgroundColor = colors.BackgroundOverlay;
		pickingMode = PickingMode.Position;

		// Main content
		k_content = new GamePanel()
			.SetVariant(PanelVariant.Card)
			.SetPadding(spacing.XL)
			.Build();
		k_content.style.width = dialogStyles.MinWidth;
		k_content.style.maxWidth = Length.Percent(90);

		// Header
		var header = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetLayout(LayoutDirection.Horizontal)
			.SetJustify(Justify.SpaceBetween)
			.SetAlignment(Align.Center)
			.Build();

		var titleLabel = new GameLabel("🏕 Camp")
			.SetStyle(LabelStyle.HeadlineMedium)
			.SetColor(LabelColor.Primary)
			.Build();

		k_timeLabel = new GameLabel("Day 1, 8:00 PM")
			.SetStyle(LabelStyle.BodyMedium)
			.SetColor(LabelColor.Secondary)
			.Build();

		header.Content.Add(titleLabel);
		header.Content.Add(k_timeLabel);

		var divider = new GameDivider().Build();
		divider.style.marginTop = spacing.MD;
		divider.style.marginBottom = spacing.MD;

		// Status section
		BuildStatusSection();

		// Level up notice (if available)
		BuildLevelUpNotice();

		// Rest section
		BuildRestSection();

		// Actions section
		BuildActionsSection();

		// Build hierarchy
		k_content.Content.Add(header);
		k_content.Content.Add(divider);
		k_content.Content.Add(k_statusSection);
		k_content.Content.Add(k_levelUpNotice);
		k_content.Content.Add(k_restSection);
		k_content.Content.Add(k_actionsSection);

		Add(k_content);
	}

	private void BuildStatusSection() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		k_statusSection = new GamePanel()
			.SetVariant(PanelVariant.Outlined)
			.SetPadding(spacing.MD)
			.Build();
		k_statusSection.style.marginBottom = spacing.MD;

		var statusHeader = new GameLabel("Current Status")
			.SetStyle(LabelStyle.TitleSmall)
			.SetColor(LabelColor.Secondary)
			.Build();
		statusHeader.style.marginBottom = spacing.SM;

		// Health bar - dynamic for auto warning/danger
		k_healthBar = new GameStatBar()
			.SetLabel("Health")
			.SetVariant(StatBarVariant.Dynamic)
			.SetDynamicBaseVariant(StatBarVariant.Health)
			.SetRange(0, 100)
			.SetValue(100)
			.SetShowValue()
			.Build();

		// Mana bar
		k_manaBar = new GameStatBar()
			.SetLabel("Mana")
			.SetVariant(StatBarVariant.Mana)
			.SetRange(0, 50)
			.SetValue(50)
			.SetShowValue()
			.Build();
		k_manaBar.style.marginTop = spacing.XS;

		// Stamina bar - dynamic for auto warning
		k_staminaBar = new GameStatBar()
			.SetLabel("Stamina")
			.SetVariant(StatBarVariant.Dynamic)
			.SetDynamicBaseVariant(StatBarVariant.Stamina)
			.SetRange(0, 100)
			.SetValue(100)
			.SetShowValue()
			.Build();
		k_staminaBar.style.marginTop = spacing.XS;

		// Fatigue bar - uses Fatigue variant (inverted, higher = worse)
		k_fatigueBar = new GameStatBar()
			.SetLabel("Fatigue")
			.SetVariant(StatBarVariant.Fatigue)
			.SetRange(0, 100)
			.SetValue(0)
			.SetShowValue()
			.Build();
		k_fatigueBar.style.marginTop = spacing.XS;

		// Supplies info
		k_suppliesLabel = new GameLabel("Camping Supplies: 2")
			.SetStyle(LabelStyle.BodySmall)
			.SetColor(LabelColor.Secondary)
			.Build();
		k_suppliesLabel.style.marginTop = spacing.SM;

		k_statusSection.Content.Add(statusHeader);
		k_statusSection.Content.Add(k_healthBar);
		k_statusSection.Content.Add(k_manaBar);
		k_statusSection.Content.Add(k_staminaBar);
		k_statusSection.Content.Add(k_fatigueBar);
		k_statusSection.Content.Add(k_suppliesLabel);
	}

	private void BuildLevelUpNotice() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var colors = theme.Colors;
		var borders = theme.Borders;

		k_levelUpNotice = new GamePanel()
			.SetVariant(PanelVariant.Outlined)
			.SetPadding(spacing.SM)
			.Build();
		k_levelUpNotice.style.marginBottom = spacing.MD;
		k_levelUpNotice.style.borderLeftColor = colors.Success;
		k_levelUpNotice.style.borderLeftWidth = borders.WidthThick;
		k_levelUpNotice.style.display = DisplayStyle.None;

		var noticeRow = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetLayout(LayoutDirection.Horizontal)
			.SetJustify(Justify.SpaceBetween)
			.SetAlignment(Align.Center)
			.Build();

		var noticeLabel = new GameLabel("✨ Level Up Available!")
			.SetStyle(LabelStyle.TitleSmall)
			.SetColor(LabelColor.Success)
			.Build();

		k_levelUpButton = new GameButton("Apply Level Up")
			.SetVariant(ButtonVariant.Success)
			.SetSize(ButtonSize.Small)
			.OnClick(OnLevelUpClicked)
			.Build();

		noticeRow.Content.Add(noticeLabel);
		noticeRow.Content.Add(k_levelUpButton);

		k_levelUpNotice.Content.Add(noticeRow);
	}

	private void BuildRestSection() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		k_restSection = new GamePanel()
			.SetVariant(PanelVariant.Outlined)
			.SetPadding(spacing.MD)
			.Build();
		k_restSection.style.marginBottom = spacing.MD;

		var restHeader = new GameLabel("Rest")
			.SetStyle(LabelStyle.TitleSmall)
			.SetColor(LabelColor.Secondary)
			.Build();
		restHeader.style.marginBottom = spacing.SM;

		var restDescription = new GameLabel("Resting restores health, mana, and stamina while reducing fatigue.")
			.SetStyle(LabelStyle.BodySmall)
			.SetColor(LabelColor.Tertiary)
			.Build();
		restDescription.style.marginBottom = spacing.SM;

		// Rest hours slider
		var sliderRow = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetLayout(LayoutDirection.Horizontal)
			.SetAlignment(Align.Center)
			.Build();

		var sliderLabel = new GameLabel("Hours:")
			.SetStyle(LabelStyle.BodyMedium)
			.SetColor(LabelColor.Primary)
			.Build();
		sliderLabel.style.marginRight = spacing.SM;

		k_restHoursSlider = new GameSlider()
			.SetRange(1, 8)
			.SetValue(4)
			.SetStep(1)
			.OnValueChanged(OnRestHoursChanged)
			.Build();
		k_restHoursSlider.style.flexGrow = 1;

		k_restHoursLabel = new GameLabel("4 hours")
			.SetStyle(LabelStyle.BodyMedium)
			.SetColor(LabelColor.Primary)
			.Build();
		k_restHoursLabel.style.marginLeft = spacing.SM;
		k_restHoursLabel.style.minWidth = spacing.XXXL * 2;

		sliderRow.Content.Add(sliderLabel);
		sliderRow.Content.Add(k_restHoursSlider);
		sliderRow.Content.Add(k_restHoursLabel);

		// Rest button
		k_restButton = new GameButton("🛏 Rest")
			.SetVariant(ButtonVariant.Primary)
			.SetSize(ButtonSize.Large)
			.SetFullWidth()
			.OnClick(OnRestClicked)
			.Build();
		k_restButton.style.marginTop = spacing.MD;

		k_restSection.Content.Add(restHeader);
		k_restSection.Content.Add(restDescription);
		k_restSection.Content.Add(sliderRow);
		k_restSection.Content.Add(k_restButton);
	}

	private void BuildActionsSection() {
		k_actionsSection = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetLayout(LayoutDirection.Horizontal)
			.SetJustify(Justify.SpaceBetween)
			.Build();

		k_inventoryButton = new GameButton("🎒 Inventory")
			.SetVariant(ButtonVariant.Outline)
			.SetSize(ButtonSize.Medium)
			.OnClick(() => GameToast.Show("Inventory (coming soon)", ToastType.Info))
			.Build();

		k_craftButton = new GameButton("⚒ Craft")
			.SetVariant(ButtonVariant.Outline)
			.SetSize(ButtonSize.Medium)
			.OnClick(() => GameToast.Show("Crafting (coming soon)", ToastType.Info))
			.Build();

		k_breakCampButton = new GameButton("📦 Break Camp")
			.SetVariant(ButtonVariant.Outline)
			.SetSize(ButtonSize.Medium)
			.OnClick(OnBreakCampClicked)
			.Build();

		k_actionsSection.Content.Add(k_inventoryButton);
		k_actionsSection.Content.Add(k_craftButton);
		k_actionsSection.Content.Add(k_breakCampButton);
	}

	#endregion

	#region Public Methods

	public void Show() {
		RefreshUI();
		style.display = DisplayStyle.Flex;
	}

	public void Hide() {
		style.display = DisplayStyle.None;
	}

	public void RefreshUI() {
		var run = k_session.CurrentRun;
		if (run == null) return;

		// Read directly from ExpeditionManager properties instead of GetStatus()
		var expedition = k_expeditionManager;

		// Update time
		k_timeLabel.SetText($"Day {GameTime.Instance.Day}, {FormatHour(GameTime.Instance.Hour)}");

		// Update stats - Dynamic/Fatigue variants handle color automatically
		k_healthBar
			.SetRange(0, run.Stats.Get(Ids.Stats.Resource.MaxHealth))
			.SetValue(run.Stats.Get(Ids.Stats.Resource.CurrentHealth));

		k_manaBar
			.SetRange(0, run.Stats.Get(Ids.Stats.Resource.MaxMana))
			.SetValue(run.Stats.Get(Ids.Stats.Resource.CurrentMana));

		k_staminaBar
			.SetRange(0, expedition.EffectiveMaxStamina)
			.SetValue(expedition.CurrentStamina);

		k_fatigueBar
			.SetRange(0, expedition.MaxFatigue)
			.SetValue(expedition.Fatigue);

		// Update supplies
		k_suppliesLabel.SetText($"Camping Supplies: {expedition.CampingSupplies}");

		// Update level up notice
		bool hasLevelUp = run.HasPendingLevelUp;
		k_levelUpNotice.style.display = hasLevelUp ? DisplayStyle.Flex : DisplayStyle.None;
	}

	#endregion

	#region Event Handlers

	private void OnRestHoursChanged(float value) {
		int hours = (int)value;
		k_restHoursLabel.SetText($"{hours} hour{(hours != 1 ? "s" : "")}");
	}

	private void OnRestClicked() {
		int hours = (int)k_restHoursSlider.Value;

		k_expeditionManager.RestAtCamp(hours);
		RefreshUI();

		GameToast.Show($"Rested for {hours} hour{(hours != 1 ? "s" : "")}", ToastType.Success);
	}

	private void OnLevelUpClicked() {
		Hide(); // Hide camp panel while showing level up
		OnLevelUpRequested?.Invoke();
	}

	private void OnBreakCampClicked() {
		k_expeditionManager.BreakCamp();
		Hide();
		OnCampBroken?.Invoke();
	}

	#endregion

	#region Helpers

	private string FormatHour(int hour) {
		int displayHour = hour % 12;
		if (displayHour == 0) displayHour = 12;
		string ampm = hour < 12 ? "AM" : "PM";
		return $"{displayHour}:00 {ampm}";
	}

	#endregion
}
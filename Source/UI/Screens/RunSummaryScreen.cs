using RPGGame.Core.Metrics;
using RPGGame.Core.Simulation;
using RPGGame.UI.Components;
using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Screens;

/// <summary>
/// Screen shown at the end of a run (death, victory, retreat).
/// Displays run statistics and rewards.
/// </summary>
public class RunSummaryScreen : VisualElement {
	#region Fields

	private readonly GameStateManager k_stateManager;
	private readonly GameSession k_session;
	private readonly MetricsManager k_metrics;

	private GamePanel k_content = null!;
	private GameLabel k_titleLabel = null!;
	private GameLabel k_subtitleLabel = null!;
	private GamePanel k_statsPanel = null!;
	private GamePanel k_rewardsPanel = null!;
	private GameButton k_continueButton = null!;

	private RunEndReason k_endReason;

	#endregion

	#region Constructor

	public RunSummaryScreen(
		GameStateManager stateManager,
		GameSession session,
		MetricsManager metrics
	) {
		k_stateManager = stateManager;
		k_session = session;
		k_metrics = metrics;

		BuildUI();
		SubscribeToEvents();

		style.display = DisplayStyle.None;
	}

	#endregion

	#region UI Building

	private void BuildUI() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var colors = theme.Colors;
		var borders = theme.Borders;
		var dialogStyles = theme.Components.Dialog;

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
		k_content.style.maxWidth = Length.Percent(95);
		k_content.style.maxHeight = Length.Percent(90);

		// Title
		k_titleLabel = new GameLabel("Run Complete")
			.SetStyle(LabelStyle.DisplaySmall)
			.SetColor(LabelColor.Primary)
			.SetTextAlign(TextAnchor.MiddleCenter)
			.Build();

		k_subtitleLabel = new GameLabel("Your journey has ended...")
			.SetStyle(LabelStyle.TitleMedium)
			.SetColor(LabelColor.Secondary)
			.SetTextAlign(TextAnchor.MiddleCenter)
			.Build();
		k_subtitleLabel.style.marginTop = spacing.XS;

		// Divider
		var divider1 = new GameDivider().Build();
		divider1.style.marginTop = spacing.XL;
		divider1.style.marginBottom = spacing.XL;

		// Stats section
		var statsHeader = new GameLabel("Run Statistics")
			.SetStyle(LabelStyle.TitleSmall)
			.SetColor(LabelColor.Primary)
			.Build();

		k_statsPanel = new GamePanel()
			.SetVariant(PanelVariant.Outlined)
			.SetPadding(spacing.MD)
			.SetLayout(LayoutDirection.Vertical)
			.Build();
		k_statsPanel.style.marginTop = spacing.SM;

		// Rewards section
		var rewardsHeader = new GameLabel("Rewards Earned")
			.SetStyle(LabelStyle.TitleSmall)
			.SetColor(LabelColor.Success)
			.Build();
		rewardsHeader.style.marginTop = spacing.XL;

		k_rewardsPanel = new GamePanel()
			.SetVariant(PanelVariant.Outlined)
			.SetPadding(spacing.MD)
			.SetLayout(LayoutDirection.Vertical)
			.Build();
		k_rewardsPanel.style.marginTop = spacing.SM;
		k_rewardsPanel.style.borderLeftColor = colors.Success;
		k_rewardsPanel.style.borderLeftWidth = borders.WidthThick;

		// Continue button
		k_continueButton = new GameButton("Continue")
			.SetVariant(ButtonVariant.Primary)
			.SetSize(ButtonSize.Large)
			.SetFullWidth()
			.OnClick(OnContinueClicked)
			.Build();
		k_continueButton.style.marginTop = spacing.XL;

		// Build hierarchy
		k_content.Content.Add(k_titleLabel);
		k_content.Content.Add(k_subtitleLabel);
		k_content.Content.Add(divider1);
		k_content.Content.Add(statsHeader);
		k_content.Content.Add(k_statsPanel);
		k_content.Content.Add(rewardsHeader);
		k_content.Content.Add(k_rewardsPanel);
		k_content.Content.Add(k_continueButton);

		Add(k_content);
	}

	#endregion

	#region Event Subscriptions

	private void SubscribeToEvents() {
		k_stateManager.OnPhaseChanged += OnPhaseChanged;
		k_session.OnRunEnded += OnRunEnded;
	}

	private void OnPhaseChanged(GamePhase oldPhase, GamePhase newPhase) {
		style.display = newPhase == GamePhase.RunSummary ? DisplayStyle.Flex : DisplayStyle.None;
	}

	private void OnRunEnded(RunEndReason reason) {
		k_endReason = reason;
		RefreshUI();
	}

	#endregion

	#region UI Updates

	private void RefreshUI() {
		// Update title based on reason
		var (title, subtitle, icon, color) = k_endReason switch {
			RunEndReason.Victory => ("Victory!", "You have pushed back the fog!", "🏆", LabelColor.Success),
			RunEndReason.TrueEnding => ("True Ending!", "You have broken the cycle!", "✨", LabelColor.Success),
			RunEndReason.Death => ("Caught in Time", "The fog has claimed another...", "💀", LabelColor.Error),
			RunEndReason.Retreat => ("Tactical Retreat", "You live to fight another day.", "🏃", LabelColor.Warning),
			RunEndReason.Abandoned => ("Run Abandoned", "The journey ends here.", "❌", LabelColor.Tertiary),
			_ => ("Run Complete", "Your journey has ended.", "📜", LabelColor.Primary)
		};

		k_titleLabel.SetText($"{icon} {title}");
		k_titleLabel.SetColor(color);
		k_subtitleLabel.SetText(subtitle);

		// Build stats
		BuildStatsDisplay();

		// Build rewards
		BuildRewardsDisplay();
	}

	private void BuildStatsDisplay() {
		k_statsPanel.ClearChildren();

		// Get run metrics from the metrics manager
		var stats = new List<(string label, string value)> {
			("Days Survived", k_metrics.Get(MetricType.CurrentRunDays).ToString()),
			("Distance Traveled", $"{k_metrics.Get(MetricType.TilesTraveled)} tiles"),
			("Enemies Defeated", k_metrics.Get(MetricType.EnemiesDefeated).ToString()),
			("Damage Dealt", k_metrics.Get(MetricType.DamageDealt).ToString()),
			("Healing Done", k_metrics.Get(MetricType.HealingDone).ToString()),
			("Items Collected", k_metrics.Get(MetricType.ItemsCollected).ToString()),
			("Events Encountered", k_metrics.Get(MetricType.EventsEncountered).ToString()),
			("Choices Made", k_metrics.Get(MetricType.ChoicesMade).ToString())
		};

		foreach (var (label, value) in stats) {
			AddStatRow(label, value);
		}
	}

	private void AddStatRow(string label, string value) {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		var row = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetLayout(LayoutDirection.Horizontal)
			.SetJustify(Justify.SpaceBetween)
			.Build();
		row.style.marginBottom = spacing.XXS;

		var labelElement = new GameLabel(label)
			.SetStyle(LabelStyle.BodyMedium)
			.SetColor(LabelColor.Secondary)
			.Build();

		var valueElement = new GameLabel(value)
			.SetStyle(LabelStyle.BodyMedium)
			.SetColor(LabelColor.Primary)
			.Build();

		row.Content.Add(labelElement);
		row.Content.Add(valueElement);

		k_statsPanel.Content.Add(row);
	}

	private void BuildRewardsDisplay() {
		k_rewardsPanel.ClearChildren();

		var meta = k_session.MetaProgression;

		// Calculate upgrade points (would normally come from EndRun logic)
		int upgradePoints = (int)(k_metrics.Get(MetricType.TilesTraveled) / 10);
		if (k_endReason == RunEndReason.Victory) {
			upgradePoints += 50;
		}

		var rewards = new List<(string icon, string label, string value)> {
			("🏗", "Village Upgrade Points", $"+{upgradePoints}"),
			("📜", "Lore Discovered", k_metrics.Get(MetricType.LoreEntriesDiscovered).ToString()),
			("🗺", "Map Revealed", $"{k_metrics.Get(MetricType.TilesRevealed)} tiles")
		};

		// Add victory-specific rewards
		if (k_endReason == RunEndReason.Victory) {
			rewards.Add(("🌟", "Fog Clears", meta.FogClears.ToString()));
		}

		foreach (var (icon, label, value) in rewards) {
			AddRewardRow(icon, label, value);
		}
	}

	private void AddRewardRow(string icon, string label, string value) {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		var row = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetLayout(LayoutDirection.Horizontal)
			.SetAlignment(Align.Center)
			.Build();
		row.style.marginBottom = spacing.XS;

		var iconLabel = new GameLabel(icon)
			.SetStyle(LabelStyle.TitleMedium)
			.Build();

		var textLabel = new GameLabel(label)
			.SetStyle(LabelStyle.BodyMedium)
			.SetColor(LabelColor.Primary)
			.Build();
		textLabel.style.marginLeft = spacing.XS;
		textLabel.style.flexGrow = 1;

		var valueLabel = new GameLabel(value)
			.SetStyle(LabelStyle.TitleSmall)
			.SetColor(LabelColor.Success)
			.Build();

		row.Content.Add(iconLabel);
		row.Content.Add(textLabel);
		row.Content.Add(valueLabel);

		k_rewardsPanel.Content.Add(row);
	}

	#endregion

	#region Button Handlers

	private void OnContinueClicked() {
		// Return to main menu
		k_stateManager.SetPhase(GamePhase.MainMenu);
	}

	#endregion
}
using RPGGame.Core;
using RPGGame.Core.Simulation;
using RPGGame.UI.Components;
using RPGGame.UI.Styles;
using UnityEngine.UIElements;

namespace RPGGame.UI.Screens;

/// <summary>
/// The main gameplay screen that serves as the base layer for all gameplay phases.
/// Manages the pause overlay and windows container. Individual screens (ExpeditionScreen,
/// CombatScreen, VillageScreen) handle their own HUD elements.
/// </summary>
public class GameplayScreen : VisualElement {
	#region Private Fields

	private readonly GameStateManager k_stateManager;
	private readonly UiManager k_uiManager;
	private readonly GameLoop k_gameLoop;
	private readonly GameDb k_gameDb;

	private VisualElement k_windowsContainer = null!;
	private VisualElement k_pauseOverlay = null!;

	// Track which phases show gameplay
	private static readonly HashSet<GamePhase> k_gameplayPhases = [
		GamePhase.Village,
		GamePhase.Expedition,
		GamePhase.Event,
		GamePhase.Combat,
		GamePhase.Camp,
		GamePhase.LevelUp,
		GamePhase.Paused
	];

	#endregion

	#region Constructor

	public GameplayScreen(
		GameStateManager stateManager,
		UiManager uiManager,
		GameLoop gameLoop,
		GameDb gameDb
	) {
		k_stateManager = stateManager;
		k_uiManager = uiManager;
		k_gameLoop = gameLoop;
		k_gameDb = gameDb;

		BuildUI();

		// Subscribe to phase changes
		k_stateManager.OnPhaseChanged += OnPhaseChanged;

		// Initially hidden
		style.display = DisplayStyle.None;
	}

	#endregion

	#region UI Building

	private void BuildUI() {
		var theme = GameTheme.Current;
		var colors = theme.Colors;

		pickingMode = PickingMode.Ignore;

		// Full screen
		style.width = Length.Percent(100);
		style.height = Length.Percent(100);
		style.position = Position.Absolute;

		// Windows container (for game windows)
		k_windowsContainer = new VisualElement {
			name = "windows-container",
			style = {
				position = Position.Absolute,
				left = 0, top = 0, right = 0, bottom = 0
			}
		};
		k_windowsContainer.pickingMode = PickingMode.Ignore;

		// Pause overlay
		k_pauseOverlay = new VisualElement {
			name = "pause-overlay",
			style = {
				position = Position.Absolute,
				left = 0, top = 0, right = 0, bottom = 0,
				display = DisplayStyle.None,
				alignItems = Align.Center,
				justifyContent = Justify.Center,
				backgroundColor = colors.BackgroundOverlay
			}
		};
		BuildPauseMenu();

		Add(k_windowsContainer);
		Add(k_pauseOverlay);
	}

	private void BuildPauseMenu() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var dialogStyles = theme.Components.Dialog;

		var pausePanel = new GamePanel()
			.SetVariant(PanelVariant.Card)
			.SetPadding(spacing.XL)
			.Build();
		pausePanel.style.width = dialogStyles.SmallWidth * 0.5f;
		pausePanel.style.alignItems = Align.Center;

		var pauseTitle = new GameLabel("PAUSED")
			.SetStyle(LabelStyle.HeadlineLarge)
			.SetColor(LabelColor.Primary)
			.Build();
		pauseTitle.style.marginBottom = spacing.XL;

		var resumeButton = new GameButton("Resume")
			.SetVariant(ButtonVariant.Primary)
			.SetSize(ButtonSize.Large)
			.SetFullWidth()
			.OnClick(() => k_stateManager.ResumeGame())
			.Build();
		resumeButton.style.marginBottom = spacing.SM;

		var settingsButton = new GameButton("Settings")
			.SetVariant(ButtonVariant.Outline)
			.SetSize(ButtonSize.Large)
			.SetFullWidth()
			.OnClick(() => { /* Show settings */ })
			.Build();
		settingsButton.style.marginBottom = spacing.SM;

		var mainMenuButton = new GameButton("Main Menu")
			.SetVariant(ButtonVariant.Outline)
			.SetSize(ButtonSize.Large)
			.SetFullWidth()
			.OnClick(() => {
				GameDialog.Confirm(
					"Return to Main Menu",
					"Any unsaved progress will be lost. Continue?",
					() => k_stateManager.GoToMainMenu(),
					null
				).Show(k_pauseOverlay);
			})
			.Build();
		mainMenuButton.style.marginBottom = spacing.SM;

		var exitButton = new GameButton("Exit Game")
			.SetVariant(ButtonVariant.Ghost)
			.SetSize(ButtonSize.Large)
			.SetFullWidth()
			.OnClick(() => {
				GameDialog.Confirm(
					"Exit Game",
					"Are you sure you want to exit?",
					() => k_stateManager.ExitGame(),
					null
				).Show(k_pauseOverlay);
			})
			.Build();

		pausePanel.Content.Add(pauseTitle);
		pausePanel.Content.Add(resumeButton);
		pausePanel.Content.Add(settingsButton);
		pausePanel.Content.Add(mainMenuButton);
		pausePanel.Content.Add(exitButton);

		k_pauseOverlay.Add(pausePanel);
	}

	#endregion

	#region Phase Handling

	private void OnPhaseChanged(GamePhase oldPhase, GamePhase newPhase) {
		// Show gameplay screen for gameplay phases
		bool shouldShow = k_gameplayPhases.Contains(newPhase);
		style.display = shouldShow ? DisplayStyle.Flex : DisplayStyle.None;

		// Show/hide pause overlay
		k_pauseOverlay.style.display = newPhase == GamePhase.Paused
			? DisplayStyle.Flex
			: DisplayStyle.None;
	}

	#endregion

	#region Cleanup

	public new void RemoveFromHierarchy() {
		k_stateManager.OnPhaseChanged -= OnPhaseChanged;
		base.RemoveFromHierarchy();
	}

	#endregion
}
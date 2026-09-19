using System.Linq;
using RPGGame.Core;
using RPGGame.Core.Save;
using RPGGame.Core.Simulation;
using RPGGame.UI.Components;
using RPGGame.UI.Styles;
using UnityEngine.UIElements;

namespace RPGGame.UI.Screens;

/// <summary>
/// The main menu screen shown when the game starts.
/// Provides options for New Game, Continue, Load, Settings, and Exit.
/// </summary>
public class MainMenuScreen : VisualElement {
	#region Private Fields

	private readonly GameStateManager k_stateManager;
	private readonly UiManager k_uiManager;
	private readonly GameSession k_session;
	private readonly GameDb k_gameDb;

	private GameContainer? k_backgroundContainer;
	private GameContainer? k_menuContainer;
	private GameContainer? k_buttonContainer;
	private GameContainer? k_footerContainer;
	private GameButton? k_continueButton;

	#endregion

	#region Constructor

	public MainMenuScreen(
		GameStateManager stateManager,
		UiManager uiManager,
		GameSession session,
		GameDb gameDb
	) {
		k_stateManager = stateManager;
		k_uiManager = uiManager;
		k_session = session;
		k_gameDb = gameDb;

		BuildUI();
		ApplyTheme();

		GameTheme.OnThemeChanged += OnThemeChanged;
	}

	#endregion

	#region UI Building

	private void BuildUI() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var dialogStyles = theme.Components.Dialog;

		// Set styles directly on this element (critical for proper sizing)
		style.width = Length.Percent(100);
		style.height = Length.Percent(100);
		style.position = Position.Absolute;
		pickingMode = PickingMode.Ignore;

		// Background container
		k_backgroundContainer = new GameContainer("main-menu-background")
			.SetAbsoluteFill()
			.Build();
		Add(k_backgroundContainer);

		// Main menu container - centered
		k_menuContainer = new GameContainer("main-menu-container")
			.SetAbsoluteFill()
			.SetColumn()
			.SetCenter()
			.Build();

		// Logo/Title area
		var logoContainer = new GameContainer("logo-container")
			.SetColumn()
			.SetAlignItems(Align.Center)
			.SetMarginBottom(spacing.XXXL)
			.Build();

		var titleLabel = new GameLabel("RPG GAME")
			.SetStyle(LabelStyle.DisplayLarge)
			.SetColor(LabelColor.Primary)
			.SetLetterSpacing(spacing.XS)
			.Build();

		var subtitleLabel = new GameLabel("An Epic Adventure Awaits")
			.SetStyle(LabelStyle.TitleMedium)
			.SetColor(LabelColor.Secondary)
			.SetMarginTop(spacing.XS)
			.Build();

		logoContainer
			.AddChild(titleLabel)
			.AddChild(subtitleLabel);

		// Button container - use DialogStyles.SmallWidth for consistent sizing
		k_buttonContainer = new GameContainer("button-container")
			.SetColumn()
			.SetAlignItems(Align.Center)
			.SetWidth(dialogStyles.SmallWidth)
			.Build();

		// Continue button (hidden by default)
		k_continueButton = new GameButton("Continue")
			.SetVariant(ButtonVariant.Primary)
			.SetSize(ButtonSize.Large)
			.SetFullWidth()
			.OnClick(OnContinueClicked)
			.SetMarginBottom(spacing.SM)
			.Build();
		k_continueButton.style.display = DisplayStyle.None;

		// New Game button
		var newGameButton = new GameButton("New Game")
			.SetVariant(ButtonVariant.Primary)
			.SetSize(ButtonSize.Large)
			.SetFullWidth()
			.OnClick(OnNewGameClicked)
			.SetMarginBottom(spacing.SM)
			.Build();

		// Load Game button
		var loadGameButton = new GameButton("Load Game")
			.SetVariant(ButtonVariant.Outline)
			.SetSize(ButtonSize.Large)
			.SetFullWidth()
			.OnClick(OnLoadGameClicked)
			.SetMarginBottom(spacing.SM)
			.Build();

		// Settings button
		var settingsButton = new GameButton("Settings")
			.SetVariant(ButtonVariant.Outline)
			.SetSize(ButtonSize.Large)
			.SetFullWidth()
			.OnClick(OnSettingsClicked)
			.SetMarginBottom(spacing.SM)
			.Build();

		// Exit button
		var exitButton = new GameButton("Exit")
			.SetVariant(ButtonVariant.Ghost)
			.SetSize(ButtonSize.Large)
			.SetFullWidth()
			.OnClick(OnExitClicked)
			.Build();

		k_buttonContainer
			.AddChild(k_continueButton)
			.AddChild(newGameButton)
			.AddChild(loadGameButton)
			.AddChild(settingsButton)
			.AddChild(exitButton);

		// Footer with version info
		k_footerContainer = new GameContainer("footer-container")
			.SetAbsolute()
			.SetBottom(spacing.LG)
			.SetLeft(0)
			.SetRight(0)
			.SetColumn()
			.SetAlignItems(Align.Center)
			.Build();

		var versionLabel = new GameLabel("Version 0.1.0 - Early Development")
			.SetStyle(LabelStyle.Caption)
			.SetColor(LabelColor.Tertiary)
			.Build();
		k_footerContainer.AddChild(versionLabel);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
		var devToolsButton = new GameButton("🔧 Dev Tools")
			.SetVariant(ButtonVariant.Ghost)
			.SetSize(ButtonSize.Small)
			.OnClick(OnDevToolsClicked)
			.SetMarginTop(spacing.XS)
			.Build();
		k_footerContainer.AddChild(devToolsButton);
#endif

		k_menuContainer
			.AddChild(logoContainer)
			.AddChild(k_buttonContainer);

		Add(k_menuContainer);
		Add(k_footerContainer);

		UpdateContinueButton();
	}

	#endregion

	#region Button Handlers

	private void OnContinueClicked() {
		var saves = k_session.GetSaveSlots()
			.Where(s => s.HasData)
			.OrderByDescending(s => s.SaveTimestamp)
			.ToList();

		if (saves.Count > 0) {
			_ = k_session.LoadGameAsync(saves[0].SlotIndex);
		}
	}

	private void OnNewGameClicked() {
		k_stateManager.SetPhase(GamePhase.NewGame);
	}

	private void OnLoadGameClicked() {
		ShowLoadGameDialog();
	}

	private void OnSettingsClicked() {
		ShowSettingsDialog();
	}

	private void OnExitClicked() {
		GameDialog.Confirm(
			"Exit Game",
			"Are you sure you want to exit?",
			() => k_stateManager.ExitGame(),
			null
		).Show(this);
	}

	private void OnDevToolsClicked() {
		k_uiManager.DEBUG_OpenUIShowcase();
	}

	#endregion

	#region Dialogs

	private void ShowLoadGameDialog() {
		var theme = GameTheme.Current;
		var dialogStyles = theme.Components.Dialog;

		var saves = k_session.GetSaveSlots()
			.Where(s => s.HasData)
			.ToList();

		if (saves.Count == 0) {
			GameDialog.Alert("No Saves Found", "You don't have any saved games yet.").Show(this);
			return;
		}

		var content = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.Build();

		var saveList = new GameListView<SaveSlotInfo>()
			.SetItems(saves)
			.SetItemTemplate(save => CreateSaveSlotDisplay(save))
			.SetSelectionType(ListSelectionType.Single)
			.SetHeight(dialogStyles.ScrollableContentHeight)
			.Build();

		content.Content.Add(saveList);

		SaveSlotInfo? selectedSave = null;
		saveList.OnSelectionChanged(items => {
			selectedSave = items.FirstOrDefault();
		});

		new GameDialog()
			.SetTitle("Load Game")
			.SetContent(content)
			.SetSize(DialogSize.Large)
			.AddAction("Cancel", DialogActionType.Secondary, null)
			.AddAction("Load", DialogActionType.Primary, () => {
				if (selectedSave != null) {
					_ = k_session.LoadGameAsync(selectedSave.SlotIndex);
				}
			})
			.Build()
			.Show(this);
	}

	private GameContainer CreateSaveSlotDisplay(SaveSlotInfo save) {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var listStyles = theme.Components.List;

		var container = new GameContainer("save-slot")
			.SetRow()
			.SetSpaceBetween()
			.SetMinHeight(listStyles.ItemHeight)
			.SetPadding(spacing.SM)
			.Build();

		var leftSide = new GameContainer("save-left")
			.SetColumn()
			.Build();

		leftSide
			.AddChild(new GameLabel(save.CharacterName)
				.SetStyle(LabelStyle.TitleMedium)
				.Build())
			.AddChild(new GameLabel($"Level {save.Level} {save.CharacterClass}")
				.SetStyle(LabelStyle.BodySmall)
				.SetColor(LabelColor.Secondary)
				.Build())
			.AddChild(new GameLabel($"Run {save.RunNumber}, Day {save.CurrentDay}")
				.SetStyle(LabelStyle.Caption)
				.SetColor(LabelColor.Tertiary)
				.Build());

		var rightSide = new GameContainer("save-right")
			.SetColumn()
			.SetAlignItems(Align.FlexEnd)
			.Build();

		rightSide
			.AddChild(new GameLabel(save.DisplayName)
				.SetStyle(LabelStyle.LabelMedium)
				.SetColor(save.IsAutoSave ? LabelColor.Warning : LabelColor.Primary)
				.Build())
			.AddChild(new GameLabel(save.SaveTimestampFormatted)
				.SetStyle(LabelStyle.Caption)
				.SetColor(LabelColor.Tertiary)
				.Build())
			.AddChild(new GameLabel(save.PlaytimeFormatted)
				.SetStyle(LabelStyle.Caption)
				.SetColor(LabelColor.Tertiary)
				.Build());

		if (save.FogClears > 0) {
			rightSide.AddChild(new GameLabel($"🌫️ {save.FogClears} Fog Clear(s)")
				.SetStyle(LabelStyle.Caption)
				.SetColor(LabelColor.Success)
				.Build());
		}

		container
			.AddChild(leftSide)
			.AddChild(rightSide);

		return container;
	}

	private void ShowSettingsDialog() {
		var content = new GameTabView()
			.AddTab("Audio", CreateAudioSettings())
			.AddTab("Video", CreateVideoSettings())
			.AddTab("Gameplay", CreateGameplaySettings())
			.AddTab("Controls", CreateControlsSettings())
			.Build();

		new GameDialog()
			.SetTitle("Settings")
			.SetContent(content)
			.SetSize(DialogSize.Large)
			.AddAction("Close", DialogActionType.Primary, null)
			.Build()
			.Show(this);
	}

	private VisualElement CreateAudioSettings() {
		return new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.AddChild(new GameSlider()
				.SetLabel("Master Volume")
				.SetRange(0, 100)
				.SetValue(100)
				.SetShowValue()
				.SetValueFormat("{0}%")
				.Build())
			.AddChild(new GameSlider()
				.SetLabel("Music Volume")
				.SetRange(0, 100)
				.SetValue(80)
				.SetShowValue()
				.SetValueFormat("{0}%")
				.Build())
			.AddChild(new GameSlider()
				.SetLabel("SFX Volume")
				.SetRange(0, 100)
				.SetValue(100)
				.SetShowValue()
				.SetValueFormat("{0}%")
				.Build())
			.AddChild(new GameSlider()
				.SetLabel("Voice Volume")
				.SetRange(0, 100)
				.SetValue(100)
				.SetShowValue()
				.SetValueFormat("{0}%")
				.Build())
			.AddChild(new GameCheckbox()
				.SetLabel("Enable UI Sounds")
				.SetChecked(true)
				.Build())
			.Build();
	}

	private VisualElement CreateVideoSettings() {
		return new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.AddChild(new GameDropdown()
				.SetLabel("Resolution")
				.SetOptions("1920x1080", "2560x1440", "3840x2160", "Windowed")
				.SetSelectedIndex(0)
				.Build())
			.AddChild(new GameDropdown()
				.SetLabel("Quality")
				.SetOptions("Low", "Medium", "High", "Ultra")
				.SetSelectedIndex(2)
				.Build())
			.AddChild(new GameCheckbox()
				.SetLabel("VSync")
				.SetChecked(true)
				.Build())
			.AddChild(new GameCheckbox()
				.SetLabel("Fullscreen")
				.SetChecked(true)
				.Build())
			.AddChild(new GameSlider()
				.SetLabel("Brightness")
				.SetRange(0, 100)
				.SetValue(50)
				.SetShowValue()
				.Build())
			.Build();
	}

	private VisualElement CreateGameplaySettings() {
		return new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.AddChild(new GameCheckbox()
				.SetLabel("Show Tutorials")
				.SetChecked(true)
				.Build())
			.AddChild(new GameCheckbox()
				.SetLabel("Show Damage Numbers")
				.SetChecked(true)
				.Build())
			.AddChild(new GameCheckbox()
				.SetLabel("Auto-Loot")
				.SetChecked(false)
				.Build())
			.AddChild(new GameCheckbox()
				.SetLabel("Screen Shake")
				.SetChecked(true)
				.Build())
			.AddChild(new GameDropdown()
				.SetLabel("Subtitles")
				.SetOptions("Off", "Dialogue Only", "All")
				.SetSelectedIndex(1)
				.Build())
			.Build();
	}

	private VisualElement CreateControlsSettings() {
		return new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.AddChild(new GameLabel("Control settings coming soon...")
				.SetStyle(LabelStyle.BodyMedium)
				.SetColor(LabelColor.Secondary)
				.Build())
			.Build();
	}

	#endregion

	#region Helpers

	private void UpdateContinueButton() {
		var hasSave = k_session.GetSaveSlots().Any(s => s.HasData);
		if (k_continueButton != null) {
			k_continueButton.style.display = hasSave ? DisplayStyle.Flex : DisplayStyle.None;
		}
	}

	public void Refresh() {
		UpdateContinueButton();
	}

	#endregion

	#region Theme

	private void ApplyTheme() {
		var colors = GameTheme.Current.Colors;
		k_backgroundContainer?.SetBackgroundColor(colors.Background);
	}

	private void OnThemeChanged(GameTheme theme) {
		ApplyTheme();
	}

	#endregion

	#region Cleanup

	public new void RemoveFromHierarchy() {
		GameTheme.OnThemeChanged -= OnThemeChanged;
		base.RemoveFromHierarchy();
	}

	#endregion
}
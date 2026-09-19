using RPGGame.Core;
using RPGGame.Core.Dungeons;
using RPGGame.Core.Events;
using RPGGame.Core.Expedition;
using RPGGame.Core.Items;
using RPGGame.Core.Rewards;
using RPGGame.Core.Simulation;
using RPGGame.Core.Stats;
using RPGGame.UI.Character;
using RPGGame.UI.Components;
using RPGGame.UI.Expedition;
using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Screens;

/// <summary>
/// The main expedition screen displayed during the travel/exploration phase.
/// Shows the map view, status bars, travel controls, and handles event display.
/// Uses floating panels for all bottom UI elements.
/// </summary>
public class ExpeditionScreen : VisualElement {
	#region Fields

	private readonly GameStateManager k_stateManager;
	private readonly ExpeditionManager k_expeditionManager;
	private readonly EventManager k_eventManager;
	private readonly DungeonManager k_dungeonManager;
	private readonly GameSession k_session;
	private readonly UiManager k_uiManager;
	private readonly GameDb k_gameDb;
	private readonly InventoryManager k_inventoryManager;

	// UI Sections
	private GamePanel k_topBar = null!;
	private GamePanel k_infoCard = null!;
	private GamePanel k_warningsPanel = null!;

	// Modal panels
	private EventPanel k_eventPanel = null!;
	private PathChoicePanel k_pathChoicePanel = null!;
	private CampPanel k_campPanel = null!;
	private DungeonPanel k_dungeonPanel = null!;
	private LevelUpPanel k_levelUpPanel = null!;
	private ItemRewardPanel k_itemRewardPanel = null!;

	// Top bar elements
	private GameLabel k_dayTimeLabel = null!;
	private GameLabel k_weatherLabel = null!;
	private GameLabel k_distanceLabel = null!;
	private GameLabel k_fogLabel = null!;
	private GameLabel k_locationLabel = null!;

	// Speed controls
	private GameButton k_pauseButton = null!;
	private GameButton k_playButton = null!;
	private GameButton k_fastButton = null!;
	private GameLabel k_speedLabel = null!;

	// Floating panels - Bottom left: Character + Pace
	private GameEntityDetailsPanel k_characterPanel = null!;
	private GamePanel k_characterOverlay = null!;
	private GamePanel k_paceOverlay = null!;

	// Pace controls
	private GameButton k_waitButton = null!;
	private GameButton k_sneakButton = null!;
	private GameButton k_walkButton = null!;
	private GameButton k_jogButton = null!;
	private GameButton k_sprintButton = null!;

	// Floating panels - Bottom center: Resources + Actions
	private GamePanel k_resourceOverlay = null!;
	private GamePanel k_goldLabel = null!;
	private GamePanel k_foodLabel = null!;
	private GamePanel k_medicalLabel = null!;
	private GamePanel k_campingLabel = null!;
	private GamePanel k_moraleLabel = null!;

	// Action buttons
	private GameButton k_campButton = null!;
	private GameButton k_inventoryButton = null!;
	private GameButton k_characterButton = null!;
	private GameButton k_mapButton = null!;
	private GameButton k_retreatButton = null!;

	// Floating panels - Bottom right: Biome
	private BiomeDisplayPanel k_biomePanel = null!;
	private GamePanel k_biomeOverlay = null!;

	// Update scheduling
	private IVisualElementScheduledItem? k_updateSchedule;

	// State Management
	private PathNodeId? k_currentDungeonNodeId;

	// Layout constants
	private const float BOTTOM_OFFSET = 16f;

	private SimulationSpeed k_preEventSimSpeed = SimulationSpeed.Normal;
	private TravelPace k_preEventPace = TravelPace.Walk;

	#endregion

	#region Constructor

	public ExpeditionScreen(
		GameStateManager stateManager,
		ExpeditionManager expeditionManager,
		EventManager eventManager,
		DungeonManager dungeonManager,
		GameSession session,
		GameDb gameDb,
		UiManager uiManager,
		InventoryManager inventoryManager
	) {
		k_stateManager = stateManager;
		k_expeditionManager = expeditionManager;
		k_eventManager = eventManager;
		k_dungeonManager = dungeonManager;
		k_session = session;
		k_gameDb = gameDb;
		k_uiManager = uiManager;
		k_inventoryManager = inventoryManager;

		BuildUI();
		CreatePanels();
		SubscribeToEvents();

		style.display = DisplayStyle.None;

		Debug.Log("ExpeditionScreen: Created");
	}

	#endregion

	#region UI Building

	private void BuildUI() {
		style.position = Position.Absolute;
		style.left = 0;
		style.top = 0;
		style.right = 0;
		style.bottom = 0;

		// Let clicks through to tilemap for non-UI areas
		pickingMode = PickingMode.Ignore;

		BuildTopBar();
		BuildFloatingPanels();
		BuildInfoCard();
		BuildWarningsPanel();
	}

	private void BuildTopBar() {
		var spacing = GameTheme.Current.Spacing;

		k_topBar = new GamePanel()
			.SetVariant(PanelVariant.Filled)
			.SetLayout(LayoutDirection.Horizontal)
			.SetJustify(Justify.SpaceBetween)
			.SetAlignment(Align.Center)
			.SetPadding(spacing.XS, spacing.SM)
			.Build();
		k_topBar.style.position = Position.Absolute;
		k_topBar.style.top = 0;
		k_topBar.style.left = 0;
		k_topBar.style.right = 0;
		k_topBar.pickingMode = PickingMode.Position;

		// Left section: Time, Weather, Location
		var leftSection = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetLayout(LayoutDirection.Horizontal)
			.SetAlignment(Align.Center)
			.Build();

		k_dayTimeLabel = new GameLabel("Day 1, 8:00 AM")
			.SetStyle(LabelStyle.TitleMedium)
			.SetColor(LabelColor.Primary)
			.Build();

		k_weatherLabel = new GameLabel("☀ Clear")
			.SetStyle(LabelStyle.BodyMedium)
			.SetColor(LabelColor.Secondary)
			.Build();
		k_weatherLabel.style.marginLeft = spacing.MD;

		k_locationLabel = new GameLabel("Village Gate")
			.SetStyle(LabelStyle.BodyMedium)
			.SetColor(LabelColor.Tertiary)
			.Build();
		k_locationLabel.style.marginLeft = spacing.MD;

		leftSection.Content.Add(k_dayTimeLabel);
		leftSection.Content.Add(k_weatherLabel);
		leftSection.Content.Add(k_locationLabel);

		// Center section: Distance and Fog
		var centerSection = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetLayout(LayoutDirection.Horizontal)
			.SetAlignment(Align.Center)
			.Build();

		k_distanceLabel = new GameLabel("Distance: 0")
			.SetStyle(LabelStyle.BodyMedium)
			.SetColor(LabelColor.Secondary)
			.Build();

		k_fogLabel = new GameLabel("🌫 Fog: Light")
			.SetStyle(LabelStyle.BodyMedium)
			.SetColor(LabelColor.Warning)
			.Build();
		k_fogLabel.style.marginLeft = spacing.MD;

		centerSection.Content.Add(k_distanceLabel);
		centerSection.Content.Add(k_fogLabel);

		// Right section: Speed controls
		var rightSection = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetLayout(LayoutDirection.Horizontal)
			.SetAlignment(Align.Center)
			.Build();

		k_speedLabel = new GameLabel("▶ Normal")
			.SetStyle(LabelStyle.BodySmall)
			.SetColor(LabelColor.Secondary)
			.SetMarginRight(spacing.XS)
			.Build();

		k_pauseButton = new GameButton("⏸")
			.SetVariant(ButtonVariant.Outline)
			.SetSize(ButtonSize.Small)
			.OnClick(OnPauseClicked)
			.Build();

		k_playButton = new GameButton("▶")
			.SetVariant(ButtonVariant.Outline)
			.SetSize(ButtonSize.Small)
			.SetMarginLeft(spacing.XXS)
			.OnClick(OnPlayClicked)
			.Build();

		k_fastButton = new GameButton("⏩")
			.SetVariant(ButtonVariant.Outline)
			.SetSize(ButtonSize.Small)
			.SetMarginLeft(spacing.XXS)
			.OnClick(OnFastClicked)
			.Build();

		var menuButton = new GameButton("☰")
			.SetVariant(ButtonVariant.Ghost)
			.SetSize(ButtonSize.Small)
			.SetMarginLeft(spacing.MD)
			.OnClick(() => k_stateManager.PauseGame())
			.Build();

		rightSection.Content.Add(k_speedLabel);
		rightSection.Content.Add(k_pauseButton);
		rightSection.Content.Add(k_playButton);
		rightSection.Content.Add(k_fastButton);
		rightSection.Content.Add(menuButton);

		k_topBar.Content.Add(leftSection);
		k_topBar.Content.Add(centerSection);
		k_topBar.Content.Add(rightSection);

		Add(k_topBar);
	}

	/// <summary>
	/// Builds all floating panels at the bottom of the screen.
	/// Layout: [Character][Pace] ... [Resources+Actions] ... [Biome]
	/// </summary>
	private void BuildFloatingPanels() {
		var spacing = GameTheme.Current.Spacing;
		var entityStyles = GameTheme.Current.Components.EntityDetails;

		// Bottom-left: Character panel
		BuildCharacterOverlay(spacing, entityStyles);

		// Bottom-left (next to character): Pace controls
		BuildPaceOverlay(spacing, entityStyles);

		// Bottom-center: Resources and action buttons
		BuildResourceOverlay(spacing);

		// Bottom-right: Biome panel
		BuildBiomeOverlay(spacing);
	}

	/// <summary>
	/// Builds the character panel as a floating overlay in the bottom-left corner.
	/// </summary>
	private void BuildCharacterOverlay(SpacingSettings spacing, EntityDetailsPanelStyles entityStyles) {
		k_characterOverlay = new GamePanel()
			.SetVariant(PanelVariant.Card)
			.SetMinWidth(entityStyles.CompactWidth*1.5f)
			.SetPadding(spacing.XS)
			.SetPosition(Position.Absolute)
			.SetPickingMode(PickingMode.Position)
			.Build();

		k_characterOverlay.style.left = spacing.MD;
		k_characterOverlay.style.bottom = BOTTOM_OFFSET;

		k_characterPanel = new GameEntityDetailsPanel(k_gameDb, k_session)
			.SetViewMode(EntityViewMode.Compact)
			.SetExpandMode(ExpandMode.NewWindow)
			.OnOpenDetailsClicked(OnOpenCharacterDetails)
			.Build();

		k_characterOverlay.Content.Add(k_characterPanel);

		Add(k_characterOverlay);
	}

	/// <summary>
	/// Builds the pace controls as a vertical floating panel next to the character panel.
	/// </summary>
	private void BuildPaceOverlay(SpacingSettings spacing, EntityDetailsPanelStyles entityStyles) {
		k_paceOverlay = new GamePanel()
			.SetVariant(PanelVariant.Card)
			.SetLayout(LayoutDirection.Vertical)
			.SetAlignment(Align.Center)
			.SetPadding(spacing.XS)
			.SetPosition(Position.Absolute)
			.SetPickingMode(PickingMode.Position)
			.Build();

		// Position: to the right of the character panel
		float characterPanelWidth = (float)(entityStyles.CompactWidth*1.5+spacing.GapLG);
		k_paceOverlay.style.left = characterPanelWidth + spacing.XS;
		k_paceOverlay.style.bottom = BOTTOM_OFFSET;

		// Header
		var paceHeader = new GameLabel("Pace")
			.SetStyle(LabelStyle.Caption)
			.SetColor(LabelColor.Tertiary)
			.Build();
		paceHeader.style.marginBottom = spacing.XS;

		// Pace buttons - vertical stack
		k_sprintButton = new GameButton("💨")
			.SetVariant(ButtonVariant.Outline)
			.SetSize(ButtonSize.Small)
			.OnClick(() => SetPace(TravelPace.Sprint))
			.SetTooltip("Sprint - Maximum speed, high stamina cost")
			.Build();

		k_jogButton = new GameButton("🏃")
			.SetVariant(ButtonVariant.Outline)
			.SetSize(ButtonSize.Small)
			.OnClick(() => SetPace(TravelPace.Jog))
			.SetTooltip("Jog - Faster travel")
			.Build();
		k_jogButton.style.marginTop = spacing.XXS;

		k_walkButton = new GameButton("🚶")
			.SetVariant(ButtonVariant.Primary)
			.SetSize(ButtonSize.Small)
			.OnClick(() => SetPace(TravelPace.Walk))
			.SetTooltip("Walk - Normal pace, stamina recovery")
			.Build();
		k_walkButton.style.marginTop = spacing.XXS;

		k_sneakButton = new GameButton("🐾")
			.SetVariant(ButtonVariant.Outline)
			.SetSize(ButtonSize.Small)
			.OnClick(() => SetPace(TravelPace.Sneak))
			.SetTooltip("Sneak - Slow and stealthy")
			.Build();
		k_sneakButton.style.marginTop = spacing.XXS;

		k_waitButton = new GameButton("⏸")
			.SetVariant(ButtonVariant.Outline)
			.SetSize(ButtonSize.Small)
			.OnClick(() => SetPace(TravelPace.Wait))
			.SetTooltip("Wait - Stay in place, recover stamina")
			.Build();
		k_waitButton.style.marginTop = spacing.XXS;

		k_paceOverlay.Content.Add(paceHeader);
		k_paceOverlay.Content.Add(k_sprintButton);
		k_paceOverlay.Content.Add(k_jogButton);
		k_paceOverlay.Content.Add(k_walkButton);
		k_paceOverlay.Content.Add(k_sneakButton);
		k_paceOverlay.Content.Add(k_waitButton);

		Add(k_paceOverlay);
	}

	/// <summary>
	/// Builds the resource panel as a floating overlay in the bottom-center.
	/// </summary>
	private void BuildResourceOverlay(SpacingSettings spacing) {
		k_resourceOverlay = new GamePanel()
			.SetVariant(PanelVariant.Card)
			.SetLayout(LayoutDirection.Vertical)
			.SetAlignment(Align.Center)
			.SetPadding(spacing.SM)
			.SetPosition(Position.Absolute)
			.SetPickingMode(PickingMode.Position)
			.Build();

		// Position: bottom-center
		k_resourceOverlay.style.left = Length.Percent(50);
		k_resourceOverlay.style.bottom = BOTTOM_OFFSET;
		k_resourceOverlay.style.translate = new Translate(Length.Percent(-50), 0);

		// Resources row
		var resourceRow = new GameContainer("resource-row")
			.SetRow()
			.SetAlignItems(Align.Center)
			.SetJustifyContent(Justify.Center)
			.Build();

		k_goldLabel = CreateResourceLabel("🪙", "0", "Gold");
		k_foodLabel = CreateResourceLabel("🍖", "0", "Food");
		k_foodLabel.style.marginLeft = spacing.MD;
		k_medicalLabel = CreateResourceLabel("💊", "0", "Medical Supplies");
		k_medicalLabel.style.marginLeft = spacing.MD;
		k_campingLabel = CreateResourceLabel("🏕", "0", "Camp Supplies");
		k_campingLabel.style.marginLeft = spacing.MD;
		k_moraleLabel = CreateResourceLabel("😊", "100%", "Morale");
		k_moraleLabel.style.marginLeft = spacing.MD;

		resourceRow
			.AddChild(k_goldLabel)
			.AddChild(k_foodLabel)
			.AddChild(k_medicalLabel)
			.AddChild(k_campingLabel)
			.AddChild(k_moraleLabel);

		// Divider
		var divider = new GameDivider()
			.SetMargin(spacing.XS, 0)
			.Build();

		// Action buttons row
		var actionRow = new GameContainer("action-row")
			.SetRow()
			.SetAlignItems(Align.Center)
			.SetJustifyContent(Justify.Center)
			.Build();

		k_campButton = new GameButton("🏕 Camp")
			.SetVariant(ButtonVariant.Outline)
			.SetSize(ButtonSize.Small)
			.OnClick(OnCampClicked)
			.Build();

		k_inventoryButton = new GameButton("🎒")
			.SetVariant(ButtonVariant.Ghost)
			.SetSize(ButtonSize.Small)
			.OnClick(OnInventoryClicked)
			.SetTooltip("Inventory")
			.Build();
		k_inventoryButton.style.marginLeft = spacing.XS;

		k_characterButton = new GameButton("👤")
			.SetVariant(ButtonVariant.Ghost)
			.SetSize(ButtonSize.Small)
			.OnClick(OnOpenCharacterDetails)
			.SetTooltip("Character")
			.Build();
		k_characterButton.style.marginLeft = spacing.XXS;

		k_mapButton = new GameButton("🗺")
			.SetVariant(ButtonVariant.Ghost)
			.SetSize(ButtonSize.Small)
			.OnClick(() => GameToast.Show("Map (coming soon)", ToastType.Info))
			.SetTooltip("Map")
			.Build();
		k_mapButton.style.marginLeft = spacing.XXS;

		k_retreatButton = new GameButton("🏠 Return")
			.SetVariant(ButtonVariant.Outline)
			.SetSize(ButtonSize.Small)
			.OnClick(OnRetreatClicked)
			.Build();
		k_retreatButton.style.marginLeft = spacing.MD;

		actionRow
			.AddChild(k_campButton)
			.AddChild(k_inventoryButton)
			.AddChild(k_characterButton)
			.AddChild(k_mapButton)
			.AddChild(k_retreatButton);

		k_resourceOverlay.Content.Add(resourceRow);
		k_resourceOverlay.Content.Add(divider);
		k_resourceOverlay.Content.Add(actionRow);

		Add(k_resourceOverlay);
	}

	/// <summary>
	/// Builds the biome panel as a floating overlay in the bottom-right corner.
	/// </summary>
	private void BuildBiomeOverlay(SpacingSettings spacing) {
		var biomeStyles = GameTheme.Current.Components.BiomeDisplay;
		var panelStyles = GameTheme.Current.Components.Panel;

		k_biomeOverlay = new GamePanel()
			.SetVariant(PanelVariant.Card)
			.SetPadding(spacing.MD)
			.SetPosition(Position.Absolute)
			.SetPickingMode(PickingMode.Position)
			.SetMinWidth(biomeStyles.CompactWidth)
			.SetFlexGrow(1f)
			.Build();

		k_biomeOverlay.style.right = spacing.MD;
		k_biomeOverlay.style.bottom = BOTTOM_OFFSET;
		k_biomeOverlay.style.width = panelStyles.MinWidth + spacing.MD;  

		k_biomePanel = new BiomeDisplayPanel(k_gameDb, k_expeditionManager)
			.SetExpanded(false)
			.SetFlexGrow()
			.Build();

		k_biomeOverlay.Content.Add(k_biomePanel);

		Add(k_biomeOverlay);
	}

	private GamePanel CreateResourceLabel(string icon, string value, string tooltip = "") {
		var spacing = GameTheme.Current.Spacing;

		var resourcePanel = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetLayout(LayoutDirection.Horizontal)
			.SetAlignment(Align.Center)
			.Build();

		if (!string.IsNullOrEmpty(tooltip)) {
			resourcePanel.tooltip = tooltip;
		}

		var iconLabel = new GameLabel(icon)
			.SetStyle(LabelStyle.TitleMedium)
			.Build();

		var valueLabel = new GameLabel(value)
			.SetStyle(LabelStyle.TitleSmall)
			.SetColor(LabelColor.Primary)
			.Build();
		valueLabel.style.marginLeft = spacing.XXS;
		valueLabel.name = "value";

		resourcePanel.Content.Add(iconLabel);
		resourcePanel.Content.Add(valueLabel);

		return resourcePanel;
	}

	private void BuildInfoCard() {
		var spacing = GameTheme.Current.Spacing;
		k_infoCard = new GamePanel()
			.SetVariant(PanelVariant.Card)
			.SetPadding(spacing.SM)
			.Build();
		k_infoCard.style.position = Position.Absolute;
		k_infoCard.style.bottom = 200;
		k_infoCard.style.left = spacing.MD;
		k_infoCard.style.width = 500;
		k_infoCard.style.display = DisplayStyle.None;
		k_infoCard.pickingMode = PickingMode.Position;

		Add(k_infoCard);
	}

	private void BuildWarningsPanel() {
		var spacing = GameTheme.Current.Spacing;
		var borders = GameTheme.Current.Borders;

		k_warningsPanel = new GamePanel()
			.SetVariant(PanelVariant.Outlined)
			.SetLayout(LayoutDirection.Vertical)
			.SetPadding(spacing.XS)
			.Build();
		k_warningsPanel.style.position = Position.Absolute;
		k_warningsPanel.style.top = 120;
		k_warningsPanel.style.right = spacing.MD;
		k_warningsPanel.style.maxWidth = 600;
		k_warningsPanel.style.display = DisplayStyle.None;
		k_warningsPanel.style.borderLeftColor = GameTheme.Current.Colors.Warning;
		k_warningsPanel.style.borderLeftWidth = borders.WidthThick;
		k_warningsPanel.pickingMode = PickingMode.Position;

		Add(k_warningsPanel);
	}

	private void CreatePanels() {
		// Event panel - modal for events
		k_eventPanel = new EventPanel(k_eventManager, k_session, k_stateManager);
		k_eventPanel.OnEventCompleted += OnEventCompleted;
		k_eventPanel.OnRewardPendingForUI += OnRewardPending;
		Add(k_eventPanel);

		// Path choice panel - modal for branch decisions
		k_pathChoicePanel = new PathChoicePanel(k_expeditionManager);
		k_pathChoicePanel.OnPathChosen += OnPathChosen;
		Add(k_pathChoicePanel);

		// Camp panel - modal for camping
		k_campPanel = new CampPanel(k_expeditionManager, k_session, k_stateManager);
		k_campPanel.OnCampBroken += OnCampBroken;
		Add(k_campPanel);

		// Dungeon panel - modal for dungeon encounters
		k_dungeonPanel = new DungeonPanel(k_dungeonManager);
		k_dungeonPanel.OnDungeonDismissed += OnDungeonDismissed;
		Add(k_dungeonPanel);

		// Level up panel - modal for level ups at camp
		k_levelUpPanel = new LevelUpPanel(k_stateManager, k_session, k_gameDb);
		k_levelUpPanel.OnLevelUpComplete += OnLevelUpComplete;
		k_levelUpPanel.OnLevelUpSkipped += OnLevelUpSkipped;
		Add(k_levelUpPanel);

		// Item reward panel - ADD LAST so it renders on top of all other modals
		k_itemRewardPanel = new ItemRewardPanel();
		Add(k_itemRewardPanel);
	}

	#endregion

	#region Event Subscriptions

	private void SubscribeToEvents() {
		k_stateManager.OnPhaseChanged += OnPhaseChanged;

		k_expeditionManager.OnExpeditionStarted += OnExpeditionStarted;
		k_expeditionManager.OnExpeditionEnded += OnExpeditionEnded;
		k_expeditionManager.OnEventTriggered += OnEventTriggered;
		k_expeditionManager.OnPathChoiceDataReady += OnPathChoiceRequired;
		k_expeditionManager.OnPaceChanged += OnPaceChanged;
		k_expeditionManager.OnSimSpeedChanged += OnSimSpeedChanged;
		k_expeditionManager.OnWeatherChanged += OnWeatherChanged;
		k_expeditionManager.OnDayChanged += OnDayChanged;
		k_expeditionManager.OnSpecialLocationReached += OnSpecialLocationReached;
		k_eventManager.OnEventStarted += OnEventStarted;

		// Camp manager events
		k_campManager.OnCampSetUp += OnCampSetUp;
		k_campPanel.OnLevelUpRequested += OnLevelUpRequested;
	}

	private CampManager k_campManager => k_expeditionManager.CampManager;

	private void UnsubscribeFromEvents() {
		k_stateManager.OnPhaseChanged -= OnPhaseChanged;

		k_expeditionManager.OnExpeditionStarted -= OnExpeditionStarted;
		k_expeditionManager.OnExpeditionEnded -= OnExpeditionEnded;
		k_expeditionManager.OnEventTriggered -= OnEventTriggered;
		k_expeditionManager.OnPathChoiceDataReady -= OnPathChoiceRequired;
		k_expeditionManager.OnPaceChanged -= OnPaceChanged;
		k_expeditionManager.OnSimSpeedChanged -= OnSimSpeedChanged;
		k_expeditionManager.OnWeatherChanged -= OnWeatherChanged;
		k_expeditionManager.OnDayChanged -= OnDayChanged;
		k_expeditionManager.OnSpecialLocationReached -= OnSpecialLocationReached;

		k_eventManager.OnEventStarted -= OnEventStarted;

		k_campManager.OnCampSetUp -= OnCampSetUp;

		k_eventPanel.OnEventCompleted -= OnEventCompleted;
		k_eventPanel.OnRewardPendingForUI -= OnRewardPending;
		k_pathChoicePanel.OnPathChosen -= OnPathChosen;
		k_campPanel.OnCampBroken -= OnCampBroken;
		k_dungeonPanel.OnDungeonDismissed -= OnDungeonDismissed;
		k_campPanel.OnLevelUpRequested -= OnLevelUpRequested;
		k_levelUpPanel.OnLevelUpComplete -= OnLevelUpComplete;
		k_levelUpPanel.OnLevelUpSkipped -= OnLevelUpSkipped;
	}

	#endregion

	#region Phase Management

	private void OnRewardPending(PendingReward reward, Action<ItemInstance?> onComplete) {
		k_itemRewardPanel.ShowReward(reward, onComplete);
	}

	private void OnPhaseChanged(GamePhase oldPhase, GamePhase newPhase) {
		bool shouldShow = newPhase == GamePhase.Expedition || 
		                  newPhase == GamePhase.Event ||
		                  newPhase == GamePhase.Camp;
		style.display = shouldShow ? DisplayStyle.Flex : DisplayStyle.None;

		// Save state when entering combat
		if (newPhase == GamePhase.Combat && oldPhase != GamePhase.Combat) {
			k_preEventSimSpeed = k_expeditionManager.SimSpeed;
			k_preEventPace = k_expeditionManager.CurrentPace;
			k_expeditionManager.SetSimulationSpeed(SimulationSpeed.Paused);
			k_expeditionManager.SetTravelPace(TravelPace.Wait);
		}
        
		// Restore state when leaving combat
		if (oldPhase == GamePhase.Combat && newPhase == GamePhase.Expedition) {
			k_expeditionManager.SetTravelPace(k_preEventPace);
			k_expeditionManager.SetSimulationSpeed(k_preEventSimSpeed);
            
			var travel = k_expeditionManager.Travel;
			if (travel != null && !travel.IsTraveling) {
				k_expeditionManager.ContinueTravel();
			}
		}

		if (shouldShow) {
			StartUpdateLoop();
			RefreshUI();

			// Show camp panel if in camp phase
			if (newPhase == GamePhase.Camp && k_expeditionManager.IsCamped) {
				k_campPanel.Show();
			}
		} else {
			StopUpdateLoop();
			k_campPanel.Hide();
		}
	}

	private void StartUpdateLoop() {
		StopUpdateLoop();
		k_updateSchedule = schedule.Execute(UpdateUI).Every(100);
	}

	private void StopUpdateLoop() {
		k_updateSchedule?.Pause();
		k_updateSchedule = null;
	}

	public void SetRewardManager(RewardManager rewardManager) {
		k_itemRewardPanel.SetRewardManager(rewardManager);
	}

	#endregion

	#region Expedition Event Handlers

	private void OnLevelUpRequested() {
		k_levelUpPanel.Show();
	}

	private void OnLevelUpComplete() {
		k_campPanel.Show();
		k_campPanel.RefreshUI();
	}

	private void OnLevelUpSkipped() {
		k_campPanel.Show();
	}

	private void OnExpeditionStarted() {
		RefreshUI();

		var travel = k_expeditionManager.Travel;
		if (travel?.CurrentNode != null && !travel.IsTraveling) {
			if (k_expeditionManager.ContinueTravel()) {
				Debug.Log("ExpeditionScreen: Auto-started travel");
			}
		}
	}

	private void OnExpeditionEnded(ExpeditionEndReason reason) {
		StopUpdateLoop();
	}

	private void OnEventTriggered(string eventId) {
		Debug.Log($"ExpeditionScreen: Event triggered: {eventId}");
		k_session.TriggerEvent(eventId);
	}

	private void OnEventStarted(Core.Prototypes.Events.EventProto evt) {
		// Store current state before pausing
		k_preEventSimSpeed = k_expeditionManager.SimSpeed;
		k_preEventPace = k_expeditionManager.CurrentPace;
        
		// Pause simulation and stop movement
		k_expeditionManager.SetSimulationSpeed(SimulationSpeed.Paused);
		k_expeditionManager.SetTravelPace(TravelPace.Wait);
        
		k_eventPanel.ShowEvent(evt);
		k_stateManager.SetPhase(GamePhase.Event);
	}

	private void OnEventCompleted() {
		k_stateManager.SetPhase(GamePhase.Expedition);

		// Restore previous pace (not sim speed - let player control that)
		k_expeditionManager.SetTravelPace(k_preEventPace);
		k_expeditionManager.SetSimulationSpeed(k_preEventSimSpeed);

		var travel = k_expeditionManager.Travel;
		if (travel != null && !travel.IsTraveling) {
			k_expeditionManager.ContinueTravel();
		}
	}

	private void OnPathChoiceRequired(List<PathChoiceData> choices) {
		// Store and pause
		k_preEventSimSpeed = k_expeditionManager.SimSpeed;
		k_preEventPace = k_expeditionManager.CurrentPace;
        
		k_expeditionManager.SetSimulationSpeed(SimulationSpeed.Paused);
		k_expeditionManager.SetTravelPace(TravelPace.Wait);
        
		k_pathChoicePanel.ShowChoices(choices);
	}

	private void OnPathChosen(PathChoiceData choice) {
		// FIRST: Make the path choice - this sets up the destination
		k_expeditionManager.ChoosePathWithModifiers(choice);
		
        
		// THEN: Restore pace and speed (destination is already set, so ContinueTravel won't re-trigger choice)
		k_expeditionManager.SetTravelPace(k_preEventPace);
		k_expeditionManager.SetSimulationSpeed(k_preEventSimSpeed);
	}

	private void OnPaceChanged(TravelPace pace) {
		UpdatePaceButtons(pace);
	}

	private void OnSimSpeedChanged(SimulationSpeed speed) {
		UpdateSpeedDisplay(speed);
	}

	private void OnWeatherChanged(WeatherType weather) {
		UpdateWeatherDisplay(weather);
	}

	private void OnDayChanged(int day) {
		RefreshUI();
	}

	private void OnSpecialLocationReached(PathNode node) {
		// Pause for special locations (dungeons, etc.)
		k_preEventSimSpeed = k_expeditionManager.SimSpeed;
		k_preEventPace = k_expeditionManager.CurrentPace;
        
		k_expeditionManager.SetSimulationSpeed(SimulationSpeed.Paused);
		k_expeditionManager.SetTravelPace(TravelPace.Wait);
        
		ShowInfoCard(node);

		if (node.Type == PathNodeType.Dungeon && !string.IsNullOrEmpty(node.LocationProtoId)) {
			k_currentDungeonNodeId = node.Id;
			var dungeonId = new Core.Prototypes.Locations.DungeonProto.ID(node.LocationProtoId);
			k_dungeonManager.ApproachDungeon(dungeonId);
		}
	}

	private void OnCampSetUp(CampQuality quality) {
		// Camp pauses simulation
		k_expeditionManager.SetSimulationSpeed(SimulationSpeed.Paused);
		k_expeditionManager.SetTravelPace(TravelPace.Wait);
        
		k_stateManager.SetPhase(GamePhase.Camp);
		k_campPanel.Show();
	}

	private void OnCampBroken() {
		k_stateManager.SetPhase(GamePhase.Expedition);

		k_expeditionManager.SetTravelPace(k_preEventPace);
		k_expeditionManager.SetSimulationSpeed(k_preEventSimSpeed);

		var travel = k_expeditionManager.Travel;
		if (travel != null && !travel.IsTraveling) {
			k_expeditionManager.ContinueTravel();
		}
	}

	private void OnDungeonDismissed() {
		// Resume movement
        
		if (k_currentDungeonNodeId.HasValue) {
			k_expeditionManager.OnDungeonBypassed(k_currentDungeonNodeId.Value);
			k_currentDungeonNodeId = null;
		} else {
			var travel = k_expeditionManager.Travel;
			if (travel != null && !travel.IsTraveling) {
				k_expeditionManager.ContinueTravel();
			}
		}

		k_expeditionManager.SetTravelPace(k_preEventPace);
		k_expeditionManager.SetSimulationSpeed(k_preEventSimSpeed);
	}

	#endregion

	#region UI Updates

	private void UpdateUI() {
		if (!k_expeditionManager.IsActive) {
			UnityEngine.Debug.LogError("[ExpeditionManager]: UpdateUI was called but IsActive is false!");
			return;
		}

		var run = k_session.CurrentRun;
		if (run == null) {
			UnityEngine.Debug.LogError("[ExpeditionManager]: Updating UI in Expedition Screen but no run is active?");
			return;
		}

		var expeditionManager = k_expeditionManager;
		var travelData = expeditionManager.Travel;

		// Update time display
		k_dayTimeLabel.SetText($"Day {GameTime.Instance.Day}, {FormatTime(GameTime.Instance.Hour, GameTime.Instance.Minute)}");

		// Update location
		k_locationLabel.SetText(travelData?.CurrentNode?.Name ?? expeditionManager.CurrentTerrain.ToString());

		// Update distance
		k_distanceLabel.SetText($"Distance: {travelData?.DistanceFromVillage ?? 0:F0}");

		// Update fog
		string fogTierName = expeditionManager.CurrentFogRegion?.CurrentTier.GetDisplayName() ?? "None";
		k_fogLabel.SetText($"🌫 {fogTierName}");

		// Update floating panels
		k_characterPanel.RefreshUI();
		k_biomePanel.RefreshUI();

		// Update resources
		var liveCharacter = k_expeditionManager.RunState?.Character;
		if (liveCharacter == null) {
			UnityEngine.Debug.LogError("[ExpeditionManager]: Updating resources in Expedition Screen but no live character?");
			return;
		}

		UpdateResourceLabel(k_goldLabel, liveCharacter.BaseStats.GetInt(Ids.Stats.Expedition.GoldOnHand, 69));
		UpdateResourceLabel(k_foodLabel, liveCharacter.BaseStats.GetInt(Ids.Stats.Expedition.FoodOnHand, 69));
		UpdateResourceLabel(k_medicalLabel, liveCharacter.BaseStats.GetInt(Ids.Stats.Expedition.MedicalSupplies, 69));
		UpdateResourceLabel(k_campingLabel, liveCharacter.BaseStats.GetInt(Ids.Stats.Expedition.CampingSupplies, 69));

		// Update morale with color coding
		UpdateMoraleLabel(liveCharacter.CurrentMorale);

		// Update warnings
		UpdateWarnings(expeditionManager.GetWarnings());

		// Update button states
		bool canCamp = !expeditionManager.IsTraveling || expeditionManager.CurrentPace == TravelPace.Wait;
		k_campButton.SetEnabled(canCamp && !expeditionManager.IsCamped);
	}

	private void UpdateMoraleLabel(float morale) {
		var valueLabel = k_moraleLabel.Q<Label>("value");
		if (valueLabel == null) return;

		Percent moralePercent = morale.AsFractionPercent();
		valueLabel.text = moralePercent.ToString();

		var colors = GameTheme.Current.Colors;
		valueLabel.style.color = moralePercent switch {
			_ when moralePercent <= 20 => colors.Error,
			_ when moralePercent <= 40 => colors.Warning,
			_ when moralePercent >= 80 => colors.Success,
			_ => colors.TextPrimary
		};
	}

	private void RefreshUI() {
		UpdateUI();

		var expedition = k_expeditionManager;
		UpdatePaceButtons(expedition.CurrentPace);
		UpdateSpeedDisplay(expedition.SimSpeed);
		UpdateWeatherDisplay(expedition.Weather.CurrentWeather);
	}

	private void UpdateResourceLabel(GamePanel inPanel, int value) {
		var valueLabel = inPanel.Q<GameLabel>("value");

		if (valueLabel == null) {
			UnityEngine.Debug.LogError($"[ExpeditionScreen]: Resource panel '{inPanel.name}' is missing a 'value' label!");
			return;
		}

		valueLabel.SetText(value.ToString());
	}

	private void UpdatePaceButtons(TravelPace pace) {
		k_waitButton.SetVariant(pace == TravelPace.Wait ? ButtonVariant.Primary : ButtonVariant.Outline);
		k_sneakButton.SetVariant(pace == TravelPace.Sneak ? ButtonVariant.Primary : ButtonVariant.Outline);
		k_walkButton.SetVariant(pace == TravelPace.Walk ? ButtonVariant.Primary : ButtonVariant.Outline);
		k_jogButton.SetVariant(pace == TravelPace.Jog ? ButtonVariant.Primary : ButtonVariant.Outline);
		k_sprintButton.SetVariant(pace == TravelPace.Sprint ? ButtonVariant.Primary : ButtonVariant.Outline);
	}

	private void UpdateSpeedDisplay(SimulationSpeed speed) {
		string icon = speed switch {
			SimulationSpeed.Paused => "⏸",
			SimulationSpeed.Slow => "▶",
			SimulationSpeed.Normal => "▶▶",
			SimulationSpeed.Fast => "⏩",
			SimulationSpeed.VeryFast => "⏩⏩",
			_ => "▶"
		};
		k_speedLabel.SetText($"{icon} {speed}");
	}

	private void UpdateWeatherDisplay(WeatherType weather) {
		string icon = weather switch {
			WeatherType.Clear => "☀",
			WeatherType.Cloudy => "☁",
			WeatherType.Overcast => "🌥",
			WeatherType.LightRain => "🌧",
			WeatherType.HeavyRain => "⛈",
			WeatherType.Storm => "🌩",
			WeatherType.Fog => "🌫",
			WeatherType.Snow => "❄",
			_ => "☀"
		};
		k_weatherLabel.SetText($"{icon} {weather}");
	}

	private void UpdateWarnings(List<string> warnings) {
		if (warnings.Count == 0) {
			k_warningsPanel.style.display = DisplayStyle.None;
			return;
		}

		k_warningsPanel.ClearChildren();
		k_warningsPanel.style.display = DisplayStyle.Flex;

		foreach (var warning in warnings) {
			var warningLabel = new GameLabel($"⚠ {warning}")
				.SetStyle(LabelStyle.BodySmall)
				.SetColor(LabelColor.Warning)
				.Build();
			k_warningsPanel.AddChild(warningLabel);
		}
	}

	private void ShowInfoCard(PathNode node) {
		var spacing = GameTheme.Current.Spacing;

		k_infoCard.ClearChildren();
		k_infoCard.style.display = DisplayStyle.Flex;

		var titleLabel = new GameLabel(node.Name ?? node.Type.GetDisplayName())
			.SetStyle(LabelStyle.TitleMedium)
			.SetColor(LabelColor.Primary)
			.Build();

		var typeLabel = new GameLabel(node.Type.GetDisplayName())
			.SetStyle(LabelStyle.Caption)
			.SetColor(LabelColor.Tertiary)
			.Build();

		k_infoCard.AddChild(titleLabel);
		k_infoCard.AddChild(typeLabel);

		if (!string.IsNullOrEmpty(node.Description)) {
			var descLabel = new GameLabel(node.Description)
				.SetStyle(LabelStyle.BodySmall)
				.SetColor(LabelColor.Secondary)
				.Build();
			descLabel.style.marginTop = spacing.XS;
			k_infoCard.AddChild(descLabel);
		}

		schedule.Execute(() => {
			k_infoCard.style.display = DisplayStyle.None;
		}).ExecuteLater(5000);
	}

	private string FormatTime(int hour, int minute) {
		int displayHour = hour % 12;
		if (displayHour == 0) displayHour = 12;
		string ampm = hour < 12 ? "AM" : "PM";
		return $"{displayHour}:{minute:D2} {ampm}";
	}

	#endregion

	#region Button Handlers

	private void OnOpenCharacterDetails() {
		if (k_uiManager.TryGetWindow("character", out var window) && window is CharacterScreen charScreen) {
			charScreen.Open();
		} else {
			k_uiManager.ShowWindow("character");
		}
	}

	private void OnInventoryClicked() {
		OnOpenCharacterDetails();
	}

	private void OnPauseClicked() {
		k_expeditionManager.SetSimulationSpeed(SimulationSpeed.Paused);
	}

	private void OnPlayClicked() {
		k_expeditionManager.SetSimulationSpeed(SimulationSpeed.Normal);
	}

	private void OnFastClicked() {
		var current = k_expeditionManager.SimSpeed;
		var next = current switch {
			SimulationSpeed.Fast => SimulationSpeed.VeryFast,
			SimulationSpeed.VeryFast => SimulationSpeed.Normal,
			_ => SimulationSpeed.Fast
		};
		k_expeditionManager.SetSimulationSpeed(next);
	}

	private void SetPace(TravelPace pace) {
		k_expeditionManager.SetTravelPace(pace);
	}

	private void OnCampClicked() {
		if (k_expeditionManager.SetupCamp()) {
			GameToast.Show("Setting up camp...", ToastType.Info);
		} else {
			GameToast.Show("Cannot set up camp while traveling", ToastType.Warning);
		}
	}

	private void OnRetreatClicked() {
		GameDialog.Confirm(
			"Return to Village",
			"Are you sure you want to return to the village? Your progress will be saved.",
			() => k_session.ReturnToVillage(),
			null
		).Show(this);
	}

	#endregion

	#region Cleanup

	public new void RemoveFromHierarchy() {
		UnsubscribeFromEvents();
		StopUpdateLoop();
		k_characterPanel?.RemoveFromHierarchy();
		k_characterOverlay?.RemoveFromHierarchy();
		k_paceOverlay?.RemoveFromHierarchy();
		k_resourceOverlay?.RemoveFromHierarchy();
		k_biomePanel?.RemoveFromHierarchy();
		k_biomeOverlay?.RemoveFromHierarchy();
		base.RemoveFromHierarchy();
	}

	#endregion
}
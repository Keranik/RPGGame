using RPGGame.Core;
using RPGGame.Core.Input;
using RPGGame.Core.Isometric.Rendering;
using RPGGame.Core.Prototypes.Village;
using RPGGame.Core.Simulation;
using RPGGame.Core.Village;
using RPGGame.UI.Components;
using RPGGame.UI.Styles;
using RPGGame.UI.Village;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Screens;

/// <summary>
/// The village screen manages the village phase UI.
/// Panels register as modals when open, blocking world input automatically.
/// 
/// <para>
/// Uses <see cref="VillageIsoRenderer"/> for all world rendering and input handling.
/// The renderer creates GameObjects for tiles and buildings with proper isometric positioning.
/// </para>
/// </summary>
public class VillageScreen : VisualElement {
    #region Fields

    private readonly GameStateManager _stateManager;
    private readonly VillageManager _villageManager;
    private readonly VillageIsoRenderer _villageRenderer;
    private readonly GameSession _session;
    private readonly UiManager _uiManager;

    // UI Panels
    private BuildingPanel _buildingPanel = null!;
    private EmptyPlotPanel _emptyPlotPanel = null!;
    private GatePanel _gatePanel = null!;
    private AnchorPanel _anchorPanel = null!;

    // HUD elements
    private GamePanel _topBar = null!;
    private GamePanel _bottomBar = null!;
    private GameLabel _upgradePointsLabel = null!;
    private GameLabel _dayLabel = null!;

    #endregion

    #region Constructor

    /// <summary>
    /// Creates a new VillageScreen.
    /// </summary>
    /// <param name="stateManager">Game state manager for phase transitions.</param>
    /// <param name="villageManager">Village manager for building and upgrade logic.</param>
    /// <param name="villageRenderer">Isometric renderer for village visualization.</param>
    /// <param name="session">Current game session.</param>
    /// <param name="uiManager">UI manager for panel management.</param>
    public VillageScreen(
        GameStateManager stateManager,
        VillageManager villageManager,
        VillageIsoRenderer villageRenderer,
        GameSession session,
        UiManager uiManager
    ) {
        _stateManager = stateManager;
        _villageManager = villageManager;
        _villageRenderer = villageRenderer;
        _session = session;
        _uiManager = uiManager;

        BuildUI();
        CreatePanels();
        SubscribeToEvents();

        style.display = DisplayStyle.None;

        Debug.Log("VillageScreen: Created");
    }

    #endregion

    #region UI Building

    private void BuildUI() {
        style.position = Position.Absolute;
        style.left = 0;
        style.top = 0;
        style.right = 0;
        style.bottom = 0;

        // Let clicks through to world renderer
        pickingMode = PickingMode.Ignore;

        BuildTopBar();
        BuildBottomBar();
    }

    private void BuildTopBar() {
        var theme = GameTheme.Current;
        var spacing = theme.Spacing;

        _topBar = new GamePanel()
            .SetVariant(PanelVariant.Filled)
            .SetLayout(LayoutDirection.Horizontal)
            .SetJustify(Justify.SpaceBetween)
            .SetAlignment(Align.Center)
            .SetPadding(spacing.SM, spacing.MD)
            .Build();
        _topBar.style.position = Position.Absolute;
        _topBar.style.top = 0;
        _topBar.style.left = 0;
        _topBar.style.right = 0;
        // Important: HUD bars block clicks
        _topBar.pickingMode = PickingMode.Position;

        var leftSection = new GamePanel()
            .SetVariant(PanelVariant.Ghost)
            .SetLayout(LayoutDirection.Horizontal)
            .SetAlignment(Align.Center)
            .Build();

        var villageName = new GameLabel("🏘 Haven Village")
            .SetStyle(LabelStyle.TitleMedium)
            .SetColor(LabelColor.Primary)
            .Build();

        _dayLabel = new GameLabel("Day 1")
            .SetStyle(LabelStyle.BodySmall)
            .SetColor(LabelColor.Secondary)
            .Build();
        _dayLabel.style.marginLeft = spacing.MD;

        leftSection.Content.Add(villageName);
        leftSection.Content.Add(_dayLabel);

        var rightSection = new GamePanel()
            .SetVariant(PanelVariant.Ghost)
            .SetLayout(LayoutDirection.Horizontal)
            .SetAlignment(Align.Center)
            .Build();

        var upgradeIcon = new GameLabel("⬆")
            .SetStyle(LabelStyle.TitleMedium)
            .Build();

        _upgradePointsLabel = new GameLabel("0")
            .SetStyle(LabelStyle.TitleMedium)
            .SetColor(LabelColor.Primary)
            .Build();
        _upgradePointsLabel.style.marginLeft = spacing.XXS;

        var upgradeLabel = new GameLabel("Upgrade Points")
            .SetStyle(LabelStyle.Caption)
            .SetColor(LabelColor.Tertiary)
            .Build();
        upgradeLabel.style.marginLeft = spacing.XXS;

        var menuButton = new GameButton("☰")
            .SetVariant(ButtonVariant.Ghost)
            .SetSize(ButtonSize.Medium)
            .OnClick(() => _stateManager.PauseGame())
            .Build();
        menuButton.style.marginLeft = spacing.MD;

        rightSection.Content.Add(upgradeIcon);
        rightSection.Content.Add(_upgradePointsLabel);
        rightSection.Content.Add(upgradeLabel);
        rightSection.Content.Add(menuButton);

        _topBar.Content.Add(leftSection);
        _topBar.Content.Add(rightSection);

        Add(_topBar);
    }

    private void BuildBottomBar() {
        var theme = GameTheme.Current;
        var spacing = theme.Spacing;

        _bottomBar = new GamePanel()
            .SetVariant(PanelVariant.Filled)
            .SetLayout(LayoutDirection.Horizontal)
            .SetJustify(Justify.Center)
            .SetAlignment(Align.Center)
            .SetPadding(spacing.SM, spacing.MD)
            .Build();
        _bottomBar.style.position = Position.Absolute;
        _bottomBar.style.bottom = 0;
        _bottomBar.style.left = 0;
        _bottomBar.style.right = 0;
        _bottomBar.pickingMode = PickingMode.Position;

        var characterButton = new GameButton("👤 Character")
            .SetVariant(ButtonVariant.Outline)
            .SetSize(ButtonSize.Medium)
            .OnClick(() => GameToast.Show("Character panel coming soon!", ToastType.Info))
            .Build();
        characterButton.style.marginRight = spacing.XS;

        var inventoryButton = new GameButton("🎒 Inventory")
            .SetVariant(ButtonVariant.Outline)
            .SetSize(ButtonSize.Medium)
            .OnClick(() => GameToast.Show("Inventory panel coming soon!", ToastType.Info))
            .Build();
        inventoryButton.style.marginRight = spacing.XS;

        var codexButton = new GameButton("📖 Codex")
            .SetVariant(ButtonVariant.Outline)
            .SetSize(ButtonSize.Medium)
            .OnClick(() => GameToast.Show("Codex panel coming soon!", ToastType.Info))
            .Build();
        codexButton.style.marginRight = spacing.MD;

        var departButton = new GameButton("⚔ Depart")
            .SetVariant(ButtonVariant.Primary)
            .SetSize(ButtonSize.Large)
            .OnClick(() => ShowPanel(_gatePanel))
            .Build();

        _bottomBar.Content.Add(characterButton);
        _bottomBar.Content.Add(inventoryButton);
        _bottomBar.Content.Add(codexButton);
        _bottomBar.Content.Add(departButton);

        Add(_bottomBar);
    }

    private void CreatePanels() {
        _buildingPanel = new BuildingPanel(_villageManager, _stateManager, _uiManager);
        Add(_buildingPanel);

        _emptyPlotPanel = new EmptyPlotPanel(_villageManager, _uiManager);
        Add(_emptyPlotPanel);

        _gatePanel = new GatePanel(_villageManager, _stateManager, _session);
        Add(_gatePanel);

        _anchorPanel = new AnchorPanel(_villageManager, _stateManager);
        Add(_anchorPanel);
    }

    #endregion

    #region Event Subscriptions

    private void SubscribeToEvents() {
        // State manager events
        _stateManager.OnPhaseChanged += OnPhaseChanged;

        // Village renderer events (isometric)
        _villageRenderer.OnBuildingClicked += OnBuildingClicked;
        _villageRenderer.OnEmptyPlotClicked += OnEmptyPlotClicked;
        _villageRenderer.OnGateClicked += OnGateClicked;
        _villageRenderer.OnAnchorClicked += OnAnchorClicked;

        // Village manager events
        _villageManager.OnUpgradePointsChanged += OnUpgradePointsChanged;

        // Panel events
        _buildingPanel.OnClosed += OnPanelClosed;
        _emptyPlotPanel.OnClosed += OnPanelClosed;
        _emptyPlotPanel.OnBuildingSelected += OnBuildingPlaced;
        _gatePanel.OnClosed += OnPanelClosed;
        _anchorPanel.OnClosed += OnPanelClosed;
    }

    private void UnsubscribeFromEvents() {
        // State manager events
        _stateManager.OnPhaseChanged -= OnPhaseChanged;

        // Village renderer events
        _villageRenderer.OnBuildingClicked -= OnBuildingClicked;
        _villageRenderer.OnEmptyPlotClicked -= OnEmptyPlotClicked;
        _villageRenderer.OnGateClicked -= OnGateClicked;
        _villageRenderer.OnAnchorClicked -= OnAnchorClicked;

        // Village manager events
        _villageManager.OnUpgradePointsChanged -= OnUpgradePointsChanged;

        // Panel events
        _buildingPanel.OnClosed -= OnPanelClosed;
        _emptyPlotPanel.OnClosed -= OnPanelClosed;
        _emptyPlotPanel.OnBuildingSelected -= OnBuildingPlaced;
        _gatePanel.OnClosed -= OnPanelClosed;
        _anchorPanel.OnClosed -= OnPanelClosed;
    }

    #endregion

    #region Panel Management

    private void ShowPanel(VisualElement panel) {
        CloseAllPanels();

        if (panel is BuildingPanel bp) bp.Show(null!);
        else if (panel is EmptyPlotPanel ep) ep.Show(Vector2Int.zero);
        else if (panel is GatePanel gp) gp.Show();
        else if (panel is AnchorPanel ap) ap.Show();

        // Register as modal - blocks world input
        GameInputManager.Instance?.RegisterModal(panel);
    }

    private void CloseAllPanels() {
        ClosePanelIfOpen(_buildingPanel);
        ClosePanelIfOpen(_emptyPlotPanel);
        ClosePanelIfOpen(_gatePanel);
        ClosePanelIfOpen(_anchorPanel);
    }

    private void ClosePanelIfOpen(VisualElement panel) {
        if (panel.style.display == DisplayStyle.Flex) {
            if (panel is BuildingPanel bp) bp.Close();
            else if (panel is EmptyPlotPanel ep) ep.Close();
            else if (panel is GatePanel gp) gp.Close();
            else if (panel is AnchorPanel ap) ap.Close();

            GameInputManager.Instance?.UnregisterModal(panel);
        }
    }

    #endregion

    #region Event Handlers

    private void OnPhaseChanged(GamePhase oldPhase, GamePhase newPhase) {
        bool shouldShow = newPhase == GamePhase.Village;
        style.display = shouldShow ? DisplayStyle.Flex : DisplayStyle.None;

        if (shouldShow) {
            RefreshUI();
        } else {
            CloseAllPanels();
            // Clear hover state when leaving village
            _villageRenderer.ClearHover();
        }
    }

    private void OnBuildingClicked(Building building) {
        CloseAllPanels();
        _buildingPanel.Show(building);
        GameInputManager.Instance?.RegisterModal(_buildingPanel);
    }

    private void OnEmptyPlotClicked(Vector2Int position) {
        CloseAllPanels();
        _emptyPlotPanel.Show(position);
        GameInputManager.Instance?.RegisterModal(_emptyPlotPanel);
    }

    private void OnGateClicked() {
        CloseAllPanels();
        _gatePanel.Show();
        GameInputManager.Instance?.RegisterModal(_gatePanel);
    }

    private void OnAnchorClicked() {
        CloseAllPanels();
        _anchorPanel.Show();
        GameInputManager.Instance?.RegisterModal(_anchorPanel);
    }

    private void OnUpgradePointsChanged(int newPoints) {
        _upgradePointsLabel.SetText(newPoints.ToString());
    }

    private void OnPanelClosed() {
        // Unregister all as modals just in case
        GameInputManager.Instance?.UnregisterModal(_buildingPanel);
        GameInputManager.Instance?.UnregisterModal(_emptyPlotPanel);
        GameInputManager.Instance?.UnregisterModal(_gatePanel);
        GameInputManager.Instance?.UnregisterModal(_anchorPanel);
    }

    private void OnBuildingPlaced(BuildingProto.ID buildingId, Vector2Int position) {
        // Renderer handles building placement via VillageLayout events
        // Just refresh the HUD
        RefreshUI();
    }

    private void RefreshUI() {
        _upgradePointsLabel.SetText(_villageManager.UpgradePoints.ToString());
        _dayLabel.SetText($"Day {GameTime.Instance.Day}");
    }

    #endregion

    #region Input Forwarding

    /// <summary>
    /// Forwards a world click to the village renderer.
    /// Called by VillageInputContext or GameInputManager.
    /// </summary>
    /// <param name="worldPosition">World position of the click.</param>
    public void HandleWorldClick(Vector3 worldPosition) {
        _villageRenderer.HandleClick(worldPosition);
    }

    /// <summary>
    /// Forwards world hover to the village renderer.
    /// Called by VillageInputContext or GameInputManager.
    /// </summary>
    /// <param name="worldPosition">World position of the hover.</param>
    public void HandleWorldHover(Vector3 worldPosition) {
        _villageRenderer.HandleHover(worldPosition);
    }

    /// <summary>
    /// Clears the hover state.
    /// Called when mouse leaves the game area.
    /// </summary>
    public void ClearWorldHover() {
        _villageRenderer.ClearHover();
    }

    #endregion

    #region Cleanup

    /// <summary>
    /// Removes the screen from the hierarchy and cleans up resources.
    /// </summary>
    public new void RemoveFromHierarchy() {
        UnsubscribeFromEvents();
        _villageRenderer.Cleanup();
        base.RemoveFromHierarchy();
    }

    #endregion
}
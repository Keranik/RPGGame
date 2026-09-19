using RPGGame.Core;
using RPGGame.Core.Characters;
using RPGGame.Core.Combat;
using RPGGame.Core.Items;
using RPGGame.Core.Prototypes.Skills;
using RPGGame.Core.Prototypes.Spells;
using RPGGame.Core.Simulation;
using RPGGame.Core.Spells;
using RPGGame.UI.Components;
using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Screens;

/// <summary>
/// The supreme turn-based combat screen - a large modal overlay with the expedition map visible behind.
/// 
/// <para>Layout:</para>
/// <list type="bullet">
/// <item>Top: Round counter, turn indicator, turn order bar</item>
/// <item>Center-Left: Battlefield with enemies (top) and player (bottom)</item>
/// <item>Center-Right: Skills/Spells panel with "Use Weapon" at top</item>
/// <item>Bottom-Left: Compact combat log</item>
/// <item>Bottom-Right: Flee button and auto-combat toggle</item>
/// </list>
/// 
/// <para>Features:</para>
/// <list type="bullet">
/// <item>Uses CombatCharacterDisplay for all combatants with floating damage numbers</item>
/// <item>Integrated skills/spells panel - no separate modal needed</item>
/// <item>"Use Weapon" pseudo-skill shows equipped weapon or unarmed damage</item>
/// <item>Color-coded combat log with ColorRPG</item>
/// <item>Click-to-select targeting</item>
/// </list>
/// </summary>
public class CombatScreen : VisualElement {
    #region Constants - Combat Log Colors

    private static readonly ColorRPG LOG_COLOR_DAMAGE = ColorRPG.RageRed;
    private static readonly ColorRPG LOG_COLOR_CRIT = ColorRPG.BuffGold;
    private static readonly ColorRPG LOG_COLOR_HEAL = ColorRPG.HealGreen;
    private static readonly ColorRPG LOG_COLOR_MISS = ColorRPG.MissGray;
    private static readonly ColorRPG LOG_COLOR_BUFF = ColorRPG.BuffGold;
    private static readonly ColorRPG LOG_COLOR_DEBUFF = ColorRPG.DebuffPurple;
    private static readonly ColorRPG LOG_COLOR_SYSTEM = ColorRPG.ShieldBlue;
    private static readonly ColorRPG LOG_COLOR_TURN = ColorRPG.Silver;

    private const int MAX_LOG_ENTRIES = 30;
    private const int LOG_FADE_START = 20;

    private const float ABILITY_SLOT_SIZE = 56f;
    private const float ABILITY_PANEL_WIDTH = 320f;
    private const float LOG_PANEL_HEIGHT = 200f;

    #endregion

    #region Fields - Dependencies

    private readonly GameStateManager k_stateManager;
    private readonly CombatManager k_combatManager;
    private readonly GameSession k_session;
    private readonly UiManager k_uiManager;
    private GameDb k_gameDb => GameServices.Db;
	private InventoryManager k_inventoryManager => GameServices.Inventory;

    #endregion

    #region Fields - UI Structure

    private VisualElement k_backdrop = null!;
    private GamePanel k_modalContainer = null!;
    private GamePanel k_topBar = null!;
    private GameContainer k_mainArea = null!;
    private GameContainer k_bottomArea = null!;

    // Top bar elements
    private GameLabel k_roundLabel = null!;
    private GameLabel k_turnLabel = null!;
    private GameContainer k_turnOrderBar = null!;

    // Battlefield
    private GameContainer k_battlefieldArea = null!;
    private GameContainer k_enemyArea = null!;
    private GameContainer k_playerArea = null!;
    private readonly List<CombatCharacterDisplay> k_enemyDisplays = [];
    private CombatCharacterDisplay? k_playerDisplay;

    // Right panel - Skills/Spells
    private GamePanel k_abilitiesPanel = null!;
    private GameScrollView k_abilitiesScroll = null!;
    private GameContainer k_weaponSlotContainer = null!;
    private GameLabel k_manaLabel = null!;
    private GameStatBar k_manaBar = null!;
    private readonly List<GameSkillSlotDisplay> k_skillSlots = [];
    private readonly List<GameSpellSlotDisplay> k_spellSlots = [];

    // Bottom-left - Combat log
    private GamePanel k_combatLogPanel = null!;
    private GameScrollView k_combatLogScroll = null!;
    private int k_logEntryCount;

    // Bottom-right - Actions
    private GameContainer k_actionsContainer = null!;
    private GameButton k_fleeButton = null!;
    private GameButton k_itemsButton = null!;
    private GameCheckbox k_autoToggle = null!;

    // Rewards panel
    private CombatRewardsPanel k_rewardsPanel = null!;

    // State
    private LiveCharacter? k_selectedTarget;
    private bool k_autoCombatEnabled;
    private IVisualElementScheduledItem? k_updateSchedule;

    #endregion

    #region Constructor

    /// <summary>
    /// Creates a new CombatScreen with all required dependencies.
    /// </summary>
    public CombatScreen(
        GameStateManager stateManager,
        CombatManager combatManager,
        GameSession session,
        UiManager uiManager,
        GameDb gameDb
    ) {
        k_stateManager = stateManager;
        k_combatManager = combatManager;
        k_session = session;
        k_uiManager = uiManager;

        BuildUI();
        SubscribeToEvents();

        style.display = DisplayStyle.None;
        Debug.Log("CombatScreen: Created with integrated abilities panel");
    }

    #endregion

    #region UI Building

    private void BuildUI() {
        var theme = GameTheme.Current;
        var colors = theme.Colors;
        var borders = theme.Borders;

        // Full-screen container
        style.position = Position.Absolute;
        style.left = 0;
        style.top = 0;
        style.right = 0;
        style.bottom = 0;
        style.flexDirection = FlexDirection.Column;
        style.justifyContent = Justify.Center;
        style.alignItems = Align.Center;
        pickingMode = PickingMode.Position;

        // Semi-transparent backdrop
        k_backdrop = new VisualElement();
        k_backdrop.style.position = Position.Absolute;
        k_backdrop.style.left = 0;
        k_backdrop.style.top = 0;
        k_backdrop.style.right = 0;
        k_backdrop.style.bottom = 0;
        k_backdrop.style.backgroundColor = colors.BackgroundOverlay;
        k_backdrop.pickingMode = PickingMode.Position;
        Add(k_backdrop);

        // Main modal container
        k_modalContainer = new GamePanel()
            .SetVariant(PanelVariant.Card)
            .SetLayout(LayoutDirection.Vertical)
            .SetPadding(0)
            .SetSize(Length.Percent(94), Length.Percent(90))
            .SetMaxSize(2800, 1800)
            .SetBorderRadius(borders.RadiusLG)
            .SetOverflow(Overflow.Hidden)
            .SetPickingMode(PickingMode.Position)
            .Build();

        BuildTopBar();
        BuildMainArea();
        BuildBottomArea();

        Add(k_modalContainer);

        // Rewards panel (rendered on top)
        k_rewardsPanel = new CombatRewardsPanel(k_session, k_gameDb);
        k_rewardsPanel.OnContinueClicked += OnRewardsContinueClicked;
        Add(k_rewardsPanel);
    }

    private void BuildTopBar() {
        var theme = GameTheme.Current;
        var spacing = theme.Spacing;
        var colors = theme.Colors;
        var borders = theme.Borders;

        k_topBar = new GamePanel()
            .SetVariant(PanelVariant.Filled)
            .SetLayout(LayoutDirection.Vertical)
            .SetPadding(spacing.SM, spacing.MD)
            .SetPickingMode(PickingMode.Position)
            .Build();

        // Row 1: Combat title, round counter, turn indicator
        var headerRow = new GameContainer("header-row")
            .SetRow()
            .SetSpaceBetween()
            .SetAlignItems(Align.Center)
            .SetFullWidth();

        // Left section
        var leftSection = new GameContainer("left")
            .SetRow()
            .SetAlignItems(Align.Center);

        leftSection.Add(new GameLabel("⚔")
            .SetStyle(LabelStyle.TitleLarge)
            .SetColor(LabelColor.Primary)
            .Build());

        leftSection.Add(new GameLabel("COMBAT")
            .SetStyle(LabelStyle.TitleMedium)
            .SetColor(LabelColor.Primary)
            .SetMarginLeft(spacing.XS)
            .Build());

        k_roundLabel = new GameLabel("Round 1")
            .SetStyle(LabelStyle.TitleSmall)
            .SetColor(LabelColor.Accent)
            .SetMarginLeft(spacing.MD)
            .SetPadding(spacing.XXS, spacing.SM)
            .Build();
        k_roundLabel.style.backgroundColor = colors.BackgroundElevated;
        borders.ApplyRadius(k_roundLabel.style, borders.RadiusSM);
        leftSection.Add(k_roundLabel);

        // Right section: Turn indicator
        k_turnLabel = new GameLabel("Your Turn")
            .SetStyle(LabelStyle.TitleSmall)
            .SetColor(LabelColor.Success)
            .SetPadding(spacing.XXS, spacing.SM)
            .Build();
        k_turnLabel.style.backgroundColor = colors.SuccessBackground;
        borders.ApplyRadius(k_turnLabel.style, borders.RadiusSM);

        headerRow.Add(leftSection);
        headerRow.Add(k_turnLabel);

        // Row 2: Turn order bar
        k_turnOrderBar = new GameContainer("turn-order")
            .SetRow()
            .SetJustifyContent(Justify.Center)
            .SetAlignItems(Align.Center)
            .SetFullWidth()
            .SetMarginTop(spacing.SM);

        k_turnOrderBar.Add(new GameLabel("Turn Order:")
            .SetStyle(LabelStyle.BodySmall)
            .SetColor(LabelColor.Secondary)
            .SetMarginRight(spacing.SM)
            .Build());

        k_topBar.Content.Add(headerRow);
        k_topBar.Content.Add(k_turnOrderBar);
        k_modalContainer.Content.Add(k_topBar);
    }

    private void BuildMainArea() {
        var theme = GameTheme.Current;
        var spacing = theme.Spacing;

        k_mainArea = new GameContainer("main-area")
            .SetRow()
            .SetGrow()
            .SetPadding(spacing.MD);

        // Left side: Battlefield
        BuildBattlefield();

        // Right side: Abilities panel
        BuildAbilitiesPanel();

        k_mainArea.Add(k_battlefieldArea);
        k_mainArea.Add(k_abilitiesPanel);

        k_modalContainer.Content.Add(k_mainArea);
    }

    private void BuildBattlefield() {
        var theme = GameTheme.Current;
        var spacing = theme.Spacing;

        k_battlefieldArea = new GameContainer("battlefield")
            .SetColumn()
            .SetJustifyContent(Justify.SpaceBetween)
            .SetAlignItems(Align.Center)
            .SetGrow();

        // Enemy area (top)
        k_enemyArea = new GameContainer("enemies")
            .SetRow()
            .SetJustifyContent(Justify.Center)
            .SetAlignItems(Align.FlexStart)
            .SetFlexWrap(Wrap.Wrap);

        // Battle separator
        var separator = new GameContainer("separator")
            .SetRow()
            .SetJustifyContent(Justify.Center)
            .SetAlignItems(Align.Center)
            .SetFullWidth()
            .SetMargin(spacing.LG, 0);

        var line1 = new GameContainer()
            .SetWidth(Length.Percent(30))
            .SetHeight(2)
            .SetBackgroundColor(theme.Colors.SurfaceBorder);

        var vsLabel = new GameLabel("⚔ VS ⚔")
            .SetStyle(LabelStyle.TitleSmall)
            .SetColor(LabelColor.Tertiary)
            .SetMargin(0, spacing.MD)
            .Build();

        var line2 = new GameContainer()
            .SetWidth(Length.Percent(30))
            .SetHeight(2)
            .SetBackgroundColor(theme.Colors.SurfaceBorder);

        separator.Add(line1);
        separator.Add(vsLabel);
        separator.Add(line2);

        // Player area (bottom)
        k_playerArea = new GameContainer("player")
            .SetRow()
            .SetJustifyContent(Justify.Center)
            .SetAlignItems(Align.FlexEnd);

        k_battlefieldArea.Add(k_enemyArea);
        k_battlefieldArea.Add(separator);
        k_battlefieldArea.Add(k_playerArea);
    }

    private void BuildAbilitiesPanel() {
        var theme = GameTheme.Current;
        var spacing = theme.Spacing;
        var colors = theme.Colors;
        var borders = theme.Borders;

        k_abilitiesPanel = new GamePanel()
            .SetVariant(PanelVariant.Outlined)
            .SetLayout(LayoutDirection.Vertical)
            .SetPadding(spacing.SM)
            .SetWidth(ABILITY_PANEL_WIDTH)
            .SetPickingMode(PickingMode.Position)
            .Build();

        // Header with mana display
        var header = new GameContainer("abilities-header")
            .SetRow()
            .SetSpaceBetween()
            .SetAlignItems(Align.Center)
            .SetMarginBottom(spacing.SM);

        header.Add(new GameLabel("⚔ Actions")
            .SetStyle(LabelStyle.TitleSmall)
            .SetColor(LabelColor.Primary)
            .Build());

        var manaContainer = new GameContainer("mana")
            .SetColumn()
            .SetAlignItems(Align.FlexEnd);

        k_manaLabel = new GameLabel("MP: 0/0")
            .SetStyle(LabelStyle.Caption)
            .SetColor(LabelColor.Info)
            .Build();

        k_manaBar = new GameStatBar()
            .SetVariant(StatBarVariant.Mana)
            .SetHeight(6)
            .SetWidth(80)
            .SetShowValue(false)
            .SetAnimateChanges(true)
            .Build();

        manaContainer.Add(k_manaLabel);
        manaContainer.Add(k_manaBar);
        header.Add(manaContainer);

        k_abilitiesPanel.Content.Add(header);

        // Weapon slot container (always at top)
        k_weaponSlotContainer = new GameContainer("weapon-slot")
            .SetRow()
            .SetAlignItems(Align.Center)
            .SetPadding(spacing.XS)
            .SetMarginBottom(spacing.SM)
            .SetBackgroundColor(colors.BackgroundElevated)
            .SetBorderRadius(borders.RadiusSM);

        k_abilitiesPanel.Content.Add(k_weaponSlotContainer);

        // Divider
        k_abilitiesPanel.Content.Add(new GameDivider()
            .SetMargin(spacing.XS)
            .Build());

        // Abilities scroll area
        k_abilitiesScroll = new GameScrollView("abilities-scroll")
            .SetVertical()
            .SetGrow()
            .Build();

        k_abilitiesPanel.Content.Add(k_abilitiesScroll);
    }

    private void BuildBottomArea() {
        var theme = GameTheme.Current;
        var spacing = theme.Spacing;
        var colors = theme.Colors;

        k_bottomArea = new GameContainer("bottom-area")
            .SetRow()
            .SetSpaceBetween()
            .SetAlignItems(Align.FlexEnd)
            .SetPadding(spacing.MD)
            .SetBackgroundColor(colors.BackgroundSecondary);

        // Left: Combat log (compact)
        BuildCombatLog();

        // Right: Actions (flee, items, auto)
        BuildActionsBar();

        k_bottomArea.Add(k_combatLogPanel);
        k_bottomArea.Add(k_actionsContainer);

        k_modalContainer.Content.Add(k_bottomArea);
    }

    private void BuildCombatLog() {
        var theme = GameTheme.Current;
        var spacing = theme.Spacing;

        k_combatLogPanel = new GamePanel()
            .SetVariant(PanelVariant.Outlined)
            .SetHeader("📜 Combat Log")
            .SetPadding(spacing.XS)
            .SetWidth(400)
            .SetHeight(LOG_PANEL_HEIGHT)
            .SetPickingMode(PickingMode.Position)
            .Build();

        k_combatLogScroll = new GameScrollView("combat-log")
            .SetVertical()
            .SetGrow()
            .Build();

        k_combatLogPanel.Content.Add(k_combatLogScroll);
    }

    private void BuildActionsBar() {
        var theme = GameTheme.Current;
        var spacing = theme.Spacing;

        k_actionsContainer = new GameContainer("actions")
            .SetRow()
            .SetAlignItems(Align.Center);

        // Items button
        k_itemsButton = new GameButton("🎒 Items")
            .SetVariant(ButtonVariant.Secondary)
            .SetSize(ButtonSize.Medium)
            .OnClick(OnItemsClicked)
            .Build();
        k_itemsButton.tooltip = "Use consumable items from your inventory.";
        k_itemsButton.style.marginRight = spacing.SM;

        // Flee button
        k_fleeButton = new GameButton("🏃 Flee")
            .SetVariant(ButtonVariant.Ghost)
            .SetSize(ButtonSize.Medium)
            .OnClick(OnFleeClicked)
            .Build();
        k_fleeButton.tooltip = "Attempt to escape from combat.\nMay fail against fast enemies.";
        k_fleeButton.style.marginRight = spacing.MD;

        // Auto-combat toggle
        var autoPanel = new GameContainer("auto")
            .SetRow()
            .SetAlignItems(Align.Center);

        autoPanel.Add(new GameLabel("Auto:")
            .SetStyle(LabelStyle.BodySmall)
            .SetColor(LabelColor.Secondary)
            .Build());

        k_autoToggle = new GameCheckbox()
            .SetLabel("")
            .OnValueChanged(OnAutoCombatToggled)
            .Build();
        k_autoToggle.style.marginLeft = spacing.XS;
        k_autoToggle.tooltip = "Enable automatic combat (AI coming soon!)";

        autoPanel.Add(k_autoToggle);

        k_actionsContainer.Add(k_itemsButton);
        k_actionsContainer.Add(k_fleeButton);
        k_actionsContainer.Add(autoPanel);
    }

    #endregion

    #region Event Subscriptions

    private void SubscribeToEvents() {
        k_stateManager.OnPhaseChanged += OnPhaseChanged;
        k_combatManager.OnCombatStarted += OnCombatStarted;
        k_combatManager.OnCombatEnded += OnCombatEnded;
        k_combatManager.OnTurnStarted += OnTurnStarted;
        k_combatManager.OnTurnEnded += OnTurnEnded;
        k_combatManager.OnActionResolved += OnActionResolved;
        k_combatManager.OnCombatantDefeated += OnCombatantDefeated;
        k_combatManager.OnNewRound += OnNewRound;
    }

    private void UnsubscribeFromEvents() {
        k_stateManager.OnPhaseChanged -= OnPhaseChanged;
        k_combatManager.OnCombatStarted -= OnCombatStarted;
        k_combatManager.OnCombatEnded -= OnCombatEnded;
        k_combatManager.OnTurnStarted -= OnTurnStarted;
        k_combatManager.OnTurnEnded -= OnTurnEnded;
        k_combatManager.OnActionResolved -= OnActionResolved;
        k_combatManager.OnCombatantDefeated -= OnCombatantDefeated;
        k_combatManager.OnNewRound -= OnNewRound;
    }

    #endregion

    #region Phase Management

    private void OnPhaseChanged(GamePhase oldPhase, GamePhase newPhase) {
        bool shouldShow = newPhase == GamePhase.Combat;
        style.display = shouldShow ? DisplayStyle.Flex : DisplayStyle.None;

        if (shouldShow) {
            StartUpdateLoop();
        } else {
            StopUpdateLoop();
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

    #endregion

    #region Combat Event Handlers

    private void OnCombatStarted(CombatEncounter encounter) {
        Debug.Log($"CombatScreen: Combat started - {encounter.Name}");

        ClearCombatants();
        BuildCombatantDisplays();
        ClearCombatLog();

        AddToLog($"⚔ Combat begins! {encounter.Name}", LOG_COLOR_SYSTEM);

        k_selectedTarget = null;
        RefreshUI();
        RefreshAbilitiesPanel();
    }

    private void OnCombatEnded(CombatResult result) {
        ColorRPG color;
        string message;

        if (result.IsVictory) {
            message = "🎉 Victory! You defeated all enemies.";
            color = ColorRPG.HealGreen;
        } else if (result.Reason == CombatEndReason.Fled) {
            message = "🏃 You fled from battle.";
            color = ColorRPG.StaminaYellow;
        } else {
            message = "💀 Defeat...";
            color = LOG_COLOR_DAMAGE;
        }

        AddToLog(message, color);

        if (result.IsVictory) {
            if (result.ExperienceGained > 0) {
                AddToLog($"  ⭐ Gained {result.ExperienceGained} XP", ColorRPG.ExperiencePurple);
            }
            if (result.GoldGained > 0) {
                AddToLog($"  💰 Found {result.GoldGained} gold", ColorRPG.Gold);
            }
        }

        SetActionsEnabled(false);

        if (result.Reason != CombatEndReason.Defeat) {
            k_rewardsPanel.ShowRewards(result);
        }
    }

    private void OnRewardsContinueClicked() {
        Debug.Log("CombatScreen: Rewards dismissed");
    }

    private void OnTurnStarted(LiveCharacter character) {
        bool isPlayer = character.IsPlayer;

        k_turnLabel.SetText(isPlayer ? "⚡ Your Turn" : $"👾 {character.Name}'s Turn");
        k_turnLabel.SetColor(isPlayer ? LabelColor.Success : LabelColor.Warning);

        var theme = GameTheme.Current;
        k_turnLabel.style.backgroundColor = isPlayer
            ? theme.Colors.SuccessBackground
            : theme.Colors.WarningBackground;

        SetActionsEnabled(isPlayer && !k_autoCombatEnabled);
        UpdateTurnIndicators(character);

        AddToLog($"── {character.Name}'s turn ──", LOG_COLOR_TURN);

        // Refresh abilities when player turn starts
        if (isPlayer) {
            RefreshAbilitiesPanel();
        }
    }

    private void OnTurnEnded(LiveCharacter character) {
        foreach (var display in k_enemyDisplays) {
            display.SetCurrentTurn(false);
        }
        k_playerDisplay?.SetCurrentTurn(false);

        RefreshUI();
    }

    private void OnActionResolved(CombatActionResult result) {
        if (!string.IsNullOrEmpty(result.Message)) {
            ColorRPG color = DetermineLogColor(result);
            AddToLog(result.Message, color);
        }

        foreach (var msg in result.AdditionalMessages) {
            AddToLog($"  {msg}", LOG_COLOR_BUFF.WithAlpha(0.8f));
        }

        ShowFloatingCombatText(result);
        RefreshUI();
        RefreshAbilitiesPanel();

        schedule.Execute(() => {
            k_combatManager.ContinueAfterAction();
        }).ExecuteLater(600);
    }

    private void OnCombatantDefeated(LiveCharacter character) {
        AddToLog($"💀 {character.Name} has been defeated!", LOG_COLOR_DAMAGE.Lighten(0.1f));

        if (k_selectedTarget == character) {
            k_selectedTarget = k_combatManager.LivingEnemies.FirstOrDefault();
            UpdateSelectionHighlights();
        }

        RefreshUI();
    }

    private void OnNewRound(int roundNumber) {
        k_roundLabel.SetText($"Round {roundNumber}");
        AddToLog($"═══ Round {roundNumber} ═══", LOG_COLOR_SYSTEM);
    }

    #endregion

    #region UI Updates

    private void UpdateUI() {
        var state = k_combatManager.GetCombatState();
        if (state == null || !state.IsInCombat) return;

        UpdateTurnOrderBar(state);
    }

    private void RefreshUI() {
        UpdateUI();
        RefreshCombatantDisplays();
    }

    private void BuildCombatantDisplays() {
        var theme = GameTheme.Current;
        var spacing = theme.Spacing;

        k_enemyDisplays.Clear();
        k_enemyArea.Clear();

        foreach (var enemy in k_combatManager.Combatants.Where(c => c.IsEnemySide)) {
            var display = new CombatCharacterDisplay()
                .SetCharacter(enemy)
                .SetIsEnemy(true)
                .SetSize(CombatDisplaySize.Large)
                .SetShowManaBar(enemy.MaxMana > 0)
                .SetShowStatusEffects(true)
                .OnClicked(() => OnEnemyClicked(enemy))
                .Build();

            display.style.marginRight = spacing.MD;
            display.style.marginBottom = spacing.SM;

            k_enemyDisplays.Add(display);
            k_enemyArea.Add(display);
        }

        k_playerArea.Clear();
        var player = k_combatManager.Player;
        if (player != null) {
            k_playerDisplay = new CombatCharacterDisplay()
                .SetCharacter(player)
                .SetIsEnemy(false)
                .SetSize(CombatDisplaySize.Large)
                .SetShowManaBar(true)
                .SetShowStatusEffects(true)
                .Build();

            k_playerArea.Add(k_playerDisplay);
        }

        if (k_enemyDisplays.Count > 0 && k_combatManager.LivingEnemies.Any()) {
            k_selectedTarget = k_combatManager.LivingEnemies.First();
            UpdateSelectionHighlights();
        }
    }

    private void ClearCombatants() {
        foreach (var display in k_enemyDisplays) {
            display.RemoveFromHierarchy();
        }
        k_enemyDisplays.Clear();
        k_enemyArea.Clear();
        k_playerArea.Clear();
        k_playerDisplay?.RemoveFromHierarchy();
        k_playerDisplay = null;
    }

    private void RefreshCombatantDisplays() {
        foreach (var display in k_enemyDisplays) {
            display.Refresh();
        }
        k_playerDisplay?.Refresh();
    }

    private void UpdateTurnOrderBar(CombatState state) {
        var theme = GameTheme.Current;
        var spacing = theme.Spacing;
        var colors = theme.Colors;
        var borders = theme.Borders;

        while (k_turnOrderBar.childCount > 1) {
            k_turnOrderBar.RemoveAt(1);
        }

        foreach (var character in state.TurnOrder.Take(10)) {
            var isCurrent = character == state.CurrentCombatant;
            var isPlayer = character.IsPlayerSide;
            var isAlive = character.IsAlive;

            var card = new GameContainer("turn-card")
                .SetRow()
                .SetAlignItems(Align.Center)
                .SetPadding(spacing.XXS, spacing.XS)
                .SetMarginLeft(spacing.XS)
                .SetBorderRadius(borders.RadiusSM);

            if (isCurrent) {
                card.SetBackgroundColor(colors.Primary.WithAlpha(0.25f));
                card.SetBorderWidth(0, 0, 2, 0);
                card.SetBorderColor(colors.Primary);
            }

            if (!isAlive) {
                card.SetOpacity(0.4f);
            }

            string icon = isPlayer ? "👤" : (character.IsBoss ? "👹" : "👾");
            if (!isAlive) icon = "💀";

            card.Add(new GameLabel(icon)
                .SetStyle(LabelStyle.BodySmall)
                .SetColor(isCurrent ? LabelColor.Primary : LabelColor.Tertiary)
                .Build());

            string shortName = character.Name.Length > 8
                ? character.Name[..8] + "."
                : character.Name;

            card.Add(new GameLabel(shortName)
                .SetStyle(LabelStyle.Caption)
                .SetColor(isCurrent ? LabelColor.Primary : LabelColor.Secondary)
                .SetMarginLeft(spacing.XXS)
                .Build());

            var initLabel = new GameLabel($"({character.InitiativeRoll})")
                .SetStyle(LabelStyle.Caption)
                .SetColor(LabelColor.Tertiary)
                .SetMarginLeft(spacing.XXS)
                .Build();
            initLabel.style.fontSize = 9;
            card.Add(initLabel);

            card.tooltip = $"{character.Name}\nInitiative: {character.InitiativeRoll}\nHP: {character.CurrentHealth:F0}/{character.MaxHealth:F0}";

            k_turnOrderBar.Add(card);
        }
    }

    private void UpdateTurnIndicators(LiveCharacter currentCharacter) {
        foreach (var display in k_enemyDisplays) {
            display.SetCurrentTurn(display.Character == currentCharacter);
        }
        k_playerDisplay?.SetCurrentTurn(k_playerDisplay.Character == currentCharacter);
    }

    private void UpdateSelectionHighlights() {
        foreach (var display in k_enemyDisplays) {
            display.SetSelected(display.Character == k_selectedTarget);
        }
    }

    private void SetActionsEnabled(bool enabled) {
        k_itemsButton.SetEnabled(enabled);

        var state = k_combatManager.GetCombatState();
        k_fleeButton.SetEnabled(enabled && (state?.CanFlee ?? false));

        // Enable/disable ability slots
        RefreshAbilitiesPanel();
    }

    #endregion

    #region Abilities Panel

    private void RefreshAbilitiesPanel() {
        var player = k_combatManager.Player;
        if (player == null) return;

        // Update mana display
        float currentMana = player.CurrentMana;
        float maxMana = player.MaxMana;
        k_manaLabel.SetText($"MP: {currentMana:F0}/{maxMana:F0}");
        k_manaBar.SetRange(0, maxMana);
        k_manaBar.SetValue(currentMana);

        // Refresh weapon slot
        RefreshWeaponSlot(player);

        // Refresh abilities list
        RefreshAbilitiesList(player);
    }

    private void RefreshWeaponSlot(LiveCharacter player) {
        k_weaponSlotContainer.Clear();

        var theme = GameTheme.Current;
        var spacing = theme.Spacing;
        var colors = theme.Colors;

        var state = k_combatManager.GetCombatState();
        bool isPlayerTurn = state?.CurrentCombatant?.IsPlayer ?? false;
        bool canAct = isPlayerTurn && !k_autoCombatEnabled;

		string weaponName;
        string weaponDamage;
        string weaponIcon;
        ColorRPG weaponColor;
        var weapon = k_inventoryManager.GetEquippedItem(SlotType.MainHand);
		if (weapon == null) {
			weapon = k_inventoryManager.GetEquippedItem(SlotType.BothHands);
		}

		if (weapon != null) {
            weaponName = weapon.DisplayName;
            weaponDamage = $"{k_inventoryManager.GetWeaponDamage()} {k_inventoryManager.GetWeaponDamageType()}";
			weaponIcon = GetWeaponIcon(WeaponType.Sword);
            weaponColor = colors.GetRarityColor(weapon.Prototype.Rarity);
        } else {
            // Unarmed
            weaponName = "Unarmed Strike";
            int strMod = player.StrengthMod;
            weaponDamage = $"1d4{(strMod >= 0 ? "+" : "")}{strMod} Bludgeoning";
            weaponIcon = "👊";
            weaponColor = ColorRPG.MissGray;
        }

        // Build weapon action row
        var weaponRow = new GameContainer("weapon-action")
            .SetRow()
            .SetAlignItems(Align.Center)
            .SetGrow();

        // Icon
        var iconLabel = new GameLabel(weaponIcon)
            .SetStyle(LabelStyle.TitleMedium)
            .SetMarginRight(spacing.SM)
            .Build();
        iconLabel.style.color = weaponColor;

        // Info column
        var infoColumn = new GameContainer("weapon-info")
            .SetColumn()
            .SetGrow();

        infoColumn.Add(new GameLabel(weaponName)
            .SetStyle(LabelStyle.BodyMedium)
            .SetColor(canAct ? LabelColor.Primary : LabelColor.Disabled)
            .Build());

        var damageLabel = new GameLabel(weaponDamage)
            .SetStyle(LabelStyle.Caption)
            .SetColor(LabelColor.Secondary)
            .Build();
        infoColumn.Add(damageLabel);

        weaponRow.Add(iconLabel);
        weaponRow.Add(infoColumn);

        // Attack button
        var attackButton = new GameButton("Attack")
            .SetVariant(ButtonVariant.Primary)
            .SetSize(ButtonSize.Small)
            .OnClick(() => OnWeaponAttackClicked())
            .Build();
        attackButton.SetEnabled(canAct);
        attackButton.tooltip = weapon != null
            ? $"Attack with {weapon.DisplayName}\n{weaponDamage}"
            : $"Unarmed attack\n{weaponDamage}";

        weaponRow.Add(attackButton);

        k_weaponSlotContainer.Add(weaponRow);

        // Make whole container clickable for attack
        if (canAct) {
            k_weaponSlotContainer.RegisterCallback<ClickEvent>(evt => {
                if (evt.target != attackButton) {
                    OnWeaponAttackClicked();
                }
            });
			k_weaponSlotContainer.AddToClassList("clickable");
        }
    }

    private void RefreshAbilitiesList(LiveCharacter player) {
        k_abilitiesScroll.ClearContent();
        ClearAbilitySlots();

        var theme = GameTheme.Current;
        var spacing = theme.Spacing;
        var colors = theme.Colors;

        var state = k_combatManager.GetCombatState();
        bool isPlayerTurn = state?.CurrentCombatant?.IsPlayer ?? false;
        bool canAct = isPlayerTurn && !k_autoCombatEnabled;
        float currentMana = player.CurrentMana;

        // Collect abilities (non-spell)
        var abilities = player.Abilities
            .Where(a => a.Type != CombatAbilityType.Spell)
            .ToList();

        // Collect spells
        var spells = player.KnownSpells
            .Select(id => k_gameDb.GetOrNull<SpellProto>(id))
            .Where(s => s != null)
            .Cast<SpellProto>()
            .OrderBy(s => s.Level)
            .ThenBy(s => s.School)
            .ToList();

        // Skills section
        if (abilities.Count > 0) {
            k_abilitiesScroll.AddChild(new GameLabel("⚔ Skills")
                .SetStyle(LabelStyle.LabelSmall)
                .SetColor(LabelColor.Secondary)
                .SetMarginBottom(spacing.XS)
                .Build());

            var skillsGrid = new GameContainer("skills-grid")
                .SetRow()
                .SetFlexWrap(Wrap.Wrap)
                .SetMarginBottom(spacing.SM);

            foreach (var ability in abilities) {
                var slot = CreateAbilitySlot(ability, canAct, currentMana);
                skillsGrid.Add(slot);
            }

            k_abilitiesScroll.AddChild(skillsGrid);
        }

        // Spells section
        if (spells.Count > 0) {
            k_abilitiesScroll.AddChild(new GameLabel("✨ Spells")
                .SetStyle(LabelStyle.LabelSmall)
                .SetColor(LabelColor.Secondary)
                .SetMarginTop(spacing.XS)
                .SetMarginBottom(spacing.XS)
                .Build());

            // Group by school
            var spellsBySchool = spells.GroupBy(s => s.School);

            foreach (var group in spellsBySchool) {
                var schoolColor = group.Key.GetColor();

                var schoolHeader = new GameContainer("school-header")
                    .SetRow()
                    .SetAlignItems(Align.Center)
                    .SetMarginBottom(spacing.XXS);

                schoolHeader.Add(new GameLabel(group.Key.GetIconName())
                    .SetStyle(LabelStyle.BodySmall)
                    .Build());

                var schoolName = new GameLabel(group.Key.GetDisplayName())
                    .SetStyle(LabelStyle.Caption)
                    .SetMarginLeft(spacing.XXS)
                    .Build();
                schoolName.style.color = schoolColor;
                schoolHeader.Add(schoolName);

                k_abilitiesScroll.AddChild(schoolHeader);

                var spellsGrid = new GameContainer($"spells-grid-{group.Key}")
                    .SetRow()
                    .SetFlexWrap(Wrap.Wrap)
                    .SetMarginBottom(spacing.SM);

                foreach (var spell in group) {
                    var slot = CreateSpellSlot(spell, canAct, currentMana);
                    spellsGrid.Add(slot);
                }

                k_abilitiesScroll.AddChild(spellsGrid);
            }
        }

        // Empty state
        if (abilities.Count == 0 && spells.Count == 0) {
            k_abilitiesScroll.AddChild(new GameLabel("No abilities available")
                .SetStyle(LabelStyle.BodyMedium)
                .SetColor(LabelColor.Tertiary)
                .SetMargin(spacing.MD)
                .Build());
        }
    }

    private GameContainer CreateAbilitySlot(CombatAbility ability, bool canAct, float currentMana) {
        var theme = GameTheme.Current;
        var spacing = theme.Spacing;
        var colors = theme.Colors;
        var borders = theme.Borders;

        bool canUse = canAct && ability.CurrentCooldown == 0 && ability.ManaCost <= currentMana;

        var container = new GameContainer($"ability-{ability.Id}")
            .SetColumn()
            .SetAlignItems(Align.Center)
            .SetWidth(ABILITY_SLOT_SIZE + spacing.SM)
            .SetPadding(spacing.XXS)
            .SetMargin(spacing.XXS)
            .SetBorderRadius(borders.RadiusSM);

		if (canUse) {
			container.RegisterCallback<MouseEnterEvent>(_ => {
				container.style.backgroundColor = colors.SurfaceHover;
			});
			container.RegisterCallback<MouseLeaveEvent>(_ => {
				container.style.backgroundColor = StyleKeyword.Null;
			});
		}

        // Try to get SkillProto for display
        var skillProto = TryGetSkillProtoForAbility(ability);

        var slot = new GameSkillSlotDisplay()
            .SetSkill(skillProto, 1)
            .SetSize(ABILITY_SLOT_SIZE)
            .SetCooldownTurns(ability.CurrentCooldown)
            .SetInteractive(canUse)
            .SetEnabled(canUse)
            .SetAllowTooltip(true)
            .SetAllowContextMenu(false)
            .OnClick(() => {
                if (canUse) UseAbility(ability);
            })
            .Build();

        k_skillSlots.Add(slot);

        // Name
        var nameLabel = new GameLabel(ability.Name.Truncate(8))
            .SetStyle(LabelStyle.Caption)
            .SetColor(canUse ? LabelColor.Primary : LabelColor.Disabled)
            .SetTextAlign(TextAnchor.MiddleCenter)
            .Build();

        // Cost/cooldown badge
        VisualElement? badge = null;
        if (ability.CurrentCooldown > 0) {
            badge = new GameBadge()
                .SetText($"CD:{ability.CurrentCooldown}")
                .SetVariant(BadgeVariant.Warning)
                .SetSize(BadgeSize.Small)
                .Build();
        } else if (ability.ManaCost > 0) {
            badge = new GameBadge()
                .SetText($"{ability.ManaCost}")
                .SetVariant(canUse ? BadgeVariant.Info : BadgeVariant.Error)
                .SetSize(BadgeSize.Small)
                .Build();
        }

        container.Add(slot);
        container.Add(nameLabel);
        if (badge != null) container.Add(badge);

        // Click handler on container
        if (canUse) {
            container.RegisterCallback<ClickEvent>(evt => {
                if (evt.target == container) UseAbility(ability);
            });
        }

        return container;
    }

    private GameContainer CreateSpellSlot(SpellProto spell, bool canAct, float currentMana) {
        var theme = GameTheme.Current;
        var spacing = theme.Spacing;
        var colors = theme.Colors;
        var borders = theme.Borders;

        int manaCost = spell.GetEffectiveManaCost();
        bool canCast = canAct && manaCost <= currentMana;

        var container = new GameContainer($"spell-{spell.Id.Value}")
            .SetColumn()
            .SetAlignItems(Align.Center)
            .SetWidth(ABILITY_SLOT_SIZE + spacing.SM)
            .SetPadding(spacing.XXS)
            .SetMargin(spacing.XXS)
            .SetBorderRadius(borders.RadiusSM);

		if (canCast) {
			container.RegisterCallback<MouseEnterEvent>(_ => {
				container.style.backgroundColor = colors.SurfaceHover;
			});
			container.RegisterCallback<MouseLeaveEvent>(_ => {
				container.style.backgroundColor = StyleKeyword.Null;
			});
		}

        var slot = new GameSpellSlotDisplay()
            .SetSpell(spell)
            .SetSize(ABILITY_SLOT_SIZE)
            .SetPrepared(canCast)
            .SetInteractive(canCast)
            .SetEnabled(canCast)
            .SetAllowTooltip(true)
            .SetAllowContextMenu(false)
            .OnClick(() => {
                if (canCast) CastSpell(spell);
            })
            .Build();

        k_spellSlots.Add(slot);

        // Name with school color
        var nameLabel = new GameLabel(spell.DisplayText.Name.Truncate(8))
            .SetStyle(LabelStyle.Caption)
            .SetTextAlign(TextAnchor.MiddleCenter)
            .Build();
        nameLabel.style.color = canCast ? spell.School.GetColor() : colors.TextDisabled;

        // Mana cost badge
        VisualElement badge;
        if (manaCost > 0) {
            badge = new GameBadge()
                .SetText($"{manaCost}")
                .SetVariant(canCast ? BadgeVariant.Info : BadgeVariant.Error)
                .SetSize(BadgeSize.Small)
                .Build();
        } else {
            badge = new GameLabel("Free")
                .SetStyle(LabelStyle.Caption)
                .SetColor(LabelColor.Success)
                .Build();
        }

        container.Add(slot);
        container.Add(nameLabel);
        container.Add(badge);

        // Click handler
        if (canCast) {
            container.RegisterCallback<ClickEvent>(evt => {
                if (evt.target == container) CastSpell(spell);
            });
        }

        return container;
    }

    private void ClearAbilitySlots() {
        foreach (var slot in k_skillSlots) {
            slot.RemoveFromHierarchy();
        }
        k_skillSlots.Clear();

        foreach (var slot in k_spellSlots) {
            slot.RemoveFromHierarchy();
        }
        k_spellSlots.Clear();
    }

    private SkillProto? TryGetSkillProtoForAbility(CombatAbility ability) {
        var protoId = new SkillProto.ID($"Skill_{ability.Id}");
        return k_gameDb.GetOrNull<SkillProto>(protoId);
    }

    private static string GetWeaponIcon(WeaponType type) {
        return type switch {
            WeaponType.Sword => "🗡️",
            WeaponType.Axe => "🪓",
            WeaponType.Mace => "🔨",
            WeaponType.Dagger => "🗡️",
            WeaponType.Spear => "🔱",
            WeaponType.Staff => "🪄",
            WeaponType.Bow => "🏹",
            WeaponType.Crossbow => "🏹",
            _ => "⚔️"
        };
    }

    #endregion

    #region Combat Actions

    private void OnWeaponAttackClicked() {
        var target = k_selectedTarget ?? k_combatManager.LivingEnemies.FirstOrDefault();
        var player = k_combatManager.Player;

        if (target != null && player != null) {
            var action = CombatAction.Attack(player, target);
            k_combatManager.ExecuteAction(action);
        }
    }

    private void UseAbility(CombatAbility ability) {
        var player = k_combatManager.Player;
        if (player == null) return;

        var target = GetTargetForAbility(ability.TargetType);
        var action = CombatAction.UseAbility(player, ability, target);
        k_combatManager.ExecuteAction(action);
    }

    private void CastSpell(SpellProto spell) {
        var player = k_combatManager.Player;
        if (player == null) return;

        var combatSpell = ConvertSpellToAbility(spell);
        var target = GetTargetForAbility(spell.TargetType);
        var action = CombatAction.CastSpell(player, combatSpell, target);
        k_combatManager.ExecuteAction(action);
    }

    private LiveCharacter? GetTargetForAbility(TargetType targetType) {
        return targetType switch {
            TargetType.Self => k_combatManager.Player,
            TargetType.SingleEnemy => k_selectedTarget ?? k_combatManager.LivingEnemies.FirstOrDefault(),
            TargetType.SingleAlly => k_combatManager.Player,
            _ => null
        };
    }

    private CombatAbility ConvertSpellToAbility(SpellProto spell) {
        var ability = new CombatAbility {
            Id = spell.Id.Value,
            Name = spell.DisplayText.Name,
            Description = spell.DisplayText.Description,
            Type = CombatAbilityType.Spell,
            TargetType = spell.TargetType,
            ManaCost = spell.GetEffectiveManaCost(),
            Cooldown = spell.Cooldown,
            CurrentCooldown = 0,
            DamageDice = spell.DamageDice,
            DamageType = spell.DamageType
        };

        if (spell.HealingDice.IsValid) {
            ability.Effects.Add(new Core.Prototypes.Combat.AbilityEffect {
                HealAmount = DiceRoller.Roll(spell.HealingDice)
            });
        }

        if (spell.AppliesCondition.HasValue) {
            ability.Effects.Add(new Core.Prototypes.Combat.AbilityEffect {
                AppliesCondition = spell.AppliesCondition,
                ConditionDuration = spell.ConditionDuration
            });
        }

        return ability;
    }

    private void OnItemsClicked() {
        GameToast.Show("Items panel coming soon!", ToastType.Info);
    }

    private void OnFleeClicked() {
        var player = k_combatManager.Player;
        if (player != null) {
            var action = CombatAction.Flee(player);
            k_combatManager.ExecuteAction(action);
        }
    }

    private void OnEnemyClicked(LiveCharacter enemy) {
        if (!enemy.IsAlive) return;

        k_selectedTarget = enemy;
        UpdateSelectionHighlights();
    }

    private void OnAutoCombatToggled(bool enabled) {
        k_autoCombatEnabled = enabled;

        if (enabled) {
            SetActionsEnabled(false);
            GameToast.Show("🤖 Auto-combat enabled (AI coming soon!)", ToastType.Info);
        } else {
            var state = k_combatManager.GetCombatState();
            bool isPlayerTurn = state?.CurrentCombatant?.IsPlayer ?? false;
            SetActionsEnabled(isPlayerTurn);
            GameToast.Show("Auto-combat disabled", ToastType.Info);
        }
    }

    #endregion

    #region Combat Log

    private void AddToLog(string message, ColorRPG color) {
        var theme = GameTheme.Current;
        var spacing = theme.Spacing;

        var logEntry = new GameLabel(message)
            .SetStyle(LabelStyle.BodySmall)
            .SetWhiteSpace(WhiteSpace.Normal)
            .SetMarginBottom(spacing.XXS)
            .Build();

        logEntry.style.color = color;

        k_combatLogScroll.AddChild(logEntry);
        k_logEntryCount++;

        ApplyLogFading();

        while (k_logEntryCount > MAX_LOG_ENTRIES && k_combatLogScroll.childCount > 0) {
            k_combatLogScroll.RemoveAt(0);
            k_logEntryCount--;
        }

        schedule.Execute(() => {
            k_combatLogScroll.ScrollToBottom();
        }).ExecuteLater(16);
    }

    private void ApplyLogFading() {
        int count = k_combatLogScroll.childCount;
        for (int i = 0; i < count; i++) {
            var child = k_combatLogScroll.ElementAt(i);
            if (i < count - LOG_FADE_START) {
                float fadeProgress = 1f - ((float)i / (count - LOG_FADE_START));
                child.style.opacity = Mathf.Lerp(0.3f, 0.7f, fadeProgress);
            } else {
                child.style.opacity = 1f;
            }
        }
    }

    private void ClearCombatLog() {
        k_combatLogScroll.ClearContent();
        k_logEntryCount = 0;
    }

    private ColorRPG DetermineLogColor(CombatActionResult result) {
        if (result.AttackRoll?.IsCritical == true) return LOG_COLOR_CRIT;
        if (!result.Success) return LOG_COLOR_MISS;
        if (result.HealingDone > 0) return LOG_COLOR_HEAL;
        if (result.DamageDealt.Sum(d => d.FinalDamage) > 0) return LOG_COLOR_DAMAGE;
        return LOG_COLOR_SYSTEM;
    }

    #endregion

    #region Floating Combat Text

    private void ShowFloatingCombatText(CombatActionResult result) {
        var target = result.Action?.Target;
        if (target == null) return;

        var targetDisplay = FindDisplayForCharacter(target);
        if (targetDisplay == null) return;

        float totalDamage = result.DamageDealt.Sum(d => d.FinalDamage);

        if (totalDamage > 0) {
            bool isCrit = result.AttackRoll?.IsCritical ?? false;
            var damageType = result.DamageDealt.FirstOrDefault()?.DamageType ?? DamageType.Physical;
            targetDisplay.ShowDamage(totalDamage, damageType, isCrit);
        }

        if (result.HealingDone > 0) {
            targetDisplay.ShowHealing(result.HealingDone);
        }

        if (!result.Success && result.AttackRoll != null) {
            targetDisplay.ShowMiss();
        }

        foreach (var (conditionTarget, condition) in result.EffectsApplied) {
            var display = FindDisplayForCharacter(conditionTarget);
            display?.ShowText($"+{condition}", LOG_COLOR_DEBUFF);
        }
    }

    private CombatCharacterDisplay? FindDisplayForCharacter(LiveCharacter character) {
        if (character == k_playerDisplay?.Character) {
            return k_playerDisplay;
        }
        return k_enemyDisplays.FirstOrDefault(d => d.Character == character);
    }

    #endregion

    #region Cleanup

    public new void RemoveFromHierarchy() {
        UnsubscribeFromEvents();
        StopUpdateLoop();
        ClearCombatants();
        ClearAbilitySlots();
        k_rewardsPanel.OnContinueClicked -= OnRewardsContinueClicked;
        k_rewardsPanel.RemoveFromHierarchy();
        base.RemoveFromHierarchy();
    }

    #endregion
}
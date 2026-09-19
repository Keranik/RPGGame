using RPGGame.Core;
using RPGGame.Core.Combat;
using RPGGame.Core.Items;
using RPGGame.Core.Prototypes.Item;
using RPGGame.Core.Simulation;
using RPGGame.UI.Components;
using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Screens;

/// <summary>
/// Modal panel displayed after combat ends to show rewards.
/// Uses GameItemSlotDisplay for loot items with proper tooltips and rarity frames.
/// Player must click Continue to dismiss and complete the combat transition.
/// </summary>
public class CombatRewardsPanel : VisualElement {
    #region Fields

    private readonly GameSession k_session;
    private readonly GameDb k_gameDb;

    private GamePanel k_content = null!;
    private GameLabel k_titleLabel = null!;
    private GameLabel k_subtitleLabel = null!;
    private GamePanel k_rewardsContainer = null!;
    private GameContainer k_lootGrid = null!;
    private GameButton k_continueButton = null!;

    private CombatResult? k_currentResult;
    private readonly List<GameItemSlotDisplay> k_lootSlots = [];

    #endregion

    #region Events

    /// <summary>
    /// Fired when the player clicks Continue after viewing rewards.
    /// </summary>
    public event Action? OnContinueClicked;

    #endregion

    #region Constructor

    /// <summary>
    /// Creates a new CombatRewardsPanel.
    /// </summary>
    /// <param name="session">Game session for state transitions.</param>
    /// <param name="gameDb">Game database for item proto lookups.</param>
    public CombatRewardsPanel(GameSession session, GameDb gameDb) {
        k_session = session;
        k_gameDb = gameDb;

        BuildUI();
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

        // Main content panel
        k_content = new GamePanel()
            .SetVariant(PanelVariant.Card)
            .SetPadding(spacing.XL)
            .Build();
        k_content.style.width = dialogStyles.SmallWidth * 1.5f;
        k_content.style.maxWidth = Length.Percent(90);
        k_content.style.maxHeight = Length.Percent(80);

        // Victory/Defeat title
        k_titleLabel = new GameLabel("Victory!")
            .SetStyle(LabelStyle.DisplaySmall)
            .SetColor(LabelColor.Success)
            .SetTextAlign(TextAnchor.MiddleCenter)
            .Build();

        // Subtitle
        k_subtitleLabel = new GameLabel("You defeated all enemies!")
            .SetStyle(LabelStyle.TitleMedium)
            .SetColor(LabelColor.Secondary)
            .SetTextAlign(TextAnchor.MiddleCenter)
            .Build();
        k_subtitleLabel.style.marginTop = spacing.XS;

        // Divider
        var divider = new GameDivider().Build();
        divider.style.marginTop = spacing.MD;
        divider.style.marginBottom = spacing.MD;

        // Rewards container
        k_rewardsContainer = new GamePanel()
            .SetVariant(PanelVariant.Outlined)
            .SetLayout(LayoutDirection.Vertical)
            .SetPadding(spacing.MD)
            .Build();

        // Loot grid for item slots
        k_lootGrid = new GameContainer("loot-grid")
            .SetRow()
            .SetJustifyContent(Justify.Center)
            .SetFlexWrap(Wrap.Wrap)
            .SetMarginTop(spacing.SM);

        // Continue button
        k_continueButton = new GameButton("Continue")
            .SetVariant(ButtonVariant.Primary)
            .SetSize(ButtonSize.Large)
            .OnClick(OnContinue)
            .Build();
        k_continueButton.style.marginTop = spacing.LG;
        k_continueButton.style.alignSelf = Align.Center;

        k_content.Content.Add(k_titleLabel);
        k_content.Content.Add(k_subtitleLabel);
        k_content.Content.Add(divider);
        k_content.Content.Add(k_rewardsContainer);
        k_content.Content.Add(k_continueButton);

        Add(k_content);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Shows the rewards panel with the given combat result.
    /// </summary>
    public void ShowRewards(CombatResult result) {
        k_currentResult = result;

        // Set title based on outcome
        if (result.IsVictory) {
            k_titleLabel.SetText("🎉 Victory!");
            k_titleLabel.SetColor(LabelColor.Success);
            k_subtitleLabel.SetText("You defeated all enemies!");
        } else if (result.Reason == CombatEndReason.Fled) {
            k_titleLabel.SetText("🏃 Escaped!");
            k_titleLabel.SetColor(LabelColor.Warning);
            k_subtitleLabel.SetText("You fled from battle.");
        } else {
            k_titleLabel.SetText("💀 Defeat");
            k_titleLabel.SetColor(LabelColor.Error);
            k_subtitleLabel.SetText("You have been defeated...");
        }

        BuildRewardsList(result);
        style.display = DisplayStyle.Flex;

        Debug.Log($"CombatRewardsPanel: Showing rewards for {result.Encounter.Name}");
    }

    /// <summary>
    /// Hides the panel.
    /// </summary>
    public void Hide() {
        style.display = DisplayStyle.None;
        k_currentResult = null;
        ClearLootSlots();
    }

    #endregion

    #region Private Methods

    private void BuildRewardsList(CombatResult result) {
        var theme = GameTheme.Current;
        var spacing = theme.Spacing;
        var colors = theme.Colors;

        k_rewardsContainer.ClearChildren();
        ClearLootSlots();

        // Header
        var headerLabel = new GameLabel("📦 Rewards")
            .SetStyle(LabelStyle.TitleSmall)
            .SetColor(LabelColor.Primary)
            .SetTextAlign(TextAnchor.MiddleCenter)
            .Build();
        k_rewardsContainer.Content.Add(headerLabel);

        // Experience row
        if (result.ExperienceGained > 0) {
            var xpRow = CreateRewardRow("⭐ Experience", $"+{result.ExperienceGained} XP", ColorRPG.ExperiencePurple);
            k_rewardsContainer.Content.Add(xpRow);
        }

        // Gold row
        if (result.GoldGained > 0) {
            var goldRow = CreateRewardRow("💰 Gold", $"+{result.GoldGained}", ColorRPG.Gold);
            k_rewardsContainer.Content.Add(goldRow);
        }

        // Loot items using GameItemSlotDisplay
        if (result.LootDropped.Count > 0) {
            var lootHeader = new GameLabel("🎁 Items Found:")
                .SetStyle(LabelStyle.BodyMedium)
                .SetColor(LabelColor.Secondary)
                .SetMarginTop(spacing.MD)
                .Build();
            k_rewardsContainer.Content.Add(lootHeader);

            // Clear and rebuild loot grid
            k_lootGrid.Clear();

            foreach (var lootEntry in result.LootDropped) {
                // Try to create an ItemInstance from the loot entry
                var itemProto = k_gameDb.GetOrNull<ItemProto>(lootEntry.ItemId);
                if (itemProto == null) continue;

                // Create a temporary ItemInstance for display
                var itemInstance = new ItemInstance(itemProto, lootEntry.MinQuantity);

                GameItemSlotDisplay slot = new GameItemSlotDisplay()
                    .SetItem(itemInstance)
                    .SetSize(64)
                    .SetDraggable(false)
                    .SetDroppable(false)
                    .Build();

                slot.style.marginRight = spacing.SM;
                slot.style.marginBottom = spacing.SM;

                k_lootSlots.Add(slot);
                k_lootGrid.Add(slot);
            }

            k_rewardsContainer.Content.Add(k_lootGrid);
        }

        // Combat stats section
        var statsDivider = new GameDivider().Build();
        statsDivider.style.marginTop = spacing.MD;
        statsDivider.style.marginBottom = spacing.SM;
        k_rewardsContainer.Content.Add(statsDivider);

        var statsHeader = new GameLabel("📊 Combat Stats")
            .SetStyle(LabelStyle.LabelSmall)
            .SetColor(LabelColor.Tertiary)
            .Build();
        k_rewardsContainer.Content.Add(statsHeader);

        // Enemies defeated
        if (result.EnemiesDefeated > 0) {
            var enemiesRow = CreateRewardRow("💀 Enemies Defeated", result.EnemiesDefeated.ToString(), ColorRPG.MissGray);
            k_rewardsContainer.Content.Add(enemiesRow);
        }

        // Rounds taken
        if (result.RoundsElapsed > 0) {
            var roundsRow = CreateRewardRow("⏱ Rounds", result.RoundsElapsed.ToString(), ColorRPG.MissGray);
            k_rewardsContainer.Content.Add(roundsRow);
        }

        // No rewards message
        if (result.ExperienceGained == 0 && result.GoldGained == 0 && result.LootDropped.Count == 0) {
            var noRewardsLabel = new GameLabel("No rewards gained")
                .SetStyle(LabelStyle.BodyMedium)
                .SetColor(LabelColor.Tertiary)
                .SetTextAlign(TextAnchor.MiddleCenter)
                .SetMarginTop(spacing.SM)
                .Build();
            k_rewardsContainer.Content.Add(noRewardsLabel);
        }
    }

    private GameContainer CreateRewardRow(string label, string value, ColorRPG valueColor) {
        var theme = GameTheme.Current;
        var spacing = theme.Spacing;

        var row = new GameContainer("reward-row")
            .SetRow()
            .SetSpaceBetween()
            .SetAlignItems(Align.Center)
            .SetFullWidth()
            .SetMarginTop(spacing.XS);

        var labelEl = new GameLabel(label)
            .SetStyle(LabelStyle.BodyMedium)
            .SetColor(LabelColor.Secondary)
            .Build();

        var valueEl = new GameLabel(value)
            .SetStyle(LabelStyle.TitleSmall)
            .Build();
        valueEl.style.color = valueColor;

        row.Add(labelEl);
        row.Add(valueEl);

        return row;
    }

    private void ClearLootSlots() {
        foreach (var slot in k_lootSlots) {
            slot.RemoveFromHierarchy();
        }
        k_lootSlots.Clear();
        k_lootGrid.Clear();
    }

    private void OnContinue() {
        Debug.Log("CombatRewardsPanel: Continue clicked");

        Hide();

        // Complete the combat transition in GameSession
        k_session.FinishCombatTransition();

        OnContinueClicked?.Invoke();
    }

    #endregion

    #region Cleanup

    /// <summary>
    /// Removes the panel and cleans up resources.
    /// </summary>
    public new void RemoveFromHierarchy() {
        ClearLootSlots();
        base.RemoveFromHierarchy();
    }

    #endregion
}
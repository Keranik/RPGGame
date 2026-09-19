using RPGGame.Core;
using RPGGame.Core.Generation;
using RPGGame.Core.Items;
using RPGGame.Core.Prototypes.Item;
using RPGGame.Core.Prototypes.Item.Equipment.Armor;
using RPGGame.Core.Prototypes.Item.Equipment.Weapon;
using RPGGame.Core.Prototypes.Stats;
using RPGGame.Core.Rewards;
using RPGGame.Core.Stats;
using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Components;

/// <summary>
/// Modal panel for revealing item rewards with a spinner animation.
/// Shows the gacha spinner, then item details with GameItemSlotDisplay, then equip prompt.
/// </summary>
public class ItemRewardPanel : VisualElement {
    #region Fields

    private readonly GameContainer k_overlay;
    private readonly GamePanel k_content;
    private readonly GameLabel k_titleLabel;
    private readonly GameContainer k_spinnerContainer;
    private readonly GameContainer k_itemDetailsContainer;
    private readonly GameButton k_equipButton;
    private readonly GameButton k_keepButton;
    private readonly GameButton k_skipSpinButton;

    private GameItemSpinner? k_spinner;
    private GameItemSlotDisplay? k_itemSlotDisplay;

    private PendingReward? k_currentReward;
    private Action<ItemInstance?>? k_onComplete;
    private RewardManager? k_rewardManager;

    private RewardPhase k_phase = RewardPhase.Spinning;

    #endregion

    #region Service Accessors
    
    /// <summary>
    /// Gets the GameDb from the service locator. Safe to use in UI since game is always initialized.
    /// </summary>
    private static GameDb GameDb => GameServices.Db;

    /// <summary>
    /// Gets the InventoryManager from the service locator.
    /// </summary>
    private static InventoryManager InventoryManager => GameServices.Inventory;

    #endregion

    #region Constructor

    public ItemRewardPanel() {
        var theme = GameTheme.Current;
        var spacing = theme.Spacing;
        var colors = theme.Colors;
        var dialogStyles = theme.Components.Dialog;
        var spinnerStyles = theme.Components.Spinner;
        float spinnerTotalWidth = spinnerStyles.ItemSlotSize * spinnerStyles.DefaultVisibleItems;
        float requiredContentWidth = spinnerTotalWidth + (spacing.XXXL * 4);

        // Full screen overlay
        style.position = Position.Absolute;
        style.left = 0;
        style.top = 0;
        style.right = 0;
        style.bottom = 0;
        style.display = DisplayStyle.None;
        

        k_overlay = new GameContainer("reward-overlay")
            .SetAbsoluteFill()
            .SetCenter()
            .SetBackgroundColor(colors.BackgroundOverlay)
            .Build();
        k_overlay.pickingMode = PickingMode.Position;

        // Main content panel
        k_content = new GamePanel()
            .SetVariant(PanelVariant.Card)
            .SetPadding(spacing.XL)
            .Build();

        k_content.style.width = MathF.Max(dialogStyles.MinWidth, requiredContentWidth);
        k_content.style.alignItems = Align.Center;

        // Title
        k_titleLabel = new GameLabel("You found something!")
            .SetStyle(LabelStyle.HeadlineMedium)
            .SetColor(LabelColor.Primary)
            .SetTextAlign(TextAnchor.MiddleCenter)
            .Build();
        k_titleLabel.style.marginBottom = spacing.XL;

        // Spinner container
        k_spinnerContainer = new GameContainer("spinner-container")
            .SetColumn()
            .SetCenter()
            .Build();
        k_spinnerContainer.style.marginBottom = spacing.XL;

        // Skip spin button
        k_skipSpinButton = new GameButton("Skip →")
            .SetVariant(ButtonVariant.Ghost)
            .SetSize(ButtonSize.Small)
            .OnClick(OnSkipSpinClicked)
            .Build();
        k_skipSpinButton.style.marginTop = spacing.XS;

        // Item details container (hidden during spin)
        k_itemDetailsContainer = new GameContainer("item-details")
            .SetColumn()
            .SetCenter()
            .Build();
        k_itemDetailsContainer.style.display = DisplayStyle.None;

        // Action buttons
        var buttonRow = new GameContainer("button-row")
            .SetRow()
            .SetJustifyContent(Justify.Center)
            .Build();
        buttonRow.style.marginTop = spacing.XL;

        k_equipButton = new GameButton("⚔️ Equip Now")
            .SetVariant(ButtonVariant.Primary)
            .SetSize(ButtonSize.Large)
            .OnClick(OnEquipClicked)
            .Build();
        k_equipButton.style.marginRight = spacing.MD;

        k_keepButton = new GameButton("🎒 Keep in Bag")
            .SetVariant(ButtonVariant.Outline)
            .SetSize(ButtonSize.Large)
            .OnClick(OnKeepClicked)
            .Build();

        buttonRow
            .AddChild(k_equipButton)
            .AddChild(k_keepButton);

        k_itemDetailsContainer.AddChild(buttonRow);

        // Build hierarchy
        k_content.Content.Add(k_titleLabel);
        k_content.Content.Add(k_spinnerContainer);
        k_content.Content.Add(k_skipSpinButton);
        k_content.Content.Add(k_itemDetailsContainer);

        k_overlay.AddChild(k_content);
        Add(k_overlay);

        GameTheme.OnThemeChanged += OnThemeChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Sets the reward manager dependency.
    /// </summary>
    public ItemRewardPanel SetRewardManager(RewardManager rewardManager) {
        k_rewardManager = rewardManager;
        return this;
    }

    /// <summary>
    /// Shows a pending reward with spinner animation.
    /// </summary>
    public void ShowReward(PendingReward reward, Action<ItemInstance?> onComplete) {
        if (!reward.IsValid) {
            Debug.LogError("Invalid reward");
            onComplete?.Invoke(null);
            return;
        }

        k_currentReward = reward;
        k_onComplete = onComplete;
        k_phase = RewardPhase.Spinning;

        // Set title based on reward type
        k_titleLabel.SetText(reward.Type switch {
            RewardType.Weapon => "⚔️ A Weapon Awaits!",
            RewardType.Armor => "🛡️ Armor Found!",
            RewardType.Accessory => "💍 Treasure Discovered!",
            _ => "You found something!"
        });

        // Build the spinner
        BuildSpinner(reward);

        // Show spinner phase - disable action buttons during spin
        k_spinnerContainer.style.display = DisplayStyle.Flex;
        k_skipSpinButton.style.display = DisplayStyle.Flex;
        k_itemDetailsContainer.style.display = DisplayStyle.None;
        k_equipButton.SetEnabled(false);
        k_keepButton.SetEnabled(false);

        style.display = DisplayStyle.Flex;

        // Start spinning after a brief delay
        schedule.Execute(() => k_spinner?.Spin()).ExecuteLater(300);
    }

    /// <summary>
    /// Hides the panel.
    /// </summary>
    public void Hide() {
        style.display = DisplayStyle.None;
        k_itemSlotDisplay?.RemoveFromHierarchy();
        k_itemSlotDisplay = null;
        k_currentReward = null;
        k_onComplete = null;
        k_phase = RewardPhase.Complete;
    }

    #endregion

    #region Spinner Building

    private void BuildSpinner(PendingReward reward) {
        var theme = GameTheme.Current;
        var listStyles = theme.Components.List;

        k_spinnerContainer.ClearChildren();

        k_spinner = new GameItemSpinner()
            .SetItems(reward.Candidates)
            .SetWinningIndex(reward.WinningIndex)
            .SetSpinDuration(2500)
            .SetVisibleItems(5)
            .SetItemSize(listStyles.ItemHeight * 2)
            .OnSpinComplete(OnSpinComplete)
            .Build();

        k_spinnerContainer.AddChild(k_spinner);
    }

    #endregion

    #region Item Details

    private void ShowItemDetails(GeneratedItem item) {
        var theme = GameTheme.Current;
        var spacing = theme.Spacing;
        var colors = theme.Colors;

        // Only transition to details if we were spinning
        if (k_phase != RewardPhase.Spinning) {
            return;
        }

        k_phase = RewardPhase.Details;

        // Hide spinner, show details
        k_spinnerContainer.style.display = DisplayStyle.None;
        k_skipSpinButton.style.display = DisplayStyle.None;
        k_itemDetailsContainer.style.display = DisplayStyle.Flex;

        // Enable action buttons now that spin is complete
        k_equipButton.SetEnabled(true);
        k_keepButton.SetEnabled(true);

        // Clear and rebuild details
        k_itemDetailsContainer.ClearChildren();

        // Create item display container
        var itemDisplayContainer = new GameContainer("item-display")
            .SetColumn()
            .SetCenter()
            .Build();
        itemDisplayContainer.style.marginBottom = spacing.MD;

        // Create a preview ItemInstance for the GameItemSlotDisplay
        var previewInstance = CreatePreviewInstance(item);

        // Use GameItemSlotDisplay for the item visualization
        float slotSize = theme.Components.List.ItemHeight * 2.5f;

        // Clean up previous slot if exists
        k_itemSlotDisplay?.RemoveFromHierarchy();

        k_itemSlotDisplay = new GameItemSlotDisplay()
            .SetSize(slotSize)
            .SetRarity(item.Rarity)
            .SetInteractive(true)
            .SetDraggable(false)
            .SetDroppable(false);

        // Set item (which will use the rarity and show initials if no icon)
        if (previewInstance != null) {
            k_itemSlotDisplay.SetItem(previewInstance);
        }

        k_itemSlotDisplay.Build();

        itemDisplayContainer.AddChild(k_itemSlotDisplay);

        // Item name with rarity color
        var nameLabel = new GameLabel(item.DisplayName)
            .SetStyle(LabelStyle.TitleLarge)
            .Build();
        nameLabel.style.color = colors.GetRarityColor(item.Rarity);
        nameLabel.style.marginTop = spacing.SM;
        itemDisplayContainer.AddChild(nameLabel);

        // Item type subtitle
        string itemType = GetItemTypeString(item);
        if (!string.IsNullOrEmpty(itemType)) {
            var typeLabel = new GameLabel(itemType)
                .SetStyle(LabelStyle.BodySmall)
                .SetColor(LabelColor.Secondary)
                .Build();
            typeLabel.style.marginTop = spacing.XXS;
            itemDisplayContainer.AddChild(typeLabel);
        }

        // Rarity badge
        var rarityBadge = new GameBadge(item.Rarity.ToString())
            .SetVariant(GetRarityBadgeVariant(item.Rarity))
            .Build();
        rarityBadge.style.marginTop = spacing.XS;
        itemDisplayContainer.AddChild(rarityBadge);

        k_itemDetailsContainer.AddChild(itemDisplayContainer);

        // Stats display panel
        var statsPanel = new GamePanel()
            .SetVariant(PanelVariant.Outlined)
            .SetPadding(spacing.SM)
            .Build();
        statsPanel.style.width = 320;
        statsPanel.style.marginBottom = spacing.MD;

        BuildStatsDisplay(statsPanel.Content, item);

        k_itemDetailsContainer.AddChild(statsPanel);

        // Add buttons back
        var buttonRow = new GameContainer("button-row")
            .SetRow()
            .SetJustifyContent(Justify.Center)
            .Build();
        buttonRow.style.marginTop = spacing.MD;

        buttonRow
            .AddChild(k_equipButton)
            .AddChild(k_keepButton);

        k_itemDetailsContainer.AddChild(buttonRow);
    }

    private void BuildStatsDisplay(VisualElement container, GeneratedItem item) {
        var theme = GameTheme.Current;
        var spacing = theme.Spacing;

        // Show base stats first (damage for weapons, armor for armor)
        if (item is GeneratedWeapon weapon) {
            var damageRow = new GameContainer("damage-row")
                .SetRow()
                .SetSpaceBetween()
                .Build();
            damageRow.style.marginBottom = spacing.XS;

            var damageLabel = new GameLabel("Damage")
                .SetStyle(LabelStyle.BodySmall)
                .SetColor(LabelColor.Secondary)
                .Build();

            var damageValue = new GameLabel(weapon.FinalDamageDice.ToString())
                .SetStyle(LabelStyle.BodySmall)
                .Build();
            damageValue.style.color = ColorRPG.RageRed;
            damageValue.style.unityFontStyleAndWeight = FontStyle.Bold;

            damageRow
                .AddChild(damageLabel)
                .AddChild(damageValue);
            container.Add(damageRow);
        } else if (item is GeneratedArmor armor && armor.FinalArmorBonus > 0) {
            var armorRow = new GameContainer("armor-row")
                .SetRow()
                .SetSpaceBetween()
                .Build();
            armorRow.style.marginBottom = spacing.XS;

            var armorLabel = new GameLabel("Armor")
                .SetStyle(LabelStyle.BodySmall)
                .SetColor(LabelColor.Secondary)
                .Build();

            var armorValue = new GameLabel($"+{armor.BaseProto.ArmorBonus + armor.BonusAC}")
                .SetStyle(LabelStyle.BodySmall)
                .Build();
            armorValue.style.color = ColorRPG.ShieldBlue;
            armorValue.style.unityFontStyleAndWeight = FontStyle.Bold;

            armorRow
                .AddChild(armorLabel)
                .AddChild(armorValue);
            container.Add(armorRow);
        }

        // Show equipment stats
        var stats = item.GetEquipmentStats();
        if (stats.Count == 0) {
            var noStatsLabel = new GameLabel("No bonus stats")
                .SetStyle(LabelStyle.Caption)
                .SetColor(LabelColor.Tertiary)
                .SetTextAlign(TextAnchor.MiddleCenter)
                .Build();
            container.Add(noStatsLabel);
        } else {
            foreach (var equipStat in stats) {
                var statRow = new GameContainer($"stat-{equipStat.Target.Value}")
                    .SetRow()
                    .SetSpaceBetween()
                    .Build();
                statRow.style.marginBottom = spacing.XXS;

                string statName = GetStatDisplayName(equipStat.Target.Value);
                var statNameLabel = new GameLabel(statName)
                    .SetStyle(LabelStyle.Caption)
                    .SetColor(LabelColor.Secondary)
                    .Build();

                string sign = equipStat.Value >= 0 ? "+" : "";
                string suffix = equipStat.Operation == ModifierOperation.PercentMore ? "%" : "";
                var statValueLabel = new GameLabel($"{sign}{equipStat.Value:F0}{suffix}")
                    .SetStyle(LabelStyle.Caption)
                    .Build();
                statValueLabel.style.color = equipStat.Value >= 0 ? ColorRPG.HealGreen : ColorRPG.RageRed;
                statValueLabel.style.unityFontStyleAndWeight = FontStyle.Bold;

                statRow
                    .AddChild(statNameLabel)
                    .AddChild(statValueLabel);

                container.Add(statRow);
            }
        }

        // Divider before metadata
        var divider = new GameDivider().Build();
        divider.style.marginTop = spacing.XS;
        divider.style.marginBottom = spacing.XS;
        container.Add(divider);

        // Show durability if applicable
        if (item.BaseDurability > 0) {
            int totalDur = item.BaseDurability + item.BonusDurability;
            string durText = item.BonusDurability > 0
                ? $"{totalDur} (+{item.BonusDurability})"
                : totalDur.ToString();

            var durRow = new GameContainer("durability-row")
                .SetRow()
                .SetSpaceBetween()
                .Build();

            var durLabel = new GameLabel("Durability")
                .SetStyle(LabelStyle.Caption)
                .SetColor(LabelColor.Tertiary)
                .Build();

            var durValueLabel = new GameLabel(durText)
                .SetStyle(LabelStyle.Caption)
                .SetColor(LabelColor.Secondary)
                .Build();

            durRow
                .AddChild(durLabel)
                .AddChild(durValueLabel);

            container.Add(durRow);
        }

        // Show value
        var valueRow = new GameContainer("value-row")
            .SetRow()
            .SetSpaceBetween()
            .Build();

        var valueLabel = new GameLabel("Value")
            .SetStyle(LabelStyle.Caption)
            .SetColor(LabelColor.Tertiary)
            .Build();

        var valueAmountLabel = new GameLabel($"{item.FinalBuyPrice:N0} gold")
            .SetStyle(LabelStyle.Caption)
            .Build();
        valueAmountLabel.style.color = ColorRPG.Gold;

        valueRow
            .AddChild(valueLabel)
            .AddChild(valueAmountLabel);

        container.Add(valueRow);
    }

    /// <summary>
    /// Creates a preview ItemInstance from a GeneratedItem for display purposes.
    /// </summary>
    private ItemInstance? CreatePreviewInstance(GeneratedItem item) {
        var gameDb = GameDb;

        ItemProto? proto = null;

        if (item is GeneratedWeapon weapon) {
            proto = gameDb.GetOrNull<WeaponProto>(
                new WeaponProto.ID(weapon.BaseProto.Id.Value));
        } else if (item is GeneratedArmor armor) {
            proto = gameDb.GetOrNull<ArmorProto>(
                new ArmorProto.ID(armor.BaseProto.Id.Value));
        }

        if (proto == null) {
            Debug.LogWarning($"ItemRewardPanel: Could not find prototype for {item.DisplayName}");
            return null;
        }

        // Create a preview instance with the generated item's properties
        int durability = item.BaseDurability + item.BonusDurability;
        var instance = new ItemInstance(proto, 1) {
            CustomName = item.DisplayName,
            Durability = durability > 0 ? durability : null,
            MaxDurability = durability > 0 ? durability : null,
            GeneratedData = item // Store the generated data for tooltip access
        };

        return instance;
    }

    private string GetItemTypeString(GeneratedItem item) {
        return item switch {
            GeneratedWeapon weapon => $"{weapon.BaseProto.WeaponType}",
            GeneratedArmor armor => $"{armor.BaseProto.ArmorType}",
            _ => ""
        };
    }

    private string GetStatDisplayName(string idValue) {
        // Try to get from GameDb first
        var statProto = GameDb.GetOrNull<StatProto>(new StatProto.ID(idValue));
        if (statProto != null) {
            return statProto.DisplayText.Name;
        }

        // Fallback: extract from ID
        var parts = idValue.Split('_');
        if (parts.Length >= 2) {
            return parts[^1];
        }
        return idValue;
    }

    private BadgeVariant GetRarityBadgeVariant(RarityType rarity) {
        return rarity switch {
            RarityType.Common => BadgeVariant.Default,
            RarityType.Uncommon => BadgeVariant.Success,
            RarityType.Rare => BadgeVariant.Info,
            RarityType.Epic => BadgeVariant.Warning,
            RarityType.Legendary => BadgeVariant.Error,
            RarityType.Mythic => BadgeVariant.Error,
            _ => BadgeVariant.Default
        };
    }

    #endregion

    #region Event Handlers

    private void OnSpinComplete(GeneratedItem item) {
        ShowItemDetails(item);
    }

    private void OnSkipSpinClicked() {
        if (k_phase != RewardPhase.Spinning) {
            return;
        }

        if (k_currentReward?.WinningItem != null) {
            k_spinner?.ShowResultImmediate();
            ShowItemDetails(k_currentReward.WinningItem);
        }
    }

    private void OnEquipClicked() {
        if (k_phase != RewardPhase.Details) {
            return;
        }

        if (k_currentReward == null || k_rewardManager == null) {
            Complete(null);
            return;
        }

        k_phase = RewardPhase.Complete;
        var instance = k_rewardManager.ClaimAndEquipReward(k_currentReward);
        Complete(instance);
    }

    private void OnKeepClicked() {
        if (k_phase != RewardPhase.Details) {
            return;
        }

        if (k_currentReward == null || k_rewardManager == null) {
            Complete(null);
            return;
        }

        k_phase = RewardPhase.Complete;
        var instance = k_rewardManager.ClaimReward(k_currentReward);
        Complete(instance);
    }

    private void Complete(ItemInstance? instance) {
        var callback = k_onComplete;
        Hide();
        callback?.Invoke(instance);
    }

    #endregion

    #region Theme

    private void OnThemeChanged(GameTheme theme) {
        var spacing = theme.Spacing;
        var colors = theme.Colors;
        var dialogStyles = theme.Components.Dialog;
        var spinnerStyles = theme.Components.Spinner;

        float spinnerTotalWidth = spinnerStyles.ItemSlotSize * spinnerStyles.DefaultVisibleItems;
        float requiredContentWidth = spinnerTotalWidth + (spacing.XXXL * 4);

        k_content.style.width = MathF.Max(dialogStyles.MinWidth, requiredContentWidth);
        k_overlay.SetBackgroundColor(colors.BackgroundOverlay);
    }

    #endregion

    #region Cleanup

    public new void RemoveFromHierarchy() {
        GameTheme.OnThemeChanged -= OnThemeChanged;
        k_spinner?.RemoveFromHierarchy();
        k_itemSlotDisplay?.RemoveFromHierarchy();
        base.RemoveFromHierarchy();
    }

    #endregion
}

#region Supporting Types

internal enum RewardPhase {
    Spinning,
    Details,
    Complete
}

#endregion
using RPGGame.Core;
using RPGGame.Core.Items;
using RPGGame.Core.Prototypes.Expedition;
using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Components;

/// <summary>
/// Slot display for terrain types (TerrainProto).
/// Supports three-tier display:
/// - Compact: Terrain color/icon with traversability indicator
/// - Summary: Movement speed, key effects, passability
/// - Detailed: Full terrain stats, environmental effects, resources
/// </summary>
public class GameTerrainSlotDisplay : BaseGameSlotDisplay<GameTerrainSlotDisplay> {
    #region Fields

    private TerrainProto? k_terrainProto;
    private bool k_showMovementIndicator = true;

    private readonly GameContainer k_terrainColorOverlay;
    private readonly GameLabel k_movementBadge;
    private readonly GameLabel k_passabilityIndicator;

    #endregion

    #region Properties

    /// <summary>The terrain prototype being displayed.</summary>
    public TerrainProto? TerrainProto => k_terrainProto;

    /// <inheritdoc/>
    public override bool IsEmpty => k_terrainProto == null;

    /// <summary>Whether this terrain is passable.</summary>
    public bool IsPassable => k_terrainProto?.IsPassable ?? true;

    #endregion

    #region Constructor

    public GameTerrainSlotDisplay() : base() {
        var theme = GameTheme.Current;
        var colors = theme.Colors;
        var borders = theme.Borders;

        // Terrain color overlay
        k_terrainColorOverlay = new GameContainer("terrain-color")
            .SetAbsoluteFill()
            .SetBorderRadius(borders.RadiusMD)
            .SetOpacity(0.4f)
            .SetPickingMode(PickingMode.Ignore);
        k_rootContainer.Insert(0, k_terrainColorOverlay);

        // Movement speed badge (top-right)
        k_movementBadge = new GameLabel()
            .SetStyle(LabelStyle.Caption)
            .SetTextAlign(TextAnchor.MiddleCenter)
            .SetPadding(0, 3)
            .Build();
        k_movementBadge.style.position = Position.Absolute;
        k_movementBadge.style.right = 2;
        k_movementBadge.style.top = 2;
        k_movementBadge.style.backgroundColor = colors.BackgroundElevated;
        k_movementBadge.style.borderTopLeftRadius = borders.RadiusXS;
        k_movementBadge.style.borderTopRightRadius = borders.RadiusXS;
        k_movementBadge.style.borderBottomLeftRadius = borders.RadiusXS;
        k_movementBadge.style.borderBottomRightRadius = borders.RadiusXS;
        k_movementBadge.style.display = DisplayStyle.None;
        k_rootContainer.Add(k_movementBadge);

        // Passability indicator (bottom-right)
        k_passabilityIndicator = new GameLabel()
            .SetStyle(LabelStyle.Caption)
            .SetTextAlign(TextAnchor.MiddleCenter)
            .Build();
        k_passabilityIndicator.style.position = Position.Absolute;
        k_passabilityIndicator.style.right = 2;
        k_passabilityIndicator.style.bottom = 2;
        k_passabilityIndicator.style.display = DisplayStyle.None;
        k_rootContainer.Add(k_passabilityIndicator);

        SetSize(GameTheme.Current.Components.Slot.SizeS);
        SetShowDurability(false);
        SetShowStackCount(false);
    }

    #endregion

    #region Fluent API - Terrain Data

    /// <summary>
    /// Sets the terrain to display.
    /// </summary>
    public GameTerrainSlotDisplay SetTerrain(TerrainProto? terrain) {
        k_terrainProto = terrain;

        if (terrain != null) {
            // Set rarity based on movement difficulty
            RarityType rarity = terrain.MovementSpeedMultiplier.Value switch {
                >= 1.2f => RarityType.Common,
                >= 1.0f => RarityType.Common,
                >= 0.7f => RarityType.Uncommon,
                >= 0.5f => RarityType.Rare,
                >= 0.3f => RarityType.Epic,
                _ => RarityType.Legendary
            };

            if (!terrain.IsPassable) {
                rarity = RarityType.Legendary;
            }

            SetRarity(rarity);
        }

        RefreshDisplay();
        return this;
    }

    /// <summary>
    /// Clears the terrain from this slot.
    /// </summary>
    public GameTerrainSlotDisplay ClearTerrain() => SetTerrain(null);

    /// <summary>
    /// Sets whether to show movement speed indicator.
    /// </summary>
    public GameTerrainSlotDisplay SetShowMovementIndicator(bool show) {
        k_showMovementIndicator = show;
        RefreshDisplay();
        return this;
    }

    #endregion

    #region Abstract Implementation

    protected override Texture2D? GetIcon() => null;

    protected override string GetDisplayName() => k_terrainProto?.DisplayText.Name ?? "Unknown Terrain";

    protected override string GetPlaceholderInitials() => GetInitials(k_terrainProto?.DisplayText.Name);

    protected override GameTooltip CreateBasicTooltip() {
        if (k_terrainProto == null) {
            return new GameTooltip()
                .SetContent("Unknown Terrain")
                .SetPosition(TooltipPosition.Top)
                .SetDelay(0);
        }

        string passText = k_terrainProto.IsPassable ? "✓ Passable" : "✗ Impassable";
        string text = $"{k_terrainProto.DisplayText.Name}\n🚶 {GetSpeedText()}\n{passText}";

        return new GameTooltip()
            .SetContent(text)
            .SetPosition(TooltipPosition.Top)
            .SetDelay(0);
    }

    protected override GameTooltip CreateExpandedTooltip() {
        var theme = GameTheme.Current;
        var spacing = theme.Spacing;
        var colors = theme.Colors;
        var borders = theme.Borders;

        var content = new GameContainer("terrain-tooltip")
            .SetColumn()
            .SetPadding(spacing.SM)
            .SetWidth(280);

        if (k_terrainProto == null) {
            content.Add(new GameLabel()
                .SetText("Unknown Terrain")
                .SetStyle(LabelStyle.BodyMedium)
                .SetColor(LabelColor.Tertiary)
                .Build());
        } else {
            // Header
            var headerRow = new GameContainer()
                .SetRow()
                .SetAlignItems(Align.Center);

            ColorRPG terrainColor = ColorRPG.FromHex(k_terrainProto.MapColor);
            var colorDot = new GameContainer()
                .SetSize(16, 16)
                .SetBorderRadius(8)
                .SetBackgroundColor(terrainColor);
            headerRow.Add(colorDot);

            var nameLabel = new GameLabel()
                .SetText(k_terrainProto.DisplayText.Name)
                .SetStyle(LabelStyle.TitleMedium)
                .SetColor(LabelColor.Primary)
                .SetMarginLeft(spacing.XS)
                .Build();
            headerRow.Add(nameLabel);

            content.Add(headerRow);

            // Category
            content.Add(new GameLabel()
                .SetText($"{k_terrainProto.Category} Terrain")
                .SetStyle(LabelStyle.Caption)
                .SetColor(LabelColor.Secondary)
                .SetMarginBottom(spacing.SM)
                .Build());

            // Description
            if (!string.IsNullOrEmpty(k_terrainProto.DisplayText.Description)) {
                content.Add(new GameLabel()
                    .SetText(k_terrainProto.DisplayText.Description)
                    .SetStyle(LabelStyle.BodySmall)
                    .SetColor(LabelColor.Secondary)
                    .SetWhiteSpace(WhiteSpace.Normal)
                    .SetMarginBottom(spacing.SM)
                    .Build());
            }

            content.Add(new GameDivider().Build());

            // Movement & Travel
            AddStatRow(content, "🚶 Movement Speed", GetSpeedText(), spacing);
            AddStatRow(content, "💪 Stamina Drain", $"{k_terrainProto.StaminaDrainPerDistance:F1}x", spacing);
            AddStatRow(content, "😓 Fatigue Rate", $"{k_terrainProto.FatigueRateMultiplier:F1}x", spacing);
            AddStatRow(content, "👁️ Vision Range", $"{k_terrainProto.VisionRangeMultiplier:P0}", spacing);

            // Passability
            if (!k_terrainProto.IsPassable) {
                content.Add(new GameLabel()
                    .SetText("⛔ Impassable")
                    .SetStyle(LabelStyle.Caption)
                    .SetColor(LabelColor.Error)
                    .SetMarginTop(spacing.XS)
                    .Build());
            }

            // Special requirements
            if (k_terrainProto.RequiresSpecialEquipment && !string.IsNullOrEmpty(k_terrainProto.RequiredEquipmentType)) {
                content.Add(new GameLabel()
                    .SetText($"🎒 Requires: {k_terrainProto.RequiredEquipmentType}")
                    .SetStyle(LabelStyle.Caption)
                    .SetColor(LabelColor.Warning)
                    .SetMarginTop(spacing.XS)
                    .Build());
            }

            // Environmental effects
            bool hasEnvEffects = k_terrainProto.EnvironmentalDamagePerHour > 0 ||
                                Math.Abs(k_terrainProto.MoraleModifierPerHour) > 0.01f ||
                                k_terrainProto.ProvidesNaturalShelter;

            if (hasEnvEffects) {
                content.Add(new GameDivider().Build());

                content.Add(new GameLabel()
                    .SetText("Environmental Effects")
                    .SetStyle(LabelStyle.Caption)
                    .SetColor(LabelColor.Secondary)
                    .Build());

                if (k_terrainProto.EnvironmentalDamagePerHour > 0) {
                    AddStatRow(content, $"💔 {k_terrainProto.EnvironmentalDamageType} Damage",
                        $"{k_terrainProto.EnvironmentalDamagePerHour}/hr", spacing);
                }

                if (k_terrainProto.MoraleModifierPerHour > 0) {
                    AddStatRow(content, "😊 Morale", $"+{k_terrainProto.MoraleModifierPerHour}/hr", spacing);
                } else if (k_terrainProto.MoraleModifierPerHour < 0) {
                    AddStatRow(content, "😔 Morale", $"{k_terrainProto.MoraleModifierPerHour}/hr", spacing);
                }

                if (k_terrainProto.ProvidesNaturalShelter) {
                    AddStatRow(content, "🏕️ Shelter", "Natural shelter available", spacing);
                    AddStatRow(content, "😴 Rest Bonus", $"{k_terrainProto.RestEffectivenessMultiplier:F1}x", spacing);
                }
            }

            // Encounters
            bool hasEncounterMods = Math.Abs(k_terrainProto.EncounterChanceMultiplier - 1.0f) > 0.01f ||
                                   Math.Abs(k_terrainProto.AmbushChanceMultiplier - 1.0f) > 0.01f ||
                                   k_terrainProto.BlocksLineOfSight;

            if (hasEncounterMods) {
                content.Add(new GameDivider().Build());

                content.Add(new GameLabel()
                    .SetText("Combat & Encounters")
                    .SetStyle(LabelStyle.Caption)
                    .SetColor(LabelColor.Secondary)
                    .Build());

                if (Math.Abs(k_terrainProto.EncounterChanceMultiplier - 1.0f) > 0.01f) {
                    AddStatRow(content, "⚔️ Encounter Chance", $"{k_terrainProto.EncounterChanceMultiplier:F1}x", spacing);
                }

                if (Math.Abs(k_terrainProto.AmbushChanceMultiplier - 1.0f) > 0.01f) {
                    AddStatRow(content, "🎯 Ambush Chance", $"{k_terrainProto.AmbushChanceMultiplier:F1}x", spacing);
                }

                if (k_terrainProto.BlocksLineOfSight) {
                    AddStatRow(content, "👁️ Line of Sight", "Blocked", spacing);
                }
            }

            // Resources
            bool hasResourceMods = k_terrainProto.ResourceCategories.Count > 0 ||
                                  Math.Abs(k_terrainProto.ForagingChanceMultiplier - 1.0f) > 0.01f ||
                                  Math.Abs(k_terrainProto.HuntingSuccessMultiplier - 1.0f) > 0.01f;

            if (hasResourceMods) {
                content.Add(new GameDivider().Build());

                content.Add(new GameLabel()
                    .SetText("Resources & Gathering")
                    .SetStyle(LabelStyle.Caption)
                    .SetColor(LabelColor.Secondary)
                    .Build());

                AddStatRow(content, "💧 Water", $"{k_terrainProto.WaterAvailability:P0}", spacing);

                if (Math.Abs(k_terrainProto.ForagingChanceMultiplier - 1.0f) > 0.01f) {
                    AddStatRow(content, "🌿 Foraging", $"{k_terrainProto.ForagingChanceMultiplier:F1}x", spacing);
                }

                if (Math.Abs(k_terrainProto.HuntingSuccessMultiplier - 1.0f) > 0.01f) {
                    AddStatRow(content, "🏹 Hunting", $"{k_terrainProto.HuntingSuccessMultiplier:F1}x", spacing);
                }

                if (k_terrainProto.ResourceCategories.Count > 0) {
                    var resourceRow = new GameContainer()
                        .SetRow()
                        .SetFlexWrap(Wrap.Wrap);

                    foreach (var category in k_terrainProto.ResourceCategories.Take(6)) {
                        var chip = new GameContainer()
                            .SetBackgroundColor(colors.Surface)
                            .SetBorderRadius(borders.RadiusXS)
                            .SetPadding(0, 4)
                            .SetMargin(0, spacing.XS, spacing.XXS, 0);
                        chip.Add(new GameLabel()
                            .SetText(category.ToString())
                            .SetStyle(LabelStyle.Caption)
                            .Build());
                        resourceRow.Add(chip);
                    }

                    content.Add(resourceRow);
                }
            }

            // Tags
            if (k_terrainProto.Tags.Count > 0) {
                content.Add(new GameDivider().Build());

                var tagRow = new GameContainer()
                    .SetRow()
                    .SetFlexWrap(Wrap.Wrap);

                foreach (var tag in k_terrainProto.Tags.Take(6)) {
                    var tagChip = new GameContainer()
                        .SetBackgroundColor(colors.Primary.WithAlpha(0.2f))
                        .SetBorderRadius(borders.RadiusXS)
                        .SetPadding(0, 4)
                        .SetMargin(0, spacing.XS, spacing.XXS, 0);
                    tagChip.Add(new GameLabel()
                        .SetText(tag.Value)
                        .SetStyle(LabelStyle.Caption)
                        .Build());
                    tagRow.Add(tagChip);
                }

                content.Add(tagRow);
            }
        }

        return new GameTooltip()
            .SetContent(content)
            .SetPosition(TooltipPosition.Top);
    }

    private void AddStatRow(GameContainer parent, string label, string value, SpacingSettings spacing) {
        var row = new GameContainer()
            .SetRow()
            .SetJustifyContent(Justify.SpaceBetween)
            .SetFullWidth();

        row.Add(new GameLabel()
            .SetText(label)
            .SetStyle(LabelStyle.Caption)
            .SetColor(LabelColor.Secondary)
            .Build());

        row.Add(new GameLabel()
            .SetText(value)
            .SetStyle(LabelStyle.Caption)
            .SetColor(LabelColor.Primary)
            .Build());

        parent.Add(row);
    }

    private string GetSpeedText() {
        if (k_terrainProto == null) return "Unknown";
        return k_terrainProto.MovementSpeedMultiplier.Value switch {
            >= 1.5f => "Very Fast",
            >= 1.2f => "Fast",
            >= 0.9f => "Normal",
            >= 0.7f => "Slow",
            >= 0.5f => "Very Slow",
            >= 0.3f => "Crawling",
            _ => "Impassable"
        };
    }

    public override ItemInstance? GetDraggedItem() => null;

    protected override void OnRightClickAction(Vector2 mousePosition) {
        if (k_terrainProto == null) return;

        var menu = GameContextMenu.Show(this, mousePosition)
            .AddHeader(k_terrainProto.DisplayText.Name);

        menu.AddItem("🔍 Inspect", () => {
            ShowDetailedInspector();
            k_onInspect?.Invoke();
        });

        menu.AddDivider();

        string passText = k_terrainProto.IsPassable ? "✓ Passable" : "✗ Impassable";
        menu.AddItem(passText, null, enabled: false);
        menu.AddItem($"🚶 Speed: {GetSpeedText()}", null, enabled: false);
        menu.AddItem($"📂 {k_terrainProto.Category}", null, enabled: false);

        if (k_terrainProto.ResourceCategories.Count > 0) {
            menu.AddDivider();
            menu.AddItem("⛏️ Gather Resources", () => {
                GameToast.Show("Starting gathering...", ToastType.Info);
            });
        }

        menu.AddDivider();
        menu.AddItem("Cancel", null);
    }

    protected override void OnRefreshComplete() {
        var colors = GameTheme.Current.Colors;

        if (k_terrainProto == null) {
            k_terrainColorOverlay.SetVisible(false);
            k_movementBadge.SetVisible(false);
            k_passabilityIndicator.SetVisible(false);
            return;
        }

        // Terrain color overlay from MapColor
        ColorRPG terrainColor = ColorRPG.FromHex(k_terrainProto.MapColor);
        k_terrainColorOverlay
            .SetBackgroundColor(terrainColor)
            .SetOpacity(0.4f)
            .SetVisible(true);

        // Movement speed badge (show if not normal speed)
        if (k_showMovementIndicator && Math.Abs(k_terrainProto.MovementSpeedMultiplier - 1.0f) > 0.1f) {
            k_movementBadge.SetText($"{k_terrainProto.MovementSpeedMultiplier:F1}x");

            ColorRPG speedColor = k_terrainProto.MovementSpeedMultiplier.Value switch {
                >= 1.2f => ColorRPG.HealthGreen,
                >= 0.8f => ColorRPG.StaminaYellow,
                >= 0.5f => ColorRPG.Fire,
                _ => ColorRPG.Blood
            };

            k_movementBadge.style.backgroundColor = speedColor;
            k_movementBadge.style.color = speedColor.ContrastingTextColor();
            k_movementBadge.SetVisible(true);
        } else {
            k_movementBadge.SetVisible(false);
        }

        // Passability indicator
        if (!k_terrainProto.IsPassable) {
            k_passabilityIndicator.SetText("⛔");
            k_passabilityIndicator.SetVisible(true);
            k_rootContainer.SetOpacity(0.6f);
        } else {
            k_passabilityIndicator.SetVisible(false);
            k_rootContainer.SetOpacity(k_isEnabled ? 1f : 0.5f);
        }

        // Placeholder color
        k_placeholderLabel.style.color = terrainColor;
    }

    #endregion
}
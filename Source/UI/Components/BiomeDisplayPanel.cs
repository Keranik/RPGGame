using RPGGame.Core;
using RPGGame.Core.Expedition;
using RPGGame.Core.Prototypes.Expedition;
using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Components;

/// <summary>
/// A panel for displaying current biome, terrain, and environmental information.
/// Uses GameBiomeSlotDisplay and GameTerrainSlotDisplay for consistent presentation.
/// Supports Compact and Expanded view modes via expand/collapse arrow.
/// </summary>
public class BiomeDisplayPanel : VisualElement {
    #region Private Fields

    private readonly GameDb k_gameDb;
    private readonly ExpeditionManager k_expeditionManager;

    private bool k_isExpanded = false;

    // Layout containers
    private GameContainer k_root = null!;
    private GameContainer k_slotsRow = null!;
    private GameContainer k_statsSection = null!;
    private GameContainer k_effectsSection = null!;
    private GameContainer k_expandButtonContainer = null!;

    // Slot displays
    private GameBiomeSlotDisplay k_biomeSlot = null!;
    private GameTerrainSlotDisplay k_terrainSlot = null!;

    // Expanded tooltips (shown when expanded)
    private GameContainer k_biomeDetails = null!;
    private GameContainer k_terrainDetails = null!;

    // Stats display
    private GameLabel k_dangerLabel = null!;
    private GameLabel k_movementLabel = null!;
    private GameLabel k_visibilityLabel = null!;

    // Effects display
    private GameContainer k_effectsList = null!;

    // Callbacks
    private Action? k_onExpandClicked;

    // Threshold constants
    private static readonly Percent DANGER_LOW = 20;
    private static readonly Percent DANGER_MODERATE = 40;
    private static readonly Percent DANGER_HIGH = 60;
    private static readonly Percent DANGER_EXTREME = 80;
    private static readonly Percent DANGER_WARNING = 30;
    private static readonly Percent DANGER_ERROR = 60;

    private static readonly Percent MOVEMENT_FULL = 100;
    private static readonly Percent MOVEMENT_REDUCED = 70;

    private static readonly Percent VISIBILITY_CLEAR = 90;
    private static readonly Percent VISIBILITY_GOOD = 70;
    private static readonly Percent VISIBILITY_REDUCED = 50;
    private static readonly Percent VISIBILITY_POOR = 30;

    private static readonly Percent FOG_EFFECT_THRESHOLD = 10;
    private static readonly Percent FOG_VISIBILITY_FACTOR = 50;

    // Slot sizes
    private float SLOT_SIZE_COMPACT = GameTheme.Current.Components.Slot.SizeS;
    private float SLOT_SIZE_EXPANDED = GameTheme.Current.Components.Slot.SizeM;

    #endregion

    #region Constructor

    public BiomeDisplayPanel(GameDb gameDb, ExpeditionManager expeditionManager) {
        k_gameDb = gameDb;
        k_expeditionManager = expeditionManager;

        BuildUI();
        ApplyTheme();

        GameTheme.OnThemeChanged += OnThemeChanged;
    }

    #endregion

    #region Fluent Configuration

    /// <summary>
    /// Sets whether the panel starts expanded.
    /// </summary>
    public BiomeDisplayPanel SetExpanded(bool expanded) {
        k_isExpanded = expanded;
        UpdateViewMode();
        return this;
    }

    /// <summary>
    /// Called when the expand button is clicked.
    /// </summary>
    public BiomeDisplayPanel OnExpandClicked(Action callback) {
        k_onExpandClicked = callback;
        return this;
    }

    /// <summary>
    /// Sets the panel width.
    /// </summary>
    public BiomeDisplayPanel SetWidth(float width) {
        style.width = width;
        k_root.SetMinWidth(width);
        return this;
    }

    /// <summary>
    /// Sets the panel width using Length.
    /// </summary>
    public BiomeDisplayPanel SetWidth(Length width) {
        style.width = width;
        return this;
    }

    /// <summary>
    /// Sets flex-grow on the root element.
    /// </summary>
    public BiomeDisplayPanel SetFlexGrow(bool grow = true) {
        style.flexGrow = grow ? 1f : 0f;
        return this;
    }

    /// <summary>
    /// Builds and returns the panel (fluent terminal).
    /// </summary>
    public BiomeDisplayPanel Build() {
        UpdateViewMode();
        RefreshUI();
        return this;
    }

    #endregion

    #region UI Building

    private void BuildUI() {
        var theme = GameTheme.Current;
        var spacing = theme.Spacing;
        var colors = theme.Colors;
        var borders = theme.Borders;
        var styles = theme.Components.BiomeDisplay;

        k_root = new GameContainer("biome-display-root")
            .SetColumn()
            .SetBackgroundColor(colors.BackgroundSecondary)
            .SetBorderRadius(borders.RadiusMD)
            .SetPadding(spacing.SM)
            .SetMinWidth(styles.CompactWidth)
            .Build();

        BuildSlotsRow();
        BuildDetailsSection();
        BuildStatsSection();
        BuildEffectsSection();
        BuildExpandButton();

        k_root
            .AddChild(k_slotsRow)
            .AddChild(k_biomeDetails)
            .AddChild(k_terrainDetails)
            .AddChild(k_statsSection)
            .AddChild(k_effectsSection)
            .AddChild(k_expandButtonContainer);

        Add(k_root);
    }

    private void BuildSlotsRow() {
        var spacing = GameTheme.Current.Spacing;

        k_slotsRow = new GameContainer("slots-row")
            .SetRow()
            .SetAlignItems(Align.Center)
            .SetJustifyContent(Justify.Center)
            .SetMarginBottom(spacing.XS)
            .Build();

        // Biome slot - no tooltips, no context menu, no drag/drop
        k_biomeSlot = new GameBiomeSlotDisplay()
            .SetSize(SLOT_SIZE_COMPACT)
            .SetInteractionOptions(
                allowTooltip: false,
                allowContextMenu: false,
                allowDrag: false,
                allowDrop: false)
            .SetInteractive(false)
            .Build();

        // Terrain slot - no tooltips, no context menu, no drag/drop
        k_terrainSlot = new GameTerrainSlotDisplay()
            .SetSize(SLOT_SIZE_COMPACT)
            .SetInteractionOptions(
                allowTooltip: false,
                allowContextMenu: false,
                allowDrag: false,
                allowDrop: false)
            .SetInteractive(false)
            .Build();

        // Spacer between slots
        var spacer = new GameContainer()
            .SetWidth(spacing.SM);

        k_slotsRow
            .AddChild(k_biomeSlot)
            .AddChild(spacer)
            .AddChild(k_terrainSlot);
    }

    private void BuildDetailsSection() {
        var spacing = GameTheme.Current.Spacing;
        var colors = GameTheme.Current.Colors;

        // Biome details (shown when expanded)
        k_biomeDetails = new GameContainer("biome-details")
            .SetColumn()
            .SetPaddingTop(spacing.XS)
            .SetMarginBottom(spacing.XS)
            .SetBorderTopOnly(1)
            .SetBorderTopColor(colors.SurfaceBorder)
            .SetVisible(false)
            .Build();

        // Terrain details (shown when expanded)
        k_terrainDetails = new GameContainer("terrain-details")
            .SetColumn()
            .SetPaddingTop(spacing.XS)
            .SetMarginBottom(spacing.XS)
            .SetBorderTopOnly(1)
            .SetBorderTopColor(colors.SurfaceBorder)
            .SetVisible(false)
            .Build();
    }

    private void BuildStatsSection() {
        var spacing = GameTheme.Current.Spacing;
        var colors = GameTheme.Current.Colors;

        k_statsSection = new GameContainer("stats-section")
            .SetColumn()
            .SetMarginBottom(spacing.XS)
            .SetPaddingTop(spacing.XS)
            .SetBorderTopOnly(1)
            .SetBorderTopColor(colors.SurfaceBorder)
            .Build();

        // Danger level
        var dangerRow = CreateStatRow("⚠", "Danger:", "Low");
        k_dangerLabel = dangerRow.Q<GameLabel>("value");
        k_statsSection.AddChild(dangerRow);

        // Movement modifier
        var movementRow = CreateStatRow("👟", "Movement:", "100%");
        k_movementLabel = movementRow.Q<GameLabel>("value");
        k_statsSection.AddChild(movementRow);

        // Visibility
        var visibilityRow = CreateStatRow("👁", "Visibility:", "Normal");
        k_visibilityLabel = visibilityRow.Q<GameLabel>("value");
        k_statsSection.AddChild(visibilityRow);
    }

    private GameContainer CreateStatRow(string icon, string label, string value) {
        var spacing = GameTheme.Current.Spacing;

        var row = new GameContainer()
            .SetRow()
            .SetSpaceBetween()
            .SetAlignItems(Align.Center)
            .SetMarginBottom(spacing.XXS)
            .Build();

        var leftSide = new GameContainer()
            .SetRow()
            .SetAlignItems(Align.Center)
            .Build();

        leftSide.Add(new GameLabel(icon)
            .SetStyle(LabelStyle.BodySmall)
            .Build());

        leftSide.Add(new GameLabel(label)
            .SetStyle(LabelStyle.Caption)
            .SetColor(LabelColor.Secondary)
            .SetMarginLeft(spacing.XXS)
            .Build());

        var valueLabel = new GameLabel(value)
            .SetStyle(LabelStyle.LabelSmall)
            .SetColor(LabelColor.Primary)
            .Build();
        valueLabel.name = "value";

        row.AddChild(leftSide);
        row.AddChild(valueLabel);

        return row;
    }

    private void BuildEffectsSection() {
        var spacing = GameTheme.Current.Spacing;
        var colors = GameTheme.Current.Colors;

        k_effectsSection = new GameContainer("effects-section")
            .SetColumn()
            .SetVisible(false)
            .SetPaddingTop(spacing.XS)
            .SetBorderTopOnly(1)
            .SetBorderTopColor(colors.SurfaceBorder)
            .Build();

        var effectsHeader = new GameLabel("Active Effects")
            .SetStyle(LabelStyle.Caption)
            .SetColor(LabelColor.Tertiary)
            .SetMarginBottom(spacing.XXS)
            .Build();

        k_effectsList = new GameContainer("effects-list")
            .SetColumn()
            .Build();

        k_effectsSection
            .AddChild(effectsHeader)
            .AddChild(k_effectsList);
    }

    private void BuildExpandButton() {
        var spacing = GameTheme.Current.Spacing;
		var biomeStyles = GameTheme.Current.Components.BiomeDisplay;

        k_expandButtonContainer = new GameContainer("expand-button-container")
            .SetRow()
            .SetJustifyContent(Justify.Center)
            .SetMarginTop(spacing.MD)
			.SetMinWidth(biomeStyles.ExpandedWidth)
            .Build();

        UpdateExpandButton();
    }

    private void UpdateExpandButton() {
        k_expandButtonContainer.ClearChildren();

        var expandIcon = k_isExpanded ? "▲" : "▼";
        var expandBtn = new GameButton(expandIcon)
            .SetVariant(ButtonVariant.Ghost)
            .SetSize(ButtonSize.Small)
            .OnClick(OnExpandButtonClicked)
            .Build();

        k_expandButtonContainer.AddChild(expandBtn);
    }

    private void OnExpandButtonClicked() {
        k_isExpanded = !k_isExpanded;
        UpdateViewMode();
        k_onExpandClicked?.Invoke();
    }

    #endregion

    #region View Mode Management

    private void UpdateViewMode() {
        var styles = GameTheme.Current.Components.BiomeDisplay;

        if (k_isExpanded) {
            ApplyExpandedStyles(styles);
        } else {
            ApplyCompactStyles(styles);
        }
        UpdateExpandButton();
    }

    private void ApplyCompactStyles(BiomeDisplayPanelStyles styles) {
        k_root.SetMinWidth(styles.CompactWidth);
		k_root.SetFlexShrink(1f);
        k_root.SetFlexGrow(0f);

		// Smaller slots
		k_biomeSlot.SetSize(SLOT_SIZE_COMPACT);
        k_terrainSlot.SetSize(SLOT_SIZE_COMPACT);

        // Hide detailed info
        k_biomeDetails.SetVisible(false);
        k_terrainDetails.SetVisible(false);
        k_effectsSection.SetVisible(false);

        // Show compact stats
        k_statsSection.SetVisible(true);
    }

    private void ApplyExpandedStyles(BiomeDisplayPanelStyles styles) {
        k_root.SetMinWidth(styles.ExpandedWidth);
        k_root.SetFlexShrink(0f);
        k_root.SetFlexGrow(1f);

		// Larger slots
		k_biomeSlot.SetSize(SLOT_SIZE_EXPANDED);
        k_terrainSlot.SetSize(SLOT_SIZE_EXPANDED);

        // Show detailed info
        k_biomeDetails.SetVisible(true);
        k_terrainDetails.SetVisible(true);
        k_effectsSection.SetVisible(true);
        k_statsSection.SetVisible(true);

        // Populate details
        PopulateBiomeDetails();
        PopulateTerrainDetails();
        PopulateEffectsSection();
    }

    #endregion

    #region UI Refresh

    /// <summary>
    /// Refreshes all displayed data from the expedition state.
    /// </summary>
    public void RefreshUI() {
        var travel = k_expeditionManager.Travel;
        if (travel == null) return;

        RefreshSlots(travel);
        RefreshStats(travel);

        if (k_isExpanded) {
            PopulateBiomeDetails();
            PopulateTerrainDetails();
            PopulateEffectsSection();
        }
    }

    private void RefreshSlots(ExpeditionManager.TravelData travel) {
        // Get and set biome
        BiomeProto? biome = null;
        if (travel.CurrentNode?.BiomeId.Value != null) {
            k_gameDb.TryGetProto(travel.CurrentNode.BiomeId, out biome);
        }
        k_biomeSlot.SetBiome(biome);

        // Get and set terrain
        TerrainProto? terrain = null;
        if (travel.CurrentTerrain.Value != null) {
            k_gameDb.TryGetProto(travel.CurrentTerrain, out terrain);
        }
        k_terrainSlot.SetTerrain(terrain);
    }

    private void RefreshStats(ExpeditionManager.TravelData travel) {
        var colors = GameTheme.Current.Colors;

        // Danger level from current node (0-1 fraction)
        Percent dangerLevel = (travel.CurrentNode?.DangerLevel ?? 0f).AsFractionPercent();

        string dangerText = dangerLevel switch {
            _ when dangerLevel < DANGER_LOW => "Safe",
            _ when dangerLevel < DANGER_MODERATE => "Low",
            _ when dangerLevel < DANGER_HIGH => "Moderate",
            _ when dangerLevel < DANGER_EXTREME => "High",
            _ => "Extreme"
        };
        k_dangerLabel.SetText(dangerText);
        k_dangerLabel.style.color = dangerLevel switch {
            _ when dangerLevel < DANGER_WARNING => colors.Success,
            _ when dangerLevel < DANGER_ERROR => colors.Warning,
            _ => colors.Error
        };

        // Movement modifier from terrain stats
        Percent movementMod = travel.CurrentTerrainStats.MovementMultiplier.AsFractionPercent();
        k_movementLabel.SetText(movementMod.ToString());
        k_movementLabel.style.color = movementMod switch {
            _ when movementMod >= MOVEMENT_FULL => colors.Success,
            _ when movementMod >= MOVEMENT_REDUCED => colors.TextPrimary,
            _ => colors.Warning
        };

        // Visibility from weather and fog
        var weather = k_expeditionManager.Weather;
        var fogRegion = k_expeditionManager.CurrentFogRegion;

        Percent visibility = weather.GetEffects().VisibilityModifier.AsFractionPercent();
        if (fogRegion != null) {
            Percent fogStrength = fogRegion.CurrentStrength.AsFractionPercent();
            Percent fogReduction = fogStrength * FOG_VISIBILITY_FACTOR;
            visibility = visibility * (Percent.Hundred - fogReduction);
        }

        string visibilityText = visibility switch {
            _ when visibility >= VISIBILITY_CLEAR => "Clear",
            _ when visibility >= VISIBILITY_GOOD => "Good",
            _ when visibility >= VISIBILITY_REDUCED => "Reduced",
            _ when visibility >= VISIBILITY_POOR => "Poor",
            _ => "Minimal"
        };
        k_visibilityLabel.SetText(visibilityText);
        k_visibilityLabel.style.color = visibility switch {
            _ when visibility >= VISIBILITY_GOOD => colors.Success,
            _ when visibility >= VISIBILITY_REDUCED => colors.TextPrimary,
            _ => colors.Warning
        };
    }

    private void PopulateBiomeDetails() {
        k_biomeDetails.ClearChildren();

        var biome = k_biomeSlot.BiomeProto;
        if (biome == null) {
            k_biomeDetails.Add(new GameLabel("No biome data")
                .SetStyle(LabelStyle.Caption)
                .SetColor(LabelColor.Tertiary)
                .Build());
            return;
        }

        var spacing = GameTheme.Current.Spacing;

        // Biome name header
        k_biomeDetails.Add(new GameLabel(biome.DisplayText.Name)
            .SetStyle(LabelStyle.TitleSmall)
            .SetColor(LabelColor.Primary)
            .SetMarginBottom(spacing.XXS)
            .Build());

        // Description if available
        if (!string.IsNullOrEmpty(biome.DisplayText.Description)) {
            k_biomeDetails.Add(new GameLabel(biome.DisplayText.Description)
                .SetStyle(LabelStyle.Caption)
                .SetColor(LabelColor.Secondary)
                .SetWhiteSpace(WhiteSpace.Normal)
                .SetMarginBottom(spacing.XS)
                .Build());
        }

        // Primary terrain
        AddDetailRow(k_biomeDetails, "Primary Terrain",
            biome.PrimaryTerrain.Value.Replace("Terrain_", ""), spacing);

        // Danger level
        string dangerText = biome.DangerLevel switch {
            <= 0.2f => "Safe",
            <= 0.4f => "Moderate",
            <= 0.6f => "Dangerous",
            <= 0.8f => "Very Dangerous",
            _ => "Deadly"
        };
        AddDetailRow(k_biomeDetails, "Danger", dangerText, spacing);

        // Segment length
        AddDetailRow(k_biomeDetails, "Length",
            $"{biome.MinLength}-{biome.MaxLength} nodes", spacing);
    }

    private void PopulateTerrainDetails() {
        k_terrainDetails.ClearChildren();

        var terrain = k_terrainSlot.TerrainProto;
        if (terrain == null) {
            k_terrainDetails.Add(new GameLabel("No terrain data")
                .SetStyle(LabelStyle.Caption)
                .SetColor(LabelColor.Tertiary)
                .Build());
            return;
        }

        var spacing = GameTheme.Current.Spacing;

        // Terrain name header
        k_terrainDetails.Add(new GameLabel(terrain.DisplayText.Name)
            .SetStyle(LabelStyle.TitleSmall)
            .SetColor(LabelColor.Primary)
            .SetMarginBottom(spacing.XXS)
            .Build());

        // Category
        k_terrainDetails.Add(new GameLabel($"{terrain.Category} Terrain")
            .SetStyle(LabelStyle.Caption)
            .SetColor(LabelColor.Tertiary)
            .SetMarginBottom(spacing.XS)
            .Build());

        // Movement speed
        string speedText = terrain.MovementSpeedMultiplier.Value switch {
            >= 1.5f => "Very Fast",
            >= 1.2f => "Fast",
            >= 0.9f => "Normal",
            >= 0.7f => "Slow",
            >= 0.5f => "Very Slow",
            _ => "Crawling"
        };
        AddDetailRow(k_terrainDetails, "Movement", speedText, spacing);

        // Stamina drain
        if (Math.Abs(terrain.StaminaDrainPerDistance - 1.0f) > 0.01f) {
            AddDetailRow(k_terrainDetails, "Stamina Drain",
                $"{terrain.StaminaDrainPerDistance:F1}x", spacing);
        }

        // Vision
        if (Math.Abs(terrain.VisionRangeMultiplier - 1.0f) > 0.01f) {
            AddDetailRow(k_terrainDetails, "Vision",
                $"{terrain.VisionRangeMultiplier:P0}", spacing);
        }

        // Shelter
        if (terrain.ProvidesNaturalShelter) {
            AddDetailRow(k_terrainDetails, "Shelter", "Available", spacing);
        }
    }

    private void AddDetailRow(GameContainer parent, string label, string value, SpacingSettings spacing) {
        var row = new GameContainer()
            .SetRow()
            .SetJustifyContent(Justify.SpaceBetween)
            .SetFullWidth()
            .SetMarginBottom(spacing.XXS)
            .Build();

        row.Add(new GameLabel(label)
            .SetStyle(LabelStyle.Caption)
            .SetColor(LabelColor.Secondary)
            .Build());

        row.Add(new GameLabel(value)
            .SetStyle(LabelStyle.Caption)
            .SetColor(LabelColor.Primary)
            .Build());

        parent.Add(row);
    }

    private void PopulateEffectsSection() {
        k_effectsList.ClearChildren();

        var colors = GameTheme.Current.Colors;

        // Weather effects
        var weather = k_expeditionManager.Weather;
        if (weather.CurrentWeather != WeatherType.Clear) {
            var effects = weather.GetEffects();
            AddEffectRow($"🌤 {weather.CurrentWeather}",
                GetWeatherEffectDescription(effects), colors.Info);
        }

        // Fog effects
        var fogRegion = k_expeditionManager.CurrentFogRegion;
        if (fogRegion != null) {
            Percent fogStrength = fogRegion.CurrentStrength.AsFractionPercent();
            if (fogStrength > FOG_EFFECT_THRESHOLD) {
                var fogEffects = fogRegion.GetCurrentEffects();
                if (fogEffects != null) {
                    Percent enemyBonus = fogEffects.Strength.AsFractionPercent();
                    AddEffectRow($"🌫 {fogRegion.CurrentTier.GetDisplayName()}",
                        $"Enemy +{enemyBonus}", colors.Warning);
                }
            }
        }

        // Terrain effects
        var travel = k_expeditionManager.Travel;
        if (travel != null) {
            var stats = travel.CurrentTerrainStats;
            Percent staminaDrain = stats.StaminaDrain.AsFractionPercent();
            Percent fatigueRate = stats.FatigueRate.AsFractionPercent();

            if (staminaDrain != Percent.Hundred) {
                Percent difference = staminaDrain > Percent.Hundred
                    ? staminaDrain - Percent.Hundred
                    : Percent.Hundred - staminaDrain;
                string drainText = staminaDrain > Percent.Hundred
                    ? $"+{difference} stamina drain"
                    : $"-{difference} stamina drain";
                var drainColor = staminaDrain > Percent.Hundred ? colors.Warning : colors.Success;
                AddEffectRow("💪 Terrain", drainText, drainColor);
            }

            if (fatigueRate != Percent.Hundred) {
                Percent difference = fatigueRate > Percent.Hundred
                    ? fatigueRate - Percent.Hundred
                    : Percent.Hundred - fatigueRate;
                string fatigueText = fatigueRate > Percent.Hundred
                    ? $"+{difference} fatigue"
                    : $"-{difference} fatigue";
                var fatigueColor = fatigueRate > Percent.Hundred ? colors.Warning : colors.Success;
                AddEffectRow("😴 Fatigue", fatigueText, fatigueColor);
            }
        }

        // Show "None" if no effects
        if (k_effectsList.childCount == 0) {
            k_effectsList.Add(new GameLabel("No active effects")
                .SetStyle(LabelStyle.Caption)
                .SetColor(LabelColor.Tertiary)
                .Build());
        }
    }

    private void AddEffectRow(string name, string effect, Color color) {
        var spacing = GameTheme.Current.Spacing;

        var row = new GameContainer()
            .SetRow()
            .SetSpaceBetween()
            .SetMarginBottom(spacing.XXS)
            .Build();

        row.Add(new GameLabel(name)
            .SetStyle(LabelStyle.Caption)
            .SetColor(LabelColor.Secondary)
            .Build());

        var effectLabel = new GameLabel(effect)
            .SetStyle(LabelStyle.Caption)
            .Build();
        effectLabel.style.color = color;
        row.Add(effectLabel);

        k_effectsList.AddChild(row);
    }

    private string GetWeatherEffectDescription(WeatherEffects effects) {
        var parts = new List<string>();

        Percent visibility = effects.VisibilityModifier.AsFractionPercent();
        Percent encounterRate = effects.EncounterRateModifier.AsFractionPercent();
        Percent movement = effects.MovementModifier.AsFractionPercent();

        if (visibility < Percent.Hundred) {
            Percent reduction = Percent.Hundred - visibility;
            parts.Add($"-{reduction} vision");
        }
        if (encounterRate != Percent.Hundred) {
            Percent difference = encounterRate > Percent.Hundred
                ? encounterRate - Percent.Hundred
                : Percent.Hundred - encounterRate;
            string prefix = encounterRate > Percent.Hundred ? "+" : "-";
            parts.Add($"{prefix}{difference} encounters");
        }
        if (movement < Percent.Hundred) {
            Percent reduction = Percent.Hundred - movement;
            parts.Add($"-{reduction} speed");
        }

        return parts.Count > 0 ? string.Join(", ", parts) : "No effects";
    }

    #endregion

    #region Theme

    private void ApplyTheme() {
        var theme = GameTheme.Current;
        var colors = theme.Colors;
        k_root.SetBackgroundColor(colors.BackgroundSecondary);
    }

    private void OnThemeChanged(GameTheme theme) {
        ApplyTheme();
        UpdateViewMode();
    }

    #endregion

    #region Cleanup

    public new void RemoveFromHierarchy() {
        GameTheme.OnThemeChanged -= OnThemeChanged;
        k_biomeSlot.RemoveFromHierarchy();
        k_terrainSlot.RemoveFromHierarchy();
        base.RemoveFromHierarchy();
    }

    #endregion
}
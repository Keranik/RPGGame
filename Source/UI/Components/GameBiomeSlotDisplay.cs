using RPGGame.Core;
using RPGGame.Core.Items;
using RPGGame.Core.Prototypes.Expedition;
using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Components;

/// <summary>
/// Slot display for biomes (BiomeProto).
/// Supports three-tier display:
/// - Compact: Biome color/icon with danger indicator
/// - Summary: Primary terrain, danger level, key info
/// - Detailed: Full biome info with terrain distribution, generation settings
/// </summary>
public class GameBiomeSlotDisplay : BaseGameSlotDisplay<GameBiomeSlotDisplay> {
    #region Fields

    private BiomeProto? k_biomeProto;
    private bool k_showDangerIndicator = true;
    private bool k_isExplored = true;

    private readonly GameContainer k_biomeColorOverlay;
    private readonly GameLabel k_dangerBadge;
    private readonly GameLabel k_orderBadge;

    #endregion

    #region Properties

    /// <summary>The biome prototype being displayed.</summary>
    public BiomeProto? BiomeProto => k_biomeProto;

    /// <inheritdoc/>
    public override bool IsEmpty => k_biomeProto == null;

    /// <summary>Whether the biome has been explored by the player.</summary>
    public bool IsExplored => k_isExplored;

    #endregion

    #region Constructor

    public GameBiomeSlotDisplay() : base() {
        var theme = GameTheme.Current;
        var colors = theme.Colors;
        var borders = theme.Borders;

        // Biome color overlay (background tint from ColorHint)
        k_biomeColorOverlay = new GameContainer("biome-color")
            .SetAbsoluteFill()
            .SetBorderRadius(borders.RadiusMD)
            .SetOpacity(0.3f)
            .SetPickingMode(PickingMode.Ignore);
        k_rootContainer.Insert(0, k_biomeColorOverlay);

        // Danger level badge (top-right)
        k_dangerBadge = new GameLabel()
            .SetStyle(LabelStyle.Caption)
            .SetTextAlign(TextAnchor.MiddleCenter)
            .SetPadding(0, 3)
            .Build();
        k_dangerBadge.style.position = Position.Absolute;
        k_dangerBadge.style.right = 2;
        k_dangerBadge.style.top = 2;
        k_dangerBadge.style.borderTopLeftRadius = borders.RadiusXS;
        k_dangerBadge.style.borderTopRightRadius = borders.RadiusXS;
        k_dangerBadge.style.borderBottomLeftRadius = borders.RadiusXS;
        k_dangerBadge.style.borderBottomRightRadius = borders.RadiusXS;
        k_dangerBadge.style.display = DisplayStyle.None;
        k_rootContainer.Add(k_dangerBadge);

        // Progression order badge (bottom-left)
        k_orderBadge = new GameLabel()
            .SetStyle(LabelStyle.Caption)
            .SetTextAlign(TextAnchor.MiddleCenter)
            .SetPadding(0, 2)
            .Build();
        k_orderBadge.style.position = Position.Absolute;
        k_orderBadge.style.left = 2;
        k_orderBadge.style.bottom = 2;
        k_orderBadge.style.backgroundColor = colors.BackgroundElevated;
        k_orderBadge.style.borderTopLeftRadius = borders.RadiusXS;
        k_orderBadge.style.borderTopRightRadius = borders.RadiusXS;
        k_orderBadge.style.borderBottomLeftRadius = borders.RadiusXS;
        k_orderBadge.style.borderBottomRightRadius = borders.RadiusXS;
        k_orderBadge.style.display = DisplayStyle.None;
        k_rootContainer.Add(k_orderBadge);

        SetSize(GameTheme.Current.Components.Slot.SizeM);
        SetShowDurability(false);
        SetShowStackCount(false);
    }

    #endregion

    #region Fluent API - Biome Data

    /// <summary>
    /// Sets the biome to display.
    /// </summary>
    public GameBiomeSlotDisplay SetBiome(BiomeProto? biome) {
        k_biomeProto = biome;

        if (biome != null) {
            // Set rarity based on danger level (0-1 float)
            RarityType rarity = biome.DangerLevel switch {
                <= 0.2f => RarityType.Common,
                <= 0.4f => RarityType.Uncommon,
                <= 0.6f => RarityType.Rare,
                <= 0.8f => RarityType.Epic,
                _ => RarityType.Legendary
            };
            SetRarity(rarity);
        }

        RefreshDisplay();
        return this;
    }

    /// <summary>
    /// Clears the biome from this slot.
    /// </summary>
    public GameBiomeSlotDisplay ClearBiome() => SetBiome(null);

    /// <summary>
    /// Sets whether to show the danger level indicator.
    /// </summary>
    public GameBiomeSlotDisplay SetShowDangerIndicator(bool show) {
        k_showDangerIndicator = show;
        RefreshDisplay();
        return this;
    }

    /// <summary>
    /// Sets whether the biome has been explored (affects display).
    /// </summary>
    public GameBiomeSlotDisplay SetExplored(bool explored) {
        k_isExplored = explored;
        RefreshDisplay();
        return this;
    }

    #endregion

    #region Abstract Implementation

    protected override Texture2D? GetIcon() => null;

    protected override string GetDisplayName() {
        if (!k_isExplored) return "???";
        return k_biomeProto?.DisplayText.Name ?? "Unknown Biome";
    }

    protected override string GetPlaceholderInitials() {
        if (!k_isExplored) return "?";
        return GetInitials(k_biomeProto?.DisplayText.Name);
    }

    protected override GameTooltip CreateBasicTooltip() {
        if (k_biomeProto == null || !k_isExplored) {
            return new GameTooltip()
                .SetContent("Unexplored Region")
                .SetPosition(TooltipPosition.Right)
                .SetDelay(0);
        }

        string text = $"{k_biomeProto.DisplayText.Name}\n" +
                      $"⚠️ Danger: {GetDangerText()}\n" +
                      $"🗺️ Primary: {k_biomeProto.PrimaryTerrain.Value.Replace("Terrain_", "")}";

        return new GameTooltip()
            .SetContent(text)
            .SetPosition(TooltipPosition.Right)
            .SetDelay(0);
    }

    protected override GameTooltip CreateExpandedTooltip() {
        var theme = GameTheme.Current;
        var spacing = theme.Spacing;
        var colors = theme.Colors;
        var borders = theme.Borders;

        var content = new GameContainer("biome-tooltip")
            .SetColumn()
            .SetPadding(spacing.SM)
            .SetWidth(300);

        if (k_biomeProto == null || !k_isExplored) {
            content.Add(new GameLabel()
                .SetText("Unexplored Region")
                .SetStyle(LabelStyle.BodyMedium)
                .SetColor(LabelColor.Tertiary)
                .Build());

            content.Add(new GameLabel()
                .SetText("Explore this area to reveal its secrets.")
                .SetStyle(LabelStyle.Caption)
                .SetColor(LabelColor.Secondary)
                .Build());
        } else {
            // Header with biome color
            var headerRow = new GameContainer()
                .SetRow()
                .SetAlignItems(Align.Center);

            ColorRPG biomeColor = ColorRPG.FromHex(k_biomeProto.ColorHint);
            var colorDot = new GameContainer()
                .SetSize(16, 16)
                .SetBorderRadius(8)
                .SetBackgroundColor(biomeColor);
            headerRow.Add(colorDot);

            var nameLabel = new GameLabel()
                .SetText(k_biomeProto.DisplayText.Name)
                .SetStyle(LabelStyle.TitleMedium)
                .SetColor(LabelColor.Primary)
                .SetMarginLeft(spacing.XS)
                .Build();
            headerRow.Add(nameLabel);

            content.Add(headerRow);

            // Description
            if (!string.IsNullOrEmpty(k_biomeProto.DisplayText.Description)) {
                var desc = new GameLabel()
                    .SetText(k_biomeProto.DisplayText.Description)
                    .SetStyle(LabelStyle.BodySmall)
                    .SetColor(LabelColor.Secondary)
                    .SetWhiteSpace(WhiteSpace.Normal)
                    .SetMargin(spacing.XS, 0, spacing.SM, 0)
                    .Build();
                content.Add(desc);
            }

            // Ambient description if available
            if (!string.IsNullOrEmpty(k_biomeProto.AmbientDescription)) {
                var ambientLabel = new GameLabel()
                    .SetText($"\"{k_biomeProto.AmbientDescription}\"")
                    .SetStyle(LabelStyle.Caption)
                    .SetColor(LabelColor.Tertiary)
                    .SetWhiteSpace(WhiteSpace.Normal)
                    .SetItalic()
                    .SetMarginBottom(spacing.SM)
                    .Build();
                content.Add(ambientLabel);
            }

            content.Add(new GameDivider().Build());

            // Stats
            AddStatRow(content, "⚠️ Danger Level", $"{k_biomeProto.DangerLevel:P0}", spacing);
            AddStatRow(content, "🗺️ Primary Terrain", k_biomeProto.PrimaryTerrain.Value.Replace("Terrain_", ""), spacing);
            AddStatRow(content, "📏 Segment Length", $"{k_biomeProto.MinLength}-{k_biomeProto.MaxLength} nodes", spacing);

            if (k_biomeProto.IsMainPathBiome) {
                AddStatRow(content, "📍 Progression", $"Order {k_biomeProto.ProgressionOrder}", spacing);
            } else {
                AddStatRow(content, "📍 Type", "Branch Only", spacing);
            }

            // Variant terrains
            if (k_biomeProto.VariantTerrains.Count > 0) {
                content.Add(new GameDivider().Build());

                content.Add(new GameLabel()
                    .SetText($"Variant Terrains ({k_biomeProto.VariantTerrainChance:P0} chance)")
                    .SetStyle(LabelStyle.Caption)
                    .SetColor(LabelColor.Secondary)
                    .Build());

                var terrainRow = new GameContainer()
                    .SetRow()
                    .SetFlexWrap(Wrap.Wrap);

                foreach (var variant in k_biomeProto.VariantTerrains.Take(5)) {
                    var chip = new GameContainer()
                        .SetBackgroundColor(colors.Surface)
                        .SetBorderRadius(borders.RadiusXS)
                        .SetPadding(0, 4)
                        .SetMargin(0, spacing.XS, spacing.XXS, 0);
                    chip.Add(new GameLabel()
                        .SetText($"{variant.Terrain.Value.Replace("Terrain_", "")} ({variant.Weight:F1})")
                        .SetStyle(LabelStyle.Caption)
                        .Build());
                    terrainRow.Add(chip);
                }

                content.Add(terrainRow);
            }

            // Generation multipliers (only show non-default values)
            bool hasModifiers = Math.Abs(k_biomeProto.BranchChanceMultiplier - 1.0f) > 0.01f ||
                               Math.Abs(k_biomeProto.EventDensityMultiplier - 1.0f) > 0.01f ||
                               Math.Abs(k_biomeProto.ResourceNodeMultiplier - 1.0f) > 0.01f ||
                               k_biomeProto.DungeonChanceBonus > 0;

            if (hasModifiers) {
                content.Add(new GameDivider().Build());

                content.Add(new GameLabel()
                    .SetText("Generation Modifiers")
                    .SetStyle(LabelStyle.Caption)
                    .SetColor(LabelColor.Secondary)
                    .Build());

                if (Math.Abs(k_biomeProto.BranchChanceMultiplier - 1.0f) > 0.01f) {
                    AddStatRow(content, "🌿 Branch Chance", $"{k_biomeProto.BranchChanceMultiplier:F1}x", spacing);
                }
                if (Math.Abs(k_biomeProto.EventDensityMultiplier - 1.0f) > 0.01f) {
                    AddStatRow(content, "⚡ Event Density", $"{k_biomeProto.EventDensityMultiplier:F1}x", spacing);
                }
                if (Math.Abs(k_biomeProto.ResourceNodeMultiplier - 1.0f) > 0.01f) {
                    AddStatRow(content, "💎 Resources", $"{k_biomeProto.ResourceNodeMultiplier:F1}x", spacing);
                }
                if (k_biomeProto.DungeonChanceBonus > 0) {
                    AddStatRow(content, "🏰 Dungeon Bonus", $"+{k_biomeProto.DungeonChanceBonus:P0}", spacing);
                }
            }

            // Tags
            if (k_biomeProto.Tags.Count > 0) {
                content.Add(new GameDivider().Build());

                var tagRow = new GameContainer()
                    .SetRow()
                    .SetFlexWrap(Wrap.Wrap);

                foreach (var tag in k_biomeProto.Tags.Take(6)) {
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
            .SetPosition(TooltipPosition.Right);
    }

    private void AddStatRow(GameContainer parentToAddTo, string label, string value, SpacingSettings spacing) {
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

        parentToAddTo.Add(row);
    }

    private string GetDangerText() {
        if (k_biomeProto == null) return "Unknown";
        return k_biomeProto.DangerLevel switch {
            <= 0.2f => "Safe",
            <= 0.4f => "Moderate",
            <= 0.6f => "Dangerous",
            <= 0.8f => "Very Dangerous",
            _ => "Deadly"
        };
    }

    public override ItemInstance? GetDraggedItem() => null;

    protected override void OnRightClickAction(Vector2 mousePosition) {
        if (k_biomeProto == null) return;

        var menu = GameContextMenu.Show(this, mousePosition)
            .AddHeader(k_isExplored ? k_biomeProto.DisplayText.Name : "Unknown Region");

        menu.AddItem("🔍 Inspect", () => {
            ShowDetailedInspector();
            k_onInspect?.Invoke();
        });

        menu.AddDivider();

        if (!k_isExplored) {
            menu.AddItem("🗺️ Scout Area", () => {
                GameToast.Show("Scouting area...", ToastType.Info);
            });
        } else {
            menu.AddItem($"⚠️ Danger: {GetDangerText()}", null, enabled: false);
            menu.AddItem($"🗺️ Terrain: {k_biomeProto.PrimaryTerrain.Value.Replace("Terrain_", "")}", null, enabled: false);

            if (k_biomeProto.CanAppearInBranches) {
                menu.AddItem("🌿 Can appear in branches", null, enabled: false);
            }
        }

        menu.AddDivider();
        menu.AddItem("Cancel", null);
    }

    protected override void OnRefreshComplete() {
        var colors = GameTheme.Current.Colors;

        if (k_biomeProto == null) {
            k_biomeColorOverlay.SetVisible(false);
            k_dangerBadge.SetVisible(false);
            k_orderBadge.SetVisible(false);
            return;
        }

        // Biome color overlay from ColorHint
        ColorRPG biomeColor = k_isExplored
            ? ColorRPG.FromHex(k_biomeProto.ColorHint)
            : colors.Surface;

        k_biomeColorOverlay
            .SetBackgroundColor(biomeColor)
            .SetOpacity(k_isExplored ? 0.3f : 0.5f)
            .SetVisible(true);

        // Danger badge
        if (k_showDangerIndicator && k_isExplored) {
            string dangerIcon = k_biomeProto.DangerLevel switch {
                <= 0.2f => "✓",
                <= 0.4f => "!",
                <= 0.6f => "!!",
                <= 0.8f => "!!!",
                _ => "☠"
            };

            ColorRPG dangerColor = k_biomeProto.DangerLevel switch {
                <= 0.2f => ColorRPG.HealthGreen,
                <= 0.4f => ColorRPG.StaminaYellow,
                <= 0.6f => ColorRPG.Fire,
                <= 0.8f => ColorRPG.Blood,
                _ => ColorRPG.DarkSoulsRed
            };

            k_dangerBadge.SetText(dangerIcon);
            k_dangerBadge.style.backgroundColor = dangerColor;
            k_dangerBadge.style.color = dangerColor.ContrastingTextColor();
            k_dangerBadge.SetVisible(true);
        } else {
            k_dangerBadge.SetVisible(false);
        }

        // Order badge (shows progression order if main path biome)
        if (k_isExplored && k_biomeProto.IsMainPathBiome) {
            k_orderBadge.SetText($"#{k_biomeProto.ProgressionOrder}");
            k_orderBadge.SetVisible(true);
        } else {
            k_orderBadge.SetVisible(false);
        }

        // Placeholder color
        k_placeholderLabel.style.color = k_isExplored
            ? ColorRPG.FromHex(k_biomeProto.ColorHint)
            : colors.TextTertiary;
    }

    #endregion
}
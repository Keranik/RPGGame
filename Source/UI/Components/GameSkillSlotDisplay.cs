using RPGGame.Core;
using RPGGame.Core.Items;
using RPGGame.Core.Prototypes;
using RPGGame.Core.Prototypes.Skills;
using RPGGame.Core.Prototypes.Stats;
using RPGGame.Core.Stats;
using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Components;

/// <summary>
/// Slot display for skills (active and passive).
/// </summary>
public class GameSkillSlotDisplay : BaseGameSlotDisplay<GameSkillSlotDisplay> {
    #region Fields

    private SkillProto? k_skillProto;
    private int k_currentRank;
    private string? k_hotkey;
    private int k_currentCooldownTurns;
    private int k_maxCooldownTurns;

    private readonly GameLabel k_costBadge;
    private readonly GameLabel k_rankLabel;

    #endregion

    #region Properties

    public SkillProto? SkillProto => k_skillProto;
    public int CurrentRank => k_currentRank;
    public override bool IsEmpty => k_skillProto == null;
    public bool IsOnCooldown => k_currentCooldownTurns > 0;
    public bool IsPassive => k_skillProto?.IsPassive ?? true;

    #endregion

    #region Constructor

    public GameSkillSlotDisplay() : base() {
        var theme = GameTheme.Current;
        var colors = theme.Colors;
        var borders = theme.Borders;

        k_costBadge = new GameLabel()
            .SetStyle(LabelStyle.Caption)
            .SetTextAlign(TextAnchor.MiddleCenter)
            .Build();
        k_costBadge.style.position = Position.Absolute;
        k_costBadge.style.left = 2;
        k_costBadge.style.bottom = 2;
        k_costBadge.style.backgroundColor = colors.ManaBackground;
        k_costBadge.style.color = colors.Mana;
        k_costBadge.style.paddingLeft = 2;
        k_costBadge.style.paddingRight = 2;
        k_costBadge.style.borderTopLeftRadius = borders.RadiusXS;
        k_costBadge.style.borderTopRightRadius = borders.RadiusXS;
        k_costBadge.style.borderBottomLeftRadius = borders.RadiusXS;
        k_costBadge.style.borderBottomRightRadius = borders.RadiusXS;
        k_costBadge.style.display = DisplayStyle.None;
        k_rootContainer.Add(k_costBadge);

        k_rankLabel = new GameLabel()
            .SetStyle(LabelStyle.Caption)
            .SetTextAlign(TextAnchor.MiddleCenter)
            .Build();
        k_rankLabel.style.position = Position.Absolute;
        k_rankLabel.style.left = 2;
        k_rankLabel.style.top = 2;
        k_rankLabel.style.backgroundColor = colors.BackgroundElevated;
        k_rankLabel.style.paddingLeft = 2;
        k_rankLabel.style.paddingRight = 2;
        k_rankLabel.style.borderTopLeftRadius = borders.RadiusXS;
        k_rankLabel.style.borderTopRightRadius = borders.RadiusXS;
        k_rankLabel.style.borderBottomLeftRadius = borders.RadiusXS;
        k_rankLabel.style.borderBottomRightRadius = borders.RadiusXS;
        k_rankLabel.style.display = DisplayStyle.None;
        k_rootContainer.Add(k_rankLabel);
    }

    #endregion

    #region Fluent API - Skill Data

    public GameSkillSlotDisplay SetSkill(SkillProto? skill, int currentRank = 1) {
        k_skillProto = skill;
        k_currentRank = Math.Max(0, currentRank);

        if (skill != null) {
            SetRarity(skill.IsPassive ? RarityType.Common : RarityType.Uncommon);
            k_maxCooldownTurns = skill.Cooldown;
        }

        RefreshDisplay();
        return this;
    }

    public GameSkillSlotDisplay SetHotkey(string? hotkey) {
        k_hotkey = hotkey;
        SetBadge(hotkey);
        return this;
    }

    public GameSkillSlotDisplay SetCooldownTurns(int remainingTurns) {
        k_currentCooldownTurns = Math.Max(0, remainingTurns);

        if (k_skillProto != null && k_skillProto.Cooldown > 0) {
            float progress = (float)k_currentCooldownTurns / k_skillProto.Cooldown;
            SetCooldown(progress, remainingTurns.Turns());
        } else {
            ClearCooldown();
        }

        return this;
    }

    public GameSkillSlotDisplay ClearSkill() {
        return SetSkill(null);
    }

    #endregion

    #region Abstract Implementation

	protected override void OnRightClickAction(Vector2 mousePosition) {
		if (k_skillProto == null) return;

		var menu = GameContextMenu.Show(this, mousePosition)
			.AddHeader(k_skillProto.DisplayText.Name);

		// Inspect
		menu.AddItem("🔍 Inspect", () => {
			ShowDetailedInspector();
			k_onInspect?.Invoke();
		});

		menu.AddDivider();

		// Skill-specific options
		string typeLabel = k_skillProto.IsPassive ? "Passive" : "Active";
		menu.AddItem($"📊 {typeLabel} Skill", null, enabled: false);

		if (k_currentRank > 0 && k_skillProto.MaxRank > 1) {
			menu.AddItem($"⬆️ Rank {k_currentRank}/{k_skillProto.MaxRank}", null, enabled: false);
		}

		if (IsOnCooldown) {
			menu.AddItem($"⏱️ Cooldown: {k_currentCooldownTurns}t", null, enabled: false);
		}

		menu.AddDivider();
		menu.AddItem("Cancel", null);
	}

    protected override Texture2D? GetIcon() => null;

    protected override string GetDisplayName() {
        return k_skillProto?.DisplayText.Name ?? "Empty";
    }

    protected override string GetPlaceholderInitials() {
        return GetInitials(k_skillProto?.DisplayText.Name);
    }

    protected override GameTooltip CreateBasicTooltip() {
    var theme = GameTheme.Current;
    var spacing = theme.Spacing;
    var colors = theme.Colors;

    var content = new GameContainer("skill-summary-tooltip")
        .SetColumn()
        .SetPadding(spacing.SM)
        .SetWidth(240);

    if (k_skillProto == null) {
        content.Add(new GameLabel().SetText("Empty Slot").SetStyle(LabelStyle.BodyMedium).SetColor(LabelColor.Tertiary).Build());
        return new GameTooltip().SetContent(content).SetPosition(TooltipPosition.Right).SetDelay(0);
    }

    // Header: Name
    content.Add(new GameLabel()
        .SetText(k_skillProto.DisplayText.Name)
        .SetStyle(LabelStyle.TitleSmall)
        .SetColor(LabelColor.Primary)
        .Build());

    // Type line: Passive/Active + Rank info
    string typeText = k_skillProto.IsPassive ? "Passive" : "Active";
    if (k_skillProto.MaxRank > 1) {
        string rankText = k_currentRank > 0 ? $"{k_currentRank}/{k_skillProto.MaxRank}" : $"Max {k_skillProto.MaxRank}";
        typeText += $" • {rankText}";
    }
    var typeLabel = new GameLabel()
        .SetText(typeText)
        .SetStyle(LabelStyle.Caption)
        .SetColor(LabelColor.Secondary)
        .Build();
    typeLabel.style.marginBottom = spacing.XS;
    content.Add(typeLabel);

    // Resource costs (for active skills)
    if (!k_skillProto.IsPassive) {
        var costParts = new List<string>();
        if (k_skillProto.ManaCost > 0) costParts.Add($"🔵 {k_skillProto.ManaCost} Mana");
        if (k_skillProto.StaminaCost > 0) costParts.Add($"🟡 {k_skillProto.StaminaCost} Stamina");
        if (k_skillProto.Cooldown > 0) costParts.Add($"⏱️ {k_skillProto.Cooldown}t CD");

        if (costParts.Count > 0) {
            var costLabel = new GameLabel()
                .SetText(string.Join("  ", costParts))
                .SetStyle(LabelStyle.Caption)
                .SetColor(LabelColor.Tertiary)
                .Build();
            costLabel.style.marginBottom = spacing.XS;
            content.Add(costLabel);
        }
    }

    // Stat modifiers (the key info for skill selection!)
    if (k_skillProto.ModifiersPerRank.Count > 0) {
        var divider = new GameDivider().Build();
        divider.style.marginTop = spacing.XS;
        divider.style.marginBottom = spacing.XS;
        content.Add(divider);

        int displayRank = k_currentRank > 0 ? k_currentRank : 1;
        var modifiers = k_skillProto.GetModifiersAtRank(displayRank);

        int shown = 0;
        foreach (var (statId, mod) in modifiers) {
            if (shown >= 4) {
                content.Add(new GameLabel()
                    .SetText($"...+{modifiers.Count - 4} more")
                    .SetStyle(LabelStyle.Caption)
                    .SetColor(LabelColor.Tertiary)
                    .Build());
                break;
            }

            string statName = GetStatDisplayName(statId);
            string modText = FormatModifier(mod);

            var modRow = new GameContainer()
                .SetRow()
                .SetJustifyContent(Justify.SpaceBetween)
                .SetFullWidth();
            modRow.Add(new GameLabel().SetText(statName).SetStyle(LabelStyle.Caption).SetColor(LabelColor.Secondary).Build());
            modRow.Add(new GameLabel().SetText(modText).SetStyle(LabelStyle.Caption).SetColor(LabelColor.Success).Build());
            content.Add(modRow);
            shown++;
        }

        if (k_skillProto.MaxRank > 1 && k_currentRank == 0) {
            var perRankNote = new GameLabel()
                .SetText("(per rank)")
                .SetStyle(LabelStyle.Caption)
                .SetColor(LabelColor.Tertiary)
                .Build();
            perRankNote.style.alignSelf = Align.FlexEnd;
            content.Add(perRankNote);
        }
    }

    // Active effects
    if (k_skillProto.ActiveEffects.Count > 0) {
        var effectsLabel = new GameLabel()
            .SetText($"✨ Applies {k_skillProto.ActiveEffects.Count} effect(s)")
            .SetStyle(LabelStyle.Caption)
            .SetColor(LabelColor.Warning)
            .Build();
        effectsLabel.style.marginTop = spacing.XS;
        content.Add(effectsLabel);
    }

    return new GameTooltip().SetContent(content).SetPosition(TooltipPosition.Right).SetDelay(0);
}

private string GetStatDisplayName(Proto.ID statId) {
    // Try to get display name from GameDb
    if (k_gameDb.TryGetProto<StatProto>(new StatProto.ID(statId.Value), out var statProto)) {
        return statProto.DisplayText.Name;
    }
    // Fallback: use ToDisplayName from StringExtensions
    return statId.Value.ToDisplayName();
}

private static string FormatModifier(ValueModifier mod) {
    string sign = mod.Value >= 0 ? "+" : "";
    return mod.Operation switch {
        ModifierOperation.FlatAdd => $"{sign}{mod.Value:0}",
        ModifierOperation.FlatSubtract => $"-{Math.Abs(mod.Value):0}",
        ModifierOperation.PercentIncrease => $"{sign}{mod.Value:0}%",
        ModifierOperation.PercentReduce => $"-{Math.Abs(mod.Value):0}%",
        ModifierOperation.PercentMore => $"{sign}{mod.Value:0}% more",
        ModifierOperation.PercentLess => $"-{Math.Abs(mod.Value):0}% less",
        _ => $"{sign}{mod.Value:0}"
    };
}





    protected override GameTooltip CreateExpandedTooltip() {
        var theme = GameTheme.Current;
        var spacing = theme.Spacing;

        var content = new GameContainer("skill-tooltip")
            .SetColumn()
            .SetPadding(spacing.SM)
            .SetWidth(250);

        if (k_skillProto == null) {
            content.Add(new GameLabel().SetText("Empty Slot").SetStyle(LabelStyle.BodyMedium).SetColor(LabelColor.Tertiary).Build());
        } else {
            content.Add(new GameLabel().SetText(k_skillProto.DisplayText.Name).SetStyle(LabelStyle.TitleMedium).SetColor(LabelColor.Primary).Build());

            var typeLabel = new GameLabel()
                .SetText(k_skillProto.IsPassive ? "Passive Skill" : "Active Skill")
                .SetStyle(LabelStyle.Caption)
                .SetColor(LabelColor.Secondary)
                .Build();
            typeLabel.style.marginBottom = spacing.SM;
            content.Add(typeLabel);

            if (!string.IsNullOrEmpty(k_skillProto.DisplayText.Description)) {
                var desc = new GameLabel()
                    .SetText(k_skillProto.DisplayText.Description)
                    .SetStyle(LabelStyle.BodySmall)
                    .SetColor(LabelColor.Secondary)
                    .Build();
                desc.style.whiteSpace = WhiteSpace.Normal;
                desc.style.marginBottom = spacing.SM;
                content.Add(desc);
            }

            content.Add(new GameDivider().Build());

            if (k_currentRank > 0) {
                AddStatRow(content, "Rank", $"{k_currentRank}/{k_skillProto.MaxRank}", spacing);
            }

            if (!k_skillProto.IsPassive) {
                if (k_skillProto.ManaCost > 0) AddStatRow(content, "Mana Cost", k_skillProto.ManaCost.ToString(), spacing);
                if (k_skillProto.StaminaCost > 0) AddStatRow(content, "Stamina Cost", k_skillProto.StaminaCost.ToString(), spacing);
                if (k_skillProto.Cooldown > 0) AddStatRow(content, "Cooldown", $"{k_skillProto.Cooldown} turns", spacing);
            }

            if (k_skillProto.RequiredLevel > 1) {
                AddStatRow(content, "Required Level", k_skillProto.RequiredLevel.ToString(), spacing);
            }
        }

        return new GameTooltip().SetContent(content).SetPosition(TooltipPosition.Right);
    }

    private void AddStatRow(GameContainer parent, string label, string value, SpacingSettings spacing) {
        var row = new GameContainer().SetRow().SetJustifyContent(Justify.SpaceBetween).SetFullWidth();
        row.Add(new GameLabel().SetText(label).SetStyle(LabelStyle.Caption).SetColor(LabelColor.Secondary).Build());
        row.Add(new GameLabel().SetText(value).SetStyle(LabelStyle.Caption).SetColor(LabelColor.Primary).Build());
        parent.Add(row);
    }

    public override ItemInstance? GetDraggedItem() => null;

    protected override void OnRefreshComplete() {
        if (k_skillProto == null) {
            k_costBadge.style.display = DisplayStyle.None;
            k_rankLabel.style.display = DisplayStyle.None;
            return;
        }

        if (!k_skillProto.IsPassive) {
            int cost = k_skillProto.ManaCost > 0 ? k_skillProto.ManaCost : k_skillProto.StaminaCost;
            if (cost > 0) {
                k_costBadge.SetText(cost.ToString());
                k_costBadge.style.display = DisplayStyle.Flex;

                var colors = GameTheme.Current.Colors;
                if (k_skillProto.ManaCost > 0) {
                    k_costBadge.style.backgroundColor = colors.ManaBackground;
                    k_costBadge.style.color = colors.Mana;
                } else {
                    k_costBadge.style.backgroundColor = colors.StaminaBackground;
                    k_costBadge.style.color = colors.Stamina;
                }
            } else {
                k_costBadge.style.display = DisplayStyle.None;
            }
        } else {
            k_costBadge.style.display = DisplayStyle.None;
        }

        if (k_currentRank > 0 && k_skillProto.MaxRank > 1) {
            k_rankLabel.SetText($"{k_currentRank}");
            k_rankLabel.style.display = DisplayStyle.Flex;
        } else {
            k_rankLabel.style.display = DisplayStyle.None;
        }
    }

    #endregion
}
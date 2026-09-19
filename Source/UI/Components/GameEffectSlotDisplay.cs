using RPGGame.Core;
using RPGGame.Core.Combat;
using RPGGame.Core.Effects;
using RPGGame.Core.Items;
using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Components;

/// <summary>
/// Slot display for buffs, debuffs, and status effects.
/// </summary>
public class GameEffectSlotDisplay : BaseGameSlotDisplay<GameEffectSlotDisplay> {
    #region Fields

    private EffectProto? k_effectProto;
    private StatusEffect? k_statusEffect;
    private int k_stacks = 1;
    private Duration k_remainingTurns;
    private string? k_sourceName;

    private readonly GameLabel k_durationLabel;
    private readonly GameContainer k_effectTypeBorder;

    #endregion

    #region Properties

    public EffectProto? EffectProto => k_effectProto;
    public StatusEffect? StatusEffect => k_statusEffect;
    public override bool IsEmpty => k_effectProto == null;
    public bool IsBuff => k_effectProto?.IsBuff ?? false;
    public bool IsPermanent => k_remainingTurns < 0;

    #endregion

    #region Constructor

    public GameEffectSlotDisplay() : base() {
        var theme = GameTheme.Current;
        var borders = theme.Borders;

        k_effectTypeBorder = new GameContainer("effect-type-border")
            .SetAbsoluteFill()
            .SetBorderWidth(2)
            .SetBorderRadius(borders.RadiusMD);
        k_effectTypeBorder.pickingMode = PickingMode.Ignore;
        k_rootContainer.Insert(0, k_effectTypeBorder);

        k_durationLabel = new GameLabel()
            .SetStyle(LabelStyle.TitleSmall)
            .SetTextAlign(TextAnchor.MiddleCenter)
            .Build();
        k_durationLabel.style.position = Position.Absolute;
        k_durationLabel.style.left = 0;
        k_durationLabel.style.right = 0;
        k_durationLabel.style.bottom = 0;
        k_durationLabel.style.backgroundColor = new Color(0, 0, 0, 0.7f);
        k_durationLabel.style.color = Color.white;
        k_durationLabel.style.fontSize = 10;
        k_durationLabel.style.display = DisplayStyle.None;
        k_rootContainer.Add(k_durationLabel);

        SetSize(GameTheme.Current.Components.Slot.SizeXS);
    }

    #endregion

    #region Fluent API - Effect Data

    public GameEffectSlotDisplay SetEffect(EffectProto? effect, int stacks = 1, Duration? remainingTurns = null) {
        k_effectProto = effect;
        k_stacks = Math.Max(1, stacks);
		k_remainingTurns = remainingTurns ?? Duration.Infinite;

        if (effect != null) {
            SetRarity(effect.IsBuff ? RarityType.Uncommon : RarityType.Common);
        }

        RefreshDisplay();
        return this;
    }

    public GameEffectSlotDisplay SetStatusEffect(StatusEffect? effect, EffectProto? proto) {
        k_statusEffect = effect;
        k_effectProto = proto;

        if (effect != null) {
            k_stacks = (int)effect.Intensity;
            k_remainingTurns = effect.RemainingDuration;
        }

        RefreshDisplay();
        return this;
    }

    public GameEffectSlotDisplay SetRemainingTurns(int turns) {
        k_remainingTurns = turns.Turns();
        RefreshDisplay();
        return this;
    }

    public GameEffectSlotDisplay SetSource(string? sourceName) {
        k_sourceName = sourceName;
        return this;
    }

    public GameEffectSlotDisplay ClearEffect() {
        k_effectProto = null;
        k_statusEffect = null;
        k_stacks = 1;
        k_remainingTurns = 0.Turns();
        RefreshDisplay();
        return this;
    }

    #endregion

    #region Abstract Implementation

	protected override void OnRightClickAction(Vector2 mousePosition) {
		if (k_effectProto == null) return;

		var theme = GameTheme.Current;
		var colors = theme.Colors;

		var menu = GameContextMenu.Show(this, mousePosition)
			.AddHeader(k_effectProto.DisplayText.Name);

		// Inspect
		menu.AddItem("🔍 Inspect", () => {
			ShowDetailedInspector();
			k_onInspect?.Invoke();
		});

		menu.AddDivider();

		// Effect info
		string typeIcon = k_effectProto.IsBuff ? "✨" : "💀";
		string typeLabel = k_effectProto.IsBuff ? "Buff" : "Debuff";
		menu.AddItem($"{typeIcon} {typeLabel}", null, enabled: false);

		if (k_stacks > 1) {
			menu.AddItem($"📚 Stacks: {k_stacks}/{k_effectProto.MaxStacks}", null, enabled: false);
		}

		if (k_remainingTurns.IsInfinite) {
			menu.AddItem("∞ Permanent", null, enabled: false);
		} else if (k_remainingTurns > Duration.Zero) {
			menu.AddItem($"⏱️ {k_remainingTurns.ToDisplayCompact()} remaining", null, enabled: false);
		}

		// Dispel option if applicable
		if (k_effectProto.CanBeDispelled) {
			menu.AddDivider();
			menu.AddItem("🧹 Dispel", () => {
				GameToast.Show($"Dispelled {k_effectProto.DisplayText.Name}", ToastType.Info);
			});
		}

		menu.AddDivider();
		menu.AddItem("Cancel", null);
	}

    protected override Texture2D? GetIcon() => null;

    protected override string GetDisplayName() {
        return k_effectProto?.DisplayText.Name ?? "Unknown Effect";
    }

    protected override string GetPlaceholderInitials() {
        return GetInitials(k_effectProto?.DisplayText.Name);
    }

	protected override GameTooltip CreateBasicTooltip() {
		string text = k_effectProto?.DisplayText.Name ?? "Unknown";
		if (k_stacks > 1) text += $" x{k_stacks}";
    
		if (k_remainingTurns.IsInfinite) {
			text += " (Permanent)";
		} else if (k_remainingTurns > Duration.Zero) {
			text += $" ({k_remainingTurns.ToDisplayCompact()})";
		}

		return new GameTooltip()
			.SetContent(text)
			.SetPosition(TooltipPosition.Top)
			.SetDelay(0);
	}

    protected override GameTooltip CreateExpandedTooltip() {
        var theme = GameTheme.Current;
        var spacing = theme.Spacing;

        var content = new GameContainer("effect-tooltip")
            .SetColumn()
            .SetPadding(spacing.SM)
            .SetWidth(220);

        if (k_effectProto == null) {
            content.Add(new GameLabel()
                .SetText("Unknown Effect")
                .SetStyle(LabelStyle.BodyMedium)
                .SetColor(LabelColor.Tertiary)
                .Build());
        } else {
            var headerRow = new GameContainer()
                .SetRow()
                .SetAlignItems(Align.Center);

            string icon = k_effectProto.IsBuff ? "✨" : "💀";
            headerRow.Add(new GameLabel().SetText(icon).SetStyle(LabelStyle.TitleSmall).Build());

            var nameLabel = new GameLabel()
                .SetText(k_effectProto.DisplayText.Name)
                .SetStyle(LabelStyle.TitleSmall)
                .SetColor(k_effectProto.IsBuff ? LabelColor.Success : LabelColor.Error)
                .Build();
            nameLabel.style.marginLeft = spacing.XS;
            headerRow.Add(nameLabel);

            content.Add(headerRow);

            string typeText = k_effectProto.IsBuff ? "Buff" : "Debuff";
            if (k_effectProto.IsDoT) typeText += " • DoT";
            if (k_effectProto.IsHoT) typeText += " • HoT";
            if (!k_effectProto.CanBeDispelled) typeText += " • Cannot be dispelled";

            var typeLabel = new GameLabel()
                .SetText(typeText)
                .SetStyle(LabelStyle.Caption)
                .SetColor(LabelColor.Tertiary)
                .Build();
            typeLabel.style.marginBottom = spacing.SM;
            content.Add(typeLabel);

            if (!string.IsNullOrEmpty(k_effectProto.DisplayText.Description)) {
                var desc = new GameLabel()
                    .SetText(k_effectProto.DisplayText.Description)
                    .SetStyle(LabelStyle.BodySmall)
                    .SetColor(LabelColor.Secondary)
                    .Build();
                desc.style.whiteSpace = WhiteSpace.Normal;
                desc.style.marginBottom = spacing.SM;
                content.Add(desc);
            }

            content.Add(new GameDivider().Build());

			if (k_remainingTurns.IsInfinite) {
				AddStatRow(content, "Duration", "Permanent", spacing);
			} else if (k_remainingTurns > Duration.Zero) {
				AddStatRow(content, "Remaining", k_remainingTurns.ToDisplayCompact(), spacing);
			}

            if (k_stacks > 1 || k_effectProto.MaxStacks > 1) {
                AddStatRow(content, "Stacks", $"{k_stacks}/{k_effectProto.MaxStacks}", spacing);
            }

            if (k_effectProto.DamagePerTurn.HasValue) {
                AddStatRow(content, "Damage/Turn", $"{k_effectProto.DamagePerTurn.Value} {k_effectProto.DamageType}", spacing);
            }
            if (k_effectProto.HealingPerTurn.HasValue) {
                AddStatRow(content, "Healing/Turn", k_effectProto.HealingPerTurn.Value.ToString(), spacing);
            }

            if (!string.IsNullOrEmpty(k_sourceName)) {
                var sourceLabel = new GameLabel()
                    .SetText($"Source: {k_sourceName}")
                    .SetStyle(LabelStyle.Caption)
                    .SetColor(LabelColor.Tertiary)
                    .Build();
                sourceLabel.style.marginTop = spacing.XS;
                content.Add(sourceLabel);
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

        row.Add(new GameLabel().SetText(label).SetStyle(LabelStyle.Caption).SetColor(LabelColor.Secondary).Build());
        row.Add(new GameLabel().SetText(value).SetStyle(LabelStyle.Caption).SetColor(LabelColor.Primary).Build());

        parent.Add(row);
    }

    public override ItemInstance? GetDraggedItem() => null;

	protected override void OnRefreshComplete() {
		var colors = GameTheme.Current.Colors;

		if (k_effectProto == null) {
			k_effectTypeBorder.SetVisible(false);
			k_durationLabel.SetVisible(false);
			return;
		}

		Color borderColor = k_effectProto.IsBuff ? colors.Success : colors.Error;
		k_effectTypeBorder.SetBorderColor(borderColor);
		k_effectTypeBorder.SetVisible(true);

		if (k_remainingTurns.IsInfinite) {
			k_durationLabel.SetText("∞");
			k_durationLabel.SetVisible(true);
		} else if (k_remainingTurns > Duration.Zero) {
			k_durationLabel.SetText(k_remainingTurns.InTurns.ToString());
			k_durationLabel.SetVisible(true);
		} else {
			k_durationLabel.SetVisible(false);
		}

		SetStackCount(k_stacks);
		k_placeholderLabel.style.color = k_effectProto.IsBuff ? colors.Success : colors.Error;
	}

    #endregion
}
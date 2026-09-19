using RPGGame.Core;
using RPGGame.Core.Characters;
using RPGGame.Core.Combat;
using RPGGame.Core.Items;
using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Components;

/// <summary>
/// Slot display for characters (party members, enemies, summons).
/// </summary>
public class GameCharacterSlotDisplay : BaseGameSlotDisplay<GameCharacterSlotDisplay> {
    #region Fields

    private LiveCharacter? k_character;
    private bool k_showManaBar = true;
    private bool k_showStatusEffects = true;
    private int k_maxVisibleEffects = 4;

    private readonly GameStatBar k_healthBar;
    private readonly GameStatBar k_manaBar;
    private readonly GameLabel k_levelBadge;
    private readonly GameLabel k_typeIndicator;
    private readonly GameContainer k_statusEffectsRow;
    private readonly GameContainer k_deadOverlay;
    private readonly GameLabel k_deadIcon;

    #endregion

    #region Properties

    public LiveCharacter? Character => k_character;
    public override bool IsEmpty => k_character == null;
    public bool IsAlive => k_character?.IsAlive ?? false;
    public bool IsPlayerSide => k_character?.IsPlayerSide ?? false;
    public bool IsEnemy => k_character?.IsEnemySide ?? false;

    #endregion

    #region Constructor

    public GameCharacterSlotDisplay() : base() {
        var theme = GameTheme.Current;
        var colors = theme.Colors;
        var borders = theme.Borders;

        k_levelBadge = new GameLabel()
            .SetStyle(LabelStyle.Caption)
            .SetTextAlign(TextAnchor.MiddleCenter)
            .Build();
        k_levelBadge.style.position = Position.Absolute;
        k_levelBadge.style.left = 2;
        k_levelBadge.style.top = 2;
        k_levelBadge.style.backgroundColor = colors.BackgroundElevated;
        k_levelBadge.style.paddingLeft = 3;
        k_levelBadge.style.paddingRight = 3;
        k_levelBadge.style.paddingTop = 1;
        k_levelBadge.style.paddingBottom = 1;
        k_levelBadge.style.borderTopLeftRadius = borders.RadiusXS;
        k_levelBadge.style.borderTopRightRadius = borders.RadiusXS;
        k_levelBadge.style.borderBottomLeftRadius = borders.RadiusXS;
        k_levelBadge.style.borderBottomRightRadius = borders.RadiusXS;
        k_levelBadge.style.display = DisplayStyle.None;
        k_rootContainer.Add(k_levelBadge);

        k_typeIndicator = new GameLabel()
            .SetStyle(LabelStyle.Caption)
            .SetTextAlign(TextAnchor.MiddleCenter)
            .Build();
        k_typeIndicator.style.position = Position.Absolute;
        k_typeIndicator.style.right = 2;
        k_typeIndicator.style.top = 2;
        k_typeIndicator.style.display = DisplayStyle.None;
        k_rootContainer.Add(k_typeIndicator);

        k_healthBar = new GameStatBar()
            .SetVariant(StatBarVariant.Dynamic)
            .SetDynamicBaseVariant(StatBarVariant.Health)
            .SetDynamicThresholds(warningAt: 0.3f, dangerAt: 0.15f)
            .SetHeight(4)
            .SetShowValue(false)
            .SetAnimateChanges(true)
            .Build();
        k_healthBar.style.position = Position.Absolute;
        k_healthBar.style.left = 4;
        k_healthBar.style.right = 4;
        k_healthBar.style.bottom = 16;
        k_rootContainer.Add(k_healthBar);

        k_manaBar = new GameStatBar()
            .SetVariant(StatBarVariant.Mana)
            .SetHeight(3)
            .SetShowValue(false)
            .SetAnimateChanges(true)
            .Build();
        k_manaBar.style.position = Position.Absolute;
        k_manaBar.style.left = 4;
        k_manaBar.style.right = 4;
        k_manaBar.style.bottom = 11;
        k_manaBar.style.display = DisplayStyle.None;
        k_rootContainer.Add(k_manaBar);

        k_statusEffectsRow = new GameContainer("status-effects")
            .SetRow()
            .SetJustifyContent(Justify.Center)
            .SetAlignItems(Align.Center);
        k_statusEffectsRow.style.position = Position.Absolute;
        k_statusEffectsRow.style.left = 0;
        k_statusEffectsRow.style.right = 0;
        k_statusEffectsRow.style.bottom = 2;
        k_statusEffectsRow.style.height = 10;
        k_rootContainer.Add(k_statusEffectsRow);

        k_deadOverlay = new GameContainer("dead-overlay")
            .SetAbsoluteFill()
            .SetCenter()
            .SetBackgroundColor(new Color(0, 0, 0, 0.6f))
            .SetBorderRadius(borders.RadiusMD);
        k_deadOverlay.style.display = DisplayStyle.None;
        k_deadOverlay.pickingMode = PickingMode.Ignore;

        k_deadIcon = new GameLabel()
            .SetText("💀")
            .SetStyle(LabelStyle.DisplaySmall)
            .SetTextAlign(TextAnchor.MiddleCenter)
            .Build();
        k_deadOverlay.Add(k_deadIcon);

        k_rootContainer.Add(k_deadOverlay);

        SetSize(GameTheme.Current.Components.Slot.SizeL);
        SetShowDurability(false);
    }

    #endregion

    #region Fluent API - Character Data

    public GameCharacterSlotDisplay SetCharacter(LiveCharacter? character) {
        k_character = character;

        if (character != null) {
            SetRarity(character.Rarity);
        }

        RefreshDisplay();
        return this;
    }

    public GameCharacterSlotDisplay ClearCharacter() {
        return SetCharacter(null);
    }

    public GameCharacterSlotDisplay SetShowManaBar(bool show) {
        k_showManaBar = show;
        RefreshDisplay();
        return this;
    }

    public GameCharacterSlotDisplay SetShowStatusEffects(bool show) {
        k_showStatusEffects = show;
        RefreshDisplay();
        return this;
    }

    public GameCharacterSlotDisplay SetMaxVisibleEffects(int max) {
        k_maxVisibleEffects = Math.Max(1, max);
        RefreshDisplay();
        return this;
    }

    public GameCharacterSlotDisplay Refresh() {
        RefreshDisplay();
        return this;
    }

    #endregion

    #region Abstract Implementation

	protected override void OnRightClickAction(Vector2 mousePosition) {
		if (k_character == null) return;

		var menu = GameContextMenu.Show(this, mousePosition)
			.AddHeader(k_character.Name);

		// Inspect
		menu.AddItem("🔍 Inspect", () => {
			ShowDetailedInspector();
			k_onInspect?.Invoke();
		});

		menu.AddDivider();

		// Character info
		menu.AddItem($"📊 Level {k_character.Level} {GetCharacterTypeText()}", null, enabled: false);

		if (k_character.ClassId.HasValue) {
			menu.AddItem($"⚔️ {k_character.ClassId.Value}", null, enabled: false);
		}

		// Health status
		float healthPercent = (float)k_character.CurrentHealth / k_character.MaxHealth;
		string healthStatus = healthPercent switch {
			<= 0 => "💀 Dead",
			< 0.25f => "🩸 Critical",
			< 0.5f => "⚠️ Wounded",
			< 0.75f => "🩹 Hurt",
			_ => "💚 Healthy"
		};
		menu.AddItem(healthStatus, null, enabled: false);

		menu.AddDivider();

		// Actions based on character type
		if (k_character.IsPlayerSide && !k_character.IsDead) {
			menu.AddItem("📋 Character Sheet", () => {
				GameToast.Show($"Opening sheet for {k_character.Name}", ToastType.Info);
			});
		}

		if (k_character.IsEnemySide) {
			menu.AddItem("🎯 Target", () => {
				GameToast.Show($"Targeting {k_character.Name}", ToastType.Info);
			});
		}

		menu.AddDivider();
		menu.AddItem("Cancel", null);
	}

    protected override Texture2D? GetIcon() => null;

    protected override string GetDisplayName() {
        return k_character?.Name ?? "Empty";
    }

    protected override string GetPlaceholderInitials() {
        return GetInitials(k_character?.Name);
    }

    protected override GameTooltip CreateBasicTooltip() {
        if (k_character == null) {
            return new GameTooltip()
                .SetContent("Empty Slot")
                .SetPosition(TooltipPosition.Top)
                .SetDelay(0);
        }

        string text = $"{k_character.Name} (Lv.{k_character.Level})";
        if (k_character.ClassId.HasValue) {
            text += $"\n{k_character.ClassId.Value}";
        }

        return new GameTooltip()
            .SetContent(text)
            .SetPosition(TooltipPosition.Top)
            .SetDelay(0);
    }

    protected override GameTooltip CreateExpandedTooltip() {
        var theme = GameTheme.Current;
        var colors = theme.Colors;
        var spacing = theme.Spacing;

        var content = new GameContainer("character-tooltip")
            .SetColumn()
            .SetPadding(spacing.SM)
            .SetWidth(280);

        if (k_character == null) {
            content.Add(new GameLabel()
                .SetText("Empty Slot")
                .SetStyle(LabelStyle.BodyMedium)
                .SetColor(LabelColor.Tertiary)
                .Build());
        } else {
            // Header
            var headerRow = new GameContainer()
                .SetRow()
                .SetAlignItems(Align.Center)
                .SetJustifyContent(Justify.SpaceBetween);

            headerRow.Add(new GameLabel()
                .SetText(k_character.Name)
                .SetStyle(LabelStyle.TitleMedium)
                .SetColor(LabelColor.Primary)
                .Build());

            headerRow.Add(new GameLabel()
                .SetText($"Level {k_character.Level}")
                .SetStyle(LabelStyle.Caption)
                .SetColor(LabelColor.Secondary)
                .Build());

            content.Add(headerRow);

            // Type line
            string typeText = GetCharacterTypeText();
            if (k_character.ClassId.HasValue) {
                typeText += $" • {k_character.ClassId.Value}";
            }
            var typeLabel = new GameLabel()
                .SetText(typeText)
                .SetStyle(LabelStyle.Caption)
                .SetColor(LabelColor.Tertiary)
                .Build();
            typeLabel.style.marginBottom = spacing.SM;
            content.Add(typeLabel);

            // Health bar
            var healthRow = new GameContainer()
                .SetRow()
                .SetAlignItems(Align.Center)
                .SetFullWidth();

            var healthLabel = new GameLabel()
                .SetText("HP")
                .SetStyle(LabelStyle.Caption)
                .SetColor(LabelColor.Secondary)
                .Build();
            healthLabel.style.width = 30;
            healthRow.Add(healthLabel);

            var healthBar = new GameStatBar()
                .SetVariant(StatBarVariant.Health)
                .SetRange(0, k_character.MaxHealth)
                .SetValue(k_character.CurrentHealth)
                .SetShowValue(true)
                .SetValueFormat("{0}/{1}")
                .SetHeight(12)
                .SetWidth(Length.Percent(100))
                .Build();
            healthBar.style.flexGrow = 1;
            healthRow.Add(healthBar);

            content.Add(healthRow);

            // Mana bar
            if (k_character.MaxMana > 0) {
                var manaRow = new GameContainer()
                    .SetRow()
                    .SetAlignItems(Align.Center)
                    .SetFullWidth();
                manaRow.style.marginTop = spacing.XXS;

                var manaLabel = new GameLabel()
                    .SetText("MP")
                    .SetStyle(LabelStyle.Caption)
                    .SetColor(LabelColor.Secondary)
                    .Build();
                manaLabel.style.width = 30;
                manaRow.Add(manaLabel);

                var manaBar = new GameStatBar()
                    .SetVariant(StatBarVariant.Mana)
                    .SetRange(0, k_character.MaxMana)
                    .SetValue(k_character.CurrentMana)
                    .SetShowValue(true)
                    .SetValueFormat("{0}/{1}")
                    .SetHeight(10)
                    .SetWidth(Length.Percent(100))
                    .Build();
                manaBar.style.flexGrow = 1;
                manaRow.Add(manaBar);

                content.Add(manaRow);
            }

            // Divider
            var divider = new GameDivider().Build();
            divider.style.marginTop = spacing.SM;
            divider.style.marginBottom = spacing.SM;
            content.Add(divider);

            // Combat stats
            var statsGrid = new GameContainer()
                .SetRow()
                .SetFlexWrap(Wrap.Wrap)
                .SetFullWidth();

            AddMiniStat(statsGrid, "AC", k_character.ArmorClass.ToString(), spacing);
            AddMiniStat(statsGrid, "ATK", FormatBonus(k_character.AttackBonus), spacing);
            AddMiniStat(statsGrid, "DMG", FormatBonus(k_character.DamageBonus), spacing);
            AddMiniStat(statsGrid, "INIT", FormatBonus(k_character.InitiativeBonus), spacing);

            content.Add(statsGrid);

            // Attributes
            var attrRow = new GameContainer()
                .SetRow()
                .SetJustifyContent(Justify.SpaceBetween)
                .SetFullWidth();
            attrRow.style.marginTop = spacing.XS;

            AddMiniStat(attrRow, "STR", k_character.Strength.ToString(), spacing);
            AddMiniStat(attrRow, "DEX", k_character.Dexterity.ToString(), spacing);
            AddMiniStat(attrRow, "CON", k_character.Constitution.ToString(), spacing);
            AddMiniStat(attrRow, "INT", k_character.Intelligence.ToString(), spacing);
            AddMiniStat(attrRow, "WIS", k_character.Wisdom.ToString(), spacing);
            AddMiniStat(attrRow, "CHA", k_character.Charisma.ToString(), spacing);

            content.Add(attrRow);

            // Status conditions
            if (k_character.Conditions.Count > 0) {
                var condDivider = new GameDivider().Build();
                condDivider.style.marginTop = spacing.SM;
                condDivider.style.marginBottom = spacing.XS;
                content.Add(condDivider);

                content.Add(new GameLabel()
                    .SetText("Status Effects:")
                    .SetStyle(LabelStyle.Caption)
                    .SetColor(LabelColor.Secondary)
                    .Build());

                var condRow = new GameContainer()
                    .SetRow()
                    .SetFlexWrap(Wrap.Wrap);

                foreach (var (condition, _) in k_character.Conditions) {
                    string icon = GetConditionIcon(condition);
                    var condChip = new GameLabel()
                        .SetText($"{icon} {condition}")
                        .SetStyle(LabelStyle.Caption)
                        .Build();
                    condChip.style.marginRight = spacing.XS;
                    condChip.style.marginBottom = spacing.XXS;
                    condRow.Add(condChip);
                }

                content.Add(condRow);
            }

            // Dead indicator
            if (k_character.IsDead) {
                var deadLabel = new GameLabel()
                    .SetText("💀 DEAD")
                    .SetStyle(LabelStyle.TitleSmall)
                    .SetColor(LabelColor.Error)
                    .SetTextAlign(TextAnchor.MiddleCenter)
                    .Build();
                deadLabel.style.marginTop = spacing.SM;
                content.Add(deadLabel);
            }
        }

        return new GameTooltip()
            .SetContent(content)
            .SetPosition(TooltipPosition.Right);
    }

    private void AddMiniStat(GameContainer parent, string label, string value, SpacingSettings spacing) {
        var stat = new GameContainer()
            .SetColumn()
            .SetAlignItems(Align.Center);
        stat.style.marginRight = spacing.SM;
        stat.style.marginBottom = spacing.XXS;

        var labelEl = new GameLabel()
            .SetText(label)
            .SetStyle(LabelStyle.Caption)
            .SetColor(LabelColor.Tertiary)
            .Build();
        labelEl.style.fontSize = 9;

        var valueEl = new GameLabel()
            .SetText(value)
            .SetStyle(LabelStyle.BodySmall)
            .SetColor(LabelColor.Primary)
            .Build();

        stat.Add(labelEl);
        stat.Add(valueEl);
        parent.Add(stat);
    }

    private static string FormatBonus(int value) {
        return value >= 0 ? $"+{value}" : value.ToString();
    }

    private string GetCharacterTypeText() {
        if (k_character == null) return "Unknown";

        return k_character.Type switch {
            CharacterType.Player => "Player",
            CharacterType.Companion => "Companion",
            CharacterType.Summon => "Summon",
            CharacterType.Enemy => "Enemy",
            CharacterType.Boss => "Boss",
            CharacterType.NPC => "NPC",
            _ => "Character"
        };
    }

    private static string GetConditionIcon(StatusCondition condition) {
        return condition switch {
            StatusCondition.Poisoned => "🤢",
            StatusCondition.Burning => "🔥",
            StatusCondition.Frozen => "❄",
            StatusCondition.Stunned => "💫",
            StatusCondition.Blinded => "🙈",
            StatusCondition.Silenced => "🤐",
            StatusCondition.Weakened => "📉",
            StatusCondition.Empowered => "📈",
            StatusCondition.Shielded => "🛡",
            StatusCondition.Regenerating => "💚",
            StatusCondition.Bleeding => "🩸",
            StatusCondition.Cursed => "☠",
            StatusCondition.Blessed => "✨",
            StatusCondition.Rooted => "🌱",
            StatusCondition.Charmed => "💕",
            StatusCondition.Slowed => "🐢",
            StatusCondition.Frightened => "😨",
            StatusCondition.Confused => "❓",
            StatusCondition.Invisible => "👻",
            StatusCondition.Hasted => "⚡",
            StatusCondition.Undying => "💀",
            _ => "❓"
        };
    }

    public override ItemInstance? GetDraggedItem() => null;

    protected override void OnRefreshComplete() {
        var colors = GameTheme.Current.Colors;

        if (k_character == null) {
            k_levelBadge.style.display = DisplayStyle.None;
            k_typeIndicator.style.display = DisplayStyle.None;
            k_healthBar.style.display = DisplayStyle.None;
            k_manaBar.style.display = DisplayStyle.None;
            k_statusEffectsRow.Clear();
            k_deadOverlay.style.display = DisplayStyle.None;
            return;
        }

        // Level badge
        k_levelBadge.SetText($"Lv{k_character.Level}");
        k_levelBadge.style.display = DisplayStyle.Flex;

        // Type indicator
        string typeIcon = k_character.Type switch {
            CharacterType.Player => "👤",
            CharacterType.Companion => "🤝",
            CharacterType.Summon => "✨",
            CharacterType.Enemy => "👾",
            CharacterType.Boss => "👹",
            CharacterType.NPC => "💬",
            _ => ""
        };
        if (!string.IsNullOrEmpty(typeIcon)) {
            k_typeIndicator.SetText(typeIcon);
            k_typeIndicator.style.display = DisplayStyle.Flex;
        } else {
            k_typeIndicator.style.display = DisplayStyle.None;
        }

        // Health bar
        k_healthBar.SetRange(0, k_character.MaxHealth);
        k_healthBar.SetValue(k_character.CurrentHealth);
        k_healthBar.style.display = DisplayStyle.Flex;

        // Mana bar
        if (k_showManaBar && k_character.MaxMana > 0) {
            k_manaBar.SetRange(0, k_character.MaxMana);
            k_manaBar.SetValue(k_character.CurrentMana);
            k_manaBar.style.display = DisplayStyle.Flex;
            k_healthBar.style.bottom = 18;
        } else {
            k_manaBar.style.display = DisplayStyle.None;
            k_healthBar.style.bottom = 14;
        }

        // Status effects row
        k_statusEffectsRow.Clear();
        if (k_showStatusEffects && k_character.Conditions.Count > 0) {
            int shown = 0;
            foreach (var (condition, _) in k_character.Conditions) {
                if (shown >= k_maxVisibleEffects) {
                    var moreLabel = new GameLabel()
                        .SetText($"+{k_character.Conditions.Count - shown}")
                        .SetStyle(LabelStyle.Caption)
                        .Build();
                    moreLabel.style.fontSize = 8;
                    moreLabel.style.color = colors.TextTertiary;
                    k_statusEffectsRow.Add(moreLabel);
                    break;
                }

                string icon = GetConditionIcon(condition);
                var iconLabel = new GameLabel()
                    .SetText(icon)
                    .Build();
                iconLabel.style.fontSize = 9;
                iconLabel.tooltip = condition.ToString();
                k_statusEffectsRow.Add(iconLabel);
                shown++;
            }
        }

        // Dead overlay
        if (k_character.IsDead) {
            k_deadOverlay.style.display = DisplayStyle.Flex;
            k_rootContainer.SetOpacity(0.7f);
        } else {
            k_deadOverlay.style.display = DisplayStyle.None;
            k_rootContainer.SetOpacity(k_isEnabled ? 1f : 0.5f);
        }

        // Placeholder color based on side
        if (k_character.IsPlayerSide) {
            k_placeholderLabel.style.color = colors.Success;
        } else if (k_character.IsEnemySide) {
            k_placeholderLabel.style.color = colors.Error;
        } else {
            k_placeholderLabel.style.color = colors.TextPrimary;
        }
    }

    #endregion
}
using RPGGame.Core;
using RPGGame.Core.Characters;
using RPGGame.Core.Combat;
using RPGGame.Core.Effects;
using RPGGame.Core.Items;
using RPGGame.UI.Components;
using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Screens;

/// <summary>
/// A large, combat-focused display for a LiveCharacter (player or enemy).
/// Designed specifically for the combat screen with emphasis on visual feedback.
/// 
/// <para>Features:</para>
/// <list type="bullet">
/// <item>Large portrait or placeholder icon with rarity frame</item>
/// <item>Smooth health and mana/stamina bars using GameStatBar</item>
/// <item>Level badge and character type indicator</item>
/// <item>Row of active status effects using GameEffectSlotDisplay</item>
/// <item>Selection ring/glow for target highlighting</item>
/// <item>Turn indicator (pulsing ring when it's this character's turn)</item>
/// <item>Floating damage numbers and text popups</item>
/// <item>Hover and click tooltips</item>
/// </list>
/// 
/// <para>Does NOT inherit from BaseGameSlotDisplay - no drag/drop needed.</para>
/// 
/// <example>
/// <code>
/// var display = new CombatCharacterDisplay()
///     .SetCharacter(enemy)
///     .SetIsEnemy(true)
///     .SetSize(CombatDisplaySize.Large)
///     .OnClicked(() => SelectTarget(enemy))
///     .Build();
/// 
/// // Show floating damage
/// display.ShowDamage(25, DamageType.Fire);
/// 
/// // Show floating text
/// display.ShowText("CRITICAL!", ColorRPG.CritRed);
/// </code>
/// </example>
/// </summary>
public class CombatCharacterDisplay : VisualElement {
    #region Constants

    /// <summary>Small display size (turn order bar).</summary>
    private const float SIZE_SMALL = 48f;

    /// <summary>Medium display size (compact view).</summary>
    private const float SIZE_MEDIUM = 80f;

    /// <summary>Large display size (main battlefield).</summary>
    private const float SIZE_LARGE = 120f;

    /// <summary>Duration for floating text animations in milliseconds.</summary>
    private const int FLOAT_ANIMATION_MS = 1500;

    /// <summary>Maximum visible status effects.</summary>
    private const int MAX_STATUS_EFFECTS = 5;

    #endregion

    #region Fields - Visual Elements

    private readonly GameContainer k_rootContainer;
    private readonly GameRarityFrame k_portraitFrame;
    private readonly GameContainer k_portraitContainer;
    private readonly GameLabel k_portraitPlaceholder;
    private readonly GameContainer k_barsContainer;
    private readonly GameStatBar k_healthBar;
    private readonly GameStatBar k_manaBar;
    private readonly GameLabel k_levelBadge;
    private readonly GameLabel k_typeIndicator;
    private readonly GameContainer k_statusEffectsRow;
    private readonly GameContainer k_selectionRing;
    private readonly GameContainer k_turnIndicator;
    private readonly GameContainer k_deadOverlay;
    private readonly GameLabel k_deadIcon;
    private readonly GameContainer k_floatingTextContainer;

    #endregion

    #region Fields - State

    private LiveCharacter? k_character;
    private bool k_isEnemy;
    private bool k_isSelected;
    private bool k_isCurrentTurn;
    private float k_displaySize = SIZE_LARGE;
    private bool k_showManaBar = true;
    private bool k_showStatusEffects = true;
    private GameDb k_gameDb => GameServices.Db;

    private IVisualElementScheduledItem? k_turnPulseAnimation;
    private readonly List<GameEffectSlotDisplay> k_effectSlots = [];

    #endregion

    #region Fields - Callbacks

    private Action? k_onClick;
    private Action? k_onRightClick;

    #endregion

    #region Properties

    /// <summary>The character being displayed.</summary>
    public LiveCharacter? Character => k_character;

    /// <summary>Whether this displays an enemy.</summary>
    public bool IsEnemy => k_isEnemy;

    /// <summary>Whether this character is currently selected as target.</summary>
    public bool IsSelected => k_isSelected;

    /// <summary>Whether it's currently this character's turn.</summary>
    public bool IsCurrentTurn => k_isCurrentTurn;

    /// <summary>Whether the character is alive.</summary>
    public bool IsAlive => k_character?.IsAlive ?? false;

    #endregion

    #region Constructor

    /// <summary>
    /// Creates a new CombatCharacterDisplay.
    /// </summary>
    public CombatCharacterDisplay() {
        var theme = GameTheme.Current;
        var colors = theme.Colors;
        var borders = theme.Borders;
        var spacing = theme.Spacing;

        // Root container
        k_rootContainer = new GameContainer("combat-character-root")
            .SetColumn()
            .SetAlignItems(Align.Center)
            .SetRelative();

        // Selection ring (behind portrait)
        k_selectionRing = new GameContainer("selection-ring")
            .SetAbsolute()
            .SetBorderRadius(borders.RadiusLG + 4)
            .SetBorderWidth(3);
        k_selectionRing.style.display = DisplayStyle.None;
        k_selectionRing.pickingMode = PickingMode.Ignore;

        // Turn indicator (pulsing ring)
        k_turnIndicator = new GameContainer("turn-indicator")
            .SetAbsolute()
            .SetBorderRadius(borders.RadiusLG + 8)
            .SetBorderWidth(4);
        k_turnIndicator.style.display = DisplayStyle.None;
        k_turnIndicator.pickingMode = PickingMode.Ignore;

        // Portrait frame with rarity styling
        k_portraitFrame = new GameRarityFrame();

        // Portrait container inside frame
        k_portraitContainer = new GameContainer("portrait")
            .SetFullSize()
            .SetCenter();

        k_portraitPlaceholder = new GameLabel()
            .SetStyle(LabelStyle.DisplayMedium)
            .SetTextAlign(TextAnchor.MiddleCenter)
            .Build();
        k_portraitPlaceholder.style.unityFontStyleAndWeight = FontStyle.Bold;

        k_portraitContainer.Add(k_portraitPlaceholder);
        k_portraitFrame.SetContent(k_portraitContainer);

        // Level badge (top-left)
        k_levelBadge = new GameLabel()
            .SetStyle(LabelStyle.Caption)
            .SetTextAlign(TextAnchor.MiddleCenter)
            .Build();
        k_levelBadge.style.position = Position.Absolute;
        k_levelBadge.style.left = -4;
        k_levelBadge.style.top = -4;
        k_levelBadge.style.backgroundColor = colors.BackgroundElevated;
        k_levelBadge.style.paddingLeft = 4;
        k_levelBadge.style.paddingRight = 4;
        k_levelBadge.style.paddingTop = 2;
        k_levelBadge.style.paddingBottom = 2;
        k_levelBadge.style.borderTopLeftRadius = borders.RadiusSM;
        k_levelBadge.style.borderTopRightRadius = borders.RadiusSM;
        k_levelBadge.style.borderBottomLeftRadius = borders.RadiusSM;
        k_levelBadge.style.borderBottomRightRadius = borders.RadiusSM;
        k_levelBadge.style.display = DisplayStyle.None;

        // Type indicator (top-right)
        k_typeIndicator = new GameLabel()
            .SetStyle(LabelStyle.BodyMedium)
            .SetTextAlign(TextAnchor.MiddleCenter)
            .Build();
        k_typeIndicator.style.position = Position.Absolute;
        k_typeIndicator.style.right = -4;
        k_typeIndicator.style.top = -4;

        // Dead overlay
        k_deadOverlay = new GameContainer("dead-overlay")
            .SetAbsoluteFill()
            .SetCenter()
            .SetBackgroundColor(new Color(0, 0, 0, 0.7f))
            .SetBorderRadius(borders.RadiusMD);
        k_deadOverlay.style.display = DisplayStyle.None;
        k_deadOverlay.pickingMode = PickingMode.Ignore;

        k_deadIcon = new GameLabel()
            .SetText("💀")
            .SetStyle(LabelStyle.DisplayMedium)
            .SetTextAlign(TextAnchor.MiddleCenter)
            .Build();
        k_deadOverlay.Add(k_deadIcon);

        // Bars container (below portrait)
        k_barsContainer = new GameContainer("bars")
            .SetColumn()
            .SetFullWidth();

        k_healthBar = new GameStatBar()
            .SetVariant(StatBarVariant.Dynamic)
            .SetDynamicBaseVariant(StatBarVariant.Health)
            .SetDynamicThresholds(warningAt: 0.3f, dangerAt: 0.15f)
            .SetShowValue(true)
            .SetValueFormat("{0}/{1}")
            .SetAnimateChanges(true)
            .Build();

        k_manaBar = new GameStatBar()
            .SetVariant(StatBarVariant.Mana)
            .SetShowValue(true)
            .SetValueFormat("{0}/{1}")
            .SetAnimateChanges(true)
            .Build();
        k_manaBar.style.marginTop = spacing.XXS;

        k_barsContainer.Add(k_healthBar);
        k_barsContainer.Add(k_manaBar);

        // Status effects row
        k_statusEffectsRow = new GameContainer("status-effects")
            .SetRow()
            .SetJustifyContent(Justify.Center)
            .SetFlexWrap(Wrap.Wrap);
        k_statusEffectsRow.style.marginTop = spacing.XS;

        // Floating text container (for damage numbers, etc.)
        k_floatingTextContainer = new GameContainer("floating-text")
            .SetAbsoluteFill()
            .SetCenter();
        k_floatingTextContainer.pickingMode = PickingMode.Ignore;
        k_floatingTextContainer.style.overflow = Overflow.Visible;

        // Assemble hierarchy
        k_rootContainer.Add(k_selectionRing);
        k_rootContainer.Add(k_turnIndicator);
        k_rootContainer.Add(k_portraitFrame);
        k_rootContainer.Add(k_levelBadge);
        k_rootContainer.Add(k_typeIndicator);
        k_rootContainer.Add(k_deadOverlay);
        k_rootContainer.Add(k_barsContainer);
        k_rootContainer.Add(k_statusEffectsRow);
        k_rootContainer.Add(k_floatingTextContainer);

        Add(k_rootContainer);

        // Register events
        RegisterCallback<ClickEvent>(OnClickEvent);
        RegisterCallback<MouseDownEvent>(OnMouseDownEvent);
        RegisterCallback<MouseEnterEvent>(OnMouseEnterEvent);
        RegisterCallback<MouseLeaveEvent>(OnMouseLeaveEvent);

        GameTheme.OnThemeChanged += OnThemeChanged;
    }

    #endregion

    #region Fluent API - Data

    /// <summary>
    /// Sets the character to display.
    /// </summary>
    public CombatCharacterDisplay SetCharacter(LiveCharacter? character) {
        k_character = character;

        if (character != null) {
            k_portraitFrame.SetRarity(character.Rarity);
        }

        return this;
    }

    /// <summary>
    /// Sets whether this is an enemy display.
    /// </summary>
    public CombatCharacterDisplay SetIsEnemy(bool isEnemy) {
        k_isEnemy = isEnemy;
        return this;
    }

    #endregion

    #region Fluent API - Appearance

    /// <summary>
    /// Sets the display size.
    /// </summary>
    public CombatCharacterDisplay SetSize(CombatDisplaySize size) {
        k_displaySize = size switch {
            CombatDisplaySize.Small => SIZE_SMALL,
            CombatDisplaySize.Medium => SIZE_MEDIUM,
            CombatDisplaySize.Large => SIZE_LARGE,
            _ => SIZE_LARGE
        };
        return this;
    }

    /// <summary>
    /// Sets a custom display size.
    /// </summary>
    public CombatCharacterDisplay SetSize(float size) {
        k_displaySize = size;
        return this;
    }

    /// <summary>
    /// Sets whether to show the mana bar.
    /// </summary>
    public CombatCharacterDisplay SetShowManaBar(bool show) {
        k_showManaBar = show;
        return this;
    }

    /// <summary>
    /// Sets whether to show status effects.
    /// </summary>
    public CombatCharacterDisplay SetShowStatusEffects(bool show) {
        k_showStatusEffects = show;
        return this;
    }

    /// <summary>
    /// Sets the selected state (target highlighting).
    /// </summary>
    public CombatCharacterDisplay SetSelected(bool selected) {
        k_isSelected = selected;
        UpdateSelectionRing();
        return this;
    }

    /// <summary>
    /// Sets whether it's this character's turn.
    /// </summary>
    public CombatCharacterDisplay SetCurrentTurn(bool isTurn) {
        k_isCurrentTurn = isTurn;
        UpdateTurnIndicator();
        return this;
    }

    #endregion

    #region Fluent API - Events

    /// <summary>
    /// Called when the display is clicked.
    /// </summary>
    public CombatCharacterDisplay OnClicked(Action callback) {
        k_onClick = callback;
        return this;
    }

    /// <summary>
    /// Called when the display is right-clicked.
    /// </summary>
    public CombatCharacterDisplay OnRightClicked(Action callback) {
        k_onRightClick = callback;
        return this;
    }

    #endregion

    #region Build

    /// <summary>
    /// Builds and finalizes the display.
    /// </summary>
    public CombatCharacterDisplay Build() {
        ApplySize();
        ApplyTheme();
        Refresh();
        return this;
    }

    #endregion

    #region Public Methods - Floating Text

    /// <summary>
    /// Shows floating damage numbers.
    /// </summary>
    /// <param name="amount">Damage amount.</param>
    /// <param name="damageType">Type of damage for color coding.</param>
    /// <param name="isCritical">Whether this was a critical hit.</param>
    public void ShowDamage(float amount, DamageType damageType, bool isCritical = false) {
        ColorRPG color = GetDamageColor(damageType);
        string text = $"-{amount:F0}";

        if (isCritical) {
            text = $"💥 {text}";
            color = ColorRPG.CritRed;
        }

        ShowFloatingText(text, color, isCritical);
    }

    /// <summary>
    /// Shows floating healing numbers.
    /// </summary>
    /// <param name="amount">Healing amount.</param>
    public void ShowHealing(float amount) {
        string text = $"+{amount:F0}";
        ShowFloatingText(text, ColorRPG.HealGreen, false);
    }

    /// <summary>
    /// Shows floating text with custom color.
    /// </summary>
    /// <param name="message">Text message.</param>
    /// <param name="color">Text color.</param>
    public void ShowText(string message, ColorRPG color) {
        ShowFloatingText(message, color, false);
    }

    /// <summary>
    /// Shows a miss indicator.
    /// </summary>
    public void ShowMiss() {
        ShowFloatingText("MISS", ColorRPG.MissGray, false);
    }

    /// <summary>
    /// Shows a dodge indicator.
    /// </summary>
    public void ShowDodge() {
        ShowFloatingText("DODGE", ColorRPG.ShieldBlue, false);
    }

    /// <summary>
    /// Shows a block indicator.
    /// </summary>
    public void ShowBlock() {
        ShowFloatingText("BLOCK", ColorRPG.BlockOrange, false);
    }

    /// <summary>
    /// Refreshes the display from current character state.
    /// </summary>
    public void Refresh() {
        if (k_character == null) return;

        RefreshPortrait();
        RefreshBars();
        RefreshStatusEffects();
        RefreshDeadState();
    }

    #endregion

    #region Private Methods - Refresh

    private void RefreshPortrait() {
        if (k_character == null) return;

        var colors = GameTheme.Current.Colors;

        // Set placeholder text (initials or icon)
        string placeholder = k_isEnemy ? "👾" : "👤";
        if (k_displaySize >= SIZE_MEDIUM) {
            placeholder = GetInitials(k_character.Name);
        }
        k_portraitPlaceholder.SetText(placeholder);

        // Color based on side
        k_portraitPlaceholder.style.color = k_isEnemy ? colors.Error : colors.Success;

        // Level badge
        k_levelBadge.SetText($"Lv{k_character.Level}");
        k_levelBadge.style.display = k_displaySize >= SIZE_MEDIUM ? DisplayStyle.Flex : DisplayStyle.None;

        // Type indicator
        string typeIcon = k_character.Type switch {
            CharacterType.Player => "👤",
            CharacterType.Companion => "🤝",
            CharacterType.Summon => "✨",
            CharacterType.Enemy => "👾",
            CharacterType.Boss => "👹",
            _ => ""
        };
        k_typeIndicator.SetText(typeIcon);
        k_typeIndicator.style.display = k_displaySize >= SIZE_LARGE ? DisplayStyle.Flex : DisplayStyle.None;

        // Update rarity frame
        k_portraitFrame.SetRarity(k_character.Rarity);
        k_portraitFrame.SetShowGlow(k_character.Rarity >= RarityType.Rare);
    }

    private void RefreshBars() {
        if (k_character == null) return;

        // Health bar
        k_healthBar
            .SetRange(0, k_character.MaxHealth)
            .SetValue(k_character.CurrentHealth);

        // Mana bar
        bool showMana = k_showManaBar && k_character.MaxMana > 0;
        k_manaBar.style.display = showMana ? DisplayStyle.Flex : DisplayStyle.None;
        if (showMana) {
            k_manaBar
                .SetRange(0, k_character.MaxMana)
                .SetValue(k_character.CurrentMana);
        }

        // Adjust bar heights based on size
        float barHeight = k_displaySize >= SIZE_LARGE ? 16 : (k_displaySize >= SIZE_MEDIUM ? 12 : 8);
        k_healthBar.SetHeight(barHeight);
        k_manaBar.SetHeight(barHeight * 0.75f);

        // Show values only on larger sizes
        bool showValues = k_displaySize >= SIZE_MEDIUM;
        k_healthBar.SetShowValue(showValues);
        k_manaBar.SetShowValue(showValues);
    }

    private void RefreshStatusEffects() {
        // Clear existing
        foreach (var slot in k_effectSlots) {
            slot.RemoveFromHierarchy();
        }
        k_effectSlots.Clear();
        k_statusEffectsRow.Clear();

        if (k_character == null || !k_showStatusEffects) return;
        if (k_displaySize < SIZE_MEDIUM) return; // Too small for effects

        var theme = GameTheme.Current;
        float effectSize = k_displaySize >= SIZE_LARGE ? 24 : 18;
        int shown = 0;

        foreach (var (condition, effect) in k_character.Conditions) {
            if (shown >= MAX_STATUS_EFFECTS) {
                // Overflow indicator
                var moreLabel = new GameLabel()
                    .SetText($"+{k_character.Conditions.Count - shown}")
                    .SetStyle(LabelStyle.Caption)
                    .SetColor(LabelColor.Tertiary)
                    .Build();
                k_statusEffectsRow.Add(moreLabel);
                break;
            }

            // Try to get EffectProto for this condition
            var effectProto = TryGetEffectProto(condition);

            GameEffectSlotDisplay effectSlot = new GameEffectSlotDisplay()
                .SetEffect(effectProto, (int)effect.Intensity, effect.RemainingDuration)
                .SetSize(effectSize)
                .Build();

            k_effectSlots.Add(effectSlot);
            k_statusEffectsRow.Add(effectSlot);
            shown++;
        }
    }

    private EffectProto? TryGetEffectProto(StatusCondition condition) {
        if (k_gameDb == null) return null;

        // Try to find matching effect proto by condition name
        // This is a simplification - in a real system you'd have a proper mapping
        var protoId = new EffectProto.ID($"Effect_{condition}");
        return k_gameDb.GetOrNull<EffectProto>(protoId);
    }

    private void RefreshDeadState() {
        if (k_character == null) return;

        bool isDead = k_character.IsDead;
        k_deadOverlay.style.display = isDead ? DisplayStyle.Flex : DisplayStyle.None;
        k_rootContainer.SetOpacity(isDead ? 0.6f : 1f);
    }

    #endregion

    #region Private Methods - Visual Updates

    private void ApplySize() {
        var spacing = GameTheme.Current.Spacing;

        k_portraitFrame.SetSize(k_displaySize);
        k_rootContainer.SetWidth(k_displaySize + spacing.MD);

        // Selection ring positioning
        k_selectionRing.style.left = -6;
        k_selectionRing.style.top = -6;
        k_selectionRing.style.right = -6;
        k_selectionRing.style.bottom = -6;

        // Turn indicator positioning
        k_turnIndicator.style.left = -10;
        k_turnIndicator.style.top = -10;
        k_turnIndicator.style.right = -10;
        k_turnIndicator.style.bottom = -10;

        // Bars width
        k_barsContainer.SetWidth(k_displaySize);
        k_barsContainer.style.marginTop = spacing.XS;

        // Font size for placeholder based on display size
        int fontSize = k_displaySize >= SIZE_LARGE ? 32 : (k_displaySize >= SIZE_MEDIUM ? 20 : 14);
        k_portraitPlaceholder.style.fontSize = fontSize;
    }

    private void ApplyTheme() {
        var theme = GameTheme.Current;
        var colors = theme.Colors;
        var borders = theme.Borders;

        // Level badge styling
        k_levelBadge.style.backgroundColor = colors.BackgroundElevated;
        k_levelBadge.style.color = colors.TextPrimary;

        // Selection ring color
        k_selectionRing.SetBorderColor(colors.Primary);

        // Turn indicator color
        k_turnIndicator.SetBorderColor(colors.Warning);
    }

    private void UpdateSelectionRing() {
        var colors = GameTheme.Current.Colors;

        if (k_isSelected && k_character?.IsAlive == true) {
            k_selectionRing.SetBorderColor(colors.Primary);
            k_selectionRing.style.display = DisplayStyle.Flex;
        } else {
            k_selectionRing.style.display = DisplayStyle.None;
        }
    }

    private void UpdateTurnIndicator() {
        var colors = GameTheme.Current.Colors;

        if (k_isCurrentTurn && k_character?.IsAlive == true) {
            k_turnIndicator.SetBorderColor(colors.Warning);
            k_turnIndicator.style.display = DisplayStyle.Flex;
            StartTurnPulse();
        } else {
            k_turnIndicator.style.display = DisplayStyle.None;
            StopTurnPulse();
        }
    }

    private void StartTurnPulse() {
        StopTurnPulse();

        float startTime = Time.time;
        k_turnPulseAnimation = schedule.Execute(() => {
            float elapsed = Time.time - startTime;
            float t = (Mathf.Sin(elapsed * 4f) + 1f) / 2f;
            float alpha = Mathf.Lerp(0.3f, 0.8f, t);

            var color = GameTheme.Current.Colors.Warning;
            k_turnIndicator.SetBorderColor(color.WithAlpha(alpha));
        }).Every(50);
    }

    private void StopTurnPulse() {
        k_turnPulseAnimation?.Pause();
        k_turnPulseAnimation = null;
    }

    #endregion

    #region Private Methods - Floating Text

    private void ShowFloatingText(string text, ColorRPG color, bool isLarge) {
        var theme = GameTheme.Current;

        var floatLabel = new GameLabel()
            .SetText(text)
            .SetStyle(isLarge ? LabelStyle.TitleLarge : LabelStyle.TitleMedium)
            .SetTextAlign(TextAnchor.MiddleCenter)
            .Build();

        floatLabel.style.color = color;
        floatLabel.style.position = Position.Absolute;
        floatLabel.style.textShadow = new TextShadow {
            offset = new Vector2(1, 1),
            blurRadius = 2,
            color = Color.black
        };

        // Random horizontal offset
        float xOffset = UnityEngine.Random.Range(-20f, 20f);
        floatLabel.style.left = k_displaySize / 2 + xOffset - 30;
        floatLabel.style.top = k_displaySize / 2;

        k_floatingTextContainer.Add(floatLabel);

        // Animate upward and fade
        float startTime = Time.time;
        float startY = k_displaySize / 2;

        var animation = schedule.Execute(() => {
            float elapsed = (Time.time - startTime) * 1000;
            float progress = Mathf.Clamp01(elapsed / FLOAT_ANIMATION_MS);

            // Move up
            float y = startY - (progress * 60f);
            floatLabel.style.top = y;

            // Fade out in last third
            float alpha = progress > 0.6f ? 1f - ((progress - 0.6f) / 0.4f) : 1f;
            floatLabel.style.opacity = alpha;

            // Scale up slightly for crits
            if (isLarge && progress < 0.2f) {
                float scale = 1f + (1f - progress / 0.2f) * 0.3f;
                floatLabel.style.scale = new Scale(new Vector3(scale, scale, 1));
            }

            if (progress >= 1f) {
                floatLabel.RemoveFromHierarchy();
            }
        }).Every(16);

        // Cleanup after animation
        schedule.Execute(() => {
            animation?.Pause();
            floatLabel.RemoveFromHierarchy();
        }).ExecuteLater(FLOAT_ANIMATION_MS + 100);
    }

    private static ColorRPG GetDamageColor(DamageType type) {
        return type switch {
            DamageType.Fire => ColorRPG.Fire,
            DamageType.Cold => ColorRPG.Ice,
            DamageType.Lightning => ColorRPG.Lightning,
            DamageType.Poison => ColorRPG.Poison,
            DamageType.Acid => ColorRPG.Poison.RotateHue(-30),
            DamageType.Holy => ColorRPG.Holy,
            DamageType.Necrotic => ColorRPG.NecroticPurple,
            DamageType.Psychic => ColorRPG.Arcane,
            DamageType.Force => ColorRPG.Arcane.Lighten(0.2f),
            _ => ColorRPG.RageRed
        };
    }

    private static string GetInitials(string? name) {
        if (string.IsNullOrEmpty(name)) return "?";
        var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0) return "?";
        if (words.Length == 1) return words[0].Length > 2 ? words[0][..2].ToUpper() : words[0].ToUpper();
        return string.Concat(words.Take(2).Select(w => char.ToUpper(w[0])));
    }

    #endregion

    #region Event Handlers

    private void OnClickEvent(ClickEvent evt) {
        if (k_character?.IsAlive == true) {
            k_onClick?.Invoke();
        }
    }

    private void OnMouseDownEvent(MouseDownEvent evt) {
        if (evt.button == 1 && k_character?.IsAlive == true) {
            k_onRightClick?.Invoke();
            evt.StopPropagation();
        }
    }

    private void OnMouseEnterEvent(MouseEnterEvent evt) {
        if (k_character?.IsAlive == true && !k_isSelected) {
            var colors = GameTheme.Current.Colors;
            k_selectionRing.SetBorderColor(colors.Primary.WithAlpha(0.5f));
            k_selectionRing.style.display = DisplayStyle.Flex;
        }
    }

    private void OnMouseLeaveEvent(MouseLeaveEvent evt) {
        if (!k_isSelected) {
            k_selectionRing.style.display = DisplayStyle.None;
        }
    }

    private void OnThemeChanged(GameTheme theme) {
        ApplyTheme();
        Refresh();
    }

    #endregion

    #region Cleanup

    /// <summary>
    /// Removes the display and cleans up resources.
    /// </summary>
    public new void RemoveFromHierarchy() {
        StopTurnPulse();
        foreach (var slot in k_effectSlots) {
            slot.RemoveFromHierarchy();
        }
        k_effectSlots.Clear();
        GameTheme.OnThemeChanged -= OnThemeChanged;
        base.RemoveFromHierarchy();
    }

    #endregion
}

/// <summary>
/// Display size presets for CombatCharacterDisplay.
/// </summary>
public enum CombatDisplaySize {
    /// <summary>Small size for turn order bar (48px).</summary>
    Small,

    /// <summary>Medium size for compact layouts (80px).</summary>
    Medium,

    /// <summary>Large size for main battlefield (120px).</summary>
    Large
}
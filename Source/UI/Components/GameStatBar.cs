using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Components;

/// <summary>
/// Stat bar visual variants.
/// </summary>
public enum StatBarVariant {
	Default,
	Health,
	Mana,
	Stamina,
	Experience,
	Primary,
	Warning,
	Danger,
	Fatigue,
	Dynamic
}

/// <summary>
/// A themed stat bar for displaying HP, MP, XP, or any value/max value pair.
/// Supports animations, colors, labels, and dynamic warning/danger states.
/// 
/// <para>Usage:</para>
/// <code>
/// var healthBar = new GameStatBar()
///     .SetLabel("HP")
///     .SetRange(0, character.MaxHealth)
///     .SetValue(character.CurrentHealth)
///     .SetVariant(StatBarVariant.Health)
///     .SetShowValue(true)
///     .Build();
/// 
/// // Dynamic bar that auto-changes color based on fill level
/// var dynamicBar = new GameStatBar()
///     .SetLabel("HP")
///     .SetVariant(StatBarVariant.Dynamic)
///     .SetDynamicThresholds(warningAt: 0.3f, dangerAt: 0.15f)
///     .Build();
/// 
/// // Fatigue bar (inverted - higher value = worse)
/// var fatigueBar = new GameStatBar()
///     .SetLabel("FT")
///     .SetVariant(StatBarVariant.Fatigue)
///     .Build();
/// </code>
/// </summary>
public class GameStatBar : VisualElement {
	#region Private Fields

	private readonly GameContainer k_container;
	private readonly GameContainer k_headerRow;
	private readonly GameLabel k_label;
	private readonly GameLabel k_valueLabel;
	private readonly GameContainer k_barContainer;
	private readonly GameContainer k_barBackground;
	private readonly GameContainer k_barFill;
	private readonly GameContainer k_barChange;
	private readonly GameContainer k_glowOverlay;

	private StatBarVariant k_variant = StatBarVariant.Default;
	private float k_minValue = 0;
	private float k_maxValue = 100;
	private float k_currentValue = 100;
	private float k_displayedValue = 100;
	private bool k_showValue = true;
	private string k_valueFormat = "{0}/{1}";
	private bool k_animateChanges = true;

	// Dynamic mode settings
	private float k_warningThreshold;
	private float k_dangerThreshold;
	private bool k_invertThresholds = false;
	private bool k_pulseOnDanger = true;
	private StatBarVariant k_dynamicBaseVariant = StatBarVariant.Health;

	private IVisualElementScheduledItem? k_animation;
	private IVisualElementScheduledItem? k_pulseAnimation;

	#endregion

	#region Constructors

	public GameStatBar() {
		var theme = GameTheme.Current;
		var statBarStyles = theme.Components.StatBar;

		// Initialize thresholds from theme
		k_warningThreshold = statBarStyles.WarningThreshold;
		k_dangerThreshold = statBarStyles.DangerThreshold;

		// Main container
		k_container = new GameContainer("stat-bar-container")
			.SetColumn();

		// Header row with label and value
		k_headerRow = new GameContainer("stat-bar-header")
			.SetRow()
			.SetJustifyContent(Justify.SpaceBetween)
			.SetMarginBottom((int)statBarStyles.LabelSpacing);

		k_label = new GameLabel()
			.SetStyle(LabelStyle.LabelSmall)
			.SetColor(LabelColor.Secondary)
			.Build();
		k_label.style.display = DisplayStyle.None;

		k_valueLabel = new GameLabel()
			.SetStyle(LabelStyle.LabelSmall)
			.SetColor(LabelColor.Primary)
			.Build();

		k_headerRow
			.AddChild(k_label)
			.AddChild(k_valueLabel);

		// Bar container (holds background, change indicator, fill, and glow)
		k_barContainer = new GameContainer("stat-bar-track")
			.SetRelative()
			.SetHeight(statBarStyles.Height)
			.SetOverflow(Overflow.Hidden);

		// Background layer
		k_barBackground = new GameContainer("stat-bar-background")
			.SetAbsoluteFill();

		// Change indicator layer (for damage/heal animations)
		k_barChange = new GameContainer("stat-bar-change")
			.SetAbsolute()
			.SetTop(0)
			.SetBottom(0)
			.SetLeft(0);
		k_barChange.style.display = DisplayStyle.None;

		// Fill layer
		k_barFill = new GameContainer("stat-bar-fill")
			.SetAbsolute()
			.SetTop(0)
			.SetBottom(0)
			.SetLeft(0);

		// Glow overlay for danger pulse
		k_glowOverlay = new GameContainer("stat-bar-glow")
			.SetAbsoluteFill();
		k_glowOverlay.style.display = DisplayStyle.None;

		k_barContainer
			.AddChild(k_barBackground)
			.AddChild(k_barChange)
			.AddChild(k_barFill)
			.AddChild(k_glowOverlay);

		k_container
			.AddChild(k_headerRow)
			.AddChild(k_barContainer);

		Add(k_container);

		GameTheme.OnThemeChanged += OnThemeChanged;
	}

	#endregion

	#region Fluent API - Content

	public GameStatBar SetLabel(string label) {
		k_label.SetText(label);
		k_label.style.display = string.IsNullOrEmpty(label) ? DisplayStyle.None : DisplayStyle.Flex;
		return this;
	}

	public GameStatBar SetRange(float min, float max) {
		k_minValue = min;
		k_maxValue = max;
		UpdateDisplay();
		return this;
	}

	public GameStatBar SetValue(float value, bool animate = true) {
		float previousValue = k_currentValue;
		k_currentValue = Mathf.Clamp(value, k_minValue, k_maxValue);

		if (animate && k_animateChanges && Mathf.Abs(previousValue - k_currentValue) > 0.01f) {
			AnimateValueChange(previousValue, k_currentValue);
		} else {
			k_displayedValue = k_currentValue;
			UpdateDisplay();
		}

		// Update dynamic state if in dynamic mode
		if (k_variant == StatBarVariant.Dynamic || k_variant == StatBarVariant.Fatigue) {
			UpdateDynamicState();
		}

		return this;
	}

	public float Value => k_currentValue;
	public float MaxValue => k_maxValue;
	public float Percent => k_maxValue > k_minValue 
		? (k_currentValue - k_minValue) / (k_maxValue - k_minValue) 
		: 0;

	public GameStatBar SetShowValue(bool show = true) {
		k_showValue = show;
		k_valueLabel.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
		return this;
	}

	public GameStatBar SetValueFormat(string format) {
		k_valueFormat = format;
		UpdateDisplay();
		return this;
	}

	#endregion

	#region Fluent API - Appearance

	public GameStatBar SetVariant(StatBarVariant variant) {
		k_variant = variant;

		// Fatigue is always inverted
		if (variant == StatBarVariant.Fatigue) {
			k_invertThresholds = true;
		}

		ApplyTheme();
		UpdateDynamicState();
		return this;
	}

	/// <summary>
	/// Configures dynamic mode thresholds.
	/// </summary>
	/// <param name="warningAt">Threshold (0-1) at which warning state activates.</param>
	/// <param name="dangerAt">Threshold (0-1) at which danger state activates.</param>
	/// <param name="invert">If true, high values trigger warning/danger (like fatigue).</param>
	/// <param name="pulseOnDanger">If true, adds pulsing glow in danger state.</param>
	public GameStatBar SetDynamicThresholds(
		float? warningAt = null,
		float? dangerAt = null,
		bool invert = false,
		bool pulseOnDanger = true
	) {
		var statBarStyles = GameTheme.Current.Components.StatBar;
		k_warningThreshold = Mathf.Clamp01(warningAt ?? statBarStyles.WarningThreshold);
		k_dangerThreshold = Mathf.Clamp01(dangerAt ?? statBarStyles.DangerThreshold);
		k_invertThresholds = invert;
		k_pulseOnDanger = pulseOnDanger;
		UpdateDynamicState();
		return this;
	}

	/// <summary>
	/// Sets the base variant used when Dynamic mode is in normal state.
	/// </summary>
	public GameStatBar SetDynamicBaseVariant(StatBarVariant baseVariant) {
		k_dynamicBaseVariant = baseVariant;
		UpdateDynamicState();
		return this;
	}

	public GameStatBar SetColors(Color fill, Color background) {
		k_barFill.SetBackgroundColor(fill);
		k_barBackground.SetBackgroundColor(background);
		return this;
	}

	public GameStatBar SetHeight(float height) {
		k_barContainer.SetHeight(height);
		return this;
	}

	public GameStatBar SetWidth(float width) {
		style.width = width;
		return this;
	}

	public GameStatBar SetWidth(Length width) {
		style.width = width;
		return this;
	}

	public GameStatBar SetAnimateChanges(bool animate) {
		k_animateChanges = animate;
		return this;
	}

	#endregion

	#region Fluent API - Layout

	public GameStatBar SetMargin(int all) {
		style.marginTop = all;
		style.marginBottom = all;
		style.marginLeft = all;
		style.marginRight = all;
		return this;
	}

	public GameStatBar SetMargin(int vertical, int horizontal) {
		style.marginTop = vertical;
		style.marginBottom = vertical;
		style.marginLeft = horizontal;
		style.marginRight = horizontal;
		return this;
	}

	public GameStatBar SetMarginTop(int top) {
		style.marginTop = top;
		return this;
	}

	public GameStatBar SetMarginBottom(int bottom) {
		style.marginBottom = bottom;
		return this;
	}

	#endregion

	#region Build

	public GameStatBar Build() {
		ApplyTheme();
		UpdateDisplay();
		UpdateDynamicState();
		return this;
	}

	#endregion

	#region Dynamic State

	private void UpdateDynamicState() {
		if (k_variant != StatBarVariant.Dynamic && k_variant != StatBarVariant.Fatigue) {
			StopPulse();
			k_glowOverlay.style.display = DisplayStyle.None;
			return;
		}

		float percent = Percent;
		bool isInDanger;
		bool isInWarning;

		if (k_invertThresholds) {
			// Inverted: high values are bad (fatigue)
			isInDanger = percent >= (1f - k_dangerThreshold);
			isInWarning = percent >= (1f - k_warningThreshold) && !isInDanger;
		} else {
			// Normal: low values are bad (health)
			isInDanger = percent <= k_dangerThreshold;
			isInWarning = percent <= k_warningThreshold && !isInDanger;
		}

		var colors = GameTheme.Current.Colors;

		if (isInDanger) {
			ApplyDangerStyle(colors);
			if (k_pulseOnDanger) {
				StartPulse(colors.Error);
			}
		} else if (isInWarning) {
			ApplyWarningStyle(colors);
			StopPulse();
		} else {
			ApplyNormalStyle(colors);
			StopPulse();
		}
	}

	private void ApplyDangerStyle(ColorPalette colors) {
		k_barFill.SetBackgroundColor(colors.Error);
		k_barBackground.SetBackgroundColor(colors.ErrorBackground);
		k_valueLabel.SetColor(colors.Error);
		k_label.SetColor(colors.Error);
	}

	private void ApplyWarningStyle(ColorPalette colors) {
		k_barFill.SetBackgroundColor(colors.Warning);
		k_barBackground.SetBackgroundColor(colors.WarningBackground);
		k_valueLabel.SetColor(colors.Warning);
		k_label.SetColor(colors.TextSecondary);
	}

	private void ApplyNormalStyle(ColorPalette colors) {
		k_glowOverlay.style.display = DisplayStyle.None;

		if (k_variant == StatBarVariant.Fatigue) {
			// Fatigue uses a gray/neutral color in normal state
			k_barFill.SetBackgroundColor(colors.TextTertiary);
			k_barBackground.SetBackgroundColor(colors.BackgroundTertiary);
		} else if (k_variant == StatBarVariant.Dynamic) {
			// Dynamic uses the base variant colors
			var (fillColor, bgColor) = GetVariantColors(k_dynamicBaseVariant, colors);
			k_barFill.SetBackgroundColor(fillColor);
			k_barBackground.SetBackgroundColor(bgColor);
		}

		k_valueLabel.SetColor(colors.TextPrimary);
		k_label.SetColor(colors.TextSecondary);
	}

	#endregion

	#region Pulse Animation

	private void StartPulse(Color glowColor) {
		if (k_pulseAnimation != null) return;

		var statBarStyles = GameTheme.Current.Components.StatBar;

		k_glowOverlay.style.display = DisplayStyle.Flex;

		float startTime = Time.time;
		float pulseDuration = statBarStyles.PulseDurationSeconds;
		float minOpacity = statBarStyles.GlowOpacityMin;
		float maxOpacity = statBarStyles.GlowOpacityMax;

		k_pulseAnimation = schedule.Execute(() => {
			float elapsed = Time.time - startTime;
			float t = (Mathf.Sin(elapsed * Mathf.PI * 2f / pulseDuration) + 1f) / 2f;

			float alpha = Mathf.Lerp(minOpacity, maxOpacity, t);
			k_glowOverlay.SetBackgroundColor(new Color(glowColor.r, glowColor.g, glowColor.b, alpha));
		}).Every(statBarStyles.PulseFrameMs);
	}

	private void StopPulse() {
		k_pulseAnimation?.Pause();
		k_pulseAnimation = null;
		k_glowOverlay.style.display = DisplayStyle.None;
	}

	#endregion

	#region Animation

	private void AnimateValueChange(float from, float to) {
		k_animation?.Pause();

		var theme = GameTheme.Current;
		var colors = theme.Colors;
		var statBarStyles = theme.Components.StatBar;

		float startTime = Time.time;
		bool isDamage = to < from;

		k_barChange.style.display = DisplayStyle.Flex;
		k_barChange.SetBackgroundColor(isDamage ? colors.Error : colors.Success);

		float fromPercent = (from - k_minValue) / (k_maxValue - k_minValue);
		float toPercent = (to - k_minValue) / (k_maxValue - k_minValue);

		if (isDamage) {
			k_barChange.style.left = Length.Percent(toPercent * 100);
			k_barChange.style.width = Length.Percent((fromPercent - toPercent) * 100);
		} else {
			k_barChange.style.left = Length.Percent(fromPercent * 100);
			k_barChange.style.width = Length.Percent((toPercent - fromPercent) * 100);
		}

		int animationDuration = statBarStyles.AnimationDurationMs;

		k_animation = schedule.Execute(() => {
			float elapsed = (Time.time - startTime) * 1000;
			float progress = Mathf.Clamp01(elapsed / animationDuration);
			progress = 1f - Mathf.Pow(1f - progress, 2f);

			k_displayedValue = Mathf.Lerp(from, to, progress);
			UpdateDisplay();

			if (progress >= 1f) {
				k_animation?.Pause();
				k_barChange.style.display = DisplayStyle.None;
			}
		}).Every(statBarStyles.AnimationFrameMs);
	}

	#endregion

	#region Display

	private void UpdateDisplay() {
		float percent = k_maxValue > k_minValue 
			? (k_displayedValue - k_minValue) / (k_maxValue - k_minValue) 
			: 0;

		k_barFill.style.width = Length.Percent(percent * 100);

		if (k_showValue) {
			k_valueLabel.SetText(string.Format(k_valueFormat, 
				Mathf.RoundToInt(k_displayedValue), 
				Mathf.RoundToInt(k_maxValue)));
		}
	}

	#endregion

	#region Theme Application

	private (Color fill, Color background) GetVariantColors(StatBarVariant variant, ColorPalette colors) {
		return variant switch {
			StatBarVariant.Health => (colors.Health, colors.HealthBackground),
			StatBarVariant.Mana => (colors.Mana, colors.ManaBackground),
			StatBarVariant.Stamina => (colors.Stamina, colors.StaminaBackground),
			StatBarVariant.Experience => (colors.Experience, colors.ExperienceBackground),
			StatBarVariant.Primary => (colors.Primary, colors.BackgroundTertiary),
			StatBarVariant.Warning => (colors.Warning, colors.WarningBackground),
			StatBarVariant.Danger => (colors.Error, colors.ErrorBackground),
			StatBarVariant.Fatigue => (colors.TextTertiary, colors.BackgroundTertiary),
			StatBarVariant.Dynamic => GetVariantColors(k_dynamicBaseVariant, colors),
			_ => (colors.Primary, colors.BackgroundTertiary)
		};
	}

	private void ApplyTheme() {
		var theme = GameTheme.Current;
		var colors = theme.Colors;
		var borders = theme.Borders;
		var statBarStyles = theme.Components.StatBar;

		var (fillColor, bgColor) = GetVariantColors(k_variant, colors);

		// Bar container styling
		k_barContainer
			.SetHeight(statBarStyles.Height)
			.SetBorderRadius(borders.ProgressBarRadius)
			.SetOverflow(Overflow.Hidden);

		// Background and fill colors
		k_barBackground.SetBackgroundColor(bgColor);
		k_barFill.SetBackgroundColor(fillColor);

		// Glow overlay radius
		k_glowOverlay.SetBorderRadius(borders.ProgressBarRadius);

		// Label styling
		k_label.SetColor(colors.TextSecondary);
		k_valueLabel.SetColor(colors.TextPrimary);
	}

	private void OnThemeChanged(GameTheme theme) {
		ApplyTheme();
		UpdateDynamicState();
	}

	#endregion

	#region Cleanup

	public new void RemoveFromHierarchy() {
		k_animation?.Pause();
		StopPulse();
		GameTheme.OnThemeChanged -= OnThemeChanged;
		base.RemoveFromHierarchy();
	}

	#endregion
}
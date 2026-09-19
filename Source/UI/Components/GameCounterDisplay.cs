using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Components;

#nullable disable

/// <summary>
/// A themed counter display for resources like gold, gems, or any numeric value.
/// Supports animated counting and icons.
/// 
/// <para>Usage:</para>
/// <code>
/// var goldCounter = new GameCounterDisplay()
///     .SetIcon(goldIcon)
///     .SetValue(player.Gold)
///     .SetFormat("N0")
///     .SetAnimated(true)
///     .Build();
/// 
/// // Update with animation
/// goldCounter.SetValue(player.Gold + 100);
/// </code>
/// </summary>
public class GameCounterDisplay : VisualElement {
	#region Private Fields

	private readonly VisualElement k_container;
	private readonly VisualElement k_iconContainer;
	private readonly Label k_valueLabel;
	private readonly Label k_labelText;

	private long k_currentValue = 0;
	private long k_displayedValue = 0;
	private string k_format = "N0";
	private bool k_animate = true;
	private int k_animationDurationMs = 500;
	private CounterSize k_size = CounterSize.Medium;

	private IVisualElementScheduledItem k_animation;

	#endregion

	#region Constructors

	public GameCounterDisplay() {
		k_container = new VisualElement {
			style = {
				flexDirection = FlexDirection.Row,
				alignItems = Align.Center
			}
		};

		k_iconContainer = new VisualElement {
			style = { display = DisplayStyle.None, marginRight = 8 }
		};

		k_valueLabel = new Label("0");

		k_labelText = new Label {
			style = { display = DisplayStyle.None, marginLeft = 4 }
		};

		k_container.Add(k_iconContainer);
		k_container.Add(k_valueLabel);
		k_container.Add(k_labelText);

		Add(k_container);

		GameTheme.OnThemeChanged += OnThemeChanged;
	}

	#endregion

	#region Fluent API

	public GameCounterDisplay SetValue(long value, bool animate = true) {
		long previousValue = k_currentValue;
		k_currentValue = value;

		if (animate && k_animate && previousValue != k_currentValue) {
			AnimateValueChange(previousValue, k_currentValue);
		} else {
			k_displayedValue = k_currentValue;
			UpdateDisplay();
		}

		return this;
	}

	public long Value => k_currentValue;

	public GameCounterDisplay SetIcon(Texture2D icon) {
		k_iconContainer.Clear();
		if (icon != null) {
			var iconElement = new VisualElement {
				style = {
					backgroundImage = new StyleBackground(icon),
					width = GetIconSize(),
					height = GetIconSize()
				}
			};
			k_iconContainer.Add(iconElement);
			k_iconContainer.style.display = DisplayStyle.Flex;
		} else {
			k_iconContainer.style.display = DisplayStyle.None;
		}
		return this;
	}

	public GameCounterDisplay SetIcon(Sprite sprite) {
		k_iconContainer.Clear();
		if (sprite != null) {
			var iconElement = new VisualElement {
				style = {
					backgroundImage = new StyleBackground(sprite),
					width = GetIconSize(),
					height = GetIconSize()
				}
			};
			k_iconContainer.Add(iconElement);
			k_iconContainer.style.display = DisplayStyle.Flex;
		} else {
			k_iconContainer.style.display = DisplayStyle.None;
		}
		return this;
	}

	public GameCounterDisplay SetLabel(string label) {
		k_labelText.text = label;
		k_labelText.style.display = string.IsNullOrEmpty(label) ? DisplayStyle.None : DisplayStyle.Flex;
		return this;
	}

	public GameCounterDisplay SetFormat(string format) {
		k_format = format;
		UpdateDisplay();
		return this;
	}

	public GameCounterDisplay SetAnimated(bool animated, int durationMs = 500) {
		k_animate = animated;
		k_animationDurationMs = durationMs;
		return this;
	}

	public GameCounterDisplay SetSize(CounterSize size) {
		k_size = size;
		ApplyTheme();
		return this;
	}

	public GameCounterDisplay SetColor(Color color) {
		k_valueLabel.style.color = color;
		return this;
	}

	#endregion

	#region Build

	public GameCounterDisplay Build() {
		ApplyTheme();
		UpdateDisplay();
		return this;
	}

	#endregion

	#region Animation

	private void AnimateValueChange(long from, long to) {
		k_animation?.Pause();

		float startTime = Time.time;

		k_animation = schedule.Execute(() => {
			float elapsed = (Time.time - startTime) * 1000;
			float progress = Mathf.Clamp01(elapsed / k_animationDurationMs);
			progress = 1f - Mathf.Pow(1f - progress, 3f); // Ease out cubic

			k_displayedValue = (long)Mathf.Lerp(from, to, progress);
			UpdateDisplay();

			if (progress >= 1f) {
				k_animation?.Pause();
				k_displayedValue = to;
				UpdateDisplay();
			}
		}).Every(16);
	}

	#endregion

	#region Display

	private void UpdateDisplay() {
		k_valueLabel.text = k_displayedValue.ToString(k_format);
	}

	private float GetIconSize() {
		return k_size switch {
			CounterSize.Small => 16,
			CounterSize.Large => 32,
			_ => 24
		};
	}

	#endregion

	#region Theme Application

	private void ApplyTheme() {
		var theme = GameTheme.Current;
		var colors = theme.Colors;
		var typography = theme.Typography;

		var textStyle = k_size switch {
			CounterSize.Small => typography.LabelSmall,
			CounterSize.Large => typography.TitleLarge,
			_ => typography.TitleMedium
		};

		textStyle.ApplyTo(k_valueLabel.style);
		k_valueLabel.style.color = colors.TextPrimary;

		typography.LabelSmall.ApplyTo(k_labelText.style);
		k_labelText.style.color = colors.TextSecondary;

		// Update icon size
		var icon = k_iconContainer.Q<VisualElement>();
		if (icon != null) {
			float size = GetIconSize();
			icon.style.width = size;
			icon.style.height = size;
		}
	}

	private void OnThemeChanged(GameTheme theme) => ApplyTheme();

	#endregion

	#region Cleanup

	new public void RemoveFromHierarchy() {
		k_animation?.Pause();
		GameTheme.OnThemeChanged -= OnThemeChanged;
		base.RemoveFromHierarchy();
	}

	#endregion
}

#region Enums

public enum CounterSize {
	Small,
	Medium,
	Large
}

#endregion
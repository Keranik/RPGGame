using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Components;

#nullable disable

/// <summary>
/// A themed timer/countdown display for cooldowns, buffs, and timed events.
/// 
/// <para>Usage:</para>
/// <code>
/// // Countdown timer
/// var cooldown = new GameTimer()
///     .SetDuration(30000) // 30 seconds
///     .SetFormat(TimerFormat.MinutesSeconds)
///     .OnComplete(() => EnableAbility())
///     .Build()
///     .Start();
/// 
/// // Count up timer
/// var elapsed = new GameTimer()
///     .SetCountUp(true)
///     .SetFormat(TimerFormat.HoursMinutesSeconds)
///     .Build()
///     .Start();
/// </code>
/// </summary>
public class GameTimer : VisualElement {
	#region Private Fields

	private readonly VisualElement k_container;
	private readonly VisualElement k_iconContainer;
	private readonly Label k_timeLabel;
	private readonly VisualElement k_progressContainer;
	private readonly VisualElement k_progressFill;

	private long k_durationMs = 0;
	private long k_remainingMs = 0;
	private long k_elapsedMs = 0;
	private bool k_isCountUp = false;
	private bool k_isRunning = false;
	private bool k_showProgress = false;
	private TimerFormat k_format = TimerFormat.MinutesSeconds;
	private TimerVariant k_variant = TimerVariant.Default;

	private float k_startTime;
	private IVisualElementScheduledItem k_timerUpdate;

	private Action k_onComplete;
	private Action<long> k_onTick;

	#endregion

	#region Constructors

	public GameTimer() {
		k_container = new VisualElement {
			style = {
				flexDirection = FlexDirection.Row,
				alignItems = Align.Center
			}
		};

		k_iconContainer = new VisualElement {
			style = { display = DisplayStyle.None, marginRight = 8 }
		};

		k_timeLabel = new Label("00:00");

		k_progressContainer = new VisualElement {
			style = {
				display = DisplayStyle.None,
				position = Position.Relative,
				flexGrow = 1,
				marginLeft = 8
			}
		};

		k_progressFill = new VisualElement {
			style = { position = Position.Absolute, left = 0, top = 0, bottom = 0 }
		};

		k_progressContainer.Add(k_progressFill);

		k_container.Add(k_iconContainer);
		k_container.Add(k_timeLabel);
		k_container.Add(k_progressContainer);

		Add(k_container);

		GameTheme.OnThemeChanged += OnThemeChanged;
	}

	#endregion

	#region Fluent API

	public GameTimer SetDuration(long milliseconds) {
		k_durationMs = milliseconds;
		k_remainingMs = milliseconds;
		UpdateDisplay();
		return this;
	}

	public GameTimer SetDurationSeconds(float seconds) {
		return SetDuration((long)(seconds * 1000));
	}

	public GameTimer SetCountUp(bool countUp = true) {
		k_isCountUp = countUp;
		return this;
	}

	public GameTimer SetFormat(TimerFormat format) {
		k_format = format;
		UpdateDisplay();
		return this;
	}

	public GameTimer SetVariant(TimerVariant variant) {
		k_variant = variant;
		ApplyTheme();
		return this;
	}

	public GameTimer SetShowProgress(bool show = true) {
		k_showProgress = show;
		k_progressContainer.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
		return this;
	}

	public GameTimer SetIcon(Texture2D icon) {
		k_iconContainer.Clear();
		if (icon != null) {
			var iconElement = new VisualElement {
				style = {
					backgroundImage = new StyleBackground(icon),
					width = 20,
					height = 20
				}
			};
			k_iconContainer.Add(iconElement);
			k_iconContainer.style.display = DisplayStyle.Flex;
		} else {
			k_iconContainer.style.display = DisplayStyle.None;
		}
		return this;
	}

	public GameTimer OnComplete(Action callback) {
		k_onComplete = callback;
		return this;
	}

	public GameTimer OnTick(Action<long> callback) {
		k_onTick = callback;
		return this;
	}

	public long RemainingMs => k_remainingMs;
	public long ElapsedMs => k_elapsedMs;
	public bool IsRunning => k_isRunning;
	public float Progress => k_durationMs > 0 ? (float)k_elapsedMs / k_durationMs : 0;

	#endregion

	#region Build

	public GameTimer Build() {
		ApplyTheme();
		UpdateDisplay();
		return this;
	}

	#endregion

	#region Timer Control

	public GameTimer Start() {
		if (k_isRunning) {
			return this;
		}

		k_isRunning = true;
		k_startTime = Time.time;

		if (!k_isCountUp) {
			k_remainingMs = k_durationMs;
		}

		k_timerUpdate = schedule.Execute(UpdateTimer).Every(100);
		return this;
	}

	public GameTimer Pause() {
		k_isRunning = false;
		k_timerUpdate?.Pause();
		return this;
	}

	public GameTimer Resume() {
		if (k_isRunning) {
			return this;
		}
		k_startTime = Time.time - (k_elapsedMs / 1000f);
		return Start();
	}

	public GameTimer Reset() {
		Pause();
		k_elapsedMs = 0;
		k_remainingMs = k_durationMs;
		UpdateDisplay();
		return this;
	}

	public GameTimer SetTime(long milliseconds) {
		if (k_isCountUp) {
			k_elapsedMs = milliseconds;
		} else {
			k_remainingMs = milliseconds;
			k_elapsedMs = k_durationMs - milliseconds;
		}
		UpdateDisplay();
		return this;
	}

	private void UpdateTimer() {
		float currentTime = Time.time;
		k_elapsedMs = (long)((currentTime - k_startTime) * 1000);

		if (k_isCountUp) {
			k_onTick?.Invoke(k_elapsedMs);
		} else {
			k_remainingMs = (long)Mathf.Max(0, k_durationMs - k_elapsedMs);  // Add (long) cast here
			k_onTick?.Invoke(k_remainingMs);

			if (k_remainingMs <= 0) {
				Pause();
				k_onComplete?.Invoke();
			}
		}

		UpdateDisplay();
	}

	#endregion

	#region Display

	private void UpdateDisplay() {
		long displayMs = k_isCountUp ? k_elapsedMs : k_remainingMs;

		k_timeLabel.text = FormatTime(displayMs);

		if (k_showProgress && k_durationMs > 0) {
			float progress = (float)k_elapsedMs / k_durationMs;
			k_progressFill.style.width = Length.Percent(progress * 100);
		}
	}

	private string FormatTime(long milliseconds) {
		long totalSeconds = milliseconds / 1000;
		long hours = totalSeconds / 3600;
		long minutes = (totalSeconds % 3600) / 60;
		long seconds = totalSeconds % 60;
		long ms = milliseconds % 1000;

		return k_format switch {
			TimerFormat.Seconds => $"{totalSeconds}",
			TimerFormat.SecondsMillis => $"{totalSeconds}.{ms / 100}",
			TimerFormat.MinutesSeconds => $"{minutes:D2}:{seconds:D2}",
			TimerFormat.HoursMinutesSeconds => $"{hours:D2}:{minutes:D2}:{seconds:D2}",
			TimerFormat.Verbose => hours > 0 
				? $"{hours}h {minutes}m {seconds}s" 
				: minutes > 0 
					? $"{minutes}m {seconds}s" 
					: $"{seconds}s",
			_ => $"{minutes:D2}:{seconds:D2}"
		};
	}

	#endregion

	#region Theme Application

	private void ApplyTheme() {
		var theme = GameTheme.Current;
		var colors = theme.Colors;
		var borders = theme.Borders;
		var typography = theme.Typography;

		Color textColor = k_variant switch {
			TimerVariant.Danger => colors.Error,
			TimerVariant.Warning => colors.Warning,
			TimerVariant.Success => colors.Success,
			_ => colors.TextPrimary
		};

		typography.TitleMedium.ApplyTo(k_timeLabel.style);
		k_timeLabel.style.color = textColor;

		// Progress bar
		k_progressContainer.style.height = 4;
		k_progressContainer.style.backgroundColor = colors.BackgroundTertiary;
		borders.ApplyRadius(k_progressContainer.style, borders.RadiusFull);

		k_progressFill.style.backgroundColor = textColor;
		borders.ApplyRadius(k_progressFill.style, borders.RadiusFull);
	}

	private void OnThemeChanged(GameTheme theme) => ApplyTheme();

	#endregion

	#region Cleanup

	new public void RemoveFromHierarchy() {
		k_timerUpdate?.Pause();
		GameTheme.OnThemeChanged -= OnThemeChanged;
		base.RemoveFromHierarchy();
	}

	#endregion
}

#region Enums

public enum TimerFormat {
	Seconds,
	SecondsMillis,
	MinutesSeconds,
	HoursMinutesSeconds,
	Verbose
}

public enum TimerVariant {
	Default,
	Danger,
	Warning,
	Success
}

#endregion
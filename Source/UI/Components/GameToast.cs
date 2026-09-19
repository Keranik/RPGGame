using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Components;

#nullable disable

/// <summary>
/// A themed toast notification component for temporary messages.
/// Supports auto-dismiss, actions, and stacking.
/// 
/// <para>Usage:</para>
/// <code>
/// // Simple toast
/// GameToast.Show("Item acquired!", ToastType.Success);
/// 
/// // With action
/// GameToast.Show("Achievement unlocked!", ToastType.Info, 5000)
///     .SetAction("View", () => ShowAchievements());
/// 
/// // Custom
/// new GameToast()
///     .SetMessage("Level Up!")
///     .SetIcon(levelUpIcon)
///     .SetType(ToastType.Success)
///     .SetDuration(3000)
///     .Show();
/// </code>
/// </summary>
public class GameToast : VisualElement {
	#region Static Toast Manager

	private static VisualElement s_toastContainer;
	private static readonly List<GameToast> s_activeToasts = new();
	private const int MAX_VISIBLE_TOASTS = 5;

	public static void Initialize(VisualElement root) {
		s_toastContainer = new VisualElement {
			name = "toast-container",
			style = {
				position = Position.Absolute,
				right = 20,
				top = 20,
				flexDirection = FlexDirection.Column,
				alignItems = Align.FlexEnd
			}
		};
		s_toastContainer.pickingMode = PickingMode.Ignore;
		root.Add(s_toastContainer);
	}

	public static GameToast Show(string message, ToastType type = ToastType.Info, int durationMs = 3000) {
		var toast = new GameToast()
			.SetMessage(message)
			.SetType(type)
			.SetDuration(durationMs)
			.Build();

		toast.Show();
		return toast;
	}

	#endregion

	#region Private Fields

	private readonly VisualElement k_container;
	private readonly VisualElement k_iconContainer;
	private readonly Label k_messageLabel;
	private readonly VisualElement k_actionContainer;
	private readonly Button k_closeButton;

	private ToastType k_type = ToastType.Info;
	private int k_durationMs = 3000;
	private bool k_isVisible = false;
	private IVisualElementScheduledItem k_dismissSchedule;

	#endregion

	#region Constructors

	public GameToast() {
		style.marginBottom = 8;

		k_container = new VisualElement {
			style = {
				flexDirection = FlexDirection.Row,
				alignItems = Align.Center
			}
		};

		k_iconContainer = new VisualElement {
			style = { marginRight = 12 }
		};

		k_messageLabel = new Label();

		k_actionContainer = new VisualElement {
			style = {
				flexDirection = FlexDirection.Row,
				marginLeft = 16,
				display = DisplayStyle.None
			}
		};

		k_closeButton = new Button { text = "✕" };
		k_closeButton.clicked += Dismiss;
		k_closeButton.RemoveFromClassList("unity-button");

		k_container.Add(k_iconContainer);
		k_container.Add(k_messageLabel);
		k_container.Add(k_actionContainer);
		k_container.Add(k_closeButton);

		Add(k_container);

		GameTheme.OnThemeChanged += OnThemeChanged;
	}

	#endregion

	#region Fluent API

	public GameToast SetMessage(string message) {
		k_messageLabel.text = message;
		return this;
	}

	public GameToast SetType(ToastType type) {
		k_type = type;
		ApplyTheme();
		return this;
	}

	public GameToast SetDuration(int milliseconds) {
		k_durationMs = milliseconds;
		return this;
	}

	public GameToast SetPersistent() {
		k_durationMs = 0;
		return this;
	}

	public GameToast SetIcon(Texture2D icon) {
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
		}
		return this;
	}

	public GameToast SetAction(string label, Action onClick) {
		k_actionContainer.style.display = DisplayStyle.Flex;

		var button = new GameButton(label)
			.SetVariant(ButtonVariant.Ghost)
			.SetSize(ButtonSize.Small)
			.OnClick(() => {
				onClick?.Invoke();
				Dismiss();
			})
			.Build();

		k_actionContainer.Add(button);
		return this;
	}

	public GameToast SetClosable(bool closable) {
		k_closeButton.style.display = closable ? DisplayStyle.Flex : DisplayStyle.None;
		return this;
	}

	#endregion

	#region Build

	public GameToast Build() {
		ApplyTheme();
		return this;
	}

	#endregion

	#region Show/Dismiss

	public void Show() {
		if (s_toastContainer == null) {
			UnityEngine.Debug.LogWarning("GameToast.Initialize() must be called before showing toasts");
			return;
		}

		// Limit visible toasts
		while (s_activeToasts.Count >= MAX_VISIBLE_TOASTS) {
			s_activeToasts[0].Dismiss();
		}

		s_toastContainer.Add(this);
		s_activeToasts.Add(this);
		k_isVisible = true;

		// Auto-dismiss
		if (k_durationMs > 0) {
			k_dismissSchedule = schedule.Execute(Dismiss).StartingIn(k_durationMs);
		}

		// Play sound
		if (GameTheme.Current.Audio.Enabled) {
			UnityEngine.Debug.Log($"[Audio] Playing: {GameTheme.Current.Audio.Notification}");
		}
	}

	public void Dismiss() {
		if (!k_isVisible) {
			return;
		}

		k_dismissSchedule?.Pause();
		k_isVisible = false;
		s_activeToasts.Remove(this);
		RemoveFromHierarchy();
	}

	#endregion

	#region Theme Application

	private void ApplyTheme() {
		var theme = GameTheme.Current;
		var colors = theme.Colors;
		var borders = theme.Borders;
		var spacing = theme.Spacing;
		var typography = theme.Typography;

		// Type-specific colors
		var (bgColor, borderColor, iconColor) = k_type switch {
			ToastType.Success => (colors.SuccessBackground, colors.Success, colors.Success),
			ToastType.Warning => (colors.WarningBackground, colors.Warning, colors.Warning),
			ToastType.Error => (colors.ErrorBackground, colors.Error, colors.Error),
			_ => (colors.InfoBackground, colors.Info, colors.Info)
		};

		k_container.style.backgroundColor = bgColor;
		borders.ApplyColor(k_container.style, borderColor);
		borders.ApplyWidth(k_container.style, borders.WidthThin);
		borders.ApplyRadius(k_container.style, borders.RadiusMD);
		spacing.CardPadding.ApplyTo(k_container.style);

		// Message styling
		typography.BodyMedium.ApplyTo(k_messageLabel.style);
		k_messageLabel.style.color = colors.TextPrimary;

		// Close button
		k_closeButton.style.backgroundColor = colors.Transparent;
		k_closeButton.style.color = colors.TextSecondary;
		k_closeButton.style.marginLeft = 12;
		borders.ApplyWidth(k_closeButton.style, 0);

		// Type icon (if not custom)
		if (k_iconContainer.childCount == 0) {
			var iconLabel = new Label(k_type switch {
				ToastType.Success => "✓",
				ToastType.Warning => "⚠",
				ToastType.Error => "✕",
				_ => "ℹ"
			});
			iconLabel.style.fontSize = 16;
			iconLabel.style.color = iconColor;
			k_iconContainer.Add(iconLabel);
		}
	}

	private void OnThemeChanged(GameTheme theme) => ApplyTheme();

	#endregion

	#region Cleanup

	new public void RemoveFromHierarchy() {
		k_dismissSchedule?.Pause();
		GameTheme.OnThemeChanged -= OnThemeChanged;
		base.RemoveFromHierarchy();
	}

	#endregion
}

#region Enums

public enum ToastType {
	Info,
	Success,
	Warning,
	Error
}

#endregion
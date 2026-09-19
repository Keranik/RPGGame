using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Components;

#nullable disable

/// <summary>
/// A themed modal dialog component for confirmations, alerts, and custom content.
/// Supports various presets and custom actions.
/// 
/// <para>Usage with ModalManager (recommended):</para>
/// <code>
/// // Using ModalManager (auto-handles pause/resume)
/// modalManager.ShowConfirm("Delete?", "Are you sure?", () => Delete());
/// 
/// // Or with custom dialog
/// var dialog = new GameDialog()
///     .SetTitle("New Game")
///     .SetContent(characterCreator)
///     .AddAction("Cancel", DialogActionType.Secondary)
///     .AddAction("Create", DialogActionType.Primary, () => CreateCharacter())
///     .Build();
/// modalManager.ShowDialog(dialog);
/// </code>
/// 
/// <para>Manual usage (does NOT auto-pause):</para>
/// <code>
/// var dialog = GameDialog.Confirm("Delete?", "Are you sure?", () => Delete());
/// dialog.Show(rootElement);
/// </code>
/// </summary>
public class GameDialog : VisualElement {
	#region Private Fields

	private readonly VisualElement k_backdrop;
	private readonly VisualElement k_dialogContainer;
	private readonly VisualElement k_headerContainer;
	private readonly Label k_titleLabel;
	private readonly Button k_closeButton;
	private readonly VisualElement k_contentContainer;
	private readonly Label k_messageLabel;
	private readonly VisualElement k_footerContainer;

	private DialogSize k_size = DialogSize.Medium;
	private bool k_isClosable = true;
	private bool k_closeOnBackdropClick = true;
	private readonly List<Action> k_closeCallbacks = new();

	#endregion

	#region Constructors

	public GameDialog() {
		// Backdrop
		k_backdrop = new VisualElement {
			style = {
				position = Position.Absolute,
				left = 0, top = 0, right = 0, bottom = 0
			}
		};
		k_backdrop.RegisterCallback<ClickEvent>(OnBackdropClick);

		// Dialog container
		k_dialogContainer = new VisualElement {
			style = {
				position = Position.Absolute,
				flexDirection = FlexDirection.Column
			}
		};

		// Header
		k_headerContainer = new VisualElement {
			style = {
				flexDirection = FlexDirection.Row,
				justifyContent = Justify.SpaceBetween,
				alignItems = Align.Center
			}
		};

		k_titleLabel = new Label();

		k_closeButton = new Button { text = "✕" };
		k_closeButton.clicked += OnCloseClicked;
		k_closeButton.RemoveFromClassList("unity-button");

		k_headerContainer.Add(k_titleLabel);
		k_headerContainer.Add(k_closeButton);

		// Content
		k_contentContainer = new VisualElement {
			style = { flexGrow = 1 }
		};

		k_messageLabel = new Label {
			style = { display = DisplayStyle.None, whiteSpace = WhiteSpace.Normal }
		};
		k_contentContainer.Add(k_messageLabel);

		// Footer
		k_footerContainer = new VisualElement {
			style = {
				flexDirection = FlexDirection.Row,
				justifyContent = Justify.FlexEnd
			}
		};

		k_dialogContainer.Add(k_headerContainer);
		k_dialogContainer.Add(k_contentContainer);
		k_dialogContainer.Add(k_footerContainer);

		Add(k_backdrop);
		Add(k_dialogContainer);

		// Initially hidden
		style.display = DisplayStyle.None;
		style.position = Position.Absolute;
		style.left = 0;
		style.top = 0;
		style.right = 0;
		style.bottom = 0;

		GameTheme.OnThemeChanged += OnThemeChanged;
	}

	#endregion

	#region Static Factory Methods

	public static GameDialog Alert(string title, string message, Action onOk = null) {
		return new GameDialog()
			.SetTitle(title)
			.SetMessage(message)
			.AddAction("OK", DialogActionType.Primary, onOk)
			.SetClosable(true)
			.Build();
	}

	public static GameDialog Confirm(string title, string message, Action onConfirm, Action onCancel = null) {
		return new GameDialog()
			.SetTitle(title)
			.SetMessage(message)
			.AddAction("Cancel", DialogActionType.Secondary, onCancel)
			.AddAction("Confirm", DialogActionType.Primary, onConfirm)
			.SetClosable(true)
			.Build();
	}

	public static GameDialog ConfirmDanger(string title, string message, Action onConfirm, Action onCancel = null) {
		return new GameDialog()
			.SetTitle(title)
			.SetMessage(message)
			.AddAction("Cancel", DialogActionType.Secondary, onCancel)
			.AddAction("Delete", DialogActionType.Danger, onConfirm)
			.SetClosable(true)
			.Build();
	}

	public static GameDialog Custom(string title, VisualElement content) {
		return new GameDialog()
			.SetTitle(title)
			.SetContent(content)
			.SetClosable(true)
			.Build();
	}

	#endregion

	#region Fluent API - Content

	public GameDialog SetTitle(string title) {
		k_titleLabel.text = title;
		k_headerContainer.style.display = string.IsNullOrEmpty(title) ? DisplayStyle.None : DisplayStyle.Flex;
		return this;
	}

	public GameDialog SetMessage(string message) {
		k_messageLabel.text = message;
		k_messageLabel.style.display = string.IsNullOrEmpty(message) ? DisplayStyle.None : DisplayStyle.Flex;
		return this;
	}

	public GameDialog SetContent(VisualElement content) {
		k_contentContainer.Clear();
		if (content != null) {
			k_contentContainer.Add(content);
		}
		return this;
	}

	public GameDialog AddContent(VisualElement content) {
		k_contentContainer.Add(content);
		return this;
	}

	public VisualElement Content => k_contentContainer;

	#endregion

	#region Fluent API - Actions

	public GameDialog AddAction(string label, DialogActionType type, Action onClick = null) {
		var variant = type switch {
			DialogActionType.Primary => ButtonVariant.Primary,
			DialogActionType.Secondary => ButtonVariant.Outline,
			DialogActionType.Danger => ButtonVariant.Danger,
			DialogActionType.Ghost => ButtonVariant.Ghost,
			_ => ButtonVariant.Outline
		};

		var button = new GameButton(label)
			.SetVariant(variant)
			.OnClick(() => {
				onClick?.Invoke();
				if (type != DialogActionType.Ghost) {
					Close();
				}
			})
			.Build();

		k_footerContainer.Add(button);
		return this;
	}

	public GameDialog ClearActions() {
		k_footerContainer.Clear();
		return this;
	}

	#endregion

	#region Fluent API - Appearance

	public GameDialog SetSize(DialogSize size) {
		k_size = size;
		ApplyTheme();
		return this;
	}

	public GameDialog SetClosable(bool closable) {
		k_isClosable = closable;
		k_closeButton.style.display = closable ? DisplayStyle.Flex : DisplayStyle.None;
		return this;
	}

	public GameDialog SetCloseOnBackdropClick(bool closeOnClick) {
		k_closeOnBackdropClick = closeOnClick;
		return this;
	}

	/// <summary>
	/// Registers a callback to be invoked when the dialog closes.
	/// Supports multiple callbacks.
	/// </summary>
	public GameDialog OnClose(Action callback) {
		if (callback != null) {
			k_closeCallbacks.Add(callback);
		}
		return this;
	}

	#endregion

	#region Build

	public GameDialog Build() {
		ApplyTheme();
		return this;
	}

	#endregion

	#region Show/Hide

	/// <summary>
	/// Shows the dialog manually.
	/// Note: For automatic pause/resume, use ModalManager.ShowDialog() instead.
	/// </summary>
	public void Show(VisualElement parent = null) {
		if (parent != null && this.parent != parent) {
			parent.Add(this);
		}

		style.display = DisplayStyle.Flex;

		// Play sound
		if (GameTheme.Current.Audio.Enabled) {
			Debug.Log($"[Audio] Playing: {GameTheme.Current.Audio.DialogOpen}");
		}
	}

	/// <summary>
	/// Closes the dialog.
	/// </summary>
	public void Close() {
		style.display = DisplayStyle.None;

		// Invoke all registered close callbacks
		foreach (var callback in k_closeCallbacks) {
			try {
				callback?.Invoke();
			} catch (System.Exception ex) {
				Debug.LogError($"GameDialog: Error in close callback: {ex}");
			}
		}

		// Play sound
		if (GameTheme.Current.Audio.Enabled) {
			Debug.Log($"[Audio] Playing: {GameTheme.Current.Audio.DialogClose}");
		}
	}

	/// <summary>
	/// Whether the dialog is currently visible.
	/// </summary>
	public bool IsVisible => style.display == DisplayStyle.Flex;

	#endregion

	#region Theme Application

	private void ApplyTheme() {
		var theme = GameTheme.Current;
		var colors = theme.Colors;
		var borders = theme.Borders;
		var spacing = theme.Spacing;
		var typography = theme.Typography;
		var dialogStyles = theme.Components.Dialog;
		var windowStyles = theme.Components.Window;

		// Backdrop
		k_backdrop.style.backgroundColor = colors.BackgroundOverlay;

		// Dialog container
		float width = k_size switch {
			DialogSize.Small => dialogStyles.SmallWidth,
			DialogSize.Large => dialogStyles.LargeWidth,
			DialogSize.ExtraLarge => dialogStyles.ExtraLargeWidth,
			DialogSize.FullWidth => -1, // Handled below
			_ => dialogStyles.MinWidth
		};

		if (k_size == DialogSize.FullWidth) {
			k_dialogContainer.style.left = spacing.XL;
			k_dialogContainer.style.right = spacing.XL;
			k_dialogContainer.style.width = StyleKeyword.Auto;
		} else {
			k_dialogContainer.style.width = width;
			k_dialogContainer.style.left = Length.Percent(50);
			k_dialogContainer.style.marginLeft = -width / 2;
		}

		k_dialogContainer.style.top = Length.Percent(50);
		k_dialogContainer.style.marginTop = -dialogStyles.MinWidth / 2.5f;
		k_dialogContainer.style.maxWidth = dialogStyles.MaxWidth;
		k_dialogContainer.style.maxHeight = Length.Percent(90);

		k_dialogContainer.style.backgroundColor = colors.BackgroundElevated;
		borders.ApplyColor(k_dialogContainer.style, colors.SurfaceBorder);
		borders.ApplyWidth(k_dialogContainer.style, borders.WidthThin);
		borders.ApplyRadius(k_dialogContainer.style, borders.DialogRadius);
		spacing.DialogPadding.ApplyTo(k_dialogContainer.style);

		// Header
		typography.TitleLarge.ApplyTo(k_titleLabel.style);
		k_titleLabel.style.color = colors.TextPrimary;
		k_headerContainer.style.marginBottom = spacing.MD;

		// Close button
		k_closeButton.style.backgroundColor = colors.Transparent;
		k_closeButton.style.color = colors.TextSecondary;
		typography.TitleSmall.ApplyTo(k_closeButton.style);
		k_closeButton.style.width = windowStyles.CloseButtonSize;
		k_closeButton.style.height = windowStyles.CloseButtonSize;
		borders.ApplyRadius(k_closeButton.style, borders.RadiusFull);
		borders.ApplyWidth(k_closeButton.style, 0);

		// Message
		typography.BodyMedium.ApplyTo(k_messageLabel.style);
		k_messageLabel.style.color = colors.TextSecondary;

		// Footer
		k_footerContainer.style.marginTop = spacing.LG;

		// Action button spacing
		foreach (var child in k_footerContainer.Children()) {
			child.style.marginLeft = dialogStyles.ButtonSpacing;
		}
		if (k_footerContainer.childCount > 0) {
			k_footerContainer[0].style.marginLeft = 0;
		}
	}

	private void OnThemeChanged(GameTheme theme) => ApplyTheme();

	#endregion

	#region Event Handlers

	private void OnBackdropClick(ClickEvent evt) {
		if (k_closeOnBackdropClick && k_isClosable && evt.target == k_backdrop) {
			Close();
		}
	}

	private void OnCloseClicked() {
		if (k_isClosable) {
			Close();
		}
	}

	#endregion

	#region Cleanup

	new public void RemoveFromHierarchy() {
		GameTheme.OnThemeChanged -= OnThemeChanged;
		k_closeCallbacks.Clear();
		base.RemoveFromHierarchy();
	}

	#endregion
}

#region Enums

public enum DialogSize {
	Small,
	Medium,
	Large,
	ExtraLarge,
	FullWidth
}

public enum DialogActionType {
	Primary,
	Secondary,
	Danger,
	Ghost
}

#endregion
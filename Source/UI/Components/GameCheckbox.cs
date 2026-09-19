using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Components;

/// <summary>
/// Checkbox visual variants.
/// </summary>
public enum CheckboxVariant {
	Default,
	Primary,
	Secondary,
	Success
}

/// <summary>
/// Label position relative to checkbox.
/// </summary>
public enum LabelPosition {
	Left,
	Right
}

/// <summary>
/// A themed checkbox/toggle component with label support.
/// 
/// <para>Usage:</para>
/// <code>
/// var enableSound = new GameCheckbox()
///     .SetLabel("Enable Sound Effects")
///     .SetChecked(true)
///     .OnValueChanged(enabled => ToggleSound(enabled))
///     .Build();
/// </code>
/// </summary>
public class GameCheckbox : VisualElement {
	#region Private Fields

	private readonly VisualElement k_container;
	private readonly VisualElement k_checkboxContainer;
	private readonly VisualElement k_checkmark;
	private readonly Label k_label;
	private readonly Label k_helperLabel;

	private CheckboxVariant k_variant = CheckboxVariant.Default;
	private bool k_isChecked = false;
	private bool k_isIndeterminate = false;
	private LabelPosition k_labelPosition = LabelPosition.Right;

	private event Action<bool>? k_onValueChanged;

	#endregion

	#region Constructors

	public GameCheckbox() {
		k_container = new VisualElement {
			style = {
				flexDirection = FlexDirection.Column
			}
		};

		// Main row with checkbox and label
		var mainRow = new VisualElement {
			style = {
				flexDirection = FlexDirection.Row,
				alignItems = Align.Center
			}
		};

		// Checkbox box
		k_checkboxContainer = new VisualElement {
			style = {
				width = 20,
				height = 20,
				alignItems = Align.Center,
				justifyContent = Justify.Center
			}
		};

		// Checkmark
		k_checkmark = new VisualElement {
			style = {
				width = 12,
				height = 12,
				display = DisplayStyle.None
			}
		};

		k_checkboxContainer.Add(k_checkmark);

		// Label
		k_label = new Label {
			style = { display = DisplayStyle.None }
		};

		mainRow.Add(k_checkboxContainer);
		mainRow.Add(k_label);

		// Helper text
		k_helperLabel = new Label {
			style = { display = DisplayStyle.None }
		};

		k_container.Add(mainRow);
		k_container.Add(k_helperLabel);

		Add(k_container);

		// Register click handler
		k_checkboxContainer.RegisterCallback<ClickEvent>(OnClick);
		k_label.RegisterCallback<ClickEvent>(OnClick);

		// Hover states
		k_checkboxContainer.RegisterCallback<MouseEnterEvent>(OnMouseEnter);
		k_checkboxContainer.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);

		// Subscribe to theme changes
		GameTheme.OnThemeChanged += OnThemeChanged;
	}

	#endregion

	#region Fluent API - Content

	public GameCheckbox SetLabel(string label) {
		k_label.text = label;
		k_label.style.display = string.IsNullOrEmpty(label) ? DisplayStyle.None : DisplayStyle.Flex;
		ApplyTheme();
		return this;
	}

	public GameCheckbox SetLabelPosition(LabelPosition position) {
		k_labelPosition = position;
		var parent = k_checkboxContainer.parent;
		parent.Clear();

		if (position == LabelPosition.Left) {
			parent.Add(k_label);
			parent.Add(k_checkboxContainer);
		} else {
			parent.Add(k_checkboxContainer);
			parent.Add(k_label);
		}

		ApplyTheme();
		return this;
	}

	public GameCheckbox SetChecked(bool isChecked) {
		k_isChecked = isChecked;
		k_isIndeterminate = false;
		UpdateVisualState();
		return this;
	}

	public GameCheckbox SetIndeterminate(bool indeterminate = true) {
		k_isIndeterminate = indeterminate;
		UpdateVisualState();
		return this;
	}

	public bool IsChecked => k_isChecked;
	public bool IsIndeterminate => k_isIndeterminate;

	public GameCheckbox SetHelperText(string helperText) {
		k_helperLabel.text = helperText;
		k_helperLabel.style.display = string.IsNullOrEmpty(helperText) ? DisplayStyle.None : DisplayStyle.Flex;
		ApplyTheme();
		return this;
	}

	#endregion

	#region Fluent API - Appearance

	public GameCheckbox SetVariant(CheckboxVariant variant) {
		k_variant = variant;
		ApplyTheme();
		return this;
	}

	public GameCheckbox SetMargin(int all) {
		style.marginTop = all;
		style.marginBottom = all;
		style.marginLeft = all;
		style.marginRight = all;
		return this;
	}

	public GameCheckbox SetMargin(int vertical, int horizontal) {
		style.marginTop = vertical;
		style.marginBottom = vertical;
		style.marginLeft = horizontal;
		style.marginRight = horizontal;
		return this;
	}

	public GameCheckbox SetMargin(int top, int right, int bottom, int left) {
		style.marginTop = top;
		style.marginRight = right;
		style.marginBottom = bottom;
		style.marginLeft = left;
		return this;
	}

	public GameCheckbox SetMarginTop(int top) {
		style.marginTop = top;
		return this;
	}

	public GameCheckbox SetMarginRight(int right) {
		style.marginRight = right;
		return this;
	}

	public GameCheckbox SetMarginBottom(int bottom) {
		style.marginBottom = bottom;
		return this;
	}

	public GameCheckbox SetMarginLeft(int left) {
		style.marginLeft = left;
		return this;
	}

	#endregion

	#region Fluent API - Behavior

	public new GameCheckbox SetEnabled(bool enabled) {
		base.SetEnabled(enabled);
		ApplyTheme();
		return this;
	}

	#endregion

	#region Fluent API - Events

	public GameCheckbox OnValueChanged(Action<bool> callback) {
		k_onValueChanged += callback;
		return this;
	}

	#endregion

	#region Build

	public GameCheckbox Build() {
		ApplyTheme();
		UpdateVisualState();
		return this;
	}

	#endregion

	#region Theme Application

	private void ApplyTheme() {
		var theme = GameTheme.Current;
		var colors = theme.Colors;
		var borders = theme.Borders;
		var spacing = theme.Spacing;
		var typography = theme.Typography;

		bool isDisabled = !enabledSelf;

		// Get variant color
		Color accentColor = k_variant switch {
			CheckboxVariant.Primary => colors.Primary,
			CheckboxVariant.Secondary => colors.Secondary,
			CheckboxVariant.Success => colors.Success,
			_ => colors.Primary
		};

		if (isDisabled) {
			accentColor = colors.TextDisabled;
		}

		// Checkbox box styling
		var boxStyle = k_checkboxContainer.style;
		borders.ApplyRadius(boxStyle, borders.RadiusXS);
		borders.ApplyWidth(boxStyle, borders.WidthMedium);

		if (k_isChecked || k_isIndeterminate) {
			boxStyle.backgroundColor = accentColor;
			borders.ApplyColor(boxStyle, accentColor);
		} else {
			boxStyle.backgroundColor = colors.Transparent;
			borders.ApplyColor(boxStyle, isDisabled ? colors.TextDisabled : colors.InputBorder);
		}

		// Checkmark styling
		k_checkmark.style.backgroundColor = colors.White;

		// Label styling
		typography.BodyMedium.ApplyTo(k_label.style);
		k_label.style.color = isDisabled ? colors.TextDisabled : colors.TextPrimary;
		k_label.style.marginLeft = k_labelPosition == LabelPosition.Right ? spacing.XS : 0;
		k_label.style.marginRight = k_labelPosition == LabelPosition.Left ? spacing.XS : 0;

		// Helper text styling
		typography.Caption.ApplyTo(k_helperLabel.style);
		k_helperLabel.style.color = colors.TextSecondary;
		k_helperLabel.style.marginTop = spacing.XXS;
		k_helperLabel.style.marginLeft = k_labelPosition == LabelPosition.Right ? 28 : 0;
	}

	private void OnThemeChanged(GameTheme theme) {
		ApplyTheme();
	}

	#endregion

	#region Private Methods

	private void UpdateVisualState() {
		if (k_isIndeterminate) {
			// Show dash/line for indeterminate
			k_checkmark.style.display = DisplayStyle.Flex;
			k_checkmark.style.width = 10;
			k_checkmark.style.height = 2;
		} else if (k_isChecked) {
			// Show checkmark
			k_checkmark.style.display = DisplayStyle.Flex;
			k_checkmark.style.width = 12;
			k_checkmark.style.height = 12;
			// In a real implementation, you'd use an icon or custom drawing
		} else {
			k_checkmark.style.display = DisplayStyle.None;
		}

		ApplyTheme();
	}

	private void OnClick(ClickEvent evt) {
		if (!enabledSelf) {
			return;
		}

		k_isChecked = !k_isChecked;
		k_isIndeterminate = false;
		UpdateVisualState();
		k_onValueChanged?.Invoke(k_isChecked);

		// Play sound
		if (GameTheme.Current.Audio.Enabled) {
			string sound = k_isChecked 
				? GameTheme.Current.Audio.ToggleOn 
				: GameTheme.Current.Audio.ToggleOff;
			UnityEngine.Debug.Log($"[Audio] Playing: {sound}");
		}
	}

	private void OnMouseEnter(MouseEnterEvent evt) {
		if (!enabledSelf) {
			return;
		}

		var colors = GameTheme.Current.Colors;
		if (!k_isChecked && !k_isIndeterminate) {
			k_checkboxContainer.style.backgroundColor = ModifyAlpha(colors.Primary, 0.1f);
		}
	}

	private void OnMouseLeave(MouseLeaveEvent evt) {
		ApplyTheme();
	}

	private static Color ModifyAlpha(Color color, float alpha) {
		return new Color(color.r, color.g, color.b, alpha);
	}

	#endregion

	#region Cleanup

	public new void RemoveFromHierarchy() {
		GameTheme.OnThemeChanged -= OnThemeChanged;
		base.RemoveFromHierarchy();
	}

	#endregion
}
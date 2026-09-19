using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Components;

#nullable disable

/// <summary>
/// A themed dropdown/select component with label and customizable options.
/// </summary>
public class GameDropdown : VisualElement {
	#region Private Fields

	private readonly VisualElement k_container;
	private readonly Label k_label;
	private readonly DropdownField k_dropdown;
	private readonly Label k_helperLabel;

	private DropdownVariant k_variant = DropdownVariant.Outlined;
	private bool k_isRequired = false;

	#endregion

	#region Constructors

	public GameDropdown() {
		k_container = new VisualElement {
			style = { flexDirection = FlexDirection.Column }
		};

		// Label
		k_label = new Label {
			style = { display = DisplayStyle.None }
		};

		// Dropdown field - don't remove the default class, just add our styling
		k_dropdown = new DropdownField();

		// Helper text
		k_helperLabel = new Label {
			style = { display = DisplayStyle.None }
		};

		k_container.Add(k_label);
		k_container.Add(k_dropdown);
		k_container.Add(k_helperLabel);

		Add(k_container);

		// Register callbacks
		k_dropdown.RegisterCallback<FocusInEvent>(OnFocusIn);
		k_dropdown.RegisterCallback<FocusOutEvent>(OnFocusOut);

		// Subscribe to theme changes
		GameTheme.OnThemeChanged += OnThemeChanged;
	}

	#endregion

	#region Fluent API - Content

	public GameDropdown SetLabel(string label) {
		k_label.text = label;
		k_label.style.display = string.IsNullOrEmpty(label) ? DisplayStyle.None : DisplayStyle.Flex;
		ApplyTheme();
		return this;
	}

	public GameDropdown SetOptions(IList<string> options) {
		k_dropdown.choices = options.ToList();
		if (options.Count > 0 && k_dropdown.index < 0) {
			k_dropdown.index = 0;
		}
		return this;
	}

	public GameDropdown SetOptions(params string[] options) {
		return SetOptions(options.ToList());
	}

	public GameDropdown SetSelectedIndex(int index) {
		if (k_dropdown.choices != null && index >= 0 && index < k_dropdown.choices.Count) {
			k_dropdown.index = index;
		}
		return this;
	}

	public GameDropdown SetSelectedValue(string value) {
		k_dropdown.value = value;
		return this;
	}

	public int SelectedIndex => k_dropdown.index;
	public string SelectedValue => k_dropdown.value;

	public GameDropdown SetHelperText(string helperText) {
		k_helperLabel.text = helperText;
		k_helperLabel.style.display = string.IsNullOrEmpty(helperText) ? DisplayStyle.None : DisplayStyle.Flex;
		ApplyTheme();
		return this;
	}

	public GameDropdown SetPlaceholder(string placeholder) {
		if (k_dropdown.choices == null || k_dropdown.choices.Count == 0 || k_dropdown.index < 0) {
			// Insert placeholder as first item or use label
			k_dropdown.value = placeholder;
		}
		return this;
	}

	#endregion

	#region Fluent API - Appearance

	public GameDropdown SetVariant(DropdownVariant variant) {
		k_variant = variant;
		ApplyTheme();
		return this;
	}

	public GameDropdown SetWidth(float width) {
		style.width = width;
		return this;
	}

	public GameDropdown SetWidth(Length width) {
		style.width = width;
		return this;
	}

	public GameDropdown SetFullWidth(bool fullWidth = true) {
		style.width = fullWidth ? Length.Percent(100) : StyleKeyword.Auto;
		return this;
	}

	public GameDropdown SetMargin(int top = 0, int right = 0, int bottom = 0, int left = 0) {
		style.marginTop = top;
		style.marginRight = right;
		style.marginBottom = bottom;
		style.marginLeft = left;
		return this;
	}

	#endregion

	#region Fluent API - Behavior

	public GameDropdown SetRequired(bool required = true) {
		k_isRequired = required;
		if (required && k_label.style.display == DisplayStyle.Flex) {
			k_label.text = k_label.text.TrimEnd('*', ' ') + " *";
		}
		return this;
	}

	new public GameDropdown SetEnabled(bool enabled) {
		k_dropdown.SetEnabled(enabled);
		ApplyTheme();
		return this;
	}

	#endregion

	#region Fluent API - Events

	private Action<string> k_onValueChanged;
	private Action<int> k_onIndexChanged;

	public GameDropdown OnValueChanged(Action<string> callback) {
		k_onValueChanged = callback;
		k_dropdown.RegisterValueChangedCallback(evt => k_onValueChanged?.Invoke(evt.newValue));
		return this;
	}

	public GameDropdown OnIndexChanged(Action<int> callback) {
		k_onIndexChanged = callback;
		k_dropdown.RegisterValueChangedCallback(evt => k_onIndexChanged?.Invoke(k_dropdown.index));
		return this;
	}

	#endregion

	#region Build

	public GameDropdown Build() {
		ApplyTheme();
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
		var components = theme.Components.Input;

		bool isDisabled = !k_dropdown.enabledSelf;

		// Label styling
		typography.LabelMedium.ApplyTo(k_label.style);
		k_label.style.color = isDisabled ? colors.TextDisabled : colors.TextSecondary;
		k_label.style.marginBottom = components.LabelSpacing;

		// Dropdown container styling
		k_dropdown.style.minWidth = components.MinWidth;

		// Variant-specific styling
		Color bgColor = isDisabled ? colors.BackgroundTertiary : colors.InputBackground;
		Color borderColor = colors.InputBorder;

		switch (k_variant) {
			case DropdownVariant.Filled:
				bgColor = isDisabled ? colors.BackgroundTertiary : colors.BackgroundSecondary;
				k_dropdown.style.borderBottomWidth = borders.WidthMedium;
				k_dropdown.style.borderBottomColor = borderColor;
				borders.ApplyRadius(k_dropdown.style, borders.InputRadius);
				break;

			default: // Outlined
				borders.ApplyColor(k_dropdown.style, borderColor);
				borders.ApplyWidth(k_dropdown.style, borders.WidthThin);
				borders.ApplyRadius(k_dropdown.style, borders.InputRadius);
				break;
		}

		k_dropdown.style.backgroundColor = bgColor;

		// Style the internal elements
		// The dropdown has a label inside that displays the selected value
		var dropdownInput = k_dropdown.Q<VisualElement>(className: "unity-base-popup-field__input");
		if (dropdownInput != null) {
			dropdownInput.style.backgroundColor = Color.clear;
			dropdownInput.style.paddingTop = spacing.XS;
			dropdownInput.style.paddingBottom = spacing.XS;
			dropdownInput.style.paddingLeft = spacing.SM;
			dropdownInput.style.paddingRight = spacing.SM;
			dropdownInput.style.minHeight = components.Height;
		}

		// Style the text label inside the dropdown
		var textElement = k_dropdown.Q<TextElement>(className: "unity-base-popup-field__text");
		if (textElement != null) {
			textElement.style.color = isDisabled ? colors.TextDisabled : colors.TextPrimary;
			typography.BodyMedium.ApplyTo(textElement.style);
		}

		// Style the arrow
		var arrow = k_dropdown.Q<VisualElement>(className: "unity-base-popup-field__arrow");
		if (arrow != null) {
			arrow.style.unityBackgroundImageTintColor = isDisabled ? colors.TextDisabled : colors.TextSecondary;
		}

		// Helper text styling
		typography.Caption.ApplyTo(k_helperLabel.style);
		k_helperLabel.style.color = colors.TextSecondary;
		k_helperLabel.style.marginTop = components.HelperTextSpacing;
	}

	private void OnThemeChanged(GameTheme theme) {
		ApplyTheme();
	}

	private void OnFocusIn(FocusInEvent evt) {
		var colors = GameTheme.Current.Colors;
		GameTheme.Current.Borders.ApplyColor(k_dropdown.style, colors.InputBorderFocus);
	}

	private void OnFocusOut(FocusOutEvent evt) {
		ApplyTheme();
	}

	#endregion

	#region Cleanup

	new public void RemoveFromHierarchy() {
		GameTheme.OnThemeChanged -= OnThemeChanged;
		base.RemoveFromHierarchy();
	}

	#endregion
}

#region Enums

public enum DropdownVariant {
	Outlined,
	Filled
}

#endregion
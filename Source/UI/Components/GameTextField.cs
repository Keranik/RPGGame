using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Components;

#nullable disable

/// <summary>
/// A themed text input field with label, placeholder, helper text, and validation support.
/// </summary>
public class GameTextField : VisualElement {
	#region Private Fields

	private readonly VisualElement k_container;
	private readonly Label k_label;
	private readonly TextField k_textField;
	private readonly Label k_helperLabel;
	private readonly Label k_errorLabel;

	private TextFieldVariant k_variant = TextFieldVariant.Outlined;
	private bool k_isRequired = false;
	private bool k_hasError = false;
	private string k_errorMessage = string.Empty;
	private int k_maxLength = 0;
	private Func<string, string> k_validator;
	private string k_placeholderText = string.Empty;

	#endregion

	#region Constructors

	public GameTextField() {
		k_container = new VisualElement {
			style = { flexDirection = FlexDirection.Column }
		};

		// Label
		k_label = new Label {
			style = { display = DisplayStyle.None }
		};

		// Text field
		k_textField = new TextField();

		// Helper text
		k_helperLabel = new Label {
			style = { display = DisplayStyle.None }
		};

		// Error text
		k_errorLabel = new Label {
			style = { display = DisplayStyle.None }
		};

		k_container.Add(k_label);
		k_container.Add(k_textField);
		k_container.Add(k_helperLabel);
		k_container.Add(k_errorLabel);

		Add(k_container);

		// Register callbacks
		k_textField.RegisterValueChangedCallback(OnValueChangedInternal);
		k_textField.RegisterCallback<FocusInEvent>(OnFocusIn);
		k_textField.RegisterCallback<FocusOutEvent>(OnFocusOut);

		// Subscribe to theme changes
		GameTheme.OnThemeChanged += OnThemeChanged;
	}

	#endregion

	#region Fluent API - Content

	public GameTextField SetLabel(string label) {
		k_label.text = label;
		k_label.style.display = string.IsNullOrEmpty(label) ? DisplayStyle.None : DisplayStyle.Flex;
		ApplyTheme();
		return this;
	}

	public GameTextField SetPlaceholder(string placeholder) {
		k_placeholderText = placeholder;
		
		// Use Unity's built-in placeholder via the textEdition if available,
		// otherwise handle manually
		if (string.IsNullOrEmpty(k_textField.value)) {
			k_textField.value = placeholder;
			k_textField.AddToClassList("placeholder");
			ApplyPlaceholderStyle(true);
		}
		
		return this;
	}

	public GameTextField SetValue(string value) {
		k_textField.RemoveFromClassList("placeholder");
		k_textField.value = value;
		ApplyPlaceholderStyle(false);
		return this;
	}

	public string Value {
		get {
			if (k_textField.ClassListContains("placeholder")) {
				return string.Empty;
			}
			return k_textField.value;
		}
	}

	public GameTextField SetHelperText(string helperText) {
		k_helperLabel.text = helperText;
		k_helperLabel.style.display = string.IsNullOrEmpty(helperText) ? DisplayStyle.None : DisplayStyle.Flex;
		ApplyTheme();
		return this;
	}

	#endregion

	#region Fluent API - Appearance

	public GameTextField SetVariant(TextFieldVariant variant) {
		k_variant = variant;
		ApplyTheme();
		return this;
	}

	public GameTextField SetWidth(float width) {
		style.width = width;
		return this;
	}

	public GameTextField SetWidth(Length width) {
		style.width = width;
		return this;
	}

	public GameTextField SetFullWidth(bool fullWidth = true) {
		style.width = fullWidth ? Length.Percent(100) : StyleKeyword.Auto;
		return this;
	}

	public GameTextField SetMargin(int top = 0, int right = 0, int bottom = 0, int left = 0) {
		style.marginTop = top;
		style.marginRight = right;
		style.marginBottom = bottom;
		style.marginLeft = left;
		return this;
	}

	#endregion

	#region Fluent API - Behavior

	public GameTextField SetRequired(bool required = true) {
		k_isRequired = required;
		if (required && k_label.style.display == DisplayStyle.Flex) {
			k_label.text = k_label.text.TrimEnd('*') + " *";
		}
		return this;
	}

	public GameTextField SetMaxLength(int maxLength) {
		k_maxLength = maxLength;
		k_textField.maxLength = maxLength;
		return this;
	}

	public GameTextField SetMultiline(bool multiline = true) {
		k_textField.multiline = multiline;
		if (multiline) {
			k_textField.style.height = 100;
			k_textField.style.whiteSpace = WhiteSpace.Normal;
		}
		return this;
	}

	public GameTextField SetPassword(bool isPassword = true) {
		k_textField.isPasswordField = isPassword;
		return this;
	}

	public GameTextField SetReadOnly(bool readOnly = true) {
		k_textField.isReadOnly = readOnly;
		ApplyTheme();
		return this;
	}

	new public GameTextField SetEnabled(bool enabled) {
		k_textField.SetEnabled(enabled);
		ApplyTheme();
		return this;
	}

	public GameTextField SetValidator(Func<string, string> validator) {
		k_validator = validator;
		return this;
	}

	#endregion

	#region Fluent API - Events

	private Action<string> k_onValueChanged;

	public GameTextField OnValueChanged(Action<string> callback) {
		k_onValueChanged = callback;
		return this;
	}

	private Action<string> k_onSubmit;

	public GameTextField OnSubmit(Action<string> callback) {
		k_onSubmit = callback;
		k_textField.RegisterCallback<KeyDownEvent>(evt => {
			if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter) {
				if (!k_textField.multiline) {
					k_onSubmit?.Invoke(Value);
				}
			}
		});
		return this;
	}

	#endregion

	#region Validation

	public GameTextField SetError(string errorMessage) {
		k_hasError = !string.IsNullOrEmpty(errorMessage);
		k_errorMessage = errorMessage;
		k_errorLabel.text = errorMessage;
		k_errorLabel.style.display = k_hasError ? DisplayStyle.Flex : DisplayStyle.None;
		k_helperLabel.style.display = k_hasError ? DisplayStyle.None : 
			(string.IsNullOrEmpty(k_helperLabel.text) ? DisplayStyle.None : DisplayStyle.Flex);
		ApplyTheme();
		return this;
	}

	public GameTextField ClearError() {
		return SetError(string.Empty);
	}

	public bool Validate() {
		string value = Value;

		if (k_isRequired && string.IsNullOrEmpty(value)) {
			SetError("This field is required");
			return false;
		}

		if (k_validator != null) {
			string error = k_validator(value);
			if (!string.IsNullOrEmpty(error)) {
				SetError(error);
				return false;
			}
		}

		ClearError();
		return true;
	}

	#endregion

	#region Build

	public GameTextField Build() {
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

		bool isDisabled = !k_textField.enabledSelf;

		// Label styling
		typography.LabelMedium.ApplyTo(k_label.style);
		k_label.style.color = isDisabled ? colors.TextDisabled : colors.TextSecondary;
		k_label.style.marginBottom = components.LabelSpacing;

		// Get the TextInput element inside the TextField
		var textInput = k_textField.Q<VisualElement>("unity-text-input");

		// TextField container styling
		k_textField.style.minWidth = components.MinWidth;

		// Variant-specific styling
		Color bgColor, borderColor;
		float borderWidth;

		switch (k_variant) {
			case TextFieldVariant.Filled:
				bgColor = isDisabled ? colors.BackgroundTertiary : colors.BackgroundSecondary;
				borderColor = colors.Transparent;
				borderWidth = 0f;
				borders.ApplyRadius(k_textField.style, borders.InputRadius);
				k_textField.style.borderBottomWidth = borders.WidthMedium;
				k_textField.style.borderBottomColor = k_hasError ? colors.Error : colors.InputBorder;
				break;

			case TextFieldVariant.Standard:
				bgColor = colors.Transparent;
				borderColor = colors.Transparent;
				borderWidth = 0f;
				k_textField.style.borderBottomWidth = borders.WidthThin;
				k_textField.style.borderBottomColor = k_hasError ? colors.Error : colors.InputBorder;
				break;

			default: // Outlined
				bgColor = isDisabled ? colors.BackgroundTertiary : colors.InputBackground;
				borderColor = k_hasError ? colors.Error : colors.InputBorder;
				borderWidth = borders.WidthThin;
				borders.ApplyRadius(k_textField.style, borders.InputRadius);
				borders.ApplyColor(k_textField.style, borderColor);
				borders.ApplyWidth(k_textField.style, borderWidth);
				break;
		}

		k_textField.style.backgroundColor = bgColor;

		// Style the actual text input element
		if (textInput != null) {
			textInput.style.backgroundColor = Color.clear;
			textInput.style.borderTopWidth = 0;
			textInput.style.borderBottomWidth = 0;
			textInput.style.borderLeftWidth = 0;
			textInput.style.borderRightWidth = 0;
			textInput.style.marginTop = 0;
			textInput.style.marginBottom = 0;
			textInput.style.marginLeft = 0;
			textInput.style.marginRight = 0;
			textInput.style.paddingTop = spacing.XS;
			textInput.style.paddingBottom = spacing.XS;
			textInput.style.paddingLeft = spacing.SM;
			textInput.style.paddingRight = spacing.SM;
			textInput.style.minHeight = components.Height;

			// Text color
			bool isPlaceholder = k_textField.ClassListContains("placeholder");
			textInput.style.color = isDisabled 
				? colors.TextDisabled 
				: (isPlaceholder ? colors.InputPlaceholder : colors.TextPrimary);
		}

		// Helper text styling
		typography.Caption.ApplyTo(k_helperLabel.style);
		k_helperLabel.style.color = colors.TextSecondary;
		k_helperLabel.style.marginTop = components.HelperTextSpacing;

		// Error text styling
		typography.Caption.ApplyTo(k_errorLabel.style);
		k_errorLabel.style.color = colors.Error;
		k_errorLabel.style.marginTop = components.HelperTextSpacing;
	}

	private void ApplyPlaceholderStyle(bool isPlaceholder) {
		var textInput = k_textField.Q<VisualElement>("unity-text-input");
		if (textInput != null) {
			var colors = GameTheme.Current.Colors;
			textInput.style.color = isPlaceholder ? colors.InputPlaceholder : colors.TextPrimary;
		}
	}

	private void OnThemeChanged(GameTheme theme) {
		ApplyTheme();
	}

	#endregion

	#region Event Handlers

	private void OnValueChangedInternal(ChangeEvent<string> evt) {
		// Handle placeholder state
		if (k_textField.ClassListContains("placeholder") && evt.newValue != k_placeholderText) {
			k_textField.RemoveFromClassList("placeholder");
			ApplyPlaceholderStyle(false);
		}

		if (!k_textField.ClassListContains("placeholder")) {
			k_onValueChanged?.Invoke(evt.newValue);

			if (k_hasError) {
				Validate();
			}
		}
	}

	private void OnFocusIn(FocusInEvent evt) {
		// Clear placeholder on focus
		if (k_textField.ClassListContains("placeholder")) {
			k_textField.SetValueWithoutNotify(string.Empty);
			k_textField.RemoveFromClassList("placeholder");
			ApplyPlaceholderStyle(false);
		}

		if (k_variant == TextFieldVariant.Outlined && !k_hasError) {
			GameTheme.Current.Borders.ApplyColor(k_textField.style, GameTheme.Current.Colors.InputBorderFocus);
		}
	}

	private void OnFocusOut(FocusOutEvent evt) {
		// Restore placeholder if empty
		if (string.IsNullOrEmpty(k_textField.value) && !string.IsNullOrEmpty(k_placeholderText)) {
			k_textField.SetValueWithoutNotify(k_placeholderText);
			k_textField.AddToClassList("placeholder");
			ApplyPlaceholderStyle(true);
		}

		ApplyTheme();

		if (k_isRequired || k_validator != null) {
			Validate();
		}
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

public enum TextFieldVariant {
	Outlined,
	Filled,
	Standard
}

#endregion
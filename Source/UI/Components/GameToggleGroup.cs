using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Components;

#nullable disable

/// <summary>
/// A themed toggle group (radio buttons) for single-select options.
/// 
/// <para>Usage:</para>
/// <code>
/// var difficulty = new GameToggleGroup()
///     .AddOption("Easy", "easy")
///     .AddOption("Normal", "normal")
///     .AddOption("Hard", "hard")
///     .SetSelected("normal")
///     .OnSelectionChanged(value => SetDifficulty(value))
///     .Build();
/// </code>
/// </summary>
public class GameToggleGroup : VisualElement {
	#region Private Fields

	private readonly VisualElement k_container;
	private readonly List<(string label, string value, VisualElement element)> k_options = new();
	private string k_selectedValue;
	private ToggleGroupLayout k_layout = ToggleGroupLayout.Vertical;
	private ToggleGroupVariant k_variant = ToggleGroupVariant.Default;

	private Action<string> k_onSelectionChanged;

	#endregion

	#region Constructors

	public GameToggleGroup() {
		k_container = new VisualElement {
			style = { flexDirection = FlexDirection.Column }
		};

		Add(k_container);

		GameTheme.OnThemeChanged += OnThemeChanged;
	}

	#endregion

	#region Fluent API

	public GameToggleGroup AddOption(string label, string value) {
		var option = CreateOption(label, value);
		k_options.Add((label, value, option));
		k_container.Add(option);
		return this;
	}

	public GameToggleGroup SetSelected(string value) {
		k_selectedValue = value;
		UpdateSelectionVisuals();
		return this;
	}

	public GameToggleGroup SetLayout(ToggleGroupLayout layout) {
		k_layout = layout;
		k_container.style.flexDirection = layout == ToggleGroupLayout.Horizontal 
			? FlexDirection.Row 
			: FlexDirection.Column;
		return this;
	}

	public GameToggleGroup SetVariant(ToggleGroupVariant variant) {
		k_variant = variant;
		ApplyTheme();
		return this;
	}

	public GameToggleGroup OnSelectionChanged(Action<string> callback) {
		k_onSelectionChanged = callback;
		return this;
	}

	public string SelectedValue => k_selectedValue;

	#endregion

	#region Build

	public GameToggleGroup Build() {
		ApplyTheme();
		return this;
	}

	#endregion

	#region Option Management

	private VisualElement CreateOption(string label, string value) {
		var option = new VisualElement {
			style = {
				flexDirection = FlexDirection.Row,
				alignItems = Align.Center
			}
		};

		var radio = new VisualElement {
			name = "radio"
		};

		var dot = new VisualElement {
			name = "dot",
			style = { display = DisplayStyle.None }
		};
		radio.Add(dot);

		var labelElement = new Label(label) {
			name = "label"
		};

		option.Add(radio);
		option.Add(labelElement);

		option.RegisterCallback<ClickEvent>(evt => SelectOption(value));

		return option;
	}

	private void SelectOption(string value) {
		if (k_selectedValue == value) {
			return;
		}

		k_selectedValue = value;
		UpdateSelectionVisuals();
		k_onSelectionChanged?.Invoke(value);
	}

	private void UpdateSelectionVisuals() {
		foreach (var (_, value, element) in k_options) {
			bool isSelected = value == k_selectedValue;
			var dot = element.Q<VisualElement>("dot");
			if (dot != null) {
				dot.style.display = isSelected ? DisplayStyle.Flex : DisplayStyle.None;
			}
			ApplyOptionStyle(element, isSelected);
		}
	}

	// Update ApplyOptionStyle to include typography:
	private void ApplyOptionStyle(VisualElement option, bool isSelected) {
		var theme = GameTheme.Current;
		var colors = theme.Colors;
		var borders = theme.Borders;
		var typography = theme.Typography;

		var radio = option.Q<VisualElement>("radio");
		var dot = option.Q<VisualElement>("dot");
		var label = option.Q<Label>("label");

		// Apply typography to label
		if (label != null) {
			typography.BodyMedium.ApplyTo(label.style);
		}

		if (radio != null) {
			radio.style.width = 20;
			radio.style.height = 20;
			radio.style.marginRight = 8;
			radio.style.alignItems = Align.Center;
			radio.style.justifyContent = Justify.Center;

			if (k_variant == ToggleGroupVariant.Buttons) {
				// Button style - no radio circle
				radio.style.display = DisplayStyle.None;
				option.style.backgroundColor = isSelected ? colors.Primary : colors.Surface;
				option.style.paddingTop = 8;
				option.style.paddingBottom = 8;
				option.style.paddingLeft = 16;
				option.style.paddingRight = 16;
				borders.ApplyRadius(option.style, borders.ButtonRadius);
				borders.ApplyWidth(option.style, borders.WidthThin);
				borders.ApplyColor(option.style, isSelected ? colors.Primary : colors.SurfaceBorder);

				if (label != null) {
					label.style.color = isSelected ? colors.White : colors.TextPrimary;
				}
			} else {
				radio.style.display = DisplayStyle.Flex;
				borders.ApplyRadius(radio.style, borders.RadiusFull);
				borders.ApplyWidth(radio.style, borders.WidthMedium);
				borders.ApplyColor(radio.style, isSelected ? colors.Primary : colors.InputBorder);

				if (dot != null) {
					dot.style.width = 10;
					dot.style.height = 10;
					dot.style.backgroundColor = colors.Primary;
					borders.ApplyRadius(dot.style, borders.RadiusFull);
				}

				if (label != null) {
					label.style.color = colors.TextPrimary;
				}
			}
		}
	}

	#endregion

	#region Theme

	private void ApplyTheme() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		foreach (var (_, _, element) in k_options) {
			bool isSelected = k_options.FindIndex(o => o.element == element) >= 0 
				&& k_options.Find(o => o.element == element).value == k_selectedValue;

			if (k_layout == ToggleGroupLayout.Vertical) {
				element.style.marginBottom = spacing.XS;
			} else {
				element.style.marginRight = spacing.SM;
			}

			ApplyOptionStyle(element, isSelected);
		}
	}

	private void OnThemeChanged(GameTheme theme) => ApplyTheme();

	#endregion

	#region Cleanup

	new public void RemoveFromHierarchy() {
		GameTheme.OnThemeChanged -= OnThemeChanged;
		base.RemoveFromHierarchy();
	}

	#endregion
}

#region Enums

public enum ToggleGroupLayout {
	Vertical,
	Horizontal
}

public enum ToggleGroupVariant {
	Default,
	Buttons
}

#endregion
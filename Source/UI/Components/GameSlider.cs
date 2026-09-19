using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Components;

#nullable disable

/// <summary>
/// A themed slider component with label, value display, and customizable range.
/// 
/// <para>Usage:</para>
/// <code>
/// var volumeSlider = new GameSlider()
///     .SetLabel("Master Volume")
///     .SetRange(0, 100)
///     .SetValue(75)
///     .SetShowValue(true)
///     .SetValueFormat("{0}%")
///     .OnValueChanged(value => SetVolume(value))
///     .Build();
/// </code>
/// </summary>
public class GameSlider : VisualElement {
	#region Private Fields

	private readonly VisualElement k_container;
	private readonly VisualElement k_headerContainer;
	private readonly Label k_label;
	private readonly Label k_valueLabel;
	private readonly Slider k_slider;
	private readonly Label k_helperLabel;

	private SliderVariant k_variant = SliderVariant.Default;
	private bool k_showValue = false;
	private string k_valueFormat = "{0:F0}";

	#endregion

	#region Constructors

	public GameSlider() {
		k_container = new VisualElement {
			style = { flexDirection = FlexDirection.Column }
		};

		// Header with label and value
		k_headerContainer = new VisualElement {
			style = {
				flexDirection = FlexDirection.Row,
				justifyContent = Justify.SpaceBetween,
				alignItems = Align.Center,
				display = DisplayStyle.None
			}
		};

		k_label = new Label();
		k_valueLabel = new Label {
			style = { display = DisplayStyle.None }
		};

		k_headerContainer.Add(k_label);
		k_headerContainer.Add(k_valueLabel);

		// Slider
		k_slider = new Slider();
		k_slider.RemoveFromClassList("unity-slider");

		// Helper text
		k_helperLabel = new Label {
			style = { display = DisplayStyle.None }
		};

		k_container.Add(k_headerContainer);
		k_container.Add(k_slider);
		k_container.Add(k_helperLabel);

		Add(k_container);

		// Register value change callback
		k_slider.RegisterValueChangedCallback(OnSliderValueChanged);

		// Subscribe to theme changes
		GameTheme.OnThemeChanged += OnThemeChanged;
	}

	#endregion

	#region Fluent API - Content

	public GameSlider SetLabel(string label) {
		k_label.text = label;
		UpdateHeaderVisibility();
		ApplyTheme();
		return this;
	}

	public GameSlider SetRange(float min, float max) {
		k_slider.lowValue = min;
		k_slider.highValue = max;
		UpdateValueLabel();
		return this;
	}

	public GameSlider SetValue(float value) {
		k_slider.value = Mathf.Clamp(value, k_slider.lowValue, k_slider.highValue);
		UpdateValueLabel();
		return this;
	}

	public float Value => k_slider.value;
	public float MinValue => k_slider.lowValue;
	public float MaxValue => k_slider.highValue;

	public GameSlider SetStep(float step) {
		// Unity Slider doesn't have native step, but we can round on change
		k_slider.RegisterValueChangedCallback(evt => {
			if (step > 0) {
				float rounded = Mathf.Round(evt.newValue / step) * step;
				if (!Mathf.Approximately(evt.newValue, rounded)) {
					k_slider.SetValueWithoutNotify(rounded);
				}
			}
		});
		return this;
	}

	public GameSlider SetShowValue(bool show = true) {
		k_showValue = show;
		k_valueLabel.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
		UpdateHeaderVisibility();
		UpdateValueLabel();
		return this;
	}

	public GameSlider SetValueFormat(string format) {
		k_valueFormat = format;
		UpdateValueLabel();
		return this;
	}

	public GameSlider SetHelperText(string helperText) {
		k_helperLabel.text = helperText;
		k_helperLabel.style.display = string.IsNullOrEmpty(helperText) ? DisplayStyle.None : DisplayStyle.Flex;
		ApplyTheme();
		return this;
	}

	#endregion

	#region Fluent API - Appearance

	public GameSlider SetVariant(SliderVariant variant) {
		k_variant = variant;
		ApplyTheme();
		return this;
	}

	public GameSlider SetTrackColor(Color color) {
		// Applied to the track element
		var track = k_slider.Q<VisualElement>("unity-tracker");
		if (track != null) {
			track.style.backgroundColor = color;
		}
		return this;
	}

	public GameSlider SetFillColor(Color color) {
		var fill = k_slider.Q<VisualElement>("unity-dragger-border");
		if (fill != null) {
			fill.style.backgroundColor = color;
		}
		return this;
	}

	public GameSlider SetWidth(float width) {
		style.width = width;
		return this;
	}

	public GameSlider SetWidth(Length width) {
		style.width = width;
		return this;
	}

	public GameSlider SetFullWidth(bool fullWidth = true) {
		style.width = fullWidth ? Length.Percent(100) : StyleKeyword.Auto;
		return this;
	}

	public GameSlider SetMargin(int top = 0, int right = 0, int bottom = 0, int left = 0) {
		style.marginTop = top;
		style.marginRight = right;
		style.marginBottom = bottom;
		style.marginLeft = left;
		return this;
	}

	#endregion

	#region Fluent API - Behavior

	new public GameSlider SetEnabled(bool enabled) {
		k_slider.SetEnabled(enabled);
		ApplyTheme();
		return this;
	}

	#endregion

	#region Fluent API - Events

	private event Action<float> k_onValueChanged;

	public GameSlider OnValueChanged(Action<float> callback) {
		k_onValueChanged += callback;
		return this;
	}

	#endregion

	#region Build

	public GameSlider Build() {
		ApplyTheme();
		UpdateValueLabel();
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

		bool isDisabled = !k_slider.enabledSelf;

		// Label styling
		typography.LabelMedium.ApplyTo(k_label.style);
		k_label.style.color = isDisabled ? colors.TextDisabled : colors.TextSecondary;

		// Value label styling
		typography.LabelMedium.ApplyTo(k_valueLabel.style);
		k_valueLabel.style.color = isDisabled ? colors.TextDisabled : colors.TextPrimary;

		// Header margin
		if (k_headerContainer.style.display == DisplayStyle.Flex) {
			k_headerContainer.style.marginBottom = spacing.XS;
		}

		// Slider track styling
		var sliderStyle = k_slider.style;
		sliderStyle.height = 24;

		// Get variant colors
		Color trackColor, fillColor, thumbColor;
		switch (k_variant) {
			case SliderVariant.Primary:
				trackColor = colors.BackgroundTertiary;
				fillColor = colors.Primary;
				thumbColor = colors.Primary;
				break;
			case SliderVariant.Secondary:
				trackColor = colors.BackgroundTertiary;
				fillColor = colors.Secondary;
				thumbColor = colors.Secondary;
				break;
			case SliderVariant.Success:
				trackColor = colors.BackgroundTertiary;
				fillColor = colors.Success;
				thumbColor = colors.Success;
				break;
			default:
				trackColor = colors.BackgroundTertiary;
				fillColor = colors.Primary;
				thumbColor = colors.TextPrimary;
				break;
		}

		if (isDisabled) {
			trackColor = colors.BackgroundSecondary;
			fillColor = colors.TextDisabled;
			thumbColor = colors.TextDisabled;
		}

		// Apply to track (these need to be applied after the element is rendered)
		k_slider.schedule.Execute(() => {
			var track = k_slider.Q<VisualElement>("unity-tracker");
			if (track != null) {
				track.style.backgroundColor = trackColor;
				track.style.height = 4;
				borders.ApplyRadius(track.style, borders.RadiusFull);
			}

			var dragger = k_slider.Q<VisualElement>("unity-dragger");
			if (dragger != null) {
				dragger.style.backgroundColor = thumbColor;
				dragger.style.width = 16;
				dragger.style.height = 16;
				borders.ApplyRadius(dragger.style, borders.RadiusFull);
			}
		});

		// Helper text styling
		typography.Caption.ApplyTo(k_helperLabel.style);
		k_helperLabel.style.color = colors.TextSecondary;
		k_helperLabel.style.marginTop = spacing.XS;
	}

	private void OnThemeChanged(GameTheme theme) {
		ApplyTheme();
	}

	#endregion

	#region Private Methods

	private void UpdateHeaderVisibility() {
		bool hasLabel = !string.IsNullOrEmpty(k_label.text);
		bool showHeader = hasLabel || k_showValue;
		k_headerContainer.style.display = showHeader ? DisplayStyle.Flex : DisplayStyle.None;
	}

	private void UpdateValueLabel() {
		if (k_showValue) {
			k_valueLabel.text = string.Format(k_valueFormat, k_slider.value);
		}
	}

	private void OnSliderValueChanged(ChangeEvent<float> evt) {
		UpdateValueLabel();
		k_onValueChanged?.Invoke(evt.newValue);
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

public enum SliderVariant {
	Default,
	Primary,
	Secondary,
	Success
}

#endregion
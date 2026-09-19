using UnityEngine;

namespace RPGGame.UI.Styles;

#nullable disable

public class AccessibilitySettings {
	public ColorBlindnessType ColorBlindnessMode { get; set; } = ColorBlindnessType.None;
	public float UIScale { get; set; } = 1.0f;
	public int FontSizeAdjustment { get; set; } = 0;
	public bool ForceHighContrast { get; set; } = false;
	public bool ReduceMotion { get; set; } = false;
	public bool AlwaysShowIconLabels { get; set; } = false;
	public float MinimumTargetSize { get; set; } = 44f;
	public bool UsePatternsForDifferentiation { get; set; } = false;

	public Color AdjustForColorBlindness(Color color) {
		return ColorBlindnessMode switch {
			ColorBlindnessType.Protanopia => SimulateProtanopia(color),
			ColorBlindnessType.Deuteranopia => SimulateDeuteranopia(color),
			ColorBlindnessType.Tritanopia => SimulateTritanopia(color),
			ColorBlindnessType.Achromatopsia => SimulateAchromatopsia(color),
			_ => color
		};
	}

	public int GetAdjustedFontSize(int baseFontSize) => Mathf.RoundToInt((baseFontSize + FontSizeAdjustment) * UIScale);
	public int GetAdjustedAnimationDuration(int baseDuration) => ReduceMotion ? 0 : baseDuration;

	private static Color SimulateProtanopia(Color c) {
		float l = 0.1127f * c.r + 0.8897f * c.g - 0.0024f * c.b;
		float s = 0.0042f * c.r - 0.0176f * c.g + 1.0154f * c.b;
		return new Color(Mathf.Clamp01(l), Mathf.Clamp01(l), Mathf.Clamp01(s), c.a);
	}

	private static Color SimulateDeuteranopia(Color c) {
		float l = 0.2924f * c.r + 0.7076f * c.g;
		float s = -0.0227f * c.r + 0.0227f * c.g + 1.0f * c.b;
		return new Color(Mathf.Clamp01(l), Mathf.Clamp01(l), Mathf.Clamp01(s), c.a);
	}

	private static Color SimulateTritanopia(Color c) {
		float l = 1.0f * c.r;
		float m = 1.0f * c.g;
		float s = -0.3956f * c.r + 0.8011f * c.g + 0.5945f * c.b;
		return new Color(Mathf.Clamp01(l), Mathf.Clamp01(m), Mathf.Clamp01(s), c.a);
	}

	private static Color SimulateAchromatopsia(Color c) {
		float gray = 0.299f * c.r + 0.587f * c.g + 0.114f * c.b;
		return new Color(gray, gray, gray, c.a);
	}

	public AccessibilitySettings Clone() {
		return new AccessibilitySettings {
			ColorBlindnessMode = ColorBlindnessMode,
			UIScale = UIScale,
			FontSizeAdjustment = FontSizeAdjustment,
			ForceHighContrast = ForceHighContrast,
			ReduceMotion = ReduceMotion,
			AlwaysShowIconLabels = AlwaysShowIconLabels,
			MinimumTargetSize = MinimumTargetSize,
			UsePatternsForDifferentiation = UsePatternsForDifferentiation
		};
	}
}

public enum ColorBlindnessType {
	None,
	Protanopia,    // Red-blind (~1% of males)
	Deuteranopia,  // Green-blind (~1% of males)
	Tritanopia,    // Blue-blind (rare)
	Achromatopsia  // Complete color blindness (very rare)
}
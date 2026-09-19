using RPGGame.Core;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace RPGGame.UI.Styles;

#nullable disable

/// <summary>
/// GameTheme provides a centralized, comprehensive theming system for all UI elements.
/// Supports runtime customization, user overrides, accessibility options, and live tweaking.
/// </summary>
public class GameTheme {
	#region Singleton & Theme Management

	private static GameTheme s_current;
	private static readonly object s_lock = new();

	public static GameTheme Current {
		get {
			if (s_current == null) {
				lock (s_lock) {
					s_current ??= ThemePresets.CreateDevTheme();
				}
			}
			return s_current;
		}
	}

	public static void SetTheme(GameTheme theme) {
		s_current = theme;
		OnThemeChanged?.Invoke(theme);
	}

	public static void NotifyThemeChanged() {
		OnThemeChanged?.Invoke(s_current);
		OnThemePropertyChanged?.Invoke(s_current, null);
	}

	public static void NotifyPropertyChanged(string propertyPath) {
		OnThemePropertyChanged?.Invoke(s_current, propertyPath);
	}

	public static event Action<GameTheme> OnThemeChanged;
	public static event Action<GameTheme, string> OnThemePropertyChanged;

	#endregion

	#region Theme Properties

	public string ThemeId { get; set; }
	public string DisplayName { get; set; }
	public ColorPalette Colors { get; set; }
	public TypographySettings Typography { get; set; }
	public SpacingSettings Spacing { get; set; }
	public BorderSettings Borders { get; set; }
	public AnimationSettings Animations { get; set; }
	public EffectSettings Effects { get; set; }
	public ComponentStyles Components { get; set; }
	public IconSettings Icons { get; set; }
	public AudioFeedbackSettings Audio { get; set; }
	public UserOverrideSettings UserOverrides { get; set; }
	public AccessibilitySettings Accessibility { get; set; }

	#endregion

	#region Cloning & Serialization

	public GameTheme Clone() {
		return new GameTheme {
			ThemeId = ThemeId + "_copy",
			DisplayName = DisplayName + " (Copy)",
			Colors = Colors.Clone(),
			Typography = Typography.Clone(),
			Spacing = new SpacingSettings { BaseUnit = Spacing.BaseUnit },
			Borders = Borders.Clone(),
			Animations = Animations.Clone(),
			Effects = Effects.Clone(),
			Components = new ComponentStyles(),
			Icons = Icons.Clone(),
			Audio = Audio.Clone(),
			UserOverrides = new UserOverrideSettings(),
			Accessibility = Accessibility.Clone()
		};
	}

	public Dictionary<string, object> ExportToDict() {
		var result = new Dictionary<string, object> {
			["themeId"] = ThemeId,
			["displayName"] = DisplayName,
			["userOverrides"] = UserOverrides.ExportOverrides(),
			["accessibility"] = new Dictionary<string, object> {
				["colorBlindnessMode"] = (int)Accessibility.ColorBlindnessMode,
				["uiScale"] = Accessibility.UIScale,
				["fontSizeAdjustment"] = Accessibility.FontSizeAdjustment,
				["forceHighContrast"] = Accessibility.ForceHighContrast,
				["reduceMotion"] = Accessibility.ReduceMotion,
				["alwaysShowIconLabels"] = Accessibility.AlwaysShowIconLabels,
				["minimumTargetSize"] = Accessibility.MinimumTargetSize,
				["usePatternsForDifferentiation"] = Accessibility.UsePatternsForDifferentiation
			}
		};
		return result;
	}

	public void ImportFromDict(Dictionary<string, object> data) {
		if (data.TryGetValue("userOverrides", out object overridesObj) && overridesObj is Dictionary<string, object> overrides) {
			UserOverrides.ImportOverrides(overrides);
		}

		if (data.TryGetValue("accessibility", out object accessObj) && accessObj is Dictionary<string, object> access) {
			if (access.TryGetValue("colorBlindnessMode", out object cbm)) {
				Accessibility.ColorBlindnessMode = (ColorBlindnessType)Convert.ToInt32(cbm);
			}
			if (access.TryGetValue("uiScale", out object scale)) {
				Accessibility.UIScale = Convert.ToSingle(scale);
			}
			if (access.TryGetValue("fontSizeAdjustment", out object fsa)) {
				Accessibility.FontSizeAdjustment = Convert.ToInt32(fsa);
			}
			if (access.TryGetValue("forceHighContrast", out object fhc)) {
				Accessibility.ForceHighContrast = Convert.ToBoolean(fhc);
			}
			if (access.TryGetValue("reduceMotion", out object rm)) {
				Accessibility.ReduceMotion = Convert.ToBoolean(rm);
			}
			if (access.TryGetValue("alwaysShowIconLabels", out object asil)) {
				Accessibility.AlwaysShowIconLabels = Convert.ToBoolean(asil);
			}
			if (access.TryGetValue("minimumTargetSize", out object mts)) {
				Accessibility.MinimumTargetSize = Convert.ToSingle(mts);
			}
			if (access.TryGetValue("usePatternsForDifferentiation", out object upfd)) {
				Accessibility.UsePatternsForDifferentiation = Convert.ToBoolean(upfd);
			}
		}
		NotifyThemeChanged();
	}

	/// <summary>
	/// Gets a font asset by category and variant.
	/// </summary>
	public FontAsset GetFontAsset(FontCategory category, FontVariant variant = FontVariant.Regular) {
		if (!GameServices.IsInitialized) return null;
		return GameServices.Assets.GetFontAsset(category, variant);
	}

	/// <summary>
	/// Gets the monospace font asset for UI Toolkit elements.
	/// </summary>
	public FontAsset GetMonospaceFontAsset() {
		if (!GameServices.IsInitialized) return null;
		return GameServices.Assets.GetMonospaceFontAsset();
	}



	#endregion

	#region Debug Helpers

	public void DebugLogColors() {
		UnityEngine.Debug.Log($"=== Theme Colors: {DisplayName} ===");
		UnityEngine.Debug.Log($"Primary: {Colors.Primary}");
		UnityEngine.Debug.Log($"Secondary: {Colors.Secondary}");
		UnityEngine.Debug.Log($"Background: {Colors.Background}");
		UnityEngine.Debug.Log($"TextPrimary: {Colors.TextPrimary}");
	}

	#endregion
}
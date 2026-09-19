using RPGGame.Core;
using UnityEngine;

namespace RPGGame.UI.Styles;

#nullable disable

public static class ThemePresets {
	public static GameTheme CreateDark() => new() {
		ThemeId = "dark",
		DisplayName = "Dark",
		Colors = new ColorPalette(),
		Typography = new TypographySettings(),
		Spacing = new SpacingSettings(),
		Borders = new BorderSettings(),
		Animations = new AnimationSettings(),
		Effects = new EffectSettings(),
		Components = new ComponentStyles(),
		Icons = new IconSettings(),
		Audio = new AudioFeedbackSettings(),
		UserOverrides = new UserOverrideSettings(),
		Accessibility = new AccessibilitySettings()
	};

	public static GameTheme CreateLight() => new() {
		ThemeId = "light",
		DisplayName = "Light",
		Colors = new ColorPalette {
			Primary = new Color(0.15f, 0.5f, 0.85f, 1f),
			PrimaryHover = new Color(0.2f, 0.55f, 0.9f, 1f),
			PrimaryPressed = new Color(0.1f, 0.45f, 0.8f, 1f),
			Background = new Color(0.96f, 0.96f, 0.98f, 1f),
			BackgroundSecondary = new Color(0.92f, 0.92f, 0.94f, 1f),
			BackgroundTertiary = new Color(0.88f, 0.88f, 0.9f, 1f),
			BackgroundElevated = new Color(1f, 1f, 1f, 1f),
			BackgroundOverlay = new Color(0f, 0f, 0f, 0.4f),
			Surface = new Color(1f, 1f, 1f, 1f),
			SurfaceHover = new Color(0.96f, 0.96f, 0.98f, 1f),
			SurfaceSelected = new Color(0.92f, 0.92f, 0.96f, 1f),
			SurfaceBorder = new Color(0.85f, 0.85f, 0.88f, 1f),
			TextPrimary = new Color(0.1f, 0.1f, 0.12f, 1f),
			TextSecondary = new Color(0.4f, 0.4f, 0.45f, 1f),
			TextTertiary = new Color(0.6f, 0.6f, 0.65f, 1f),
			TextDisabled = new Color(0.7f, 0.7f, 0.75f, 1f),
			TextInverse = new Color(1f, 1f, 1f, 1f),
			InputBackground = new Color(1f, 1f, 1f, 1f),
			InputBorder = new Color(0.8f, 0.8f, 0.85f, 1f),
			InputBorderFocus = new Color(0.2f, 0.5f, 0.85f, 1f),
			TooltipBackground = new Color(0.2f, 0.2f, 0.25f, 0.95f),
			TooltipBorder = new Color(0.3f, 0.3f, 0.35f, 1f)
		},
		Typography = new TypographySettings(),
		Spacing = new SpacingSettings(),
		Borders = new BorderSettings(),
		Animations = new AnimationSettings(),
		Effects = new EffectSettings {
			ShadowSM = new ShadowPreset { OffsetX = 0, OffsetY = 1, Blur = 3, Spread = 0, Color = new Color(0, 0, 0, 0.08f) },
			ShadowMD = new ShadowPreset { OffsetX = 0, OffsetY = 4, Blur = 6, Spread = -1, Color = new Color(0, 0, 0, 0.1f) },
			ShadowLG = new ShadowPreset { OffsetX = 0, OffsetY = 10, Blur = 15, Spread = -3, Color = new Color(0, 0, 0, 0.12f) },
			ShadowXL = new ShadowPreset { OffsetX = 0, OffsetY = 20, Blur = 25, Spread = -5, Color = new Color(0, 0, 0, 0.15f) }
		},
		Components = new ComponentStyles(),
		Icons = new IconSettings(),
		Audio = new AudioFeedbackSettings(),
		UserOverrides = new UserOverrideSettings(),
		Accessibility = new AccessibilitySettings()
	};

	public static GameTheme CreateFantasy() => new() {
		ThemeId = "fantasy",
		DisplayName = "Fantasy",
		Colors = new ColorPalette {
			Primary = new Color(0.8f, 0.65f, 0.2f, 1f),
			PrimaryHover = new Color(0.9f, 0.75f, 0.3f, 1f),
			PrimaryPressed = new Color(0.7f, 0.55f, 0.15f, 1f),
			Secondary = new Color(0.5f, 0.25f, 0.6f, 1f),
			SecondaryHover = new Color(0.6f, 0.35f, 0.7f, 1f),
			Accent = new Color(0.85f, 0.2f, 0.15f, 1f),
			Background = new Color(0.08f, 0.06f, 0.1f, 1f),
			BackgroundSecondary = new Color(0.12f, 0.1f, 0.15f, 1f),
			BackgroundTertiary = new Color(0.16f, 0.14f, 0.2f, 1f),
			BackgroundElevated = new Color(0.14f, 0.12f, 0.18f, 1f),
			Surface = new Color(0.1f, 0.08f, 0.14f, 1f),
			SurfaceBorder = new Color(0.4f, 0.35f, 0.25f, 1f),
			TextPrimary = new Color(0.95f, 0.9f, 0.8f, 1f),
			TextSecondary = new Color(0.75f, 0.7f, 0.6f, 1f),
			Health = new Color(0.8f, 0.15f, 0.1f, 1f),
			Mana = new Color(0.3f, 0.3f, 0.9f, 1f),
			Stamina = new Color(0.6f, 0.5f, 0.2f, 1f),
			RarityCommon = new Color(0.6f, 0.55f, 0.5f, 1f),
			RarityUncommon = new Color(0.4f, 0.7f, 0.35f, 1f),
			RarityRare = new Color(0.3f, 0.45f, 0.85f, 1f),
			RarityEpic = new Color(0.6f, 0.3f, 0.8f, 1f),
			RarityLegendary = new Color(0.9f, 0.7f, 0.2f, 1f),
			RarityMythic = new Color(0.9f, 0.3f, 0.35f, 1f)
		},
		Typography = new TypographySettings { DisplayFontFamily = "Cinzel Decorative", PrimaryFontFamily = "Lora" },
		Spacing = new SpacingSettings(),
		Borders = new BorderSettings { RadiusSM = 2f, RadiusMD = 4f, RadiusLG = 6f, ButtonRadius = 3f, CardRadius = 4f },
		Animations = new AnimationSettings(),
		Effects = new EffectSettings(),
		Components = new ComponentStyles(),
		Icons = new IconSettings(),
		Audio = new AudioFeedbackSettings(),
		UserOverrides = new UserOverrideSettings(),
		Accessibility = new AccessibilitySettings()
	};

	public static GameTheme CreateSciFi() => new() {
		ThemeId = "scifi",
		DisplayName = "Sci-Fi",
		Colors = new ColorPalette {
			Primary = new Color(0f, 0.9f, 0.95f, 1f),
			PrimaryHover = new Color(0.2f, 1f, 1f, 1f),
			PrimaryPressed = new Color(0f, 0.75f, 0.8f, 1f),
			Secondary = new Color(0.95f, 0.2f, 0.6f, 1f),
			SecondaryHover = new Color(1f, 0.4f, 0.7f, 1f),
			Accent = new Color(0.6f, 1f, 0.2f, 1f),
			Background = new Color(0.02f, 0.04f, 0.08f, 1f),
			BackgroundSecondary = new Color(0.04f, 0.06f, 0.12f, 1f),
			BackgroundTertiary = new Color(0.06f, 0.08f, 0.16f, 1f),
			BackgroundElevated = new Color(0.05f, 0.07f, 0.14f, 1f),
			Surface = new Color(0.03f, 0.05f, 0.1f, 1f),
			SurfaceBorder = new Color(0f, 0.5f, 0.55f, 0.5f),
			TextPrimary = new Color(0.9f, 0.95f, 1f, 1f),
			TextSecondary = new Color(0.6f, 0.7f, 0.8f, 1f),
			Health = new Color(0.95f, 0.2f, 0.3f, 1f),
			Mana = new Color(0.3f, 0.5f, 1f, 1f),
			Stamina = new Color(0.2f, 0.95f, 0.4f, 1f),
			InputBorderFocus = new Color(0f, 0.9f, 0.95f, 1f)
		},
		Typography = new TypographySettings { PrimaryFontFamily = "Orbitron", SecondaryFontFamily = "Exo 2", MonospaceFontFamily = "Share Tech Mono" },
		Spacing = new SpacingSettings(),
		Borders = new BorderSettings { WidthThin = 1f, WidthMedium = 1f, RadiusSM = 2f, RadiusMD = 4f, RadiusLG = 6f, ButtonRadius = 2f, CardRadius = 4f },
		Animations = new AnimationSettings { DurationFast = 80, DurationNormal = 150, DurationSlow = 250 },
		Effects = new EffectSettings {
			GlowPrimary = new GlowPreset { Color = new Color(0f, 0.9f, 0.95f, 0.5f), Blur = 12, Spread = 4 },
			GlowSuccess = new GlowPreset { Color = new Color(0.2f, 0.95f, 0.4f, 0.5f), Blur = 12, Spread = 4 },
			GlowError = new GlowPreset { Color = new Color(0.95f, 0.2f, 0.3f, 0.5f), Blur = 12, Spread = 4 }
		},
		Components = new ComponentStyles(),
		Icons = new IconSettings(),
		Audio = new AudioFeedbackSettings(),
		UserOverrides = new UserOverrideSettings(),
		Accessibility = new AccessibilitySettings()
	};

	public static GameTheme CreateHighContrast() => new() {
		ThemeId = "highcontrast",
		DisplayName = "High Contrast",
		Colors = new ColorPalette {
			Primary = new Color(1f, 1f, 0f, 1f),
			PrimaryHover = new Color(1f, 1f, 0.4f, 1f),
			PrimaryPressed = new Color(0.9f, 0.9f, 0f, 1f),
			Secondary = new Color(0f, 1f, 1f, 1f),
			Accent = new Color(1f, 0f, 1f, 1f),
			Background = new Color(0f, 0f, 0f, 1f),
			BackgroundSecondary = new Color(0.1f, 0.1f, 0.1f, 1f),
			BackgroundElevated = new Color(0.05f, 0.05f, 0.05f, 1f),
			Surface = new Color(0.05f, 0.05f, 0.05f, 1f),
			SurfaceBorder = new Color(1f, 1f, 1f, 1f),
			TextPrimary = new Color(1f, 1f, 1f, 1f),
			TextSecondary = new Color(0.9f, 0.9f, 0.9f, 1f),
			Success = new Color(0f, 1f, 0f, 1f),
			Warning = new Color(1f, 1f, 0f, 1f),
			Error = new Color(1f, 0f, 0f, 1f),
			Info = new Color(0f, 0.8f, 1f, 1f),
			InputBorder = new Color(1f, 1f, 1f, 1f),
			InputBorderFocus = new Color(1f, 1f, 0f, 1f)
		},
		Typography = new TypographySettings { FontSizes = new FontSizeScale { BodyMedium = 18, BodySmall = 16, LabelMedium = 16, Caption = 14 } },
		Spacing = new SpacingSettings { BaseUnit = 6 },
		Borders = new BorderSettings { WidthThin = 2f, WidthMedium = 3f, WidthThick = 4f },
		Animations = new AnimationSettings(),
		Effects = new EffectSettings(),
		Components = new ComponentStyles(),
		Icons = new IconSettings { SizeSM = 20f, SizeMD = 24f, SizeLG = 32f },
		Audio = new AudioFeedbackSettings(),
		UserOverrides = new UserOverrideSettings(),
		Accessibility = new AccessibilitySettings { ForceHighContrast = true, MinimumTargetSize = 48f }
	};

	public static GameTheme CreateColorblindFriendly() => new() {
		ThemeId = "colorblind",
		DisplayName = "Colorblind Friendly",
		Colors = new ColorPalette {
			Primary = new Color(0.2f, 0.4f, 0.8f, 1f),
			PrimaryHover = new Color(0.3f, 0.5f, 0.9f, 1f),
			PrimaryPressed = new Color(0.15f, 0.35f, 0.7f, 1f),
			Secondary = new Color(0.9f, 0.55f, 0.1f, 1f),
			SecondaryHover = new Color(1f, 0.65f, 0.2f, 1f),
			Accent = new Color(0.8f, 0.8f, 0.2f, 1f),
			Background = new Color(0.12f, 0.12f, 0.14f, 1f),
			BackgroundSecondary = new Color(0.16f, 0.16f, 0.18f, 1f),
			RarityCommon = new Color(0.6f, 0.6f, 0.6f, 1f),
			RarityUncommon = new Color(0.3f, 0.7f, 0.9f, 1f),
			RarityRare = new Color(0.2f, 0.4f, 0.9f, 1f),
			RarityEpic = new Color(0.9f, 0.5f, 0.1f, 1f),
			RarityLegendary = new Color(0.95f, 0.85f, 0.2f, 1f),
			RarityMythic = new Color(0.95f, 0.3f, 0.5f, 1f),
			Success = new Color(0.3f, 0.7f, 0.9f, 1f),
			Warning = new Color(0.95f, 0.75f, 0.2f, 1f),
			Error = new Color(0.9f, 0.3f, 0.5f, 1f),
			Health = new Color(0.9f, 0.35f, 0.5f, 1f),
			Mana = new Color(0.3f, 0.5f, 0.95f, 1f),
			Stamina = new Color(0.95f, 0.75f, 0.2f, 1f)
		},
		Typography = new TypographySettings(),
		Spacing = new SpacingSettings(),
		Borders = new BorderSettings(),
		Animations = new AnimationSettings(),
		Effects = new EffectSettings(),
		Components = new ComponentStyles(),
		Icons = new IconSettings(),
		Audio = new AudioFeedbackSettings(),
		UserOverrides = new UserOverrideSettings(),
		Accessibility = new AccessibilitySettings { UsePatternsForDifferentiation = true, AlwaysShowIconLabels = true }
	};

	public static GameTheme CreateDevTheme() => new() {
		ThemeId = "DevTheme",
		DisplayName = "Developer's Theme",
		Colors = new ColorPalette {
			Background = ColorRPG.FromHex("0a0a12"), // Deep abyss black with subtle navy undertone
			BackgroundSecondary = ColorRPG.FromHex("151a22"), // Shadowed slate for panels/separators
			Primary = ColorRPG.FromHex("8b1c1a"), // Muted blood crimson (PoE-style action buttons)
			PrimaryHover = ColorRPG.FromHex("9f2a3a"), // Subtle crimson glow on hover
			PrimaryPressed = ColorRPG.FromHex("5f151f"), // Darkened pressed state
			Secondary = ColorRPG.FromHex("1a1f2e"), // Desaturated midnight blue-gray
			SecondaryHover = ColorRPG.FromHex("2a2f44"), // Faint ethereal lift
			Accent = ColorRPG.FromHex("4a5a70"), // Cold steel blue-gray for highlights
			RarityCommon = ColorRPG.FromHex("4a4a4a"), // Forged iron gray
			RarityUncommon = ColorRPG.FromHex("2e5d32"), // Muted necrotic green
			RarityRare = ColorRPG.FromHex("1e4a7a"), // Deep abyssal blue
			RarityEpic = ColorRPG.FromHex("5e1a6f"), // Void purple
			RarityLegendary = ColorRPG.FromHex("b55f00"), // Cursed amber orange
			RarityMythic = ColorRPG.FromHex("b89e00"), // Ancient relic gold (desaturated)
			Success = ColorRPG.FromHex("2e5d32"), // Subtle verdant success (matches Uncommon)
			Warning = ColorRPG.FromHex("b55f00"), // Ominous amber (matches Legendary)
			Error = ColorRPG.FromHex("7b1c2a"), // Bloodied crimson (matches Primary)
			Health = ColorRPG.FromHex("8b1a1a"), // Visceral deep red
			Mana = ColorRPG.FromHex("1e3a5f"), // Arcane void blue
			Stamina = ColorRPG.FromHex("8b5a1a"), // Worn leather yellow-brown
		},
		Typography = new TypographySettings(),
		Spacing = new SpacingSettings(),
		Borders = new BorderSettings(),
		Animations = new AnimationSettings(),
		Effects = new EffectSettings(),
		Components = new ComponentStyles(),
		Icons = new IconSettings(),
		Audio = new AudioFeedbackSettings(),
		UserOverrides = new UserOverrideSettings(),
		Accessibility = new AccessibilitySettings { UsePatternsForDifferentiation = true, AlwaysShowIconLabels = true }
	};
}
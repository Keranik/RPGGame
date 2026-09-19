using RPGGame.Core;
using RPGGame.Core.Items;
using UnityEngine;

namespace RPGGame.UI.Styles;

#nullable disable

/// <summary>
/// Color palette using ColorRPG for all color values.
/// Provides seamless HSL manipulation and implicit conversion to Unity Color.
/// </summary>
public class ColorPalette {
	// === Core Brand Colors ===
	public ColorRPG Primary { get; set; } = ColorRPG.FromHex("#3399E6");
	public ColorRPG PrimaryHover { get; set; } = ColorRPG.FromHex("#4DB3FF");
	public ColorRPG PrimaryPressed { get; set; } = ColorRPG.FromHex("#2680CC");
	public ColorRPG PrimaryDisabled { get; set; } = ColorRPG.FromHex("#3399E6").WithAlpha(0.4f);

	public ColorRPG Secondary { get; set; } = ColorRPG.FromHex("#9966CC");
	public ColorRPG SecondaryHover { get; set; } = ColorRPG.FromHex("#B380E6");
	public ColorRPG SecondaryPressed { get; set; } = ColorRPG.FromHex("#804DB3");
	public ColorRPG SecondaryDisabled { get; set; } = ColorRPG.FromHex("#9966CC").WithAlpha(0.4f);

	public ColorRPG Accent { get; set; } = ColorRPG.FromHex("#FFCC33");
	public ColorRPG AccentHover { get; set; } = ColorRPG.FromHex("#FFD966");
	public ColorRPG AccentPressed { get; set; } = ColorRPG.FromHex("#E6B31A");

	// === Background Colors ===
	public ColorRPG Background { get; set; } = ColorRPG.FromHex("#1A1A1F");
	public ColorRPG BackgroundSecondary { get; set; } = ColorRPG.FromHex("#26262E");
	public ColorRPG BackgroundTertiary { get; set; } = ColorRPG.FromHex("#33333D");
	public ColorRPG BackgroundElevated { get; set; } = ColorRPG.FromHex("#2E2E38");
	public ColorRPG BackgroundOverlay { get; set; } = ColorRPG.FromHex("#000000").WithAlpha(0.7f);

	// === Surface Colors ===
	public ColorRPG Surface { get; set; } = ColorRPG.FromHex("#24242B");
	public ColorRPG SurfaceHover { get; set; } = ColorRPG.FromHex("#2E2E38");
	public ColorRPG SurfaceSelected { get; set; } = ColorRPG.FromHex("#383847");
	public ColorRPG SurfaceBorder { get; set; } = ColorRPG.FromHex("#4D4D59");

	// === Text Colors ===
	public ColorRPG TextPrimary { get; set; } = ColorRPG.White;
	public ColorRPG TextSecondary { get; set; } = ColorRPG.FromHex("#B3B3BF");
	public ColorRPG TextTertiary { get; set; } = ColorRPG.FromHex("#80808C");
	public ColorRPG TextDisabled { get; set; } = ColorRPG.FromHex("#666673");
	public ColorRPG TextInverse { get; set; } = ColorRPG.FromHex("#1A1A1F");
	public ColorRPG TextLink { get; set; } = ColorRPG.FromHex("#66B3FF");
	public ColorRPG TextLinkHover { get; set; } = ColorRPG.FromHex("#99D9FF");

	// === Semantic Colors ===
	public ColorRPG Success { get; set; } = ColorRPG.FromHex("#4DD966");
	public ColorRPG SuccessHover { get; set; } = ColorRPG.FromHex("#66F280");
	public ColorRPG SuccessBackground { get; set; } = ColorRPG.FromHex("#4DD966").WithAlpha(0.15f);

	public ColorRPG Warning { get; set; } = ColorRPG.FromHex("#FFBF33");
	public ColorRPG WarningHover { get; set; } = ColorRPG.FromHex("#FFD966");
	public ColorRPG WarningBackground { get; set; } = ColorRPG.FromHex("#FFBF33").WithAlpha(0.15f);

	public ColorRPG Error { get; set; } = ColorRPG.FromHex("#F24D4D");
	public ColorRPG ErrorHover { get; set; } = ColorRPG.FromHex("#FF7373");
	public ColorRPG ErrorBackground { get; set; } = ColorRPG.FromHex("#F24D4D").WithAlpha(0.15f);

	public ColorRPG Info { get; set; } = ColorRPG.FromHex("#4DB3F2");
	public ColorRPG InfoHover { get; set; } = ColorRPG.FromHex("#73CCFF");
	public ColorRPG InfoBackground { get; set; } = ColorRPG.FromHex("#4DB3F2").WithAlpha(0.15f);

	// === Game-Specific Colors ===
	public ColorRPG Health { get; set; } = ColorRPG.HealthGreen;
	public ColorRPG HealthBackground { get; set; } = ColorRPG.FromHex("#4D1A1A");
	public ColorRPG Mana { get; set; } = ColorRPG.ManaBlue;
	public ColorRPG ManaBackground { get; set; } = ColorRPG.FromHex("#1A2659");
	public ColorRPG Stamina { get; set; } = ColorRPG.StaminaYellow;
	public ColorRPG StaminaBackground { get; set; } = ColorRPG.FromHex("#1A401A");
	public ColorRPG Experience { get; set; } = ColorRPG.ExperiencePurple;
	public ColorRPG ExperienceBackground { get; set; } = ColorRPG.FromHex("#40331A");

	// === Rarity Colors ===
	public ColorRPG RarityCommon { get; set; } = ColorRPG.RarityCommon;
	public ColorRPG RarityUncommon { get; set; } = ColorRPG.RarityUncommon;
	public ColorRPG RarityRare { get; set; } = ColorRPG.RarityRare;
	public ColorRPG RarityEpic { get; set; } = ColorRPG.RarityEpic;
	public ColorRPG RarityLegendary { get; set; } = ColorRPG.RarityLegendary;
	public ColorRPG RarityMythic { get; set; } = ColorRPG.RarityMythic;

	// === Interactive Element Colors ===
	public ColorRPG InputBackground { get; set; } = ColorRPG.FromHex("#14141A");
	public ColorRPG InputBorder { get; set; } = ColorRPG.FromHex("#4D4D59");
	public ColorRPG InputBorderFocus { get; set; } = ColorRPG.FromHex("#4D99E6");
	public ColorRPG InputPlaceholder { get; set; } = ColorRPG.FromHex("#666673");

	public ColorRPG ScrollbarTrack { get; set; } = ColorRPG.FromHex("#26262E");
	public ColorRPG ScrollbarThumb { get; set; } = ColorRPG.FromHex("#595966");
	public ColorRPG ScrollbarThumbHover { get; set; } = ColorRPG.FromHex("#737380");

	public ColorRPG TooltipBackground { get; set; } = ColorRPG.FromHex("#0D0D14").WithAlpha(0.95f);
	public ColorRPG TooltipBorder { get; set; } = ColorRPG.FromHex("#666673");

	// === Utility Colors ===
	public ColorRPG Transparent { get; set; } = ColorRPG.Transparent;
	public ColorRPG White { get; set; } = ColorRPG.White;
	public ColorRPG Black { get; set; } = ColorRPG.Black;

	/// <summary>
	/// Gets the color for a rarity type.
	/// </summary>
	public ColorRPG GetRarityColor(RarityType rarity) {
		string key = $"Rarity{rarity}";
		if (GameTheme.Current?.UserOverrides?.TryGetColor(key, out var overrideColor) == true) {
			return overrideColor;
		}

		var baseColor = rarity switch {
			RarityType.Common => RarityCommon,
			RarityType.Uncommon => RarityUncommon,
			RarityType.Rare => RarityRare,
			RarityType.Epic => RarityEpic,
			RarityType.Legendary => RarityLegendary,
			RarityType.Mythic => RarityMythic,
			_ => RarityCommon
		};

		if (GameTheme.Current?.Accessibility?.ColorBlindnessMode != ColorBlindnessType.None) {
			Color adjusted = GameTheme.Current.Accessibility.AdjustForColorBlindness(baseColor);
			return adjusted;
		}

		return baseColor;
	}

	/// <summary>
	/// Gets a color by property name.
	/// </summary>
	public ColorRPG GetColor(string propertyName) {
		if (GameTheme.Current?.UserOverrides?.TryGetColor(propertyName, out var overrideColor) == true) {
			return overrideColor;
		}

		var prop = GetType().GetProperty(propertyName);
		if (prop != null && prop.PropertyType == typeof(ColorRPG)) {
			return (ColorRPG)prop.GetValue(this);
		}

		return White;
	}

	/// <summary>
	/// Sets a color by property name.
	/// </summary>
	public void SetColor(string propertyName, ColorRPG value) {
		var prop = GetType().GetProperty(propertyName);
		if (prop != null && prop.PropertyType == typeof(ColorRPG) && prop.CanWrite) {
			prop.SetValue(this, value);
			GameTheme.NotifyPropertyChanged($"Colors.{propertyName}");
		}
	}

	/// <summary>
	/// Creates a deep copy of this palette.
	/// </summary>
	public ColorPalette Clone() {
		return new ColorPalette {
			Primary = Primary, PrimaryHover = PrimaryHover, PrimaryPressed = PrimaryPressed, PrimaryDisabled = PrimaryDisabled,
			Secondary = Secondary, SecondaryHover = SecondaryHover, SecondaryPressed = SecondaryPressed, SecondaryDisabled = SecondaryDisabled,
			Accent = Accent, AccentHover = AccentHover, AccentPressed = AccentPressed,
			Background = Background, BackgroundSecondary = BackgroundSecondary, BackgroundTertiary = BackgroundTertiary,
			BackgroundElevated = BackgroundElevated, BackgroundOverlay = BackgroundOverlay,
			Surface = Surface, SurfaceHover = SurfaceHover, SurfaceSelected = SurfaceSelected, SurfaceBorder = SurfaceBorder,
			TextPrimary = TextPrimary, TextSecondary = TextSecondary, TextTertiary = TextTertiary,
			TextDisabled = TextDisabled, TextInverse = TextInverse, TextLink = TextLink, TextLinkHover = TextLinkHover,
			Success = Success, SuccessHover = SuccessHover, SuccessBackground = SuccessBackground,
			Warning = Warning, WarningHover = WarningHover, WarningBackground = WarningBackground,
			Error = Error, ErrorHover = ErrorHover, ErrorBackground = ErrorBackground,
			Info = Info, InfoHover = InfoHover, InfoBackground = InfoBackground,
			Health = Health, HealthBackground = HealthBackground, Mana = Mana, ManaBackground = ManaBackground,
			Stamina = Stamina, StaminaBackground = StaminaBackground, Experience = Experience, ExperienceBackground = ExperienceBackground,
			RarityCommon = RarityCommon, RarityUncommon = RarityUncommon, RarityRare = RarityRare,
			RarityEpic = RarityEpic, RarityLegendary = RarityLegendary, RarityMythic = RarityMythic,
			InputBackground = InputBackground, InputBorder = InputBorder, InputBorderFocus = InputBorderFocus, InputPlaceholder = InputPlaceholder,
			ScrollbarTrack = ScrollbarTrack, ScrollbarThumb = ScrollbarThumb, ScrollbarThumbHover = ScrollbarThumbHover,
			TooltipBackground = TooltipBackground, TooltipBorder = TooltipBorder
		};
	}
}
using RPGGame.Core;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

namespace RPGGame.UI.Styles;

#nullable disable

/// <summary>
/// Font categories for the D&D-style typography system.
/// Each category has a specific purpose and associated fonts.
/// </summary>
public enum FontCategory {
	/// <summary>Body text, descriptions, dialogue (Bookinsanity)</summary>
	Body,
	/// <summary>Section headings, labels (Mr Eaves Small Caps)</summary>
	Heading,
	/// <summary>Tables, stats, skill lists (Scaly Sans)</summary>
	Table,
	/// <summary>Major titles, quest headers (Nodesto Caps Condensed)</summary>
	Title,
	/// <summary>Table headers, flavor text (Zatanna Misdirection)</summary>
	TableHeader,
	/// <summary>Drop caps for lore paragraphs (Solbera Imitation)</summary>
	DropCap,
	/// <summary>Dice notation, coordinates, codes (JetBrains Mono)</summary>
	Monospace
}

/// <summary>
/// Font weight/style variant for a font family.
/// </summary>
public enum FontVariant {
	Regular,
	Bold,
	Italic,
	BoldItalic,
	SmallCaps,
	Caps
}

public class TypographySettings {
	#region Font Families

	// ═══════════════════════════════════════════════════════════════
	// FONT FAMILY NAMES (for reference/CSS)
	// ═══════════════════════════════════════════════════════════════

	/// <summary>Body text font (Bookinsanity - PHB body style)</summary>
	public string BodyFontFamily { get; set; } = "Bookinsanity";

	/// <summary>Heading font (Mr Eaves Small Caps - PHB heading style)</summary>
	public string HeadingFontFamily { get; set; } = "Mr Eaves Small Caps";

	/// <summary>Table/stats font (Scaly Sans - clean sans-serif)</summary>
	public string TableFontFamily { get; set; } = "Scaly Sans";

	/// <summary>Major titles font (Nodesto Caps Condensed - cover/title style)</summary>
	public string TitleFontFamily { get; set; } = "Nodesto Caps Condensed";

	/// <summary>Table header/flavor font (Zatanna Misdirection)</summary>
	public string TableHeaderFontFamily { get; set; } = "Zatanna Misdirection";

	/// <summary>Drop cap font (Solbera Imitation)</summary>
	public string DropCapFontFamily { get; set; } = "Solbera Imitation";

	/// <summary>Alternative drop cap font (Dungeon Drop Case)</summary>
	public string DropCapAltFontFamily { get; set; } = "Dungeon Drop Case";

	/// <summary>Monospace font for dice, coordinates, codes</summary>
	public string MonospaceFontFamily { get; set; } = "JetBrains Mono";

	// Legacy properties for compatibility
	public string PrimaryFontFamily { get => BodyFontFamily; set => BodyFontFamily = value; }
	public string SecondaryFontFamily { get => TableFontFamily; set => TableFontFamily = value; }
	public string DisplayFontFamily { get => TitleFontFamily; set => TitleFontFamily = value; }

	#endregion

	#region Font Asset Paths

	// ═══════════════════════════════════════════════════════════════
	// SDF FONT ASSET PATHS (for UI Toolkit)
	// Paths are relative to Resources folder, without .asset extension
	// ═══════════════════════════════════════════════════════════════

	/// <summary>Path to Bookinsanity Regular SDF</summary>
	public string BodyFontPath { get; set; } = "Fonts/Raw/Bookinsanity/Bookinsanity SDF";

	/// <summary>Path to Bookinsanity Bold SDF</summary>
	public string BodyBoldFontPath { get; set; } = "Fonts/Raw/Bookinsanity/Bookinsanity Bold SDF";

	/// <summary>Path to Bookinsanity Italic SDF</summary>
	public string BodyItalicFontPath { get; set; } = "Fonts/Raw/Bookinsanity/Bookinsanity Italic SDF";

	/// <summary>Path to Bookinsanity Bold Italic SDF</summary>
	public string BodyBoldItalicFontPath { get; set; } = "Fonts/Raw/Bookinsanity/Bookinsanity Bold Italic SDF";

	/// <summary>Path to Mr Eaves Small Caps SDF</summary>
	public string HeadingFontPath { get; set; } = "Fonts/Raw/Mr Eaves/Mr Eaves Small Caps SDF";

	/// <summary>Path to Scaly Sans Regular SDF</summary>
	public string TableFontPath { get; set; } = "Fonts/Raw/Scaly Sans/Scaly Sans SDF";

	/// <summary>Path to Scaly Sans Bold SDF</summary>
	public string TableBoldFontPath { get; set; } = "Fonts/Raw/Scaly Sans/Scaly Sans Bold SDF";

	/// <summary>Path to Scaly Sans Italic SDF</summary>
	public string TableItalicFontPath { get; set; } = "Fonts/Raw/Scaly Sans/Scaly Sans Italic SDF";

	/// <summary>Path to Scaly Sans Bold Italic SDF</summary>
	public string TableBoldItalicFontPath { get; set; } = "Fonts/Raw/Scaly Sans/Scaly Sans Bold Italic SDF";

	/// <summary>Path to Scaly Sans Caps SDF</summary>
	public string TableCapsFontPath { get; set; } = "Fonts/Raw/Scaly Sans Caps/Scaly Sans Caps SDF";

	/// <summary>Path to Nodesto Caps Condensed SDF</summary>
	public string TitleFontPath { get; set; } = "Fonts/Raw/Nodesto Caps Condensed/Nodesto Caps Condensed SDF";

	/// <summary>Path to Nodesto Caps Condensed Bold SDF</summary>
	public string TitleBoldFontPath { get; set; } = "Fonts/Raw/Nodesto Caps Condensed/NodestoCapsCondensed-Bold SDF";

	/// <summary>Path to Nodesto Caps Condensed Italic SDF</summary>
	public string TitleItalicFontPath { get; set; } = "Fonts/Raw/Nodesto Caps Condensed/NodestoCapsCondensed-Italic SDF";

	/// <summary>Path to Nodesto Caps Condensed Bold Italic SDF</summary>
	public string TitleBoldItalicFontPath { get; set; } = "Fonts/Raw/Nodesto Caps Condensed/NodestoCapsCondensed-Bold Italic SDF";

	/// <summary>Path to Zatanna Misdirection SDF</summary>
	public string TableHeaderFontPath { get; set; } = "Fonts/Raw/Zatanna Misdirection/Zatanna Misdirection SDF";

	/// <summary>Path to Zatanna Misdirection Bold SDF</summary>
	public string TableHeaderBoldFontPath { get; set; } = "Fonts/Raw/Zatanna Misdirection/Zatanna Misdirection Bold SDF";

	/// <summary>Path to Zatanna Misdirection Italic SDF</summary>
	public string TableHeaderItalicFontPath { get; set; } = "Fonts/Raw/Zatanna Misdirection/Zatanna Misdirection Italic SDF";

	/// <summary>Path to Zatanna Misdirection Bold Italic SDF</summary>
	public string TableHeaderBoldItalicFontPath { get; set; } = "Fonts/Raw/Zatanna Misdirection/Zatanna Misdirection Bold Italic SDF";

	/// <summary>Path to Solbera Imitation SDF (drop caps)</summary>
	public string DropCapFontPath { get; set; } = "Fonts/Raw/Solbera Imitation/Solbera Imitation SDF";

	/// <summary>Path to Dungeon Drop Case SDF (alternative drop caps)</summary>
	public string DropCapAltFontPath { get; set; } = "Fonts/Raw/Dungeon Drop Case/Dungeon Drop Case SDF";

	/// <summary>Path to JetBrains Mono SDF (monospace). Generate "Mono SDF"
	/// in Unity via Window > TextMeshPro > Font Asset Creator from
	/// Fonts/Raw/Mono/JetBrainsMono-Regular.ttf.</summary>
	public string MonospaceFontAssetPath { get; set; } = "Fonts/Raw/Mono/Mono SDF";

	#endregion

	#region Font Sizes

	public FontSizeScale FontSizes { get; set; } = new();

	#endregion

	#region Font Weights

	public FontWeight WeightLight { get; set; } = FontWeight.Light;
	public FontWeight WeightRegular { get; set; } = FontWeight.Normal;
	public FontWeight WeightMedium { get; set; } = FontWeight.Medium;
	public FontWeight WeightSemiBold { get; set; } = FontWeight.SemiBold;
	public FontWeight WeightBold { get; set; } = FontWeight.Bold;

	#endregion

	#region Line Heights

	public float LineHeightTight { get; set; } = 1.1f;
	public float LineHeightNormal { get; set; } = 1.4f;
	public float LineHeightRelaxed { get; set; } = 1.6f;
	public float LineHeightLoose { get; set; } = 1.8f;

	#endregion

	#region Letter Spacing

	public float LetterSpacingTight { get; set; } = -0.5f;
	public float LetterSpacingNormal { get; set; } = 0f;
	public float LetterSpacingWide { get; set; } = 1f;
	public float LetterSpacingExtraWide { get; set; } = 2f;

	#endregion

	#region Text Styles - Display (Major Titles - Nodesto)

	/// <summary>Village names, quest titles, book covers</summary>
	public TextStyle DisplayLarge => new() {
		FontSize = GetAdjustedFontSize(FontSizes.DisplayLarge),
		FontWeight = WeightBold,
		LetterSpacing = LetterSpacingWide,
		LineHeight = LineHeightTight,
		FontCategory = FontCategory.Title,
		TextTransformRpg = TextTransformRPG.Uppercase
	};

	/// <summary>Major section headers, chapter titles</summary>
	public TextStyle DisplayMedium => new() {
		FontSize = GetAdjustedFontSize(FontSizes.DisplayMedium),
		FontWeight = WeightBold,
		LetterSpacing = LetterSpacingNormal,
		LineHeight = LineHeightTight,
		FontCategory = FontCategory.Title,
		TextTransformRpg = TextTransformRPG.Uppercase
	};

	/// <summary>Card titles, popup headers</summary>
	public TextStyle DisplaySmall => new() {
		FontSize = GetAdjustedFontSize(FontSizes.DisplaySmall),
		FontWeight = WeightSemiBold,
		LetterSpacing = LetterSpacingNormal,
		LineHeight = LineHeightTight,
		FontCategory = FontCategory.Title
	};

	#endregion

	#region Text Styles - Headlines (Section Headers - Mr Eaves)

	/// <summary>Major panel titles (e.g., "Character Sheet", "Inventory")</summary>
	public TextStyle HeadlineLarge => new() {
		FontSize = GetAdjustedFontSize(FontSizes.HeadlineLarge),
		FontWeight = WeightSemiBold,
		LetterSpacing = LetterSpacingNormal,
		LineHeight = LineHeightNormal,
		FontCategory = FontCategory.Heading
	};

	/// <summary>Section headers (e.g., "Abilities", "Skills", "Equipment")</summary>
	public TextStyle HeadlineMedium => new() {
		FontSize = GetAdjustedFontSize(FontSizes.HeadlineMedium),
		FontWeight = WeightSemiBold,
		LetterSpacing = LetterSpacingNormal,
		LineHeight = LineHeightNormal,
		FontCategory = FontCategory.Heading
	};

	/// <summary>Subsection headers</summary>
	public TextStyle HeadlineSmall => new() {
		FontSize = GetAdjustedFontSize(FontSizes.HeadlineSmall),
		FontWeight = WeightMedium,
		LetterSpacing = LetterSpacingNormal,
		LineHeight = LineHeightNormal,
		FontCategory = FontCategory.Heading
	};

	#endregion

	#region Text Styles - Titles (UI Element Titles - Mr Eaves)

	/// <summary>Dialog titles, tooltip headers</summary>
	public TextStyle TitleLarge => new() {
		FontSize = GetAdjustedFontSize(FontSizes.TitleLarge),
		FontWeight = WeightMedium,
		LetterSpacing = LetterSpacingNormal,
		LineHeight = LineHeightNormal,
		FontCategory = FontCategory.Heading
	};

	/// <summary>Item names, spell names in lists</summary>
	public TextStyle TitleMedium => new() {
		FontSize = GetAdjustedFontSize(FontSizes.TitleMedium),
		FontWeight = WeightMedium,
		LetterSpacing = LetterSpacingNormal,
		LineHeight = LineHeightNormal,
		FontCategory = FontCategory.Heading
	};

	/// <summary>Small titles, compact headers</summary>
	public TextStyle TitleSmall => new() {
		FontSize = GetAdjustedFontSize(FontSizes.TitleSmall),
		FontWeight = WeightMedium,
		LetterSpacing = LetterSpacingNormal,
		LineHeight = LineHeightNormal,
		FontCategory = FontCategory.Heading
	};

	#endregion

	#region Text Styles - Body (Lore, Descriptions - Bookinsanity)

	/// <summary>Long-form lore text, detailed descriptions</summary>
	public TextStyle BodyLarge => new() {
		FontSize = GetAdjustedFontSize(FontSizes.BodyLarge),
		FontWeight = WeightRegular,
		LetterSpacing = LetterSpacingNormal,
		LineHeight = LineHeightRelaxed,
		FontCategory = FontCategory.Body
	};

	/// <summary>Standard body text, spell descriptions, dialogue</summary>
	public TextStyle BodyMedium => new() {
		FontSize = GetAdjustedFontSize(FontSizes.BodyMedium),
		FontWeight = WeightRegular,
		LetterSpacing = LetterSpacingNormal,
		LineHeight = LineHeightNormal,
		FontCategory = FontCategory.Body
	};

	/// <summary>Compact descriptions, tooltips</summary>
	public TextStyle BodySmall => new() {
		FontSize = GetAdjustedFontSize(FontSizes.BodySmall),
		FontWeight = WeightRegular,
		LetterSpacing = LetterSpacingNormal,
		LineHeight = LineHeightNormal,
		FontCategory = FontCategory.Body
	};

	#endregion

	#region Text Styles - Labels (Stats, Tables - Scaly Sans)

	/// <summary>Large stat labels, inventory headers</summary>
	public TextStyle LabelLarge => new() {
		FontSize = GetAdjustedFontSize(FontSizes.LabelLarge),
		FontWeight = WeightMedium,
		LetterSpacing = LetterSpacingWide,
		LineHeight = LineHeightTight,
		FontCategory = FontCategory.Table
	};

	/// <summary>Standard stat labels, table cells</summary>
	public TextStyle LabelMedium => new() {
		FontSize = GetAdjustedFontSize(FontSizes.LabelMedium),
		FontWeight = WeightMedium,
		LetterSpacing = LetterSpacingWide,
		LineHeight = LineHeightTight,
		FontCategory = FontCategory.Table
	};

	/// <summary>Small stat labels, compact tables</summary>
	public TextStyle LabelSmall => new() {
		FontSize = GetAdjustedFontSize(FontSizes.LabelSmall),
		FontWeight = WeightMedium,
		LetterSpacing = LetterSpacingWide,
		LineHeight = LineHeightTight,
		FontCategory = FontCategory.Table
	};

	#endregion

	#region Text Styles - Special Purpose

	/// <summary>Small helper text, footnotes (Scaly Sans)</summary>
	public TextStyle Caption => new() {
		FontSize = GetAdjustedFontSize(FontSizes.Caption),
		FontWeight = WeightRegular,
		LetterSpacing = LetterSpacingNormal,
		LineHeight = LineHeightNormal,
		FontCategory = FontCategory.Table
	};

	/// <summary>Category labels, section dividers (Scaly Sans Caps)</summary>
	public TextStyle Overline => new() {
		FontSize = GetAdjustedFontSize(FontSizes.Overline),
		FontWeight = WeightMedium,
		LetterSpacing = LetterSpacingExtraWide,
		LineHeight = LineHeightTight,
		TextTransformRpg = TextTransformRPG.Uppercase,
		FontCategory = FontCategory.Table
	};

	/// <summary>Button text (Scaly Sans Bold)</summary>
	public TextStyle Button => new() {
		FontSize = GetAdjustedFontSize(FontSizes.Button),
		FontWeight = WeightSemiBold,
		LetterSpacing = LetterSpacingWide,
		LineHeight = LineHeightTight,
		TextTransformRpg = TextTransformRPG.Uppercase,
		FontCategory = FontCategory.Table,
		FontVariant = FontVariant.Bold
	};

	/// <summary>Monospace text - dice notation, coordinates (JetBrains Mono)</summary>
	public TextStyle Monospace => new() {
		FontSize = GetAdjustedFontSize(FontSizes.BodyMedium),
		FontWeight = WeightRegular,
		LetterSpacing = LetterSpacingNormal,
		LineHeight = LineHeightNormal,
		FontCategory = FontCategory.Monospace
	};

	/// <summary>Small monospace - inline code, small stats</summary>
	public TextStyle MonospaceSmall => new() {
		FontSize = GetAdjustedFontSize(FontSizes.BodySmall),
		FontWeight = WeightRegular,
		LetterSpacing = LetterSpacingNormal,
		LineHeight = LineHeightNormal,
		FontCategory = FontCategory.Monospace
	};

	/// <summary>Table header text (Zatanna Misdirection)</summary>
	public TextStyle TableHeader => new() {
		FontSize = GetAdjustedFontSize(FontSizes.LabelMedium),
		FontWeight = WeightMedium,
		LetterSpacing = LetterSpacingWide,
		LineHeight = LineHeightTight,
		FontCategory = FontCategory.TableHeader
	};

	/// <summary>Stat block values - bold numbers (Scaly Sans Bold)</summary>
	public TextStyle StatValue => new() {
		FontSize = GetAdjustedFontSize(FontSizes.LabelLarge),
		FontWeight = WeightBold,
		LetterSpacing = LetterSpacingNormal,
		LineHeight = LineHeightTight,
		FontCategory = FontCategory.Table,
		FontVariant = FontVariant.Bold
	};

	/// <summary>Flavor/italic text in descriptions (Bookinsanity Italic)</summary>
	public TextStyle FlavorText => new() {
		FontSize = GetAdjustedFontSize(FontSizes.BodyMedium),
		FontWeight = WeightRegular,
		LetterSpacing = LetterSpacingNormal,
		LineHeight = LineHeightNormal,
		FontCategory = FontCategory.Body,
		FontVariant = FontVariant.Italic
	};

	/// <summary>Drop cap style for lore paragraphs (Solbera Imitation)</summary>
	public TextStyle DropCap => new() {
		FontSize = GetAdjustedFontSize(FontSizes.DisplayLarge),
		FontWeight = WeightRegular,
		LetterSpacing = LetterSpacingNormal,
		LineHeight = LineHeightTight,
		FontCategory = FontCategory.DropCap
	};

	#endregion

	#region Helper Methods

	private int GetAdjustedFontSize(int baseSize) {
		var accessibility = GameTheme.Current?.Accessibility;
		return accessibility?.GetAdjustedFontSize(baseSize) ?? baseSize;
	}

	/// <summary>
	/// Gets the font asset path for a given category and variant.
	/// </summary>
	public string GetFontPath(FontCategory category, FontVariant variant = FontVariant.Regular) {
		return (category, variant) switch {
			// Body (Bookinsanity)
			(FontCategory.Body, FontVariant.Regular) => BodyFontPath,
			(FontCategory.Body, FontVariant.Bold) => BodyBoldFontPath,
			(FontCategory.Body, FontVariant.Italic) => BodyItalicFontPath,
			(FontCategory.Body, FontVariant.BoldItalic) => BodyBoldItalicFontPath,

			// Heading (Mr Eaves Small Caps)
			(FontCategory.Heading, _) => HeadingFontPath,

			// Table (Scaly Sans)
			(FontCategory.Table, FontVariant.Regular) => TableFontPath,
			(FontCategory.Table, FontVariant.Bold) => TableBoldFontPath,
			(FontCategory.Table, FontVariant.Italic) => TableItalicFontPath,
			(FontCategory.Table, FontVariant.BoldItalic) => TableBoldItalicFontPath,
			(FontCategory.Table, FontVariant.Caps) => TableCapsFontPath,

			// Title (Nodesto)
			(FontCategory.Title, FontVariant.Bold) => TitleBoldFontPath,
			(FontCategory.Title, FontVariant.Italic) => TitleItalicFontPath,
			(FontCategory.Title, FontVariant.BoldItalic) => TitleBoldItalicFontPath,
			(FontCategory.Title, _) => TitleFontPath,

			// Table Header (Zatanna)
			(FontCategory.TableHeader, FontVariant.Bold) => TableHeaderBoldFontPath,
			(FontCategory.TableHeader, FontVariant.Italic) => TableHeaderItalicFontPath,
			(FontCategory.TableHeader, FontVariant.BoldItalic) => TableHeaderBoldItalicFontPath,
			(FontCategory.TableHeader, _) => TableHeaderFontPath,

			// Drop Cap
			(FontCategory.DropCap, _) => DropCapFontPath,

			// Monospace (JetBrains Mono)
			(FontCategory.Monospace, _) => MonospaceFontAssetPath,

			// Fallback
			_ => BodyFontPath
		};
	}

	#endregion

	#region Cloning

	public TypographySettings Clone() {
		return new TypographySettings {
			// Font families
			BodyFontFamily = BodyFontFamily,
			HeadingFontFamily = HeadingFontFamily,
			TableFontFamily = TableFontFamily,
			TitleFontFamily = TitleFontFamily,
			TableHeaderFontFamily = TableHeaderFontFamily,
			DropCapFontFamily = DropCapFontFamily,
			DropCapAltFontFamily = DropCapAltFontFamily,
			MonospaceFontFamily = MonospaceFontFamily,

			// Font paths
			BodyFontPath = BodyFontPath,
			BodyBoldFontPath = BodyBoldFontPath,
			BodyItalicFontPath = BodyItalicFontPath,
			BodyBoldItalicFontPath = BodyBoldItalicFontPath,
			HeadingFontPath = HeadingFontPath,
			TableFontPath = TableFontPath,
			TableBoldFontPath = TableBoldFontPath,
			TableItalicFontPath = TableItalicFontPath,
			TableBoldItalicFontPath = TableBoldItalicFontPath,
			TableCapsFontPath = TableCapsFontPath,
			TitleFontPath = TitleFontPath,
			TitleBoldFontPath = TitleBoldFontPath,
			TitleItalicFontPath = TitleItalicFontPath,
			TitleBoldItalicFontPath = TitleBoldItalicFontPath,
			TableHeaderFontPath = TableHeaderFontPath,
			TableHeaderBoldFontPath = TableHeaderBoldFontPath,
			TableHeaderItalicFontPath = TableHeaderItalicFontPath,
			TableHeaderBoldItalicFontPath = TableHeaderBoldItalicFontPath,
			DropCapFontPath = DropCapFontPath,
			DropCapAltFontPath = DropCapAltFontPath,
			MonospaceFontAssetPath = MonospaceFontAssetPath,

			// Font sizes
			FontSizes = new FontSizeScale {
				DisplayLarge = FontSizes.DisplayLarge,
				DisplayMedium = FontSizes.DisplayMedium,
				DisplaySmall = FontSizes.DisplaySmall,
				HeadlineLarge = FontSizes.HeadlineLarge,
				HeadlineMedium = FontSizes.HeadlineMedium,
				HeadlineSmall = FontSizes.HeadlineSmall,
				TitleLarge = FontSizes.TitleLarge,
				TitleMedium = FontSizes.TitleMedium,
				TitleSmall = FontSizes.TitleSmall,
				BodyLarge = FontSizes.BodyLarge,
				BodyMedium = FontSizes.BodyMedium,
				BodySmall = FontSizes.BodySmall,
				LabelLarge = FontSizes.LabelLarge,
				LabelMedium = FontSizes.LabelMedium,
				LabelSmall = FontSizes.LabelSmall,
				Caption = FontSizes.Caption,
				Overline = FontSizes.Overline,
				Button = FontSizes.Button,
				Tooltip = FontSizes.Tooltip,
				Badge = FontSizes.Badge
			}
		};
	}

	#endregion
}

public class FontSizeScale {
	// Large display text – expedition titles, main menu headers
	public int DisplayLarge { get; set; } = 96;
	public int DisplayMedium { get; set; } = 80;
	public int DisplaySmall { get; set; } = 72;

	// Section headlines – inventory panel titles, quest log headers
	public int HeadlineLarge { get; set; } = 64;
	public int HeadlineMedium { get; set; } = 56;
	public int HeadlineSmall { get; set; } = 48;

	// Item names, stat labels
	public int TitleLarge { get; set; } = 42;
	public int TitleMedium { get; set; } = 38;
	public int TitleSmall { get; set; } = 34;

	// Body text – item descriptions, lore paragraphs
	public int BodyLarge { get; set; } = 36;
	public int BodyMedium { get; set; } = 33;
	public int BodySmall { get; set; } = 30;

	// Buttons, interactive labels
	public int LabelLarge { get; set; } = 34;
	public int LabelMedium { get; set; } = 32;
	public int LabelSmall { get; set; } = 30;

	// Supporting text
	public int Caption { get; set; } = 28;
	public int Overline { get; set; } = 26;

	// UI-specific
	public int Button { get; set; } = 32;
	public int Tooltip { get; set; } = 30;
	public int Badge { get; set; } = 26;
}

public class TextStyle {
	public int FontSize { get; set; }
	public FontWeight FontWeight { get; set; }
	public float LetterSpacing { get; set; }
	public float LineHeight { get; set; }
	public TextTransformRPG TextTransformRpg { get; set; } = TextTransformRPG.None;
	public FontCategory FontCategory { get; set; } = FontCategory.Body;
	public FontVariant FontVariant { get; set; } = FontVariant.Regular;

	// Legacy compatibility
	public bool IsMonospace {
		get => FontCategory == FontCategory.Monospace;
		set { if (value) FontCategory = FontCategory.Monospace; }
	}

	/// <summary>
	/// Applies this text style to a UI Toolkit element's style.
	/// Note: TextTransform is not supported in Unity USS/IStyle.
	/// Use ApplyTextTransform() on the text string for uppercase/lowercase.
	/// </summary>
	public void ApplyTo(IStyle style) {
		style.fontSize = FontSize;
		style.letterSpacing = LetterSpacing;

		// Apply font weight via style
		style.unityFontStyleAndWeight = (FontWeight, FontVariant) switch {
			(FontWeight.Bold, _) => FontStyle.Bold,
			(FontWeight.SemiBold, _) => FontStyle.Bold,
			(_, FontVariant.Bold) => FontStyle.Bold,
			(_, FontVariant.Italic) => FontStyle.Italic,
			(_, FontVariant.BoldItalic) => FontStyle.BoldAndItalic,
			_ => FontStyle.Normal
		};

		// Apply font asset if available
		var fontAsset = GetFontAsset();
		if (fontAsset != null) {
			style.unityFontDefinition = new StyleFontDefinition(fontAsset);
		}
	}

	/// <summary>
	/// Applies text transformation to a string.
	/// Unity UI Toolkit does NOT support CSS text-transform, so this must be
	/// called when setting text content that requires uppercase/lowercase.
	/// </summary>
	public string ApplyTextTransform(string text) {
		if (string.IsNullOrEmpty(text)) return text;

		return TextTransformRpg switch {
			TextTransformRPG.Uppercase => text.ToUpperInvariant(),
			TextTransformRPG.Lowercase => text.ToLowerInvariant(),
			TextTransformRPG.Capitalize => text.ToTitleCase(),
			_ => text
		};
	}

	/// <summary>
	/// Returns true if this style requires text transformation.
	/// </summary>
	public bool RequiresTextTransform => TextTransformRpg != TextTransformRPG.None;

	/// <summary>
	/// Gets the appropriate font asset for this text style.
	/// </summary>
	public FontAsset GetFontAsset() {
		if (!GameServices.IsInitialized) return null;

		var typography = GameTheme.Current?.Typography;
		if (typography == null) return null;

		string path = typography.GetFontPath(FontCategory, FontVariant);
		return GameServices.Assets.GetFontAsset(path);
	}
}

public enum FontWeight { Light = 300, Normal = 400, Medium = 500, SemiBold = 600, Bold = 700 }
public enum TextTransformRPG { None, Uppercase, Lowercase, Capitalize }
namespace RPGGame.UI.Styles;

#nullable disable

public class ComponentStyles {
	public ButtonStyles Button { get; set; } = new();
	public InputStyles Input { get; set; } = new();
	public PanelStyles Panel { get; set; } = new();
	public WindowStyles Window { get; set; } = new();
	public ProgressBarStyles ProgressBar { get; set; } = new();
	public ListStyles List { get; set; } = new();
	public TooltipStyles Tooltip { get; set; } = new();
	public DialogStyles Dialog { get; set; } = new();
	public ScrollViewStyles ScrollView { get; set; } = new();
	public TabStyles Tab { get; set; } = new();
	public BadgeStyles Badge { get; set; } = new();
	public DividerStyles Divider { get; set; } = new();
	public SpinnerStyles Spinner { get; set; } = new();
	public EntityDetailsPanelStyles EntityDetails { get; set; } = new();
	public ItemDetailsPanelStyles ItemDetails { get; set; } = new();
	public StatBarStyles StatBar { get; set; } = new();
	public BiomeDisplayPanelStyles BiomeDisplay { get; set; } = new();
	public SlotStyles Slot { get; set; } = new();
}

public class SlotStyles {
	/// <summary>Extra small slot (status effects in compact views). 4K: 48px</summary>
	public float SizeXS { get; set; } = 48f;

	/// <summary>Small slot (inline status effects, mini displays). 4K: 64px</summary>
	public float SizeS { get; set; } = 64f;

	/// <summary>Medium slot (default inventory, skills). 4K: 96px</summary>
	public float SizeM { get; set; } = 96f;

	/// <summary>Large slot (featured items, combat displays). 4K: 128px</summary>
	public float SizeL { get; set; } = 128f;

	/// <summary>Extra large slot (inspected items, character portraits). 4K: 160px</summary>
	public float SizeXL { get; set; } = 160f;

	/// <summary>Badge font size for stack counts, levels, etc.</summary>
	public float BadgeFontSize { get; set; } = 14f;

	/// <summary>Icon padding within the slot frame.</summary>
	public float IconPadding { get; set; } = 8f;
}

public class ButtonStyles {
	public float MinWidth { get; set; } = 160f;
	public float MinHeight { get; set; } = 72f;
	public float IconSize { get; set; } = 40f;
	public float IconSpacing { get; set; } = 16f;
	public (float Height, int FontSize, PaddingPreset Padding) Small => (56f, 24, new PaddingPreset { Vertical = 8, Horizontal = 24 });
	public (float Height, int FontSize, PaddingPreset Padding) Medium => (72f, 28, new PaddingPreset { Vertical = 16, Horizontal = 32 });
	public (float Height, int FontSize, PaddingPreset Padding) Large => (88f, 32, new PaddingPreset { Vertical = 24, Horizontal = 48 });
}

public class InputStyles {
	public float Height { get; set; } = 80f;
	public float MinWidth { get; set; } = 400f;
	public float LabelSpacing { get; set; } = 12f;
	public float HelperTextSpacing { get; set; } = 8f;
}

public class PanelStyles {
	public float MinWidth { get; set; } = 400f;
	public float MinHeight { get; set; } = 200f;
}

public class WindowStyles {
	public float MinWidth { get; set; } = 600f;
	public float MinHeight { get; set; } = 400f;
	public float HeaderHeight { get; set; } = 96f;
	public float FooterHeight { get; set; } = 112f;
	public float CloseButtonSize { get; set; } = 64f;
}

public class ProgressBarStyles {
	public float Height { get; set; } = 16f;
	public float HeightLarge { get; set; } = 32f;
	public float HeightSmall { get; set; } = 8f;
}

public class ListStyles {
	public float ItemHeight { get; set; } = 96f;
	public float ItemHeightCompact { get; set; } = 72f;
	public float ItemHeightLarge { get; set; } = 128f;
	public float IconSize { get; set; } = 48f;
}

public class TooltipStyles {
	public float MaxWidth { get; set; } = 600f;
	public float ArrowSize { get; set; } = 16f;
	public int ShowDelay { get; set; } = 500;
	public int HideDelay { get; set; } = 100;
}

public class DialogStyles {
	public float MinWidth { get; set; } = 800f;
	public float MaxWidth { get; set; } = 1200f;
	public float ButtonSpacing { get; set; } = 24f;
	public float SmallWidth { get; set; } = 640f;     
	public float LargeWidth { get; set; } = 1280f;    
	public float ExtraLargeWidth { get; set; } = 1600f;

	/// <summary>
	/// Golden ratio height (~61.8% of MaxWidth). Aesthetically pleasing for most dialogs.
	/// </summary>
	public float ContentMaxHeight => MaxWidth * 0.618f;

	/// <summary>
	/// Classic 4:3 ratio height. Good for content-heavy dialogs.
	/// </summary>
	public float ContentMaxHeight43 => MaxWidth * 0.75f;

	/// <summary>
	/// 16:9 ratio height. Good for media-heavy or cinematic dialogs.
	/// </summary>
	public float ContentMaxHeight169 => MaxWidth * 0.5625f;

	/// <summary>
	/// Height capped to reasonable maximum. Uses 70% of MaxWidth but caps at 1800px for 4K.
	/// </summary>
	public float ContentMaxHeightCapped => MathF.Min(MaxWidth * 0.7f, 1800f);

	/// <summary>
	/// Preferred content height with min/max clamping for predictable sizing.
	/// Clamps between 800-1600px for 4K displays.
	/// </summary>
	public float PreferredContentHeight => Math.Clamp(MaxWidth * 0.65f, 800f, 1600f);

	/// <summary>
	/// Height for scrollable content areas (lists, logs, etc.). Based on golden ratio of MinWidth.
	/// </summary>
	public float ScrollableContentHeight => MinWidth * 0.618f;

	/// <summary>
	/// Compact dialog height for simple confirmations/alerts.
	/// </summary>
	public float CompactHeight => SmallWidth * 0.5f;
}

public class ScrollViewStyles {
	public float ScrollbarWidth { get; set; } = 16f;
	public float ScrollbarWidthHover { get; set; } = 24f;
	public float MinThumbLength { get; set; } = 80f;
}

public class TabStyles {
	public float Height { get; set; } = 88f;
	public float MinWidth { get; set; } = 160f;
	public float IndicatorHeight { get; set; } = 6f;
}

public class BadgeStyles {
	public float MinWidth { get; set; } = 40f;
	public float Height { get; set; } = 40f;
	public float HeightSmall { get; set; } = 32f;
	public float HeightMedium { get; set; } = 44f;    
	public float HeightLarge { get; set; } = 56f;     
	public float HeightExtraLarge { get; set; } = 72f; 
}

public class DividerStyles {
	public float Thickness { get; set; } = 2f;
	public float Spacing { get; set; } = 32f;
}

public class SpinnerStyles {
	public float Width { get; set; } = 700f;
	public float ItemSlotSize { get; set; } = 140f;
	public float IconSize { get; set; } = 120f;
	public int DefaultVisibleItems { get; set; } = 5;
	public float HighlightPadding { get; set; } = 4f;
	public float ItemSlotSizeCompact { get; set; } = 100f;
	public float ItemSlotSizeLarge { get; set; } = 180f;
}

public class EntityDetailsPanelStyles {
	public float PortraitSize { get; set; } = 48f;
	public float PortraitSizeLarge { get; set; } = 64f;
	public float BarHeight { get; set; } = 14f;
	public float BarHeightCompact { get; set; } = 10f;
	public float CompactWidth { get; set; } = 280f;
	public float SummaryWidth { get; set; } = 320f;
	public float FullWidth { get; set; } = 400f;
}

public class ItemDetailsPanelStyles {
	public float IconSize { get; set; } = 40f;
	public float IconSizeLarge { get; set; } = 56f;
	public float CompactWidth { get; set; } = 240f;
	public float SummaryWidth { get; set; } = 300f;
	public float FullWidth { get; set; } = 380f;
	public int CompactMaxStats { get; set; } = 3;
}

public class StatBarStyles {
	public float Height { get; set; } = 16f;
	public float HeightCompact { get; set; } = 10f;
	public float HeightLarge { get; set; } = 24f;
	public float LabelSpacing { get; set; } = 4f;
	public float GlowOpacityMin { get; set; } = 0.1f;
	public float GlowOpacityMax { get; set; } = 0.4f;
	public float PulseDurationSeconds { get; set; } = 1.0f;
	public int AnimationDurationMs { get; set; } = 300;
	public int AnimationFrameMs { get; set; } = 16;
	public int PulseFrameMs { get; set; } = 32;
	public float WarningThreshold { get; set; } = 0.3f;
	public float DangerThreshold { get; set; } = 0.15f;
}

public class BiomeDisplayPanelStyles {
	public float CompactWidth { get; set; } = 200f;
	public float ExpandedWidth { get; set; } = 260f;
	public float IconSize { get; set; } = 32f;
	public float StatRowHeight { get; set; } = 20f;
}
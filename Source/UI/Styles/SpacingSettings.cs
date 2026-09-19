using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Styles;

#nullable disable

public class SpacingSettings {
	public int BaseUnit { get; set; } = 8;

	public int None => 0;
	public int XXS => BaseUnit;
	public int XS => BaseUnit * 2;
	public int SM => BaseUnit * 3;
	public int MD => BaseUnit * 4;
	public int LG => BaseUnit * 6;
	public int XL => BaseUnit * 8;
	public int XXL => BaseUnit * 12;
	public int XXXL => BaseUnit * 16;

	public PaddingPreset ButtonPadding => new() { Vertical = XS, Horizontal = MD };
	public PaddingPreset ButtonPaddingSmall => new() { Vertical = XXS, Horizontal = XS };
	public PaddingPreset ButtonPaddingLarge => new() { Vertical = SM, Horizontal = LG };
	public PaddingPreset InputPadding => new() { Vertical = XS, Horizontal = SM };
	public PaddingPreset CardPadding => new() { Vertical = MD, Horizontal = MD };
	public PaddingPreset DialogPadding => new() { Vertical = LG, Horizontal = LG };
	public PaddingPreset TooltipPadding => new() { Vertical = XS, Horizontal = SM };
	public PaddingPreset WindowPadding => new() { Vertical = MD, Horizontal = MD };
	public PaddingPreset PanelPadding => new() { Vertical = SM, Horizontal = SM };

	public int GapXS => XS;
	public int GapSM => SM;
	public int GapMD => MD;
	public int GapLG => LG;

	public MarginPreset SectionMargin => new() { Top = XL, Bottom = XL };
	public MarginPreset ElementMargin => new() { Bottom = SM };
	public MarginPreset ListItemMargin => new() { Bottom = XS };

	public int GetScaled(int value) {
		float scale = GameTheme.Current?.Accessibility?.UIScale ?? 1f;
		return Mathf.RoundToInt(value * scale);
	}
}

public struct PaddingPreset {
	public int Vertical { get; set; }
	public int Horizontal { get; set; }
	public int Top => Vertical;
	public int Bottom => Vertical;
	public int Left => Horizontal;
	public int Right => Horizontal;

	public void ApplyTo(IStyle style) {
		float scale = GameTheme.Current?.Accessibility?.UIScale ?? 1f;
		style.paddingTop = Mathf.RoundToInt(Top * scale);
		style.paddingBottom = Mathf.RoundToInt(Bottom * scale);
		style.paddingLeft = Mathf.RoundToInt(Left * scale);
		style.paddingRight = Mathf.RoundToInt(Right * scale);
	}
}

public struct MarginPreset {
	public int Top { get; set; }
	public int Bottom { get; set; }
	public int Left { get; set; }
	public int Right { get; set; }

	public void ApplyTo(IStyle style) {
		float scale = GameTheme.Current?.Accessibility?.UIScale ?? 1f;
		style.marginTop = Mathf.RoundToInt(Top * scale);
		style.marginBottom = Mathf.RoundToInt(Bottom * scale);
		style.marginLeft = Mathf.RoundToInt(Left * scale);
		style.marginRight = Mathf.RoundToInt(Right * scale);
	}
}
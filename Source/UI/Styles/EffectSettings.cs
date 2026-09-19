using UnityEngine;

namespace RPGGame.UI.Styles;

#nullable disable

public class EffectSettings {
	public ShadowPreset ShadowNone => new() { OffsetX = 0, OffsetY = 0, Blur = 0, Spread = 0, Color = Color.clear };
	public ShadowPreset ShadowSM { get; set; } = new() { OffsetX = 0, OffsetY = 1, Blur = 3, Spread = 0, Color = new Color(0, 0, 0, 0.12f) };
	public ShadowPreset ShadowMD { get; set; } = new() { OffsetX = 0, OffsetY = 4, Blur = 6, Spread = -1, Color = new Color(0, 0, 0, 0.15f) };
	public ShadowPreset ShadowLG { get; set; } = new() { OffsetX = 0, OffsetY = 10, Blur = 15, Spread = -3, Color = new Color(0, 0, 0, 0.2f) };
	public ShadowPreset ShadowXL { get; set; } = new() { OffsetX = 0, OffsetY = 20, Blur = 25, Spread = -5, Color = new Color(0, 0, 0, 0.25f) };

	public ShadowPreset CardShadow => ShadowMD;
	public ShadowPreset DropdownShadow => ShadowLG;
	public ShadowPreset ModalShadow => ShadowXL;
	public ShadowPreset TooltipShadow => ShadowSM;
	public ShadowPreset ButtonShadow => ShadowSM;

	public GlowPreset GlowPrimary { get; set; } = new() { Color = new Color(0.2f, 0.6f, 0.9f, 0.4f), Blur = 8, Spread = 2 };
	public GlowPreset GlowSuccess { get; set; } = new() { Color = new Color(0.3f, 0.85f, 0.4f, 0.4f), Blur = 8, Spread = 2 };
	public GlowPreset GlowError { get; set; } = new() { Color = new Color(0.95f, 0.3f, 0.3f, 0.4f), Blur = 8, Spread = 2 };
	public GlowPreset GlowWarning { get; set; } = new() { Color = new Color(1f, 0.75f, 0.2f, 0.4f), Blur = 8, Spread = 2 };

	public float BlurNone => 0f;
	public float BlurLight { get; set; } = 4f;
	public float BlurMedium { get; set; } = 8f;
	public float BlurHeavy { get; set; } = 16f;

	public EffectSettings Clone() {
		return new EffectSettings {
			ShadowSM = ShadowSM, ShadowMD = ShadowMD, ShadowLG = ShadowLG, ShadowXL = ShadowXL,
			GlowPrimary = GlowPrimary, GlowSuccess = GlowSuccess, GlowError = GlowError, GlowWarning = GlowWarning,
			BlurLight = BlurLight, BlurMedium = BlurMedium, BlurHeavy = BlurHeavy
		};
	}
}

public struct ShadowPreset {
	public float OffsetX { get; set; }
	public float OffsetY { get; set; }
	public float Blur { get; set; }
	public float Spread { get; set; }
	public Color Color { get; set; }
}

public struct GlowPreset {
	public Color Color { get; set; }
	public float Blur { get; set; }
	public float Spread { get; set; }
}
namespace RPGGame.UI.Styles;

#nullable disable

public class AnimationSettings {
	public int DurationInstant { get; set; } = 0;
	public int DurationFast { get; set; } = 100;
	public int DurationNormal { get; set; } = 200;
	public int DurationSlow { get; set; } = 300;
	public int DurationVerySlow { get; set; } = 500;

	public string EaseLinear => "linear";
	public string EaseIn => "ease-in";
	public string EaseOut => "ease-out";
	public string EaseInOut => "ease-in-out";
	public string EaseBounce => "ease-out-back";

	public int GetAdjustedDuration(int baseDuration) {
		return GameTheme.Current?.Accessibility?.GetAdjustedAnimationDuration(baseDuration) ?? baseDuration;
	}

	public TransitionPreset FadeIn => new() { Property = "opacity", Duration = GetAdjustedDuration(DurationNormal), Easing = EaseOut };
	public TransitionPreset ScaleUp => new() { Property = "scale", Duration = GetAdjustedDuration(DurationFast), Easing = EaseOut };
	public TransitionPreset SlideIn => new() { Property = "translate", Duration = GetAdjustedDuration(DurationNormal), Easing = EaseOut };
	public TransitionPreset ButtonHover => new() { Property = "background-color", Duration = GetAdjustedDuration(DurationFast), Easing = EaseOut };
	public TransitionPreset ColorChange => new() { Property = "color", Duration = GetAdjustedDuration(DurationFast), Easing = EaseOut };

	public AnimationSettings Clone() {
		return new AnimationSettings {
			DurationInstant = DurationInstant, DurationFast = DurationFast, DurationNormal = DurationNormal,
			DurationSlow = DurationSlow, DurationVerySlow = DurationVerySlow
		};
	}
}

public struct TransitionPreset {
	public string Property { get; set; }
	public int Duration { get; set; }
	public string Easing { get; set; }
}
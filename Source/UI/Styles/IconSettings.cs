namespace RPGGame.UI.Styles;

#nullable disable

public class IconSettings {
	public float SizeXS { get; set; } = 12f;
	public float SizeSM { get; set; } = 16f;
	public float SizeMD { get; set; } = 20f;
	public float SizeLG { get; set; } = 24f;
	public float SizeXL { get; set; } = 32f;
	public float SizeXXL { get; set; } = 48f;

	public float ButtonIcon => SizeMD;
	public float MenuIcon => SizeLG;
	public float ListItemIcon => SizeLG;
	public float HeaderIcon => SizeXL;
	public float EmptyStateIcon => SizeXXL;

	public float GetScaled(float baseSize) {
		float scale = GameTheme.Current?.Accessibility?.UIScale ?? 1f;
		return baseSize * scale;
	}

	public IconSettings Clone() {
		return new IconSettings {
			SizeXS = SizeXS, SizeSM = SizeSM, SizeMD = SizeMD,
			SizeLG = SizeLG, SizeXL = SizeXL, SizeXXL = SizeXXL
		};
	}
}
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Styles;

#nullable disable

public class BorderSettings {
	public float WidthNone => 0f;
	public float WidthThin { get; set; } = 1f;
	public float WidthMedium { get; set; } = 2f;
	public float WidthThick { get; set; } = 3f;
	public float WidthHeavy { get; set; } = 4f;

	public float RadiusNone => 0f;
	public float RadiusXS { get; set; } = 2f;
	public float RadiusSM { get; set; } = 4f;
	public float RadiusMD { get; set; } = 8f;
	public float RadiusLG { get; set; } = 12f;
	public float RadiusXL { get; set; } = 16f;
	public float RadiusXXL { get; set; } = 24f;
	public float RadiusFull { get; set; } = 9999f;

	public float ButtonRadius { get; set; } = 6f;
	public float InputRadius { get; set; } = 4f;
	public float CardRadius { get; set; } = 8f;
	public float DialogRadius { get; set; } = 12f;
	public float TooltipRadius { get; set; } = 4f;
	public float BadgeRadius { get; set; } = 9999f;
	public float ProgressBarRadius { get; set; } = 4f;

	public void ApplyRadius(IStyle style, float radius) {
		style.borderTopLeftRadius = radius;
		style.borderTopRightRadius = radius;
		style.borderBottomLeftRadius = radius;
		style.borderBottomRightRadius = radius;
	}

	public void ApplyWidth(IStyle style, float width) {
		style.borderTopWidth = width;
		style.borderBottomWidth = width;
		style.borderLeftWidth = width;
		style.borderRightWidth = width;
	}

	public void ApplyColor(IStyle style, Color color) {
		style.borderTopColor = color;
		style.borderBottomColor = color;
		style.borderLeftColor = color;
		style.borderRightColor = color;
	}

	public void ApplyBorder(IStyle style, float width, Color color, float radius) {
		ApplyWidth(style, width);
		ApplyColor(style, color);
		ApplyRadius(style, radius);
	}

	public BorderSettings Clone() {
		return new BorderSettings {
			WidthThin = WidthThin, WidthMedium = WidthMedium, WidthThick = WidthThick, WidthHeavy = WidthHeavy,
			RadiusXS = RadiusXS, RadiusSM = RadiusSM, RadiusMD = RadiusMD, RadiusLG = RadiusLG,
			RadiusXL = RadiusXL, RadiusXXL = RadiusXXL, RadiusFull = RadiusFull,
			ButtonRadius = ButtonRadius, InputRadius = InputRadius, CardRadius = CardRadius,
			DialogRadius = DialogRadius, TooltipRadius = TooltipRadius, BadgeRadius = BadgeRadius, ProgressBarRadius = ProgressBarRadius
		};
	}
}
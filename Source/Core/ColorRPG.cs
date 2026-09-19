using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.Core;

/// <summary>
/// An immutable, HSL-based color wrapper designed for RPGs and roguelikes.
/// Stores color internally as HSL (Hue 0-360, Saturation 0-1, Lightness 0-1, Alpha 0-1)
/// for intuitive manipulation, while providing seamless implicit conversion to UnityEngine.Color.
/// 
/// <example>
/// <code>
/// // All of these just work:
/// ColorRPG fire = "#FF4500";
/// ColorRPG ice = Color.cyan;
/// ColorRPG poison = 0xFF7FFF00u;
/// ColorRPG blood = "blood";
/// ColorRPG health = ColorRPG.HealthGreen;
/// ColorRPG darkRed = ColorRPG.Blood.Darken(0.3f);
/// Gizmos.color = ColorRPG.Fire;  // Implicit conversion!
/// </code>
/// </example>
/// </summary>
[Serializable]
public readonly struct ColorRPG : IEquatable<ColorRPG>, IFormattable {
	#region Fields

	/// <summary>Hue component (0-360 degrees).</summary>
	private readonly float _hue;

	/// <summary>Saturation component (0-1).</summary>
	private readonly float _saturation;

	/// <summary>Lightness component (0-1).</summary>
	private readonly float _lightness;

	/// <summary>Alpha component (0-1).</summary>
	private readonly float _alpha;

	/// <summary>Epsilon for floating point comparisons.</summary>
	private const float EPSILON = 0.001f;

	#endregion

	#region Constructors

	/// <summary>
	/// Creates a new ColorRPG from HSL values.
	/// </summary>
	/// <param name="hue">Hue in degrees (0-360, wraps automatically).</param>
	/// <param name="saturation">Saturation (0-1, clamped).</param>
	/// <param name="lightness">Lightness (0-1, clamped).</param>
	/// <param name="alpha">Alpha transparency (0-1, clamped).</param>
	private ColorRPG(float hue, float saturation, float lightness, float alpha) {
		_hue = WrapHue(hue);
		_saturation = Mathf.Clamp01(saturation);
		_lightness = Mathf.Clamp01(lightness);
		_alpha = Mathf.Clamp01(alpha);
	}

	#endregion

	#region Properties

	/// <summary>Gets the HSL components as a tuple.</summary>
	public (float h, float s, float l, float a) HSL => (_hue, _saturation, _lightness, _alpha);

	/// <summary>Gets the hue component (0-360).</summary>
	public float Hue => _hue;

	/// <summary>Gets the saturation component (0-1).</summary>
	public float Saturation => _saturation;

	/// <summary>Gets the lightness component (0-1).</summary>
	public float Lightness => _lightness;

	/// <summary>Gets the alpha component (0-1).</summary>
	public float Alpha => _alpha;

	/// <summary>Gets whether this color is grayscale (saturation near zero).</summary>
	public bool IsGrayscale => _saturation < EPSILON;

	/// <summary>Gets whether this color is fully transparent.</summary>
	public bool IsTransparent => _alpha < EPSILON;

	/// <summary>Gets whether this color is fully opaque.</summary>
	public bool IsOpaque => _alpha > 1f - EPSILON;

	#endregion

	#region Implicit Conversions

	/// <summary>
	/// Implicitly converts ColorRPG to UnityEngine.Color.
	/// This is the magic that makes ColorRPG a seamless drop-in replacement.
	/// </summary>
	/// <param name="c">The ColorRPG to convert.</param>
	/// <returns>A Unity Color.</returns>
	public static implicit operator Color(ColorRPG c) => c.ToUnityColor();

	/// <summary>
	/// Implicitly converts a hex string to ColorRPG.
	/// </summary>
	/// <param name="hex">Hex color string (e.g., "#FF4500" or "FF4500").</param>
	/// <returns>A ColorRPG.</returns>
	public static implicit operator ColorRPG(string hex) {
		// Check if it's a named color first
		if (s_namedColors.TryGetValue(hex.ToLowerInvariant(), out var named)) {
			return named;
		}
		return FromHex(hex);
	}

	/// <summary>
	/// Implicitly converts a uint ARGB value to ColorRPG.
	/// </summary>
	/// <param name="argb">ARGB color as uint (e.g., 0xFF7FFF00u).</param>
	/// <returns>A ColorRPG.</returns>
	public static implicit operator ColorRPG(uint argb) => FromARGB(argb);

	/// <summary>
	/// Implicitly converts UnityEngine.Color to ColorRPG.
	/// </summary>
	/// <param name="c">The Unity Color to convert.</param>
	/// <returns>A ColorRPG.</returns>
	public static implicit operator ColorRPG(Color c) => FromUnityColor(c);

	/// <summary>
	/// Implicit conversion to StyleColor for UIElements.
	/// This avoids the need for .ToStyleColor() everywhere.
	/// </summary>
	public static implicit operator StyleColor(ColorRPG color)
	{
		return new StyleColor(color); // uses the existing ColorRPG → Color implicit
	}

	#endregion

	#region Factory Methods

	/// <summary>
	/// Creates a ColorRPG from a hex color string.
	/// </summary>
	/// <param name="hex">Hex string with or without # prefix. Supports RGB, RGBA, RRGGBB, RRGGBBAA.</param>
	/// <returns>A ColorRPG.</returns>
	/// <example>
	/// <code>
	/// var red = ColorRPG.FromHex("#FF0000");
	/// var semiTransparent = ColorRPG.FromHex("FF000080");
	/// </code>
	/// </example>
	public static ColorRPG FromHex(string hex) {
		if (string.IsNullOrEmpty(hex)) {
			return Black;
		}

		hex = hex.TrimStart('#');

		// Expand shorthand (RGB ? RRGGBB, RGBA ? RRGGBBAA)
		if (hex.Length == 3) {
			hex = $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}";
		} else if (hex.Length == 4) {
			hex = $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}{hex[3]}{hex[3]}";
		}

		if (hex.Length < 6) {
			return Black;
		}

		byte r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
		byte g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
		byte b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
		byte a = hex.Length >= 8 ? byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber) : (byte)255;

		return FromUnityColor(new Color(r / 255f, g / 255f, b / 255f, a / 255f));
	}

	/// <summary>
	/// Creates a ColorRPG from an ARGB uint value.
	/// </summary>
	/// <param name="argb">Color as 0xAARRGGBB.</param>
	/// <returns>A ColorRPG.</returns>
	public static ColorRPG FromARGB(uint argb) {
		byte a = (byte)((argb >> 24) & 0xFF);
		byte r = (byte)((argb >> 16) & 0xFF);
		byte g = (byte)((argb >> 8) & 0xFF);
		byte b = (byte)(argb & 0xFF);
		return FromUnityColor(new Color(r / 255f, g / 255f, b / 255f, a / 255f));
	}

	/// <summary>
	/// Creates a ColorRPG from HSL values.
	/// </summary>
	/// <param name="hue360">Hue in degrees (0-360).</param>
	/// <param name="saturation">Saturation (0-1).</param>
	/// <param name="lightness">Lightness (0-1).</param>
	/// <param name="alpha">Alpha (0-1).</param>
	/// <returns>A ColorRPG.</returns>
	public static ColorRPG FromHSL(float hue360, float saturation = 1f, float lightness = 0.5f, float alpha = 1f) {
		return new ColorRPG(hue360, saturation, lightness, alpha);
	}

	/// <summary>
	/// Creates a ColorRPG from a Unity Color (RGB ? HSL conversion).
	/// </summary>
	/// <param name="c">The Unity Color.</param>
	/// <returns>A ColorRPG.</returns>
	public static ColorRPG FromUnityColor(Color c) {
		float r = c.r;
		float g = c.g;
		float b = c.b;

		float max = Mathf.Max(r, Mathf.Max(g, b));
		float min = Mathf.Min(r, Mathf.Min(g, b));
		float delta = max - min;

		float h = 0f;
		float s = 0f;
		float l = (max + min) / 2f;

		if (delta > EPSILON) {
			s = l > 0.5f ? delta / (2f - max - min) : delta / (max + min);

			if (Mathf.Abs(max - r) < EPSILON) {
				h = ((g - b) / delta + (g < b ? 6f : 0f)) * 60f;
			} else if (Mathf.Abs(max - g) < EPSILON) {
				h = ((b - r) / delta + 2f) * 60f;
			} else {
				h = ((r - g) / delta + 4f) * 60f;
			}
		}

		return new ColorRPG(h, s, l, c.a);
	}

	/// <summary>
	/// Creates a ColorRPG from a named color.
	/// </summary>
	/// <param name="name">Color name (case-insensitive). E.g., "fire", "blood", "manablue".</param>
	/// <returns>A ColorRPG, or Black if name not found.</returns>
	public static ColorRPG FromName(string name) {
		if (string.IsNullOrEmpty(name)) {
			return Black;
		}

		return s_namedColors.TryGetValue(name.ToLowerInvariant(), out var color) ? color : Black;
	}

	/// <summary>
	/// Creates a random fully saturated color.
	/// </summary>
	public static ColorRPG RandomSaturated() {
		var rng = GameRandom.FromSeed((uint)DateTime.UtcNow.Ticks);
		return FromHSL(rng.NextFloat() * 360f, 1f, 0.5f);
	}

	/// <summary>
	/// Creates a random pastel color (high lightness, medium saturation).
	/// </summary>
	public static ColorRPG RandomPastel() {
		var rng = GameRandom.FromSeed((uint)DateTime.UtcNow.Ticks);
		return FromHSL(
				rng.NextFloat() * 360f,
				0.4f + rng.NextFloat() * 0.3f,
				0.7f + rng.NextFloat() * 0.15f
			);
	}

	/// <summary>
	/// Returns a random color from one of the built-in palettes.
	/// </summary>
	public static ColorRPG RandomPalette() {
		var rng = GameRandom.FromSeed((uint)DateTime.UtcNow.Ticks);
		var palettes = new[] { 
			DarkSoulsPalette, DiabloPalette, CyberNeon, GruvboxDark, 
			ClassicRoguelike, VampirePalette, EldritchPalette 
		};
		var palette = rng.Pick(palettes);
		return rng.Pick(palette);
	}

	#endregion

	#region Conversion Methods

	/// <summary>
	/// Converts this ColorRPG to a Unity Color (HSL ? RGB).
	/// </summary>
	/// <returns>A UnityEngine.Color.</returns>
	public Color ToUnityColor() {
		float h = _hue / 360f;
		float s = _saturation;
		float l = _lightness;

		if (s < EPSILON) {
			return new Color(l, l, l, _alpha);
		}

		float q = l < 0.5f ? l * (1f + s) : l + s - l * s;
		float p = 2f * l - q;

		float r = HueToRgb(p, q, h + 1f / 3f);
		float g = HueToRgb(p, q, h);
		float b = HueToRgb(p, q, h - 1f / 3f);

		return new Color(r, g, b, _alpha);
	}

	private static float HueToRgb(float p, float q, float t) {
		if (t < 0f) t += 1f;
		if (t > 1f) t -= 1f;
		if (t < 1f / 6f) return p + (q - p) * 6f * t;
		if (t < 1f / 2f) return q;
		if (t < 2f / 3f) return p + (q - p) * (2f / 3f - t) * 6f;
		return p;
	}

	/// <summary>
	/// Converts this color to a hex string.
	/// </summary>
	/// <param name="includeHash">Whether to prefix with #.</param>
	/// <param name="includeAlpha">Whether to include alpha component.</param>
	/// <returns>Hex string representation.</returns>
	public string ToHex(bool includeHash = true, bool includeAlpha = false) {
		Color c = ToUnityColor();
		byte r = (byte)Mathf.RoundToInt(c.r * 255);
		byte g = (byte)Mathf.RoundToInt(c.g * 255);
		byte b = (byte)Mathf.RoundToInt(c.b * 255);
		byte a = (byte)Mathf.RoundToInt(c.a * 255);

		string prefix = includeHash ? "#" : "";
		return includeAlpha || a < 255
			? $"{prefix}{r:X2}{g:X2}{b:X2}{a:X2}"
			: $"{prefix}{r:X2}{g:X2}{b:X2}";
	}

	#endregion

	#region Chainable Manipulation Methods

	/// <summary>
	/// Returns a new ColorRPG with the specified hue.
	/// </summary>
	/// <param name="hue360">New hue in degrees (0-360).</param>
	/// <returns>A new ColorRPG.</returns>
	public ColorRPG WithHue(float hue360) => new(hue360, _saturation, _lightness, _alpha);

	/// <summary>
	/// Returns a new ColorRPG with the specified saturation.
	/// </summary>
	/// <param name="saturation">New saturation (0-1).</param>
	/// <returns>A new ColorRPG.</returns>
	public ColorRPG WithSaturation(float saturation) => new(_hue, saturation, _lightness, _alpha);

	/// <summary>
	/// Returns a new ColorRPG with the specified lightness.
	/// </summary>
	/// <param name="lightness">New lightness (0-1).</param>
	/// <returns>A new ColorRPG.</returns>
	public ColorRPG WithLightness(float lightness) => new(_hue, _saturation, lightness, _alpha);

	/// <summary>
	/// Returns a new ColorRPG with the specified alpha.
	/// </summary>
	/// <param name="alpha">New alpha (0-1).</param>
	/// <returns>A new ColorRPG.</returns>
	public ColorRPG WithAlpha(float alpha) => new(_hue, _saturation, _lightness, alpha);

	/// <summary>
	/// Rotates the hue by the specified degrees.
	/// </summary>
	/// <param name="degrees">Degrees to rotate (can be negative).</param>
	/// <returns>A new ColorRPG with rotated hue.</returns>
	public ColorRPG RotateHue(float degrees) => new(_hue + degrees, _saturation, _lightness, _alpha);

	/// <summary>
	/// Increases lightness by the specified amount.
	/// </summary>
	/// <param name="amount">Amount to add to lightness (0-1).</param>
	/// <returns>A lighter ColorRPG.</returns>
	public ColorRPG Lighten(float amount) => new(_hue, _saturation, _lightness + amount, _alpha);

	/// <summary>
	/// Decreases lightness by the specified amount.
	/// </summary>
	/// <param name="amount">Amount to subtract from lightness (0-1).</param>
	/// <returns>A darker ColorRPG.</returns>
	public ColorRPG Darken(float amount) => new(_hue, _saturation, _lightness - amount, _alpha);

	/// <summary>
	/// Increases saturation by the specified amount.
	/// </summary>
	/// <param name="amount">Amount to add to saturation (0-1).</param>
	/// <returns>A more saturated ColorRPG.</returns>
	public ColorRPG Saturate(float amount) => new(_hue, _saturation + amount, _lightness, _alpha);

	/// <summary>
	/// Decreases saturation by the specified amount.
	/// </summary>
	/// <param name="amount">Amount to subtract from saturation (0-1).</param>
	/// <returns>A less saturated ColorRPG.</returns>
	public ColorRPG Desaturate(float amount) => new(_hue, _saturation - amount, _lightness, _alpha);

	/// <summary>
	/// Tints the color toward white by the specified strength.
	/// </summary>
	/// <param name="strength">Tint strength (0 = no change, 1 = pure white).</param>
	/// <returns>A tinted ColorRPG.</returns>
	public ColorRPG Tint(float strength) {
		float newLightness = Mathf.Lerp(_lightness, 1f, strength);
		float newSaturation = Mathf.Lerp(_saturation, 0f, strength * 0.5f);
		return new ColorRPG(_hue, newSaturation, newLightness, _alpha);
	}

	/// <summary>
	/// Shades the color toward black by the specified strength.
	/// </summary>
	/// <param name="strength">Shade strength (0 = no change, 1 = pure black).</param>
	/// <returns>A shaded ColorRPG.</returns>
	public ColorRPG Shade(float strength) {
		float newLightness = Mathf.Lerp(_lightness, 0f, strength);
		return new ColorRPG(_hue, _saturation, newLightness, _alpha);
	}

	/// <summary>
	/// Converts this color to grayscale.
	/// </summary>
	/// <returns>A grayscale ColorRPG.</returns>
	public ColorRPG Grayscale() => new(_hue, 0f, _lightness, _alpha);

	/// <summary>
	/// Returns the complementary color (180° hue rotation).
	/// </summary>
	/// <returns>The complementary ColorRPG.</returns>
	public ColorRPG Complementary() => RotateHue(180f);

	/// <summary>
	/// Returns a triadic color scheme (this color + two 120° rotations).
	/// </summary>
	/// <returns>Array of 3 ColorRPGs forming a triadic scheme.</returns>
	public ColorRPG[] Triadic() => new[] {
		this,
		RotateHue(120f),
		RotateHue(240f)
	};

	/// <summary>
	/// Returns analogous colors (neighboring hues).
	/// </summary>
	/// <param name="steps">Number of colors to generate.</param>
	/// <param name="spread">Total degrees to spread across.</param>
	/// <returns>Array of analogous ColorRPGs.</returns>
	public ColorRPG[] Analogous(int steps = 3, float spread = 60f) {
		var result = new ColorRPG[steps];
		float stepSize = spread / (steps - 1);
		float startOffset = -spread / 2f;

		for (int i = 0; i < steps; i++) {
			result[i] = RotateHue(startOffset + stepSize * i);
		}
		return result;
	}

	/// <summary>
	/// Returns a split-complementary color scheme.
	/// </summary>
	/// <returns>Array of 3 ColorRPGs forming a split-complementary scheme.</returns>
	public ColorRPG[] SplitComplementary() => new[] {
		this,
		RotateHue(150f),
		RotateHue(210f)
	};

	/// <summary>
	/// Returns a tetradic (square) color scheme.
	/// </summary>
	/// <returns>Array of 4 ColorRPGs forming a square scheme.</returns>
	public ColorRPG[] Tetradic() => new[] {
		this,
		RotateHue(90f),
		RotateHue(180f),
		RotateHue(270f)
	};

	/// <summary>
	/// Interpolates between this color and another in HSL space.
	/// </summary>
	/// <param name="other">Target color.</param>
	/// <param name="t">Interpolation factor using Percent type.</param>
	/// <returns>Interpolated ColorRPG.</returns>
	public ColorRPG LerpHSL(ColorRPG other, Percent t) {
		float factor = t;

		// Handle hue wraparound (take shortest path)
		float hueDiff = other._hue - _hue;
		if (hueDiff > 180f) hueDiff -= 360f;
		if (hueDiff < -180f) hueDiff += 360f;

		return new ColorRPG(
			_hue + hueDiff * factor,
			Mathf.Lerp(_saturation, other._saturation, factor),
			Mathf.Lerp(_lightness, other._lightness, factor),
			Mathf.Lerp(_alpha, other._alpha, factor)
		);
	}

	/// <summary>
	/// Interpolates between this color and another in HSL space.
	/// </summary>
	/// <param name="other">Target color.</param>
	/// <param name="t">Interpolation factor (0-1).</param>
	/// <returns>Interpolated ColorRPG.</returns>
	public ColorRPG LerpHSL(ColorRPG other, float t) {
		float hueDiff = other._hue - _hue;
		if (hueDiff > 180f) hueDiff -= 360f;
		if (hueDiff < -180f) hueDiff += 360f;

		return new ColorRPG(
			_hue + hueDiff * t,
			Mathf.Lerp(_saturation, other._saturation, t),
			Mathf.Lerp(_lightness, other._lightness, t),
			Mathf.Lerp(_alpha, other._alpha, t)
		);
	}

	/// <summary>
	/// Adds random hue variation within the specified range.
	/// </summary>
	public ColorRPG WithRandomHueVariation(float maxDegrees = 15f) {
		var rng = GameRandom.FromSeed((uint)DateTime.UtcNow.Ticks);
		float variation = (rng.NextFloat() * 2f - 1f) * maxDegrees;
		return RotateHue(variation);
	}

	/// <summary>
	/// Adds random saturation/lightness variation.
	/// </summary>
	public ColorRPG WithRandomVariation(float maxVariation = 0.1f) {
		var rng = GameRandom.FromSeed((uint)DateTime.UtcNow.Ticks);
		float satVar = (rng.NextFloat() * 2f - 1f) * maxVariation;
		float lightVar = (rng.NextFloat() * 2f - 1f) * maxVariation;
		return new ColorRPG(_hue, _saturation + satVar, _lightness + lightVar, _alpha);
	}

	/// <summary>
	/// Creates a color that contrasts well with this one for text readability.
	/// </summary>
	/// <returns>A contrasting ColorRPG (black or white).</returns>
	public ColorRPG ContrastingTextColor() {
		return _lightness > 0.5f ? Black : White;
	}

	/// <summary>
	/// Inverts the color (complementary with inverted lightness).
	/// </summary>
	/// <returns>An inverted ColorRPG.</returns>
	public ColorRPG Invert() => new(_hue + 180f, _saturation, 1f - _lightness, _alpha);

	#endregion

	#region Utility Methods

	private static float WrapHue(float hue) {
		hue %= 360f;
		return hue < 0 ? hue + 360f : hue;
	}

	#endregion

	#region Equality & Comparison

	/// <summary>
	/// Determines if two ColorRPGs are equal within epsilon tolerance.
	/// </summary>
	public bool Equals(ColorRPG other) {
		return Math.Abs(_hue - other._hue) < EPSILON * 360 &&
		       Math.Abs(_saturation - other._saturation) < EPSILON &&
		       Math.Abs(_lightness - other._lightness) < EPSILON &&
		       Math.Abs(_alpha - other._alpha) < EPSILON;
	}

	/// <inheritdoc/>
	public override bool Equals(object? obj) => obj is ColorRPG other && Equals(other);

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(_hue, _saturation, _lightness, _alpha);

	/// <summary>Equality operator.</summary>
	public static bool operator ==(ColorRPG left, ColorRPG right) => left.Equals(right);

	/// <summary>Inequality operator.</summary>
	public static bool operator !=(ColorRPG left, ColorRPG right) => !left.Equals(right);

	#endregion

	#region Formatting

	/// <inheritdoc/>
	public override string ToString() => ToHex();

	/// <summary>
	/// Formats the color according to the specified format string.
	/// </summary>
	/// <param name="format">
	/// "Hex" or "X" - Hex format (#RRGGBB)
	/// "HSL" or "H" - HSL format (hsl(h,s%,l%))
	/// "RGB" or "R" - RGB format (rgb(r,g,b))
	/// "RGBA" - RGBA format (rgba(r,g,b,a))
	/// null or "" - Default hex format
	/// </param>
	/// <param name="formatProvider">Format provider (unused).</param>
	/// <returns>Formatted string.</returns>
	public string ToString(string? format, IFormatProvider? formatProvider = null) {
		if (string.IsNullOrEmpty(format)) {
			return ToHex();
		}

		return format.ToUpperInvariant() switch {
			"HEX" or "X" => ToHex(),
			"HSL" or "H" => $"hsl({_hue:F0},{_saturation * 100:F0}%,{_lightness * 100:F0}%)",
			"RGB" or "R" => FormatAsRgb(),
			"RGBA" => FormatAsRgba(),
			_ => ToHex()
		};
	}

	private string FormatAsRgb() {
		Color c = ToUnityColor();
		return $"rgb({(int)(c.r * 255)},{(int)(c.g * 255)},{(int)(c.b * 255)})";
	}

	private string FormatAsRgba() {
		Color c = ToUnityColor();
		return $"rgba({(int)(c.r * 255)},{(int)(c.g * 255)},{(int)(c.b * 255)},{c.a:F2})";
	}

	#endregion

	#region RPG-Themed Presets

	// ???????????????????????????????????????????????????????????????
	// ELEMENTAL COLORS
	// ???????????????????????????????????????????????????????????????

	/// <summary>Blazing fire orange-red.</summary>
	public static readonly ColorRPG Fire = FromHex("#FF4500");

	/// <summary>Cool ice cyan-blue.</summary>
	public static readonly ColorRPG Ice = FromHex("#00BFFF");

	/// <summary>Electric lightning yellow.</summary>
	public static readonly ColorRPG Lightning = FromHex("#FFD700");

	/// <summary>Toxic poison green.</summary>
	public static readonly ColorRPG Poison = FromHex("#7FFF00");

	/// <summary>Mystical arcane purple.</summary>
	public static readonly ColorRPG Arcane = FromHex("#9932CC");

	/// <summary>Dark shadow purple-black.</summary>
	public static readonly ColorRPG Shadow = FromHex("#2F1B41");

	/// <summary>Divine holy gold-white.</summary>
	public static readonly ColorRPG Holy = FromHex("#FFFACD");

	/// <summary>Deep blood red.</summary>
	public static readonly ColorRPG Blood = FromHex("#8B0000");

	/// <summary>Frostbite pale blue.</summary>
	public static readonly ColorRPG Frost = FromHex("#E0FFFF");

	/// <summary>Nature verdant green.</summary>
	public static readonly ColorRPG Nature = FromHex("#228B22");

	// ???????????????????????????????????????????????????????????????
	// UI/RESOURCE COLORS
	// ???????????????????????????????????????????????????????????????

	/// <summary>Health bar green.</summary>
	public static readonly ColorRPG HealthGreen = FromHex("#22C55E");

	/// <summary>Mana bar blue.</summary>
	public static readonly ColorRPG ManaBlue = FromHex("#3B82F6");

	/// <summary>Rage/fury red.</summary>
	public static readonly ColorRPG RageRed = FromHex("#EF4444");

	/// <summary>Stamina/energy yellow.</summary>
	public static readonly ColorRPG StaminaYellow = FromHex("#EAB308");

	/// <summary>Experience purple.</summary>
	public static readonly ColorRPG ExperiencePurple = FromHex("#A855F7");

	/// <summary>Shield/armor blue.</summary>
	public static readonly ColorRPG ShieldBlue = FromHex("#60A5FA");

	// ???????????????????????????????????????????????????????????????
	// COMBAT FEEDBACK COLORS
	// ???????????????????????????????????????????????????????????????

	/// <summary>Critical hit red.</summary>
	public static readonly ColorRPG CritRed = FromHex("#FF0033");

	/// <summary>Healing green.</summary>
	public static readonly ColorRPG HealGreen = FromHex("#00FFAA");

	/// <summary>Buff/positive gold.</summary>
	public static readonly ColorRPG BuffGold = FromHex("#FFDD44");

	/// <summary>Debuff/negative purple.</summary>
	public static readonly ColorRPG DebuffPurple = FromHex("#AA00FF");

	/// <summary>Miss/evade gray.</summary>
	public static readonly ColorRPG MissGray = FromHex("#9CA3AF");

	/// <summary>Block/parry orange.</summary>
	public static readonly ColorRPG BlockOrange = FromHex("#F97316");

	// ???????????????????????????????????????????????????????????????
	// METAL/MATERIAL COLORS
	// ???????????????????????????????????????????????????????????????

	/// <summary>Precious gold.</summary>
	public static readonly ColorRPG Gold = FromHex("#FFD700");

	/// <summary>Shining silver.</summary>
	public static readonly ColorRPG Silver = FromHex("#C0C0C0");

	/// <summary>Aged bronze.</summary>
	public static readonly ColorRPG Bronze = FromHex("#CD7F32");

	/// <summary>Sturdy iron gray.</summary>
	public static readonly ColorRPG Iron = FromHex("#434343");

	/// <summary>Copper metallic.</summary>
	public static readonly ColorRPG Copper = FromHex("#B87333");

	/// <summary>Platinum shimmer.</summary>
	public static readonly ColorRPG Platinum = FromHex("#E5E4E2");

	/// <summary>Mythril blue-silver.</summary>
	public static readonly ColorRPG Mythril = FromHex("#7EB8DA");

	/// <summary>Adamantine deep purple.</summary>
	public static readonly ColorRPG Adamantine = FromHex("#4A0080");

	// ???????????????????????????????????????????????????????????????
	// GEMSTONE COLORS
	// ???????????????????????????????????????????????????????????????

	/// <summary>Emerald green.</summary>
	public static readonly ColorRPG Emerald = FromHex("#50C878");

	/// <summary>Ruby red.</summary>
	public static readonly ColorRPG Ruby = FromHex("#E0115F");

	/// <summary>Sapphire blue.</summary>
	public static readonly ColorRPG Sapphire = FromHex("#0F52BA");

	/// <summary>Amethyst purple.</summary>
	public static readonly ColorRPG Amethyst = FromHex("#9966CC");

	/// <summary>Topaz golden-orange.</summary>
	public static readonly ColorRPG Topaz = FromHex("#FFC87C");

	/// <summary>Obsidian black.</summary>
	public static readonly ColorRPG Obsidian = FromHex("#1A1A1A");

	/// <summary>Diamond white sparkle.</summary>
	public static readonly ColorRPG Diamond = FromHex("#B9F2FF");

	// ???????????????????????????????????????????????????????????????
	// DARK FANTASY COLORS
	// ???????????????????????????????????????????????????????????????

	/// <summary>Void black with purple tint.</summary>
	public static readonly ColorRPG VoidBlack = FromHex("#0D0015");

	/// <summary>Abyssal deep purple.</summary>
	public static readonly ColorRPG AbyssalPurple = FromHex("#1A0033");

	/// <summary>Dark Souls ember red.</summary>
	public static readonly ColorRPG DarkSoulsRed = FromHex("#8B2E2E");

	/// <summary>Undead corpse gray.</summary>
	public static readonly ColorRPG UndeadGray = FromHex("#4A4A4A");

	/// <summary>Bonfire warm orange.</summary>
	public static readonly ColorRPG BonfireOrange = FromHex("#FF8C00");

	/// <summary>Plague sickly green.</summary>
	public static readonly ColorRPG PlagueGreen = FromHex("#556B2F");

	/// <summary>Frost white-blue.</summary>
	public static readonly ColorRPG FrostWhite = FromHex("#F0F8FF");

	/// <summary>Lava molten orange.</summary>
	public static readonly ColorRPG LavaOrange = FromHex("#FF4500");

	/// <summary>Necrotic decay purple.</summary>
	public static readonly ColorRPG NecroticPurple = FromHex("#301934");

	/// <summary>Eldritch unknowable teal.</summary>
	public static readonly ColorRPG EldritchTeal = FromHex("#008B8B");

	/// <summary>Corruption dark magenta.</summary>
	public static readonly ColorRPG Corruption = FromHex("#8B008B");

	/// <summary>Ancient worn parchment.</summary>
	public static readonly ColorRPG Parchment = FromHex("#F5DEB3");

	// ???????????????????????????????????????????????????????????????
	// RARITY COLORS
	// ???????????????????????????????????????????????????????????????

	/// <summary>Common item gray.</summary>
	public static readonly ColorRPG RarityCommon = FromHex("#9CA3AF");

	/// <summary>Uncommon item green.</summary>
	public static readonly ColorRPG RarityUncommon = FromHex("#22C55E");

	/// <summary>Rare item blue.</summary>
	public static readonly ColorRPG RarityRare = FromHex("#3B82F6");

	/// <summary>Epic item purple.</summary>
	public static readonly ColorRPG RarityEpic = FromHex("#A855F7");

	/// <summary>Legendary item orange.</summary>
	public static readonly ColorRPG RarityLegendary = FromHex("#F97316");

	/// <summary>Mythic item red-gold.</summary>
	public static readonly ColorRPG RarityMythic = FromHex("#DC2626");

	/// <summary>Artifact unique cyan.</summary>
	public static readonly ColorRPG RarityArtifact = FromHex("#06B6D4");

	// ???????????????????????????????????????????????????????????????
	// BASIC COLORS
	// ???????????????????????????????????????????????????????????????

	/// <summary>Pure white.</summary>
	public static readonly ColorRPG White = FromHex("#FFFFFF");

	/// <summary>Pure black.</summary>
	public static readonly ColorRPG Black = FromHex("#000000");

	/// <summary>Pure red.</summary>
	public static readonly ColorRPG Red = FromHex("#FF0000");

	/// <summary>Pure green.</summary>
	public static readonly ColorRPG Green = FromHex("#00FF00");

	/// <summary>Pure blue.</summary>
	public static readonly ColorRPG Blue = FromHex("#0000FF");

	/// <summary>Pure yellow.</summary>
	public static readonly ColorRPG Yellow = FromHex("#FFFF00");

	/// <summary>Pure cyan.</summary>
	public static readonly ColorRPG Cyan = FromHex("#00FFFF");

	/// <summary>Pure magenta.</summary>
	public static readonly ColorRPG Magenta = FromHex("#FF00FF");

	/// <summary>Transparent (zero alpha).</summary>
	public static readonly ColorRPG Transparent = FromHex("#00000000");

	#endregion

	#region Color Palettes

	/// <summary>Dark Souls inspired palette - muted, dark, melancholic.</summary>
	public static readonly ColorRPG[] DarkSoulsPalette = {
		FromHex("#1b1a17"), // Deep charcoal
		FromHex("#38322b"), // Ash brown
		FromHex("#695442"), // Worn leather
		FromHex("#a78a6d"), // Faded parchment
		FromHex("#f4efde")  // Bone white
	};

	/// <summary>Diablo inspired palette - hellish purples and blacks.</summary>
	public static readonly ColorRPG[] DiabloPalette = {
		FromHex("#0b0314"), // Abyss black
		FromHex("#190934"), // Deep hell purple
		FromHex("#220b40"), // Demon purple
		FromHex("#4b1782"), // Corrupted violet
		FromHex("#46198e"), // Soul purple
		FromHex("#6d2ba8")  // Arcane purple
	};

	/// <summary>Cyberpunk neon palette - vibrant, electric.</summary>
	public static readonly ColorRPG[] CyberNeon = {
		FromHex("#ff004c"), // Hot pink
		FromHex("#00ffea"), // Electric cyan
		FromHex("#ffe800"), // Neon yellow
		FromHex("#6e00ff"), // Purple flash
		FromHex("#ff00d0")  // Magenta blast
	};

	/// <summary>Gruvbox dark palette - warm, retro terminal feel.</summary>
	public static readonly ColorRPG[] GruvboxDark = {
		FromHex("#282828"), // Background
		FromHex("#cc241d"), // Red
		FromHex("#98971a"), // Green
		FromHex("#d79921"), // Yellow
		FromHex("#458588"), // Blue
		FromHex("#b16286"), // Purple
		FromHex("#689d6a"), // Aqua
		FromHex("#a89984"), // Gray
		FromHex("#ebdbb2")  // Foreground
	};

	/// <summary>Solarized dark palette - balanced, scientific.</summary>
	public static readonly ColorRPG[] SolarizedDark = {
		FromHex("#002b36"), // Base03
		FromHex("#073642"), // Base02
		FromHex("#586e75"), // Base01
		FromHex("#657b83"), // Base00
		FromHex("#839496"), // Base0
		FromHex("#93a1a1"), // Base1
		FromHex("#eee8d5"), // Base2
		FromHex("#fdf6e3"), // Base3
		FromHex("#b58900"), // Yellow
		FromHex("#cb4b16"), // Orange
		FromHex("#dc322f"), // Red
		FromHex("#d33682"), // Magenta
		FromHex("#6c71c4"), // Violet
		FromHex("#268bd2"), // Blue
		FromHex("#2aa198"), // Cyan
		FromHex("#859900")  // Green
	};

	/// <summary>Classic 16-color roguelike palette.</summary>
	public static readonly ColorRPG[] ClassicRoguelike = {
		FromHex("#000000"), // Black
		FromHex("#800000"), // Dark Red
		FromHex("#008000"), // Dark Green
		FromHex("#808000"), // Dark Yellow
		FromHex("#000080"), // Dark Blue
		FromHex("#800080"), // Dark Magenta
		FromHex("#008080"), // Dark Cyan
		FromHex("#c0c0c0"), // Light Gray
		FromHex("#808080"), // Dark Gray
		FromHex("#ff0000"), // Red
		FromHex("#00ff00"), // Green
		FromHex("#ffff00"), // Yellow
		FromHex("#0000ff"), // Blue
		FromHex("#ff00ff"), // Magenta
		FromHex("#00ffff"), // Cyan
		FromHex("#ffffff")  // White
	};

	/// <summary>Vampire/Gothic palette - blood reds and midnight blacks.</summary>
	public static readonly ColorRPG[] VampirePalette = {
		FromHex("#0a0a0a"), // Midnight
		FromHex("#1a0a0a"), // Blood shadow
		FromHex("#3d0c0c"), // Dark crimson
		FromHex("#6b1010"), // Blood red
		FromHex("#8b0000"), // Deep red
		FromHex("#b22222"), // Firebrick
		FromHex("#2a1a2a"), // Purple shadow
		FromHex("#4a1a4a")  // Royal purple
	};

	/// <summary>Eldritch/Lovecraftian palette - unsettling greens and purples.</summary>
	public static readonly ColorRPG[] EldritchPalette = {
		FromHex("#0a0f0f"), // Void
		FromHex("#0f1a1a"), // Deep sea
		FromHex("#1a2f2f"), // Murky depths
		FromHex("#2f4a4a"), // Eldritch teal
		FromHex("#1a0f2a"), // Cosmic purple
		FromHex("#2a1a4a"), // Otherworldly violet
		FromHex("#4a6a6a"), // Sickly cyan
		FromHex("#8fbc8f")  // Pale decay
	};

	/// <summary>Steampunk palette - brass, copper, and steam.</summary>
	public static readonly ColorRPG[] SteampunkPalette = {
		FromHex("#1a1612"), // Soot black
		FromHex("#2d2319"), // Coal
		FromHex("#5c4033"), // Dark leather
		FromHex("#8b7355"), // Worn brass
		FromHex("#b87333"), // Copper
		FromHex("#cd9b1d"), // Polished brass
		FromHex("#daa520"), // Golden gear
		FromHex("#f5deb3")  // Steam white
	};

	/// <summary>Forest/Nature palette - greens and earth tones.</summary>
	public static readonly ColorRPG[] ForestPalette = {
		FromHex("#1a2f1a"), // Deep forest
		FromHex("#2d4a2d"), // Shadow green
		FromHex("#228b22"), // Forest green
		FromHex("#32cd32"), // Lime green
		FromHex("#8fbc8f"), // Pale green
		FromHex("#5c4033"), // Bark brown
		FromHex("#8b4513"), // Saddle brown
		FromHex("#deb887")  // Burlywood
	};

	/// <summary>Ocean/Water palette - blues and teals.</summary>
	public static readonly ColorRPG[] OceanPalette = {
		FromHex("#000033"), // Abyss
		FromHex("#000066"), // Deep ocean
		FromHex("#003366"), // Ocean blue
		FromHex("#006699"), // Sea blue
		FromHex("#0099cc"), // Tropical blue
		FromHex("#00cccc"), // Shallow water
		FromHex("#66ffff"), // Surf foam
		FromHex("#e0ffff")  // Light cyan
	};

	#endregion

	#region Named Colors Dictionary

	private static readonly Dictionary<string, ColorRPG> s_namedColors = new(StringComparer.OrdinalIgnoreCase) {
		// Elements
		["fire"] = Fire,
		["ice"] = Ice,
		["lightning"] = Lightning,
		["poison"] = Poison,
		["arcane"] = Arcane,
		["shadow"] = Shadow,
		["holy"] = Holy,
		["blood"] = Blood,
		["frost"] = Frost,
		["nature"] = Nature,

		// Resources
		["health"] = HealthGreen,
		["healthgreen"] = HealthGreen,
		["mana"] = ManaBlue,
		["manablue"] = ManaBlue,
		["rage"] = RageRed,
		["ragered"] = RageRed,
		["stamina"] = StaminaYellow,
		["staminayellow"] = StaminaYellow,
		["experience"] = ExperiencePurple,
		["xp"] = ExperiencePurple,
		["shield"] = ShieldBlue,

		// Combat
		["crit"] = CritRed,
		["critical"] = CritRed,
		["heal"] = HealGreen,
		["buff"] = BuffGold,
		["debuff"] = DebuffPurple,
		["miss"] = MissGray,
		["block"] = BlockOrange,

		// Metals
		["gold"] = Gold,
		["silver"] = Silver,
		["bronze"] = Bronze,
		["iron"] = Iron,
		["copper"] = Copper,
		["platinum"] = Platinum,
		["mythril"] = Mythril,
		["mithril"] = Mythril,
		["adamantine"] = Adamantine,

		// Gems
		["emerald"] = Emerald,
		["ruby"] = Ruby,
		["sapphire"] = Sapphire,
		["amethyst"] = Amethyst,
		["topaz"] = Topaz,
		["obsidian"] = Obsidian,
		["diamond"] = Diamond,

		// Dark fantasy
		["void"] = VoidBlack,
		["abyss"] = AbyssalPurple,
		["abyssal"] = AbyssalPurple,
		["undead"] = UndeadGray,
		["bonfire"] = BonfireOrange,
		["plague"] = PlagueGreen,
		["lava"] = LavaOrange,
		["necrotic"] = NecroticPurple,
		["eldritch"] = EldritchTeal,
		["corruption"] = Corruption,
		["parchment"] = Parchment,

		// Rarity
		["common"] = RarityCommon,
		["uncommon"] = RarityUncommon,
		["rare"] = RarityRare,
		["epic"] = RarityEpic,
		["legendary"] = RarityLegendary,
		["mythic"] = RarityMythic,
		["artifact"] = RarityArtifact,

		// Basics
		["white"] = White,
		["black"] = Black,
		["red"] = Red,
		["green"] = Green,
		["blue"] = Blue,
		["yellow"] = Yellow,
		["cyan"] = Cyan,
		["magenta"] = Magenta,
		["transparent"] = Transparent
	};

	#endregion

	#region Usage Examples
	/*
	// ???????????????????????????????????????????????????????????????
	// DAMAGE NUMBERS
	// ???????????????????????????????????????????????????????????????
	
	void ShowDamageNumber(int damage, bool isCrit, DamageType type) {
		ColorRPG color = type switch {
			DamageType.Fire => ColorRPG.Fire,
			DamageType.Ice => ColorRPG.Ice,
			DamageType.Lightning => ColorRPG.Lightning,
			DamageType.Poison => ColorRPG.Poison,
			DamageType.Holy => ColorRPG.Holy,
			DamageType.Shadow => ColorRPG.Shadow,
			_ => ColorRPG.White
		};
		
		if (isCrit) {
			color = ColorRPG.CritRed.LerpHSL(color, 50.Percent());
		}
		
		// Implicit conversion to Unity Color
		damageText.color = color;
	}
	
	// ???????????????????????????????????????????????????????????????
	// HEALTH/MANA BARS WITH DYNAMIC COLORING
	// ???????????????????????????????????????????????????????????????
	
	void UpdateHealthBar(float healthPercent) {
		// Lerp from red (low) to green (high)
		ColorRPG barColor = ColorRPG.RageRed.LerpHSL(ColorRPG.HealthGreen, healthPercent.Percent());
		healthBarImage.color = barColor;
		
		// Add pulsing effect when low
		if (healthPercent < 0.25f) {
			float pulse = Mathf.Sin(Time.time * 4f) * 0.3f + 0.7f;
			healthBarImage.color = barColor.WithAlpha(pulse);
		}
	}
	
	// ???????????????????????????????????????????????????????????????
	// PROCEDURAL ENEMY TINTING
	// ???????????????????????????????????????????????????????????????
	
	void TintEnemy(SpriteRenderer renderer, EnemyCategory category, int level) {
		ColorRPG baseColor = category switch {
			EnemyCategory.Undead => ColorRPG.UndeadGray,
			EnemyCategory.Demon => ColorRPG.DarkSoulsRed,
			EnemyCategory.Elemental => ColorRPG.Arcane,
			EnemyCategory.Beast => ColorRPG.Nature,
			_ => ColorRPG.White
		};
		
		// Higher level = more saturated and slightly darker
		float levelMod = Mathf.Min(level / 20f, 1f);
		ColorRPG tint = baseColor
			.Saturate(levelMod * 0.2f)
			.Darken(levelMod * 0.1f)
			.WithRandomHueVariation(10f);
		
		renderer.color = tint;
	}
	
	// ???????????????????????????????????????????????????????????????
	// THEME SWITCHING
	// ???????????????????????????????????????????????????????????????
	
	void ApplyTheme(bool darkMode) {
		ColorRPG background = darkMode 
			? ColorRPG.FromHex("#1a1a2e") 
			: ColorRPG.FromHex("#f5f5f5");
		ColorRPG text = background.ContrastingTextColor();
		ColorRPG accent = darkMode ? ColorRPG.CyberNeon[1] : ColorRPG.Sapphire;
		
		Camera.main.backgroundColor = background;
		foreach (var label in labels) {
			label.color = text;
		}
		highlightColor = accent;
	}
	
	// ???????????????????????????????????????????????????????????????
	// RARITY ITEM FRAMES
	// ???????????????????????????????????????????????????????????????
	
	ColorRPG GetRarityColor(RarityType rarity) => rarity switch {
		RarityType.Common => ColorRPG.RarityCommon,
		RarityType.Uncommon => ColorRPG.RarityUncommon,
		RarityType.Rare => ColorRPG.RarityRare,
		RarityType.Epic => ColorRPG.RarityEpic,
		RarityType.Legendary => ColorRPG.RarityLegendary,
		RarityType.Mythic => ColorRPG.RarityMythic,
		_ => ColorRPG.White
	};
	
	// ???????????????????????????????????????????????????????????????
	// GIZMOS DEBUGGING
	// ???????????????????????????????????????????????????????????????
	
	void OnDrawGizmos() {
		// Direct assignment - implicit conversion!
		Gizmos.color = ColorRPG.Arcane;
		Gizmos.DrawWireSphere(transform.position, detectionRadius);
		
		Gizmos.color = ColorRPG.Fire.WithAlpha(0.3f);
		Gizmos.DrawSphere(attackOrigin, attackRadius);
		
		Gizmos.color = ColorRPG.HealthGreen;
		Gizmos.DrawLine(patrolStart, patrolEnd);
	}
	
	// ???????????????????????????????????????????????????????????????
	// COLOR SCHEMES FOR PROCEDURAL DUNGEONS
	// ???????????????????????????????????????????????????????????????
	
	ColorRPG[] GenerateDungeonPalette(DungeonTheme theme) {
		ColorRPG primary = theme switch {
			DungeonTheme.Crypt => ColorRPG.UndeadGray,
			DungeonTheme.Volcano => ColorRPG.LavaOrange,
			DungeonTheme.Depths => ColorRPG.AbyssalPurple,
			DungeonTheme.Frozen => ColorRPG.Frost,
			_ => ColorRPG.Iron
		};
		
		// Generate a cohesive palette
		return new[] {
			primary.Darken(0.3f),      // Shadows
			primary,                    // Primary
			primary.Lighten(0.2f),     // Highlights
			primary.Complementary().Desaturate(0.3f), // Accent
			primary.RotateHue(30f).Desaturate(0.5f)   // Secondary
		};
	}
	
	// ???????????????????????????????????????????????????????????????
	// BUFF/DEBUFF ICON TINTING
	// ???????????????????????????????????????????????????????????????
	
	void UpdateStatusIcon(Image icon, StatusEffect effect) {
		ColorRPG baseColor = effect.IsPositive ? ColorRPG.BuffGold : ColorRPG.DebuffPurple;
		
		// Fade as duration decreases
		float fadePercent = effect.RemainingDuration / effect.TotalDuration;
		icon.color = baseColor.WithAlpha(0.5f + fadePercent * 0.5f);
		
		// Pulse when about to expire
		if (fadePercent < 0.2f) {
			float pulse = Mathf.Abs(Mathf.Sin(Time.time * 8f));
			icon.color = baseColor.LerpHSL(ColorRPG.White, pulse * 0.3f);
		}
	}
	
	// ???????????????????????????????????????????????????????????????
	// STRING FORMATTING FOR LOGS/UI
	// ???????????????????????????????????????????????????????????????
	
	void LogColorInfo() {
		Debug.Log($"Fire color: {ColorRPG.Fire:Hex}");      // #FF4500
		Debug.Log($"Fire as HSL: {ColorRPG.Fire:HSL}");     // hsl(16,100%,50%)
		Debug.Log($"Fire as RGB: {ColorRPG.Fire:RGB}");     // rgb(255,69,0)
		
		ColorRPG custom = ColorRPG.FromHSL(200, 0.8f, 0.6f);
		Debug.Log($"Custom color: {custom}");               // #4DA6CC
	}
	
	// ???????????????????????????????????????????????????????????????
	// PARTICLE SYSTEM GRADIENTS
	// ???????????????????????????????????????????????????????????????
	
	Gradient CreateFireGradient() {
		var gradient = new Gradient();
		gradient.SetKeys(
			new[] {
				new GradientColorKey(ColorRPG.Yellow, 0f),
				new GradientColorKey(ColorRPG.Fire, 0.3f),
				new GradientColorKey(ColorRPG.DarkSoulsRed, 0.7f),
				new GradientColorKey(ColorRPG.Shadow, 1f)
			},
			new[] {
				new GradientAlphaKey(1f, 0f),
				new GradientAlphaKey(1f, 0.5f),
				new GradientAlphaKey(0f, 1f)
			}
		);
		return gradient;
	}
	*/
	#endregion
}

public static class ColorRPGExtensions
{
	/// <summary>
	/// Converts a ColorRPG to a StyleColor for UIElements style assignments.
	/// Uses the existing implicit conversion to UnityEngine.Color.
	/// </summary>
	public static StyleColor ToStyleColor(this ColorRPG color)
	{
		return new StyleColor(color); 
	}
}
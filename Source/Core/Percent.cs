using Newtonsoft.Json;
using RPGGame.Core.Save;

namespace RPGGame.Core;

/// <summary>
/// An immutable percentage/multiplier value stored internally as a fraction (≥0.0).
/// Perfect for RPG damage multipliers, buff stacks, and stat bonuses.
/// </summary>
/// <remarks>
/// <para>Values are clamped to ≥0.0 (no negative percentages). No upper limit — 300% is valid!</para>
/// <para>Internal storage is a multiplier: 75% = 0.75f, 300% = 3.0f</para>
/// <para>Use <see cref="Clamped"/> when you need normalized 0-100% (health bars, progress, etc.)</para>
/// </remarks>
/// <example>
/// <code>
/// Percent damageBonus = 300;        // 300% bonus
/// float totalDamage = 100 * (1 + damageBonus);  // 100 * 4.0 = 400
/// 
/// Percent healthPercent = (currentHp / maxHp).AsFractionPercent().Clamped();
/// </code>
/// </example>
[Serializable]
public readonly struct Percent : IEquatable<Percent>, IComparable<Percent>, IFormattable {
	#region Fields

	/// <summary>
	/// Internal storage: multiplier/fraction, always ≥0.0f.
	/// 75% = 0.75f, 300% = 3.0f
	/// </summary>
	private readonly float _value;

	private const float EPSILON = 1e-6f;

	#endregion

	#region Constants

	/// <summary>0% (0.0 multiplier)</summary>
	public static readonly Percent Zero = new(0f);

	/// <summary>100% (1.0 multiplier)</summary>
	public static readonly Percent Hundred = new(1f);

	/// <summary>100% (1.0 multiplier) - alias for One</summary>
	public static readonly Percent Full = new(1f);

	/// <summary>50% (0.5 multiplier)</summary>
	public static readonly Percent Half = new(0.5f);

	/// <summary>200% (2.0 multiplier)</summary>
	public static readonly Percent Double = new(2f);

	/// <summary>300% (3.0 multiplier)</summary>
	public static readonly Percent Triple = new(3f);

	/// <summary>10% (0.1 multiplier)</summary>
	public static readonly Percent Ten = new(0.1f);

	/// <summary>25% (0.25 multiplier)</summary>
	public static readonly Percent TwentyFive = new(0.25f);

	/// <summary>50% (0.5 multiplier) - alias for Half</summary>
	public static readonly Percent Fifty = new(0.5f);

	/// <summary>75% (0.75 multiplier)</summary>
	public static readonly Percent SeventyFive = new(0.75f);

	/// <summary>90% (0.9 multiplier)</summary>
	public static readonly Percent Ninety = new(0.9f);

	#endregion

	#region Constructors

	/// <summary>
	/// Creates a Percent from a multiplier/fraction value (≥0.0).
	/// Values below 0 are clamped to 0.
	/// </summary>
	/// <param name="fraction">A multiplier value (0.75 = 75%, 3.0 = 300%)</param>
	private Percent(float fraction) {
		_value = ClampMin(fraction);
	}

	#endregion

	#region Properties

	/// <summary>
	/// Gets the percentage as a fraction/multiplier (≥0.0).
	/// </summary>
	/// <example>75% returns 0.75, 300% returns 3.0</example>
	public float Fraction => _value;

	/// <summary>
	/// Gets the percentage as a display value (0 to ∞).
	/// </summary>
	/// <example>0.75 returns 75, 3.0 returns 300</example>
	[DoNotSave(Reason = "Computed from _value")]
	public float Value => _value * 100f;

	/// <summary>
	/// Gets whether this represents 0% (no effect).
	/// </summary>
	[DoNotSave(Reason = "Computed from _value")]
	public bool IsZero => _value <= EPSILON;

	/// <summary>
	/// Gets whether this represents exactly 100% (1.0 multiplier).
	/// </summary>
	[DoNotSave(Reason = "Computed from _value")]
	public bool IsOne => Math.Abs(_value - 1f) <= EPSILON;

	/// <summary>
	/// Gets whether this represents exactly 100% (1.0 multiplier).
	/// Alias for IsOne.
	/// </summary>
	[DoNotSave(Reason = "Alias for IsOne")]
	public bool IsFull => IsOne;

	/// <summary>
	/// Gets the inverse of this percentage (1.0 / fraction).
	/// Returns Zero if this is Zero to avoid division by zero.
	/// </summary>
	/// <example>50% (0.5) returns 200% (2.0), 200% returns 50%</example>
	[DoNotSave(Reason = "Self-referencing - computed on access")]
	public Percent Inverse => IsZero ? Zero : new(1f / _value);

	/// <summary>
	/// Gets the complement (1.0 - fraction), clamped to ≥0.
	/// Useful for "remaining" calculations.
	/// </summary>
	/// <example>75% returns 25%, 150% returns 0%</example>
	[DoNotSave(Reason = "Self-referencing - computed on access")]
	public Percent Complement => new(1f - _value);

	#endregion

	#region Factory Methods

	/// <summary>
	/// Creates a Percent from an integer percentage value (0-∞).
	/// </summary>
	/// <param name="value">Integer percentage (e.g., 75 for 75%, 300 for 300%)</param>
	/// <returns>A Percent with the value clamped to ≥0</returns>
	/// <example>Percent.FromPercent(300) → 300% (3.0 multiplier)</example>
	public static Percent FromPercent(int value) => new(value / 100f);

	/// <summary>
	/// Creates a Percent from a float percentage value (0-∞).
	/// </summary>
	/// <param name="value">Float percentage (e.g., 75.5f for 75.5%, 300f for 300%)</param>
	/// <returns>A Percent with the value clamped to ≥0</returns>
	/// <example>Percent.FromPercent(300f) → 300% (3.0 multiplier)</example>
	public static Percent FromPercent(float value) => new(value / 100f);

	/// <summary>
	/// Creates a Percent from a fraction/multiplier value (0.0-∞).
	/// </summary>
	/// <param name="fraction">Fraction value (e.g., 0.75 for 75%, 3.0 for 300%)</param>
	/// <returns>A Percent with the value clamped to ≥0</returns>
	/// <example>Percent.FromFraction(3.0f) → 300%</example>
	public static Percent FromFraction(float fraction) => new(fraction);

	/// <summary>
	/// Creates a Percent from an integer percentage value (0-∞).
	/// Explicit naming for maximum clarity.
	/// </summary>
	/// <param name="percent">Integer percentage (e.g., 75 for 75%)</param>
	/// <returns>A Percent with the value clamped to ≥0</returns>
	/// <example>Percent.FromInt(300) → 300%</example>
	public static Percent FromInt(int percent) => new(percent / 100f);

	/// <summary>
	/// Creates a Percent from a float value treated as a percentage (0-∞).
	/// Use this when you have a float like 75.0f that represents 75%.
	/// </summary>
	/// <param name="percent">Float percentage value (e.g., 300f for 300%)</param>
	/// <returns>A Percent with the value clamped to ≥0</returns>
	/// <example>Percent.FromFloatPercent(300f) → 300%</example>
	public static Percent FromFloatPercent(float percent) => new(percent / 100f);

	#endregion

	#region Clamping

	/// <summary>
	/// Returns this percentage clamped to [0.0, 1.0] (0% to 100%).
	/// Use for normalized values like health bars, progress indicators, etc.
	/// </summary>
	/// <returns>A new Percent clamped between 0% and 100%</returns>
	/// <example>
	/// Percent.FromPercent(150).Clamped() → 100%
	/// Percent.FromPercent(-50).Clamped() → 0%
	/// </example>
	public Percent Clamped() => new(Math.Clamp(_value, 0f, 1f));

	/// <summary>
	/// Returns this percentage clamped to a maximum value.
	/// </summary>
	/// <param name="max">Maximum allowed percentage</param>
	/// <returns>A new Percent clamped to the maximum</returns>
	public Percent ClampedMax(Percent max) => _value > max._value ? max : this;

	/// <summary>
	/// Returns this percentage clamped to a minimum value.
	/// </summary>
	/// <param name="min">Minimum allowed percentage</param>
	/// <returns>A new Percent clamped to the minimum</returns>
	public Percent ClampedMin(Percent min) => _value < min._value ? min : this;

	/// <summary>
	/// Returns this percentage clamped between min and max.
	/// </summary>
	/// <param name="min">Minimum allowed percentage</param>
	/// <param name="max">Maximum allowed percentage</param>
	/// <returns>A new Percent clamped to the range</returns>
	public Percent ClampedRange(Percent min, Percent max) => new(Math.Clamp(_value, min._value, max._value));

	#endregion

	#region Implicit Conversions

	/// <summary>
	/// Implicitly converts an integer to a Percent (treated as 0-∞ percentage).
	/// </summary>
	/// <param name="value">Integer percentage value</param>
	/// <example>Percent p = 75; // 75% (0.75 multiplier)</example>
	/// <example>Percent p = 300; // 300% (3.0 multiplier)</example>
	public static implicit operator Percent(int value) => FromPercent(value);

	/// <summary>
	/// Implicitly converts a float to a Percent.
	/// Values ≤ 1.5 are treated as fractions; values > 1.5 are treated as percentages.
	/// </summary>
	/// <param name="value">Float value (fraction if ≤1.5, percentage if >1.5)</param>
	/// <example>
	/// Percent p1 = 0.75f;  // 75% (treated as fraction)
	/// Percent p2 = 75f;    // 75% (treated as percentage)
	/// Percent p3 = 3.0f;   // 300% (treated as 3x multiplier, NOT 3%)
	/// </example>
	/// <remarks>
	/// The threshold of 1.5 allows common multipliers (1.0, 1.25, 1.5) to work as fractions
	/// while treating larger values like 75f, 100f, 300f as percentage values.
	/// </remarks>
	public static implicit operator Percent(float value) {
		// Smart detection: if > 1.5, assume it's a percentage value (0-∞ scale)
		// This allows 0.5f, 1.0f, 1.25f, 1.5f to work as multipliers
		// While 75f, 100f, 300f are treated as percentage values
		return value > 1.5f ? FromPercent(value) : FromFraction(value);
	}

	/// <summary>
	/// Implicitly converts a Percent to a float (returns the fraction/multiplier).
	/// </summary>
	/// <param name="p">The Percent to convert</param>
	/// <example>float f = myPercent; // 75% → 0.75f, 300% → 3.0f</example>
	public static implicit operator float(Percent p) => p._value;

	#endregion

	#region Arithmetic Operators

	/// <summary>Adds two percentages together.</summary>
	public static Percent operator +(Percent a, Percent b) => new(a._value + b._value);

	/// <summary>Adds a float to a percentage.</summary>
	public static Percent operator +(Percent a, float b) => new(a._value + b);

	/// <summary>Adds a percentage to a float.</summary>
	public static Percent operator +(float a, Percent b) => new(a + b._value);

	/// <summary>Adds an integer percentage to a percentage.</summary>
	public static Percent operator +(Percent a, int b) => new(a._value + b / 100f);

	/// <summary>Adds a percentage to an integer percentage.</summary>
	public static Percent operator +(int a, Percent b) => new(a / 100f + b._value);

	/// <summary>Subtracts one percentage from another (clamped to ≥0).</summary>
	public static Percent operator -(Percent a, Percent b) => new(a._value - b._value);

	/// <summary>Subtracts a float from a percentage (clamped to ≥0).</summary>
	public static Percent operator -(Percent a, float b) => new(a._value - b);

	/// <summary>Subtracts a percentage from a float (clamped to ≥0).</summary>
	public static Percent operator -(float a, Percent b) => new(a - b._value);

	/// <summary>Subtracts an integer percentage from a percentage (clamped to ≥0).</summary>
	public static Percent operator -(Percent a, int b) => new(a._value - b / 100f);

	/// <summary>Subtracts a percentage from an integer percentage (clamped to ≥0).</summary>
	public static Percent operator -(int a, Percent b) => new(a / 100f - b._value);

	/// <summary>Multiplies two percentages.</summary>
	public static Percent operator *(Percent a, Percent b) => new(a._value * b._value);

	/// <summary>Multiplies a percentage by a float.</summary>
	public static Percent operator *(Percent a, float b) => new(a._value * b);

	/// <summary>Multiplies a float by a percentage.</summary>
	public static Percent operator *(float a, Percent b) => new(a * b._value);

	/// <summary>Multiplies a percentage by an integer.</summary>
	public static Percent operator *(Percent a, int b) => new(a._value * b);

	/// <summary>Multiplies an integer by a percentage.</summary>
	public static Percent operator *(int a, Percent b) => new(a * b._value);

	/// <summary>Divides a percentage by another. Division by zero returns Zero.</summary>
	public static Percent operator /(Percent a, Percent b) =>
		b.IsZero ? Zero : new(a._value / b._value);

	/// <summary>Divides a percentage by a float. Division by zero returns Zero.</summary>
	public static Percent operator /(Percent a, float b) =>
		Math.Abs(b) < EPSILON ? Zero : new(a._value / b);

	/// <summary>Divides a float by a percentage. Division by zero returns Zero.</summary>
	public static Percent operator /(float a, Percent b) =>
		b.IsZero ? Zero : new(a / b._value);

	/// <summary>Divides a percentage by an integer. Division by zero returns Zero.</summary>
	public static Percent operator /(Percent a, int b) =>
		b == 0 ? Zero : new(a._value / b);

	/// <summary>Divides an integer by a percentage. Division by zero returns Zero.</summary>
	public static Percent operator /(int a, Percent b) =>
		b.IsZero ? Zero : new(a / b._value);

	/// <summary>Unary plus - returns the same value.</summary>
	public static Percent operator +(Percent p) => p;

	/// <summary>Unary minus - returns Zero (can't have negative percentages).</summary>
	public static Percent operator -(Percent p) => Zero;

	#endregion

	#region Comparison Operators

	/// <summary>Checks if two percentages are equal (within epsilon).</summary>
	public static bool operator ==(Percent a, Percent b) => Math.Abs(a._value - b._value) < EPSILON;

	/// <summary>Checks if two percentages are not equal.</summary>
	public static bool operator !=(Percent a, Percent b) => !(a == b);

	/// <summary>Checks if one percentage is less than another.</summary>
	public static bool operator <(Percent a, Percent b) => a._value < b._value - EPSILON;

	/// <summary>Checks if one percentage is greater than another.</summary>
	public static bool operator >(Percent a, Percent b) => a._value > b._value + EPSILON;

	/// <summary>Checks if one percentage is less than or equal to another.</summary>
	public static bool operator <=(Percent a, Percent b) => a._value <= b._value + EPSILON;

	/// <summary>Checks if one percentage is greater than or equal to another.</summary>
	public static bool operator >=(Percent a, Percent b) => a._value >= b._value - EPSILON;

	#endregion

	#region Utility Methods

	/// <summary>
	/// Linearly interpolates between two percentages.
	/// </summary>
	/// <param name="a">Start percentage</param>
	/// <param name="b">End percentage</param>
	/// <param name="t">Interpolation factor (0.0 to 1.0, or a Percent)</param>
	/// <returns>Interpolated percentage</returns>
	/// <example>Percent.Lerp(Percent.Zero, Percent.Double, Percent.Half) → 100%</example>
	public static Percent Lerp(Percent a, Percent b, Percent t) {
		float tClamped = Math.Clamp(t._value, 0f, 1f);
		return new Percent(a._value + (b._value - a._value) * tClamped);
	}

	/// <summary>
	/// Linearly interpolates between two percentages.
	/// </summary>
	/// <param name="a">Start percentage</param>
	/// <param name="b">End percentage</param>
	/// <param name="t">Interpolation factor (0.0 to 1.0)</param>
	/// <returns>Interpolated percentage</returns>
	public static Percent Lerp(Percent a, Percent b, float t) {
		float tClamped = Math.Clamp(t, 0f, 1f);
		return new Percent(a._value + (b._value - a._value) * tClamped);
	}

	/// <summary>
	/// Returns the larger of two percentages.
	/// </summary>
	public static Percent Max(Percent a, Percent b) => a._value >= b._value ? a : b;

	/// <summary>
	/// Returns the smaller of two percentages.
	/// </summary>
	public static Percent Min(Percent a, Percent b) => a._value <= b._value ? a : b;

	/// <summary>
	/// Applies this percentage as a multiplier to a value.
	/// </summary>
	/// <param name="value">The value to multiply</param>
	/// <returns>The value multiplied by this percentage's fraction</returns>
	/// <example>Percent.Double.Of(50) → 100</example>
	public float Of(float value) => value * _value;

	/// <summary>
	/// Applies this percentage as a multiplier to an integer value.
	/// </summary>
	public float Of(int value) => value * _value;

	/// <summary>
	/// Returns the value after applying this as a reduction (value * (1 - fraction)).
	/// Reduction is clamped so result is never negative.
	/// </summary>
	/// <param name="value">The original value</param>
	/// <returns>The value after reduction (minimum 0)</returns>
	/// <example>Percent.TwentyFive.Reduce(100) → 75 (25% reduction)</example>
	public float Reduce(float value) => value * Math.Max(0f, 1f - _value);

	/// <summary>
	/// Returns the value after applying this as a reduction.
	/// </summary>
	public float Reduce(int value) => value * Math.Max(0f, 1f - _value);

	/// <summary>
	/// Returns the bonus amount when applying this percentage.
	/// Same as Of(), but semantically clearer for additive bonuses.
	/// </summary>
	/// <param name="value">The base value</param>
	/// <returns>The bonus amount (base * percentage)</returns>
	/// <example>
	/// Percent bonus = 50; // 50% bonus
	/// float total = baseDamage + bonus.Bonus(baseDamage); // 150% of base
	/// </example>
	public float Bonus(float value) => value * _value;

	/// <summary>
	/// Returns the bonus amount for an integer value.
	/// </summary>
	public float Bonus(int value) => value * _value;

	/// <summary>
	/// Returns the total after applying this as an additive bonus (value * (1 + fraction)).
	/// </summary>
	/// <param name="value">The base value</param>
	/// <returns>The value with bonus applied</returns>
	/// <example>
	/// Percent bonus = 300; // 300% bonus
	/// float total = bonus.AddTo(100); // 100 * (1 + 3.0) = 400
	/// </example>
	public float AddTo(float value) => value * (1f + _value);

	/// <summary>
	/// Returns the total after applying this as an additive bonus.
	/// </summary>
	public float AddTo(int value) => value * (1f + _value);

	#endregion

	#region Equality & Comparison

	/// <inheritdoc />
	public bool Equals(Percent other) => this == other;

	/// <inheritdoc />
	public override bool Equals(object? obj) => obj is Percent other && Equals(other);

	/// <inheritdoc />
	public override int GetHashCode() => _value.GetHashCode();

	/// <inheritdoc />
	public int CompareTo(Percent other) => _value.CompareTo(other._value);

	#endregion

	#region Formatting

	/// <summary>
	/// Returns a string representation of this percentage.
	/// </summary>
	/// <returns>Percentage string (e.g., "75%", "300%")</returns>
	public override string ToString() => $"{Value:0.#}%";

	/// <summary>
	/// Returns a string representation with the specified number of decimal places.
	/// </summary>
	/// <param name="decimals">Number of decimal places</param>
	/// <returns>Formatted percentage string</returns>
	/// <example>percent.ToString(2) → "75.00%", "300.00%"</example>
	public string ToString(int decimals) {
		if (decimals <= 0) {
			return $"{Value:0}%";
		}
		string format = "0." + new string('0', decimals);
		return $"{Value.ToString(format)}%";
	}

	/// <summary>
	/// Formats the percentage according to the specified format string.
	/// </summary>
	/// <param name="format">
	/// Format string. Use "P" for percent display (with % sign).
	/// Use "F" for fraction/multiplier format.
	/// Use "V" for value format (0-∞).
	/// Standard numeric formats also work.
	/// </param>
	/// <param name="formatProvider">Format provider (culture)</param>
	/// <returns>Formatted string</returns>
	/// <example>
	/// $"{percent:P1}" → "75.0%" or "300.0%"
	/// $"{percent:F2}" → "0.75" or "3.00"
	/// $"{percent:V0}" → "75" or "300"
	/// </example>
	public string ToString(string? format, IFormatProvider? formatProvider) {
		if (string.IsNullOrEmpty(format)) {
			return ToString();
		}

		char specifier = char.ToUpperInvariant(format[0]);
		string precision = format.Length > 1 ? format[1..] : "";

		return specifier switch {
			'P' => Value.ToString($"F{precision}", formatProvider) + "%",
			'F' => _value.ToString($"F{precision}", formatProvider),
			'V' => Value.ToString($"F{precision}", formatProvider),
			_ => Value.ToString(format, formatProvider) + "%"
		};
	}

	#endregion

	#region Private Helpers

	/// <summary>
	/// Clamps a value to ≥0.0 (no upper limit).
	/// </summary>
	private static float ClampMin(float value) => Math.Max(0f, value);

	#endregion
}

#region Extension Methods

/// <summary>
/// Extension methods for creating Percent values from numeric types.
/// </summary>
public static class PercentExtensions {
	/// <summary>
	/// Converts an integer to a Percent (treated as 0-∞ percentage).
	/// </summary>
	/// <param name="value">Integer percentage value</param>
	/// <returns>A Percent representing the value</returns>
	/// <example>75.Percent() → 75%, 300.Percent() → 300%</example>
	public static Percent Percent(this int value) => Core.Percent.FromPercent(value);

	/// <param name="value">Float percentage value</param>
	extension(float value) {
		/// <summary>
		/// Converts a float to a Percent (treated as percentage value).
		/// </summary>
		/// <returns>A Percent representing the value</returns>
		/// <example>75f.Percent() → 75%, 300f.Percent() → 300%</example>
		public Percent Percent() => Core.Percent.FromPercent(value);
		/// <summary>
		/// Converts a float fraction/multiplier to a Percent.
		/// Use this when you explicitly have a multiplier value.
		/// </summary>
		/// <returns>A Percent representing the fraction</returns>
		/// <example>0.75f.AsFractionPercent() → 75%, 3.0f.AsFractionPercent() → 300%</example>
		public Percent AsFractionPercent() => Core.Percent.FromFraction(value);
	}

	/// <summary>
	/// Converts an integer to a Percent, ensuring non-negative.
	/// </summary>
	/// <param name="value">Integer to convert</param>
	/// <returns>A Percent clamped to ≥0%</returns>
	/// <example>(-50).ClampPercent() → 0%</example>
	public static Percent ClampPercent(this int value) => Core.Percent.FromPercent(Math.Max(0, value));

	/// <param name="value">Float to convert</param>
	extension(float value) {
		/// <summary>
		/// Converts a float to a Percent, ensuring non-negative.
		/// Treats value as a percentage (not fraction).
		/// </summary>
		/// <returns>A Percent clamped to ≥0%</returns>
		/// <example>(-50f).ClampPercent() → 0%</example>
		public Percent ClampPercent() => Core.Percent.FromPercent(Math.Max(0f, value));
		/// <summary>
		/// Converts a float treated explicitly as a percentage (0-∞) to a Percent.
		/// </summary>
		/// <returns>A Percent representing the value</returns>
		/// <example>75f.AsPercent() → 75%</example>
		public Percent AsPercent() => Core.Percent.FromFloatPercent(value);
		/// <summary>
		/// Converts a float treated explicitly as a fraction/multiplier to a Percent.
		/// </summary>
		/// <returns>A Percent representing the fraction</returns>
		/// <example>0.75f.AsFraction() → 75%, 3.0f.AsFraction() → 300%</example>
		public Percent AsFraction() => Core.Percent.FromFraction(value);
	}

	/// <summary>
	/// Returns this Percent clamped to [0%, 100%] for normalized uses.
	/// </summary>
	/// <param name="percent">The Percent to clamp</param>
	/// <returns>A new Percent clamped between 0% and 100%</returns>
	/// <example>Percent.Triple.Clamped() → 100%</example>
	public static Percent Clamped(this Percent percent) => percent.Clamped();
}

#endregion

#region Usage Examples

/*
═══════════════════════════════════════════════════════════════════════════════
USAGE EXAMPLES - RPG Math Made Delightful
═══════════════════════════════════════════════════════════════════════════════

// ─── Creation Styles (All Compile & Work) ───────────────────────────────────

Percent p1 = 75;                      // 75% (0.75 multiplier)
Percent p2 = 0.75f;                   // 75% (fraction → multiplier)
Percent p3 = 75f;                     // 75% (smart detection)
Percent p4 = 75.Percent();            // 75% (extension)
Percent p5 = 0.75f.AsFractionPercent(); // 75% (explicit fraction)
Percent p6 = Percent.FromPercent(75); // 75% (factory)
Percent p7 = Percent.FromPercent(75f);// 75% (factory)
Percent p8 = Percent.FromFraction(0.75f); // 75% (fraction factory)
Percent p9 = Percent.FromInt(75);     // 75% (super explicit)
Percent p10 = Percent.FromFloatPercent(75f); // 75%
Percent p11 = Percent.Zero;           // 0%
Percent p12 = Percent.One;            // 100%
Percent p13 = Percent.Half;           // 50%

// ─── Damage Calculation with 300% Bonus ─────────────────────────────────────

float baseDamage = 100f;
Percent critBonus = 300;              // 300% bonus = 3.0x multiplier

// Method 1: Using AddTo (base + bonus)
float totalDamage = critBonus.AddTo(baseDamage);  // 100 * (1 + 3.0) = 400

// Method 2: Using Bonus explicitly
float bonusAmount = critBonus.Bonus(baseDamage);  // 300
float total = baseDamage + bonusAmount;           // 400

// Method 3: Direct multiplication for pure multiplier
Percent damageMultiplier = 400;       // 400% = 4.0x
float scaled = damageMultiplier.Of(baseDamage);  // 400

// ─── Damage Reduction (Armor) ───────────────────────────────────────────────

Percent armorReduction = 25;          // 25% damage reduction
float incomingDamage = 100f;
float finalDamage = armorReduction.Reduce(incomingDamage);  // 75

// Stacking reductions (multiplicative)
Percent physicalArmor = 30;
Percent magicShield = 20;
float afterArmor = physicalArmor.Reduce(100);     // 70
float afterBoth = magicShield.Reduce(afterArmor); // 56

// ─── Health Bar (Clamped to 0-100%) ─────────────────────────────────────────

float currentHp = 150f;
float maxHp = 100f;
Percent healthPercent = (currentHp / maxHp).AsFractionPercent().Clamped(); // 100% (not 150%)

float lowHp = 25f;
Percent lowHealthPercent = (lowHp / maxHp).AsFractionPercent(); // 25%
if (lowHealthPercent < Percent.TwentyFive) {
    // Trigger low health warning!
}

// ─── Buff Stacking (No Upper Limit!) ────────────────────────────────────────

Percent attackSpeedBase = Percent.One;  // 100% base
Percent hasteBonus = 50;                // +50%
Percent berserkerRage = 100;            // +100%
Percent bloodlust = 30;                 // +30%

Percent totalSpeed = attackSpeedBase + hasteBonus + berserkerRage + bloodlust; // 280%!

float attackInterval = 1.0f;            // 1 second base
float actualInterval = attackInterval / totalSpeed.Fraction; // 0.357 seconds

// ─── Lerping for Smooth Animations ──────────────────────────────────────────

Percent fadeStart = Percent.Zero;       // 0% opacity
Percent fadeEnd = Percent.One;          // 100% opacity
Percent halfway = Percent.Lerp(fadeStart, fadeEnd, Percent.Half); // 50%

// Lerp between damage multipliers
Percent lowDamage = 100;                // 100% (1x)
Percent highDamage = 300;               // 300% (3x)
Percent midDamage = Percent.Lerp(lowDamage, highDamage, 0.5f); // 200%

// ─── String Formatting ──────────────────────────────────────────────────────

Percent dropChance = 0.5f;              // 50%
Percent massiveBonus = 1234;            // 1234%

Console.WriteLine($"Drop chance: {dropChance}");           // "Drop chance: 50%"
Console.WriteLine($"Damage bonus: {massiveBonus}");        // "Damage bonus: 1234%"
Console.WriteLine($"Precise: {dropChance:P2}");            // "Precise: 50.00%"
Console.WriteLine($"Multiplier: {massiveBonus:F2}");       // "Multiplier: 12.34"

// ─── Inverse for Reciprocal Calculations ────────────────────────────────────

Percent attackSpeed = 200;              // 200% attack speed
Percent attackTime = attackSpeed.Inverse; // 50% of base time (faster!)

Percent cooldownReduction = 40;
Percent effectiveCooldown = (Percent.One - cooldownReduction).Clamped(); // 60%

// ─── Comparison for Thresholds ──────────────────────────────────────────────

Percent resistance = 75;
if (resistance >= Percent.SeventyFive) {
    // "Highly resistant!"
}

if (resistance > Percent.One) {
    // Over 100% resistance - immune with reflection?
}

// ─── Safe Arithmetic (Always ≥0) ────────────────────────────────────────────

Percent debuff = 150;                   // -150% to something
Percent stat = 100;
Percent result = stat - debuff;         // Clamped to 0%, not -50%!

// ─── Path of Exile Style Stacking ───────────────────────────────────────────

// Increased damage sources (additive with each other)
Percent[] increasedDamage = [20, 30, 50, 100]; // +20%, +30%, +50%, +100%
Percent totalIncreased = increasedDamage.Aggregate(Percent.Zero, (a, b) => a + b); // 200%

// More damage sources (multiplicative)
Percent[] moreDamage = [50, 30]; // 50% more, 30% more
Percent totalMore = moreDamage.Aggregate(Percent.One, (a, b) => new Percent(a.Fraction * (1 + b.Fraction)));
// 1.0 * 1.5 * 1.3 = 1.95 = 195%

float baseDmg = 100;
float withIncreased = (Percent.One + totalIncreased).Of(baseDmg); // 100 * 3.0 = 300
float withMore = totalMore.Of(withIncreased); // 300 * 1.95 = 585

*/

#endregion
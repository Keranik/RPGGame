using System;
using Unity.Mathematics;

namespace RPGGame.Core;

/// <summary>
/// An immutable, self-documenting probability wrapper (0.0 to 1.0).
/// 
/// <para>
/// Use <see cref="RandomStream.Check(ChanceRPG)"/> for deterministic rolls,
/// or <see cref="RollsSuccess()"/> for quick non-deterministic checks.
/// </para>
/// 
/// <example>
/// <code>
/// // Creation
/// ChanceRPG crit = 5.Chance();           // 5%
/// ChanceRPG likely = 0.75f.Probability(); // 75%
/// ChanceRPG rare = ChanceRPG.Rare;       // 5%
/// 
/// // Deterministic rolling (preferred for gameplay)
/// var rng = GameRandom.For("Combat");
/// if (rng.Check(crit)) { ApplyCritical(); }
/// 
/// // Quick non-deterministic check (for UI, non-critical)
/// if (ChanceRPG.FiftyFifty.RollsSuccess()) { /* coin flip */ }
/// 
/// // Modification
/// ChanceRPG boosted = crit.WithAdvantage(); // ~9.75%
/// </code>
/// </example>
/// </summary>
[Serializable]
public readonly struct ChanceRPG : IEquatable<ChanceRPG>, IComparable<ChanceRPG>, IFormattable {
	#region Fields

	private readonly float _probability;
	private const float EPSILON = 0.0001f;

	#endregion

	#region Constructor

	/// <summary>
	/// Creates a ChanceRPG from a probability value (0.0 to 1.0).
	/// Values are clamped to valid range.
	/// </summary>
	public ChanceRPG(float probability) {
		_probability = math.clamp(probability, 0f, 1f);
	}

	#endregion

	#region Static Presets

	/// <summary>0% - Impossible.</summary>
	public static ChanceRPG Impossible => new(0f);

	/// <summary>5% - Rare (natural 20 on d20).</summary>
	public static ChanceRPG Rare => new(0.05f);

	/// <summary>10% - Uncommon.</summary>
	public static ChanceRPG Uncommon => new(0.10f);

	/// <summary>20% - Low.</summary>
	public static ChanceRPG Low => new(0.20f);

	/// <summary>25% - Quarter.</summary>
	public static ChanceRPG Quarter => new(0.25f);

	/// <summary>33% - Third.</summary>
	public static ChanceRPG Third => new(0.333f);

	/// <summary>50% - Coin flip.</summary>
	public static ChanceRPG FiftyFifty => new(0.50f);

	/// <summary>66% - Two thirds.</summary>
	public static ChanceRPG TwoThirds => new(0.666f);

	/// <summary>75% - Likely.</summary>
	public static ChanceRPG Likely => new(0.75f);

	/// <summary>90% - Very likely.</summary>
	public static ChanceRPG VeryLikely => new(0.90f);

	/// <summary>95% - Almost certain.</summary>
	public static ChanceRPG AlmostCertain => new(0.95f);

	/// <summary>100% - Guaranteed.</summary>
	public static ChanceRPG Guaranteed => new(1.0f);

	// RPG Specials
	/// <summary>5% - Critical hit (natural 20).</summary>
	public static ChanceRPG CriticalHit => Rare;

	/// <summary>5% - Critical failure (natural 1).</summary>
	public static ChanceRPG CriticalFailure => Rare;

	/// <summary>1% - Epic drop rate.</summary>
	public static ChanceRPG EpicDrop => new(0.01f);

	/// <summary>0.1% - Legendary drop rate.</summary>
	public static ChanceRPG LegendaryDrop => new(0.001f);

	#endregion

	#region Factories

	/// <summary>Creates from an integer percentage (0-100).</summary>
	public static ChanceRPG FromPercent(int percent) => new(percent / 100f);

	/// <summary>Creates from a float percentage (0-100).</summary>
	public static ChanceRPG FromPercent(float percent) => new(percent / 100f);

	/// <summary>Creates from a Percent struct.</summary>
	public static ChanceRPG FromPercent(Percent percent) => new(percent.Fraction);

	/// <summary>Creates from a probability fraction (0.0-1.0).</summary>
	public static ChanceRPG FromProbability(float probability) => new(probability);

	/// <summary>Creates from odds (e.g., "1 in 4" = 25%).</summary>
	public static ChanceRPG FromOdds(int successCount, int totalPool) {
		if (totalPool <= 0) return Impossible;
		return new ChanceRPG((float)successCount / totalPool);
	}

	/// <summary>Returns the minimum of two chances.</summary>
	public static ChanceRPG Min(ChanceRPG a, ChanceRPG b) =>
		new(math.min(a._probability, b._probability));

	/// <summary>Returns the maximum of two chances.</summary>
	public static ChanceRPG Max(ChanceRPG a, ChanceRPG b) =>
		new(math.max(a._probability, b._probability));

	/// <summary>Linearly interpolates between two chances.</summary>
	public static ChanceRPG Lerp(ChanceRPG a, ChanceRPG b, float t) =>
		new(math.lerp(a._probability, b._probability, math.clamp(t, 0f, 1f)));

	#endregion

	#region Properties

	/// <summary>The probability as a fraction (0.0 to 1.0).</summary>
	public float Probability => _probability;

	/// <summary>Alias for Probability.</summary>
	public float Value => _probability;

	/// <summary>The probability as a percentage (0 to 100).</summary>
	public float AsPercent => _probability * 100f;

	/// <summary>Converts to a Percent struct.</summary>
	public Percent ToPercent => Percent.FromFraction(_probability);

	/// <summary>True if this chance is 0%.</summary>
	public bool IsImpossible => _probability <= EPSILON;

	/// <summary>True if this chance is 100%.</summary>
	public bool IsGuaranteed => _probability >= 1f - EPSILON;

	/// <summary>True if this chance is ≤20%.</summary>
	public bool IsRare => _probability <= 0.2f + EPSILON;

	/// <summary>True if this chance is ≥70%.</summary>
	public bool IsLikely => _probability >= 0.7f - EPSILON;

	/// <summary>True if this chance is between 45-55%.</summary>
	public bool IsEvenOdds => _probability >= 0.45f && _probability <= 0.55f;

	/// <summary>Descriptive name for this probability range.</summary>
	public string DescriptiveName => _probability switch {
		<= EPSILON => "Impossible",
		<= 0.05f + EPSILON => "Rare",
		<= 0.10f + EPSILON => "Uncommon",
		<= 0.25f + EPSILON => "Unlikely",
		<= 0.40f + EPSILON => "Possible",
		<= 0.60f + EPSILON => "Even Odds",
		<= 0.75f + EPSILON => "Likely",
		<= 0.90f + EPSILON => "Very Likely",
		<= 0.99f + EPSILON => "Almost Certain",
		_ => "Guaranteed"
	};

	#endregion

	#region Rolling (Non-Deterministic)

	/// <summary>
	/// Rolls this chance using Unity.Mathematics.Random.
	/// <b>Note:</b> For deterministic gameplay, use <see cref="RandomStream.Check(ChanceRPG)"/> instead.
	/// </summary>
	public bool RollsSuccess() {
		if (IsGuaranteed) return true;
		if (IsImpossible) return false;

		// Use a simple non-deterministic approach for quick checks
		uint seed = (uint)DateTime.UtcNow.Ticks;
		var rng = new Unity.Mathematics.Random(seed == 0 ? 1 : seed);
		return rng.NextFloat() < _probability;
	}

	/// <summary>
	/// Rolls this chance against another (opposed check).
	/// </summary>
	public bool BeatsOpposition(ChanceRPG opposition) {
		return RollsSuccess() && !opposition.RollsSuccess();
	}

	#endregion

	#region Modification

	/// <summary>Returns the inverse probability (1 - p).</summary>
	public ChanceRPG Inverse() => new(1f - _probability);

	/// <summary>
	/// Returns the chance with advantage (roll twice, take better).
	/// P = 1 - (1-p)²
	/// </summary>
	public ChanceRPG WithAdvantage() {
		float fail = 1f - _probability;
		return new ChanceRPG(1f - fail * fail);
	}

	/// <summary>
	/// Returns the chance with disadvantage (roll twice, take worse).
	/// P = p²
	/// </summary>
	public ChanceRPG WithDisadvantage() {
		return new ChanceRPG(_probability * _probability);
	}

	/// <summary>
	/// Returns the chance with super advantage (roll three times, take best).
	/// P = 1 - (1-p)³
	/// </summary>
	public ChanceRPG WithSuperAdvantage() {
		float fail = 1f - _probability;
		return new ChanceRPG(1f - fail * fail * fail);
	}

	/// <summary>Boosts by adding a probability amount.</summary>
	public ChanceRPG BoostedBy(float amount) => new(_probability + amount);

	/// <summary>Boosts by adding a Percent.</summary>
	public ChanceRPG BoostedBy(Percent bonus) => new(_probability + bonus.Fraction);

	/// <summary>Reduces by subtracting a probability amount.</summary>
	public ChanceRPG ReducedBy(float amount) => new(_probability - amount);

	/// <summary>Reduces by subtracting a Percent.</summary>
	public ChanceRPG ReducedBy(Percent penalty) => new(_probability - penalty.Fraction);

	/// <summary>Multiplies by a factor.</summary>
	public ChanceRPG MultipliedBy(float multiplier) => new(_probability * multiplier);

	/// <summary>Returns this chance halved.</summary>
	public ChanceRPG Halved() => new(_probability * 0.5f);

	/// <summary>Returns this chance doubled (capped at 100%).</summary>
	public ChanceRPG Doubled() => new(_probability * 2f);

	/// <summary>Clamps to a minimum value.</summary>
	public ChanceRPG ClampedMin(ChanceRPG min) => new(math.max(_probability, min._probability));

	/// <summary>Clamps to a maximum value.</summary>
	public ChanceRPG ClampedMax(ChanceRPG max) => new(math.min(_probability, max._probability));

	/// <summary>Clamps between min and max.</summary>
	public ChanceRPG Clamped(ChanceRPG min, ChanceRPG max) =>
		new(math.clamp(_probability, min._probability, max._probability));

	#endregion

	#region Combining

	/// <summary>
	/// Combined chance of both this AND another succeeding.
	/// P(A and B) = P(A) × P(B)
	/// </summary>
	public ChanceRPG And(ChanceRPG other) => new(_probability * other._probability);

	/// <summary>
	/// Combined chance of either this OR another succeeding.
	/// P(A or B) = P(A) + P(B) - P(A)×P(B)
	/// </summary>
	public ChanceRPG Or(ChanceRPG other) =>
		new(_probability + other._probability - _probability * other._probability);

	/// <summary>
	/// Chance of succeeding at least once in N attempts.
	/// P = 1 - (1-p)^n
	/// </summary>
	public ChanceRPG AtLeastOnceIn(int attempts) {
		if (attempts <= 0) return Impossible;
		if (IsGuaranteed) return Guaranteed;
		float fail = math.pow(1f - _probability, attempts);
		return new ChanceRPG(1f - fail);
	}

	/// <summary>
	/// Expected number of attempts needed for success.
	/// E[X] = 1/p
	/// </summary>
	public float ExpectedAttempts => _probability > EPSILON ? 1f / _probability : float.PositiveInfinity;

	/// <summary>Combines multiple chances with AND logic.</summary>
	public static ChanceRPG CombineAnd(params ChanceRPG[] chances) {
		if (chances == null || chances.Length == 0) return Guaranteed;

		float combined = 1f;
		foreach (var c in chances) {
			combined *= c._probability;
			if (combined <= EPSILON) return Impossible;
		}
		return new ChanceRPG(combined);
	}

	/// <summary>Combines multiple chances with OR logic.</summary>
	public static ChanceRPG CombineOr(params ChanceRPG[] chances) {
		if (chances == null || chances.Length == 0) return Impossible;

		float allFail = 1f;
		foreach (var c in chances) {
			allFail *= 1f - c._probability;
			if (allFail <= EPSILON) return Guaranteed;
		}
		return new ChanceRPG(1f - allFail);
	}

	#endregion

	#region Operators

	public static implicit operator float(ChanceRPG c) => c._probability;
	public static implicit operator ChanceRPG(float f) => f > 1f ? new(f / 100f) : new(f);
	public static implicit operator ChanceRPG(int percent) => new(percent / 100f);
	public static implicit operator Percent(ChanceRPG c) => Percent.FromFraction(c._probability);
	public static implicit operator ChanceRPG(Percent p) => new(p.Fraction);

	public static ChanceRPG operator +(ChanceRPG a, ChanceRPG b) => new(a._probability + b._probability);
	public static ChanceRPG operator +(ChanceRPG a, float b) => new(a._probability + b);
	public static ChanceRPG operator -(ChanceRPG a, ChanceRPG b) => new(a._probability - b._probability);
	public static ChanceRPG operator -(ChanceRPG a, float b) => new(a._probability - b);
	public static ChanceRPG operator *(ChanceRPG a, ChanceRPG b) => new(a._probability * b._probability);
	public static ChanceRPG operator *(ChanceRPG a, float b) => new(a._probability * b);
	public static ChanceRPG operator /(ChanceRPG a, float b) => new(b > EPSILON ? a._probability / b : 0f);
	public static ChanceRPG operator !(ChanceRPG c) => c.Inverse();

	public static bool operator ==(ChanceRPG a, ChanceRPG b) => math.abs(a._probability - b._probability) < EPSILON;
	public static bool operator !=(ChanceRPG a, ChanceRPG b) => !(a == b);
	public static bool operator <(ChanceRPG a, ChanceRPG b) => a._probability < b._probability - EPSILON;
	public static bool operator >(ChanceRPG a, ChanceRPG b) => a._probability > b._probability + EPSILON;
	public static bool operator <=(ChanceRPG a, ChanceRPG b) => a._probability <= b._probability + EPSILON;
	public static bool operator >=(ChanceRPG a, ChanceRPG b) => a._probability >= b._probability - EPSILON;

	#endregion

	#region IEquatable / IComparable

	public bool Equals(ChanceRPG other) => this == other;
	public override bool Equals(object? obj) => obj is ChanceRPG other && Equals(other);
	public override int GetHashCode() => _probability.GetHashCode();
	public int CompareTo(ChanceRPG other) => _probability.CompareTo(other._probability);

	#endregion

	#region Formatting

	public override string ToString() => $"{_probability * 100f:0.#}%";

	public string ToString(string? format, IFormatProvider? formatProvider = null) {
		if (string.IsNullOrEmpty(format) || format == "G") return ToString();

		return format.ToLowerInvariant() switch {
			"desc" => DescriptiveName,
			"odds" => ToOddsString(),
			"frac" => _probability.ToString("0.###", formatProvider),
			_ => $"{(_probability * 100f).ToString(format, formatProvider)}%"
		};
	}

	private string ToOddsString() {
		if (IsImpossible) return "never";
		if (IsGuaranteed) return "always";

		int[] denoms = [2, 3, 4, 5, 6, 8, 10, 20, 100];
		foreach (int d in denoms) {
			float n = _probability * d;
			int rounded = (int)math.round(n);
			if (rounded > 0 && rounded <= d && math.abs(n - rounded) < 0.1f) {
				int gcd = GCD(rounded, d);
				return $"{rounded / gcd} in {d / gcd}";
			}
		}
		return $"{_probability * 100:0}%";
	}

	private static int GCD(int a, int b) {
		while (b != 0) { int t = b; b = a % b; a = t; }
		return a;
	}

	#endregion
}

/// <summary>
/// Extension methods for creating ChanceRPG from numeric types.
/// </summary>
public static class ChanceRPGExtensions {
	/// <summary>Creates a ChanceRPG from an integer percentage.</summary>
	/// <example>75.Chance() → 75%</example>
	public static ChanceRPG Chance(this int percent) => ChanceRPG.FromPercent(percent);

	/// <summary>Creates a ChanceRPG from a float percentage.</summary>
	/// <example>75.5f.Chance() → 75.5%</example>
	public static ChanceRPG Chance(this float percent) => ChanceRPG.FromPercent(percent);

	/// <summary>Creates a ChanceRPG from a probability fraction.</summary>
	/// <example>0.75f.Probability() → 75%</example>
	public static ChanceRPG Probability(this float fraction) => ChanceRPG.FromProbability(fraction);

	/// <summary>Converts a Percent to ChanceRPG.</summary>
	public static ChanceRPG ToChance(this Percent percent) => ChanceRPG.FromPercent(percent);
}
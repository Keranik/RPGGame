namespace RPGGame.Core;

using System;
using System.Text.RegularExpressions;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Immutable representation of hit dice (e.g. 3d8+4).
/// Used for damage rolls, healing, stat generation, and any dice-based mechanics.
/// </summary>
[Serializable]
public readonly struct HitDice : IEquatable<HitDice>, IComparable<HitDice> {
	#region Backing Fields

	[SerializeField] private readonly int numberOfDice;
	[SerializeField] private readonly int sidesOfDice;
	[SerializeField] private readonly int bonus;

	#endregion

	#region Properties - Full Names

	/// <summary>Number of dice to roll (the N in NdS+B).</summary>
	public int NumberOfDice => numberOfDice;

	/// <summary>Number of sides on each die (the S in NdS+B).</summary>
	public int SidesOfDice => sidesOfDice;

	/// <summary>Flat bonus added to the roll (the B in NdS+B).</summary>
	public int Bonus => bonus;

	#endregion

	#region Properties - Short Aliases

	/// <summary>Alias for NumberOfDice.</summary>
	public int Count => numberOfDice;

	/// <summary>Alias for SidesOfDice.</summary>
	public int Sides => sidesOfDice;

	/// <summary>Alias for Bonus.</summary>
	public int Modifier => bonus;

	#endregion

	#region Common Dice Constants

	public static readonly HitDice None = new(0, 0, 0);
	public static readonly HitDice D4 = new(1, 4);
	public static readonly HitDice D6 = new(1, 6);
	public static readonly HitDice D8 = new(1, 8);
	public static readonly HitDice D10 = new(1, 10);
	public static readonly HitDice D12 = new(1, 12);
	public static readonly HitDice D20 = new(1, 20);
	public static readonly HitDice D100 = new(1, 100);

	public static readonly HitDice TwoD4 = new(2, 4);
	public static readonly HitDice TwoD6 = new(2, 6);
	public static readonly HitDice TwoD8 = new(2, 8);
	public static readonly HitDice TwoD10 = new(2, 10);
	public static readonly HitDice TwoD12 = new(2, 12);
	public static readonly HitDice ThreeD6 = new(3, 6);
	public static readonly HitDice FourD6 = new(4, 6);

	#endregion

	#region Constructors

	public HitDice(int numberOfDice, int sidesOfDice, int bonus = 0) {
		this.numberOfDice = math.max(0, numberOfDice);
		this.sidesOfDice = math.max(0, sidesOfDice);
		this.bonus = bonus;
	}

	/// <summary>
	/// Creates a HitDice from a single die type with optional count and bonus.
	/// </summary>
	public static HitDice Create(int sides, int count = 1, int bonus = 0) {
		return new HitDice(count, sides, bonus);
	}

	#endregion

	#region Modification Methods

	/// <summary>Adds dice of the same type.</summary>
	public HitDice AddDice(int diceToAdd) => new(NumberOfDice + diceToAdd, SidesOfDice, Bonus);

	/// <summary>Removes dice (minimum 0).</summary>
	public HitDice RemoveDice(int diceToRemove) => new(math.max(0, NumberOfDice - diceToRemove), SidesOfDice, Bonus);

	/// <summary>Upgrades the die size (e.g., d6 -> d8).</summary>
	public HitDice UpgradeDieSize(int steps = 1) {
		int[] sizes = [4, 6, 8, 10, 12, 20];
		int currentIndex = Array.IndexOf(sizes, SidesOfDice);
		if (currentIndex < 0) {
			return new HitDice(NumberOfDice, SidesOfDice + (2 * steps), Bonus);
		}
		int newIndex = math.min(currentIndex + steps, sizes.Length - 1);
		return new HitDice(NumberOfDice, sizes[newIndex], Bonus);
	}

	/// <summary>Downgrades the die size (e.g., d8 -> d6).</summary>
	public HitDice DowngradeDieSize(int steps = 1) {
		int[] sizes = [4, 6, 8, 10, 12, 20];
		int currentIndex = Array.IndexOf(sizes, SidesOfDice);
		if (currentIndex < 0) {
			return new HitDice(NumberOfDice, math.max(4, SidesOfDice - (2 * steps)), Bonus);
		}
		int newIndex = math.max(0, currentIndex - steps);
		return new HitDice(NumberOfDice, sizes[newIndex], Bonus);
	}

	/// <summary>Adds sides to the die.</summary>
	public HitDice AddSides(int sidesToAdd) => new(NumberOfDice, math.max(1, SidesOfDice + sidesToAdd), Bonus);

	/// <summary>Adds to the flat bonus.</summary>
	public HitDice AddBonus(int bonusToAdd) => new(NumberOfDice, SidesOfDice, Bonus + bonusToAdd);

	/// <summary>Sets the bonus to a specific value.</summary>
	public HitDice WithBonus(int newBonus) => new(NumberOfDice, SidesOfDice, newBonus);

	/// <summary>Multiplies the number of dice.</summary>
	public HitDice MultiplyDice(int multiplier) => new(NumberOfDice * multiplier, SidesOfDice, Bonus);

	/// <summary>Doubles the dice (for critical hits).</summary>
	public HitDice Double() => new(NumberOfDice * 2, SidesOfDice, Bonus);

	/// <summary>Maximizes the roll (all dice show max value).</summary>
	public HitDice Maximize() => new(0, 0, Maximum);

	#endregion

	#region Statistics

	/// <summary>Gets the average roll value.</summary>
	public double Average => NumberOfDice * (SidesOfDice + 1) / 2.0 + Bonus;

	/// <summary>Gets the average roll as an integer (rounded).</summary>
	public int AverageInt => (int)Math.Round(Average);

	/// <summary>Gets the minimum possible roll.</summary>
	public int Minimum => NumberOfDice > 0 ? NumberOfDice + Bonus : Bonus;

	/// <summary>Gets the maximum possible roll.</summary>
	public int Maximum => (NumberOfDice * SidesOfDice) + Bonus;

	/// <summary>Gets the range of possible results.</summary>
	public (int min, int max) Range => (Minimum, Maximum);

	/// <summary>Gets the variance of the roll.</summary>
	public double Variance => NumberOfDice * ((SidesOfDice * SidesOfDice - 1) / 12.0);

	/// <summary>Gets the standard deviation of the roll.</summary>
	public double StandardDeviation => Math.Sqrt(Variance);

	/// <summary>Whether this represents a valid die roll.</summary>
	public bool IsValid => NumberOfDice > 0 && SidesOfDice > 0;

	/// <summary>Whether this is empty/zero.</summary>
	public bool IsEmpty => NumberOfDice == 0 || SidesOfDice == 0;

	#endregion

	#region Parsing

	private static readonly Regex DiceNotationRegex = new(
		@"^(\d+)?d(\d+)([+-]\d+)?$",
		RegexOptions.IgnoreCase | RegexOptions.Compiled
	);

	/// <summary>
	/// Parses a dice notation string (e.g., "2d6+3") into a HitDice.
	/// </summary>
	public static HitDice Parse(string notation) {
		if (string.IsNullOrWhiteSpace(notation)) return D6;

		var match = DiceNotationRegex.Match(notation.Trim());
		if (!match.Success) {
			Debug.LogWarning($"Invalid dice notation: {notation}. Defaulting to 1d6.");
			return D6;
		}

		int count = string.IsNullOrEmpty(match.Groups[1].Value) ? 1 : int.Parse(match.Groups[1].Value);
		int sides = int.Parse(match.Groups[2].Value);
		int mod = string.IsNullOrEmpty(match.Groups[3].Value) ? 0 : int.Parse(match.Groups[3].Value);

		return new HitDice(count, sides, mod);
	}

	/// <summary>
	/// Tries to parse a dice notation string.
	/// </summary>
	public static bool TryParse(string notation, out HitDice result) {
		result = default;
		if (string.IsNullOrWhiteSpace(notation)) return false;

		var match = DiceNotationRegex.Match(notation.Trim());
		if (!match.Success) return false;

		int count = string.IsNullOrEmpty(match.Groups[1].Value) ? 1 : int.Parse(match.Groups[1].Value);
		int sides = int.Parse(match.Groups[2].Value);
		int mod = string.IsNullOrEmpty(match.Groups[3].Value) ? 0 : int.Parse(match.Groups[3].Value);

		result = new HitDice(count, sides, mod);
		return true;
	}

	#endregion

	#region Factory Methods

	/// <summary>
	/// Creates a HitDice that averages around the target value using d6.
	/// </summary>
	public static HitDice CreateFromAverage(int targetAverage) {
		return CreateFromAverage(targetAverage, 6);
	}

	/// <summary>
	/// Creates a HitDice that averages around the target value using specified die size.
	/// </summary>
	public static HitDice CreateFromAverage(int targetAverage, int preferredDieSize) {
		if (targetAverage <= 0) return default;

		double avgPerDie = (preferredDieSize + 1) / 2.0;
		int dice = math.max(1, (int)Math.Round(targetAverage / avgPerDie));
		int mod = targetAverage - (int)(dice * avgPerDie);

		return new HitDice(dice, preferredDieSize, mod);
	}

	/// <summary>
	/// Creates a HitDice scaled for a given level/CR.
	/// </summary>
	public static HitDice CreateForLevel(int level, int baseDieSize = 8) {
		int dice = math.max(1, (level + 1) / 2);
		int mod = level / 4;
		return new HitDice(dice, baseDieSize, mod);
	}

	#endregion

	#region Rolling (Non-Deterministic)

	/// <summary>
	/// Rolls this dice using non-deterministic random.
	/// For deterministic gameplay, use <see cref="RandomStream.Roll"/> instead.
	/// </summary>
	public int Roll() => DiceRoller.Roll(this);

	/// <summary>
	/// Rolls with advantage (non-deterministic).
	/// </summary>
	public int RollAdvantage() => DiceRoller.RollAdvantage(this);

	/// <summary>
	/// Rolls with disadvantage (non-deterministic).
	/// </summary>
	public int RollDisadvantage() => DiceRoller.RollDisadvantage(this);

	/// <summary>
	/// Rolls and checks against a target DC (non-deterministic).
	/// </summary>
	public (int roll, bool success) RollAgainst(int targetDC) {
		int roll = Roll();
		return (roll, roll >= targetDC);
	}

	#endregion

	#region Probability

	/// <summary>
	/// Calculates the chance to roll at least the target value.
	/// </summary>
	public ChanceRPG GetChanceToMeet(int target) {
		if (!IsValid) return ChanceRPG.Impossible;

		if (target <= Minimum) return ChanceRPG.Guaranteed;
		if (target > Maximum) return ChanceRPG.Impossible;

		// Single die: exact calculation
		if (NumberOfDice == 1) {
			int successCount = Maximum - target + 1;
			return ChanceRPG.FromProbability((float)successCount / SidesOfDice);
		}

		// Multiple dice: normal approximation
		double stdDev = StandardDeviation;
		if (stdDev < 0.001) {
			return AverageInt >= target ? ChanceRPG.Guaranteed : ChanceRPG.Impossible;
		}

		double z = (target - 0.5 - Average) / stdDev;
		double probability = 1.0 - NormalCDF(z);
		return ChanceRPG.FromProbability((float)probability);
	}

	/// <summary>
	/// Calculates the chance to roll strictly higher than the target.
	/// </summary>
	public ChanceRPG GetChanceToExceed(int target) => GetChanceToMeet(target + 1);

	/// <summary>
	/// Gets the critical hit chance for this die.
	/// For d20, returns 5%. For other dice, returns chance of max roll.
	/// </summary>
	public ChanceRPG CritChance {
		get {
			if (!IsValid) return ChanceRPG.Impossible;
			if (SidesOfDice == 20 && NumberOfDice == 1) return ChanceRPG.CriticalHit;
			return ChanceRPG.FromProbability(math.pow(1f / SidesOfDice, NumberOfDice));
		}
	}

	/// <summary>
	/// Gets the chance of rolling maximum on all dice.
	/// </summary>
	public ChanceRPG MaxRollChance {
		get {
			if (!IsValid) return ChanceRPG.Impossible;
			return ChanceRPG.FromProbability(math.pow(1f / SidesOfDice, NumberOfDice));
		}
	}

	/// <summary>
	/// Gets the chance of rolling minimum on all dice.
	/// </summary>
	public ChanceRPG MinRollChance => MaxRollChance;

	private static double NormalCDF(double z) {
		const double a1 = 0.254829592;
		const double a2 = -0.284496736;
		const double a3 = 1.421413741;
		const double a4 = -1.453152027;
		const double a5 = 1.061405429;
		const double p = 0.3275911;

		int sign = z < 0 ? -1 : 1;
		z = Math.Abs(z) / Math.Sqrt(2);

		double t = 1.0 / (1.0 + p * z);
		double y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-z * z);

		return 0.5 * (1.0 + sign * y);
	}

	#endregion

	#region String Conversion

	/// <summary>
	/// Returns the standard dice notation string (e.g., "2d6+3").
	/// </summary>
	public override string ToString() {
		if (IsEmpty) return "0";

		string baseNotation = $"{NumberOfDice}d{SidesOfDice}";

		if (Bonus > 0) return $"{baseNotation}+{Bonus}";
		if (Bonus < 0) return $"{baseNotation}{Bonus}";
		return baseNotation;
	}

	/// <summary>Returns dice notation string.</summary>
	public string ToNotation() => ToString();

	/// <summary>Returns a display-friendly string with average.</summary>
	public string ToDisplayString() {
		if (IsEmpty) return "0";
		return $"{this} (avg {AverageInt})";
	}

	/// <summary>
	/// Gets a tooltip-friendly display of this dice's statistics.
	/// </summary>
	public string GetTooltip() {
		if (IsEmpty) return "No dice";

		return $"{this}\n" +
			   $"Range: {Minimum}-{Maximum}\n" +
			   $"Average: {AverageInt}\n" +
			   $"Std Dev: {StandardDeviation:0.#}";
	}

	/// <summary>
	/// Gets a tooltip showing chance to meet various DCs.
	/// </summary>
	public string GetDCChanceTooltip(params int[] dcs) {
		if (IsEmpty || dcs.Length == 0) return "";

		var lines = new System.Text.StringBuilder();
		lines.AppendLine($"{this}:");

		foreach (int dc in dcs) {
			var chance = GetChanceToMeet(dc);
			lines.AppendLine($"  vs DC {dc}: {chance}");
		}

		return lines.ToString().TrimEnd();
	}

	#endregion

	#region Operators

	public static HitDice operator +(HitDice a, HitDice b) {
		if (a.IsEmpty) return b;
		if (b.IsEmpty) return a;

		if (a.SidesOfDice == b.SidesOfDice) {
			return new HitDice(a.NumberOfDice + b.NumberOfDice, a.SidesOfDice, a.Bonus + b.Bonus);
		}

		int totalAvg = (int)Math.Round(a.Average + b.Average);
		int sides = math.max(a.SidesOfDice, b.SidesOfDice);
		return CreateFromAverage(totalAvg, sides);
	}

	public static HitDice operator +(HitDice dice, int bonus) => dice.AddBonus(bonus);
	public static HitDice operator -(HitDice dice, int penalty) => dice.AddBonus(-penalty);
	public static HitDice operator *(HitDice dice, int multiplier) => dice.MultiplyDice(multiplier);

	public static implicit operator HitDice((int count, int sides) tuple) => new(tuple.count, tuple.sides);
	public static implicit operator HitDice((int count, int sides, int bonus) tuple) => new(tuple.count, tuple.sides, tuple.bonus);

	#endregion

	#region Equality & Comparison

	public bool Equals(HitDice other) =>
		numberOfDice == other.numberOfDice &&
		sidesOfDice == other.sidesOfDice &&
		bonus == other.bonus;

	public override bool Equals(object? obj) => obj is HitDice other && Equals(other);
	public override int GetHashCode() => HashCode.Combine(numberOfDice, sidesOfDice, bonus);
	public int CompareTo(HitDice other) => Average.CompareTo(other.Average);

	public static bool operator ==(HitDice left, HitDice right) => left.Equals(right);
	public static bool operator !=(HitDice left, HitDice right) => !left.Equals(right);
	public static bool operator <(HitDice left, HitDice right) => left.CompareTo(right) < 0;
	public static bool operator >(HitDice left, HitDice right) => left.CompareTo(right) > 0;
	public static bool operator <=(HitDice left, HitDice right) => left.CompareTo(right) <= 0;
	public static bool operator >=(HitDice left, HitDice right) => left.CompareTo(right) >= 0;

	#endregion
}

/// <summary>
/// Extension methods for HitDice with RandomStream integration.
/// </summary>
public static class HitDiceExtensions {
	/// <summary>
	/// Rolls this dice using a RandomStream (deterministic).
	/// </summary>
	public static int Roll(this HitDice dice, ref RandomStream rng) => rng.Roll(dice);

	/// <summary>
	/// Rolls with advantage using a RandomStream.
	/// </summary>
	public static int RollAdvantage(this HitDice dice, ref RandomStream rng) => rng.RollAdvantage(dice);

	/// <summary>
	/// Rolls with disadvantage using a RandomStream.
	/// </summary>
	public static int RollDisadvantage(this HitDice dice, ref RandomStream rng) => rng.RollDisadvantage(dice);

	/// <summary>
	/// Rolls and checks against a target DC using a RandomStream.
	/// </summary>
	public static (int roll, bool success) RollAgainst(this HitDice dice, ref RandomStream rng, int targetDC) {
		int roll = rng.Roll(dice);
		return (roll, roll >= targetDC);
	}

	/// <summary>
	/// Performs a d20 check using this dice's bonus as modifier.
	/// Only valid for d20 dice.
	/// </summary>
	public static D20Result RollD20Check(this HitDice dice, ref RandomStream rng, int targetDC) {
		if (dice.SidesOfDice != 20 || dice.NumberOfDice != 1) {
			throw new InvalidOperationException("RollD20Check requires a d20");
		}
		return rng.RollD20(dice.Bonus, targetDC);
	}
}
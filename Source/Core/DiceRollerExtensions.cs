using System;
using System.Text.RegularExpressions;
using Unity.Mathematics;
using UnityEngine;

namespace RPGGame.Core;

/// <summary>
/// Extension methods for dice notation parsing and detailed roll results.
/// </summary>
public static class DiceRollerExtensions {
	private static readonly Regex DiceNotationRegex = new(
		@"^(\d+)?d(\d+)([+-]\d+)?$",
		RegexOptions.IgnoreCase | RegexOptions.Compiled
	);

	#region Parsing

	/// <summary>
	/// Parses a dice notation string (e.g., "2d6+3") into HitDice.
	/// </summary>
	public static HitDice ParseDiceNotation(string notation) {
		return HitDice.Parse(notation);
	}

	/// <summary>
	/// Tries to parse a dice notation string.
	/// </summary>
	public static bool TryParseDiceNotation(string notation, out HitDice hitDice) {
		return HitDice.TryParse(notation, out hitDice);
	}

	#endregion

	#region Convenience Roll Methods

	/// <summary>
	/// Rolls dice using a notation string (non-deterministic).
	/// </summary>
	public static int Roll(string notation) {
		return DiceRoller.Roll(HitDice.Parse(notation));
	}

	/// <summary>
	/// Rolls dice with advantage using a notation string.
	/// </summary>
	public static int RollAdvantage(string notation) {
		return DiceRoller.RollAdvantage(HitDice.Parse(notation));
	}

	/// <summary>
	/// Rolls dice with disadvantage using a notation string.
	/// </summary>
	public static int RollDisadvantage(string notation) {
		return DiceRoller.RollDisadvantage(HitDice.Parse(notation));
	}

	/// <summary>
	/// Rolls a single die.
	/// </summary>
	public static int RollSingle(int sides) => DiceRoller.RollDie(sides);

	/// <summary>
	/// Rolls a d100 (percentile).
	/// </summary>
	public static int RollD100() => DiceRoller.RollDie(100);

	#endregion

	#region Detailed Rolls

	/// <summary>
	/// Rolls dice and returns detailed results (non-deterministic).
	/// </summary>
	public static DiceRollResult RollDetailed(this HitDice dice) {
		var rng = GameRandom.FromSeed((uint)DateTime.UtcNow.Ticks);
		return RollDetailed(dice, ref rng);
	}

	/// <summary>
	/// Rolls dice with detailed results using a RandomStream (deterministic).
	/// </summary>
	public static DiceRollResult RollDetailed(this HitDice dice, ref RandomStream rng) {
		var rolls = new int[dice.NumberOfDice];
		int total = dice.Bonus;
		int maxRolls = 0;
		int minRolls = 0;

		for (int i = 0; i < dice.NumberOfDice; i++) {
			int roll = rng.RollDie(dice.SidesOfDice);
			rolls[i] = roll;
			total += roll;

			if (roll == dice.SidesOfDice) maxRolls++;
			if (roll == 1) minRolls++;
		}

		return new DiceRollResult {
			Dice = dice,
			IndividualRolls = rolls,
			Total = total,
			MaxRolls = maxRolls,
			MinRolls = minRolls
		};
	}

	/// <summary>
	/// Rolls dice with detailed results using notation string.
	/// </summary>
	public static DiceRollResult RollDetailed(string notation) {
		return HitDice.Parse(notation).RollDetailed();
	}

	#endregion

	#region Stat Generation

	/// <summary>
	/// Rolls 4d6 drop lowest for stat generation (non-deterministic).
	/// </summary>
	public static int Roll4d6DropLowest() {
		var rng = GameRandom.FromSeed((uint)DateTime.UtcNow.Ticks);
		return rng.Roll4d6DropLowest();
	}

	/// <summary>
	/// Generates a full set of 6 stats using 4d6 drop lowest (non-deterministic).
	/// </summary>
	public static int[] GenerateStatArray() {
		var rng = GameRandom.FromSeed((uint)DateTime.UtcNow.Ticks);
		return GenerateStatArray(ref rng);
	}

	/// <summary>
	/// Generates stats using a RandomStream (deterministic).
	/// </summary>
	public static int[] GenerateStatArray(ref RandomStream rng) {
		var stats = new int[6];

		for (int i = 0; i < 6; i++) {
			stats[i] = rng.Roll4d6DropLowest();
		}

		Array.Sort(stats);
		Array.Reverse(stats);

		return stats;
	}

	#endregion
}

/// <summary>
/// Detailed result of a dice roll showing individual die results.
/// </summary>
public class DiceRollResult {
	/// <summary>The dice that were rolled.</summary>
	public HitDice Dice { get; init; }

	/// <summary>Individual roll results for each die.</summary>
	public int[] IndividualRolls { get; init; } = Array.Empty<int>();

	/// <summary>Total result including bonus.</summary>
	public int Total { get; init; }

	/// <summary>Number of dice that rolled their maximum value.</summary>
	public int MaxRolls { get; init; }

	/// <summary>Number of dice that rolled 1.</summary>
	public int MinRolls { get; init; }

	/// <summary>Whether all dice rolled their maximum.</summary>
	public bool IsMaxRoll => MaxRolls == Dice.NumberOfDice;

	/// <summary>Whether all dice rolled 1.</summary>
	public bool IsMinRoll => MinRolls == Dice.NumberOfDice;

	/// <summary>Gets a formatted breakdown string.</summary>
	public string GetBreakdown() {
		string rolls = string.Join(" + ", IndividualRolls);

		if (Dice.Bonus > 0) return $"[{rolls}] + {Dice.Bonus} = {Total}";
		if (Dice.Bonus < 0) return $"[{rolls}] - {Math.Abs(Dice.Bonus)} = {Total}";
		return $"[{rolls}] = {Total}";
	}

	public override string ToString() => GetBreakdown();
}
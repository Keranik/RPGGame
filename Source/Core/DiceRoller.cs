using System;

namespace RPGGame.Core;

/// <summary>
/// Static utility class for non-deterministic dice rolling.
/// For deterministic gameplay, use <see cref="RandomStream"/> methods directly.
/// </summary>
public static class DiceRoller {
	#region Core Rolling

	/// <summary>
	/// Rolls dice (non-deterministic).
	/// </summary>
	public static int Roll(HitDice dice) {
		var rng = CreateQuickRng();
		return rng.Roll(dice);
	}

	/// <summary>
	/// Rolls dice with advantage.
	/// </summary>
	public static int RollAdvantage(HitDice dice) {
		var rng = CreateQuickRng();
		return rng.RollAdvantage(dice);
	}

	/// <summary>
	/// Rolls dice with disadvantage.
	/// </summary>
	public static int RollDisadvantage(HitDice dice) {
		var rng = CreateQuickRng();
		return rng.RollDisadvantage(dice);
	}

	/// <summary>
	/// Rolls dice with super advantage (3 rolls, take highest).
	/// </summary>
	public static int RollSuperAdvantage(HitDice dice) {
		var rng = CreateQuickRng();
		return rng.RollSuperAdvantage(dice);
	}

	/// <summary>
	/// Rolls a single die.
	/// </summary>
	public static int RollDie(int sides) {
		var rng = CreateQuickRng();
		return rng.RollDie(sides);
	}

	/// <summary>
	/// Rolls multiple dice.
	/// </summary>
	public static int RollDice(int count, int sides) {
		var rng = CreateQuickRng();
		return rng.RollDice(count, sides);
	}

	#endregion

	#region D20 Rolls

	/// <summary>Rolls a d20.</summary>
	public static int RollD20() => RollDie(20);

	/// <summary>Rolls a d20 with modifier.</summary>
	public static int RollD20(int modifier) => RollDie(20) + modifier;

	/// <summary>Performs a d20 check.</summary>
	public static D20Result RollD20Check(int modifier, int targetDC) {
		var rng = CreateQuickRng();
		return rng.RollD20(modifier, targetDC);
	}

	/// <summary>Performs a d20 check with advantage.</summary>
	public static D20Result RollD20CheckAdvantage(int modifier, int targetDC) {
		var rng = CreateQuickRng();
		return rng.RollD20Advantage(modifier, targetDC);
	}

	/// <summary>Performs a d20 check with disadvantage.</summary>
	public static D20Result RollD20CheckDisadvantage(int modifier, int targetDC) {
		var rng = CreateQuickRng();
		return rng.RollD20Disadvantage(modifier, targetDC);
	}

	#endregion

	#region Probability Checks

	/// <summary>Performs a probability check.</summary>
	public static bool Check(float probability) {
		var rng = CreateQuickRng();
		return rng.Check(probability);
	}

	/// <summary>Performs a ChanceRPG check.</summary>
	public static bool Check(ChanceRPG chance) => Check(chance.Probability);

	#endregion

	#region Probability Calculations

	/// <summary>Calculates the chance to succeed on a d20 check.</summary>
	public static ChanceRPG GetD20SuccessChance(int modifier, int targetDC) {
		return HitDice.D20.AddBonus(modifier).GetChanceToMeet(targetDC);
	}

	/// <summary>Calculates success chance with advantage.</summary>
	public static ChanceRPG GetD20SuccessChanceAdvantage(int modifier, int targetDC) {
		return GetD20SuccessChance(modifier, targetDC).WithAdvantage();
	}

	/// <summary>Calculates success chance with disadvantage.</summary>
	public static ChanceRPG GetD20SuccessChanceDisadvantage(int modifier, int targetDC) {
		return GetD20SuccessChance(modifier, targetDC).WithDisadvantage();
	}

	#endregion

	#region Utility

	/// <summary>Selects a random index based on weights.</summary>
	public static int SelectWeightedIndex(float[] weights) {
		var rng = CreateQuickRng();
		return rng.PickWeightedIndex(weights);
	}

	private static RandomStream CreateQuickRng() {
		uint seed = (uint)(DateTime.UtcNow.Ticks ^ Environment.TickCount);
		return new RandomStream(seed == 0 ? 1 : seed);
	}

	#endregion
}
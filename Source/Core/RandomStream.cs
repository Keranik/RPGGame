using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

namespace RPGGame.Core;

/// <summary>
/// A deterministic random stream wrapping Unity.Mathematics.Random.
/// 
/// <para>
/// Provides RPG-focused methods for dice rolling, probability checks, and selection.
/// Tracks consumption count for debugging determinism issues.
/// </para>
/// 
/// <para>
/// <b>Important:</b> This is a mutable struct. Pass by ref to preserve state,
/// or use <see cref="Fork(int)"/> to create independent child streams.
/// </para>
/// </summary>
public struct RandomStream {
	#region Fields

	private Random _rng;
	private readonly uint _seed;
	private int _consumed;

	#endregion

	#region Properties

	/// <summary>The seed used to create this stream.</summary>
	public readonly uint Seed => _seed;

	/// <summary>Number of random values consumed.</summary>
	public readonly int Consumed => _consumed;

	#endregion

	#region Constructor

	/// <summary>
	/// Creates a new random stream with the specified seed.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public RandomStream(uint seed) {
		_seed = seed == 0 ? 1u : seed;
		_rng = new Random(_seed);
		_consumed = 0;
	}

	#endregion

	#region State Management

	/// <summary>
	/// Resets the stream to its initial state.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Reset() {
		_rng = new Random(_seed);
		_consumed = 0;
	}

	/// <summary>
	/// Creates an independent child stream with an integer salt.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly RandomStream Fork(int salt) {
		return new RandomStream(GameRandom.HashCombine(_seed, (uint)salt));
	}

	/// <summary>
	/// Creates an independent child stream with a string identifier.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly RandomStream Fork(string identifier) {
		return new RandomStream(GameRandom.HashCombine(_seed, GameRandom.HashString(identifier)));
	}

	/// <summary>
	/// Creates an independent child stream for coordinates.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly RandomStream Fork(int x, int y) {
		uint hash = GameRandom.HashCombine(_seed, (uint)x);
		hash = GameRandom.HashCombine(hash, (uint)y);
		return new RandomStream(hash);
	}

	#endregion

	#region Core Random

	/// <summary>Returns a random float in [0, 1).</summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float NextFloat() {
		_consumed++;
		return _rng.NextFloat();
	}

	/// <summary>Returns a random float in [min, max).</summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float NextFloat(float min, float max) {
		_consumed++;
		return _rng.NextFloat(min, max);
	}

	/// <summary>Returns a random double in [0, 1).</summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public double NextDouble() {
		_consumed++;
		return _rng.NextDouble();
	}

	/// <summary>Returns a random int in [0, int.MaxValue).</summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int NextInt() {
		_consumed++;
		return _rng.NextInt();
	}

	/// <summary>Returns a random int in [0, max).</summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int NextInt(int max) {
		_consumed++;
		return _rng.NextInt(max);
	}

	/// <summary>Returns a random int in [min, max).</summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int NextInt(int min, int max) {
		_consumed++;
		return _rng.NextInt(min, max);
	}

	/// <summary>Returns a random uint.</summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public uint NextUInt() {
		_consumed++;
		return _rng.NextUInt();
	}

	/// <summary>Returns a random bool (50/50).</summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool NextBool() {
		_consumed++;
		return _rng.NextBool();
	}

	/// <summary>Returns a random float2 in [0,1)².</summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float2 NextFloat2() {
		_consumed += 2;
		return _rng.NextFloat2();
	}

	/// <summary>Returns a random float3 in [0,1)³.</summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float3 NextFloat3() {
		_consumed += 3;
		return _rng.NextFloat3();
	}

	/// <summary>Returns a random angle in radians [0, 2π).</summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float NextAngle() => NextFloat() * math.PI * 2f;

	/// <summary>Returns a random point inside a unit circle.</summary>
	public float2 NextInsideUnitCircle() {
		float angle = NextAngle();
		float radius = math.sqrt(NextFloat());
		return new float2(math.cos(angle) * radius, math.sin(angle) * radius);
	}

	/// <summary>Returns a random direction on a unit circle.</summary>
	public float2 NextDirection2D() {
		float angle = NextAngle();
		return new float2(math.cos(angle), math.sin(angle));
	}

	#endregion

	#region Angles

	/// <summary>
	/// Returns a random AngleRPG between 0 and 360 degrees.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public AngleRPG NextAngleRPG() => new(NextFloat() * 360f);

	/// <summary>
	/// Returns a random AngleRPG within a range.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public AngleRPG NextAngleRPG(float minDegrees, float maxDegrees) => 
		new(NextFloat(minDegrees, maxDegrees));

	/// <summary>
	/// Returns a random AngleRPG within a range.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public AngleRPG NextAngleRPG(AngleRPG min, AngleRPG max) => 
		new(NextFloat(min.Degrees, max.Degrees));

	/// <summary>
	/// Adds random variance to an angle.
	/// </summary>
	/// <param name="baseAngle">The base angle.</param>
	/// <param name="variance">Maximum variance in degrees (±).</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public AngleRPG WithVariance(AngleRPG baseAngle, float variance) => 
		new(baseAngle.Degrees + NextFloat(-variance, variance));

	#endregion

	#region Dice Rolling

	/// <summary>
	/// Rolls a single die with the specified sides.
	/// </summary>
	/// <returns>Result in [1, sides].</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int RollDie(int sides) {
		if (sides <= 0) return 0;
		_consumed++;
		return _rng.NextInt(1, sides + 1);
	}

	/// <summary>
	/// Rolls multiple dice and returns the sum.
	/// </summary>
	public int RollDice(int count, int sides) {
		if (count <= 0 || sides <= 0) return 0;

		int total = 0;
		for (int i = 0; i < count; i++) {
			_consumed++;
			total += _rng.NextInt(1, sides + 1);
		}
		return total;
	}

	/// <summary>
	/// Rolls dice according to a HitDice specification.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int Roll(HitDice dice) {
		if (!dice.IsValid) return dice.Bonus;
		return RollDice(dice.NumberOfDice, dice.SidesOfDice) + dice.Bonus;
	}

	/// <summary>
	/// Rolls dice with advantage (roll twice, take higher).
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int RollAdvantage(HitDice dice) {
		int a = Roll(dice);
		int b = Roll(dice);
		return math.max(a, b);
	}

	/// <summary>
	/// Rolls dice with disadvantage (roll twice, take lower).
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int RollDisadvantage(HitDice dice) {
		int a = Roll(dice);
		int b = Roll(dice);
		return math.min(a, b);
	}

	/// <summary>
	/// Rolls dice with super advantage (three times, take highest).
	/// </summary>
	public int RollSuperAdvantage(HitDice dice) {
		int a = Roll(dice);
		int b = Roll(dice);
		int c = Roll(dice);
		return math.max(a, math.max(b, c));
	}

	/// <summary>
	/// Rolls 4d6 drop lowest (D&D stat generation).
	/// </summary>
	public int Roll4d6DropLowest() {
		int a = RollDie(6);
		int b = RollDie(6);
		int c = RollDie(6);
		int d = RollDie(6);
		return a + b + c + d - math.min(math.min(a, b), math.min(c, d));
	}

	#endregion

	#region D20 Checks

	/// <summary>
	/// Performs a d20 check against a target DC.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool D20Check(int modifier, int targetDC) {
		int natural = RollDie(20);
		if (natural == 20) return true;
		if (natural == 1) return false;
		return natural + modifier >= targetDC;
	}

	/// <summary>
	/// Performs a d20 check and returns detailed results.
	/// </summary>
	public D20Result RollD20(int modifier, int targetDC) {
		int natural = RollDie(20);
		int total = natural + modifier;
		bool success = natural == 20 || (natural != 1 && total >= targetDC);

		return new D20Result {
			NaturalRoll = natural,
			Modifier = modifier,
			Total = total,
			TargetDC = targetDC,
			Success = success,
			IsCritical = natural == 20,
			IsFumble = natural == 1
		};
	}

	/// <summary>
	/// Performs a d20 check with advantage.
	/// </summary>
	public D20Result RollD20Advantage(int modifier, int targetDC) {
		int roll1 = RollDie(20);
		int roll2 = RollDie(20);
		int natural = math.max(roll1, roll2);
		int total = natural + modifier;
		bool success = natural == 20 || (natural != 1 && total >= targetDC);

		return new D20Result {
			NaturalRoll = natural,
			Modifier = modifier,
			Total = total,
			TargetDC = targetDC,
			Success = success,
			IsCritical = natural == 20,
			IsFumble = natural == 1,
			HadAdvantage = true,
			OtherRoll = math.min(roll1, roll2)
		};
	}

	/// <summary>
	/// Performs a d20 check with disadvantage.
	/// </summary>
	public D20Result RollD20Disadvantage(int modifier, int targetDC) {
		int roll1 = RollDie(20);
		int roll2 = RollDie(20);
		int natural = math.min(roll1, roll2);
		int total = natural + modifier;
		bool success = natural == 20 || (natural != 1 && total >= targetDC);

		return new D20Result {
			NaturalRoll = natural,
			Modifier = modifier,
			Total = total,
			TargetDC = targetDC,
			Success = success,
			IsCritical = natural == 20,
			IsFumble = natural == 1,
			HadDisadvantage = true,
			OtherRoll = math.max(roll1, roll2)
		};
	}

	#endregion

	#region Probability Checks

	/// <summary>
	/// Performs a probability check.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Check(float probability) {
		if (probability <= 0f) return false;
		if (probability >= 1f) return true;
		return NextFloat() < probability;
	}

	/// <summary>
	/// Performs a ChanceRPG check.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Check(ChanceRPG chance) => Check(chance.Probability);

	/// <summary>
	/// Performs a Percent check.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Check(Percent percent) => Check(percent.Fraction);

	/// <summary>
	/// Performs multiple checks and returns success count.
	/// </summary>
	public int CheckCount(float probability, int attempts) {
		if (attempts <= 0) return 0;
		if (probability <= 0f) return 0;
		if (probability >= 1f) return attempts;

		int successes = 0;
		for (int i = 0; i < attempts; i++) {
			if (Check(probability)) successes++;
		}
		return successes;
	}

	#endregion

	#region Selection

	/// <summary>
	/// Picks a random element from an array.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T Pick<T>(T[] array) {
		if (array == null || array.Length == 0)
			throw new ArgumentException("Array cannot be null or empty", nameof(array));
		return array[NextInt(array.Length)];
	}

	/// <summary>
	/// Picks a random element from a list.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T Pick<T>(IReadOnlyList<T> list) {
		if (list == null || list.Count == 0)
			throw new ArgumentException("List cannot be null or empty", nameof(list));
		return list[NextInt(list.Count)];
	}

	/// <summary>
	/// Tries to pick a random element. Returns false if empty.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryPick<T>(IReadOnlyList<T>? list, out T result) {
		if (list == null || list.Count == 0) {
			result = default!;
			return false;
		}
		result = list[NextInt(list.Count)];
		return true;
	}

	/// <summary>
	/// Picks a random element or returns default if empty.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T? PickOrDefault<T>(IReadOnlyList<T>? list) {
		if (list == null || list.Count == 0) return default;
		return list[NextInt(list.Count)];
	}

	/// <summary>
	/// Selects a random index based on weights.
	/// </summary>
	public int PickWeightedIndex(IReadOnlyList<float> weights) {
		if (weights == null || weights.Count == 0) return -1;

		float total = 0f;
		for (int i = 0; i < weights.Count; i++) total += weights[i];
		if (total <= 0f) return 0;

		float roll = NextFloat() * total;
		float cumulative = 0f;

		for (int i = 0; i < weights.Count; i++) {
			cumulative += weights[i];
			if (roll < cumulative) return i;
		}
		return weights.Count - 1;
	}

	/// <summary>
	/// Picks a random element based on weights.
	/// </summary>
	public T PickWeighted<T>(IReadOnlyList<T> items, IReadOnlyList<float> weights) {
		if (items == null || items.Count == 0)
			throw new ArgumentException("Items cannot be null or empty", nameof(items));
		int index = PickWeightedIndex(weights);
		return items[math.max(0, index)];
	}

	/// <summary>
	/// Picks a random element based on a weight selector.
	/// </summary>
	public T PickWeighted<T>(IReadOnlyList<T> items, Func<T, float> weightSelector) {
		if (items == null || items.Count == 0)
			throw new ArgumentException("Items cannot be null or empty", nameof(items));

		float total = 0f;
		for (int i = 0; i < items.Count; i++) total += weightSelector(items[i]);
		if (total <= 0f) return items[0];

		float roll = NextFloat() * total;
		float cumulative = 0f;

		for (int i = 0; i < items.Count; i++) {
			cumulative += weightSelector(items[i]);
			if (roll < cumulative) return items[i];
		}
		return items[^1];
	}

	/// <summary>
	/// Shuffles an array in-place using Fisher-Yates.
	/// </summary>
	public void Shuffle<T>(T[] array) {
		if (array == null || array.Length <= 1) return;

		for (int i = array.Length - 1; i > 0; i--) {
			int j = NextInt(i + 1);
			(array[i], array[j]) = (array[j], array[i]);
		}
	}

	/// <summary>
	/// Shuffles a list in-place using Fisher-Yates.
	/// </summary>
	public void Shuffle<T>(IList<T> list) {
		if (list == null || list.Count <= 1) return;

		for (int i = list.Count - 1; i > 0; i--) {
			int j = NextInt(i + 1);
			(list[i], list[j]) = (list[j], list[i]);
		}
	}

	/// <summary>
	/// Returns N unique random elements from a list.
	/// </summary>
	public List<T> PickMultiple<T>(IReadOnlyList<T> source, int count) {
		if (source == null || source.Count == 0 || count <= 0)
			return new List<T>();

		count = math.min(count, source.Count);
		var result = new List<T>(count);

		if (count == source.Count) {
			result.AddRange(source);
			Shuffle(result);
			return result;
		}

		if (count <= source.Count / 4) {
			var picked = new HashSet<int>();
			while (result.Count < count) {
				int index = NextInt(source.Count);
				if (picked.Add(index)) result.Add(source[index]);
			}
		} else {
			var indices = new int[source.Count];
			for (int i = 0; i < indices.Length; i++) indices[i] = i;

			for (int i = 0; i < count; i++) {
				int j = NextInt(i, source.Count);
				(indices[i], indices[j]) = (indices[j], indices[i]);
				result.Add(source[indices[i]]);
			}
		}

		return result;
	}

	#endregion

	#region Utility

	/// <summary>
	/// Gets debug information about this stream.
	/// </summary>
	public readonly override string ToString() => $"RandomStream(seed={_seed}, consumed={_consumed})";

	#endregion
}

/// <summary>
/// Result of a d20 check with full details.
/// </summary>
public readonly struct D20Result {
	public int NaturalRoll { get; init; }
	public int Modifier { get; init; }
	public int Total { get; init; }
	public int TargetDC { get; init; }
	public bool Success { get; init; }
	public bool IsCritical { get; init; }
	public bool IsFumble { get; init; }
	public bool HadAdvantage { get; init; }
	public bool HadDisadvantage { get; init; }
	public int OtherRoll { get; init; }

	public int Margin => Total - TargetDC;
	public bool IsExceptionalSuccess => Success && Margin >= 10;
	public bool IsExceptionalFailure => !Success && Margin <= -10;

	public override string ToString() {
		string adv = HadAdvantage ? " (adv)" : HadDisadvantage ? " (dis)" : "";
		string mod = Modifier >= 0 ? $"+{Modifier}" : Modifier.ToString();
		string result = IsCritical ? "CRIT!" : IsFumble ? "FUMBLE!" : Success ? "Hit" : "Miss";
		return $"d20{adv}: {NaturalRoll}{mod}={Total} vs DC{TargetDC} → {result}";
	}
}
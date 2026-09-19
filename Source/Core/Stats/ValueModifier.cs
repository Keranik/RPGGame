namespace RPGGame.Core.Stats;

/// <summary>
/// A self-describing value modification that clearly defines how it applies to stats.
/// Supports flat values, additive percentages (increased/reduced), and multiplicative percentages (more/less).
/// </summary>
public readonly struct ValueModifier : IEquatable<ValueModifier> {
	#region Properties

	/// <summary>The numeric value of the modification.</summary>
	public float Value { get; }

	/// <summary>How this modifier is applied.</summary>
	public ModifierOperation Operation { get; }

	/// <summary>Whether this is a positive or negative modifier.</summary>
	public bool IsPositive => Operation switch {
		ModifierOperation.FlatAdd => Value >= 0,
		ModifierOperation.FlatSubtract => false,
		ModifierOperation.PercentIncrease => true,
		ModifierOperation.PercentReduce => false,
		ModifierOperation.PercentMore => true,
		ModifierOperation.PercentLess => false,
		_ => Value >= 0
	};

	/// <summary>Whether this stacks additively with similar modifiers.</summary>
	public bool IsAdditive => Operation switch {
		ModifierOperation.FlatAdd => true,
		ModifierOperation.FlatSubtract => true,
		ModifierOperation.PercentIncrease => true,
		ModifierOperation.PercentReduce => true,
		ModifierOperation.PercentMore => false,
		ModifierOperation.PercentLess => false,
		_ => true
	};

	/// <summary>Whether this stacks multiplicatively.</summary>
	public bool IsMultiplicative => !IsAdditive;

	/// <summary>Whether this is a percentage-based modifier.</summary>
	public bool IsPercent => Operation switch {
		ModifierOperation.PercentIncrease => true,
		ModifierOperation.PercentReduce => true,
		ModifierOperation.PercentMore => true,
		ModifierOperation.PercentLess => true,
		_ => false
	};

	#endregion

	#region Constructor

	private ValueModifier(float value, ModifierOperation operation) {
		Value = Math.Abs(value); // Always store positive, operation determines sign
		Operation = operation;
	}

	#endregion

	#region Factory Methods

	/// <summary>Creates a flat additive bonus (e.g., +5 to stat).</summary>
	public static ValueModifier FlatAdd(float value) => new(value, ModifierOperation.FlatAdd);

	/// <summary>Creates a flat subtractive penalty (e.g., -5 to stat).</summary>
	public static ValueModifier FlatSubtract(float value) => new(value, ModifierOperation.FlatSubtract);

	/// <summary>Creates an additive percentage increase (stacks additively with other increases).</summary>
	public static ValueModifier PercentIncrease(float percent) => new(percent, ModifierOperation.PercentIncrease);

	/// <summary>Creates an additive percentage reduction (stacks additively with other reductions).</summary>
	public static ValueModifier PercentReduce(float percent) => new(percent, ModifierOperation.PercentReduce);

	/// <summary>Creates a multiplicative percentage increase (stacks multiplicatively).</summary>
	public static ValueModifier PercentMore(float percent) => new(percent, ModifierOperation.PercentMore);

	/// <summary>Creates a multiplicative percentage decrease (stacks multiplicatively).</summary>
	public static ValueModifier PercentLess(float percent) => new(percent, ModifierOperation.PercentLess);

	/// <summary>No modification.</summary>
	public static ValueModifier None => new(0, ModifierOperation.FlatAdd);

	public static ValueModifier Create(ModifierOperation operation, float value) {
		return operation switch {
			ModifierOperation.FlatAdd => FlatAdd(value),
			ModifierOperation.FlatSubtract => FlatSubtract(value),
			ModifierOperation.PercentIncrease => PercentIncrease(value),
			ModifierOperation.PercentReduce => PercentReduce(value),
			ModifierOperation.PercentMore => PercentMore(value),
			ModifierOperation.PercentLess => PercentLess(value),
			_ => None
		};
	}

	#endregion

	#region Application

	/// <summary>
	/// Gets the effective multiplier for this modifier.
	/// For multiplicative operations, this is the factor to multiply by.
	/// </summary>
	public float GetMultiplier() {
		return Operation switch {
			ModifierOperation.PercentMore => 1f + (Value / 100f),
			ModifierOperation.PercentLess => 1f - (Value / 100f),
			_ => 1f
		};
	}

	/// <summary>
	/// Gets the additive value for this modifier.
	/// For flat operations, returns the value. For additive %, returns the percentage.
	/// </summary>
	public float GetAdditiveValue() {
		return Operation switch {
			ModifierOperation.FlatAdd => Value,
			ModifierOperation.FlatSubtract => -Value,
			ModifierOperation.PercentIncrease => Value,
			ModifierOperation.PercentReduce => -Value,
			_ => 0f
		};
	}

	#endregion

	#region Display

	/// <summary>Gets a human-readable display string.</summary>
	public override string ToString() {
		return Operation switch {
			ModifierOperation.FlatAdd => $"+{Value:F0}",
			ModifierOperation.FlatSubtract => $"-{Value:F0}",
			ModifierOperation.PercentIncrease => $"+{Value:F0}% increased",
			ModifierOperation.PercentReduce => $"-{Value:F0}% reduced",
			ModifierOperation.PercentMore => $"+{Value:F0}% more",
			ModifierOperation.PercentLess => $"-{Value:F0}% less",
			_ => Value.ToString("F0")
		};
	}

	/// <summary>Gets a short display string for UI.</summary>
	public string ToShortString() {
		return Operation switch {
			ModifierOperation.FlatAdd => $"+{Value:F0}",
			ModifierOperation.FlatSubtract => $"-{Value:F0}",
			ModifierOperation.PercentIncrease => $"+{Value:F0}%",
			ModifierOperation.PercentReduce => $"-{Value:F0}%",
			ModifierOperation.PercentMore => $"×{1 + Value / 100:F2}",
			ModifierOperation.PercentLess => $"×{1 - Value / 100:F2}",
			_ => Value.ToString("F0")
		};
	}

	/// <summary>Gets a color-coded display string (green = good, red = bad).</summary>
	public string ToColoredString(bool higherIsBetter = true) {
		bool isGood = higherIsBetter ? IsPositive : !IsPositive;
		string color = isGood ? "green" : "red";
		return $"<color={color}>{this}</color>";
	}

	#endregion

	#region Equality

	public bool Equals(ValueModifier other) {
		return Math.Abs(Value - other.Value) < 0.0001f && Operation == other.Operation;
	}

	public override bool Equals(object? obj) => obj is ValueModifier other && Equals(other);

	public override int GetHashCode() => HashCode.Combine(Value, Operation);

	public static bool operator ==(ValueModifier left, ValueModifier right) => left.Equals(right);
	public static bool operator !=(ValueModifier left, ValueModifier right) => !left.Equals(right);

	#endregion
}

/// <summary>
/// How a value modifier is applied to a stat.
/// </summary>
public enum ModifierOperation {
	/// <summary>Add a flat value to the stat.</summary>
	FlatAdd,

	/// <summary>Subtract a flat value from the stat.</summary>
	FlatSubtract,

	/// <summary>Additive percentage increase. Stacks additively with other increases.</summary>
	PercentIncrease,

	/// <summary>Additive percentage reduction. Stacks additively with other reductions.</summary>
	PercentReduce,

	/// <summary>Multiplicative percentage increase. Stacks multiplicatively (compounding).</summary>
	PercentMore,

	/// <summary>Multiplicative percentage decrease. Stacks multiplicatively (compounding).</summary>
	PercentLess
}

/// <summary>
/// Extension methods for fluent ValueModifier creation.
/// Allows syntax like: 10.Flat(), 15.PercentMore(), 5.PercentLess()
/// </summary>
public static class ValueModifierExtensions {
	// ???????????????????????????????????????????????????????????????
	// INT EXTENSIONS
	// ???????????????????????????????????????????????????????????????

	/// <summary>Creates a flat additive bonus. Usage: 5.Flat()</summary>
	public static ValueModifier Flat(this int value) => ValueModifier.FlatAdd(value);

	/// <summary>Creates a flat additive bonus. Usage: 5.FlatAdd()</summary>
	public static ValueModifier FlatAdd(this int value) => ValueModifier.FlatAdd(value);

	/// <summary>Creates a flat subtractive penalty. Usage: 5.FlatSubtract()</summary>
	public static ValueModifier FlatSubtract(this int value) => ValueModifier.FlatSubtract(value);

	/// <summary>Creates an additive % increase. Usage: 10.PercentIncrease()</summary>
	public static ValueModifier PercentIncrease(this int value) => ValueModifier.PercentIncrease(value);

	/// <summary>Creates an additive % reduction. Usage: 10.PercentReduce()</summary>
	public static ValueModifier PercentReduce(this int value) => ValueModifier.PercentReduce(value);

	/// <summary>Creates a multiplicative % increase. Usage: 10.PercentMore()</summary>
	public static ValueModifier PercentMore(this int value) => ValueModifier.PercentMore(value);

	/// <summary>Creates a multiplicative % decrease. Usage: 10.PercentLess()</summary>
	public static ValueModifier PercentLess(this int value) => ValueModifier.PercentLess(value);

	// ???????????????????????????????????????????????????????????????
	// FLOAT EXTENSIONS
	// ???????????????????????????????????????????????????????????????

	/// <summary>Creates a flat additive bonus. Usage: 5.5f.Flat()</summary>
	public static ValueModifier Flat(this float value) => ValueModifier.FlatAdd(value);

	/// <summary>Creates a flat additive bonus. Usage: 5.5f.FlatAdd()</summary>
	public static ValueModifier FlatAdd(this float value) => ValueModifier.FlatAdd(value);

	/// <summary>Creates a flat subtractive penalty. Usage: 5.5f.FlatSubtract()</summary>
	public static ValueModifier FlatSubtract(this float value) => ValueModifier.FlatSubtract(value);

	/// <summary>Creates an additive % increase. Usage: 10.5f.PercentIncrease()</summary>
	public static ValueModifier PercentIncrease(this float value) => ValueModifier.PercentIncrease(value);

	/// <summary>Creates an additive % reduction. Usage: 10.5f.PercentReduce()</summary>
	public static ValueModifier PercentReduce(this float value) => ValueModifier.PercentReduce(value);

	/// <summary>Creates a multiplicative % increase. Usage: 10.5f.PercentMore()</summary>
	public static ValueModifier PercentMore(this float value) => ValueModifier.PercentMore(value);

	/// <summary>Creates a multiplicative % decrease. Usage: 10.5f.PercentLess()</summary>
	public static ValueModifier PercentLess(this float value) => ValueModifier.PercentLess(value);
}
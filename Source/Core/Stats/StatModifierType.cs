namespace RPGGame.Core.Stats;

/// <summary>
/// Defines when a stat modifier expires.
/// </summary>
public enum StatModifierDuration {
	/// <summary>
	/// Lasts until end of combat or manually removed.
	/// </summary>
	Temporary,

	/// <summary>
	/// Lasts for the entire run (until death/victory).
	/// </summary>
	Permanent,

	/// <summary>
	/// Persists across runs (meta progression).
	/// </summary>
	Meta,

	/// <summary>
	/// Equipment-based, removed when unequipped.
	/// </summary>
	Equipment
}

/// <summary>
/// Extension methods for stat modifier duration.
/// </summary>
public static class StatModifierDurationExtensions {
	/// <summary>
	/// Gets a display name for the duration type.
	/// </summary>
	public static string GetDisplayName(this StatModifierDuration duration) {
		return duration switch {
			StatModifierDuration.Temporary => "Temporary",
			StatModifierDuration.Permanent => "Permanent",
			StatModifierDuration.Meta => "Meta Bonus",
			StatModifierDuration.Equipment => "Equipment",
			_ => duration.ToString()
		};
	}

	/// <summary>
	/// Returns the processing order for this duration type.
	/// Lower numbers are processed first.
	/// </summary>
	public static int GetDurationOrder(this StatModifierDuration duration) {
		return duration switch {
			StatModifierDuration.Meta => 0,      // Applied first (base bonuses)
			StatModifierDuration.Permanent => 1, // Then run bonuses
			StatModifierDuration.Temporary => 2, // Then temporary effects
			StatModifierDuration.Equipment => 1, // Same as permanent
			_ => 99
		};
	}

	/// <summary>
	/// Returns the color to use when displaying this duration type.
	/// </summary>
	public static string GetColorHex(this StatModifierDuration duration) {
		return duration switch {
			StatModifierDuration.Temporary => "#FFD700", // Gold
			StatModifierDuration.Permanent => "#00FF00", // Green
			StatModifierDuration.Meta => "#FF00FF",      // Magenta
			StatModifierDuration.Equipment => "#00BFFF", // Deep Sky Blue
			_ => "#FFFFFF"
		};
	}
}

/// <summary>
/// Extension methods for modifier operations.
/// </summary>
public static class ModifierOperationExtensions {
	/// <summary>
	/// Returns the processing order for this operation type.
	/// Lower numbers are processed first.
	/// </summary>
	public static int GetOperationOrder(this ModifierOperation operation) {
		return operation switch {
			ModifierOperation.FlatAdd => 0,
			ModifierOperation.FlatSubtract => 0,
			ModifierOperation.PercentIncrease => 1,
			ModifierOperation.PercentReduce => 1,
			ModifierOperation.PercentMore => 2,
			ModifierOperation.PercentLess => 2,
			_ => 99
		};
	}

	/// <summary>
	/// Gets a display string for the operation.
	/// </summary>
	public static string GetDisplayString(this ModifierOperation operation, float value) {
		return operation switch {
			ModifierOperation.FlatAdd => $"+{value:0}",
			ModifierOperation.FlatSubtract => $"-{value:0}",
			ModifierOperation.PercentIncrease => $"+{value:0}%",
			ModifierOperation.PercentReduce => $"-{value:0}%",
			ModifierOperation.PercentMore => $"{value:0}% more",
			ModifierOperation.PercentLess => $"{value:0}% less",
			_ => value.ToString("0")
		};
	}
}
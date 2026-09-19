namespace RPGGame.Core.Expedition;

/// <summary>
/// Player-selectable travel speed settings.
/// </summary>
public enum TravelSpeed {
	/// <summary>Travel is paused.</summary>
	Paused,
	/// <summary>Slow and careful - better perception, less fatigue.</summary>
	Slow,
	/// <summary>Normal travel pace.</summary>
	Normal,
	/// <summary>Fast travel - more fatigue, may miss things.</summary>
	Fast,
	/// <summary>Maximum speed - high fatigue, low perception.</summary>
	Sprint
}

/// <summary>
/// Extension methods for TravelSpeed.
/// </summary>
public static class TravelSpeedExtensions {
	/// <summary>
	/// Gets the game time multiplier for this speed.
	/// Higher = faster time progression.
	/// </summary>
	public static float GetTimeMultiplier(this TravelSpeed speed) {
		return speed switch {
			TravelSpeed.Paused => 0f,
			TravelSpeed.Slow => 0.5f,
			TravelSpeed.Normal => 1f,
			TravelSpeed.Fast => 2f,
			TravelSpeed.Sprint => 4f,
			_ => 1f
		};
	}

	/// <summary>
	/// Gets the movement speed multiplier.
	/// </summary>
	public static float GetMovementMultiplier(this TravelSpeed speed) {
		return speed switch {
			TravelSpeed.Paused => 0f,
			TravelSpeed.Slow => 0.5f,
			TravelSpeed.Normal => 1f,
			TravelSpeed.Fast => 1.5f,
			TravelSpeed.Sprint => 2f,
			_ => 1f
		};
	}

	/// <summary>
	/// Gets the fatigue accumulation multiplier.
	/// </summary>
	public static float GetFatigueMultiplier(this TravelSpeed speed) {
		return speed switch {
			TravelSpeed.Paused => 0f,
			TravelSpeed.Slow => 0.5f,
			TravelSpeed.Normal => 1f,
			TravelSpeed.Fast => 1.5f,
			TravelSpeed.Sprint => 3f,
			_ => 1f
		};
	}

	/// <summary>
	/// Gets the perception/awareness modifier.
	/// Higher = better chance to notice things.
	/// </summary>
	public static float GetPerceptionModifier(this TravelSpeed speed) {
		return speed switch {
			TravelSpeed.Paused => 2f,
			TravelSpeed.Slow => 1.5f,
			TravelSpeed.Normal => 1f,
			TravelSpeed.Fast => 0.7f,
			TravelSpeed.Sprint => 0.4f,
			_ => 1f
		};
	}

	/// <summary>
	/// Gets the ambush avoidance modifier.
	/// Lower = easier to be ambushed.
	/// </summary>
	public static float GetAmbushAvoidance(this TravelSpeed speed) {
		return speed switch {
			TravelSpeed.Paused => 1f, // Can't be ambushed while stopped
			TravelSpeed.Slow => 0.8f,
			TravelSpeed.Normal => 0.5f,
			TravelSpeed.Fast => 0.3f,
			TravelSpeed.Sprint => 0.1f,
			_ => 0.5f
		};
	}

	/// <summary>
	/// Gets the display name for this speed.
	/// </summary>
	public static string GetDisplayName(this TravelSpeed speed) {
		return speed switch {
			TravelSpeed.Paused => "⏸ Paused",
			TravelSpeed.Slow => "🐢 Slow",
			TravelSpeed.Normal => "🚶 Normal",
			TravelSpeed.Fast => "🏃 Fast",
			TravelSpeed.Sprint => "💨 Sprint",
			_ => speed.ToString()
		};
	}

	/// <summary>
	/// Gets a short icon for UI.
	/// </summary>
	public static string GetIcon(this TravelSpeed speed) {
		return speed switch {
			TravelSpeed.Paused => "⏸",
			TravelSpeed.Slow => "▶",
			TravelSpeed.Normal => "▶▶",
			TravelSpeed.Fast => "▶▶▶",
			TravelSpeed.Sprint => "⏩",
			_ => "?"
		};
	}
}
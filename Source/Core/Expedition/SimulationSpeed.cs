namespace RPGGame.Core.Expedition;

/// <summary>
/// Game simulation speed settings.
/// Controls how fast game time passes relative to real time.
/// </summary>
public enum SimulationSpeed {
	/// <summary>Time is paused.</summary>
	Paused = 0,

	/// <summary>Slow speed (0.5x).</summary>
	Slow = 1,

	/// <summary>Normal speed (1x).</summary>
	Normal = 2,

	/// <summary>Fast speed (2x).</summary>
	Fast = 3,

	/// <summary>Very fast speed (4x).</summary>
	VeryFast = 4
}

public static class SimulationSpeedExtensions {
	/// <summary>
	/// Gets the time multiplier for this speed.
	/// </summary>
	public static float GetMultiplier(this SimulationSpeed speed) {
		return speed switch {
			SimulationSpeed.Paused => 0f,
			SimulationSpeed.Slow => 0.5f,
			SimulationSpeed.Normal => 1f,
			SimulationSpeed.Fast => 2f,
			SimulationSpeed.VeryFast => 4f,
			_ => 1f
		};
	}

	/// <summary>
	/// Gets the display name for this speed.
	/// </summary>
	public static string GetDisplayName(this SimulationSpeed speed) {
		return speed switch {
			SimulationSpeed.Paused => "Paused",
			SimulationSpeed.Slow => "0.5x",
			SimulationSpeed.Normal => "1x",
			SimulationSpeed.Fast => "2x",
			SimulationSpeed.VeryFast => "4x",
			_ => "1x"
		};
	}
}

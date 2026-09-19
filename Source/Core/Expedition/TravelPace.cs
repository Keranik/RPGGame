namespace RPGGame.Core.Expedition;

/// <summary>
/// Character travel pace - affects stamina, noise, and perception.
/// Separate from SimulationSpeed which controls game time.
/// </summary>
public enum TravelPace {
	/// <summary>Stationary - not moving but time passes. Can set up camp.</summary>
	Wait,
	/// <summary>Slow and stealthy movement.</summary>
	Sneak,
	/// <summary>Normal walking pace.</summary>
	Walk,
	/// <summary>Light jogging - faster but more noise.</summary>
	Jog,
	/// <summary>Running - fast travel, high stamina cost.</summary>
	Run,
	/// <summary>Full sprint - maximum speed, very high stamina cost.</summary>
	Sprint
}

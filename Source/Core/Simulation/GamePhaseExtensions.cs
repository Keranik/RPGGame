namespace RPGGame.Core.Simulation;

public static partial class GamePhaseExtensions {
	/// <summary>
	/// Returns true if the game loop should be running during this phase.
	/// </summary>
	public static bool IsGameLoopActive(this GamePhase phase) {
		return phase switch {
			GamePhase.Expedition => true,
			GamePhase.Camp => true, // Time passes while camping
			_ => false
		};
	}

	/// <summary>
	/// Returns true if this phase allows saving.
	/// </summary>
	public static bool CanSave(this GamePhase phase) {
		return phase switch {
			GamePhase.Village => true,
			GamePhase.Camp => true,
			GamePhase.Paused => true,
			_ => false
		};
	}

	/// <summary>
	/// Returns true if this phase is considered "in a run".
	/// </summary>
	public static bool IsInRun(this GamePhase phase) {
		return phase switch {
			GamePhase.Expedition => true,
			GamePhase.Event => true,
			GamePhase.Combat => true,
			GamePhase.Camp => true,
			GamePhase.LevelUp => true,
			GamePhase.Paused => true, // Still in run, just paused
			_ => false
		};
	}

	/// <summary>
	/// Returns true if this is a gameplay phase (not menu/loading).
	/// </summary>
	public static bool IsGameplayPhase(this GamePhase phase) {
		return phase switch {
			GamePhase.Village => true,
			GamePhase.Departure => true,
			GamePhase.Expedition => true,
			GamePhase.Event => true,
			GamePhase.Combat => true,
			GamePhase.Camp => true,
			GamePhase.LevelUp => true,
			_ => false
		};
	}

	/// <summary>
	/// Returns true if this phase shows the tilemap.
	/// </summary>
	public static bool ShowsTilemap(this GamePhase phase) {
		return phase switch {
			GamePhase.Village => true,
			GamePhase.Departure => true,
			GamePhase.Expedition => true,
			GamePhase.Event => true, // Tilemap visible behind event UI
			GamePhase.Combat => true, // Tilemap visible behind combat UI
			GamePhase.Camp => true,
			_ => false
		};
	}

	/// <summary>
	/// Returns the UI screen name associated with this phase.
	/// </summary>
	public static string GetScreenName(this GamePhase phase) {
		return phase switch {
			GamePhase.MainMenu => "mainMenu",
			GamePhase.Village => "village",
			GamePhase.Departure => "departure",
			GamePhase.Expedition => "expedition",
			GamePhase.Event => "event",
			GamePhase.Combat => "combat",
			GamePhase.Camp => "camp",
			GamePhase.LevelUp => "levelUp",
			GamePhase.Paused => "pause",
			GamePhase.TimeRewind => "timeRewind",
			GamePhase.Victory => "victory",
			GamePhase.TrueEnding => "trueEnding",
			GamePhase.Credits => "credits",
			_ => "unknown"
		};
	}
}
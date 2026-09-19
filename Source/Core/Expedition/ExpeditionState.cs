namespace RPGGame.Core.Expedition;

/// <summary>
/// The current state of expedition travel.
/// Controls the flow of exploration, events, and combat.
/// </summary>
public enum ExpeditionState {
	/// <summary>Not on expedition (in village).</summary>
	None,

	/// <summary>Just departed from village, intro sequence.</summary>
	Departing,

	/// <summary>Actively traveling along the path.</summary>
	Traveling,

	/// <summary>Travel paused by player or system.</summary>
	Paused,

	/// <summary>Stopped at a planned stop (queue stop).</summary>
	Stopped,

	/// <summary>An event has triggered, awaiting player choice.</summary>
	InEvent,

	/// <summary>In active combat encounter.</summary>
	InCombat,

	/// <summary>Setting up camp.</summary>
	SettingUpCamp,

	/// <summary>At camp, can rest/manage.</summary>
	AtCamp,

	/// <summary>Dismantling camp to resume travel.</summary>
	DismantlingCamp,

	/// <summary>Entering a local map (dungeon/POI).</summary>
	EnteringLocation,

	/// <summary>Exploring a local map (turn-based movement).</summary>
	InLocalMap,

	/// <summary>Exiting a local map.</summary>
	ExitingLocation,

	/// <summary>Run ended - player died.</summary>
	Dead,

	/// <summary>Run ended - reached goal.</summary>
	Victory
}

/// <summary>
/// Reasons why travel might be paused.
/// </summary>
[Flags]
public enum PauseReason {
	None = 0,
	PlayerPaused = 1 << 0,
	EventTriggered = 1 << 1,
	CombatStarted = 1 << 2,
	ReachedQueueStop = 1 << 3,
	StaminaDepleted = 1 << 4,
	NightFall = 1 << 5,
	StormWarning = 1 << 6,
	LowHealth = 1 << 7,
	LowSupplies = 1 << 8,
	PathBlocked = 1 << 9,
	ReachedDestination = 1 << 10
}
namespace RPGGame.Core.Simulation;

/// <summary>
/// Represents the high-level phase of the game.
/// Controls which systems are active and which UI screens are shown.
/// </summary>
public enum GamePhase {
	/// <summary>
	/// Initial loading, splash screens, asset loading.
	/// No gameplay systems active.
	/// </summary>
	Initializing,

	/// <summary>
	/// Loading a saved game or assets.
	/// Shows loading screen.
	/// </summary>
	Loading,

	/// <summary>
	/// Main menu screen - New Game, Load, Settings, Exit.
	/// No gameplay systems active.
	/// </summary>
	MainMenu,

	/// <summary>
	/// Character creation / new game setup.
	/// Player selects class, name, etc.
	/// </summary>
	NewGame,

	/// <summary>
	/// Player is in the village hub between expeditions.
	/// Can interact with buildings, manage inventory, prepare for departure.
	/// GameLoop is paused but village systems active.
	/// </summary>
	Village,

	/// <summary>
	/// Player is at the village gate, reviewing pre-departure checklist.
	/// Last chance to prepare before venturing into the fog.
	/// </summary>
	Departure,

	/// <summary>
	/// Active expedition - traveling through the fog.
	/// Auto-movement with events, resource management, exploration.
	/// GameLoop running at selected speed.
	/// </summary>
	Expedition,

	/// <summary>
	/// An event has triggered during expedition.
	/// Could be combat, encounter, choice, discovery, etc.
	/// GameLoop paused while player resolves event.
	/// </summary>
	Event,

	/// <summary>
	/// Active combat encounter.
	/// Turn-based battle with D&D mechanics.
	/// </summary>
	Combat,

	/// <summary>
	/// Player has set up camp to rest.
	/// Can level up, recover, manage inventory.
	/// Time passes while resting.
	/// </summary>
	Camp,

	/// <summary>
	/// Player is leveling up / using character editor.
	/// Allocate stats, choose abilities, etc.
	/// Only accessible at camp.
	/// </summary>
	LevelUp,

	/// <summary>
	/// Game is paused (in-game pause menu).
	/// Can access settings, save, return to main menu.
	/// </summary>
	Paused,

	/// <summary>
	/// Run has ended - showing summary screen.
	/// Displays stats, loot, experience gained.
	/// </summary>
	RunSummary,

	/// <summary>
	/// Player has been "caught in time" (death).
	/// Show run summary, meta-progress gained.
	/// Will return to village with new character.
	/// </summary>
	TimeRewind,

	/// <summary>
	/// Player has cleared the fog (victory).
	/// Show victory screen, high score, meta-progress.
	/// Can start new expedition or work toward true ending.
	/// </summary>
	Victory,

	/// <summary>
	/// Player has achieved the true ending.
	/// Final lore reveal, credits, new game plus unlocked.
	/// </summary>
	TrueEnding,

	/// <summary>
	/// Credits screen.
	/// </summary>
	Credits
}
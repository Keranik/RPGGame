using RPGGame.Core.Characters;
using RPGGame.Core.Save;
using RPGGame.Core.Stats;

namespace RPGGame.Core.Simulation;

/// <summary>
/// Manages the high-level game phase transitions and run state.
/// </summary>
[Dependency(registrationType: RegistrationType.Singleton)]
public class GameStateManager {
	#region Fields

	private readonly GameLoop k_gameLoop;
	private MetaProgression k_metaProgression;

	private GamePhase k_currentPhase = GamePhase.Initializing;
	private GamePhase k_previousPhase = GamePhase.Initializing;
	private RunState? k_runState;

	#endregion

	#region Properties

	/// <summary>Current high-level game phase.</summary>
	public GamePhase CurrentPhase => k_currentPhase;

	/// <summary>Previous game phase (for returning from pause/events).</summary>
	public GamePhase PreviousPhase => k_previousPhase;

	/// <summary>Current run state (null if not in a run).</summary>
	public RunState? RunState => k_runState;

	/// <summary>Meta progression data (persists across runs).</summary>
	public MetaProgression MetaProgression => k_metaProgression;

	/// <summary>Whether a run is currently active.</summary>
	public bool IsInRun => k_runState != null;

	#endregion

	#region Events

	/// <summary>Fired when the game phase changes.</summary>
	public event Action<GamePhase, GamePhase>? OnPhaseChanged;

	/// <summary>Fired when a new run starts.</summary>
	public event Action<RunState>? OnRunStarted;

	/// <summary>Fired when a run ends.</summary>
	public event Action? OnRunEnded;

	#endregion

	#region Constructor

	public GameStateManager(GameLoop gameLoop, MetaProgression metaProgression) {
		k_gameLoop = gameLoop;
		k_metaProgression = metaProgression;
		UnityEngine.Debug.Log("GameStateManager Initialized");
	}

	#endregion

	#region Phase Management

	/// <summary>
	/// Sets the current game phase.
	/// </summary>
	public void SetPhase(GamePhase newPhase) {
		if (k_currentPhase == newPhase) {
			return;
		}

		var oldPhase = k_currentPhase;
		k_previousPhase = oldPhase;
		k_currentPhase = newPhase;

		// Handle game loop state based on phase
		UpdateGameLoopForPhase(newPhase, oldPhase);

		OnPhaseChanged?.Invoke(oldPhase, newPhase);
		UnityEngine.Debug.Log($"Game phase changed: {oldPhase} -> {newPhase}");
	}

	private void UpdateGameLoopForPhase(GamePhase newPhase, GamePhase oldPhase) {
		switch (newPhase) {
			case GamePhase.Expedition:
				// Expedition needs the game loop running
				if (!k_gameLoop.IsActive) {
					k_gameLoop.Start();
					UnityEngine.Debug.Log("GameStateManager: Started game loop for expedition");
				} else {
					k_gameLoop.Resume();
					UnityEngine.Debug.Log("GameStateManager: Resumed game loop for expedition");
				}
				break;

			case GamePhase.Village:
				// Village can have the loop running but paused (for time-based activities)
				if (!k_gameLoop.IsActive) {
					k_gameLoop.Start();
				}
				k_gameLoop.Pause();
				break;

			case GamePhase.Event:
				// Events pause the simulation but keep it active
				k_gameLoop.Pause();
				break;

			case GamePhase.Combat:
				// Combat has its own turn-based loop, pause main loop
				k_gameLoop.Pause();
				break;

			case GamePhase.Camp:
				// Camp is similar to village - paused but can advance time
				k_gameLoop.Pause();
				break;

			case GamePhase.LevelUp:
			case GamePhase.Paused:
			case GamePhase.RunSummary:
				// These fully pause everything
				k_gameLoop.Pause();
				break;

			case GamePhase.MainMenu:
			case GamePhase.NewGame:
			case GamePhase.Loading:
			case GamePhase.Initializing:
				// These stop the loop entirely
				if (k_gameLoop.IsActive) {
					k_gameLoop.Stop();
				}
				break;

			default:
				// Default to paused
				k_gameLoop.Pause();
				break;
		}
	}

	/// <summary>
	/// Transition to the main menu phase.
	/// </summary>
	public void GoToMainMenu() {
		SetPhase(GamePhase.MainMenu);
	}

	/// <summary>
	/// Pause the game.
	/// </summary>
	public void PauseGame() {
		if (k_currentPhase == GamePhase.Expedition ||
			k_currentPhase == GamePhase.Village ||
			k_currentPhase == GamePhase.Camp) {
			SetPhase(GamePhase.Paused);
		}
	}

	/// <summary>
	/// Resume the game from pause.
	/// </summary>
	public void ResumeGame() {
		if (k_currentPhase != GamePhase.Paused) {
			return;
		}

		// Return to previous phase
		if (k_previousPhase != GamePhase.Paused && k_previousPhase != GamePhase.Initializing) {
			SetPhase(k_previousPhase);
		} else if (k_runState != null) {
			SetPhase(GamePhase.Village);
		}
	}

	/// <summary>
	/// Returns to the previous phase (useful after events).
	/// </summary>
	public void ReturnToPreviousPhase() {
		if (k_previousPhase != GamePhase.Initializing && k_previousPhase != k_currentPhase) {
			SetPhase(k_previousPhase);
		}
	}

	/// <summary>
	/// Exit the game.
	/// </summary>
	public void ExitGame() {
		k_gameLoop.Stop();

#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
		UnityEngine.Application.Quit();
#endif
	}

	#endregion

	#region Run Management

	/// <summary>
	/// Safely attempts to get the current run state.
	/// </summary>
	/// <param name="runState">The current run state if a run is active; otherwise null.</param>
	/// <returns>True if a run is active and runState is set; false if no run is active.</returns>
	/// <example>
	/// if (stateManager.TryGetRunState(out RunState run))
	/// {
	///     // Safe to use 'run'
	///     Debug.Log($"Current day: {run.CurrentDay}");
	/// }
	/// else
	/// {
	///     Debug.Log("No active run");
	/// }
	/// </example>
	public bool TryGetRunState(out RunState? runState)
	{
		if (k_runState != null)
		{
			runState = k_runState;
			return true;
		}

		runState = null;
		return false;
	}

	/// <summary>
	/// Initializes fresh meta progression for a new player.
	/// </summary>
	public void InitializeNewMeta() {
		k_metaProgression = new MetaProgression();
		UnityEngine.Debug.Log("Initialized new meta progression");
	}

	/// <summary>
	/// Starts a new run with the given run state.
	/// </summary>
	public void StartRun(RunState runState) {
		k_runState = runState;
		OnRunStarted?.Invoke(runState);
		UnityEngine.Debug.Log($"Run started: #{k_metaProgression.TotalRuns}");
	}

	/// <summary>
	/// Ends the current run.
	/// </summary>
	public void EndRun() {
		k_runState = null;
		OnRunEnded?.Invoke();
		UnityEngine.Debug.Log("Run ended");
	}

	/// <summary>
	/// Restores run state from a save.
	/// </summary>
	public void RestoreRunState(RunState runState) {
		k_runState = runState;
		UnityEngine.Debug.Log($"Run state restored: Day {GameTime.Instance.Day}");
	}

	/// <summary>
	/// Restores meta progression from a save.
	/// </summary>
	public void RestoreMetaProgression(MetaProgression meta) {
		k_metaProgression = meta;
	}

	#endregion
}
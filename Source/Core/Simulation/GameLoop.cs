#nullable disable
using RPGGame.Core.Isometric;
using RPGGame.Core.Isometric.Rendering;
using UnityEngine;

namespace RPGGame.Core.Simulation;

/// <summary>
/// Drives the fixed-timestep game simulation.
/// 
/// Separates:
/// - Render update (Unity frame rate)
/// - Simulation update (fixed timestep: 0.1s)
/// - Calendar time (GameTime) advancement (scaled, phase-gated)
/// </summary>
[Dependency(registrationType: RegistrationType.Singleton)]
public class GameLoop {
	#region Fields

	private GameSession k_gameSession;
	private readonly IsoCameraController k_cameraController;
	private readonly LightController k_lightController;

	// ═══════════════════════════════════════════════════════════════════════════
	// SIMULATION TICK CONFIGURATION
	// One simulation tick = 0.1 real seconds.
	// This drives simulation and turn-based mechanics.
	// ═══════════════════════════════════════════════════════════════════════════

	/// <summary>
	/// The duration of one simulation tick in game-time terms for turn-based systems.
	/// NOTE: Calendar time advancement is handled separately.
	/// </summary>
	private static readonly Duration TICK_DURATION = Duration.OneMinute;

	/// <summary>
	/// Real-time interval in seconds for one simulation tick.
	/// </summary>
	private const float TICK_INTERVAL_SECONDS = 0.1f;

	private float k_accumulatedTime = 0f;

	private readonly GameTime k_gameTime;
	private bool k_isPaused = false;
	private bool k_isActive = false;

	// ═══════════════════════════════════════════════════════════════════════════
	// CALENDAR TIME CONFIGURATION
	// 6 real minutes == 1 in-game hour
	// => 360 real seconds == 3600 in-game seconds
	// => 1 real second == 10 in-game seconds
	// => 1 real second == 100 game ticks (because TICKS_PER_SECOND=10)
	// ═══════════════════════════════════════════════════════════════════════════

	private const double CALENDAR_GAME_SECONDS_PER_REAL_SECOND = 10.0;
	private static readonly double CALENDAR_TICKS_PER_REAL_SECOND =
		CALENDAR_GAME_SECONDS_PER_REAL_SECOND * GameTime.TICKS_PER_SECOND;

	private double k_calendarTickAccumulator;

	#endregion

	#region Properties

	public GameSession CurrentSession {
		get => k_gameSession;
		set => k_gameSession = value;
	}

	public GameTime GameTime => k_gameTime;

	public IsoCameraController CameraController => k_cameraController;

	public bool IsPaused => k_isPaused;

	public bool IsActive => k_isActive;

	public TimestampRPG CurrentTime => TimestampRPG.Now;

	public static Duration TickDuration => TICK_DURATION;

	#endregion

	#region Events

	public event Action OnPreUpdate;
	public event Action OnUpdate;
	public event Action OnPostUpdate;
	public event Action<GameTime> OnSyncUpdate;
	public event Action OnRenderUpdate;
	public event Action<Duration> OnTick;

	#endregion

	#region Constructor

	public GameLoop(GameTime gameTime, IsoCameraController cameraController, LightController lightController) {
		k_gameTime = gameTime;
		k_cameraController = cameraController;
		k_lightController = lightController;
		Debug.Log("GameLoop Initialized");
	}

	#endregion

	#region Lifecycle Methods

	public void Start() {
		if (k_isActive) {
			return;
		}

		k_isActive = true;
		k_isPaused = false;
		k_accumulatedTime = 0f;
		k_calendarTickAccumulator = 0;

		if (!k_cameraController.IsInitialized) {
			k_cameraController.Initialize();
		}

		Debug.Log("Game loop started.");
	}

	public void Stop() {
		if (!k_isActive) {
			return;
		}

		k_isActive = false;
		k_isPaused = true;

		OnPreUpdate = null;
		OnUpdate = null;
		OnPostUpdate = null;
		OnSyncUpdate = null;
		OnTick = null;
		OnRenderUpdate = null;

		Debug.Log("Game loop stopped.");
	}

	public void SetGameSession(GameSession session) {
		k_gameSession = session;
	}

	public void Pause() {
		k_isPaused = true;
		Debug.Log("Game loop paused.");
	}

	public void Resume() {
		k_isPaused = false;
		Debug.Log("Game loop resumed.");
	}

	#endregion

	#region Update Methods

	public void Tick(float deltaTime) {
		if (!k_isActive || k_isPaused) {
			return;
		}

		k_accumulatedTime += deltaTime;

		while (k_accumulatedTime >= TICK_INTERVAL_SECONDS) {
			// Simulation tick events
			OnPreUpdate?.Invoke();
			OnUpdate?.Invoke();
			OnPostUpdate?.Invoke();

			// Calendar time advancement (phase gated)
			AdvanceCalendarTime(TICK_INTERVAL_SECONDS);

			SyncUpdate(k_gameTime);

			// Turn-based tick duration notification
			OnTick?.Invoke(TICK_DURATION);

			k_accumulatedTime -= TICK_INTERVAL_SECONDS;
		}
	}

	private void AdvanceCalendarTime(float realSeconds) {
		if (k_gameSession?.CurrentRun == null) {
			return;
		}

		GamePhase phase = k_gameSession.CurrentPhase;

		// Only phases where world time should pass.
		// (Combat/Event/dialog should generally not advance the calendar.)
		bool shouldAdvanceCalendar = phase is GamePhase.Expedition or GamePhase.Camp;
		if (!shouldAdvanceCalendar) {
			return;
		}

		double ticksToAdd = realSeconds * CALENDAR_TICKS_PER_REAL_SECOND;
		k_calendarTickAccumulator += ticksToAdd;

		long wholeTicks = (long)k_calendarTickAccumulator;
		if (wholeTicks <= 0) {
			return;
		}

		k_calendarTickAccumulator -= wholeTicks;
		k_gameTime.AdvanceTicks(wholeTicks);
	}

	public void SyncUpdate(GameTime gameTime) {
		OnSyncUpdate?.Invoke(gameTime);
	}

	public void RenderUpdate() {
		// Render-driven camera smoothing
		k_cameraController.Update(Time.deltaTime);

		// Render-driven lighting smoothing + flicker
		k_lightController.Update(Time.deltaTime);

		OnRenderUpdate?.Invoke();
	}

	#endregion
}
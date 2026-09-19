using RPGGame.Core.Characters;
using RPGGame.Core.Events;
using RPGGame.Core.Generation;
using RPGGame.Core.Metrics;
using RPGGame.Core.Prototypes;
using RPGGame.Core.Prototypes.Events;
using RPGGame.Core.Prototypes.Expedition;
using RPGGame.Core.Simulation;
using RPGGame.Core.Stats;
using UnityEngine;

namespace RPGGame.Core.Expedition;

/// <summary>
/// Manages the overall expedition - travel, events, resources, and game flow.
/// This is the SINGLE SOURCE OF TRUTH for all expedition-related game logic.
/// 
/// TravelData is a pure data container owned by this class.
/// All game decisions, events, and flow control happen here.
/// </summary>
[Dependency(RegistrationType.Singleton)]
public partial class ExpeditionManager {
	#region Fields

	private readonly GameLoop k_gameLoop;
	private readonly MetricsManager k_metrics;
	private readonly CampManager k_campManager;
	private readonly CharacterManager k_characterManager;
	private readonly EventManager k_eventManager;
	private readonly GameDb? k_gameDb;

	private RunState? k_runState;
	private TravelData? k_travel;
	private FogOfWar? k_fogOfWar;
	private FogRegion? k_currentFogRegion;
	private WeatherSystem k_weather;

	private float k_accumulatedTime;
	private float k_accumulatedGameMinutes;

	private int k_lastHourChecked;
	private static readonly float HOURS_PER_TICK = 1f / GameTime.TICKS_PER_HOUR;

	private int k_lastDayChecked;

	private float k_distanceSinceLastEvent;

	private readonly TilesRPG MIN_EVENT_SPACING = 3.Tiles();
	private readonly float BASE_EVENT_CHANCE = 55.Percent();

	private IsoPathGenerator? k_pathGenerator;
	private IsoExpeditionPath? k_expeditionPath;
	private bool k_tutorialComplete;

	#endregion

	#region Properties

	public bool IsActive { get; private set; }
	public RunState? RunState => k_runState;
	public TravelData? Travel => k_travel;
	public FogOfWar? FogOfWar => k_fogOfWar;
	public CampManager CampManager => k_campManager;
	public bool IsTraveling => k_travel?.IsTraveling ?? false;
	public bool IsCamped => k_campManager.IsCamped;
	public Vector2Int CurrentPosition => k_travel?.CurrentPosition ?? Vector2Int.zero;
	public TerrainProto.ID CurrentTerrain => k_travel?.CurrentTerrain ?? Ids.Terrains.Roads.Path;
	public bool TutorialComplete => k_tutorialComplete;
	public TravelPace CurrentPace { get; private set; } = TravelPace.Walk;
	public SimulationSpeed SimSpeed { get; private set; } = SimulationSpeed.Normal;
	private bool k_isSubscribedToGameTime;

	public float CurrentStamina => k_runState?.Character != null 
		? k_characterManager.GetStat(k_runState.Character, Ids.Stats.Resource.CurrentStamina) 
		: 0f;

	public float EffectiveMaxStamina => k_runState?.Character != null 
		? k_characterManager.GetEffectiveMaxStamina(k_runState.Character) 
		: 100f;

	public float Fatigue => k_runState?.Character != null 
		? k_characterManager.GetStat(k_runState.Character, Ids.Stats.Resource.Fatigue) 
		: 0f;

	public float MaxFatigue => k_runState?.Character != null 
		? k_characterManager.GetStat(k_runState.Character, Ids.Stats.Resource.MaxFatigue) 
		: 100f;

	public bool IsExhausted => k_runState?.Character != null 
		&& k_characterManager.IsExhausted(k_runState.Character);

	public bool IsOverfatigued => k_runState?.Character != null 
		&& k_characterManager.IsOverfatigued(k_runState.Character);

	public int Food => k_runState?.Character.BaseStats.GetInt(Ids.Stats.Expedition.FoodOnHand) ?? 0;
	public int MedicalSupplies => k_runState?.Character.BaseStats.GetInt(Ids.Stats.Expedition.MedicalSupplies) ?? 0;
	public int CampingSupplies => k_runState?.Character.BaseStats.GetInt(Ids.Stats.Expedition.CampingSupplies) ?? 0;

	public IsoExpeditionPath? CurrentIsoPath => k_expeditionPath;
	public IsoPathGenerator? GetIsoPathGenerator() => k_pathGenerator;

	public float Morale => k_runState?.Morale ?? 100f;
	public bool CanSetupCamp => CampingSupplies > 0;
	public bool FoodCritical => Food <= 1;
	public bool IsStarving => Food <= 0;
	public bool MoraleCritical => Morale <= 20;

	public TravelQueue StopQueue { get; } = new();
	public FogRegion? CurrentFogRegion => k_currentFogRegion;
	public WeatherSystem Weather => k_weather;
	public List<RunModifier> ActiveModifiers { get; } = [];
	public GameDb GameDb => k_gameDb!;

	#endregion

	#region Events

	public event Action? OnExpeditionStarted;
	public event Action<ExpeditionEndReason>? OnExpeditionEnded;
	public event Action<float>? OnTick;
	public event Action<int>? OnHourChanged;
	public event Action<int>? OnDayChanged;
	public event Action<string>? OnEventTriggered;
	public event Action<PathNode, List<PathConnection>>? OnPathChoiceRequired;
	public event Action<List<PathChoiceData>>? OnPathChoiceDataReady;
	public event Action<PathNode>? OnSpecialLocationReached;
	public event Action<string>? OnPlayerDied;
	public event Action? OnFogSourceReached;
	public event Action<TravelPace>? OnPaceChanged;
	public event Action<SimulationSpeed>? OnSimSpeedChanged;
	public event Action<FogRegion>? OnFogRegionChanged;
	public event Action<WeatherType>? OnWeatherChanged;
	public event Action<QueuedStop>? OnQueuedStopReached;
	public event Action? OnStaminaDepleted;
	public event Action? OnTutorialCompleted;
	public event Action<PathNode>? OnNodeArrived;
	public event Action<TerrainProto.ID>? OnTerrainChanged;

	#endregion

	#region Constructor

	public ExpeditionManager(GameLoop gameLoop, MetricsManager metrics, CampManager campManager, CharacterManager characterManager, EventManager eventManager, GameDb? gameDb = null) {
		k_gameLoop = gameLoop;
		k_metrics = metrics;
		k_campManager = campManager;
		k_characterManager = characterManager;
		k_eventManager = eventManager;
		k_gameDb = gameDb;
		k_weather = new WeatherSystem();

		k_gameLoop.OnUpdate += OnGameUpdate;

		StopQueue.OnStopReached += HandleQueuedStopReached;
		k_weather.OnWeatherChanged += (_, newWeather) => OnWeatherChanged?.Invoke(newWeather);
		k_eventManager.OnEventEnded += HandleEventEnded;

		Debug.Log("ExpeditionManager initialized");
	}

	#endregion

	#region Expedition Lifecycle

	public void StartExpedition(RunState runState, HashSet<Vector2Int>? revealedTiles = null) {
		if (IsActive) {
			Debug.LogWarning("Expedition already active");
			return;
		}

		k_runState = runState;
		k_accumulatedGameMinutes = 0;
		k_lastHourChecked = GameTime.Instance.Hour;

		k_travel = new TravelData(Vector2Int.zero);
		k_fogOfWar = new FogOfWar(revealedTiles);

		CreateStartingPath();

		k_travel.MovementSpeedModifier = runState.Stats.Get(Ids.Stats.Movement.MovementSpeed);
		if (k_travel.MovementSpeedModifier <= 0) {
			k_travel.MovementSpeedModifier = 1f;
		}

		CurrentPace = TravelPace.Walk;
		SimSpeed = SimulationSpeed.Normal;
		k_currentFogRegion = new FogRegion(0, 0, 20, 0.2f);
		k_distanceSinceLastEvent = 0;
		ActiveModifiers.Clear();
		StopQueue.Clear();

		if (runState.Character != null) {
			k_characterManager.FullRestStamina(runState.Character);
		}

		k_weather = new WeatherSystem();
		k_weather.OnWeatherChanged += (_, newWeather) => OnWeatherChanged?.Invoke(newWeather);

		k_lastDayChecked = GameTime.Instance.Day;
		k_accumulatedTime = 0;
		IsActive = true;

		k_metrics.StartRun();
		SubscribeToGameTime();
		OnExpeditionStarted?.Invoke();

		//AutoStartTravel();

		Debug.Log("Expedition started - travel initiated");
	}

	private void AutoStartTravel() {
		if (k_travel?.CurrentNode == null) {
			Debug.LogWarning("AutoStartTravel: No current node set");
			return;
		}

		var availablePaths = k_travel.CurrentNode.Connections
			.Where(c => !c.IsBlocked)
			.ToList();

		if (availablePaths.Count == 0) {
			Debug.LogWarning("AutoStartTravel: No available paths from starting node");
			return;
		}

		var targetNodeId = availablePaths[0].TargetNodeId;
		if (StartTravelTo(targetNodeId)) {
			k_travel.Resume();
			Debug.Log($"AutoStartTravel: Started traveling to {targetNodeId}");
		} else {
			Debug.LogWarning($"AutoStartTravel: Failed to start travel to {targetNodeId}");
		}
	}

	public void EndExpedition(ExpeditionEndReason reason) {
		if (!IsActive) {
			return;
		}

		IsActive = false;
		UnsubscribeFromGameTime();
		if (k_runState != null && k_fogOfWar != null) {
			k_runState.RevealedTiles = k_fogOfWar.GetAllRevealedTiles();
		}

		if (k_runState != null && k_travel != null) {
			k_runState.DistanceFromVillage = k_travel.DistanceFromVillage;
			k_runState.TotalDistanceTraveled = k_travel.TotalDistanceTraveled;
		}

		if (k_runState?.Character != null) {
			k_runState.Fatigue = k_characterManager.GetStat(k_runState.Character, Ids.Stats.Resource.Fatigue);
		}

		if (k_campManager.IsCamped) {
			k_campManager.BreakCamp();
		}

		k_metrics.EndRun(reason == ExpeditionEndReason.Victory);
		k_expeditionPath = null;
		OnExpeditionEnded?.Invoke(reason);

		Debug.Log($"Expedition ended: {reason}");
	}

	private void CreateStartingPath() {
		if (k_travel == null || k_gameDb == null) {
			return;
		}

		k_tutorialComplete = false;

		k_pathGenerator = new IsoPathGenerator(k_gameDb, k_eventManager);

		var config = new IsoPathGenerator.Config {
			IsTutorialRun = true,
			GuaranteedStartEvents = [
				Ids.Events.Intro.GateGuardBoon,
				Ids.Events.Intro.DefenseBoonChoice,
				Ids.Events.Intro.FirstCombatEncounter
			],
			GuaranteedEventNodeIndices = [1, 2, 3],
			TutorialDungeonNodeIndex = 4,
			GuaranteedFirstDungeon = Ids.Locations.Dungeons.OvergrownRuins,
			FirstBranchNodeIndex = 5
		};

		int seed = k_runState?.WorldSeed ?? 42;
		k_expeditionPath = k_pathGenerator.GenerateInitial(seed, config);

		foreach (var isoNode in k_expeditionPath.Nodes.Values) {
			var legacyNode = ConvertToLegacyNode(isoNode);
			k_travel.RegisterNode(legacyNode);
		}

		if (k_expeditionPath.StartNode != null) {
			var startLegacy = k_travel.GetNode(k_expeditionPath.StartNode.Id);
			if (startLegacy != null) {
				k_travel.SetCurrentNode(startLegacy);
			}
		}

		Debug.Log($"CreateStartingPath: Generated {k_travel.NodeCount} nodes using IsoPathGenerator");
	}

	#endregion

	#region IsoPath Conversion Helpers

	private PathNode ConvertToLegacyNode(IsoPathNode isoNode) {
		var legacyNode = new PathNode(isoNode.Id, isoNode.Position, isoNode.Type) {
			TerrainId = isoNode.TerrainId,
			BiomeId = isoNode.BiomeId,
			Name = isoNode.Name,
			Description = isoNode.Description,
			EventId = isoNode.EventId,
			LocationProtoId = isoNode.LocationProtoId,
			DangerLevel = isoNode.DangerLevel
		};

		foreach (var isoConn in isoNode.Connections) {
			legacyNode.Connections.Add(new PathConnection {
				TargetNodeId = isoConn.TargetNodeId,
				Distance = isoConn.Distance.Value,
				TerrainId = isoConn.TerrainId,
				IsExplored = false
			});
		}

		return legacyNode;
	}

	private void SyncPathNodesToTravelData() {
		if (k_expeditionPath == null || k_travel == null) return;

		int newNodes = 0;
		int updatedConnections = 0;

		foreach (var isoNode in k_expeditionPath.Nodes.Values) {
			var existingNode = k_travel.GetNode(isoNode.Id);

			if (existingNode == null) {
				var legacyNode = ConvertToLegacyNode(isoNode);
				k_travel.RegisterNode(legacyNode);
				newNodes++;
			} else {
				foreach (var isoConn in isoNode.Connections) {
					bool hasConnection = existingNode.Connections.Any(c => c.TargetNodeId == isoConn.TargetNodeId);
					if (!hasConnection) {
						existingNode.Connections.Add(new PathConnection {
							TargetNodeId = isoConn.TargetNodeId,
							Distance = isoConn.Distance.Value,
							TerrainId = isoConn.TerrainId,
							IsExplored = false
						});
						updatedConnections++;
					}
				}
			}
		}

		if (newNodes > 0 || updatedConnections > 0) {
			Debug.Log($"SyncPathNodesToTravelData: Added {newNodes} new nodes, updated {updatedConnections} connections");
		}
	}

	#endregion

	#region Travel Actions

	private void UpdateTerrainStats(TerrainProto.ID terrainId) {
		if (k_travel == null || k_gameDb == null) return;

		var proto = k_gameDb.Get<TerrainProto>(terrainId);
		if (proto != null) {
			k_travel.CurrentTerrainStats = new TerrainStatsLite(
				proto.MovementSpeedMultiplier,
				proto.StaminaDrainPerDistance,
				proto.FatigueRateMultiplier,
				ColorRPG.FromHex(proto.MapColor)
			);
		}
	}

	public bool StartTravelTo(PathNodeId targetNodeId) {
		if (k_travel?.CurrentNode == null) return false;

		var connection = k_travel.CurrentNode.GetConnectionTo(targetNodeId);
		if (connection == null) {
			Debug.LogWarning($"No connection from {k_travel.CurrentNode.Id} to {targetNodeId}");
			return false;
		}

		if (connection.IsBlocked) {
			Debug.Log($"Path to {targetNodeId} is blocked");
			return false;
		}

		bool success = k_travel.SetupTravelTo(targetNodeId);
		if (success) {
			UpdateTerrainStats(k_travel.CurrentTerrain);
			OnTerrainChanged?.Invoke(k_travel.CurrentTerrain);
		}

		return success;
	}

	public bool ContinueTravel() {
		if (k_travel == null) {
			return false;
		}

		if (k_travel.HasPendingDestination) {
			k_travel.Resume();
			Debug.Log($"ContinueTravel: Resuming to existing target {k_travel.TargetNode?.Id}");
			return true;
		}

		if (k_travel.CurrentNode == null) {
			return false;
		}

		var forwardPaths = k_travel.GetForwardConnections();

		if (forwardPaths.Count == 0) {
			// Always try to extend at dead ends, even during tutorial
			// Dead ends on teaser branches need extension regardless of tutorial state
			EnsurePathAhead();
			forwardPaths = k_travel.GetForwardConnections();

			if (forwardPaths.Count == 0) {
				Debug.Log("ContinueTravel: Dead end reached");
				return false;
			}
		}

		if (forwardPaths.Count == 1) {
			if (StartTravelTo(forwardPaths[0].TargetNodeId)) {
				k_travel.Resume();
				Debug.Log($"ContinueTravel: Auto-continuing to {forwardPaths[0].TargetNodeId}");
				return true;
			}
			return false;
		}

		Debug.Log($"ContinueTravel: Branch point with {forwardPaths.Count} paths");
		k_travel.Pause();
		var choiceData = GeneratePathChoiceData(k_travel.CurrentNode, forwardPaths);
		OnPathChoiceDataReady?.Invoke(choiceData);
		OnPathChoiceRequired?.Invoke(k_travel.CurrentNode, forwardPaths);
		return false;
	}

	public bool CanCampHere() {
		if (k_travel == null) {
			return false;
		}

		if (k_travel.IsTraveling && CurrentPace != TravelPace.Wait) {
			return false;
		}

		if (k_travel.CurrentNode == null) {
			return false;
		}

		if (k_travel.CurrentNode.Type.CanCamp()) {
			return true;
		}
		
		return k_travel.CurrentTerrain != Ids.Terrains.Water.Ocean &&
		       k_travel.CurrentTerrain != Ids.Terrains.Special.Blocked;
	}

	public void PauseTravel() {
		k_travel?.Pause();
		k_metrics.Increment(MetricType.TimesPaused);
	}

	public void ResumeTravel() {
		if (k_travel == null) return;

		if (!k_travel.IsTraveling && !k_travel.HasPendingDestination) {
			if (!ContinueTravel()) {
				return;
			}
		}

		k_travel.Resume();
	}

	public void SetTravelSpeed(TravelSpeed speed) {
		k_travel?.SetSpeed(speed);
		k_metrics.Increment(MetricType.TimesChangedSpeed);
	}

	#endregion

	#region Procedural Generation

	private bool IsTutorialDungeon(PathNode dungeonNode) {
		if (dungeonNode.LocationProtoId == Ids.Locations.Dungeons.OvergrownRuins.Value) {
			return true;
		}
		return false;
	}

	private void CompleteTutorial() {
		if (k_tutorialComplete) return;

		k_tutorialComplete = true;
		k_runState?.ActiveConditions.Add("tutorial_complete");

		if (k_expeditionPath?.GenerationState != null) {
			var genState = k_expeditionPath.GenerationState;
			genState.TutorialComplete = true;
			genState.BiomeState.CurrentBiomeId = Ids.Biomes.Farmlands.Value;
			genState.BiomeState.CurrentBiomeIndex = 1;
			genState.BiomeState.NodesRemainingInBiome = 6;
			genState.BiomeState.CurrentDangerLevel = 0.15f;
			Debug.Log($"Tutorial complete - Biome: {genState.BiomeState.CurrentBiomeId}");
		}

		OnTutorialCompleted?.Invoke();
		Debug.Log("Tutorial complete! Full procedural generation now active.");
	}

	public void OnDungeonCompleted(PathNodeId dungeonNodeId) {
		if (k_travel == null || k_runState == null) return;
		var dungeonNode = k_travel.GetNode(dungeonNodeId);
		if (dungeonNode == null) return;

		Debug.Log($"OnDungeonCompleted: {dungeonNodeId} ({dungeonNode.Name})");

		if (!k_tutorialComplete && IsTutorialDungeon(dungeonNode)) {
			CompleteTutorial();
		}

		bool hasForwardPath = dungeonNode.Connections.Any(c => 
			!c.IsBlocked && c.TargetNodeId != k_travel.PreviousNode?.Id);

		if (!hasForwardPath) {
			GenerateProceduralPathFrom(dungeonNode);
		}

		ContinueTravel();
	}

	public void OnDungeonBypassed(PathNodeId dungeonNodeId) {
		if (k_travel == null || k_runState == null) return;
		var dungeonNode = k_travel.GetNode(dungeonNodeId);
		if (dungeonNode == null) return;

		Debug.Log($"OnDungeonBypassed: {dungeonNodeId} ({dungeonNode.Name})");

		if (!k_tutorialComplete && IsTutorialDungeon(dungeonNode)) {
			CompleteTutorial();
		}

		bool hasForwardPath = dungeonNode.Connections.Any(c => 
			!c.IsBlocked && c.TargetNodeId != k_travel.PreviousNode?.Id);

		if (!hasForwardPath) {
			Debug.Log("Dungeon has no forward path, generating procedural content...");
			GenerateProceduralPathFrom(dungeonNode);
		}

		if (!ContinueTravel()) {
			Debug.Log("OnDungeonBypassed: At dead end after generation attempt");
		}
		
	}

	public PathNodeId? GetCurrentDungeonNodeId() {
		if (k_travel?.CurrentNode?.Type == PathNodeType.Dungeon) {
			return k_travel.CurrentNode.Id;
		}
		return null;
	}

	private void GenerateProceduralPathFrom(PathNode fromNode) {
		if (k_travel == null || k_runState == null || k_pathGenerator == null || k_expeditionPath == null) {
			Debug.LogWarning("GenerateProceduralPathFrom: Missing required state");
			return;
		}

		var isoNode = k_expeditionPath.GetNode(fromNode.Id);
		if (isoNode != null) {
			bool extended = k_pathGenerator.ExtendPathFromNode(k_expeditionPath, isoNode);
			if (extended) {
				SyncPathNodesToTravelData();
				Debug.Log($"GenerateProceduralPathFrom: Extended path from {fromNode.Id}");
			} else {
				Debug.LogWarning($"GenerateProceduralPathFrom: Failed to extend from {fromNode.Id}");
			}
		} else {
			Debug.LogWarning($"GenerateProceduralPathFrom: Node {fromNode.Id} not found in IsoExpeditionPath");
		}
	}

	private void EnsurePathAhead() {
		if (k_travel?.CurrentNode == null) {
			return;
		}

		if (k_pathGenerator == null || k_expeditionPath == null) {
			return;
		}

		var isoNode = k_expeditionPath.GetNode(k_travel.CurrentNode.Id);
		if (isoNode == null) {
			Debug.LogWarning($"EnsurePathAhead: Node {k_travel.CurrentNode.Id} not found in IsoExpeditionPath");
			return;
		}

		// Check if we're at a dead end - always extend dead ends regardless of tutorial state
		bool isDeadEnd = isoNode.Connections.Count <= 1;
    
		if (isDeadEnd) {
			// Dead end - must extend to continue
			// Also complete tutorial if we're extending from a dead end (player has chosen a path)
			if (!k_tutorialComplete) {
				CompleteTutorial();
			}
        
			bool extended = k_pathGenerator.ExtendPathFromNode(k_expeditionPath, isoNode);
			if (extended) {
				SyncPathNodesToTravelData();
				Debug.Log($"EnsurePathAhead: Extended dead-end at node {k_travel.CurrentNode.Id}");
			}
			return;
		}

		// Proactive extension only after tutorial
		if (!k_tutorialComplete) {
			return;
		}

		bool extended2 = k_pathGenerator.OnPlayerReachedNode(k_expeditionPath, isoNode);
		if (extended2) {
			SyncPathNodesToTravelData();
			Debug.Log($"EnsurePathAhead: Proactively extended path from node {k_travel.CurrentNode.Id}");
		}
	}

	#endregion

	#region Game Loop

	private void SubscribeToGameTime() {
		if (k_isSubscribedToGameTime) {
			return;
		}

		GameTime.Instance.OnHourChanged += HandleGameTimeHourChanged;
		GameTime.Instance.OnDayChanged += HandleGameTimeDayChanged;

		k_isSubscribedToGameTime = true;
	}

	private void UnsubscribeFromGameTime() {
		if (!k_isSubscribedToGameTime) {
			return;
		}

		GameTime.Instance.OnHourChanged -= HandleGameTimeHourChanged;
		GameTime.Instance.OnDayChanged -= HandleGameTimeDayChanged;

		k_isSubscribedToGameTime = false;
	}

	private void HandleGameTimeHourChanged(int hour) {
		if (!IsActive || k_runState == null || k_travel == null) {
			return;
		}

		k_lastHourChecked = hour;
		OnHourChanged?.Invoke(hour);

		bool isDay = GameTime.Instance.TimeOfDay.IsDay();
		OnHourPassedUpdateMorale(k_travel.CurrentTerrain, isDay);
	}

	private void HandleGameTimeDayChanged(int day) {
		if (!IsActive || k_runState == null || k_travel == null) {
			return;
		}

		k_lastDayChecked = day;
		OnDayChanged?.Invoke(day);

		ProcessNewDay();
	}

	private void OnGameUpdate() {
		if (!IsActive || k_runState == null || k_travel == null) {
			return;
		}

		if (SimSpeed == SimulationSpeed.Paused) {
			return;
		}

		if (k_travel.IsPaused && !k_campManager.IsCamped && CurrentPace != TravelPace.Wait) {
			return;
		}

		float simMultiplier = SimSpeed.GetMultiplier();
		float deltaTime = Time.deltaTime * k_travel.Speed.GetTimeMultiplier() * simMultiplier;

		k_accumulatedTime += deltaTime;

		while (k_accumulatedTime >= HOURS_PER_TICK) {
			k_accumulatedTime -= HOURS_PER_TICK;
			ProcessTick(HOURS_PER_TICK);
		}

		if (k_travel.IsTraveling && CurrentPace.IsMoving()) {
			float paceMultiplier = CurrentPace.GetSpeedMultiplier();
			float effectiveDelta = deltaTime * paceMultiplier;

			if (k_travel.UpdateTravelProgress(effectiveDelta)) {
				HandleNodeArrival();
			}

			UpdateStaminaForTravel(Time.deltaTime * simMultiplier);
			k_distanceSinceLastEvent += effectiveDelta;
		}

		bool isResting = k_campManager.IsCamped || !k_travel.IsTraveling || CurrentPace == TravelPace.Wait;
		if (k_runState?.Character != null) {
			float fatigueRateMod = k_runState.Stats.Get(Ids.Stats.Expedition.FatigueRate);
			k_characterManager.UpdateStaminaForTravel(
				k_runState.Character,
				Time.deltaTime * simMultiplier,
				CurrentPace,
				isResting,
				fatigueRateMod
			);
		}

		k_weather.Update(deltaTime / 60f);

		int visionRange = k_runState?.Stats.GetInt(Ids.Stats.Expedition.VisionRange) ?? 6;
		if (visionRange <= 0) visionRange = 6;

		visionRange = (int)(visionRange * k_weather.GetEffects().VisibilityModifier);

		if (CurrentPace == TravelPace.Wait) {
			visionRange = (int)(visionRange * CurrentPace.GetPerceptionModifier());
		}

		k_fogOfWar?.UpdateVisibility(k_travel.CurrentPosition, visionRange);

		CheckFogRegionTransition();

		OnTick?.Invoke(deltaTime);
	}

	private void UpdateStaminaForTravel(float realDeltaTime) {
		if (k_runState?.Character == null) return;
        
		var character = k_runState.Character;
		float staminaCost = CurrentPace.GetStaminaCostPerTile();

		if (staminaCost > 0) {
			k_characterManager.ConsumeStamina(character, staminaCost * realDeltaTime);

			if (k_characterManager.GetStat(character, Ids.Stats.Resource.CurrentStamina) <= 0) {
				HandleStaminaDepleted();
			}

			float minStamina = CurrentPace.GetMinimumStamina();
			if (!k_characterManager.HasStamina(character, minStamina)) {
				var fallbackPace = k_characterManager.GetFallbackPace(character, CurrentPace);
				if (fallbackPace != CurrentPace) {
					SetTravelPace(fallbackPace);
				}
			}
		}

		float fatigueRateMod = k_runState.Stats.Get(Ids.Stats.Expedition.FatigueRate);

		float fatigueRate = CurrentPace.GetFatigueRatePerHour() * fatigueRateMod / GameTime.SECONDS_PER_HOUR;
        
		k_characterManager.AddFatigue(character, fatigueRate * realDeltaTime);
	}

	

	private void ProcessTick(float hours) {
		if (k_runState == null || k_travel == null) {
			return;
		}

		// Convert hours to minutes and accumulate
		float minutes = hours * 60f;
		k_accumulatedGameMinutes += minutes;
    
		// Advance time every game minute for smoother updates
		if (k_accumulatedGameMinutes >= 1.0f) {
			int wholeMinutes = (int)k_accumulatedGameMinutes;
			k_accumulatedGameMinutes -= wholeMinutes;

		}

		if (k_travel.IsTraveling) {
			CheckForRandomEvent();
		}

		CheckDeathConditions();

		k_metrics.Tick(hours);
		if (k_travel.IsTraveling) {
			k_metrics.Increment(MetricType.TimeTravelingSeconds);
		}
	}

	/// <summary>
	/// Updates morale based on terrain and conditions. Fatigue is handled by CharacterManager.
	/// </summary>
	private void OnHourPassedUpdateMorale(TerrainProto.ID terrain, bool isDay) {
		if (k_runState == null) return;
		var liveCharacter = k_runState.Character;
		var fullProto = k_gameDb?.Get<TerrainProto>(terrain);
		// Morale is affected by terrain
		float moraleMod = fullProto?.MoraleModifierPerHour ?? 100.Percent();
		var currentMorale = liveCharacter.BaseStats.Get(Ids.Stats.Resource.Morale, 100.Percent());

		if (moraleMod != 0) {
			liveCharacter.BaseStats.Set(Ids.Stats.Resource.Morale, currentMorale + moraleMod);
		}

		// Low food affects morale
		if (liveCharacter.BaseStats.GetInt(Ids.Stats.Expedition.FoodOnHand) <= 1) {
			liveCharacter.BaseStats.Set(Ids.Stats.Resource.Morale, 
				Math.Clamp(liveCharacter.BaseStats.Get(Ids.Stats.Resource.Morale, 100.Percent()) - 2f, 0, 100));
		}

		float currentFatigue = liveCharacter.BaseStats.Get(Ids.Stats.Resource.Fatigue);
		float maxFatigue = liveCharacter.BaseStats.Get(Ids.Stats.Resource.MaxFatigue);

		// High fatigue affects morale (read from character)

		if (maxFatigue > 0 && currentFatigue / maxFatigue >= 0.7f) {
			liveCharacter.BaseStats.Set(Ids.Stats.Resource.Morale,
				Math.Clamp(liveCharacter.BaseStats.Get(Ids.Stats.Resource.Morale, 100.Percent()) - 1f, 0, 100));
			
		}
	}

	private void ProcessNewDay() {
		if (k_runState == null) {
			return;
		}
		var liveCharacter = k_runState.Character;

		if (liveCharacter.BaseStats.GetInt(Ids.Stats.Expedition.FoodOnHand) > 0) {
			liveCharacter.BaseStats.Subtract(Ids.Stats.Expedition.FoodOnHand, 1);
		} else {
			liveCharacter.BaseStats.Set(Ids.Stats.Resource.Morale, Math.Clamp(liveCharacter.BaseStats.Get(Ids.Stats.Resource.Morale) - 10f, 0, 100));
		}

		k_metrics.Increment(MetricType.TotalDaysSurvived);
		k_metrics.Increment(MetricType.CurrentRunDays);

		if (k_runState.DaysSinceFullRest > 0) {
			k_metrics.SetMax(MetricType.MaxDaysWithoutSleep, k_runState.DaysSinceFullRest);
		}

		Debug.Log($"Day {GameTime.Instance.Day} begins");
	}

	private void CheckDeathConditions() {
		if (k_runState == null) {
			return;
		}

		if (k_runState.IsDead) {
			OnPlayerDied?.Invoke("You have fallen...");
			EndExpedition(ExpeditionEndReason.Death);
			return;
		}

		if (IsStarving) {
			float damage = k_runState.Stats.Get(Ids.Stats.Resource.MaxHealth) * 0.05f;
			k_runState.Stats.Subtract(Ids.Stats.Resource.CurrentHealth, damage);
			k_metrics.Increment(MetricType.TimesRanOutOfFood);
		}

		if (IsOverfatigued) {
			k_travel?.Pause();
			OnEventTriggered?.Invoke(Ids.Events.Random.Exhaustion.Value);
		}

		if (k_runState.Character != null) {
			float fatigue = k_characterManager.GetStat(k_runState.Character, Ids.Stats.Resource.Fatigue);
			float maxFatigue = k_characterManager.GetStat(k_runState.Character, Ids.Stats.Resource.MaxFatigue);
			if (fatigue >= maxFatigue) {
				HandleCollapse();
			}
		}

		if (Morale <= 0) {
			k_metrics.Increment(MetricType.TimesMoraleHitZero);
			k_runState.ActiveConditions.Add("Broken");
		}
	}

	private void CheckFogRegionTransition() {
		if (k_travel == null) return;

		float distance = k_travel.DistanceFromVillage;

		if (k_currentFogRegion != null && k_currentFogRegion.ContainsTile(distance)) {
			return;
		}

		int regionSize = 20;
		int regionIndex = (int)(distance / regionSize);
		int tileStart = regionIndex * regionSize;
		int tileEnd = (regionIndex + 1) * regionSize;
		float baseStrength = Math.Min(1.0f, 0.2f + (regionIndex * 0.2f));

		k_currentFogRegion = new FogRegion(regionIndex, tileStart, tileEnd, baseStrength);
		OnFogRegionChanged?.Invoke(k_currentFogRegion);
	}

	#endregion

	#region Node Arrival Handling

	private void HandleNodeArrival() {
		if (k_travel?.TargetNode == null || k_runState == null) {
			return;
		}

		var arrivedNode = k_travel.TargetNode;

		float distanceTraveled = k_travel.CompleteArrival();

		Debug.Log($"HandleNodeArrival: {arrivedNode.Name ?? arrivedNode.Id.ToString()} (Type: {arrivedNode.Type})");

		k_runState.VisitedPathNodes.Add(arrivedNode.Id);
		k_metrics.Increment(MetricType.TilesTraveled);
		k_metrics.RecordTileMove(
				k_travel.DistanceFromVillage,
				!k_runState.RevealedTiles.Contains(arrivedNode.Position)
			);

		OnNodeArrived?.Invoke(arrivedNode);

		// Complete tutorial when we leave the first intersection (i.e., we've chosen a branch)
		// The previous node being an intersection means we just made a choice
		if (!k_tutorialComplete && k_travel.PreviousNode?.Type == PathNodeType.Intersection) {
			CompleteTutorial();
		}

		if (k_pathGenerator != null && k_expeditionPath != null) {
			var isoNode = k_expeditionPath.GetNode(arrivedNode.Id);
			if (isoNode != null) {
				k_pathGenerator.OnPlayerReachedNode(k_expeditionPath, isoNode);
				SyncPathNodesToTravelData();
			}
		}

		if (StopQueue.IsNextStop(arrivedNode.Position)) {
			var queuedStop = StopQueue.PopNextStop();
			if (queuedStop != null) {
				HandleQueuedStopReached(queuedStop);
				return;
			}
		}

		if (!string.IsNullOrEmpty(arrivedNode.EventId) && 
			!k_runState.EncounteredEvents.Contains(arrivedNode.EventId)) {
			Debug.Log($"HandleNodeArrival: Triggering event {arrivedNode.EventId}");
			k_runState.EncounteredEvents.Add(arrivedNode.EventId);
			k_travel.Pause();
			OnEventTriggered?.Invoke(arrivedNode.EventId);
			return;
		}

		if (k_tutorialComplete) {
			EnsurePathAhead();
		}

		switch (arrivedNode.Type) {
			case PathNodeType.CampSite:
				OnSpecialLocationReached?.Invoke(arrivedNode);
				ContinueTravel();
				break;

			case PathNodeType.Settlement:
				OnSpecialLocationReached?.Invoke(arrivedNode);
				ContinueTravel();
				break;

			case PathNodeType.ResourceNode:
				ContinueTravel();
				break;

			case PathNodeType.Dungeon:
			case PathNodeType.BossLocation:
				k_travel.Pause();
				OnSpecialLocationReached?.Invoke(arrivedNode);
				break;

			case PathNodeType.FogSource:
				k_travel.Pause();
				OnFogSourceReached?.Invoke();
				break;

			case PathNodeType.Landmark:
				if (arrivedNode.Name != null) {
					k_runState.DiscoveredLandmarks.Add(arrivedNode.Name);
					k_metrics.Increment(MetricType.LandmarksDiscovered);
				}
				ContinueTravel();
				break;

			case PathNodeType.Waypoint:
			case PathNodeType.Intersection:
			default:
				ContinueTravel();
				break;
		}
	}

	#endregion

	#region Event Handlers

	private void HandleEventEnded(EventProto evt, EventOutcomeProto? outcome) {
		if (k_runState == null) return;

		if (k_travel?.CurrentNode != null && 
			k_travel.CurrentNode.Type != PathNodeType.Dungeon &&
			k_travel.CurrentNode.Type != PathNodeType.BossLocation) {
			ContinueTravel();
		}
	}

	private void HandleQueuedStopReached(QueuedStop stop) {
		k_travel?.Pause();
		OnQueuedStopReached?.Invoke(stop);

		switch (stop.Reason) {
			case StopReason.Camp:
				SetupCamp();
				break;

			case StopReason.Event:
				if (!string.IsNullOrEmpty(stop.InteractionId)) {
					OnEventTriggered?.Invoke(stop.InteractionId);
				}
				break;

			case StopReason.Resource:
				break;

			case StopReason.Location:
				break;
		}
	}

	private void HandleStaminaDepleted() {
		if (CurrentPace != TravelPace.Walk) {
			SetTravelPace(TravelPace.Walk);
		}
		OnStaminaDepleted?.Invoke();
	}

	private void HandleCollapse() {
		k_travel?.Pause();
		OnEventTriggered?.Invoke(Ids.Events.Random.Collapse.Value);
	}

	#endregion

	#region Player Actions - Travel Pace & Simulation Speed

	public void SetTravelPace(TravelPace pace) {
		if (pace != TravelPace.Wait && k_runState?.Character != null) {
			float minStamina = pace.GetMinimumStamina();
			if (!k_characterManager.HasStamina(k_runState.Character, minStamina) && pace > TravelPace.Walk) {
				pace = k_characterManager.GetFallbackPace(k_runState.Character, pace);
			}
		}

		if (CurrentPace != pace) {
			TravelPace previousPace = CurrentPace;
			CurrentPace = pace;

			if (pace == TravelPace.Wait && k_travel != null) {
				k_travel.Pause();
			}
			else if (previousPace == TravelPace.Wait && pace != TravelPace.Wait && k_travel != null) {
				// Only call ContinueTravel if we don't already have a destination
				if (!k_travel.HasPendingDestination) {
					ContinueTravel();
				} else {
					k_travel.Resume();
				}
			}

			OnPaceChanged?.Invoke(pace);
			k_metrics.Increment(MetricType.TimesChangedSpeed);
		}
	}

	public void SetSimulationSpeed(SimulationSpeed speed) {
		if (SimSpeed != speed) {
			SimSpeed = speed;
			OnSimSpeedChanged?.Invoke(speed);
			Debug.Log($"Simulation speed set to: {speed}");
		}
	}

	public void TogglePause() {
		if (SimSpeed == SimulationSpeed.Paused) {
			SetSimulationSpeed(SimulationSpeed.Normal);
		} else {
			SetSimulationSpeed(SimulationSpeed.Paused);
		}
	}

	#endregion

	#region Player Actions - Queued Stops

	public void QueueStopAtTile(Vector2Int tile, StopReason reason, string? interactionId = null) {
		StopQueue.AddStop(tile, reason, interactionId);
	}

	public void QueueStopAtNode(PathNodeId nodeId, StopReason reason, string? interactionId = null) {
		var node = k_travel?.GetNode(nodeId);
		if (node != null) {
			StopQueue.AddStop(node.Position, reason, interactionId);
		}
	}

	public void QueueCampAtTile(Vector2Int tile) {
		StopQueue.AddCampStop(tile);
	}

	public void QueueCampAtNode(PathNodeId nodeId) {
		var node = k_travel?.GetNode(nodeId);
		if (node != null) {
			StopQueue.AddCampStop(node.Position);
		}
	}

	public void CancelQueuedStopAtTile(Vector2Int tile) {
		StopQueue.RemoveStopAt(tile);
	}

	public void ClearQueuedStops() {
		StopQueue.Clear();
	}

	#endregion

	#region Player Actions - Path Choice

	public bool ChoosePath(PathNodeId targetNodeId) {
		if (k_travel == null) return false;
		k_metrics.Increment(MetricType.ChoicesMade);
		bool success = StartTravelTo(targetNodeId);
		if (success) {
			k_travel.Resume();
		}
		return success;
	}

	public bool ChoosePathWithModifiers(PathChoiceData choice) {
		if (k_travel == null) {
			return false;
		}

		foreach (var bonus in choice.Bonuses) {
			ActiveModifiers.Add(new RunModifier {
				Type = bonus.Type,
				Value = bonus.Value,
				Source = $"Path: {choice.Name}"
			});
		}

		foreach (var penalty in choice.Penalties) {
			ActiveModifiers.Add(new RunModifier {
				Type = penalty.Type,
				Value = -penalty.Value,
				Source = $"Path: {choice.Name}"
			});
		}

		k_metrics.Increment(MetricType.ChoicesMade);

		// EXTEND THE CHOSEN BRANCH BEFORE starting travel
		// This ensures the path ahead is visible immediately when the player starts moving
		if (k_pathGenerator != null && k_expeditionPath != null && k_travel.CurrentNode != null) {
			var isoIntersection = k_expeditionPath.GetNode(k_travel.CurrentNode.Id);
			if (isoIntersection != null) {
				k_pathGenerator.OnPathChosen(k_expeditionPath, isoIntersection, choice.NodeId);
				SyncPathNodesToTravelData();
			}
		}

		bool success = StartTravelTo(choice.NodeId);
        
		if (success) {
			k_travel.Resume();
		}
		return success;
	}

	#endregion

	#region Player Actions - Camp & Resources

	public bool SetupCamp() {
		if (k_runState == null || k_travel == null) {
			return false;
		}

		var liveCharacter = k_runState.Character;

		if (!CanCampHere()) {
			return false;
		}

		if (liveCharacter.BaseStats.GetInt(Ids.Stats.Expedition.CampingSupplies) <= 0) {
			return false;
		}

		liveCharacter.BaseStats.Subtract(Ids.Stats.Expedition.CampingSupplies, 1);
		
		var locationType = k_travel.CurrentNode?.Type ?? PathNodeType.Waypoint;
		bool success = k_campManager.SetupCamp(locationType);

		if (success) {
			k_metrics.Increment(MetricType.CampsSetUp);
		}

		return success;
	}

	public void BreakCamp() {
		k_campManager.BreakCamp();

		if (k_travel == null) {
			return;
		}

		if (!ContinueTravel()) {
			Debug.Log("BreakCamp: At branch point, waiting for player choice");
		}
	}

	public void RestAtCamp(int hours) {
		if (k_runState?.Character == null || !k_campManager.IsCamped) return;

		k_runState.Rest(hours);

		float fatigueReduction = hours * 10f;
		k_characterManager.ReduceFatigue(k_runState.Character, fatigueReduction);
		k_characterManager.RecoverStamina(k_runState.Character, k_characterManager.GetEffectiveMaxStamina(k_runState.Character));

		k_metrics.Add(MetricType.HoursRested, hours);
		OnHourChanged?.Invoke(GameTime.Instance.Hour);
	}

	public bool UseMedicalSupply() {
		if (k_runState == null) {
			return false;
		}

		var liveCharacter = k_runState.Character;

		if (liveCharacter.BaseStats.GetInt(Ids.Stats.Expedition.MedicalSupplies) <= 0) {
			return false;
		}

		liveCharacter.BaseStats.Subtract(Ids.Stats.Expedition.MedicalSupplies, 1);
		float healAmount = 10 + liveCharacter.WisdomMod * 2;

		float healingBonus = GetModifierValue(RunModifierType.HealingBonus);
		healAmount *= (1f + healingBonus / 100f);

		float maxHealth = liveCharacter.MaxHealth;
		float currentHealth = liveCharacter.CurrentHealth;
		float effectiveHeal = Mathf.Min(healAmount, maxHealth - currentHealth);
		if (effectiveHeal <= 0) {
			return false;
		}

		liveCharacter.BaseStats.Add(Ids.Stats.Resource.CurrentHealth, effectiveHeal);
		k_metrics.Add(MetricType.HealingDone, (long)healAmount);
		return true;
	}

	public void ReturnToVillage() {
		EndExpedition(ExpeditionEndReason.Retreat);
	}

	#endregion

	#region Random Events

	public string? CheckForRandomEvent() {
		if (!IsActive || k_runState == null || k_travel == null) {
			return null;
		}

		if (k_distanceSinceLastEvent < MIN_EVENT_SPACING) {
			return null;
		}
		var fullProto = k_gameDb?.Get<TerrainProto>(k_travel.CurrentTerrain);
		float eventChance = BASE_EVENT_CHANCE;
		eventChance *= fullProto?.EncounterChanceMultiplier ?? 100.Percent();
		eventChance *= CurrentPace.GetNoiseLevel();

		if (k_runState.TimeOfDay == TimeOfDayPeriod.Night) {
			eventChance *= 1.5f;
		}

		eventChance *= k_weather.GetEffects().EncounterRateModifier;

		float encounterMod = GetModifierValue(RunModifierType.EncounterRateIncrease);
		eventChance *= (1f + encounterMod / 100f);

		if (UnityEngine.Random.value > eventChance) {
			return null;
		}

		var eventId = GenerateRandomEventId();

		if (!string.IsNullOrEmpty(eventId)) {
			k_distanceSinceLastEvent = 0;
			k_travel.Pause();
			k_metrics.Increment(MetricType.EventsEncountered);
			OnEventTriggered?.Invoke(eventId);
		}

		return eventId;
	}

	private string? GenerateRandomEventId() {
		if (k_runState == null || k_travel == null) {
			return null;
		}

		if (k_gameDb != null) {
			var validEvents = k_gameDb.GetAll<EventProto>()
				.Where(e => e.Type != EventType.Story)
				.Where(e => !k_runState.EncounteredEvents.Contains(e.Id.Value) || e.Rarity != EventRarity.Unique)
				.Where(e => e.MinDistance <= k_travel.DistanceFromVillage)
				.Where(e => e.MaxDistance < 0 || e.MaxDistance >= k_travel.DistanceFromVillage)
				.ToList();

			if (validEvents.Count > 0) {
				float totalWeight = validEvents.Sum(e => e.SpawnWeight);
				float roll = UnityEngine.Random.value * totalWeight;
				float cumulative = 0;

				foreach (var evt in validEvents) {
					cumulative += evt.SpawnWeight;
					if (roll <= cumulative) {
						return evt.Id.Value;
					}
				}
			}
		}

		var eventPool = new List<(EventProto.ID id, float weight)> {
			(Ids.Events.Random.WildAnimal, 3f),
			(Ids.Events.Random.HerbPatch, 2f),
			(Ids.Events.Random.MysteriousStranger, 1.5f),
			(Ids.Events.Random.AncientShrine, 1f),
			(Ids.Events.Random.HiddenCache, 0.5f)
		};

		if (k_runState.TimeOfDay == TimeOfDayPeriod.Night) {
			eventPool[0] = (Ids.Events.Random.WolfAmbush, 5f);
		}

		if (k_travel.CurrentTerrain == Ids.Terrains.Forests.Forest) {
			eventPool[1] = (Ids.Events.Random.HerbPatch, 4f);
		}

		float totalWeightFallback = eventPool.Sum(e => e.weight);
		float rollFallback = UnityEngine.Random.value * totalWeightFallback;
		float cumulativeFallback = 0;

		foreach (var (id, weight) in eventPool) {
			cumulativeFallback += weight;
			if (rollFallback <= cumulativeFallback) {
				return id.Value;
			}
		}

		return null;
	}

	#endregion

	#region Reset

	public void Reset() {
		IsActive = false;
		k_runState = null;
		k_travel = null;
		k_fogOfWar = null;
		k_currentFogRegion = null;
		k_pathGenerator = null;
		k_expeditionPath = null;
		k_tutorialComplete = false;
		k_accumulatedTime = 0;
		k_lastDayChecked = 0;
		k_distanceSinceLastEvent = 0;
		k_accumulatedGameMinutes = 0;
		k_lastHourChecked = 0;
		UnsubscribeFromGameTime();
		CurrentPace = TravelPace.Walk;
		SimSpeed = SimulationSpeed.Normal;
		ActiveModifiers.Clear();
		StopQueue.Clear();

		if (k_campManager.IsCamped) {
			k_campManager.BreakCamp();
		}

		Debug.Log("ExpeditionManager: Reset");
	}

	#endregion

	#region Path Choice Generation

	private List<PathChoiceData> GeneratePathChoiceData(PathNode node, List<PathConnection> paths) {
		var choices = new List<PathChoiceData>();
    
		int baseSeed = (k_runState?.WorldSeed ?? 0) 
			+ node.Position.GetHashCode() 
			+ (int)(k_travel?.TotalDistanceTraveled ?? 0) 
			+ Environment.TickCount;

		float fogStrength = k_currentFogRegion?.CurrentStrength ?? 0.2f;

		for (int i = 0; i < paths.Count; i++) {
			var path = paths[i];
			var targetNode = k_travel?.GetNode(path.TargetNodeId);

			AngleRPG direction = AngleRPG.East;
			if (targetNode != null && k_travel?.CurrentNode != null) {
				var from = (Vector2)k_travel.CurrentNode.Position;
				var to = (Vector2)targetNode.Position;
				direction = AngleRPG.FromTo(from, to);
			}

			var choice = new PathChoiceData {
				NodeId = path.TargetNodeId,
				Name = targetNode?.Name ?? $"Path {i + 1}",
				Description = GeneratePathDescription(path, targetNode),
				TerrainIcon = k_gameDb?.Get<TerrainProto>(path.TerrainId)?.IconName ?? "🛤️",
				TerrainName = k_gameDb?.Get<TerrainProto>(path.TerrainId)?.DisplayText.Name ?? "Unknown",
				Bonuses = GeneratePathBonuses(baseSeed + i * 1000, path, fogStrength),
				Penalties = GeneratePathPenalties(baseSeed + i * 2000, path, fogStrength),
				DifficultyHint = CalculatePathDifficulty(path, targetNode),
				Direction = direction,
				Distance = path.Distance
			};

			choices.Add(choice);
		}

		return choices;
	}

	private List<PathBonusData> GeneratePathBonuses(int seed, PathConnection path, float fogStrength) {
		var bonuses = new List<PathBonusData>();
		var rng = GameRandom.For("Expedition", seed);

		int bonusCount = 1 + (int)(fogStrength * 2);
		bonusCount = Math.Min(bonusCount, 3);

		float valueScale = 1.0f + (fogStrength * 1.5f);

		var possibleBonuses = new (RunModifierType type, int baseMin, int baseMax)[] {
			(RunModifierType.ExperienceBonus, 5, 15),
			(RunModifierType.MaxHealthBonus, 5, 20),
			(RunModifierType.DamageBonus, 3, 8),
			(RunModifierType.GoldFindBonus, 10, 25),
			(RunModifierType.LootQualityBonus, 5, 15),
			(RunModifierType.HealingBonus, 5, 15),
			(RunModifierType.FatigueReduction, 10, 20)
		};

		var usedTypes = new HashSet<RunModifierType>();

		for (int i = 0; i < bonusCount; i++) {
			RunModifierType type;
			int baseMin, baseMax;
			int attempts = 0;
			do {
				var (t, min, max) = possibleBonuses[rng.NextInt(possibleBonuses.Length)];
				type = t;
				baseMin = min;
				baseMax = max;
				attempts++;
			} while (usedTypes.Contains(type) && attempts < 10);

			if (usedTypes.Contains(type)) continue;
			usedTypes.Add(type);

			int scaledMin = (int)(baseMin * valueScale);
			int scaledMax = (int)(baseMax * valueScale);
        
			bonuses.Add(new PathBonusData {
				Type = type,
				Value = rng.NextInt(scaledMin, scaledMax + 1)
			});
		}

		return bonuses;
	}

	private List<PathPenaltyData> GeneratePathPenalties(int seed, PathConnection path, float fogStrength) {
		var penalties = new List<PathPenaltyData>();
		var rng = GameRandom.For("Expedition.Events", seed);

		bool isRiskyPath = (seed % 2) == 1;
    
		if (!isRiskyPath) {
			return penalties;
		}

		float valueScale = 1.0f + (fogStrength * 1.5f);

		var possiblePenalties = new (RunModifierType type, int baseMin, int baseMax)[] {
			(RunModifierType.EnemyStrengthBonus, 5, 15),
			(RunModifierType.EnemyHealthBonus, 10, 25),
			(RunModifierType.EnemyArmorBonus, 1, 3),
			(RunModifierType.HealingReduction, 10, 25),
			(RunModifierType.FatigueIncrease, 10, 20),
			(RunModifierType.EncounterRateIncrease, 15, 30)
		};

		var (type, baseMin, baseMax) = possiblePenalties[rng.NextInt(possiblePenalties.Length)];
    
		int scaledMin = (int)(baseMin * valueScale);
		int scaledMax = (int)(baseMax * valueScale);
    
		penalties.Add(new PathPenaltyData {
			Type = type,
			Value = rng.NextInt(scaledMin, scaledMax + 1)
		});

		return penalties;
	}

	private string GeneratePathDescription(PathConnection path, PathNode? targetNode) {
		var descProto = k_gameDb?.Get<TerrainProto>(path.TerrainId);
		string terrainDesc = descProto?.DisplayText.Description ?? "";

		if (targetNode?.Type == PathNodeType.Dungeon) {
			terrainDesc += " A dangerous location lies ahead.";
		} else if (targetNode?.Type == PathNodeType.CampSite) {
			terrainDesc += " A safe resting spot awaits.";
		} else if (targetNode?.Type == PathNodeType.Landmark) {
			terrainDesc += " Something interesting catches your eye.";
		}

		return terrainDesc;
	}

	private int CalculatePathDifficulty(PathConnection path, PathNode? targetNode) {
		float difficulty = 1f;

		var terrainProto = k_gameDb?.Get<TerrainProto>(path.TerrainId);
		difficulty += terrainProto?.BaseDifficulty ?? 0f;

		float fogStrength = k_currentFogRegion?.CurrentStrength ?? 0f;
		difficulty += fogStrength * 2;

		if (targetNode != null) {
			difficulty += targetNode.Type switch {
				PathNodeType.BossLocation => 3,
				PathNodeType.Dungeon => 2,
				PathNodeType.Landmark => 0.5f,
				_ => 0
			};
		}

		return (int)Math.Clamp(difficulty, 1, 5);
	}

	#endregion

	#region Run Modifiers

	public float GetModifierValue(RunModifierType type) {
		return ActiveModifiers
			.Where(m => m.Type == type)
			.Sum(m => m.Value);
	}

	public void RecordEnemyDefeat(int enemyCount, int totalCombatPower) {
		k_currentFogRegion?.RecordCombatVictory(enemyCount, totalCombatPower);
	}

	public float GetFogStrengthModifier() {
		if (k_currentFogRegion == null) return 0f;
		return k_currentFogRegion.CurrentStrength;
	}

	public TagProto.ID? GetCurrentFogTag() {
		return k_currentFogRegion?.GetCurrentFogTag();
	}

	public FogTierEffects? GetCurrentFogEffects() {
		return k_currentFogRegion?.GetCurrentEffects();
	}

	#endregion

	#region Queries

	public ResourceStatus GetResourceStatus() {
		if (IsStarving || Morale <= 0 || IsOverfatigued) {
			return ResourceStatus.Critical;
		}

		if (FoodCritical || MoraleCritical || (k_runState?.Character != null && 
			k_characterManager.GetFatiguePercent(k_runState.Character) >= 0.8f)) {
			return ResourceStatus.Warning;
		}

		if (Food <= 2 || Morale <= 40 || (k_runState?.Character != null && 
			k_characterManager.GetFatiguePercent(k_runState.Character) >= 0.5f)) {
			return ResourceStatus.Caution;
		}

		return ResourceStatus.Good;
	}

	public List<string> GetWarnings() {
		var warnings = new List<string>();

		if (k_runState == null) return warnings;

		if (IsStarving) {
			warnings.Add("⚠ STARVING - Take damage each day!");
		} else if (FoodCritical) {
			warnings.Add("⚠ Food critically low!");
		}

		if (Morale <= 0) {
			warnings.Add("⚠ BROKEN - Severe penalties!");
		} else if (MoraleCritical) {
			warnings.Add("⚠ Morale critically low!");
		}

		if (IsExhausted) {
			warnings.Add("⚠ Stamina critically low!");
		}

		if (IsOverfatigued) {
			warnings.Add("⚠ EXHAUSTED - Must rest!");
		} else if (k_runState.Character != null) {
			float fatigue = k_characterManager.GetStat(k_runState.Character, Ids.Stats.Resource.Fatigue);
			float maxFatigue = k_characterManager.GetStat(k_runState.Character, Ids.Stats.Resource.MaxFatigue);
			if (maxFatigue > 0 && fatigue / maxFatigue >= 0.8f) {
				warnings.Add("⚠ Very tired - rest soon!");
			}
		}

		if (MedicalSupplies == 0) {
			warnings.Add("No medical supplies!");
		}

		if (CampingSupplies == 0) {
			warnings.Add("No camping supplies - cannot rest!");
		}

		if (k_weather.CurrentWeather == WeatherType.Storm) {
			warnings.Add("Storm conditions!");
		}

		if (k_currentFogRegion != null && (int)k_currentFogRegion.CurrentTier >= (int)FogTier.Thick) {
			warnings.Add($"Dense fog: {k_currentFogRegion.CurrentTier.GetDisplayName()}");
		}

		return warnings;
	}
	
	#endregion

	#region Serialization

	public ExpeditionSaveData? GetSaveData() {
		if (!IsActive) {
			return null;
		}

		return new ExpeditionSaveData {
			TravelData = k_travel?.ToSaveState(),
			RevealedTiles = k_fogOfWar?.GetAllRevealedTiles()
				.Select(t => new TileCoord(t.x, t.y)).ToList() ?? [],
			AccumulatedTime = k_accumulatedTime,
			LastDayChecked = k_lastDayChecked,
			CurrentPace = CurrentPace,
			SimSpeed = SimSpeed,
			Weather = k_weather.ToData(),
			Queue = StopQueue.ToData(),
			FogRegion = k_currentFogRegion?.ToData(),
			DistanceSinceLastEvent = k_distanceSinceLastEvent,
			TutorialComplete = k_tutorialComplete,
			ActiveModifiers = ActiveModifiers.Select(m => new RunModifierData {
				Type = m.Type,
				Value = m.Value,
				Source = m.Source
			}).ToList()
		};
	}

	public void LoadFromSaveData(RunState runState, ExpeditionSaveData data) {
		k_runState = runState;

		if (data.TravelData != null) {
			k_travel = TravelData.FromSaveState(data.TravelData);
		}

		var revealedTiles = data.RevealedTiles
			.Select(t => new Vector2Int(t.X, t.Y))
			.ToHashSet();
		k_fogOfWar = new FogOfWar(revealedTiles);

		k_accumulatedTime = data.AccumulatedTime;
		k_lastDayChecked = data.LastDayChecked;

		CurrentPace = data.CurrentPace;
		SimSpeed = data.SimSpeed;
		k_distanceSinceLastEvent = data.DistanceSinceLastEvent;
		k_tutorialComplete = data.TutorialComplete;

		if (k_tutorialComplete && k_gameDb != null) {
			k_pathGenerator = new IsoPathGenerator(k_gameDb, k_eventManager);
		}

		if (data.Weather != null) {
			k_weather = WeatherSystem.FromData(data.Weather);
			k_weather.OnWeatherChanged += (_, newWeather) => OnWeatherChanged?.Invoke(newWeather);
		}

		if (data.Queue != null) {
			StopQueue.Clear();
			var loadedQueue = TravelQueue.FromData(data.Queue);
			foreach (var stop in loadedQueue.Stops) {
				StopQueue.AddStop(stop.Tile, stop.Reason, stop.InteractionId, stop.AutoPause);
			}
		}

		if (data.FogRegion != null) {
			k_currentFogRegion = FogRegion.FromData(data.FogRegion);
		}

		ActiveModifiers.Clear();
		foreach (var modData in data.ActiveModifiers) {
			ActiveModifiers.Add(new RunModifier {
				Type = modData.Type,
				Value = modData.Value,
				Source = modData.Source
			});
		}

		IsActive = true;

		Debug.Log("Expedition loaded from save");
	}

	#endregion
}

#region Path Choice Data

public class PathChoiceData {
	public PathNodeId NodeId { get; set; }
	public string Name { get; set; } = "";
	public string Description { get; set; } = "";
	public string TerrainIcon { get; set; } = "";
	public string TerrainName { get; set; } = "";
	public List<PathBonusData> Bonuses { get; set; } = [];
	public List<PathPenaltyData> Penalties { get; set; } = [];
	public int DifficultyHint { get; set; } = 1;
	public AngleRPG Direction { get; set; } = AngleRPG.North;
	public float Distance { get; set; } = 0f;

	public string DirectionArrow => Direction.Degrees switch {
		>= 337.5f or < 22.5f => "→",
		>= 22.5f and < 67.5f => "↗",
		>= 67.5f and < 112.5f => "↑",
		>= 112.5f and < 157.5f => "↖",
		>= 157.5f and < 202.5f => "←",
		>= 202.5f and < 247.5f => "↙",
		>= 247.5f and < 292.5f => "↓",
		_ => "↘"
	};

	public string DirectionName => Direction.ToDirectionName();
	public bool IsRisky => Penalties.Count > 0;
	public bool PointsLeft => Direction.Degrees is >= 112.5f and < 247.5f;
	public bool PointsRight => Direction.Degrees is (>= 0f and < 67.5f) or (>= 292.5f and < 360f);
}

public class PathBonusData {
	public RunModifierType Type { get; set; }
	public float Value { get; set; }
}

public class PathPenaltyData {
	public RunModifierType Type { get; set; }
	public float Value { get; set; }
}

#endregion

#region Run Modifiers

public class RunModifier {
	public RunModifierType Type { get; set; }
	public float Value { get; set; }
	public string Source { get; set; } = "";
}

public enum RunModifierType {
	ExperienceBonus,
	MaxHealthBonus,
	MaxManaBonus,
	DamageBonus,
	ArmorBonus,
	GoldFindBonus,
	LootQualityBonus,
	HealingBonus,
	FatigueReduction,
	SpellCostReduction,

	EnemyStrengthBonus,
	EnemyHealthBonus,
	EnemyArmorBonus,
	HealingReduction,
	FatigueIncrease,
	VisionReduction,
	EncounterRateIncrease
}

public class RunModifierData {
	public RunModifierType Type { get; set; }
	public float Value { get; set; }
	public string Source { get; set; } = "";
}

#endregion

#region Save Data

public class ExpeditionSaveData {
	public ExpeditionManager.TravelDataSaveState? TravelData { get; set; }
	public List<TileCoord> RevealedTiles { get; set; } = [];
	public float AccumulatedTime { get; set; }
	public int LastDayChecked { get; set; }

	public TravelPace CurrentPace { get; set; }
	public SimulationSpeed SimSpeed { get; set; }
	public WeatherData? Weather { get; set; }
	public TravelQueueData? Queue { get; set; }
	public FogRegionData? FogRegion { get; set; }
	public float DistanceSinceLastEvent { get; set; }
	public bool TutorialComplete { get; set; }
	public List<RunModifierData> ActiveModifiers { get; set; } = [];
}

#endregion
using RPGGame.Core.Characters;
using RPGGame.Core.Characters.Creation;
using RPGGame.Core.Combat;
using RPGGame.Core.Events;
using RPGGame.Core.Expedition;
using RPGGame.Core.Items;
using RPGGame.Core.Metrics;
using RPGGame.Core.Prototypes;
using RPGGame.Core.Prototypes.Characters;
using RPGGame.Core.Prototypes.Combat;
using RPGGame.Core.Prototypes.Events;
using RPGGame.Core.Save;
using RPGGame.Core.Stats;
using RPGGame.Core.Village;
using UnityEngine;

namespace RPGGame.Core.Simulation;

/// <summary>
/// Manages a complete game session, coordinating all game systems.
/// This is the central orchestrator that ties together all game subsystems.
/// </summary>
/// <remarks>
/// Responsibilities:
/// - Session lifecycle (initialize, shutdown)
/// - Run management (start, end, rewards)
/// - Coordination between managers (village, expedition, combat, events)
/// - Save/load coordination
/// - Phase transitions
/// </remarks>
[Dependency(RegistrationType.Singleton)]
public class GameSession {
	#region Fields

	private readonly GameDb k_gameDb;
	private readonly GameLoop k_gameLoop;
	private readonly GameStateManager k_stateManager;
	private readonly MetricsManager k_metrics;
	private readonly VillageManager k_village;
	private readonly ExpeditionManager k_expedition;
	private readonly EventManager k_events;
	private readonly CombatManager k_combat;
	private readonly InventoryManager k_inventory;
	private readonly SaveManager k_saveManager;

	private bool k_isInitialized;
	private float k_sessionStartTime;

	#endregion

	#region Properties

	/// <summary>
	/// Whether the session is initialized and ready.
	/// </summary>
	public bool IsInitialized => k_isInitialized;

	/// <summary>
	/// Current game phase.
	/// </summary>
	public GamePhase CurrentPhase => k_stateManager.CurrentPhase;

	/// <summary>
	/// Current run state (if in a run).
	/// </summary>
	public RunState? CurrentRun => k_stateManager.RunState;

	/// <summary>
	/// Meta progression data.
	/// </summary>
	public MetaProgression MetaProgression => k_stateManager.MetaProgression;

	/// <summary>
	/// Whether currently in combat.
	/// </summary>
	public bool IsInCombat => k_combat.IsInCombat;

	/// <summary>
	/// Whether currently on expedition.
	/// </summary>
	public bool IsOnExpedition => k_expedition.IsActive;

	/// <summary>
	/// Session duration in seconds.
	/// </summary>
	public float SessionDuration => Time.time - k_sessionStartTime;

	public GameDb GameDatabase => k_gameDb;

	#endregion

	#region Events

	public event Action? OnSessionStarted;
	public event Action? OnSessionEnded;
	public event Action<GamePhase, GamePhase>? OnPhaseChanged;
	public event Action<RunState>? OnRunStarted;
	public event Action<RunEndReason>? OnRunEnded;

	#endregion

	#region Public Access for Managers and Stuff

	/// <summary>
	/// The event manager for this session.
	/// </summary>
	public EventManager EventManager => k_events;

	/// <summary>
	/// The expedition manager for this session.
	/// </summary>
	public ExpeditionManager ExpeditionManager => k_expedition;

	#endregion

	#region Constructor

	public GameSession(
		GameDb gameDb,
		GameLoop gameLoop,
		GameStateManager stateManager,
		MetricsManager metrics,
		VillageManager village,
		ExpeditionManager expedition,
		EventManager events,
		CombatManager combat,
		InventoryManager inventory,
		SaveManager saveManager
	) {
		k_gameDb = gameDb;
		k_gameLoop = gameLoop;
		k_stateManager = stateManager;
		k_metrics = metrics;
		k_village = village;
		k_expedition = expedition;
		k_events = events;
		k_combat = combat;
		k_inventory = inventory;
		k_saveManager = saveManager;

		k_gameLoop.SetGameSession(this);

		SubscribeToEvents();

		Debug.Log("GameSession created");
	}

	#endregion

	#region Initialization

	/// <summary>
	/// Initializes the game session.
	/// </summary>
	public void Initialize() {
		Debug.Log("GameSession.Initialize: START");
    
		if (k_isInitialized) {
			Debug.Log("GameSession.Initialize: Already initialized, skipping");
			return;
		}

		k_sessionStartTime = Time.time;

		Debug.Log("GameSession.Initialize: Loading meta progression...");
		LoadMetaProgression();
		Debug.Log("GameSession.Initialize: Meta progression loaded");

		Debug.Log("GameSession.Initialize: Initializing village...");
		InitializeVillage();
		Debug.Log("GameSession.Initialize: Village initialized");

		k_isInitialized = true;
    
		Debug.Log("GameSession.Initialize: Setting phase to MainMenu...");
		k_stateManager.SetPhase(GamePhase.MainMenu);
		Debug.Log("GameSession.Initialize: Phase set");

		OnSessionStarted?.Invoke();
		Debug.Log("GameSession initialized");
	}

	/// <summary>
	/// Shuts down the game session.
	/// </summary>
	public void Shutdown() {
		if (!k_isInitialized) {
			return;
		}

		// Auto-save before shutdown
		//if (CurrentRun != null) {
		//	k_saveManager.SaveGame(SaveManager.AUTOSAVE_SLOT);
		//}

		UnsubscribeFromEvents();

		k_isInitialized = false;
		OnSessionEnded?.Invoke();

		Debug.Log("GameSession shutdown");
	}

	/// <summary>
	/// Generates a new world seed for a fresh run.
	/// Uses multiple entropy sources for good randomness.
	/// </summary>
	public static int GenerateWorldSeed() {
		// Combine time and GUID for good entropy without being overkill
		return HashCode.Combine(
				DateTime.UtcNow.Ticks,
				Environment.TickCount,
				Guid.NewGuid().GetHashCode()
			);
	}

	private void LoadMetaProgression() {
		Debug.Log("LoadMetaProgression: Getting slot info...");
    
		// Try to load existing meta from most recent save
		var slots = k_saveManager.GetAllSlotInfo();
    
		Debug.Log($"LoadMetaProgression: Found {slots.Count} slots");
    
		var mostRecent = slots
			.Where(s => s.HasData)
			.OrderByDescending(s => s.SaveTimestamp)
			.FirstOrDefault();

		if (mostRecent != null) {
			Debug.Log($"LoadMetaProgression: Loading from slot {mostRecent.SlotIndex}...");
			// Don't load saves during initial startup - just initialize fresh
			// The save can be loaded later via menu
			Debug.Log("LoadMetaProgression: Skipping save load during startup, initializing fresh");
			k_stateManager.InitializeNewMeta();
		} else {
			Debug.Log("LoadMetaProgression: No saves found, initializing fresh meta");
			k_stateManager.InitializeNewMeta();
		}
    
		Debug.Log("LoadMetaProgression: Done");
	}

	private void InitializeVillage() {
		// Apply unlocked buildings from meta
		foreach (var buildingId in MetaProgression.UnlockedBuildings) {
			if (MetaProgression.BuildingLevels.TryGetValue(buildingId, out int level)) {
				// Village manager handles building setup internally
				// Just ensure they're tracked in meta
			}
		}

		// Initialize new village if needed
		if (!k_village.IsInitialized) {
			k_village.InitializeNewVillage();
		}
	}

	#endregion

	#region Run Management

	/// <summary>
	/// Starts a new run with the specified character.
	/// </summary>
	public bool StartNewRun(string characterName, CharacterClassProto.ID classId) {
		if (CurrentRun != null) {
			Debug.LogWarning("Run already in progress");
			return false;
		}

		// Validate class is unlocked
		if (!IsClassUnlocked(classId)) {
			Debug.LogError($"Class not unlocked: {classId}");
			return false;
		}

		// Get class proto
		if (!k_gameDb.TryGetProto<CharacterClassProto>(classId, out var classProto)) {
			Debug.LogError($"Class proto not found: {classId}");
			return false;
		}

		int masterSeed = GenerateWorldSeed();
		GameRandom.Initialize(masterSeed);
		// Create run state
		var runState = CreateRunStateFromClass(characterName, classProto, masterSeed);

		// Apply meta bonuses
		ApplyMetaBonuses(runState);

		// Give starting equipment
		GiveStartingEquipment(classProto);

		// Start the run
		k_stateManager.StartRun(runState);
		k_stateManager.SetPhase(GamePhase.Village);

		MetaProgression.TotalRuns++;
		k_metrics.Increment(MetricType.RunsStarted);

		OnRunStarted?.Invoke(runState);
		Debug.Log($"Started run #{MetaProgression.TotalRuns} as {characterName} ({classProto.DisplayText.Name})");

		return true;
	}

	/// <summary>
	/// Starts a new run with the specified character (string ID overload for backwards compatibility).
	/// </summary>
	public bool StartNewRun(string characterName, string classIdString) {
		var classId = new CharacterClassProto.ID(classIdString);
		return StartNewRun(characterName, classId);
	}

	/// <summary>
	/// Starts a new run with a fully customized character from the character creator.
	/// </summary>
	public bool StartNewRunFromCreation(string characterName, CharacterCreationState creationState) {
		if (CurrentRun != null) {
			Debug.LogWarning("Run already in progress");
			return false;
		}

		if (!creationState.SelectedClassId.HasValue) {
			Debug.LogError("No class selected in creation state");
			return false;
		}

		var classId = creationState.SelectedClassId.Value;

		if (!IsClassUnlocked(classId)) {
			Debug.LogError($"Class not unlocked: {classId}");
			return false;
		}

		if (!k_gameDb.TryGetProto<CharacterClassProto>(classId, out var classProto)) {
			Debug.LogError($"Class proto not found: {classId}");
			return false;
		}
		int masterSeed = GenerateWorldSeed();
		GameRandom.Initialize(masterSeed);
		var runState = CreateRunStateFromCreation(characterName, classProto, creationState, masterSeed);

		ApplyMetaBonuses(runState);
		GiveStartingEquipment(classProto);

		k_stateManager.StartRun(runState);
		k_stateManager.SetPhase(GamePhase.Village);

		MetaProgression.TotalRuns++;
		k_metrics.Increment(MetricType.RunsStarted);

		OnRunStarted?.Invoke(runState);
		Debug.Log($"Started run #{MetaProgression.TotalRuns} as {characterName} ({classProto.DisplayText.Name}) from character creator");

		return true;
	}

	private RunState CreateRunStateFromCreation(
		string characterName,
		CharacterClassProto classProto,
		CharacterCreationState creationState,
		int masterSeed
	) {
		// Create the LiveCharacter using the existing factory method
		var character = LiveCharacter.CreatePlayer(characterName, classProto, creationState);

		return new RunState {
			CharacterClassId = classProto.Id.Value,
			Character = character,
			Inventory = new Inventory(),
			Morale = 100,
			Fatigue = 0,
			WorldSeed = masterSeed
		};
	}

	private RunState CreateRunStateFromClass(string characterName, CharacterClassProto classProto, int masterSeed) {
		// Create a default creation state from class defaults
		var creationState = new CharacterCreationState {
			SelectedClassId = classProto.Id
		};

		// Set default attributes from class
		creationState.Attributes[Ids.Stats.Attributes.Strength] = classProto.BaseAttributes.Strength;
		creationState.Attributes[Ids.Stats.Attributes.Dexterity] = classProto.BaseAttributes.Dexterity;
		creationState.Attributes[Ids.Stats.Attributes.Constitution] = classProto.BaseAttributes.Constitution;
		creationState.Attributes[Ids.Stats.Attributes.Intelligence] = classProto.BaseAttributes.Intelligence;
		creationState.Attributes[Ids.Stats.Attributes.Wisdom] = classProto.BaseAttributes.Wisdom;
		creationState.Attributes[Ids.Stats.Attributes.Charisma] = classProto.BaseAttributes.Charisma;

		// Create the LiveCharacter
		var character = LiveCharacter.CreatePlayer(characterName, classProto, creationState);

		return new RunState {
			CharacterClassId = classProto.Id.Value,
			Character = character,
			Inventory = new Inventory(),
			Morale = 100,
			Fatigue = 0,
			WorldSeed = masterSeed
		};
	}

	public bool IsClassUnlocked(CharacterClassProto.ID classId) {
		// Starter classes are always available
		if (IsStarterClass(classId)) {
			return true;
		}

		// Check if unlocked in meta progression
		return MetaProgression.UnlockedClasses.Contains(classId.Value);
	}

	public bool IsStarterClass(CharacterClassProto.ID classId) {
		return classId == Ids.CharacterClasses.Fighter ||
			   classId == Ids.CharacterClasses.Rogue ||
			   classId == Ids.CharacterClasses.Mage ||
			   classId == Ids.CharacterClasses.Cleric;
	}

	/// <summary>
	/// Ends the current run.
	/// </summary>
	public void EndRun(RunEndReason reason) {
		if (CurrentRun == null) {
			return;
		}

		var run = CurrentRun;

		// End any active combat first
		if (k_combat.IsInCombat) {
			k_combat.EndCombat(CombatEndReason.Interrupted);
		}

		// Reset all run-scoped managers
		k_events.Reset();
		k_expedition.Reset();
		k_inventory.Clear();

		// Calculate and apply rewards
		ApplyRunRewards(run, reason);

		// Track metrics
		TrackRunMetrics(run, reason);

		// End the run in state manager (clears RunState)
		k_stateManager.EndRun();

		// Transition to run summary
		k_stateManager.SetPhase(GamePhase.RunSummary);

		OnRunEnded?.Invoke(reason);
		Debug.Log($"Run ended: {reason}");
	}

	private void ApplyMetaBonuses(RunState runState) {
		// Apply stat bonuses from meta progression
		var metaModifiers = MetaProgression.GetAllMetaModifiers();
		foreach (var modifier in metaModifiers) {
			runState.AddModifier(modifier);
		}

		// Apply village starting bonuses
		var startingBonuses = k_village.GetStartingBonuses();
		runState.Character.BaseStats.Add(Ids.Stats.Expedition.GoldOnHand, startingBonuses.BonusGold);
		runState.Character.BaseStats.Add(Ids.Stats.Expedition.FoodOnHand, startingBonuses.BonusFood);
		runState.Character.BaseStats.Add(Ids.Stats.Expedition.MedicalSupplies, startingBonuses.BonusMedicalSupplies);
		runState.Character.BaseStats.Add(Ids.Stats.Expedition.CampingSupplies, startingBonuses.BonusCampingSupplies);

		// Apply village stat bonuses
		foreach (var modifier in k_village.GetAllBuildingBonuses()) {
			runState.AddModifier(modifier);
		}
	}

	private void GiveStartingEquipment(CharacterClassProto classProto) {
		k_inventory.Clear();
		k_inventory.Gold = classProto.StartingGold;

		foreach (var itemId in classProto.StartingEquipment) {
			var item = k_inventory.AddItem(new Proto.ID(itemId.Value));
			if (item != null && item.Prototype.EquipSlot.HasValue) {
				k_inventory.EquipItem(item);
			}
		}
	}

	private void ApplyRunRewards(RunState run, RunEndReason reason) {
		// Village upgrade points based on distance traveled
		int upgradePoints = (int)(run.TotalDistanceTraveled / 10);

		// Bonus for victory
		if (reason == RunEndReason.Victory) {
			upgradePoints += 50;
			MetaProgression.FogClears++;

			// Check for true ending
			if (CheckTrueEndingConditions(run)) {
				MetaProgression.TrueEndingAchieved = true;
				MetaProgression.TrueEndingCount++;
				upgradePoints += 200;
			}
		}

		MetaProgression.VillageUpgradePoints += upgradePoints;
		MetaProgression.TotalVillageUpgradePointsEarned += upgradePoints;

		// Transfer discovered lore
		foreach (var lore in run.DiscoveredLore) {
			MetaProgression.DiscoveredLore.Add(lore);
		}

		// Transfer discovered landmarks
		foreach (var landmark in run.DiscoveredLandmarks) {
			MetaProgression.DiscoveredLandmarks.Add(landmark);
		}

		// Transfer revealed tiles
		foreach (var tile in run.RevealedTiles) {
			MetaProgression.PermanentlyRevealedTiles.Add(tile);
		}

		// Update furthest distance
		if (run.TotalDistanceTraveled > MetaProgression.FurthestDistanceReached) {
			MetaProgression.FurthestDistanceReached = run.TotalDistanceTraveled;
		}

		// Check for class unlocks
		CheckClassUnlocks(run);
	}

	private void TrackRunMetrics(RunState run, RunEndReason reason) {
		k_metrics.Increment(MetricType.RunsCompleted);

		if (reason == RunEndReason.Victory) {
			k_metrics.Increment(MetricType.FogClears);
		} else if (reason == RunEndReason.Death) {
			k_metrics.Increment(MetricType.TimesCaughtInTime);
		}

		k_metrics.Add(MetricType.TotalDaysSurvived, GameTime.Instance.Day);
		k_metrics.SetMax(MetricType.FurthestDistanceEver, (long)run.TotalDistanceTraveled);
		k_metrics.SetMax(MetricType.HighestLevel, run.Stats.GetInt(Ids.Stats.Meta.Level));
	}

	private bool CheckTrueEndingConditions(RunState run) {
		// Collected all fog clues
		if (MetaProgression.FogClues.Count < 5) {
			return false;
		}

		// Defeated the Herald with all clues
		return run.ActiveConditions.Contains("has_all_clues");
	}

	private void CheckClassUnlocks(RunState run) {
		// Ranger: Explore 50 tiles
		if (k_metrics.Get(MetricType.TilesRevealed) >= 50) {
			UnlockClass(Ids.CharacterClasses.Ranger);
		}

		// Paladin: Heal 500 HP total
		if (k_metrics.Get(MetricType.HealingDone) >= 500) {
			UnlockClass(Ids.CharacterClasses.Paladin);
		}

		// Bard: Complete 20 peaceful resolutions
		if (k_metrics.Get(MetricType.PeacefulChoices) >= 20) {
			UnlockClass(Ids.CharacterClasses.Bard);
		}

		// Druid: Discover nature lore
		if (MetaProgression.DiscoveredLore.Any(l => l.Contains("nature") || l.Contains("druid"))) {
			UnlockClass(Ids.CharacterClasses.Druid);
		}

		// Ascended: True ending
		if (MetaProgression.TrueEndingAchieved) {
			UnlockClass(Ids.CharacterClasses.Ascended);
		}

		// Time Walker: 10 fog clears
		if (MetaProgression.FogClears >= 10) {
			UnlockClass(Ids.CharacterClasses.TimeWalker);
		}
	}

	private void UnlockClass(CharacterClassProto.ID classId) {
		if (!MetaProgression.UnlockedClasses.Contains(classId.Value)) {
			MetaProgression.UnlockedClasses.Add(classId.Value);
			k_metrics.Increment(MetricType.ClassesUnlocked);
			Debug.Log($"Unlocked class: {classId}");
		}
	}

	#endregion

	#region Expedition

	/// <summary>
	/// Starts an expedition from the village.
	/// </summary>
	public bool StartExpedition() {
		if (CurrentRun == null || CurrentPhase != GamePhase.Village) {
			return false;
		}

		// Check supplies
		if (CurrentRun.Stats.GetInt(Ids.Stats.Expedition.FoodOnHand) <= 0) {
			Debug.LogWarning("Cannot start expedition without food");
			return false;
		}

		k_expedition.StartExpedition(CurrentRun, MetaProgression.PermanentlyRevealedTiles);
		k_stateManager.SetPhase(GamePhase.Expedition);

		k_metrics.Increment(MetricType.ExpeditionsStarted);

		return true;
	}

	/// <summary>
	/// Returns to the village from an expedition.
	/// </summary>
	public void ReturnToVillage() {
		if (!k_expedition.IsActive) {
			return;
		}

		k_expedition.ReturnToVillage();
		k_stateManager.SetPhase(GamePhase.Village);
	}

	#endregion

	#region Combat

	/// <summary>
	/// Starts a combat encounter.
	/// </summary>
	public bool StartCombat(EncounterProto.ID encounterId) {
		Debug.Log($"GameSession.StartCombat: Attempting to start encounter '{encounterId.Value}'");
    
		if (CurrentRun == null) {
			Debug.LogError($"GameSession.StartCombat: Failed - CurrentRun is null");
			return false;
		}


		bool started = k_combat.StartCombat(encounterId, CurrentRun);
    
		Debug.Log($"GameSession.StartCombat: CombatManager.StartCombat returned {started}");

		if (started) {
			k_stateManager.SetPhase(GamePhase.Combat);
			Debug.Log($"GameSession.StartCombat: Combat started successfully, phase set to Combat");
		} else {
			Debug.LogError($"GameSession.StartCombat: CombatManager failed to start combat");
		}

		return started;
	}

	/// <summary>
	/// Called when combat ends.
	/// </summary>
	private void OnCombatEnded(CombatResult result) {
		if (CurrentRun == null) {
			return;
		}

		// Check for defeat first - player died in combat
		if (result.Reason == CombatEndReason.Defeat) {
			EndRun(RunEndReason.Death);
			return;
		}

		// Apply rewards on victory
		if (result.IsVictory) {
			int xpToAward;
        
			// ═══════════════════════════════════════════════════════════════════════════
			// TUTORIAL: First combat guarantees enough XP to reach level 2
			// This ensures players experience the level-up system as part of the tutorial.
			// ═══════════════════════════════════════════════════════════════════════════
			if (IsTutorialFirstCombat()) {
				xpToAward = CalculateTutorialLevelUpXP();
				Debug.Log($"Tutorial first combat: Awarding {xpToAward} XP to guarantee level 2");
			} else {
				xpToAward = result.ExperienceGained;
			}

			// Update the result so the rewards panel displays the correct amount
			result.ExperienceGained = xpToAward;

			int previousXP = (int)CurrentRun.Character.Experience;
			CurrentRun.AddExperience(xpToAward);
			Debug.Log($"XP Award: {previousXP} + {xpToAward} = {CurrentRun.Character.Experience}");
        
			CurrentRun.Stats.Add(Ids.Stats.Expedition.GoldOnHand, result.GoldGained);

			// Add loot
			foreach (var loot in result.LootDropped) {
				k_inventory.AddItem(loot.ItemId, loot.MinQuantity);
			}
		}

		// Check for death from HP (fallback check)
		if (CurrentRun.Stats.Get(Ids.Stats.Resource.CurrentHealth) <= 0) {
			EndRun(RunEndReason.Death);
			return;
		}

		// NOTE: For victories and escapes, we do NOT transition phase here.
		// The CombatRewardsPanel will call FinishCombatTransition() when dismissed.
		// This keeps the CombatScreen visible so the rewards panel can be seen.
	}

	/// <summary>
	/// Checks if the current combat was triggered by the tutorial first combat event.
	/// </summary>
	private bool IsTutorialFirstCombat() {
		// Check if we're in the first combat event
		if (!k_events.IsEventActive) {
			return false;
		}

		var currentEvent = k_events.CurrentEvent;
		if (currentEvent == null) {
			return false;
		}

		// Check if this is the first combat encounter event
		return currentEvent.Id == Ids.Events.Intro.FirstCombatEncounter;
	}

	/// <summary>
	/// Calculates exactly how much XP is needed to reach level 2 from current state.
	/// </summary>
	private int CalculateTutorialLevelUpXP() {
		if (CurrentRun == null) {
			return 100; // Fallback: default XP to level 2
		}

		int currentXP = (int)CurrentRun.Character.Experience;
		int xpNeeded = (int)CurrentRun.Character.ExperienceToNextLevel;

		// Award exactly enough to reach level 2 (with a small buffer for satisfaction)
		int xpToAward = xpNeeded - currentXP;

		// Ensure we award at least some XP (in case they somehow already have enough)
		return Math.Max(xpToAward, 10);
	}

	/// <summary>
	/// Called by CombatRewardsPanel when the player dismisses the rewards.
	/// Completes the event and transitions back to expedition/village.
	/// </summary>
	public void FinishCombatTransition() {
		if (CurrentRun == null) return;

		// Complete any active event that triggered this combat
		if (k_events.IsEventActive) {
			Debug.Log($"FinishCombatTransition: Completing event '{k_events.CurrentEvent?.Title}'");
			k_events.CompleteEvent(CurrentRun, MetaProgression);
		}

		// Return to previous phase
		if (k_expedition.IsActive) {
			k_stateManager.SetPhase(GamePhase.Expedition);

			// Resume travel
			var travel = k_expedition.Travel;
			if (travel != null && !travel.IsTraveling) {
				k_expedition.ContinueTravel();
			}
		} else {
			k_stateManager.SetPhase(GamePhase.Village);
		}
	}

	#endregion

	#region Events

	/// <summary>
	/// Triggers an event.
	/// </summary>
	public bool TriggerEvent(string eventId) {
		if (CurrentRun == null) {
			Debug.LogWarning($"TriggerEvent failed: CurrentRun is null. Event: {eventId}");
			return false;
		}
    
		bool result = k_events.StartEvent(eventId, CurrentRun, MetaProgression);
		if (!result) {
			Debug.LogWarning($"TriggerEvent failed: EventManager.StartEvent returned false. Event: {eventId}");
		}
		return result;
	}

	/// <summary>
	/// Triggers an event by typed ID.
	/// </summary>
	public bool TriggerEvent(EventProto.ID eventId) {
		if (CurrentRun == null) {
			Debug.LogWarning($"TriggerEvent failed: CurrentRun is null. Event: {eventId}");
			return false;
		}
    
		bool result = k_events.StartEvent(eventId, CurrentRun, MetaProgression);
		if (!result) {
			Debug.LogWarning($"TriggerEvent failed: EventManager.StartEvent returned false. Event: {eventId}");
		}
		return result;
	}

	private void OnEventEnded(EventProto evt, EventOutcomeProto? outcome) {
		if (CurrentRun == null) {
			return;
		}

		// NOTE: Combat triggers are now handled exclusively by OnCombatTriggeredFromEvent
		// which is called when the OutcomeEffect.StartCombat effect is applied.
		// We should NOT start combat here to avoid duplicate combat triggers.
	
		// The old code tried to start combat here based on the outcome's effects,
		// but that combat was ALREADY started by the effect handler.
		// Removing this prevents the duplicate combat bug.
	}

	/// <summary>
	/// Handler for combat triggered from events.
	/// </summary>
	private void OnCombatTriggeredFromEvent(string encounterId) {
		Debug.Log($"GameSession: Combat triggered from event, encounter: {encounterId}");
	
		// Guard against duplicate triggers
		if (k_combat.IsInCombat) {
			Debug.LogWarning($"GameSession: Ignoring duplicate combat trigger for '{encounterId}' - already in combat");
			return;
		}
    
		bool success = StartCombat(new EncounterProto.ID(encounterId));
    
		if (!success) {
			Debug.LogError($"GameSession: Failed to start combat for encounter '{encounterId}'");
		}
	}

	#endregion

	#region Event Subscriptions

	private void SubscribeToEvents() {
		k_stateManager.OnPhaseChanged += HandlePhaseChanged;
		k_combat.OnCombatEnded += OnCombatEnded;
		k_events.OnEventEnded += OnEventEnded;
		k_events.OnCombatTriggered += OnCombatTriggeredFromEvent;
		k_gameLoop.OnUpdate += OnGameUpdate;
	}

	private void UnsubscribeFromEvents() {
		k_stateManager.OnPhaseChanged -= HandlePhaseChanged;
		k_combat.OnCombatEnded -= OnCombatEnded;
		k_events.OnEventEnded -= OnEventEnded;
		k_events.OnCombatTriggered -= OnCombatTriggeredFromEvent;
		k_gameLoop.OnUpdate -= OnGameUpdate;
	}

	private void HandlePhaseChanged(GamePhase oldPhase, GamePhase newPhase) {
		OnPhaseChanged?.Invoke(oldPhase, newPhase);
	}

	private void OnGameUpdate() {
		// Auto-save check
		k_saveManager.TryAutoSave(Time.deltaTime);
	}

	#endregion

	#region Save/Load

	/// <summary>
	/// Saves the game to a slot.
	/// </summary>
	public async Task<bool> SaveGameAsync(int slot) {
		return await k_saveManager.SaveGameAsync(slot);
	}

	/// <summary>
	/// Loads a game from a slot.
	/// </summary>
	public async Task<bool> LoadGameAsync(int slot) {
		bool success = await k_saveManager.LoadGameAsync(slot);

		if (success && CurrentRun != null) {
			// Restore proper phase based on run state
			if (k_expedition.IsActive) {
				k_stateManager.SetPhase(GamePhase.Expedition);
			} else {
				k_stateManager.SetPhase(GamePhase.Village);
			}
		}

		return success;
	}

	/// <summary>
	/// Gets all save slot info.
	/// </summary>
	public List<SaveSlotInfo> GetSaveSlots() {
		return k_saveManager.GetAllSlotInfo();
	}

	#endregion

	#region Queries

	/// <summary>
	/// Gets available character classes (unlocked or starter).
	/// </summary>
	public List<CharacterClassProto> GetAvailableClasses() {
		var classes = new List<CharacterClassProto>();

		// Get all class protos
		var allClasses = k_gameDb.GetAllProtosOfType<CharacterClassProto>();

		foreach (var classProto in allClasses) {
			if (IsClassUnlocked(classProto.Id)) {
				classes.Add(classProto);
			}
		}

		return classes;
	}

	/// <summary>
	/// Gets locked classes with unlock hints.
	/// </summary>
	public List<(CharacterClassProto proto, string hint)> GetLockedClasses() {
		var locked = new List<(CharacterClassProto, string)>();

		var allClasses = k_gameDb.GetAllProtosOfType<CharacterClassProto>();

		foreach (var classProto in allClasses) {
			// Skip starter classes (never locked)
			if (IsStarterClass(classProto.Id)) {
				continue;
			}

			// Check if not unlocked
			if (!MetaProgression.UnlockedClasses.Contains(classProto.Id.Value)) {
				locked.Add((classProto, classProto.UnlockHint));
			}
		}

		return locked;
	}

	/// <summary>
	/// Gets a specific class proto by ID.
	/// </summary>
	public CharacterClassProto? GetClassProto(CharacterClassProto.ID classId) {
		if (k_gameDb.TryGetProto<CharacterClassProto>(classId, out var proto)) {
			return proto;
		}
		return null;
	}

	/// <summary>
	/// Gets a specific class proto by string ID.
	/// </summary>
	public CharacterClassProto? GetClassProto(string classIdString) {
		return GetClassProto(new CharacterClassProto.ID(classIdString));
	}

	/// <summary>
	/// Gets current session state for UI.
	/// </summary>
	public SessionState GetSessionState() {
		return new SessionState {
			Phase = CurrentPhase,
			IsInRun = CurrentRun != null,
			RunNumber = MetaProgression.TotalRuns,
			CurrentDay = GameTime.Instance.Day,
			IsInCombat = IsInCombat,
			IsOnExpedition = IsOnExpedition,
			FogClears = MetaProgression.FogClears,
			TrueEndingAchieved = MetaProgression.TrueEndingAchieved
		};
	}

	#endregion
}

#region Supporting Types

/// <summary>
/// Reason why a run ended.
/// </summary>
public enum RunEndReason {
	/// <summary>Player died (caught in time).</summary>
	Death,
	/// <summary>Player chose to retreat.</summary>
	Retreat,
	/// <summary>Player cleared the fog.</summary>
	Victory,
	/// <summary>Player achieved the true ending.</summary>
	TrueEnding,
	/// <summary>Player quit mid-run.</summary>
	Abandoned
}

/// <summary>
/// Lightweight info about a character for run state.
/// </summary>
public class CharacterInfo {
	public string Name { get; set; } = "";
	public string ClassId { get; set; } = "";
	public int Level { get; set; } = 1;
	public string PortraitName { get; set; } = "portrait_default";
}

/// <summary>
/// Current session state for UI display.
/// </summary>
public class SessionState {
	public GamePhase Phase { get; set; }
	public bool IsInRun { get; set; }
	public int RunNumber { get; set; }
	public int CurrentDay { get; set; }
	public bool IsInCombat { get; set; }
	public bool IsOnExpedition { get; set; }
	public int FogClears { get; set; }
	public bool TrueEndingAchieved { get; set; }
}

#endregion